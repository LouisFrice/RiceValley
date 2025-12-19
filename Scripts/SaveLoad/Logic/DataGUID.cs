using UnityEngine;

[ExecuteAlways]
public class DataGUID : MonoBehaviour
{
    public string GUID;

    private void Awake()
    {
        if(GUID == string.Empty)
        {
            //生成16位字符串，保证唯一性
            GUID = System.Guid.NewGuid().ToString();
        }
    }
}
