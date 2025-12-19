using LouisFrice.CropPlant;
using LouisFrice.Save;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace LouisFrice.Map
{
    public class GridManager : Singleton<GridManager>, ISaveable
    {
        [Header("种地的瓦片信息")]
        public RuleTile digTile;
        public RuleTile waterTile;
        private Tilemap digTilemap;
        private Tilemap waterTilemap;

        [Header("地图信息")]
        public List<MapData_SO> mapDataList;

        //当前季节
        private Season currentSeason;
        
        //key是场景名+坐标
        private Dictionary<string,TileDetails> tileDetailsDic = new Dictionary<string,TileDetails>();

        //场景是否第一次加载
        private Dictionary<string,bool> firstLoadDic = new Dictionary<string,bool>();

        private Grid currentGrid;

        //杂草列表
        private List<ReapItem> itemInRadius;

        public string GUID => GetComponent<DataGUID>().GUID;

        private void Start()
        {
            foreach (var mapData in mapDataList)
            {
                //默认所有地图都是第一次加载，加载后改成false
                firstLoadDic.Add(mapData.sceneName, true);
                InitTileDetailsDict(mapData);
            }

            //注册当前实体去存档
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }

        private void OnEnable()
        {
            EventHandler.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
            EventHandler.RefreshCurrentMap += RefreshMap;
        }
        private void OnDisable()
        {
            EventHandler.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
            EventHandler.RefreshCurrentMap -= RefreshMap ;
        }


        private void OnGameDayEvent(int day, Season season)
        {
            currentSeason = season;
            foreach (var tile in tileDetailsDic)
            {
                //浇水了一天就干渴
                if (tile.Value.daySinceWatered > -1)
                {
                    tile.Value.daySinceWatered = -1;
                }
                if (tile.Value.daysSinceDug > -1)
                {
                    tile.Value.daysSinceDug++;
                }
                //种地超过2天 && 没有播种
                if(tile.Value.daysSinceDug > 2 && tile.Value.seedItemID == -1)
                {
                    //变回枯地
                    tile.Value.daysSinceDug = -1;
                    tile.Value.canDig = true;
                    tile.Value.growthDays = -1;
                }
                //该瓦片位置有种子就开始计算种植天数
                if (tile.Value.seedItemID > -1)
                {
                    tile.Value.growthDays++;
                }

            }
            //每天刷新地图
            RefreshMap();
        }

        private void OnAfterSceneLoadedEvent()
        {
            currentGrid = FindObjectOfType<Grid>();
            digTilemap = GameObject.FindWithTag("Dig").GetComponent<Tilemap>();
            waterTilemap = GameObject.FindWithTag("Water").GetComponent<Tilemap>();

            //如果是第一次加载就生成树木，防止切换场景重复生成导致树木无法成长
            if (firstLoadDic[SceneManager.GetActiveScene().name])
            {
                //预先生成农作物(树木）
                EventHandler.CallGenerateCropEvent();
                firstLoadDic[SceneManager.GetActiveScene().name] = false;
            }
            
            //删除瓦片后显示地图
            RefreshMap();
        }
        /// <summary>
        /// 执行实际的物品和工具功能
        /// </summary>
        /// <param name="mouseWorldPos">鼠标位置</param>
        /// <param name="itemDetails">物品信息</param>
        private void OnExecuteActionAfterAnimation(Vector3 mouseWorldPos, ItemDetails itemDetails)
        {
            Vector3Int mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);
            TileDetails currentTile = GetTileDetailsOnMousePosition(mouseGridPos);

            if (currentTile != null)
            {
                Crop currentCrop = GetCropObject(mouseWorldPos);

                //WORKFLOW:物品使用的实际功能
                switch (itemDetails.itemType)
                {
                    //丢在地上
                    case ItemType.Commodity:
                        EventHandler.CallDropItemEvent(itemDetails.itemID, mouseGridPos,itemDetails.itemType);
                        break;
                    //锄头挖坑
                    case ItemType.HoeTool:
                        SetDigGround(currentTile);
                        currentTile.canDig = false;
                        currentTile.canDropItem = false;
                        currentTile.daysSinceDug = 0;
                        //音效
                        EventHandler.CallPlaySoundEvent(SoundName.Hoe);
                        break;
                    //浇水
                    case ItemType.WaterTool:
                        SetWaterGround(currentTile);
                        currentTile.daySinceWatered = 0;
                        //音效
                        EventHandler.CallPlaySoundEvent(SoundName.Water);
                        break;
                    //播种
                    case ItemType.Seed:
                        EventHandler.CallPlantSeedEvent(itemDetails.itemID, currentTile);
                        EventHandler.CallDropItemEvent(itemDetails.itemID,mouseWorldPos,itemDetails.itemType);
                        EventHandler.CallPlaySoundEvent(SoundName.Plant);
                        break;
                    //收集果实
                    case ItemType.CollectTool:
                        //执行收割方法
                        currentCrop?.ProcessToolAction(itemDetails,currentTile);
                        break;
                    //砍树
                    case ItemType.ChopTool:
                        //传的是当前作物对应的瓦片
                        currentCrop?.ProcessToolAction(itemDetails, currentCrop.tileDetails);
                        break;
                    //挖矿
                    case ItemType.BreakTool:
                        //传的是当前作物对应的瓦片
                        currentCrop?.ProcessToolAction(itemDetails, currentCrop.tileDetails);
                        break;
                    //割草
                    case ItemType.ReapTool:
                        int reapCount = 0;
                        for (int i = 0; i < itemInRadius.Count; i++)
                        {
                            EventHandler.CallParticleEffectEvent(ParticleEffectType.ReapableScenery, 
                                                                itemInRadius[i].transform.position + Vector3.up);
                            itemInRadius[i].SpawnHarvestItems();
                            Destroy(itemInRadius[i].gameObject);

                            //限制最大割草数量
                            reapCount++;
                            if (reapCount >= Settings.reapAmount) { break; }
                        }
                        EventHandler.CallPlaySoundEvent(SoundName.Reap);
                        break;
                    //建造家具
                    case ItemType.Furniture:
                        //在地图上生成物品 ItemManager
                        //移除图纸 InventoryManager
                        //移除建造材料 InventoryManager
                        EventHandler.CallBuildFurnitureEvent(itemDetails.itemID,mouseWorldPos);
                        break;
                }
                //改变地形后及时更新瓦片信息,换场景后重新Display
                UpdateTileDetails(currentTile);
            }

        }
        /// <summary>
        /// 通过鼠标位置获取种植物的脚本
        /// </summary>
        /// <param name="mouseWorldPos"></param>
        /// <returns></returns>
        public Crop GetCropObject(Vector3 mouseWorldPos)
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);
            Crop currentCrop = null;

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].GetComponent<Crop>() != null)
                {
                    currentCrop = colliders[i].GetComponent<Crop>();
                }
            }
            return currentCrop;
        }


        /// <summary>
        /// 根据地图信息生成瓦片信息字典
        /// </summary>
        /// <param name="mapData">地图信息</param>
        private void InitTileDetailsDict(MapData_SO mapData)
        {
            foreach (TileProperty tileProperty in mapData.tileProperties)
            {
                TileDetails tileDetails = new TileDetails() 
                {
                    gridX = tileProperty.tileCoordinate.x,
                    gridY = tileProperty.tileCoordinate.y,
                };
                //字典的key
                string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + mapData.sceneName;
                if (GetTileDetails(key) != null)
                {
                    tileDetails = GetTileDetails(key);
                }

                switch (tileProperty.gridType)
                {
                    case GridType.Diggable:
                        tileDetails.canDig = tileProperty.boolTypeValue; 
                        break;
                    case GridType.DropItem:
                        tileDetails.canDropItem = tileProperty.boolTypeValue;
                        break;
                    case GridType.PlaceFuniture:
                        tileDetails.canPlaceFurniture = tileProperty.boolTypeValue;
                        break;
                    case GridType.NPCObstacle:
                        tileDetails.isNPCObstacle = tileProperty.boolTypeValue;
                        break;
                }

                if (GetTileDetails(key) != null)
                {
                    tileDetailsDic[key] = tileDetails;
                }
                else
                {
                    tileDetailsDic.Add(key,tileDetails);
                }
            }
        }
        /// <summary>
        /// 根据Key返回瓦片信息
        /// </summary>
        /// <param name="key">x+y+地图名字</param>
        /// <returns></returns>
        public TileDetails GetTileDetails(string key)
        {
            if (tileDetailsDic.ContainsKey(key))
            {
                return tileDetailsDic[key];
            }
            //找不到key会直接抛出 KeyNotFoundException 异常，导致程序中断
            //所以需要手写返回null
            return null;
        }
        /// <summary>
        /// 根据鼠标的网格坐标返回当前瓦片信息
        /// </summary>
        /// <param name="mouseGridPos">鼠标的网格坐标</param>
        /// <returns></returns>
        public TileDetails GetTileDetailsOnMousePosition(Vector3Int mouseGridPos)
        {
            //拿到字典的key
            string key = mouseGridPos.x + "x" + mouseGridPos.y + "y" + SceneManager.GetActiveScene().name;
            return GetTileDetails(key);
        }
        /// <summary>
        /// 显示挖坑瓦片
        /// </summary>
        /// <param name="tile"></param>
        private void SetDigGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if (digTilemap != null)
            {
                digTilemap.SetTile(pos, digTile);
            }
        }
        /// <summary>
        /// 显示浇水瓦片
        /// </summary>
        /// <param name="tile"></param>
        private void SetWaterGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if (waterTile != null)
            {
                waterTilemap.SetTile(pos, waterTile);
            }
        }
        /// <summary>
        /// 更新瓦片数据存入字典
        /// </summary>
        /// <param name="tileDetails">瓦片详情</param>
        public void UpdateTileDetails(TileDetails tileDetails)
        {
            string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + SceneManager.GetActiveScene().name;
            if (tileDetailsDic.ContainsKey(key))
            {
                tileDetailsDic[key] = tileDetails;
            }
            else
            {
                tileDetailsDic.Add(key, tileDetails);   
            }

        }
        /// <summary>
        /// 删除瓦片刷新地图
        /// </summary>
        private void RefreshMap()
        {
            if (digTilemap != null)
            {
                digTilemap.ClearAllTiles();
            }
            if(waterTilemap != null)
            {
                waterTilemap.ClearAllTiles();
            }

            foreach (var crop in FindObjectsOfType<Crop>())
            {
                Destroy(crop.gameObject);
            }

            DisplayMap(SceneManager.GetActiveScene().name);
        }
        /// <summary>
        /// 显示挖坑和浇水的地图瓦片
        /// </summary>
        /// <param name="sceneName">场景名</param>
        private void DisplayMap(string sceneName)
        {
            foreach (var tile in tileDetailsDic)
            {
                string key = tile.Key;
                TileDetails tileDetails = tile.Value;
                //检查Key里面有没有包含场景名字
                if (key.Contains(sceneName))
                {
                    if(tileDetails.daysSinceDug > -1)
                    {
                        SetDigGround(tileDetails);
                    }
                    if(tileDetails.daySinceWatered > -1)
                    {
                        SetWaterGround(tileDetails);
                    }
                    //有种子
                    if(tileDetails.seedItemID > -1)
                    {
                        EventHandler.CallPlantSeedEvent(tileDetails.seedItemID, tileDetails);
                    }
                }
            }
        }

        /// <summary>
        /// 检测鼠标周围有无可收割的物品
        /// </summary>
        /// <param name="tool">工具详情，为了拿检测半径</param>
        /// <returns></returns>
        public bool HaveReapableItemsInRadius(Vector3 mouseWorldPos , ItemDetails tool)
        {
            itemInRadius = new List<ReapItem>();
            //限制数组上限20个，防止检测太多东西
            Collider2D[] colliders = new Collider2D[20];
            Physics2D.OverlapCircleNonAlloc(mouseWorldPos, tool.itemUseRadius, colliders);

            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] != null)
                    {
                        if (colliders[i].GetComponent<ReapItem>())
                        {
                            ReapItem item = colliders[i].GetComponent<ReapItem>();
                            itemInRadius.Add(item);
                        }
                    }
                }
            }
            //检测到杂草就返回True
            return itemInRadius.Count > 0;
        }
        /// <summary>
        /// 根据场景名字构建网格范围，输出范围和原点
        /// </summary>
        /// <param name="sceneName">场景名字</param>
        /// <param name="gridDimension">网格范围</param>
        /// <param name="gridOrigin">网格原点</param>
        /// <returns>是否有当前场景信息</returns>
        public bool GetGridDimensions(string sceneName, out Vector2Int gridDimension, out Vector2Int gridOrigin)
        {
            gridDimension = Vector2Int.zero;
            gridOrigin = Vector2Int.zero;

            foreach (var mapData in mapDataList)
            {
                if(mapData.sceneName == sceneName)
                {
                    gridDimension.x = mapData.gridWidth;
                    gridDimension.y = mapData.gridHeight;

                    gridOrigin.x = mapData.originX;
                    gridOrigin.y = mapData.originY;

                    return true;
                }
            }
            return false;
        }

        public GameSaveData GenerateSavaData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.tileDetailsDic = this.tileDetailsDic;
            saveData.firstLoadDic = this.firstLoadDic;
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            this.tileDetailsDic = saveData.tileDetailsDic;
            this.firstLoadDic = saveData.firstLoadDic;
        }
    }
}