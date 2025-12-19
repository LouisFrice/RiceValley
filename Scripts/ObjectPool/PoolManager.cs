using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


public class PoolManager : MonoBehaviour
{
    public int maxPoolSize = 10; 
    //多个对象池，每个Prefab对应一个对象池
    public List<GameObject> poolPrefabs;
    private List<ObjectPool<GameObject>> poolEffectList = new List<ObjectPool<GameObject>>();
    
    //音效队列，先进先出
    private Queue<GameObject> soundQueue = new Queue<GameObject>();

    private void Start()
    {
        CreatePool();
    }
    private void OnEnable()
    {
        EventHandler.ParticleEffectEvent += OnParticleEffectEvent;
        EventHandler.InitSoundEffect += OnInitSoundEffect;
    }
    private void OnDisable()
    {
        EventHandler.ParticleEffectEvent -= OnParticleEffectEvent;
        EventHandler.InitSoundEffect -= OnInitSoundEffect;
    }


    private void OnParticleEffectEvent(ParticleEffectType effectType, Vector3 pos)
    {
        //WORKFLOW:根据特效补全
        ObjectPool<GameObject> objPool = effectType switch
        {
            ParticleEffectType.LeavesFalling01 => poolEffectList[0],
            ParticleEffectType.LeavesFalling02 => poolEffectList[1],
            ParticleEffectType.RockBreak => poolEffectList[2],
            ParticleEffectType.ReapableScenery => poolEffectList[3],
            //[4]是音效
            _ => null
        };

        GameObject obj = objPool.Get();
        obj.transform.position = pos;
        StartCoroutine(ReleaseRoutine(objPool,obj));
    }

    /// <summary>
    /// 等待粒子播放结束后释放
    /// </summary>
    /// <param name="pool">对象池</param>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    private IEnumerator ReleaseRoutine(ObjectPool<GameObject> pool , GameObject obj)
    {
        yield return new WaitForSeconds(1.5f);
        pool.Release(obj);
    }

    /// <summary>
    /// 创建对象池
    /// </summary>
    private void CreatePool()
    {
        foreach (GameObject gameObject in poolPrefabs)
        {
            Transform parent = new GameObject(gameObject.name).transform;
            parent.SetParent(this.transform);
            var newPool = new ObjectPool<GameObject>(() => { return OnCreatFunc(gameObject, parent); }, 
                                                                    OnActionOnGet, 
                                                                    OnActionOnRelease, 
                                                                    OnActionOnDestroy, 
                                                                    true, maxPoolSize,10000);
            poolEffectList.Add(newPool);
        }
    }

    private GameObject OnCreatFunc(GameObject gameObject,Transform parent)
    {
        return Instantiate(gameObject, parent);
    }

    void OnActionOnGet(GameObject gameObject)
    {
        gameObject.SetActive(true);
    }
    void OnActionOnRelease(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }
    void OnActionOnDestroy(GameObject gameObject)
    {
        Destroy(gameObject);
    }

    //音效相关
    //有问题，没办法使用该API实现音效对象池
    /// <summary>
    /// 初始化音效
    /// </summary>
    /// <param name="soundDetails"></param>
    //private void OnInitSoundEffect(SoundDetails soundDetails)
    //{
    //    ObjectPool<GameObject> pool = poolEffectList[4];
    //    GameObject obj = pool.Get();
    //    obj.GetComponent<Sound>().SetSound(soundDetails);
    //    StartCoroutine(DisableSound(pool, obj, soundDetails));
    //}

    ///// <summary>
    ///// 摧毁回收音效
    ///// </summary>
    ///// <param name="pool"></param>
    ///// <param name="obj"></param>
    ///// <param name="soundDetails"></param>
    ///// <returns></returns>
    //private IEnumerator DisableSound(ObjectPool<GameObject> pool, GameObject obj, SoundDetails soundDetails)
    //{
    //    //等待音效播放时长结束
    //    yield return new WaitForSeconds(soundDetails.soundClip.length);
    //    pool.Release(obj);
    //}


    /// <summary>
    /// 手动创建音效对象池
    /// </summary>
    private void CreatSoundPool()
    {
        Transform parent = new GameObject(poolPrefabs[4].name).transform;
        parent.SetParent(this.transform);

        for (int i = 0; i < 20; i++)
        {
            GameObject sound = Instantiate(poolPrefabs[4],parent);
            sound.SetActive(false);
            soundQueue.Enqueue(sound);
        }
    }
    /// <summary>
    /// 获得对象池里的音效
    /// </summary>
    /// <returns></returns>
    private GameObject GetPoolObject()
    {
        if(soundQueue.Count < 2)
        {
            CreatSoundPool();
        }
        return soundQueue.Dequeue();
    }
    private void OnInitSoundEffect(SoundDetails soundDetails)
    {
        GameObject obj = GetPoolObject();
        obj.GetComponent<Sound>().SetSound(soundDetails);
        obj.SetActive(true);

        StartCoroutine(DisableSound(obj, soundDetails));
    }
    private IEnumerator DisableSound(GameObject obj, SoundDetails soundDetails)
    {
        //等待音效播放时长结束
        yield return new WaitForSeconds(soundDetails.soundClip.length);
        obj.SetActive(false);
        soundQueue.Enqueue(obj);
    }
}
