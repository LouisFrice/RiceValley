using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineManager : Singleton<TimelineManager>
{
    public PlayableDirector startDirector;
    private PlayableDirector currentDirector;
    private bool isPause;
    private bool isDone;
    private bool isNewGame;
    private bool canSkip;
    public bool IsDone { set => isDone = value; }
    //属性是用的时候才会执行里面代码，不会启动游戏立即执行
    public float TimelineDuration { get => startDirector != null ? (float)startDirector.duration : 0f; }

    protected override void Awake()
    {
        base.Awake();
        currentDirector = startDirector;
    }
    private void Update()
    {
        if (canSkip && Input.GetKeyDown(KeyCode.Escape))
        {
            currentDirector.time = currentDirector.duration;

            canSkip = false;
        }
        if (isPause && Input.GetKeyDown(KeyCode.Space) && isDone)
        {
            isPause = false;
            //恢复速度
            currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1d);
        }
    }
    private void OnEnable()
    {
        //currentDirector.played += TimelinePlayed;
        //currentDirector.stopped += TimelineStopped;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
        //新游戏播放Timeline
        isNewGame = true;
        canSkip = true;
    }

    private void OnAfterSceneLoadedEvent()
    {
        if (isNewGame)
        {
            currentDirector = FindObjectOfType<PlayableDirector>();
            if (currentDirector != null)
            {
                currentDirector.Play();
            }
            isNewGame = false;
        }
    }
    public void PauseTimeline(PlayableDirector director)
    {
        currentDirector = director;
        //设置速度0暂停Timeline
        currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0d);
        isPause = true;
    }
    //private void TimelineStopped(PlayableDirector director)
    //{
    //    if (director != null)
    //    {
    //        EventHandler.CallUpdateGameStateEvent(GameState.Play);
    //        director.gameObject.SetActive(false);
    //    }
    //}

    //private void TimelinePlayed(PlayableDirector director)
    //{
    //    if(director != null)
    //    {
    //        //播放Timeline的时候暂停游戏
    //        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
    //    }
    //}


}
