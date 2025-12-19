using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CropDataList_SO", menuName = "Crop/CropDataList")]
public class CropDataList_SO : ScriptableObject
{
    public List<CropDetails> CropDetailsList;

    public CropDetails GetCropDetails(int itemID)
    {
        return CropDetailsList.Find((item) => { return item.seedItemID == itemID; });
    }
}
