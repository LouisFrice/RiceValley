using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemEditor : EditorWindow
{
    private ItemDataList_SO dataBase;
    private List<ItemDetails> itemList = new List<ItemDetails>();
    private VisualTreeAsset itemRowTemplate;

    //左侧的ItemList
    private ListView itemListView;
    //右侧的ItemDetails
    private ScrollView itemDetailsSection;
    //左侧当前选中的Item
    private ItemDetails activeItem;
    //Icon修改后的预览图
    private VisualElement iconPreview;
    //默认的预览图片
    private Sprite defaultIcon;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("LouisFrice/ItemEditor")]
    public static void ShowExample()
    {
        ItemEditor wnd = GetWindow<ItemEditor>();
        wnd.titleContent = new GUIContent("ItemEditor");
    }
    /// <summary>
    /// 打开窗口自动调用一次
    /// </summary>
    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        //VisualElement label = new Label("Hello World! From C#");
        //root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();  
        root.Add(labelFromUXML);
        //默认的预览图片
        defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Mstudio/Art/Items/Icons/icon_Game.png");
        //拿到itemRowTemplate模板
        itemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UIBuilder/ItemRowTemplate.uxml");
        //拿到ItemEditor里的itemListView
        //root是最顶层的 VisualElement
        //可以直接越级查找，重名返回第一个
        itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");
        itemDetailsSection = root.Q<ScrollView>("ItemDetails");
        iconPreview = itemDetailsSection.Q<VisualElement>("Icon");

        //获得按键
        root.Q<Button>("AddButton").clicked += OnAddButtonClicked;
        root.Q<Button>("DeleteButton").clicked += OnDeleteButtonClicked;

        //加载ScriptObject数据
        LoadDataBase();
        //生成listview
        GenerateListView();
    }

    private void OnDeleteButtonClicked()
    {
        //移除当前选择的ItemDetails
        itemList.Remove(activeItem);
        itemListView.Rebuild();
        itemDetailsSection.visible = false;
    }

    private void OnAddButtonClicked()
    {
        //按加号添加新的ItemDetails
        ItemDetails newItem = new ItemDetails();
        newItem.itemID = 1001 + itemList.Count;
        newItem.ItemName = "NewItem";
        itemList.Add(newItem);
        itemListView.Rebuild();
    }
    /// <summary>
    /// 加载ItemDataList_SO(ScriptableObject)
    /// </summary>
    private void LoadDataBase()
    {
        //找到Asset里的ItemDataList_SO这个文件的GUIDs存入string数组里，有两个，有多个同名的也会一起找到
        string[] dataArray = AssetDatabase.FindAssets("ItemDataList_SO");
        if(dataArray.Length > 1)
        {
            //Debug.Log(dataArray[0]); //是ItemDataList_SO.ScriptObject
            //Debug.Log(dataArray[1]); //是ItemDataList_SO.cs
            //GUIDs转换成文件路径
            string path = AssetDatabase.GUIDToAssetPath(dataArray[0]);
            //Debug.Log(path);
            //通过文件路径加载存入ItemDataList_SO
            dataBase = AssetDatabase.LoadAssetAtPath<ItemDataList_SO>(path);
        }
        //得到ItemDataList_SO里的List<ItemDetails> 
        itemList = dataBase.itemDetailList;
        //如果不标记就无法保存数据-固定写法
        EditorUtility.SetDirty(dataBase);
        //Debug.Log(itemList[0].itemID);
    }
    /// <summary>
    /// 生成物品列表
    /// </summary>
    private void GenerateListView()
    {
        //克隆itemRowTemplate
        Func<VisualElement> makeItem = () => itemRowTemplate.CloneTree();
        //绑定item，e是对应的VisualElement，i是序号，类似For里的i
        Action<VisualElement, int> bindItem = (e, i) =>
        {
            if(i < itemList.Count)
            {
                if (itemList[i] != null) 
                {
                    e.Q<VisualElement>("Icon").style.backgroundImage = itemList[i].itemIcon == null ? defaultIcon.texture : itemList[i].itemIcon.texture;
                }
                e.Q<Label>("Name").text = itemList[i] == null ? "Null Item" : itemList[i].ItemName;
            }
        };
        //传入得到的itemlist给listview
        //itemListView.fixedItemHeight = 60;
        itemListView.itemsSource = itemList;
        itemListView.makeItem = makeItem;
        itemListView.bindItem = bindItem;

        //Itemlist选择的部分修改触发事件
        itemListView.selectionChanged += OnListSelectionChange;

        //右侧面板默认不可见
        itemDetailsSection.visible = false;
    }
    /// <summary>
    /// 在左侧选择物品后的回调
    /// </summary>
    /// <param name="selectedItem"></param>
    private void OnListSelectionChange(IEnumerable<object> selectedItem)
    {
        activeItem = (ItemDetails)selectedItem.First();
        GetItemDetails();
        //选择后显示右侧面板
        itemDetailsSection.visible = true;
    }
    /// <summary>
    /// 获取物品的详细信息
    /// </summary>
    private void GetItemDetails()
    {
        //在UItoolkit修改数值,同步到ScriptObject
        itemDetailsSection.MarkDirtyRepaint();
        //注册右侧面板修改事件
        itemDetailsSection.Q<IntegerField>("ItemID").value = activeItem.itemID;
        itemDetailsSection.Q<IntegerField>("ItemID").RegisterValueChangedCallback((evt) =>
        {
            activeItem.itemID = evt.newValue;
        });
        itemDetailsSection.Q<TextField>("ItemName").value = activeItem.ItemName;
        itemDetailsSection.Q<TextField>("ItemName").RegisterValueChangedCallback((evt) =>
        {
            activeItem.ItemName = evt.newValue;
            itemListView.Rebuild();  //修改右侧面板左侧跟着重新绘制
        });
        itemDetailsSection.Q<EnumField>("ItemType").value = activeItem.itemType;
        itemDetailsSection.Q<EnumField>("ItemType").RegisterValueChangedCallback((evt) =>
        {
            activeItem.itemType = (ItemType)evt.newValue;
        });
        //修改右侧面板左侧跟着重新绘制
        iconPreview.style.backgroundImage = activeItem.itemIcon == null ? defaultIcon.texture : activeItem.itemIcon.texture;
        itemDetailsSection.Q<ObjectField>("ItemIcon").value = activeItem.itemIcon;
        itemDetailsSection.Q<ObjectField>("ItemIcon").RegisterValueChangedCallback((evt) =>
        {
            Sprite newIcon = evt.newValue as Sprite;
            activeItem.itemIcon = newIcon;
            iconPreview.style.backgroundImage = newIcon == null ? defaultIcon.texture : newIcon.texture;
            itemListView.Rebuild();  //修改右侧面板左侧跟着重新绘制
        });
        itemDetailsSection.Q<ObjectField>("ItemSprite").value = activeItem.itemOnWorldSprite;
        itemDetailsSection.Q<ObjectField>("ItemSprite").RegisterValueChangedCallback((evt) =>
        {
            activeItem.itemOnWorldSprite = evt.newValue as Sprite;
        });
        itemDetailsSection.Q<TextField>("Description").value = activeItem.itemDescription;
        itemDetailsSection.Q<TextField>("Description").RegisterValueChangedCallback((evt) =>
        {
            activeItem.itemDescription = evt.newValue;
        });
        itemDetailsSection.Q<IntegerField>("ItemUseRadius").value = activeItem.itemUseRadius;
        itemDetailsSection.Q<IntegerField>("ItemUseRadius").RegisterValueChangedCallback((evt) =>
        {
            activeItem.itemUseRadius = evt.newValue;
        });
        itemDetailsSection.Q<Toggle>("CanPickedUp").value = activeItem.canPickUp;
        itemDetailsSection.Q<Toggle>("CanPickedUp").RegisterValueChangedCallback((evt) =>
        {
            activeItem.canPickUp = evt.newValue;
        });
        itemDetailsSection.Q<Toggle>("CanDropped").value = activeItem.canDropped;
        itemDetailsSection.Q<Toggle>("CanDropped").RegisterValueChangedCallback((evt) =>
        {
            activeItem.canDropped = evt.newValue;
        });
        itemDetailsSection.Q<Toggle>("CanCarried").value = activeItem.canCarried;
        itemDetailsSection.Q<Toggle>("CanCarried").RegisterValueChangedCallback((evt) =>
        {
            activeItem.canCarried = evt.newValue;
        });
        itemDetailsSection.Q<IntegerField>("Price").value = activeItem.itemPrice;
        itemDetailsSection.Q<IntegerField>("Price").RegisterValueChangedCallback((evt) =>
        {
            activeItem.itemPrice = evt.newValue;
        });
        itemDetailsSection.Q<Slider>("SellPercentage").value = activeItem.sellPercentage;
        itemDetailsSection.Q<Slider>("SellPercentage").RegisterValueChangedCallback((evt) =>
        {
            activeItem.sellPercentage = evt.newValue;
        });
    }
}
