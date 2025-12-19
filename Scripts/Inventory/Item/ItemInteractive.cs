using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteractive : MonoBehaviour
{
    private bool isAnimating;
    //左右摇晃的时间
    private WaitForSeconds pause = new WaitForSeconds(0.04f);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //防止协程排队延迟触发导致报错
        if (!gameObject.activeInHierarchy || !enabled) return;

        if (!isAnimating)
        {
            if (collision.transform.position.x < this.transform.position.x)
            {
                //对方在左侧 向右摇晃
                StartCoroutine(RotateRight());
            }
            else
            {
                //对方在右侧 向左摇晃
                StartCoroutine(RotateLeft());
            }
            EventHandler.CallPlaySoundEvent(SoundName.Rustle);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        //防止协程排队延迟触发导致报错
        if (!gameObject.activeInHierarchy || !enabled) return;

        if (!isAnimating)
        {
            if (collision.transform.position.x > this.transform.position.x)
            {
                //对方在左侧 向右摇晃
                StartCoroutine(RotateRight());
            }
            else
            {
                //对方在右侧 向左摇晃
                StartCoroutine(RotateLeft());
            }
            EventHandler.CallPlaySoundEvent(SoundName.Rustle);
        }
    }

    /// <summary>
    /// 植物向左摇晃
    /// </summary>
    /// <returns></returns>
    private IEnumerator RotateLeft()
    {
        isAnimating = true;
        //向左旋转4下，向右旋转5下，再回正，有果冻效果
        for (int i = 0; i < 4; i++)
        {
            this.transform.GetChild(0).Rotate(0, 0, 2);
            yield return pause;
        }
        for (int i = 0; i < 5; i++)
        {
            this.transform.GetChild(0).Rotate(0, 0, -2);
            yield return pause;
        }
        this.transform.GetChild(0).Rotate(0, 0, 2);
        yield return pause;

        isAnimating = false;
    }
    /// <summary>
    /// 植物向右摇晃
    /// </summary>
    /// <returns></returns>
    private IEnumerator RotateRight()
    {
        isAnimating = true;
        //向左旋转4下，向右旋转5下，再回正，有果冻效果
        for (int i = 0; i < 4; i++)
        {
            this.transform.GetChild(0).Rotate(0, 0, -2);
            yield return pause;
        }
        for (int i = 0; i < 5; i++)
        {
            this.transform.GetChild(0).Rotate(0, 0, 2);
            yield return pause;
        }
        this.transform.GetChild(0).Rotate(0, 0, -2);
        yield return pause;

        isAnimating = false;
    }
}
