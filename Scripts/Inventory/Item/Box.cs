using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LouisFrice.Inventory
{
    public class Box : MonoBehaviour
    {
        public InventoryBag_SO boxBagTemplate;
        public InventoryBag_SO boxBagData;
        public GameObject mouseIcon;

        private bool canOpen = false;
        private bool isOpen;

        public int index;

        private void OnEnable()
        {
            if(boxBagData == null)
            {
                //创建一个boxbag副本
                boxBagData = Instantiate(boxBagTemplate);
            }
        }
        private void Update()
        {
            //按鼠标右键打开
            if(!isOpen && canOpen && Input.GetMouseButtonDown(1))
            {
                //打开箱子
                EventHandler.CallBaseBagOpenEvent(SlotType.Box, boxBagData);
                isOpen = true;
            }
            //离开碰撞范围关闭
            if (isOpen && !canOpen)
            {
                //关闭箱子
                EventHandler.CallBaseBagCloseEvent(SlotType.Box, boxBagData);
                isOpen= false;
            }
            //按Esc关闭
            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                //关闭箱子
                EventHandler.CallBaseBagCloseEvent(SlotType.Box, boxBagData);
                isOpen = false;
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                canOpen = true;
                mouseIcon.gameObject.SetActive(true);
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                canOpen = false;
                mouseIcon.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 初始化箱子数据
        /// </summary>
        /// <param name="boxIndex"></param>
        public void InitBox(int boxIndex)
        {
            index = boxIndex;
            string key = this.name + index;
            //刷新地图读取数据
            if (InventoryManager.Instance.GetBoxDataList(key) != null)
            {
                //拿到字典里的数据
                boxBagData.itemList = InventoryManager.Instance.GetBoxDataList(key);
            }
            //新箱子
            else
            {
                InventoryManager.Instance.AddBoxDataDic(this);
            }
        }
    }
}