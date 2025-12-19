using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace LouisFrice.Save
{
    public class SaveLoadManager : Singleton<SaveLoadManager>
    {
        private List<ISaveable> saveableList = new List<ISaveable>();
        //构造函数创建3个空的DataSlot （Count=3，Capacity=3）
        public List<DataSlot> dataSlots = new List<DataSlot>(new DataSlot[3]);

        private string jsonFolder;

        private int currentDataIndex;

        protected override void Awake()
        {
            base.Awake();
            jsonFolder = Application.persistentDataPath + "/SaveData";
            ReadSaveData();
        }
        //private void Update()
        //{
        //    //测试保存加载
        //    if (Input.GetKeyDown(KeyCode.I))
        //    {
        //        Save(currentDataIndex);
        //    }
        //    if (Input.GetKeyDown(KeyCode.O))
        //    {
        //        Load(currentDataIndex);
        //    }
        //}
        private void OnEnable()
        {
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }
        private void OnDisable()
        {
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }

        private void OnEndGameEvent()
        {
            //返回菜单自动保存游戏
            Save(currentDataIndex);
        }

        private void OnStartNewGameEvent(int index)
        {
            currentDataIndex = index;
        }

        private void ReadSaveData()
        {
            if (Directory.Exists(jsonFolder))
            {
                for (int i = 0; i < dataSlots.Count; i++)
                {
                    string resultPath = jsonFolder + "/data" + i + ".json";
                    if (File.Exists(resultPath))
                    {
                        string stringData = File.ReadAllText(resultPath);
                        DataSlot jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);
                        dataSlots[i] = jsonData;
                    }
                }
            }
        }


        public void RegisterSaveable(ISaveable saveable)
        {
            if (!saveableList.Contains(saveable))
            {
                saveableList.Add(saveable);
            }
        }

        public void Save(int index)
        {
            DataSlot data = new DataSlot();
            foreach (ISaveable saveable in saveableList)
            {
                data.dataDic.Add(saveable.GUID, saveable.GenerateSavaData());
            }
            dataSlots[index] = data;

            string resultPath = jsonFolder + "/data" + index + ".json";
            //List数据转Json存储，Formatting.Indented代表生成的数据要排版方便观看
            string jsonData = JsonConvert.SerializeObject(dataSlots[index], Formatting.Indented);

            if (!File.Exists(resultPath))
            {
                //创建文件夹
                Directory.CreateDirectory(jsonFolder);
            }
            //写入jsonFolder文件夹
            File.WriteAllText(resultPath, jsonData);

            Debug.Log("data" + index + "保存成功");
        }

        public void Load(int index)
        {
            currentDataIndex = index;
            string resultPath = jsonFolder + "/data" + index + ".json";
            string stringData = File.ReadAllText(resultPath);
            DataSlot jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);

            foreach (ISaveable saveable in saveableList)
            {
                saveable.RestoreData(jsonData.dataDic[saveable.GUID]);
            }
        }
    }
}