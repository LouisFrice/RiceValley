using LouisFrice.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorOverride : MonoBehaviour
{
    private Animator[] animators;
    public SpriteRenderer holdItem;

    [Header("各部分动画列表")]
    public List<AnimatorType> animatorTypes; 
    //记录身体部位Animator的字典
    private Dictionary<string,Animator> animatorNameDic = new Dictionary<string,Animator>();
    private void Awake()
    {
        //拿到每个身体部位(Body,Arm)的Animator
        animators = GetComponentsInChildren<Animator>();
        foreach (var animator in animators)
        {
            //'Body'-Body的animator、'Arm'-Arm的animator、'Hair'-Hair的animator
            animatorNameDic.Add(animator.name, animator);
        }
    }

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
    }
    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
    }

    private void OnHarvestAtPlayerPosition(int id)
    {
        Sprite itemSprite = InventoryManager.Instance.GetItemDetails(id).itemOnWorldSprite;
        if (itemSprite == null)
        {
            itemSprite = InventoryManager.Instance.GetItemDetails(id).itemIcon;
        }
        if (holdItem.enabled == false)
        {
            StartCoroutine(ShowItem(itemSprite));
        }
    }

    private IEnumerator ShowItem(Sprite itemSprite)
    {
        holdItem.sprite = itemSprite;
        holdItem.enabled = true;
        //举起固定时间后消失
        yield return new WaitForSeconds(Settings.showItemTime);
        holdItem.enabled = false;
    }

    private void OnBeforeSceneUnloadEvent()
    {
        //加载场景取消举起动画
        holdItem.enabled = false;
        SwitchAnimator(PartType.None);
    }

    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        //WORKFLOW:不同的工具返回不同的动画要在这里补全
        PartType currentType = itemDetails.itemType switch
        {
            ItemType.Seed => PartType.Carry,
            ItemType.Commodity => PartType.Carry,
            ItemType.HoeTool => PartType.Hoe,
            ItemType.WaterTool => PartType.Water,
            ItemType.CollectTool => PartType.Collect,
            ItemType.ChopTool => PartType.Chop,
            ItemType.BreakTool => PartType.Break,
            ItemType.ReapTool => PartType.Reap,
            ItemType.Furniture => PartType.Carry,
            _ => PartType.None
        };
        //如果没选择就切回默认放下状态
        if(isSelected == false)
        {
            currentType = PartType.None;
            holdItem.enabled = false;
        }
        else
        {
            if (currentType == PartType.Carry)
            {
                holdItem.sprite = itemDetails.itemOnWorldSprite == null ? itemDetails.itemIcon : itemDetails.itemOnWorldSprite;
                holdItem.enabled = true;
            }
            else
            {
                holdItem.enabled = false;
            }
        }
        SwitchAnimator(currentType);
    }
    /// <summary>
    /// 切换动作模组
    /// </summary>
    /// <param name="partType">动作的类型</param>
    private void SwitchAnimator(PartType partType)
    {
        //先把模组全部变成默认模组
        foreach (var actionType in animatorTypes)
        {
            //Carry、None
            //如果是当前动作的类型（Carry-举起物品动作）(Hoe-锄地动作)（Water-浇水动作）
            if (actionType.partType == PartType.None)
            {
                //animatorNameDic[Arm].现在的动画 = Arm_Hold
                animatorNameDic[actionType.partName.ToString()].runtimeAnimatorController = actionType.overrideController;
            }
        }

        //再根据对应部位修改动作模组
        foreach (var actionType in animatorTypes)
        {
            //Carry、None
            //如果是当前动作的类型（Carry-举起物品动作）(Hoe-锄地动作)（Water-浇水动作）
            if (actionType.partType == partType)
            {
                //animatorNameDic[Arm].现在的动画 = Arm_Hold
                animatorNameDic[actionType.partName.ToString()].runtimeAnimatorController = actionType.overrideController;
            }
        }
    }
}
