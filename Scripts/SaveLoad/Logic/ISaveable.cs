namespace LouisFrice.Save
{
    public interface ISaveable
    {
        string GUID {  get; }
        /// <summary>
        /// 存储数据
        /// </summary>
        /// <returns></returns>
        GameSaveData GenerateSavaData();
        /// <summary>
        /// 读取恢复数据
        /// </summary>
        /// <param name="saveData"></param>
        void RestoreData(GameSaveData saveData);

        void RegisterSaveable()
        {
            SaveLoadManager.Instance.RegisterSaveable(this);
        }
        
    }
}