using LouisFrice.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LouisFrice.Inventory
{
    public class ItemManager : MonoBehaviour,ISaveable
    {
        public Item itemPrefab;
        //物体扔出去的预制体
        public Item bounceItemPrefab;
        private Transform itemParent;

        private Transform playerTransform => FindObjectOfType<PlayerController>().transform;

        public string GUID => GetComponent<DataGUID>().GUID;

        //[场景名]-当前场景物体列表
        //记录场景的Item
        private Dictionary<string,List<SceneItem>> sceneItemDic = new Dictionary<string,List<SceneItem>>();
        //记录场景的家具
        private Dictionary<string,List<SceneFurniture>> sceneFurnitureDic = new Dictionary<string, List<SceneFurniture>>();

        private void Start()
        {
            //注册当前实体去存档
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }

        private void OnEnable()
        {
            EventHandler.InstantiateItemInScene += OninstantiateItemInScene;
            EventHandler.DropItemEvent += OnDropItemEvent;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
			EventHandler.StartNewGameEvent += OnStartNewGameEvent;
		}
        private void OnDisable()
        {
            EventHandler.InstantiateItemInScene -= OninstantiateItemInScene;
            EventHandler.DropItemEvent -= OnDropItemEvent;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
			EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
		}

        private void OnStartNewGameEvent(int index)
        {
            sceneItemDic.Clear();
            sceneFurnitureDic.Clear();
        }

        private void OnBuildFurnitureEvent(int id, Vector3 MousePos)
        {
            BlueprintDetails blueprintDetails = InventoryManager.Instance.blueprintData.GetBlueprintDetails(id);
            GameObject buildItem = Instantiate(blueprintDetails.buildPrefab, MousePos, Quaternion.identity, itemParent);
            if (buildItem.GetComponent<Box>())
            {
                //通过箱子数量赋值索引，0、1、2、3
                buildItem.GetComponent<Box>().index = InventoryManager.Instance.BoxDataAmount;
                buildItem.GetComponent<Box>().InitBox(buildItem.GetComponent<Box>().index);
            }
        }

        private void OnDropItemEvent(int id, Vector3 mousePos, ItemType itemType)
        {
            //如果是种子就直接丢掉不生成
            if(itemType == ItemType.Seed) { return; }

            //扔物品在世界中
            Item item = Instantiate(bounceItemPrefab, playerTransform.position, Quaternion.identity, itemParent);
            item.itemID = id;
            //归一化获得模长为1的纯方向
            Vector3 dir = (mousePos - playerTransform.position).normalized;

            item.GetComponent<ItemBounce>().InitBounceItem(mousePos, dir);
        }

        private void OnBeforeSceneUnloadEvent()
        {
            //场景切换前获取所有的物品
            GetAllSceneItems();
            GetAllSceneFurniture();
        }

        private void OnAfterSceneLoadedEvent()
        {
            itemParent = GameObject.FindWithTag("ItemParent").transform;
            //场景切换后刷新物品
            RecreateAllItems();
            RebuildFurniture();
        }

        private void OninstantiateItemInScene(int id, Vector3 pos)
        {
            Item item = Instantiate(bounceItemPrefab, pos, Quaternion.identity, itemParent);
            item.itemID = id;

            item.GetComponent<ItemBounce>().InitBounceItem(pos, Vector3.up);
        }

        /// <summary>
        /// 获取当前场景上的所有物体保存在sceneItemDic字典中
        /// </summary>
        public void GetAllSceneItems()
        {
            List<SceneItem> currentSceneItems = new List<SceneItem>();
            //比如：世界有5个物品，玩家拾取了2个，切换场景前记录3个
            //获得场景上的所有物品存入currentSceneItems列表
            foreach (var item in FindObjectsOfType<Item>())
            {
                SceneItem sceneItem = new SceneItem()
                {
                    itemID = item.itemID,
                    position = new SerializableVector3(item.transform.position)
                };
                currentSceneItems.Add(sceneItem);
            }

            //当前激活的场景是否已经存在字典中
            if (sceneItemDic.ContainsKey(SceneManager.GetActiveScene().name))
            {
                //更新数据
                sceneItemDic[SceneManager.GetActiveScene().name] = currentSceneItems;
            }
            else //新场景
            {
                sceneItemDic.Add(SceneManager.GetActiveScene().name, currentSceneItems);
            }
        }

        /// <summary>
        /// 获得当前场景所有家具
        /// </summary>
        public void GetAllSceneFurniture()
        {
            List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();
            //比如：世界有5个物品，玩家拾取了2个，切换场景前记录3个
            //获得场景上的所有物品存入currentSceneItems列表
            foreach (var item in FindObjectsOfType<Furniture>())
            {
                SceneFurniture sceneFurniture = new SceneFurniture()
                {
                    itemID = item.itemID,
                    position = new SerializableVector3(item.transform.position)
                };
                //如果是箱子就记录箱子index
                if (item.GetComponent<Box>())
                {
                    sceneFurniture.boxIndex = item.GetComponent<Box>().index;
                }
                currentSceneFurniture.Add(sceneFurniture);
            }

            //当前激活的场景是否已经存在字典中
            if (sceneFurnitureDic.ContainsKey(SceneManager.GetActiveScene().name))
            {
                //更新数据
                sceneFurnitureDic[SceneManager.GetActiveScene().name] = currentSceneFurniture;
            }
            else //新场景
            {
                sceneFurnitureDic.Add(SceneManager.GetActiveScene().name, currentSceneFurniture);
            }
        }

        /// <summary>
        /// 加载场景后刷新重建当前场景的物品，防止被拾取后重复生成
        /// </summary>
        private void RecreateAllItems() 
        {
            List<SceneItem> currentSceneItems = new List<SceneItem>();

            if(sceneItemDic.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneItems))
            {
                if (currentSceneItems != null)
                {
                    //删除场景上的所有物体
                    foreach (var item in FindObjectsOfType<Item>())
                    {
                        Destroy(item.gameObject);
                    }
                    //重新创建列表里存的物体
                    foreach (SceneItem item in currentSceneItems)
                    {
                        Item newItem = Instantiate(itemPrefab, item.position.ToVector3(), Quaternion.identity, itemParent);
                        newItem.Init(item.itemID);
                    }
                }
                    
            }
        }
        /// <summary>
        /// 重新生成家具在场景中
        /// </summary>
        private void RebuildFurniture()
        {
            List<SceneFurniture> currentSceneFurnitures = new List<SceneFurniture>();
            if(sceneFurnitureDic.TryGetValue(SceneManager.GetActiveScene().name , out currentSceneFurnitures))
            {
                if(currentSceneFurnitures != null)
                {
                    foreach (SceneFurniture sceneFurniture in currentSceneFurnitures)
                    {
                        BlueprintDetails blueprintDetails = InventoryManager.Instance.blueprintData.GetBlueprintDetails(sceneFurniture.itemID);
                        GameObject buildItem = Instantiate(blueprintDetails.buildPrefab, sceneFurniture.position.ToVector3(), Quaternion.identity, itemParent);
                        if (buildItem.GetComponent<Box>())
                        {
                            buildItem.GetComponent<Box>().InitBox(sceneFurniture.boxIndex);
                        }
                    }
                }
            }
        }

        public GameSaveData GenerateSavaData()
        {
            //先保存一下数据
            GetAllSceneItems();
            GetAllSceneFurniture();

            GameSaveData saveData = new GameSaveData();
            saveData.sceneItemDic = this.sceneItemDic;
            saveData.sceneFurnitureDic = this.sceneFurnitureDic;
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            sceneItemDic = saveData.sceneItemDic;
            sceneFurnitureDic = saveData.sceneFurnitureDic;

            //刷新场景
            RecreateAllItems();
            RebuildFurniture();
        }
    }


}