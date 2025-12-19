using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace LouisFrice.Dialogue
{
    [System.Serializable]
    public class DialoguePiece
    {
        [Header("对话详情")]
        public Sprite faceImage;
        public bool onLeft;
        public string name;
        [TextArea]
        public string dialogueText;
        public bool hasToPause;
        [HideInInspector]
        //对话是否结束
        public bool isDone;
        //每次对话结束后可以单独添加事件
        //public UnityEvent afterTalkEvent;
    }
}