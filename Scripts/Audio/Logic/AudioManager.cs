using LouisFrice.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : Singleton<AudioManager>,ISaveable
{
    [Header("音乐数据库")]
    public SoundDetailsList_SO soundDetailsData;
    public SceneSoundList_SO sceneSoundData;
    [Header("AudioSource")]
    public AudioSource ambientSource;
    public AudioSource gameSource;
    [Header("AudioMixer")]
    public AudioMixer audioMixer;
    [Header("Snapshots")]
    public AudioMixerSnapshot normalSnapshots;
    public AudioMixerSnapshot AmbientOnlySnapshot;
    public AudioMixerSnapshot MuteSnapshot;

    private bool isNewGame;
    private float MasterVolume = 1;

    //音乐协程
    private Coroutine soundRoutine;

    //音乐延迟开始播放的时间
    public float MusicStartSecond => Random.Range(5f, 10f);

    public string GUID => GetComponent<DataGUID>().GUID;

    private void Start()
    {
        //注册当前实体去存档
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.PlaySoundEvent += OnPlaySoundEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.PlaySoundEvent -= OnPlaySoundEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
        isNewGame = true;
    }

    private void OnEndGameEvent()
    {
        if(soundRoutine != null)
        {
            StopCoroutine(soundRoutine);
        }
        MuteSnapshot.TransitionTo(1f);
        
    }

    private void OnPlaySoundEvent(SoundName soundName)
    {
        SoundDetails soundDetails = soundDetailsData.GetSoundDetails(soundName);
        if(soundDetails != null)
        {
            EventHandler.CallInitSoundEffect(soundDetails);
        }
    }

    private void OnAfterSceneLoadedEvent()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        SceneSoundItem sceneSound = sceneSoundData.GetSceneSoundItem(currentScene);
        if (sceneSound == null) { return; }
        SoundDetails ambient = soundDetailsData.GetSoundDetails(sceneSound.ambient);
        SoundDetails music = soundDetailsData.GetSoundDetails(sceneSound.music);

        if (soundRoutine != null)
        {
            StopCoroutine(soundRoutine);
        }
        soundRoutine = StartCoroutine(PlaySoundRoutine(music, ambient));
    }

    /// <summary>
    /// 延迟播放音乐协程
    /// </summary>
    /// <param name="music"></param>
    /// <param name="ambient"></param>
    /// <returns></returns>
    private IEnumerator PlaySoundRoutine(SoundDetails music, SoundDetails ambient)
    {
        if (isNewGame)
        {
            //等待timeline播放结束后才播放下面音乐
            yield return new WaitForSeconds(TimelineManager.Instance.TimelineDuration);
            isNewGame = false;
        }
        if (music != null && ambient != null)
        {
            //先播放几秒音效再播放背景音乐
            PlayAmbientCilp(ambient, Settings.ambientTransitionSecond);
            yield return new WaitForSeconds(MusicStartSecond);
            PlayMusicCilp(music, Settings.musicTransitionSecond);
        }
    }
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="soundDetails"></param>
    private void PlayMusicCilp(SoundDetails soundDetails,float transitionTime)
    {
        audioMixer.SetFloat("MusicVolume", ConvertSoundVolumeTodB(soundDetails.soundVolume));
        gameSource.clip = soundDetails.soundClip;
        //可以在别的地方主动关闭
        if (gameSource.isActiveAndEnabled)
        {
            gameSource.Play();
        }
        //过渡到该音量
        normalSnapshots.TransitionTo(transitionTime);
    }
    /// <summary>
    /// 播放环境音效
    /// </summary>
    /// <param name="soundDetails"></param>
    private void PlayAmbientCilp(SoundDetails soundDetails, float transitionTime)
    {
        audioMixer.SetFloat("AmbientVolume", ConvertSoundVolumeTodB(soundDetails.soundVolume));
        ambientSource.clip = soundDetails.soundClip;
        //可以在别的地方主动关闭
        if (ambientSource.isActiveAndEnabled)
        {
            ambientSource.Play();
        }
        //过渡到该音量
        AmbientOnlySnapshot.TransitionTo(transitionTime);
    }
    /// <summary>
    /// volume（0~1） 转换为 dB（-80~20）
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    private float ConvertSoundVolumeTodB(float volume)
    {
        //return (volume * 100 - 80);

        //豆包生成，0~0.5 覆盖低音量、0.5~1 覆盖高音量，-60~0 dB
        // 第一步：限制滑块值在0~1，防止异常输入
        volume = Mathf.Clamp01(volume);
        float dbValue;

        if (volume <= 0.5f)
        {
            // 0~0.5 → -60~-10 dB（低音量区间，占50 dB范围，调节更细腻）
            // 公式推导：
            // volume=0 → -60 + (0*2)*50 = -60 dB
            // volume=0.5 → -60 + (0.5*2)*50 = -10 dB
            dbValue = -60f + (volume * 2) * 50f;
        }
        else
        {
            // 0.5~1 → -10~0 dB（高音量区间，占10 dB范围，避免音量突变）
            // 公式推导：
            // volume=0.5 → -10 + ((0.5-0.5)*2)*10 = -10 dB
            // volume=1 → -10 + ((1-0.5)*2)*10 = 0 dB
            dbValue = -10f + ((volume - 0.5f) * 2) * 10f;
        }

        // 最终限制dB范围在-60~0，双重保险
        return Mathf.Clamp(dbValue, -60f, 0f);
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", ConvertSoundVolumeTodB(volume));
        MasterVolume = volume;
    }

    public float GetMasterVolume()
    {
        return MasterVolume;
    }

    public GameSaveData GenerateSavaData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.MasterVolume = this.MasterVolume;
        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        this.MasterVolume = saveData.MasterVolume;
    }
}
