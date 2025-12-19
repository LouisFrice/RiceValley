using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightPatternList_SO", menuName = "Light/Light Pattern")]
public class LightPatternList_SO : ScriptableObject
{
    public List<LightDetails> lightPatternList;

    /// <summary>
    /// 根据季节和白天夜晚返回灯光详情
    /// </summary>
    /// <param name="season"></param>
    /// <param name="lightShift"></param>
    /// <returns></returns>
    public LightDetails GetLightDetails(Season season, LightShift lightShift)
    {
        return lightPatternList.Find((light) => { return light.season == season && light.lightShift == lightShift; });
    }
}
