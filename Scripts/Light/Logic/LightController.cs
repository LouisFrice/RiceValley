using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    public LightPatternList_SO lightData;
    private Light2D currentLight;
    private LightDetails currentLightDetails;

    private void Awake()
    {
        currentLight = GetComponent<Light2D>();
    }
    
    public void ChangeLightShift(Season season, LightShift lightShift, float timeDifferent)
    {
        currentLightDetails = lightData.GetLightDetails(season, lightShift);
        //如果已经不足25秒变化时间，就补偿上颜色差值
        //Settings.lightChangeDuation是现实里的25秒
        if (timeDifferent < Settings.lightChangeDuation)
        {
            //下面这段AI说删掉手动计算，保留DOTween
            ////手动计算颜色偏移量，每秒变化量 = 颜色总差值 / Settings.lightChangeDuation，「已持续时间内的颜色偏移量」（该变多少）= 每秒变化量 * timeDifferent
            //Color colorOffset = (currentLightDetails.lightColor - currentLight.color) / Settings.lightChangeDuation * timeDifferent;
            ////加上颜色补偿
            //currentLight.color += colorOffset;
            DOTween.To(() => { return currentLight.color; }, 
                               (color) => { currentLight.color = color; },
                               currentLightDetails.lightColor,
                               Settings.lightChangeDuation - timeDifferent);
            DOTween.To(() => { return currentLight.intensity; },
                               (intensity) => { currentLight.intensity = intensity; },
                               currentLightDetails.intensity,
                               Settings.lightChangeDuation - timeDifferent);
        }
        //如果时间差大于变化时间
        if (timeDifferent >= Settings.lightChangeDuation)
        {
            if (currentLight != null)
            {
                currentLight.color = currentLightDetails.lightColor;
                currentLight.intensity = currentLightDetails.intensity;
            }
            else if(currentLight == null)
            {
                Debug.Log("currentLight报空");
            }
        }
    }
}
