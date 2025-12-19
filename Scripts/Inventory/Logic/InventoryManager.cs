using LouisFrice.Save;
using System.Collections.Generic;
using UnityEngine;

namespace LouisFrice.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>,ISaveable
    {
        [Header("物品数据")]
        public ItemDataList_SO itemDataList_SO;
        [Header("背包数据")]
        //玩家新背包
        public InventoryBag_SO playerbagTemp;
        public InventoryBag_SO playerBag;
        private InventoryBag_SO currentBoxBag;
        [Header("蓝图数据")]
        public BlueprintDataList_SO blueprintData;
        [Header("交易")]
        public int playerMoney;
        
        private Dictionary<string,List<InventoryItem>> boxDataDic = new Dictionary<string,List<InventoryItem>>();

        public int BoxDataAmount => boxDataDic.Count;

        public string GUID => GetComponent<DataGUID>().GUID;

        private void Start()
        {
            ////游戏一开始就更新背包UI
            //EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
            //注册当前实体去存档
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }

        private void OnEnable()
        {
            EventHandler.DropItemEvent += OnDropItemEvent;
            EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;

		}
        private void OnDisable()
        {
            EventHandler.DropItemEvent -= OnDropItemEvent;
            EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
            EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
			EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
		}

        private void OnStartNewGameEvent(int index)
        {
            playerBag = Instantiate(playerbagTemp);
            playerMoney = Settings.playerStartMoney;
            boxDataDic.Clear();
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag.itemList);
        }

        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bag_SO)
        {
            //拿到打开箱子的数据
            currentBoxBag = bag_SO;
        }

        private void OnBuildFurnitureEvent(int id, Vector3 mousePos)
        {
            RemoveItem(id, 1);
            BlueprintDetails blueprintDetails = blueprintData.GetBlueprintDetails(id);
            foreach (var item in blueprintDetails.resourceItem)
            {
                RemoveItem(item.itemID, item.itemAmount);
            }

        }

        private void OnHarvestAtPlayerPosition(int id)
        {
            //是否已经有该物品 -1代表没有该物品
            int index = GetItemIndexInBag(id);
            //通过序号增加背包里的物品
            AddItemAtIndex(id, index, 1);
            //更新背包UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        private void OnDropItemEvent(int id, Vector3 pos, ItemType itemType)
        {
            RemoveItem(id, 1);
        }

        /// <summary>
        /// 通过ID返回物品信息
        /// </summary>
        /// <param name="id">物品ID</param>
        /// <returns></returns>
        public ItemDetails GetItemDetails(int id)
        {
            return itemDataList_SO.itemDetailList.Find(item => item.itemID == id);
        }

        /// <summary>
        /// 增加物品到Player背包里
        /// </summary>
        /// <param name="item"></param>
        /// <param name="toDestroy">是否摧毁该物品</param>
        public void AddItem(Item item , bool toDestroy)
        {
            //是否已经有该物品 -1代表没有该物品
            int index = GetItemIndexInBag(item.itemID);
            //通过序号增加背包里的物品
            AddItemAtIndex(item.itemID, index, 1);

            Debug.Log("捡到了" + item.itemDetails.ItemName);
            if (toDestroy)
            {
                Destroy(item.gameObject);
            }

            //更新背包UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
        /// <summary>
        /// 检查背包是否有空位
        /// </summary>
        /// <returns>true代表有空位</returns>
        private bool CheckBagCapacity()
        {
            for (int i = 0; i < playerBag.itemList.Count; i++) {
                //背包结构体默认ID是0，代表空位
                if (playerBag.itemList[i].itemID == 0)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 通过物品ID找到背包里的物品序号
        /// </summary>
        /// <param name="id">物品ID</param>
        /// <returns>有则返回序号，没有则返回-1</returns>
        private int GetItemIndexInBag(int id)
        {
            for (int i = 0; i < playerBag.itemList.Count; i++)
            {
                //找到该ID返回序号
                if (playerBag.itemList[i].itemID == id)
                {
                    return i;
                }
            }
            //找不到返回-1
            return -1;
        }
        /// <summary>
        /// 通过背包里的序号添加物品
        /// </summary>
        /// <param name="id">物品ID</param>
        /// <param name="index">背包物品序号</param>
        /// <param name="amount">物品数量</param>
        private void AddItemAtIndex(int id, int index, int amount)
        {
            //-1代表背包里没有这个物品 && 背包有容量
            if (index == -1 && CheckBagCapacity())
            {
                InventoryItem item = new InventoryItem() { itemID = id, itemAmount = amount };
                for (int i = 0; i < playerBag.itemList.Count; i++)
                {
                    //背包结构体默认ID是0，代表空位
                    if (playerBag.itemList[i].itemID == 0)
                    {
                        //放在背包第一个空位
                        playerBag.itemList[i] = item;
                        break;
                    }
                }
            }
            //背包里有这个物品
            else
            {
                //加上背包里原本的数量
                int currentAmount = playerBag.itemList[index].itemAmount + amount;
                InventoryItem item = new InventoryItem() { itemID = id, itemAmount = currentAmount };
                playerBag.itemList[index] = item;   
            }
        }
        /// <summary>
        /// 交换背包里的两个物品位置
        /// </summary>
        /// <param name="fromIndex">当前位置索引</param>
        /// <param name="toIndex">目标位置索引</param>
        public void SwapItem(int fromIndex, int toIndex)
        {
            InventoryItem currentItem = playerBag.itemList[fromIndex];
            InventoryItem targetItem = playerBag.itemList[toIndex];

            //如果目标位置有物品就交换位置
            if(targetItem.itemID != 0)
            {
                playerBag.itemList[fromIndex] = targetItem;
                playerBag.itemList[toIndex] = currentItem;
            }
            //堆叠
            else if(currentItem.itemID == targetItem.itemID && currentItem.itemID != 0)
            {
                targetItem.itemAmount += currentItem.itemAmount;
                playerBag.itemList[toIndex] = targetItem;
                playerBag.itemList[fromIndex] = new InventoryItem();
            }
            //如果目标位置没有物品就移动物品
            else
            {
                playerBag.itemList[toIndex] = currentItem;
                playerBag.itemList[fromIndex] = new InventoryItem(); 
            }
            //换好位置后更新UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
        /// <summary>
        /// 跨背包交换数据，背包箱子交换
        /// </summary>
        /// <param name="fromIndex">当前索引</param>
        /// <param name="toIndex">目标索引</param>
        /// <param name="fromLocation">当前背包类型</param>
        /// <param name="toLocation">目标背包类型</param>
        public void SwapItem(int fromIndex, int toIndex, InventoryLocation fromLocation, InventoryLocation toLocation)
        {
            List<InventoryItem> currentBagList = GetItemList(fromLocation);
            List<InventoryItem> targetBagList = GetItemList(toLocation);

            InventoryItem currentItem = currentBagList[fromIndex];
            //箱子格子数量比目标位置大
            if (toIndex < targetBagList.Count)
            {
                InventoryItem targetItem = targetBagList[toIndex];
                //目标位置不为空 且 两个不同的物品 --交换位置
                if (targetItem.itemID != 0 && currentItem.itemID != targetItem.itemID)
                {
                    currentBagList[fromIndex] = targetItem;
                    targetBagList[toIndex] = currentItem;
                }
                //相同的两个物品 --堆叠
                else if (currentItem.itemID == targetItem.itemID)
                {
                    targetItem.itemAmount += currentItem.itemAmount;
                    targetBagList[toIndex] = targetItem;
                    currentBagList[fromIndex] = new InventoryItem();
                }
                //目标是空格子
                else
                {
                    targetBagList[toIndex] = currentItem;
                    currentBagList[fromIndex] = new InventoryItem();
                }
                //换好位置后更新UI
                EventHandler.CallUpdateInventoryUI(fromLocation,currentBagList);
                EventHandler.CallUpdateInventoryUI(toLocation, targetBagList);
            }
        }
        /// <summary>
        /// 根据存储位置返回背包列表
        /// </summary>
        /// <param name="inventoryLocation"></param>
        /// <returns></returns>
        public List<InventoryItem> GetItemList(InventoryLocation inventoryLocation)
        {
            return inventoryLocation switch
            {
                InventoryLocation.Player => playerBag.itemList,
                InventoryLocation.Box => currentBoxBag.itemList,
                _ => null,
            };
        }

        /// <summary>
        /// 移除背包物品
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="removeAmount">数量</param>
        public void RemoveItem(int id , int removeAmount)
        {
            int index = GetItemIndexInBag(id);
            if (playerBag.itemList[index].itemAmount > removeAmount)
            {
                int newAmount = playerBag.itemList[index].itemAmount - removeAmount;
                InventoryItem newItem = new InventoryItem()
                {
                    itemAmount = newAmount,
                    itemID = id
                };
                playerBag.itemList[index] = newItem;
            }
            else if(playerBag.itemList[index].itemAmount == removeAmount)
            {
                //如果相等直接清空
                InventoryItem newItem = new InventoryItem();
                playerBag.itemList[index] = newItem;
            }
            //更新UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player , playerBag.itemList);
        }
        /// <summary>
        /// 交易物品
        /// </summary>
        /// <param name="itemDetails">物品详情</param>
        /// <param name="amount">交易数量</param>
        /// <param name="isSellTrade">是否卖出</param>
        public void TradeItem(ItemDetails itemDetails , int amount, bool isSellTrade)
        {
            int cost = itemDetails.itemPrice * amount;
            //获取物品背包位置
            int index = GetItemIndexInBag(itemDetails.itemID);
            //卖物品
            if (isSellTrade)
            {
                if (playerBag.itemList[index].itemAmount >= amount)
                {
                    RemoveItem(itemDetails.itemID, amount);
                    //卖出的总价
                    cost = (int)(cost * itemDetails.sellPercentage);
                    playerMoney += cost;
                }
            }
            //买物品
            else if(playerMoney - cost >= 0)
            {
                if (CheckBagCapacity())
                {
                    AddItemAtIndex(itemDetails.itemID,index, amount);
                }
                playerMoney -= cost;
            }
            //刷新UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
        /// <summary>
        /// 检查物品库存是否满足建造需求
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CheckStock(int id)
        {
            BlueprintDetails blueprintDetails = blueprintData.GetBlueprintDetails(id);
            foreach (var resourceItem in blueprintDetails.resourceItem)
            {
                InventoryItem itemStock = playerBag.GetInventoryItem(resourceItem.itemID);
                if(itemStock.itemAmount >= resourceItem.itemAmount)
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 通过key获得箱子数据列表
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<InventoryItem> GetBoxDataList(string key)
        {
            if (boxDataDic.ContainsKey(key))
            {
                return boxDataDic[key];
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 添加箱子数据到字典
        /// </summary>
        /// <param name="box"></param>
        public void AddBoxDataDic(Box box)
        {
            string key = box.name + box.index;
            if (!boxDataDic.ContainsKey(key))
            {
                boxDataDic.Add(key, box.boxBagData.itemList);
            }
        }

        public GameSaveData GenerateSavaData()
        {
            GameSaveData saveData = new GameSaveData();
            //保存玩家金币
            saveData.playerMoney = this.playerMoney;

            //保存玩家背包数据
            saveData.inventoryDic = new Dictionary<string, List<InventoryItem>>();
            saveData.inventoryDic.Add(playerBag.name, playerBag.itemList);
            //循环保存所有箱子的数据
            foreach (var boxData in boxDataDic)
            {
                saveData.inventoryDic.Add(boxData.Key,boxData.Value);
            }
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            //加载保存的数据
            this.playerMoney = saveData.playerMoney;
            //初始化，否则直接赋值会报错
            playerBag = Instantiate(playerbagTemp);
            this.playerBag.itemList = saveData.inventoryDic[playerBag.name];

            foreach (var boxData in saveData.inventoryDic)
            {
                if (boxDataDic.ContainsKey(boxData.Key))
                {
                    this.boxDataDic[boxData.Key] = boxData.Value;
                }
            }
            //刷新玩家身上UI显示
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
    }
}
