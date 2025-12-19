using UnityEditor;
using UnityEngine;

//PropertyDrawer需要放到Editor目录下
[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameDrawer : PropertyDrawer
{
    //个人觉得该方法太麻烦，建议直接用Enum来实现下拉列表或者用插件

    private int sceneIndex = -1;// 当前选中的场景索引（-1 表示未初始化）
    GUIContent[] sceneNames;// 存储场景名称的数组（GUIContent 用于显示文本和 tooltip）

    readonly string[] scenePathSplit = { "/", ".unity" };// 用于分割场景路径的分隔符（提取纯名称）
    /// <summary>
    /// 在 Inspector 中绘制字段的 UI
    /// </summary>
    /// <param name="position">绘制区域的矩形位置</param>
    /// <param name="property">当前处理的序列化字段,这里是字符串类型</param>
    /// <param name="label">字段在 Inspector 中显示的标签</param>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //检查 Build Settings 中是否有添加场景
        if (EditorBuildSettings.scenes.Length == 0) { return; }

        //首次绘制,调用 GetSceneNameArray 方法 初始化sceneNames 数组和 sceneIndex
        if (sceneIndex == -1)
        {
            GetSceneNameArray(property); 
        }
        int oldSceneIndex = sceneIndex;
        //绘制下拉框，根据选择的场景序号赋值给SceneIndex
        sceneIndex = EditorGUI.Popup(position,label,sceneIndex,sceneNames);
        //如果老的Index改变了，再修改，防止重复修改
        if (oldSceneIndex != sceneIndex) 
        {
            property.stringValue = sceneNames[sceneIndex].text;
        }
    }
    /// <summary>
    /// 读取 Build Settings 中的场景，解析出纯场景名称
    /// </summary>
    /// <param name="property"></param>
    private void GetSceneNameArray(SerializedProperty property)
    {
        //获取 Build Settings 中所有已添加的场景,包含场景路径等信息
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        //初始化数组
        sceneNames = new GUIContent[scenes.Length];

        for (int i = 0; i < scenes.Length; i++)
        {
            string path = scenes[i].path;// 场景的完整路径（如 "Assets/Scenes/01.Field.unity"）
            // 分割路径：用 "/" 和 ".unity" 作为分隔符，去掉空字符串
            //得到splitPath[0] = Assetss ,splitPath[1] = Scenes ,splitPath[2] = 01.Field
            string[] splitPath = path.Split(scenePathSplit, System.StringSplitOptions.RemoveEmptyEntries);
            string sceneName = "";

            if(splitPath.Length > 0)
            {
                // 取分割后的最后一个元素splitPath[2] = 01.Field
                sceneName = splitPath[splitPath.Length - 1];
            }
            else
            {
                // 路径解析失败时的占位符
                sceneName = "(Delete Scene)";
            }
            //存入数组（GUIContent 方便显示）
            sceneNames[i] = new GUIContent(sceneName);
        }

        if (sceneNames.Length == 0)
        {
            // 无场景时的提示
            sceneNames = new[] { new GUIContent("Check Build Settings") };
        }


        // 根据字段当前值，设置默认选中的场景索引,property是当前选择的值
        if (!string.IsNullOrEmpty(property.stringValue))
        {
            bool nameFound = false;
            // 如果字段当前值与某个场景名匹配，就选中该索引
            for (int i = 0; i < sceneNames.Length; i++)
            {
                if (sceneNames[i].text == property.stringValue)
                {
                    sceneIndex = i;
                    nameFound = true;
                    break;
                }
            }
            // 未找到匹配的场景名，默认选中第一个
            if (!nameFound) 
            {
                sceneIndex = 0;
            }
        }
        else
        {
            // 字段值为空时，默认选中第一个场景
            sceneIndex = 0;
        }
        // 确保字段值与选中的场景名一致
        property.stringValue = sceneNames[sceneIndex].text;
    }
}
