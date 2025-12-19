using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode] //在编辑模式和运行模式都会执行
public class GridMap : MonoBehaviour
{
    public MapData_SO mapData;
    public GridType gridType;
    private Tilemap currentTilemap;

    //编辑模式下是(激活/挂载)该脚本就会运行
    private void OnEnable()
    {
        //Unity的编辑模式下
        if (!Application.IsPlaying(this))
        {
            currentTilemap = GetComponent<Tilemap>();
            if(mapData  != null)
            {
                mapData.tileProperties.Clear();
            }
        }
        
    }

    //编辑模式下是取消(激活/挂载)该脚本就会运行
    //场景被关闭或 Unity 编辑器关闭时也会运行
    private void OnDisable()
    {
        if (!Application.IsPlaying(this))
        {
            currentTilemap = GetComponent<Tilemap>();
            //更新当前瓦片信息
            UpdateTileProperties();

#if UNITY_EDITOR //确保是在编辑器模式下运行，正常运行游戏没有影响
            if(mapData != null)
            {
                //标记Dirty才可以实时的保存ScriptObject，否则退出Unity数据不会保存
                EditorUtility.SetDirty(mapData);
            }
#endif

        }
    }

    /// <summary>
    /// 更新地图上的瓦片信息存入mapData
    /// </summary>
    private void UpdateTileProperties()
    {
        //压缩瓦片地图大小，去掉不存在的瓦片
        currentTilemap.CompressBounds();
        if (!Application.IsPlaying(this))
        {
            if (mapData != null)
            {
                //获得瓦片地图的左下角和右上角坐标
                Vector3Int startPos = currentTilemap.cellBounds.min;
                Vector3Int endPos = currentTilemap.cellBounds.max;
                for (int x = startPos.x; x < endPos.x; x++)
                {
                    for(int y = startPos.y; y < endPos.y; y++)
                    {
                        //拿到了每个瓦片的信息
                        TileBase tile = currentTilemap.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                        {
                            TileProperty newTile = new TileProperty()
                            {
                                tileCoordinate = new Vector2Int(x, y),
                                boolTypeValue = true,
                                gridType = this.gridType
                            };
                            //把每个瓦片信息存入MapData
                            mapData.tileProperties.Add(newTile);
                        }
                    }
                }
            }
        }
    }
}
