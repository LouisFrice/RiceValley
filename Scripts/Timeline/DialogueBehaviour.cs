using LouisFrice.Dialogue;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class DialogueBehaviour : PlayableBehaviour
{
    private PlayableDirector director;
    public DialoguePiece dialoguePiece;


    public override void OnPlayableCreate(Playable playable)
    {
        director = playable.GetGraph().GetResolver() as PlayableDirector;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        EventHandler.CallShowDialogueEvent(dialoguePiece);
        if (Application.isPlaying)
        {
            if (dialoguePiece.hasToPause)
            {
                //暂停timeline
                TimelineManager.Instance.PauseTimeline(director);
            }
            else
            {
                //直接关闭当前对话窗口
                EventHandler.CallShowDialogueEvent(null);
            }
        }
    }

    //Timeline每帧执行
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (Application.isPlaying)
        {
            TimelineManager.Instance.IsDone = dialoguePiece.isDone;
        }
    }
    //Timeline当前 Behaviour 对应的时间段暂停 / 结束时
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        //直接关闭当前对话窗口
        EventHandler.CallShowDialogueEvent(null);
    }
    //Timeline全局开始
    public override void OnGraphStart(Playable playable)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
    }
    //Timeline全局结束
    public override void OnGraphStop(Playable playable)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Play);
        director.gameObject.SetActive(false);
    }
}
