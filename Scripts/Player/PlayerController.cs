using LouisFrice.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour,ISaveable
{
    private Rigidbody2D rb;

    public float speed = 5;
    private float inputX;
    private float inputY;
    private Vector2 movementInput;
    //动画相关
    private Animator[] animators;
    private bool isMoving;
    //禁止操控玩家
    private bool inputDisable;

    //动画使用工具
    private float mouseX;
    private float mouseY;

    //获取唯一GUID
    public string GUID => GetComponent<DataGUID>().GUID;

    //private bool useTool;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animators = GetComponentsInChildren<Animator>();
        inputDisable = true;
    }
    private void Start()
    {
        //注册当前实体去存档
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }

    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.MoveToPosition += OnMoveToPosition;
        EventHandler.MouseClickedEvent += OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent += OnCallStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;


    }
    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.MoveToPosition -= OnMoveToPosition;
        EventHandler.MouseClickedEvent -= OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
		EventHandler.StartNewGameEvent -= OnCallStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void OnEndGameEvent()
    {
        inputDisable = true;
    }

    private void OnCallStartNewGameEvent(int index)
    {
        inputDisable = false;
        //回到初始位置
        this.transform.position = Settings.playerStartPos;
    }

    private void OnUpdateGameStateEvent(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Play:
                inputDisable = false;
                break;
            case GameState.Pause:
                inputDisable = true;
                break;
        }
    }

    private void OnMouseClickedEvent(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        //工具类的逻辑
        if(itemDetails.itemType != ItemType.Commodity && itemDetails.itemType != ItemType.Seed && itemDetails.itemType != ItemType.Furniture)
        {
            mouseX = mouseWorldPos.x - this.transform.position.x;
            //人物的碰撞体在脚底，Y加上0.85到人物中间，利于判断工具动画
            mouseY = mouseWorldPos.y - (this.transform.position.y + 0.85f);
            //排除斜方向，只执行4种方向的动画
            if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY))
            {
                mouseY = 0;
            }
            else
            {
                mouseX = 0;
            }
            StartCoroutine(useToolRoutine(mouseWorldPos, itemDetails));
        }
        //商品、种子、家具的逻辑
        else
        {
            EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        }
    }
    /// <summary>
    /// 使用工具的动画逻辑
    /// </summary>
    /// <param name="mouseWorldPos">鼠标世界坐标</param>
    /// <param name="itemDetails">物品详情</param>
    /// <returns></returns>
    private IEnumerator useToolRoutine(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        //useTool = true;
        inputDisable = true;
        yield return null;
        foreach (var animator in animators)
        {
            animator.SetTrigger("UseTool");
            //同步人物的面朝方向
            animator.SetFloat("InputX", mouseX);
            animator.SetFloat("InputY", mouseY);
        }
        //等0.45f是因为动画在0.45秒的时候锄头会碰到地上
        yield return  new WaitForSeconds(0.45f);
        EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        //等动画执行结束
        yield return new WaitForSeconds(0.25f);

        //useTool = false;
        inputDisable = false;
    }

    private void OnBeforeSceneUnloadEvent()
    {
        //加载场景前到加载结束不允许控制玩家
        inputDisable = true;
    }

    private void OnAfterSceneLoadedEvent()
    {
        //加载结束后可以控制玩家
        inputDisable = false;
    }

    private void OnMoveToPosition(Vector3 targetPosition)
    {
        //切换场景时移动Player去目标位置
        transform.position = targetPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (inputDisable == false)
        {
            PlayerInput();
        }
        else
        {
            //切场景时禁止动画
            isMoving = false;
        }
        
        SwitchAnimation();
    }
    void FixedUpdate()
    {
        if (inputDisable == false)
        {
            Movement();
        }
        
    }


    private void PlayerInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        //如果是斜着走路就限制速度
        if(inputX != 0 && inputY != 0)
        {
            inputX = inputX * 0.6f;
            inputY = inputY * 0.6f;
        }
        //按下Shift就减慢速度走路
        if (Input.GetKey(KeyCode.LeftShift))
        {
            inputX = inputX * 0.5f;
            inputY = inputY * 0.5f;
        }

        movementInput = new Vector2(inputX, inputY);

        //移动的时候isMoving = true
        isMoving = movementInput != Vector2.zero;
    }

    private void Movement()
    {
        //调用Rigidbody2d移动方法，带物理效果
        rb.MovePosition(rb.position + movementInput * speed * Time.deltaTime);
    }

    private void SwitchAnimation()
    {
        foreach (var animator in animators)
        {
            animator.SetBool("IsMoving",isMoving);
            animator.SetFloat("MouseX", mouseX);
            animator.SetFloat("MouseY", mouseY);

            if (isMoving)
            {
                animator.SetFloat("InputX", inputX);
                animator.SetFloat("InputY", inputY);
            }
        }
    }

    public GameSaveData GenerateSavaData()
    {
        GameSaveData saveData = new GameSaveData();
        //保存角色位置数据
        saveData.characterPosDic = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDic.Add(this.name, new SerializableVector3(transform.position));
        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        //拿到存的坐标赋值给玩家
        var targetPosition = saveData.characterPosDic[this.name].ToVector3();
        this.transform.position = targetPosition;
    }
}
