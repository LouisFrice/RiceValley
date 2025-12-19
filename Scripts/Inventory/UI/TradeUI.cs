using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace LouisFrice.Inventory
{
    public class TradeUI : MonoBehaviour
    {
        public Image itemIcon;
        public Text itemName;
        public InputField tradeAmount;
        public Button submitButton;
        public Button cancelButton;

        private ItemDetails item;
        private bool isSellTrade;

        private void Awake()
        {
            cancelButton.onClick.AddListener(CancelTrade);
            submitButton.onClick.AddListener(TradeItem);
        }

        /// <summary>
        /// 设置交易UI显示
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isSell"></param>
        public void SetUpTradeUI(ItemDetails item, bool isSell)
        {
            this.item = item;
            itemIcon.sprite = item.itemIcon;
            itemName.text = item.ItemName;
            isSellTrade = isSell;
            tradeAmount.text = string.Empty;
        }
        /// <summary>
        /// 取消交易
        /// </summary>
        private void CancelTrade()
        {
            this.gameObject.SetActive(false);
        }
        /// <summary>
        /// 交易物品
        /// </summary>
        private void TradeItem()
        {
            //string转int
            int amount = Convert.ToInt32(tradeAmount.text);
            InventoryManager.Instance.TradeItem(item, amount, isSellTrade);
            //交易完成关闭窗口
            CancelTrade();
        }
    }
}