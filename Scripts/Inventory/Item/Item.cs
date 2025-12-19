using LouisFrice.CropPlant;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LouisFrice.Inventory
{
    public class Item : MonoBehaviour
    {
        public int itemID;
        private SpriteRenderer spriteRenderer;
        public ItemDetails itemDetails;
        private BoxCollider2D boxCollider;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();
        }
        private void Start()
        {
            if (itemID != 0)
            {
                Init(itemID);
            }
        }

        public void Init(int id)
        {
            itemID = id;
            itemDetails = InventoryManager.Instance.GetItemDetails(itemID);
            if (itemDetails != null)
            {
                spriteRenderer.sprite = itemDetails.itemOnWorldSprite != null ? itemDetails.itemOnWorldSprite : itemDetails.itemIcon;
                //修改碰撞体尺寸，根据物体实时修改,size是pix/pix per unit，此时的size就是1
                Vector2 newSize = new Vector2(spriteRenderer.sprite.bounds.size.x,spriteRenderer.sprite.bounds.size.y);
                boxCollider.size = newSize;
                //bounds.center.y 本质是精灵自身几何中心相对于其锚点的偏移量
                //如果锚点是bottom在锚点上方 0.5 单位 → bounds.center.y = 0.5
                //如果锚点是center精灵中心与锚点重合 → bounds.center.y = 0
                boxCollider.offset = new Vector2(0, spriteRenderer.sprite.bounds.center.y);
            }

            if(itemDetails.itemType == ItemType.ReapableScenery)
            {
                gameObject.AddComponent<ReapItem>().InitCropData(itemDetails.itemID);
                gameObject.AddComponent<ItemInteractive>();
            }
        }
    }


}