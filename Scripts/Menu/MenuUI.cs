using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public GameObject[] panels;
    // 状态标记：是否已点击过一次退出按钮（进入确认状态）
    private bool isExitConfirming = false;
    private int exitPanelIndex = 2;

    public void SwitchPanel(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if(i == index)
            {
                //设置为最后一个，显示在最上方，覆盖其他的Panel
                panels[i].transform.SetAsLastSibling();
            }
        }

        //不是主面板就切回false，必须连按两次退出才可以退出
        if (index != exitPanelIndex)
        {
            isExitConfirming = false;
        }
    }

    public void MenuPanelExitGame()
    {
        if (!isExitConfirming)
        {
            isExitConfirming = true;
        }
        else
        {
            Debug.Log("退出游戏");
            Application.Quit();
        }
    }
    public void ExitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }
}
