using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LouisFrice.Inventory
{
    public class ItemBounce : MonoBehaviour
    {
        private Transform spriteTransform;
        private BoxCollider2D boxCollider;

        public float gravity = -3.5f;
        private bool isGround;
        private float distance;
        private Vector2 direction;
        private Vector3 targetPos;

        private void Awake()
        {
            //拿到第一个子物体sprite
            spriteTransform = transform.GetChild(0);
            boxCollider = GetComponent<BoxCollider2D>();
            //一开始关闭碰撞体
            boxCollider.enabled = false;
        }

        private void Update()
        {
            Bounce();
        }


        /// <summary>
        /// 初始化物品位置方向
        /// </summary>
        /// <param name="target">目标鼠标位置</param>
        /// <param name="dir">方向</param>
        public void InitBounceItem(Vector3 target, Vector2 dir)
        {
            boxCollider.enabled = false;
            targetPos = target;
            direction = dir;
            distance = Vector3.Distance(target, this.transform.position);

            //从人物头顶扔出去
            spriteTransform.position += Vector3.up * 1.5f;
        }
        /// <summary>
        /// 物品扔出去
        /// </summary>
        private void Bounce()
        {
            //如果阴影大于物体位置，就表示落到地上了
            isGround = spriteTransform.position.y <= this.transform.position.y;
            //判断值太小会飞出去
            if (Vector3.Distance(targetPos, this.transform.position) > 0.1f) 
            {
                //横向移动
                this.transform.position += (Vector3)direction * distance * -gravity * Time.deltaTime;
            }
            if (!isGround)
            {
                //竖向移动
                spriteTransform.position += Vector3.up * gravity * Time.deltaTime;
            }
            else
            {
                spriteTransform.position = this.transform.position;
                boxCollider.enabled = true;
            }
        }
    }
}