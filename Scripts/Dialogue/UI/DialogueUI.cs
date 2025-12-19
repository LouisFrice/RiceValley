using DG.Tweening;
using LouisFrice.Dialogue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public GameObject dialoguePanel;
    public GameObject continueBox;
    public Text dialogueText;
    public Image faceRight, faceLeft;
    public Text nameRight, nameLeft;

    private void Awake()
    {
        continueBox.SetActive(false);
    }
    private void OnEnable()
    {
        EventHandler.ShowDialogueEvent += OnShowDialogueEvent;
    }
    private void OnDisable()
    {
        EventHandler.ShowDialogueEvent -= OnShowDialogueEvent;
    }

    private void OnShowDialogueEvent(DialoguePiece piece)
    {
        StartCoroutine(ShowDialogue(piece));
    }
    /// <summary>
    /// 显示对话框
    /// </summary>
    /// <param name="piece">对话切片</param>
    /// <returns></returns>
    private IEnumerator ShowDialogue(DialoguePiece piece)
    {
        if (piece != null)
        {
            piece.isDone = false;
            dialoguePanel.SetActive(true);
            continueBox.SetActive(false);
            dialogueText.text = string.Empty;

            if (piece.name != string.Empty)
            {
                //头像在左侧
                if (piece.onLeft)
                {
                    faceRight.gameObject.SetActive(false);
                    faceLeft.gameObject.SetActive(true);
                    faceLeft.sprite = piece.faceImage;
                    nameLeft.text = piece.name;
                }
                //头像在右侧
                else
                {
                    faceRight.gameObject.SetActive(true);
                    faceLeft.gameObject.SetActive(false);
                    faceRight.sprite = piece.faceImage;
                    nameRight.text = piece.name;
                }
            }
            //漏填NPC名字
            else
            {
                Debug.Log("对话漏填NPC名字");
                faceRight.gameObject.SetActive(false);
                faceLeft.gameObject.SetActive(false);
                nameLeft.gameObject.SetActive(false);
                nameRight.gameObject.SetActive(false);
            }
            //DoText实现打字效果，逐字显示，持续1秒，结束后才允许显示下一段对话
            yield return dialogueText.DOText(piece.dialogueText, 1f).WaitForCompletion();

            piece.isDone = true;

            //对话结束，显示空格进行下一步
            if(piece.isDone && piece.hasToPause)
            {
                continueBox.SetActive(true);
            }
        }
        else
        {
            dialoguePanel.SetActive(false);
            //终止协程,不会执行后面逻辑
            yield break;
        }
    }
}
