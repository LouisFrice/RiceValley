using LouisFrice.Save;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>, ISaveable
{
    //游戏时间
    private int gameSecond, gameMinute,gameHour,gameDay,gameMonth,gameYear;
    private Season gameSeason = Season.春天;
    private int monthInSeason = 3;

    public bool gameClockPause;
    private float tikTime;

    //灯光时间差
    private float timeDifference;

    public TimeSpan GameTime => new TimeSpan(gameHour, gameMinute, gameSecond);

    public Season GameSeaon => gameSeason;

    public string GUID => GetComponent<DataGUID>().GUID;

    private void Start()
    {
        ////事件的注册在OnEnable，Awake在OnEnable之前，所以放在Start
        //EventHandler.CallGameMinuteEvent(gameMinute, gameHour, gameDay,gameSeason);
        //EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
        ////切换灯光
        //EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);


        gameClockPause = true;
		//注册当前实体去存档
		ISaveable saveable = this;
        saveable.RegisterSaveable();
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
		EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;

    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
		EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void OnEndGameEvent()
    {
        gameClockPause = true;
    }

    private void OnStartNewGameEvent(int index)
    {
        NewGameTime();
		//gameClockPause = false;
	}

    private void OnUpdateGameStateEvent(GameState gameState)
    {
        //暂停游戏时间
        gameClockPause = gameState == GameState.Pause;
    }

    private void OnBeforeSceneUnloadEvent()
    {
        gameClockPause = true;
    }

    private void OnAfterSceneLoadedEvent()
    {
        gameClockPause = false;

        //加载存档，刷新时间和灯光
        EventHandler.CallGameMinuteEvent(gameMinute, gameHour, gameDay, gameSeason);
        EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
        EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);
    }

    private void FixedUpdate()
    {
        if(!gameClockPause)
        {
            tikTime += Time.deltaTime;
            if(tikTime >= Settings.secondThreshold)
            { 
                tikTime -= Settings.secondThreshold;
                UpdateGameTime();
            }
        }
        //按＋号加速时间
        if (Input.GetKey(KeyCode.Equals))
        {
            for (int i = 0; i < 60; i++)
            {
                UpdateGameTime();
            }
        }
        //按-号加速更多时间
        if (Input.GetKey(KeyCode.Minus))
        {
            for (int i = 0; i < (60 * 60); i++)
            {
                UpdateGameTime();
            }
        }
        ////按F9跳过一天
        //if (Input.GetKeyDown(KeyCode.F9))
        //{
        //    for (int i = 0; i < (60 * 60 * 24); i++)
        //    {
        //        UpdateGameTime();
        //    }
        //}
        ////按F10跳过一个月
        //if (Input.GetKeyDown(KeyCode.F10))
        //{
        //    for (int i = 0; i < (60 * 60 * 24 * 10); i++)
        //    {
        //        UpdateGameTime();
        //    }
        //}
        ////按F11跳过一个季节
        //if (Input.GetKeyDown(KeyCode.F11))
        //{
        //    for (int i = 0; i < (60 * 60 * 24 * 30); i++)
        //    {
        //        UpdateGameTime();
        //    }
        //}
    }
    private void NewGameTime()
    {
        gameSecond = 0;
        gameMinute = 0;
        gameHour = 6;
        gameDay = 1;
        gameMonth = 1;
        gameYear = 2025;
        gameSeason = Season.春天;
    }

    /// <summary>
    /// 更新游戏时间
    /// </summary>
    private void UpdateGameTime()
    {
        gameSecond++;
        if(gameSecond > Settings.secondHold)
        {
            gameSecond = 0;
            gameMinute++;
            if( gameMinute > Settings.minuteHold)
            {
                gameMinute = 0;
                gameHour++;
                if(gameHour > Settings.hourHold)
                {
                    gameHour = 0;
                    gameDay++;
                    if(gameDay > Settings.dayHold)
                    {
                        gameDay = 1;
                        gameMonth++;
                        if(gameMonth > Settings.monthHold)
                        {
                            gameMonth = 1;
                        }

                        //季节变更
                        monthInSeason--;
                        if(monthInSeason == 0)
                        {
                            monthInSeason = 3;

                            //季节Enum转Int
                            int seasonNumber = (int)gameSeason;
                            seasonNumber++;

                            if(seasonNumber > Settings.seasonHold)
                            {
                                seasonNumber = 0;
                                gameYear++;
                            }

                            //Int转季节Enum
                            gameSeason = (Season)seasonNumber;

                            if(gameYear > 9999) { gameYear = 2025; }
                        }
                    }
                    //每天调用
                    //通知锄地浇水和农作物生长的地图更新
                    EventHandler.CallGameDayEvent(gameDay, gameSeason);
                }
                //每小时调用
                EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
            }
            //每秒调用
            EventHandler.CallGameMinuteEvent(gameMinute, gameHour,gameDay,gameSeason);
            //切换灯光
            EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);
        }
        //Debug.Log(gameHour + ":" + gameMinute + ":" + gameSecond);
    }

    /// <summary>
    /// 拿到当前时辰并计算时间差
    /// </summary>
    /// <returns></returns>
    private LightShift GetCurrentLightShift()
    {
        //（早上时间）早上~晚上
        if(GameTime >= Settings.morningTime &&  GameTime < Settings.nightTime)
        {
            timeDifference = (float)(GameTime - Settings.morningTime).TotalMinutes;
            //Debug.Log("早上-" + timeDifference);
            return LightShift.Morning;
        }
        //（晚上时间）早上~晚上
        if (GameTime >= Settings.nightTime || GameTime < Settings.morningTime)
        {
            timeDifference = Mathf.Abs((float)(GameTime - Settings.nightTime).TotalMinutes);
            //Debug.Log("晚上-" + timeDifference);
            return LightShift.Night;
        }
        //默认早上
        return LightShift.Morning;
    }

    public GameSaveData GenerateSavaData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.timeDic = new Dictionary<string, int>();
        //gameSecond, gameMinute,gameHour,gameDay,gameMonth,gameYear;
        saveData.timeDic.Add("gameSecond", gameSecond);
        saveData.timeDic.Add("gameMinute", gameMinute);
        saveData.timeDic.Add("gameHour", gameHour);
        saveData.timeDic.Add("gameDay", gameDay);
        saveData.timeDic.Add("gameMonth", gameMonth);
        saveData.timeDic.Add("gameYear", gameYear);
        saveData.timeDic.Add("gameSeason", (int)gameSeason);

        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        gameSecond = saveData.timeDic["gameSecond"];
        gameMinute = saveData.timeDic["gameMinute"];
        gameHour = saveData.timeDic["gameHour"];
        gameDay = saveData.timeDic["gameDay"];
        gameMonth = saveData.timeDic["gameMonth"];
        gameYear = saveData.timeDic["gameYear"];
        gameSeason = (Season)(saveData.timeDic["gameSeason"]);
    }
}
