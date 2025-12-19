using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : MonoBehaviour
{
    public CropDetails cropDetails;
    public TileDetails tileDetails;
    //收割的点击次数
    private int harvestActionCount;
    //作物已经成熟可以收获
    public bool CanHarvest => tileDetails.growthDays >= cropDetails.TotalGrowthDays;

    private Animator animator;
    private Transform playerTransform => FindObjectOfType<PlayerController>().transform;

    public void ProcessToolAction(ItemDetails tool, TileDetails tile)
    {
        tileDetails = tile;

        //工具的使用次数
        int requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);
        //无法使用该工具
        if (requireActionCount == -1) { return; }

        animator = GetComponentInChildren<Animator>();

        //计数器
        if(requireActionCount > harvestActionCount)
        {
            harvestActionCount++;

            if (animator != null && cropDetails.hasAnimation)
            {
                if (playerTransform.position.x < this.transform.position.x)
                {
                    animator.SetTrigger("RotateRight");
                }
                else
                {
                    animator.SetTrigger("RotateLeft");
                }
            }
            //播放粒子特效
            if (cropDetails.hasParticalEffect)
            {
                EventHandler.CallParticleEffectEvent(cropDetails.effectType, cropDetails.effectPos + transform.position);
            }
            //播放声音
            if(cropDetails.soundEffect != SoundName.None)
            {
                EventHandler.CallPlaySoundEvent(cropDetails.soundEffect);
            }
        }

        if (requireActionCount <= harvestActionCount)
        {
            //可以生成物品到身体上 或 没有收获动画
            if (cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation)
            {
                //生成收获物品
                SpawnHarvestItems();
            }
            //是否有收获动画，树倒下
            else if (cropDetails.hasAnimation)
            {
                if (playerTransform.position.x < this.transform.position.x)
                {
                    animator.SetTrigger("FallingRight");
                }
                else
                {
                    animator.SetTrigger("FallingLeft");
                }
                EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
                StartCoroutine(HarvestAfterAnimation());
            }
        }
    }
    /// <summary>
    /// 动画结束再生成收获物品
    /// </summary>
    /// <returns></returns>
    private IEnumerator HarvestAfterAnimation()
    {
        //如果播放的不是结束动画就跳过
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("END"))
        {
            yield return null;
        }

        SpawnHarvestItems();

        if (cropDetails.transferItemID > 0)
        {
            //收割后转换成新物体
            CreateTransferCrop();
        }
    }

    /// <summary>
    /// 收割后转换新物体
    /// </summary>
    private void CreateTransferCrop()
    {
        tileDetails.seedItemID = cropDetails.transferItemID;
        tileDetails.daysSinceLastHarvest = -1;
        tileDetails.growthDays = 0;

        EventHandler.CallRefreshCurrentMap();
    }

    /// <summary>
    /// 生成收获物品
    /// </summary>
    public void SpawnHarvestItems()
    {
        for (int i = 0; i < cropDetails.producedItemID.Length; i++)
        {
            int amountToProduce;
            //果实如果是固定数量
            if (cropDetails.producedMaxAmount[i] == cropDetails.producedMinAmount[i])
            {
                amountToProduce = cropDetails.producedMaxAmount[i];
            }
            //随机数量
            else
            {
                //Random左包含 右不包含
                amountToProduce = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i] + 1);
            }

            //生成指定数量的物品
            for (int j = 0; j < amountToProduce; j++)
            {
                //在人物身上生成物品
                if (cropDetails.generateAtPlayerPosition)
                {
                    EventHandler.CallHarvestAtPlayerPosition(cropDetails.producedItemID[i]);
                }
                else //在地图生成物品
                {
                    //玩家在作物左边砍，物品生成在右边
                    //大于是作物在玩家的右边，1是生成在右边，-1是生成在左边
                    int dirX = this.transform.position.x > playerTransform.position.x ? 1 : -1;
                    //作物位置随机生成，作物位置X + dirX ~ spawnRadius.x ， Y是 -spawnRadius.y ~ spawnRadius.y
                    Vector3 spawnPos = new Vector3(transform.position.x + Random.Range(dirX,dirX * cropDetails.spawnRadius.x),
                                                   transform.position.y + Random.Range(-cropDetails.spawnRadius.y,cropDetails.spawnRadius.y),0);

                    EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i], spawnPos);
                } 
            }
        }

        if (tileDetails != null)
        {
            tileDetails.daysSinceLastHarvest++;

            //是否可以重复生长
            if (cropDetails.daysToRegrow > 0 && tileDetails.daysSinceLastHarvest < cropDetails.regrowTimes)
            {
                //作物收获后退回到某一个阶段（某一天）
                tileDetails.growthDays = cropDetails.TotalGrowthDays - cropDetails.daysToRegrow;
                //刷新种子
                EventHandler.CallRefreshCurrentMap();
            }
            //不可重复生长
            else
            {
                tileDetails.daysSinceLastHarvest = -1;
                tileDetails.seedItemID = -1;
            }

            Destroy(this.gameObject);
        }

    }
}
