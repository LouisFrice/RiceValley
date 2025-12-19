using LouisFrice.Dialogue;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//事件处理器
public static class EventHandler 
{
    //背包更新UI
    public static event Action<InventoryLocation,List<InventoryItem>> UpdateInventoryUI;
    public static void CallUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
    {
        UpdateInventoryUI?.Invoke(location, list);
    }

    //实例化物品在世界中
    public static event Action<int, Vector3> InstantiateItemInScene;
    public static void CallInstantiateItemInScene(int id,Vector3 pos)
    {
        InstantiateItemInScene?.Invoke(id, pos);
    }

    //扔物品在地图中
    public static event Action<int, Vector3, ItemType> DropItemEvent;
    public static void CallDropItemEvent(int id,Vector3 pos,ItemType itemType)
    {
        DropItemEvent?.Invoke(id, pos, itemType);
    }

    //选中物品
    public static event Action<ItemDetails, bool> ItemSelectedEvent;
    public static void CallItemSelectedEvent(ItemDetails itemDetails,bool isSelected)
    {
        ItemSelectedEvent?.Invoke(itemDetails, isSelected);
    }

    //时间：分钟，小时，天，季节
    public static event Action<int, int, int, Season> GameMinuteEvent;
    public static void CallGameMinuteEvent(int minute,int hour, int day, Season season)
    {
        GameMinuteEvent?.Invoke(minute, hour, day, season);
    }
    //日期：小时，天，月，年，季节
    public static event Action<int,int,int,int,Season> GameDateEvent;
    public static void CallGameDateEvent(int hour, int day, int month, int year, Season season)
    {
        GameDateEvent?.Invoke(hour, day, month, year, season);  
    }
    //日期：天，季节
    public static event Action<int, Season> GameDayEvent;
    public static void CallGameDayEvent(int day, Season season)
    {
        GameDayEvent?.Invoke(day, season);
    }

    //切换场景
    public static event Action<string, Vector3> TransitionEvent;
    public static void CallTransitionEvent(string sceneName, Vector3 targetposition)
    {
        TransitionEvent?.Invoke(sceneName, targetposition);
    }

    //卸载场景前
    public static event Action BeforeSceneUnloadEvent;
    public static void CallBeforeSceneUnloadEvent()
    {
        BeforeSceneUnloadEvent?.Invoke();
    }
    //卸载场景后
    public static event Action AfterSceneLoadedEvent;
    public static void CallAfterSceneLoadedEvent()
    {
        AfterSceneLoadedEvent?.Invoke();
    }

    //移动人物坐标
    public static event Action<Vector3> MoveToPosition;
    public static void CallMoveToPosition(Vector3 targetPosition)
    {
        MoveToPosition?.Invoke(targetPosition);
    }

    //鼠标按下
    public static event Action<Vector3,ItemDetails> MouseClickedEvent;
    public static void CallMouseClickedEvent(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        MouseClickedEvent?.Invoke(mouseWorldPos, itemDetails);
    }

    //在动画播放后执行动作
    public static event Action<Vector3, ItemDetails> ExecuteActionAfterAnimation;
    public static void CallExecuteActionAfterAnimation(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        ExecuteActionAfterAnimation?.Invoke(mouseWorldPos, itemDetails);
    }

    //种植种子
    public static event Action<int, TileDetails> PlantSeedEvent;
    public static void CallPlantSeedEvent(int id , TileDetails tileDetails)
    {
        PlantSeedEvent?.Invoke(id , tileDetails);
    }

    //收获到玩家手里
    public static event Action<int> HarvestAtPlayerPosition;
    public static void CallHarvestAtPlayerPosition(int id)
    {
        HarvestAtPlayerPosition?.Invoke(id);
    }

    //刷新当前地图
    public static event Action RefreshCurrentMap;
    public static void CallRefreshCurrentMap()
    {
        RefreshCurrentMap?.Invoke();
    }

    //粒子特效
    public static event Action<ParticleEffectType, Vector3> ParticleEffectEvent;
    public static void CallParticleEffectEvent(ParticleEffectType effectType, Vector3 pos)
    {
        ParticleEffectEvent?.Invoke(effectType, pos);
    }

    //生成种子
    public static event Action GenerateCropEvent;
    public static void CallGenerateCropEvent()
    {
        GenerateCropEvent?.Invoke();
    }

    //显示对话UI
    public static event Action<DialoguePiece> ShowDialogueEvent;
    public static void CallShowDialogueEvent(DialoguePiece piece)
    {
        ShowDialogueEvent?.Invoke(piece);
    }

    //开启背包(通用)
    public static event Action<SlotType, InventoryBag_SO> BaseBagOpenEvent;
    public static void CallBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bag_SO)
    {
        BaseBagOpenEvent?.Invoke(slotType, bag_SO);
    }

    //关闭背包
    public static event Action<SlotType, InventoryBag_SO> BaseBagCloseEvent;
    public static void CallBaseBagCloseEvent(SlotType slotType, InventoryBag_SO bag_SO)
    {
        BaseBagCloseEvent?.Invoke(slotType, bag_SO);
    }

    //游戏运行状态
    public static event Action<GameState> UpdateGameStateEvent;
    public static void CallUpdateGameStateEvent(GameState gameState)
    {
        UpdateGameStateEvent?.Invoke(gameState);
    }

    //显示交易UI，卖是True，买是false
    public static event Action<ItemDetails, bool> ShowTradeUI;
    public static void CallShowTradeUI(ItemDetails itemDetails, bool isSell)
    {
        ShowTradeUI?.Invoke(itemDetails, isSell);
    }

    //建造家具
    public static event Action<int,Vector3> BuildFurnitureEvent;
    public static void CallBuildFurnitureEvent(int id,Vector3 MousePos)
    {
        BuildFurnitureEvent?.Invoke(id, MousePos);
    }

    //早上晚上切换灯光
    public static event Action<Season, LightShift, float> LightShiftChangeEvent;
    public static void CallLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifferent)
    {
        LightShiftChangeEvent?.Invoke(season, lightShift, timeDifferent);
    }

    //初始化音效
    public static event Action<SoundDetails> InitSoundEffect;
    public static void CallInitSoundEffect(SoundDetails soundDetails)
    {
        InitSoundEffect?.Invoke(soundDetails);
    }

    //播放音效
    public static event Action<SoundName> PlaySoundEvent;
    public static void CallPlaySoundEvent(SoundName soundName)
    {
        PlaySoundEvent?.Invoke(soundName);
    }

    //开启新游戏
    public static event Action<int> StartNewGameEvent;
    public static void CallStartNewGameEvent(int index)
    {
        StartNewGameEvent?.Invoke(index);
    }

    //结束游戏
    public static event Action EndGameEvent;
    public static void CallEndGameEvent()
    {
        EndGameEvent?.Invoke();
    }
}
