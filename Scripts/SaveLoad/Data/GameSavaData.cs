using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LouisFrice.Save
{
    [System.Serializable]
    public class GameSaveData
    {
        //场景名
        public string dataSceneName;
        //玩家金币
        public int playerMoney;
        //存储人物坐标，key是人物名
        public Dictionary<string, SerializableVector3> characterPosDic;
        //背包+箱子库存
        public Dictionary<string, List<InventoryItem>> inventoryDic;
        //场景物品
        public Dictionary<string, List<SceneItem>> sceneItemDic;
        //场景家具
        public Dictionary<string, List<SceneFurniture>> sceneFurnitureDic;
        //key是场景名+坐标
        public Dictionary<string, TileDetails> tileDetailsDic;
        //场景是否第一次加载
        public Dictionary<string, bool> firstLoadDic;
        //时间
        public Dictionary<string, int> timeDic;

        //NPC
        public string targetScene;
        public bool interactable;
        //动画片段的文件ID
        public int animationInstanceID;
        //总音量
        public float MasterVolume;
    }
}