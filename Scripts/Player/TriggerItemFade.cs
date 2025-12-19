using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerItemFade : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ItemFader[] items = collision.GetComponentsInChildren<ItemFader>();
        if (items.Length > 0)
        {
            foreach (var item in items)
            {
                //Öð½¥Í¸Ã÷
                item.FadeOut();
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        ItemFader[] items = collision.GetComponentsInChildren<ItemFader>();
        if (items.Length > 0)
        {
            foreach (var item in items)
            {
                //Öð½¥²»Í¸Ã÷
                item.FadeIn();
            }
        }
    }
}
