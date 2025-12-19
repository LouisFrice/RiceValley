using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private GameObject menuCanvas;
    public GameObject menuPanel;

    public Button pauseButton;
    public GameObject pausePanel;
    public Slider musicSlider;

    private bool isVolumeLoaded;


    private void Awake()
    {
        pauseButton.onClick.AddListener(TogglePausePanel);
        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
    }
    private void Start()
    {
        menuCanvas = GameObject.FindWithTag("MenuCanvas");
        //加载主菜单
        Instantiate(menuPanel, menuCanvas.transform);

        //musicSlider.value = AudioManager.Instance.GetMasterVolume();
    }
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }

    private void OnAfterSceneLoadedEvent()
    {
        if(menuCanvas.transform.childCount > 0)
        {
            //进入游戏时摧毁主菜单
            Destroy(menuCanvas.transform.GetChild(0).gameObject);
        }

        if (!isVolumeLoaded)
        {
            //场景加载后就加载一次音量
            musicSlider.value = AudioManager.Instance.GetMasterVolume();
            isVolumeLoaded = true;
        }
    }

    private void TogglePausePanel()
    {
        //获得面板打开状态
        bool isOpen = pausePanel.activeInHierarchy;

        //如果是开的就关掉
        if (isOpen)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1.0f;
        }
        else
        {
            //强制回收内存
            System.GC.Collect();

            pausePanel.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void ReturnMenuCanvas()
    {
        Time.timeScale = 1.0f;
        StartCoroutine(BackToMenu());
    }

    private IEnumerator BackToMenu()
    {
        pausePanel.SetActive(false);
        EventHandler.CallEndGameEvent();
        yield return new WaitForSeconds(1f);
        //加载主菜单
        Instantiate(menuPanel, menuCanvas.transform);
    }
}
