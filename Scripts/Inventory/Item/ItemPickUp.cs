using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LouisFrice.Inventory
{
    public class ItemPickUp : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            Item item = collision.GetComponent<Item>();
            if (item != null)
            {
                if (item.itemDetails.canPickUp)
                {
                    //拾取物品到背包
                    InventoryManager.Instance.AddItem(item,true);

                    EventHandler.CallPlaySoundEvent(SoundName.Pickup);
                }
            }
        }
    }
}