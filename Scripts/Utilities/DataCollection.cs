
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] //Unity可以编辑和保存数据
public class ItemDetails
{
    public int itemID;
    public string ItemName;
    public ItemType itemType;
    public Sprite itemIcon;
    public Sprite itemOnWorldSprite;
    public string itemDescription;
    public int itemUseRadius;
    public bool canPickUp;
    public bool canDropped;
    public bool canCarried;
    public int itemPrice;
    [Range(0,1)]
    public float sellPercentage;
}

[System.Serializable]
public struct InventoryItem
{
    public int itemID;
    public int itemAmount;
}

[System.Serializable]
public class AnimatorType
{
    public PartType partType;
    public PartName partName;
    public AnimatorOverrideController overrideController;
}

[System.Serializable]
public class SerializableVector3
{
    //重新手写 Vector3
    //让 Vector3 数据能被通用序列化器（如 JSON 库）识别并正确序列化
    public float x, y, z;

    public SerializableVector3(Vector3 pos)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
    public Vector2Int ToVector2Int()
    {
        return new Vector2Int((int)x, (int)y);
    }
}

[System.Serializable]
public class SceneItem
{
    public int itemID;
    public SerializableVector3 position;
}

[System.Serializable]
public class TileProperty
{
    //瓦片位置
    public Vector2Int tileCoordinate;
    public bool boolTypeValue;
    //瓦片类型
    public GridType gridType;
}

[System.Serializable]
public class TileDetails
{
    public int gridX, gridY;
    public bool canDig;
    public bool canDropItem;
    public bool canPlaceFurniture;
    public bool isNPCObstacle;
    // -1 代表无数据
    public int daysSinceDug = -1;
    public int daySinceWatered = -1;
    public int seedItemID = -1;
    public int growthDays = -1;
    public int daysSinceLastHarvest = -1;
}

[System.Serializable]
public class NPCPosition
{
    public Transform npc;
    public string startScene;
    public Vector3 position;
}

[System.Serializable]
public class SceneRoute
{
    public string fromSceneName;
    public string gotoSceneName;
    public List<ScenePath> scenePathList;
}

[System.Serializable]
public class ScenePath
{
    public string sceneName;
    public Vector2Int formGridCell;
    public Vector2Int gotoGridCell;
}

[System.Serializable]
public class BlueprintDetails
{
    public int id;
    public InventoryItem[] resourceItem;
    public GameObject buildPrefab;
}

[System.Serializable]
public class SceneFurniture
{
    public int itemID;
    public SerializableVector3 position;
    public int boxIndex;
}

[System.Serializable]
public class LightDetails
{
    public Season season;
    //早上或晚上
    public LightShift lightShift;
    //灯光颜色
    public Color lightColor;
    //灯光强度
    public float intensity;
}

[System.Serializable]
public class SoundDetails
{
    public SoundName soundName;
    public AudioClip soundClip;
    [Range(0.1f, 1.5f)]
    public float soundPitchMin;
    [Range(0.1f, 1.5f)]
    public float soundPitchMax;
    [Range(0.1f, 1.5f)]
    public float soundVolume;
}

[System.Serializable]
public class SceneSoundItem
{
    [SceneName] 
    public string sceneName;
    public SoundName ambient;
    public SoundName music;
}