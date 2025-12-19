using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LouisFrice.CropPlant
{
    public class ReapItem : MonoBehaviour
    {
        private CropDetails cropDetails;
        private Transform playerTransform => FindObjectOfType<PlayerController>().transform;

        public void InitCropData(int id)
        {
            cropDetails = CropManager.Instance.GetCropDetails(id);
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
                        Vector3 spawnPos = new Vector3(transform.position.x + Random.Range(dirX, dirX * cropDetails.spawnRadius.x),
                                                       transform.position.y + Random.Range(-cropDetails.spawnRadius.y, cropDetails.spawnRadius.y), 0);

                        EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i], spawnPos);
                    }
                }
            }
        }
    }
}