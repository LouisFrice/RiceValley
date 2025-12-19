using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LouisFrice.Inventory
{
    public class SlotUI : MonoBehaviour,IPointerClickHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        [Header("组件获取")]
        [SerializeField]
        private Image slotImage;
        public Image slotHighlight;
        [SerializeField]
        private TextMeshProUGUI amountText;
        [SerializeField]
        private Button button;
        public SlotType slotType;
        //物品是否可选
        public bool isSelected;
        //物品格子序号
        public int slotIndex;

        //物品信息
        public ItemDetails itemDetails;
        public int itemAmount;

        //拿到父级InventoryUI
        public InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();

        //背包类型转库存位置
        public InventoryLocation Location
        {
            get 
            {
                return slotType switch
                {
                    SlotType.Bag => InventoryLocation.Player,
                    SlotType.Box => InventoryLocation.Box,
                    _ => InventoryLocation.Player,
                };
            }
        }

        private void Start()
        {
            isSelected = false;
            if (itemDetails == null)
            {
                UpdataEmptySlot();
            }

        }

        /// <summary>
        /// 更新格子UI信息
        /// </summary>
        /// <param name="item">物品信息</param>
        /// <param name="amount">物品数量</param>
        public void UpdateSlot(ItemDetails item, int amount)
        {
            itemDetails = item;
            slotImage.sprite = item.itemIcon;
            itemAmount = amount;
            amountText.text = amount.ToString();
            slotImage.enabled = true;
            button.interactable = true;
        }
        /// <summary>
        /// 更新物品格子为空
        /// </summary>
        public void UpdataEmptySlot()
        {
            if (isSelected)
            {
                isSelected = false;
                //取消高亮
                inventoryUI.UpdateSlotHighlight(-1);
                //通知物品被选中的状态和信息
                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
            }
            //清空物品信息
            itemDetails = null;

            slotImage.enabled = false;
            amountText.text = string.Empty;
            button.interactable = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(itemDetails == null) { return; } 
            isSelected = !isSelected;
            //高亮显示
            inventoryUI.UpdateSlotHighlight(slotIndex);
            //如果选中的是背包
            if(slotType == SlotType.Bag)
            {
                //通知物品被选中的状态和信息
                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if(itemAmount != 0)
            {
                inventoryUI.dragItem.enabled = true;
                inventoryUI.dragItem.sprite = slotImage.sprite;
                inventoryUI.dragItem.SetNativeSize(); 

                isSelected = true;
                inventoryUI.UpdateSlotHighlight(slotIndex);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            //UI跟随鼠标位置移动
            inventoryUI.dragItem.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.enabled = false;
            //鼠标碰撞的物体不为空
            if(eventData.pointerCurrentRaycast.gameObject != null)
            {
                //鼠标碰撞的物体不是背包格子UI
                if(eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null)
                {
                    return;
                }

                SlotUI targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();
                int targetIndex = targetSlot.slotIndex;

                //背包和背包之间的物品交换
                if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag)
                {
                    InventoryManager.Instance.SwapItem(slotIndex, targetIndex);
                }
                //商店拖到背包，买物品
                else if (slotType == SlotType.Shop && targetSlot.slotType == SlotType.Bag)
                {
                    EventHandler.CallShowTradeUI(itemDetails, false);
                }
                //背包拖到商店，卖物品
                else if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Shop)
                {
                    EventHandler.CallShowTradeUI(itemDetails, true);
                }
                //背包拖到箱子 和 箱子拖到背包
                else if (slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && slotType != targetSlot.slotType)
                {
                    //跨背包交换物品
                    InventoryManager.Instance.SwapItem(slotIndex, targetIndex, Location, targetSlot.Location);
                }

                //隐藏高亮显示
                inventoryUI.UpdateSlotHighlight(-1);
            }
            ////测试丢在地上
            //else
            //{
            //    if (itemDetails.canDropped)
            //    {
            //        //得到鼠标的当前的世界位置
            //        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
            //        EventHandler.CallInstantiateItemInScene(itemDetails.itemID, pos);
            //    }
            //}
        }
    }

}