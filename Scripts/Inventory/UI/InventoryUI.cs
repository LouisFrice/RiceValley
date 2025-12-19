using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LouisFrice.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        public ItemTooltip itemTooltip;

        [Header("拖拽图片")]
        public Image dragItem;

        [Header("玩家背包UI")]
        [SerializeField]
        private GameObject bagUI;
        private bool bagOpened;
        [Header("通用背包")]
        [SerializeField]
        private GameObject baseBag;
        public GameObject shopSlotPrefab;
        public GameObject boxSlotPrefab;
        [Header("交易UI")]
        public TradeUI tradeUI;
        public TextMeshProUGUI playerMoneyText;


        [SerializeField]
        private SlotUI[] playerSlotUIs; //拿到10个快捷栏和16个背包格子
        [SerializeField]
        private List<SlotUI> baseBagSlots;

        private void Start()
        {
            //从快捷栏开始格子序号0-25
            for (int i = 0; i < playerSlotUIs.Length; i++)
            {
                playerSlotUIs[i].slotIndex = i; 
            }
            //拿到游戏对象在场景中是否处于激活状态
            bagOpened = bagUI.activeInHierarchy;

            //刷新玩家当前金币数量
            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                OpenBagUI();
            }
        }
        private void OnEnable()
        {
            EventHandler.UpdateInventoryUI += OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent += OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI += OnShowTradeUI;
        }

        private void OnDisable()
        {
            EventHandler.UpdateInventoryUI -= OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent -= OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI -= OnShowTradeUI;
        }

        private void OnShowTradeUI(ItemDetails item, bool isSell)
        {
            tradeUI.gameObject.SetActive(true);
            tradeUI.SetUpTradeUI(item, isSell);
        }

        private void OnBaseBagCloseEvent(SlotType slotType, InventoryBag_SO sO)
        {
            baseBag.SetActive(false);
            itemTooltip.gameObject.SetActive(false);
            UpdateSlotHighlight(-1);
            foreach (var slot in baseBagSlots)
            {
                Destroy(slot.gameObject);
            }
            baseBagSlots.Clear();

            //关闭玩家背包
            if (slotType == SlotType.Shop || slotType == SlotType.Box)
            {
                //右移玩家背包
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                bagUI.SetActive(false);
                bagOpened = false;
            }
        }

        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            //TODO:箱子prefab
            GameObject prefab = slotType switch
            {
                SlotType.Shop => shopSlotPrefab,
                SlotType.Box => boxSlotPrefab,
                _ => null,
            };

            baseBag.SetActive(true);
            baseBagSlots = new List<SlotUI>();

            for (int i = 0; i < bagData.itemList.Count; i++)
            {
                SlotUI slot = Instantiate(prefab, baseBag.transform.GetChild(0)).GetComponent<SlotUI>();
                slot.slotIndex = i;
                baseBagSlots.Add(slot);
            }

            //开启玩家背包
            if(slotType == SlotType.Shop || slotType == SlotType.Box)
            {
                //右移玩家背包
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(-0.1f, 0.5f);
                bagUI.SetActive(true);
                bagOpened = true;
            }

            //强制刷新Layout，否则排列显示会有异常
            LayoutRebuilder.ForceRebuildLayoutImmediate(baseBag.GetComponent<RectTransform>());
            //更新UI显示
            OnUpdateInventoryUI(InventoryLocation.Box, bagData.itemList);
        }

        private void OnBeforeSceneUnloadEvent()
        {
            //加载场景取消高亮选择
            UpdateSlotHighlight(-1); 
        }

        /// <summary>
        /// 更新快捷栏和背包UI
        /// </summary>
        /// <param name="location">格子位置</param>
        /// <param name="list">格子列表</param>
        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
        {
            switch (location)
            {
                //玩家背包
                case InventoryLocation.Player:
                    for (int i = 0; i < playerSlotUIs.Length; i++)
                    {
                        //该格子有物品就更新UI
                        if (list[i].itemAmount > 0)
                        {
                            ItemDetails item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            playerSlotUIs[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            //没有物品更新为空格子UI
                            playerSlotUIs[i].UpdataEmptySlot();
                        }
                    }
                    break;
                //商店和箱子
                case InventoryLocation.Box:
                    for (int i = 0; i < baseBagSlots.Count; i++)
                    {
                        //该格子有物品就更新UI
                        if (list[i].itemAmount > 0)
                        {
                            ItemDetails item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            baseBagSlots[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            //没有物品更新为空格子UI
                            baseBagSlots[i].UpdataEmptySlot();
                        }
                    }
                    break;
            }
            //刷新玩家当前金币数量
            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
        }
        /// <summary>
        /// 切换背包UI的激活状态
        /// </summary>
        public void OpenBagUI()
        {
            //切换背包UI的激活状态
            bagOpened = !bagOpened;
            bagUI.SetActive(bagOpened);
            if (!bagOpened)
            {
                itemTooltip.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 更新物品格子高亮显示
        /// </summary>
        /// <param name="index">物品序号</param>
        public void UpdateSlotHighlight(int index)
        {
            foreach(var slot in playerSlotUIs)
            {
                if(slot.isSelected && slot.slotIndex == index)
                {
                    slot.slotHighlight.gameObject.SetActive(true);
                }
                else
                {
                    slot.isSelected = false;
                    slot.slotHighlight.gameObject.SetActive(false);
                }
            }
        }
    }
}