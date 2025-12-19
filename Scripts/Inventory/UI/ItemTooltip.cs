using LouisFrice.Inventory;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemTooltip : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private TextMeshProUGUI typeText;
    [SerializeField]
    private TextMeshProUGUI descriptionText;
    [SerializeField]
    private Text valueText;
    [SerializeField]
    private GameObject bottomPart;
    [Header("建造")]
    public GameObject resourcePanel;
    [SerializeField]
    private Image[] resourceItem;

    public void SetupTooltip(ItemDetails itemDetails, SlotType slotType)
    {
        nameText.text = itemDetails.ItemName;
        typeText.text = GetItemType(itemDetails.itemType);
        descriptionText.text = "描述: \n" + itemDetails.itemDescription;
        //如果是可售的就显示价格
        if (itemDetails.itemType == ItemType.Seed || itemDetails.itemType == ItemType.Commodity || itemDetails.itemType == ItemType.Furniture)
        {
            bottomPart.SetActive(true);
            int price = itemDetails.itemPrice;
            if (slotType == SlotType.Bag)
            {
                price = (int)(price * itemDetails.sellPercentage);
            }
            valueText.text = price.ToString();
        }
        else
        {
            bottomPart.SetActive(false);
        }
        //强制重新渲染，否则UI显示上会出现修改多行文字时的延迟，导致文字排列不正常
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private string GetItemType(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Seed => "种子",
            ItemType.Commodity => "商品",
            ItemType.Furniture => "家具",
            ItemType.ReapTool => "工具",
            ItemType.HoeTool => "工具",
            ItemType.CollectTool => "工具",
            ItemType.BreakTool => "工具",
            ItemType.WaterTool => "工具",
            ItemType.ChopTool => "工具",
            _ => "无"    //类似于default
        };
    }

    public void SetupResourcePanel(int id)
    {
        BlueprintDetails blueprintDetails = InventoryManager.Instance.blueprintData.GetBlueprintDetails(id);
        for (int i = 0; i < resourceItem.Length; i++)
        {
            if(i < blueprintDetails.resourceItem.Length)
            {
                InventoryItem item = blueprintDetails.resourceItem[i];
                resourceItem[i].gameObject.SetActive(true);
                resourceItem[i].sprite = InventoryManager.Instance.GetItemDetails(item.itemID).itemIcon;
                resourceItem[i].transform.GetChild(0).GetComponent<Text>().text = item.itemAmount.ToString();
            }
            else
            {
                resourceItem[i].gameObject.SetActive(false);
            }
        }
    }
}
