using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CropDetails 
{
    public int seedItemID;
    [Header("不同阶段需要的天数")]
    public int[] growthDays;
    public int TotalGrowthDays
    {
        get
        {
            int amount = 0;
            foreach (var day in growthDays)
            {
                amount += day;
            }
            return amount;  
        }
    }
    [Header("不同生长阶段的物品Prefab")]
    public GameObject[] growthPrefabs;
    [Header("不同阶段的图片")]
    public Sprite[] growthSprites;
    [Header("可种植的季节")]
    public Season[] seasons;

    [Space]

    [Header("收割工具")]
    public int[] harvestToolItemID;
    [Header("每种工具使用次数")]
    public int[] requireActionCount;
    [Header("转换成新物品的ID")]
    public int transferItemID;

    [Space]

    [Header("收割果实的信息")]
    public int[] producedItemID;
    public int[] producedMaxAmount;
    public int[] producedMinAmount;
    public Vector2 spawnRadius;

    [Header("再次生长的时间")]
    public int daysToRegrow;
    public int regrowTimes;

    [Header("Options")]
    public bool generateAtPlayerPosition;
    public bool hasAnimation;
    public bool hasParticalEffect;

    //音效、特效
    public ParticleEffectType effectType;
    public Vector3 effectPos;
    public SoundName soundEffect;

    /// <summary>
    /// 检查当前工具是否可用
    /// </summary>
    /// <param name="toolID">工具ID</param>
    /// <returns></returns>
    public bool CheckToolAvailable(int toolID)
    {
        foreach (var id in harvestToolItemID)
        {
            if(id == toolID)
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 获得工具需要使用的次数，-1代表该工具无法使用
    /// </summary>
    /// <param name="toolID">工具ID</param>
    /// <returns></returns>
    public int GetTotalRequireCount(int toolID)
    {
        for (int i = 0; i < harvestToolItemID.Length; i++)
        {
            //工具的i和工具的使用次数i是对应的
            if (harvestToolItemID[i] == toolID)
            {
                return requireActionCount[i];
            }
        }
        return -1;
    }

    /// <summary>
    /// 检查种子当前季节能不能种植
    /// </summary>
    /// <param name="currentSeason"></param>
    /// <returns></returns>
    public bool CheckCropSeasonAvailable(Season currentSeason)
    {
        foreach (var season in seasons)
        {
            if(season == currentSeason)
            {
                return true;
            }
        }
        return false;
    }
}
