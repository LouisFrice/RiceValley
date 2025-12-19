using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryBag_SO", menuName = "Inventory/InventoryBag")]
public class InventoryBag_SO : ScriptableObject
{
    public List<InventoryItem> itemList;

    public InventoryItem GetInventoryItem(int id)
    {
        return itemList.Find((item) => { return item.itemID == id; });
    }
}
