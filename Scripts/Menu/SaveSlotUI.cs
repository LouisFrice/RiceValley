using LouisFrice.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
    public Text dataTime, dataScene;
    private Button currentButton;

    private DataSlot currentData;

    //拿到当前 GameObject 在「父物体的子物体列表中」的索引位置 0~2
    private int Index => transform.GetSiblingIndex();

    private void Awake()
    {
        currentButton = GetComponent<Button>();
        currentButton.onClick.AddListener(LoadGameData);
    }

    private void OnEnable()
    {
        SetupSlotUI();
    }


    /// <summary>
    /// 设置UI显示
    /// </summary>
    private void SetupSlotUI()
    {
        currentData = SaveLoadManager.Instance.dataSlots[Index];
        if (currentData != null)
        {
            dataTime.text = currentData.DataTime;
            dataScene.text = currentData.DataScene;
        }
        else
        {
            dataTime.text = "开启新的旅程";
            dataScene.text = "空存档";
        }
    }
    private void LoadGameData()
    {
        //加载游戏
        if (currentData != null)
        {
            SaveLoadManager.Instance.Load(Index);
        }
        //新游戏
        else
        {
            Debug.Log("新游戏");
            EventHandler.CallStartNewGameEvent(Index);
        }
    }
}
