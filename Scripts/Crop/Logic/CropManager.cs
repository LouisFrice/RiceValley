using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LouisFrice.CropPlant
{


    public class CropManager : Singleton<CropManager>
    {
        public CropDataList_SO CropData;
        private Transform cropParent;
        private Grid currentGrid;
        private Season currentSeason;


        private void OnEnable()
        {
            EventHandler.PlantSeedEvent += OnPlantSeedEvent;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
        }
        private void OnDisable()
        {
            EventHandler.PlantSeedEvent -= OnPlantSeedEvent;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
        }

        private void OnGameDayEvent(int day, Season season)
        {
            currentSeason = season;
        }

        private void OnAfterSceneLoadedEvent()
        {
            currentGrid = FindObjectOfType<Grid>();
            cropParent = GameObject.FindWithTag("CropParent").transform;
        }

        private void OnPlantSeedEvent(int id, TileDetails tileDetails)
        {
            CropDetails currentCrop = GetCropDetails(id);
            //当前种子不为空 && 当前季节可以种植 && 瓦片里没有种子
            if (currentCrop != null && SeasonAvailable(currentCrop) && tileDetails.seedItemID == -1)
            {
                //开始种植
                tileDetails.seedItemID = id;
                tileDetails.growthDays = 0;
                //显示农作物
                DisplayCropPlant(tileDetails, currentCrop);
            }
            //格子里存在种子，用于刷新地图
            else if (tileDetails.seedItemID != -1)
            {
                DisplayCropPlant(tileDetails, currentCrop);
            }
        }
        /// <summary>
        /// 显示农作物
        /// </summary>
        /// <param name="tileDetails">瓦片详情</param>
        /// <param name="cropDetails">种子详情</param>
        private void DisplayCropPlant(TileDetails tileDetails , CropDetails cropDetails)
        {
            //成长阶段 5
            int growthStages = cropDetails.growthDays.Length;
            //当前阶段
            int currentStage = 0;
            //总天数 1+2+3+4+5 = 15天
            int dayCounter = cropDetails.TotalGrowthDays;


            //假设作物已生长 tileDetails.growthDays = 10 天
            //初始值：dayCounter = 15（总天数），currentStage = 0，循环从 i = 4（最后一个阶段）开始。
            //第一次循环（i = 4，阶段 4）：
            //判断：10 >= 15？→ 否（10 天 <= 15 天，说明还没到阶段 4）。
            //调整 dayCounter：减去阶段 4 的天数（5）→ dayCounter = 15 - 5 = 10。
            //第二次循环（i = 3，阶段 3）：
            //判断：10 >= 10？→ 是（10 天 <= 10 天，到阶段 3）。
            //因此 currentStage = 3，跳出循环

            //倒序计算当前的生长阶段，更高效但不符合直观思想
            for (int i = growthStages - 1; i >= 0; i--)
            {
                //瓦片的生长天数 >= dayCounter
                if (tileDetails.growthDays >= dayCounter)
                {
                    currentStage = i;
                    break;
                }
                //剩下的天数，dayCounter - 该阶段的天数
                dayCounter -= cropDetails.growthDays[i];
            }

            //获取当前阶段的prefab
            GameObject cropPrefab = cropDetails.growthPrefabs[currentStage];
            Sprite cropSprite = cropDetails.growthSprites[currentStage];

            //gridX和Y是在网格左下角，需要+0.5显示到正中间
            Vector3 pos = new Vector3(tileDetails.gridX + 0.5f, tileDetails.gridY + 0.5f , 0);

            GameObject cropInstance = Instantiate(cropPrefab, pos, Quaternion.identity, cropParent);
            cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;
            //拿到该Crop的详情
            cropInstance.GetComponent<Crop>().cropDetails = cropDetails;
            cropInstance.GetComponent<Crop>().tileDetails = tileDetails;
        }

        /// <summary>
        /// 通过物品ID找到种子信息
        /// </summary>
        /// <param name="id">物品ID</param>
        /// <returns></returns>
        public CropDetails GetCropDetails(int id)
        {
            return CropData.CropDetailsList.Find(cropDetails => cropDetails.seedItemID == id);
        }

        /// <summary>
        /// 判断种子当前季节是否能种植
        /// </summary>
        /// <param name="cropDetails">种子详情</param>
        /// <returns></returns>
        private bool SeasonAvailable(CropDetails cropDetails)
        {
            for (int i = 0; i < cropDetails.seasons.Length; i++)
            {
                if (cropDetails.seasons[i] == currentSeason)
                {
                    return true;
                }
            }
            return false;
        }
    }


}