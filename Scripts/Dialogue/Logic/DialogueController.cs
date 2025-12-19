using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LouisFrice.Dialogue
{
    [RequireComponent(typeof(NPCMovement))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueController : MonoBehaviour
    {
        private NPCMovement npc => GetComponent<NPCMovement>();
        public UnityEvent OnFinishEvent;
        public List<DialoguePiece> dialogueList = new List<DialoguePiece>();
        //对话堆，方便按顺序读取对话切片
        private Stack<DialoguePiece> dialogueStack;

        private bool canTalk;
        private bool isTalking;
        private GameObject uiSign;

        private void Awake()
        {
            uiSign = transform.GetChild(1).gameObject;
            FillDialogueStack();
        }
        private void Update()
        {
            uiSign.SetActive(canTalk);
            //按下空格对话
            if(canTalk && !isTalking && Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(DialogueRoutine());
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                //NPC没在移动 且 计划内可以交互
                if(!npc.isMoving && npc.interactable)
                {
                    //就可以和NPC对话互动
                    canTalk = true;
                }
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                canTalk = false;
            }
        }
        /// <summary>
        /// 对话协程，只要对话堆里有对话就会开始对话
        /// </summary>
        /// <returns></returns>
        private IEnumerator DialogueRoutine()
        {
            isTalking = true;
            //有对话切片
            if(dialogueStack.TryPop(out DialoguePiece result))
            {
                //显示对话UI
                EventHandler.CallShowDialogueEvent(result);
                //玩家禁止移动
                EventHandler.CallUpdateGameStateEvent(GameState.Pause);
                //等待对话结束
                yield return new WaitUntil(() => result.isDone == true);
                isTalking = false;
            }
            //对话结束 或者 没有对话切片
            else
            {
                //传空代表不显示UI
                EventHandler.CallShowDialogueEvent(null);
                //允许玩家移动
                EventHandler.CallUpdateGameStateEvent(GameState.Play);
                //重新填充对话内容，可以一直对话
                FillDialogueStack();
                isTalking = false;
                //调用对话结束事件
                if(OnFinishEvent  != null)
                {
                    OnFinishEvent.Invoke();
                    //不允许再次对话
                    canTalk = false;
                }
            }
        }
        /// <summary>
        /// 填充对话堆，倒序填进去
        /// </summary>
        private void FillDialogueStack()
        {
            dialogueStack = new Stack<DialoguePiece>();
            for (int i = dialogueList.Count - 1; i > -1; i--)
            {
                dialogueList[i].isDone = false;
                dialogueStack.Push(dialogueList[i]);
            }
        }
    }

    
}