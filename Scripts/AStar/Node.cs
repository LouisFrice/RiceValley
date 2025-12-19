using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LouisFrice.AStar
{
    public class Node : IComparable<Node>  //IComparable接口可以实现排序
    {
        public Vector2Int gridPosition; //当前格子的网格坐标
        public int gCost = 0; //距离起点格子的距离
        public int hCost = 0; //距离目标格子的距离
        public int FCost => gCost + hCost;  //当前格子的值
        public bool isObstacle = false; //当前格子是否是障碍
        public Node parentNode;  //父节点
        public Node (Vector2Int pos)
        {
            gridPosition = pos;
            parentNode = null;
        }

        public int CompareTo(Node other)
        {
            //比较其他的FCost，返回-1，0，1
            //正数代表比other大，0代表当前对象 == 传入的比较对象，负数代表当前对象 < 传入的比较对象
            int result = FCost.CompareTo(other.FCost);
            //如果FCost相等，比较hCost
            if (result == 0)
            {
                result = hCost.CompareTo(other.hCost);
            }
            return result;
        }
    }
}
