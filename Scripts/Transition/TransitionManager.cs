using LouisFrice.Save;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LouisFrice.Transition
{
    public class TransitionManager : Singleton<TransitionManager>,ISaveable
    {
        [SceneName]
        public string startSceneName = string.Empty;

        private CanvasGroup fadeCanvasGroup;
        private bool isFade;

        public string GUID => GetComponent<DataGUID>().GUID;


        protected override void Awake()
        {
            base.Awake();
            //叠加
            SceneManager.LoadScene("UI", LoadSceneMode.Additive);
        }
        private void Start()
        {
            //注册当前实体去存档
            ISaveable saveable = this;
            saveable.RegisterSaveable();

            fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
        }
        //TODO:转换成开始新游戏
        //private IEnumerator Start()
        //{
        //    //注册当前实体去存档
        //    ISaveable saveable = this;
        //    saveable.RegisterSaveable();

        //    //CanvasGroup是唯一的 
        //    fadeCanvasGroup = FindObjectOfType<CanvasGroup>();

        //    yield return StartCoroutine(LoadSceneSetActive(startSceneName));
        //    EventHandler.CallAfterSceneLoadedEvent();
        //}

        private void OnEnable()
        {
            EventHandler.TransitionEvent += OnTransitionEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }
        private void OnDisable()
        {
            EventHandler.TransitionEvent -= OnTransitionEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }

        private void OnEndGameEvent()
        {
            StartCoroutine(UnloadScene());
        }

        private void OnStartNewGameEvent(int index)
        {
            StartCoroutine(LoadSaveDataScene(startSceneName));
        }

        private void OnTransitionEvent(string sceneName, Vector3 targetPosition)
        {
            if (!isFade)
            {
                StartCoroutine(Transition(sceneName, targetPosition));
            }
        }

        /// <summary>
        /// 异步加载场景并激活
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        private IEnumerator LoadSceneSetActive(string sceneName)
        {
            //异步加载场景并设置为增加模式，非切换模式
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            //获取当前异步加载出来的最后的一个场景并激活
            Scene newScene = SceneManager.GetSceneAt(SceneManager.loadedSceneCount - 1);
            SceneManager.SetActiveScene(newScene);
        }

        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="targetPosition">触发的目标位置</param>
        /// <returns></returns>
        private IEnumerator Transition(string sceneName, Vector3 targetPosition)
        {
            //卸载前的事件
            EventHandler.CallBeforeSceneUnloadEvent();
            //切入加载场景
            yield return TransitionFade(1);
            //卸载当前的场景
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            //加载下一个场景
            yield return LoadSceneSetActive(sceneName);
            //移动人物坐标
            EventHandler.CallMoveToPosition(targetPosition);
            //加载场景后的事件
            EventHandler.CallAfterSceneLoadedEvent();
            //取消加载场景
            yield return TransitionFade(0);
        }

        /// <summary>
        /// 淡入淡出场景
        /// </summary>
        /// <param name="targetAlpha">目标透明度，1是黑，0是透明</param>
        /// <returns></returns>
        private IEnumerator TransitionFade(float targetAlpha)
        {
            isFade = true;
            //在渐变过程中取消鼠标点击
            fadeCanvasGroup.blocksRaycasts = true;
            float speed = MathF.Abs(fadeCanvasGroup.alpha -  targetAlpha)/Settings.transitionFadeDuration;

            while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
            {
                //匀速插值计算，每帧变化speed值，逐渐变到targetAlpha
                fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha,targetAlpha,speed*Time.deltaTime);
                //每帧执行一次
                yield return null;
            }

            //渐变结束
            fadeCanvasGroup.blocksRaycasts = false;
            isFade = false;
        }

        private IEnumerator LoadSaveDataScene(string sceneName)
        {
            yield return TransitionFade(1);
            //如果是游戏过程中加载游戏进度，本游戏没有设置
            if(SceneManager.GetActiveScene().name != "PersistentScene")
            {
                EventHandler.CallBeforeSceneUnloadEvent();
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            }
            //加载场景并激活
            yield return LoadSceneSetActive(sceneName);

            EventHandler.CallAfterSceneLoadedEvent();
            yield return TransitionFade(0);
        }

        private IEnumerator UnloadScene()
        {
            EventHandler.CallBeforeSceneUnloadEvent();
            yield return TransitionFade(1f);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            yield return TransitionFade(0);
        }

        public GameSaveData GenerateSavaData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.dataSceneName = SceneManager.GetActiveScene().name;

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            //加载游戏进度场景
            StartCoroutine(LoadSaveDataScene(saveData.dataSceneName));
        }
    }
}