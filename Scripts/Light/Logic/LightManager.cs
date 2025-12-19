using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    private LightController[] sceneLights;
    private LightShift currentLightShift;
    private Season currentSeason;
    private float timeDifferent = Settings.lightChangeDuation;


    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.LightShiftChangeEvent += OnLightShiftChangeEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.LightShiftChangeEvent -= OnLightShiftChangeEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int index)
    {
        currentLightShift = LightShift.Morning;
    }

    private void OnLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifferent)
    {
        this.currentSeason = season;
        this.timeDifferent = timeDifferent;
        //如果时辰变了
        if(currentLightShift != lightShift)
        {
            this.currentLightShift = lightShift;
            //防止报错
            if (sceneLights != null)
            {
                foreach (LightController light in sceneLights)
                {
                    light.ChangeLightShift(currentSeason, currentLightShift, timeDifferent);
                }
            }
        }
    }

    private void OnAfterSceneLoadedEvent()
    {
        sceneLights = FindObjectsOfType<LightController>();
        foreach (LightController light in sceneLights)
        {
            light.ChangeLightShift(currentSeason, currentLightShift, timeDifferent);
        }

    }
}
