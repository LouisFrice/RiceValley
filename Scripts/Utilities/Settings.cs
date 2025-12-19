using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings 
{
    //遮挡透明度
    public const float itemFadeDuration = 0.35f;  //透明度变化速率
    public const float targetAlpha = 0.45f;  //透明度目标值
    //时间
    public const float secondThreshold = 0.02f;  //数值越小时间越快
    public const int secondHold = 59;
    public const int minuteHold = 59;
    public const int hourHold = 23;
    public const int dayHold = 10;
    public const int monthHold = 12;
    public const int seasonHold = 3;
    //场景切换透明度
    public const float transitionFadeDuration = 0.8f;
    //收获后举起物品的时间
    public const float showItemTime = 0.5f;
    //一次割草的数量
    public const int reapAmount = 3;
    //NPC网格移动
    public const float gridCellSize = 1;
    //网格对角线尺寸
    public const float gridCellDiagonalSize = 1.41f;
    //像素大小 1/20
    public const float pixelSize = 0.05f;
    //NPC待机动画间隔
    public const float animationBreakTime = 5f;
    //最大网格尺寸 9999×9999，超过该范围忽视
    public const int maxGridSize = 9999;
    //早晚时间
    public static TimeSpan morningTime = new TimeSpan(5, 30, 0);
    public static TimeSpan nightTime = new TimeSpan(18, 0, 0); 
    //灯光切换时长，现实25秒
    public const float lightChangeDuation = 25f;
    //音乐过渡时间
    public const float musicTransitionSecond = 8f;
    //环境音效过渡时间
    public const float ambientTransitionSecond = 1f;
    //玩家初始坐标
    public static Vector3 playerStartPos = new Vector3(-13.5f, -2.5f, 0);
    //玩家初始金币
    public const int playerStartMoney = 1000;
}
