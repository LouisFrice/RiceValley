using LouisFrice.Transition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LouisFrice.Save
{
    /// <summary>
    /// 对应一个存档栏
    /// </summary>
    public class DataSlot
    {
        //游戏存档，key是GUID
        public Dictionary<string,GameSaveData> dataDic = new Dictionary<string,GameSaveData>();

        //用来UI显示游戏进度
        public string DataTime
        {
            get
            {
                string key = TimeManager.Instance.GUID;
                if (dataDic.ContainsKey(key))
                {
                    GameSaveData timeData = dataDic[key];
                    return timeData.timeDic["gameYear"] + "年" + timeData.timeDic["gameMonth"] + "月" + 
                           timeData.timeDic["gameDay"] + "日" + "-" + (Season)(timeData.timeDic["gameSeason"]);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string DataScene
        {
            get
            {
                string key = TransitionManager.Instance.GUID;
                if (dataDic.ContainsKey(key))
                {
                    GameSaveData sceneData = dataDic[key];
                    return sceneData.dataSceneName switch
                    {
                        "00.Beach" => "<<沙滩>>",
                        "01.Field" => "<<田地>>",
                        "02.Home" => "<<室内>>",
                        "03.Town" => "<<集市>>",
                        _ => string.Empty,
                    };
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}