using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlueprintDataList_SO", menuName = "Inventory/BlueprintDataList_SO")]
public class BlueprintDataList_SO : ScriptableObject
{
    public List<BlueprintDetails> blueprintDataList;

    public BlueprintDetails GetBlueprintDetails(int itemID)
    {
        return blueprintDataList.Find((item) => { return item.id == itemID; });
    }
}
