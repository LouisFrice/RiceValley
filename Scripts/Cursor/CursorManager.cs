using LouisFrice.CropPlant;
using LouisFrice.Inventory;
using LouisFrice.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public Sprite normal, tool, seed, item;
    private Sprite currentSprite;
    private Image cursorImage;
    private RectTransform cursorCanvas;

    //建造图标跟随
    private Image buildImage;

    private Camera mainCarmera;
    private Grid currentGrid;
    private Vector3 mouseWorldPos;
    private Vector3Int mouseGridPos;

    private bool cursorEnable;
    //鼠标当前位置是否可用，不可用标红
    private bool cursorPositionValid;

    //物品信息
    private ItemDetails currentItem;
    //玩家位置
    private Transform playerTransform => FindObjectOfType<PlayerController>().transform;


    private void Start()
    {
        //进游戏就隐藏真实鼠标显示虚拟鼠标
        Cursor.visible = false;

        cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
        //拿到第一个子物体Image
        cursorImage = cursorCanvas.GetChild(0).GetComponent<Image>();
        //拿到建造图片
        buildImage = cursorCanvas.GetChild(1).GetComponent<Image>();
        buildImage.gameObject.SetActive(false);

        currentSprite = normal;
        SetCursorImage(currentSprite);

        mainCarmera = Camera.main;
    }

    private void Update()
    {
        if (cursorCanvas == null) { return;}
        cursorImage.transform.position = Input.mousePosition;

        //没有接触UI && 鼠标可用
        if (!InteractWithUI() && cursorEnable)
        {
            SetCursorImage(currentSprite);
            CheckCursorValid();
            CheckPlayerInput();
        }
        //鼠标放在UI上就显示普通鼠标图片
        else
        {
            SetCursorImage(normal);
            buildImage.gameObject.SetActive(false);
        }
    }

    private void CheckPlayerInput()
    {
        //左键按下 && 鼠标位置可用
        if(Input.GetMouseButtonDown(0) && cursorPositionValid)
        {
            //调用鼠标按下事件
            EventHandler.CallMouseClickedEvent(mouseWorldPos, currentItem);
        }
    }

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEven;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }
    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEven;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }

    private void OnBeforeSceneUnloadEven()
    {
        //场景加载出来前要禁用鼠标
        cursorEnable = false;
    }

    private void OnAfterSceneLoadedEvent()
    {
        currentGrid = FindObjectOfType<Grid>();
    }

    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        
        if (!isSelected)
        {
            currentItem = null;
            cursorEnable = false;

            currentSprite = normal;

            buildImage.gameObject.SetActive(false);
        }
        else //物品被选中才更换图标
        {
            currentItem = itemDetails;
            //物品添加新类型可以修改对应鼠标图标
            currentSprite = itemDetails.itemType switch
            {
                ItemType.HoeTool => tool,
                ItemType.CollectTool => tool,
                ItemType.ReapTool => tool,
                ItemType.ChopTool => tool,
                ItemType.WaterTool => tool,
                ItemType.BreakTool => tool,
                ItemType.Seed => seed,
                ItemType.Commodity => item,
                _ => normal,
            };

            cursorEnable = true;

            //显示家具建造图片
            if(itemDetails.itemType == ItemType.Furniture)
            {
                buildImage.gameObject.SetActive(true);
                buildImage.sprite = itemDetails.itemOnWorldSprite;
                buildImage.SetNativeSize();
            }
        }
    }
    /// <summary>
    /// 设置鼠标图片
    /// </summary>
    /// <param name="sprite">图片Sprite</param>
    private void SetCursorImage(Sprite sprite)
    {
        cursorImage.sprite = sprite;
        cursorImage.color = new Color(1, 1, 1, 1);
    }

    /// <summary>
    /// 鼠标是否在UI上
    /// </summary>
    /// <returns></returns>
    private bool InteractWithUI()
    {
        //EventSystem存在且鼠标在UI的对象上
        if(EventSystem.current != null&& EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 检查鼠标图标状态是否可用
    /// </summary>
    private void CheckCursorValid()
    {
        mouseWorldPos = mainCarmera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,-mainCarmera.transform.position.z));
        mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);
        //Debug.Log(mouseWorldPos + "|" + mouseGridPos);

        Vector3Int playerGridPos = currentGrid.WorldToCell(playerTransform.position);

        //建造图片跟随鼠标移动
        buildImage.rectTransform.position = Input.mousePosition;

        //判断在使用范围内
        if(Mathf.Abs(mouseGridPos.x - playerGridPos.x) > currentItem.itemUseRadius || 
            Mathf.Abs(mouseGridPos.y - playerGridPos.y) > currentItem.itemUseRadius)
        {
            SetCursorInValid();
            return;
        }

        TileDetails currentTile = GridManager.Instance.GetTileDetailsOnMousePosition(mouseGridPos);
        if(currentTile != null)
        {
            //当前地上的Crop
            CropDetails currentCrop = CropManager.Instance.GetCropDetails(currentTile.seedItemID);
            //鼠标指上的Crop
            Crop crop = GridManager.Instance.GetCropObject(mouseWorldPos);
            //手上拿的种子信息
            CropDetails selectCrop = CropManager.Instance.GetCropDetails(currentItem.itemID);

            //WORKFLOW:补充物品类型
            switch (currentItem.itemType)
            {
                case ItemType.Commodity:
                    //物品可以扔&&该瓦片允许被扔
                    if(currentItem.canDropped&&currentTile.canDropItem){ SetCursorValid(); } else { SetCursorInValid(); }
                    break;
                case ItemType.HoeTool:
                    //该瓦片允许挖坑
                    if (currentTile.canDig){ SetCursorValid(); }else { SetCursorInValid(); }
                    break;
                case ItemType.WaterTool:
                    //该瓦片挖过坑 && 该瓦片没被浇水
                    if (currentTile.daysSinceDug > -1 && currentTile.daySinceWatered == -1) { SetCursorValid(); } else { SetCursorInValid(); }
                    break;
                case ItemType.Seed:
                    //该瓦片被挖过 && 没种过种子 && 是不是应季种子
                    if(currentTile.daysSinceDug > -1 && currentTile.seedItemID == -1 && selectCrop.CheckCropSeasonAvailable(TimeManager.Instance.GameSeaon)) 
                    { SetCursorValid(); } else { SetCursorInValid(); }
                    break;
                //收获
                case ItemType.CollectTool:
                    if(currentCrop != null)
                    {
                        //检查该工具是否可以采集
                        if (currentCrop.CheckToolAvailable(currentItem.itemID))
                        {
                            //种子生长天数 > 种子成熟天数
                            if (currentTile.growthDays >= currentCrop.TotalGrowthDays) { SetCursorValid(); } else { SetCursorInValid(); }
                        }
                    }
                    else //该位置没有种子
                    {
                        SetCursorInValid();
                    }
                    break;
                //砍树
                case ItemType.ChopTool:
                    if (crop != null)
                    {
                        //作物已经成熟 && 该工具可以收获该作物
                        if (crop.CanHarvest && crop.cropDetails.CheckToolAvailable(currentItem.itemID)) { SetCursorValid(); } else { SetCursorInValid(); }
                    }
                    else 
                    {
                        SetCursorInValid();
                    }
                    break;
                //挖坑
                case ItemType.BreakTool:
                    if (crop != null)
                    {
						//该工具可以收获该作物
						if (crop.cropDetails.CheckToolAvailable(currentItem.itemID)) { SetCursorValid(); } else { SetCursorInValid(); }
					}
					else
					{
						SetCursorInValid();
					}
					break;
                //镰刀除杂草
                case ItemType.ReapTool:
                    //如果鼠标周围有可收割的杂草
                    if (GridManager.Instance.HaveReapableItemsInRadius(mouseWorldPos,currentItem)) { SetCursorValid(); } else { SetCursorInValid(); }
                    break;
                //放置家具
                case ItemType.Furniture:
                    BlueprintDetails blueprintDetails = InventoryManager.Instance.blueprintData.GetBlueprintDetails(currentItem.itemID);
                    //如果 瓦片可以放置家具 && 有建造材料 && 鼠标当前位置没有其他家具
                    if (currentTile.canPlaceFurniture && InventoryManager.Instance.CheckStock(currentItem.itemID) && !HaveFurnitureInRadius(blueprintDetails)) 
                    { SetCursorValid(); } else { SetCursorInValid(); }
                    break;
            }
        }
        else
        {
            //没有瓦片鼠标设置为无法互动
            SetCursorInValid();
        }
    }
    /// <summary>
    /// 设置鼠标可用
    /// </summary>
    private void SetCursorValid()
    {
        cursorPositionValid = true;
        cursorImage.color = new Color(1, 1, 1, 1);
        buildImage.color = new Color(1,1,1,1);
    }
    /// <summary>
    /// 设置鼠标不可用，变透明
    /// </summary>
    private void SetCursorInValid()
    {
        cursorPositionValid = false;
        cursorImage.color = new Color(1, 1, 1, 0.4f);
        buildImage.color = new Color(1, 1, 1, 0.4f);
    }
    /// <summary>
    /// 检测当前位置是否已经放置过家具，防止叠放
    /// </summary>
    /// <param name="blueprintDetails"></param>
    /// <returns></returns>
    private bool HaveFurnitureInRadius(BlueprintDetails blueprintDetails)
    {
        GameObject buildItem = blueprintDetails.buildPrefab;
        //拿到鼠标位置
        Vector2 point = mouseWorldPos;
        //拿到家具的碰撞体大小
        var size = buildItem.GetComponent<BoxCollider2D>().size;
        //鼠标检测，返回一个碰撞体包括trigger的，地图范围需要Layer 改为 Ignore Raycast，否则会优先返回地图范围的collider
        var collider = Physics2D.OverlapBox(point, size, 0);
        if (collider != null)
        {
            //如果有家具脚本就不能放置
            return collider.GetComponent<Furniture>();
        }
        else
        {
            return false;
        }
    }
}
