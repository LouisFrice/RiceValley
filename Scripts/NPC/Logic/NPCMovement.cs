using LouisFrice.AStar;
using LouisFrice.Save;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class NPCMovement : MonoBehaviour, ISaveable
{
    public ScheduleDataList_SO scheduleData;
    private SortedSet<ScheduleDetails> scheduleSet;
    private ScheduleDetails currentSchedule;

    [SerializeField]
    private string currentScene;
    private string targetScene;
    private Vector3Int currentGridPosition;
    private Vector3Int targetGridPosition;
    private Vector3Int nextGridPosition;
    private Vector3 nextWorldPosition;
    public string StartScene { set => currentScene = value; }
    [Header("移动属性")]
    public float normalSpeed = 2f;
    private float minSpeed = 1;
    private float maxSpeed = 3;
    private Vector2 dir;
    public bool isMoving;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D coll;
    private Animator anim;
    private Grid grid;

    private Stack<MovementStep> movementSteps;
    private Coroutine npcMoveRoutine;

    private bool isInitialised;
    private bool npcMove;
    private bool sceneLoaded;
    public bool interactable;
    public bool isFirstLoad;
    private Season currentSeason;

    //动画计时器
    private float animationBreakTime;
    private bool canPlayStopAnimation;
    private AnimationClip stopAnimationClip;
    public AnimationClip blankAnimationClip;    
    private AnimatorOverrideController animOverride;

    private TimeSpan GameTime => TimeManager.Instance.GameTime;

    public string GUID => GetComponent<DataGUID>().GUID;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        movementSteps = new Stack<MovementStep>();

        animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
        anim.runtimeAnimatorController = animOverride;

        scheduleSet = new SortedSet<ScheduleDetails>();
        foreach (var schedule in scheduleData.scheduleList)
        {
            scheduleSet.Add(schedule);
        }
    }
    private void Start()
    {
        //注册当前实体去存档
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }
    private void Update()
    {
        if (sceneLoaded)
        {
            SwitchAnimation();
        }
        //计时器
        animationBreakTime -= Time.deltaTime;
        //时间间隔小于0，就可以播放NPC待机动画了
        canPlayStopAnimation = animationBreakTime < 0;
    }
    private void FixedUpdate()
    {
        if (sceneLoaded)
        {
            Movement();
        }
    }
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int index)
    {
        isInitialised = false;
        isFirstLoad = true;
    }

    private void OnEndGameEvent()
    {
        sceneLoaded = false;
        npcMove = false;
        //停止Npc移动协程
        if(npcMoveRoutine  != null)
        {
            StopCoroutine(npcMoveRoutine);
        }
    }

    private void OnGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        int time = (hour * 100) + minute;
        currentSeason = season;

        ScheduleDetails matchSchedule = null;
        foreach (var schedule in scheduleSet)
        {
            //如果计划的时间就是当前的时间
            if (schedule.Time == time)
            {
                if(schedule.day != day && schedule.day != 0)
                {
                    continue;
                }
                if (schedule.season != season)
                {
                    continue;
                }
                matchSchedule = schedule;
            }
            //如果计划的时间还没到就直接跳出循环
            //这里因为scheduleSet已经是根据时间排好顺序的，不会影响后续计划
            else if(schedule.Time > time)
            {
                break;
            }
        }
        //如果有对应的计划就创造路径，有路径会自动移动
        if (matchSchedule != null)
        {
            BuildPath(matchSchedule);
        }
    }

    private void OnBeforeSceneUnloadEvent()
    {
        sceneLoaded = false;
    }

    private void OnAfterSceneLoadedEvent()
    {
        grid = FindObjectOfType<Grid>();
        CheckVisiable();

        if (!isInitialised)
        {
            initNPC();
            isInitialised = true;
        }

        sceneLoaded = true;

        //不是第一次加载就重新创建NPC的移动路径，防止NPC移动过程中保存
        if (!isFirstLoad)
        {
            currentGridPosition = grid.WorldToCell(transform.position);
            //0时0分0天代表立即执行
            ScheduleDetails schedule = new ScheduleDetails(0, 0, 0, currentSeason, 0, targetScene, (Vector2Int)targetGridPosition, stopAnimationClip, interactable);
            BuildPath(schedule);
            isFirstLoad = true;
        }
    }
    /// <summary>
    /// NPC移动方法
    /// </summary>
    private void Movement()
    {
        if (!npcMove)
        {
            if (movementSteps.Count > 0)
            {
                MovementStep step = movementSteps.Pop();
                currentScene = step.sceneName;
                CheckVisiable();
                nextGridPosition = (Vector3Int)step.gridCoordinate;
                TimeSpan stepTime = new TimeSpan(step.hour, step.minute, step.second);

                MoveToGridPosition(nextGridPosition, stepTime);
            }
            else if (!isMoving && canPlayStopAnimation)
            {
                StartCoroutine(SetStopAnimation());
            }
        }
    }

    private void MoveToGridPosition(Vector3Int gridPos,TimeSpan stepTime)
    {
        npcMoveRoutine = StartCoroutine(MoveRoutine(gridPos,stepTime));
    }
    /// <summary>
    /// NPC实际移动函数
    /// </summary>
    /// <param name="gridPos">移动的下一个点</param>
    /// <param name="stepTime">移动时间</param>
    /// <returns></returns>
    private IEnumerator MoveRoutine(Vector3Int gridPos, TimeSpan stepTime)
    {
        npcMove = true;
        nextWorldPosition = GetWorldPosition(gridPos);
        //如果还有剩余时间
        if (stepTime > GameTime)
        {
            //用来移动的时间差，秒为单位
            float timeToMove = (float)(stepTime.TotalSeconds - GameTime.TotalSeconds);
            //实际移动距离
            float distance = Vector3.Distance(transform.position, nextWorldPosition);   
            //实际移动速度，确保不低于最小速度
            float speed = Mathf.Max(minSpeed, (distance / timeToMove / Settings.secondThreshold));

            if(speed <= maxSpeed)
            {
                //大于像素最小距离，继续移动直到小于最小像素距离，代表到达位置了
                while (Vector3.Distance(transform.position, nextWorldPosition) > Settings.pixelSize)
                {
                    dir = (nextWorldPosition - transform.position).normalized;
                    Vector2 posOffset = new Vector2(dir.x * speed * Time.fixedDeltaTime , dir.y * speed * Time.fixedDeltaTime);
                    rb.MovePosition(rb.position + posOffset);
                    yield return new WaitForFixedUpdate();
                }
            }
        }
        //如果时间到了就瞬移
        rb.position = nextWorldPosition;
        currentGridPosition = gridPos;
        nextGridPosition = currentGridPosition;
        npcMove = false;
    }

    /// <summary>
    /// 根据Schedule构建路径
    /// </summary>
    /// <param name="schedule"></param>
    public void BuildPath(ScheduleDetails schedule)
    {
        movementSteps.Clear();
        currentSchedule = schedule;
        this.targetScene = schedule.targetScene;
        targetGridPosition = (Vector3Int)schedule.targetGridPosition;
        stopAnimationClip = schedule.clipAtStop;
        this.interactable = schedule.interactable;
        //同场景移动
        if (schedule.targetScene == currentScene)
        {
            AStar.Instance.BuildPath(schedule.targetScene, (Vector2Int)currentGridPosition, schedule.targetGridPosition, movementSteps);
        }
        //跨场景移动    
        else if (schedule.targetScene != currentScene)
        {
            SceneRoute sceneRoute = NPCManager.Instance.GetSceneRoute(currentScene, schedule.targetScene);

            if(sceneRoute != null)
            {
                for (int i = 0; i < sceneRoute.scenePathList.Count; i++)
                {
                    Vector2Int fromPos, gotoPos;
                    ScenePath path = sceneRoute.scenePathList[i];

                    //起始位置如果超过最大尺寸，99999
                    if(path.formGridCell.x >= Settings.maxGridSize || path.formGridCell.y >= Settings.maxGridSize)
                    {
                        //直接等于当前位置
                        fromPos = (Vector2Int)currentGridPosition;
                    }
                    else
                    {
                        fromPos = path.formGridCell;
                    }

                    //目标位置如果超过最大尺寸，99999
                    if (path.gotoGridCell.x >= Settings.maxGridSize || path.gotoGridCell.y >= Settings.maxGridSize)
                    {
                        //直接等于计划里的目标位置
                        gotoPos = schedule.targetGridPosition;
                    }
                    else
                    {
                        gotoPos = path.gotoGridCell;
                    }

                    AStar.Instance.BuildPath(path.sceneName, fromPos, gotoPos,movementSteps);
                }
            }
        }

        if(movementSteps.Count > 1)
        {
            //更新每一步对应的时间戳
            UpdateTimeOnPath();
        }
    }

    /// <summary>
    /// 更新路径时间
    /// </summary>
    private void UpdateTimeOnPath()
    {
        MovementStep previousStep = null;
        TimeSpan currentGameTime = GameTime;
        foreach (MovementStep step in movementSteps)
        {
            if(previousStep == null)
            {
                previousStep = step;
            }
            step.hour = currentGameTime.Hours;
            step.minute = currentGameTime.Minutes;
            step.second = currentGameTime.Seconds;

            TimeSpan gridMovementStepTime;
            //如果是斜向移动
            if (MoveInDiagonal(step, previousStep))
            {
                //斜向走一格需要的时间
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / normalSpeed / Settings.secondThreshold));
            }
            else
            {
                //走一格需要的时间
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellSize / normalSpeed / Settings.secondThreshold));
            }
            //累加获得下一步的时间戳
            currentGameTime = currentGameTime.Add(gridMovementStepTime);
            //循环下一步
            previousStep = step;
        }
    }
    /// <summary>
    /// 判断是否斜向移动
    /// </summary>
    /// <param name="currentStep">当前步</param>
    /// <param name="previousStep">上一步</param>
    /// <returns></returns>
    private bool MoveInDiagonal(MovementStep currentStep, MovementStep previousStep)
    {
        //如果当前这步和上一步的X、Y不相等，就是斜向的
        return (currentStep.gridCoordinate.x != previousStep.gridCoordinate.x) && (currentStep.gridCoordinate.y != previousStep.gridCoordinate.y);
    }

    /// <summary>   
    /// 检查当前场景是否为激活场景，显示隐藏NPC
    /// </summary>
    private void CheckVisiable()
    {
        if(currentScene == SceneManager.GetActiveScene().name)
        {
            SetActiveInScene();
        }
        else
        {
            SetInactiveInScene();
        }
    }

    /// <summary>
    /// 初始化NPC位置，放在网格中心
    /// </summary>
    private void initNPC()
    {
        targetScene = currentScene;

        //保持在当前坐标的网格中心点
        currentGridPosition = grid.WorldToCell(transform.position); 
        transform.position = new Vector3(currentGridPosition.x + Settings.gridCellSize / 2f, currentGridPosition.y + Settings.gridCellSize / 2f, 0);
        targetGridPosition = currentGridPosition;
    }

    /// <summary>
    /// NPC显示
    /// </summary>
    private void SetActiveInScene()
    {
        spriteRenderer.enabled = true;
        coll.enabled = true;
        //关闭影子
        transform.GetChild(0).gameObject.SetActive(true);
    }
    /// <summary>
    /// NPC隐藏
    /// </summary>
    private void SetInactiveInScene()
    {
        spriteRenderer.enabled = false;
        coll.enabled = false;
        //关闭影子
        transform.GetChild(0).gameObject.SetActive(false);
    }

    /// <summary>
    /// 输入网格坐标返回世界坐标中心点
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    private Vector3 GetWorldPosition(Vector3Int gridPos)
    {
        Vector3 worldPos = grid.CellToWorld(gridPos);
        return new Vector3(worldPos.x + Settings.gridCellSize / 2f, worldPos.y + Settings.gridCellSize / 2f, 0);
    }

    /// <summary>
    /// 切换NPC动画
    /// </summary>
    private void SwitchAnimation()
    {
        isMoving = transform.position != GetWorldPosition(targetGridPosition);
        anim.SetBool("IsMoving", isMoving);
        if (isMoving)
        {
            anim.SetBool("Exit", true);
            anim.SetFloat("DirX", dir.x);
            anim.SetFloat("DirY", dir.y);
        }
        else
        {
            anim.SetBool("Exit", false);
        }
    }
    /// <summary>
    /// 设置停止状态NPC动作
    /// </summary>
    /// <returns></returns>
    private IEnumerator SetStopAnimation()
    {
        //强制面向镜头
        anim.SetFloat("DirX", 0);
        anim.SetFloat("DirY", -1);

        animationBreakTime = Settings.animationBreakTime;

        if(stopAnimationClip != null)
        {
            //找到这个空的剪辑，替换为待机动画
            animOverride[blankAnimationClip] = stopAnimationClip;
            anim.SetBool("EventAnimation", true);
            yield return null;
            anim.SetBool("EventAnimation", false);
        }
        else
        {
            animOverride[stopAnimationClip] = blankAnimationClip;
            anim.SetBool("EventAnimation", false);
        }
    }

    public GameSaveData GenerateSavaData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.dataSceneName = this.currentScene;
        saveData.targetScene = this.targetScene;
        saveData.interactable = this.interactable;

        saveData.characterPosDic = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDic.Add("targetGridPosition", new SerializableVector3(targetGridPosition));
        saveData.characterPosDic.Add("currentPosition", new SerializableVector3(transform.position));
        
        if(stopAnimationClip != null)
        {
            //保存动画文件ID
            saveData.animationInstanceID = stopAnimationClip.GetInstanceID();
        }
        //记录当前季节给NPC的移动判断用
        saveData.timeDic = new Dictionary<string, int>();
        saveData.timeDic.Add("currentSeason", (int)currentSeason);
        
        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        //加载直接赋值已初始化
        isInitialised = true;
        isFirstLoad = false;
        this.currentScene = saveData.dataSceneName;
        this.targetScene = saveData.targetScene;
        this.interactable = saveData.interactable;

        Vector3 pos = saveData.characterPosDic["currentPosition"].ToVector3();
        Vector3Int gridPos = (Vector3Int)saveData.characterPosDic["targetGridPosition"].ToVector2Int();
        this.transform.position = pos;
        this.targetGridPosition = gridPos;

        if(saveData.animationInstanceID != 0)
        {
            this.stopAnimationClip = Resources.InstanceIDToObject(saveData.animationInstanceID) as AnimationClip;
        }
        this.currentSeason = (Season)saveData.timeDic["currentSeason"];
    }
}
