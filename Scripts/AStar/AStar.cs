using LouisFrice.Map;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace LouisFrice.AStar
{
    public class AStar : Singleton<AStar>
    {
        private GridNodes gridNodes;
        private Node startNode;
        private Node targetNode;

        private int gridWidth;
        private int gridHeight;
        private int originX;
        private int originY;

        private List<Node> openNodeList;  //当前选中节点的周围8个点
        private HashSet<Node> closeNodeList;  //所有被选中的点  (HashSet是高性能、无重复的无序集合)

        private bool pathFound;

        /// <summary>
        /// 构建路径更新Stack的每一步
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <param name="startPos">开始点</param>
        /// <param name="endPos">结束点</param>
        /// <param name="npcMovementStep">NPC移动堆栈</param>
        public void BuildPath(string sceneName, Vector2Int startPos, Vector2Int endPos, Stack<MovementStep> npcMovementStep)
        {
            pathFound = false;
            if (GenerateGridNodes(sceneName, startPos, endPos))
            {
                //找到最短路径
                if (FindShortestPath())
                {
                    //构建NPC移动路径
                    UpdatePathOnMovementStepStack(sceneName, npcMovementStep);
                }
            }
        }


        /// <summary>
        /// 构建网格节点信息，初始化两个列表
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <param name="startPos">起点</param>
        /// <param name="endPos">终点</param>
        /// <returns></returns>
        private bool GenerateGridNodes(string sceneName, Vector2Int startPos, Vector2Int endPos)
        {
            if (GridManager.Instance.GetGridDimensions(sceneName, out Vector2Int gridDimension, out Vector2Int gridOrigin))
            {
                //根据瓦片地图范围构建网格移动节点范围数组
                gridNodes = new GridNodes(gridDimension.x, gridDimension.y);
                gridWidth = gridDimension.x;
                gridHeight = gridDimension.y;
                originX = gridOrigin.x;
                originY = gridOrigin.y;

                openNodeList = new List<Node>();

                closeNodeList = new HashSet<Node>();
            }
            else
            {
                return false;
            }
            //gridNode的范围是0，0开始，所以需要减去原点坐标得到实际位置
            startNode = gridNodes.GetGridNode(startPos.x - originX, startPos.y - originY);
            targetNode = gridNodes.GetGridNode(endPos.x - originX, endPos.y - originY);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3Int tilePos = new Vector3Int(x + originX, y + originY, 0);

                    string key = tilePos.x + "x" + tilePos.y + "y" + sceneName;
                    TileDetails tile = GridManager.Instance.GetTileDetails(key);

                    if (tile != null)
                    {
                        Node node = gridNodes.GetGridNode(x, y);
                        if (tile.isNPCObstacle)
                        {
                            node.isObstacle = true;
                        }
                    }

                }
            }

            return true;
        }

        /// <summary>
        /// 找到最短路径，存入CloseNodeList
        /// </summary>
        /// <returns>是否找到最短路径</returns>
        private bool FindShortestPath()
        {
            //添加起点
            openNodeList.Add(startNode);

            while (openNodeList.Count > 0)
            {
                //节点排序
                openNodeList.Sort();

                Node closeNode = openNodeList[0];
                openNodeList.RemoveAt(0);
                closeNodeList.Add(closeNode);

                if (closeNode == targetNode)
                {
                    pathFound = true;
                    break;
                }
                //计算周围8个node补充到OpenList
                EvaluateNeighbourNodes(closeNode);
            }
            return pathFound;
        }


        /// <summary>
        /// 评估周围8个点，生成对应的消耗值
        /// </summary>
        /// <param name="currentNode"></param>
        private void EvaluateNeighbourNodes(Node currentNode)
        {
            Vector2Int currentNodePos = currentNode.gridPosition;
            Node validNeighbourNode;

            //循环周围8个点
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) { continue; }
                    validNeighbourNode = GetValidNeighbourNode(currentNodePos.x + x, currentNodePos.y + y);

                    if (validNeighbourNode != null)
                    {
                        if (!openNodeList.Contains(validNeighbourNode))
                        {
                            validNeighbourNode.gCost = currentNode.gCost + GetDistance(currentNode, validNeighbourNode);
                            validNeighbourNode.hCost = GetDistance(validNeighbourNode,targetNode);
                            //链接父节点
                            validNeighbourNode.parentNode = currentNode;
                            openNodeList.Add(validNeighbourNode);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 获取有效的周围的点，排除地图外，障碍物，已选择
        /// </summary>
        /// <param name="x">网格X</param>
        /// <param name="y">网格Y</param>
        /// <returns></returns>
        private Node GetValidNeighbourNode(int x, int y)
        {
            //超出地图范围的点
            if (x >= gridWidth || y >= gridHeight || x < 0 || y < 0)
            {
                return null;
            }

            Node neighbourNode = gridNodes.GetGridNode(x, y);

            //如果是障碍物 或者是 列表里已经存在
            if(neighbourNode.isObstacle || closeNodeList.Contains(neighbourNode))
            {
                return null;
            }
            else
            {
                return neighbourNode;
            }
        }

        /// <summary>
        /// 返回两点的距离值
        /// </summary>
        /// <param name="nodeA">点A</param>
        /// <param name="nodeB">点B</param>
        /// <returns>直线10，斜线14</returns>
        private int GetDistance(Node nodeA, Node nodeB)
        {
            int xDistance = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
            int yDistance = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

            if(xDistance > yDistance)
            {
                //减的yDistance是因为斜着走的，多几个y就减几个y
                return 14 * yDistance + 10 * (xDistance - yDistance);
            }
            return 14 * xDistance + 10 * (yDistance - xDistance);
        }

        /// <summary>
        /// 更新路径每一步的坐标和场景名字
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="npcMovementStep"></param>
        private void UpdatePathOnMovementStepStack(string sceneName, Stack<MovementStep> npcMovementStep)
        {
            Node nextNode = targetNode;

            while (nextNode != null)
            {
                MovementStep newStep = new MovementStep();
                newStep.sceneName = sceneName;
                newStep.gridCoordinate = new Vector2Int(nextNode.gridPosition.x + originX, nextNode.gridPosition.y + originY);
                //压入堆栈
                npcMovementStep.Push(newStep);
                nextNode = nextNode.parentNode;
            }
        }
    }
}