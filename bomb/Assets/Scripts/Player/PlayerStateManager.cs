using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
public class PlayerStateManager : NetworkBehaviour
{
    private enum PlayerState
    {
        Idle,
        Run,
        Jump,
        Stun,
        Dead,
        Cast,
        Drop,
        Push
    }

    private StateMachine stateMachine;
    // 상태를 저장할 딕셔너리 생성
    private Dictionary<PlayerState, IState> dicState = new Dictionary<PlayerState, IState>();
    private Animator VFXanim;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject curItemImagePrefab;
    private GameObject curItemImage;
    [SerializeField] private Text nickNameText;
    [SerializeField] private Image bombStateImage;
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private GameObject playerObject;
    [SerializeField] SpriteRenderer ghostSprite;
    [SerializeField] private List<Animator> ItemVFX = new List<Animator>();
    public Sprite LeaderBoardIcon;
    private List<SpriteRenderer> CustomObjects = new List<SpriteRenderer>();

    public SpriteRenderer spriteRenderer { set; get; }
    public Rigidbody2D rigid2d { set; get; }
    public Collider2D coll { set; get; }
    public GameObject curItemObj { set; get; }
    public PhysicsMaterial2D idlePhysicsMat;
    public PhysicsMaterial2D stunPhysicsMat;
    private SoundManager Smanager = SoundManager.Instance;
    private AudioSource SoundSource;

    // 폭탄 글로벌 타이머 (For Debugging)
    [SerializeField] private Text timer;

    [Header("Player Control Value")]

    [SerializeField] private float moveSpeed = 0f;
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float berserkMaxSpeed = 15f;
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float accelaration = 10f;
    [SerializeField] private float ghostSpeed = 10f;
    [SerializeField] private float power = 30f;
    [SerializeField] private float normalGravityScale = 1.7f;
    public float NormalGravityScale => normalGravityScale;
    [SerializeField] private float descendGravityScale = 1.7f  * .75f;

    public float MoveSpeed => moveSpeed;
    public float JumpForce => jumpForce;
    public float MaxSpeed => maxSpeed;
    public float BerserkMaxSpeed => berserkMaxSpeed;
    public float MinSpeed => minSpeed;
    public float Accelaration => accelaration;
    public float GhostSpeed => ghostSpeed;
    private float dashTime = 0f;
    public float dashVel { get; set; }
    private float curDashTime = 0f;
    private float lerpT = 0f;
    [SerializeField] private float jumpBufferTime = 0.05f;
    private float jumpBufferTimeCnt;
    [SerializeField] private float hangTime = 0.1f;
    private float hangTimeCnt;
    [SerializeField] private int curGhostSkillCount;
    [SerializeField] private float pushCoolDown = .5f;
    private float curPushCoolDown;
    [SerializeField] private float curGroundAngle;

    [Header("Player Current State Value")]

    [SyncVar(hook = nameof(OnChangeItem))][SerializeField] public Item curItem;
    [SyncVar(hook = nameof(OnChangePlayerLocalBombTime))]
    public float playerLocalBombTime;

    public bool IsGround => isGround;
    [SerializeField] private bool isGround = false;
    [SerializeField] private bool onGround = false;
    [SerializeField] private bool isWallJumpable;
    //[SerializeField] private int wallJumpCnt = 1;
    private int curWallJumpCnt;
    [SerializeField] public bool isWallAttached;

    private bool lastIsGround = true;
    [SyncVar] public bool isHeadingRight = false;
    [SyncVar] private bool isTransferable = true;
    [SyncVar] private bool isFlickering = false;
    [SyncVar(hook = nameof(OnChangeAisRunning))] public bool AisRunning = false;
    [SyncVar(hook = nameof(OnChangeAisStunned))] public bool AisStunned = false;
    [SyncVar(hook = nameof(OnChangeAisJumping))] public bool AisJumping = false;
    [SyncVar(hook = nameof(OnChangeAisTurning))] public bool AisTurning = false;
    [SyncVar(hook = nameof(OnChangeHasBomb))]
    public bool hasBomb = false;
    public bool isCasting { set; get; } = false;
    public bool IsTransferable => isTransferable;
    [SyncVar] public int roundScore;
    
    [SyncVar(hook = nameof(OnChangeNickName))]
    public string playerNickname;
    [SyncVar(hook = nameof(OnChangeBombState))] public int bombState;
    [SerializeField] private List<Sprite> bombSpriteList = new List<Sprite>();
    [SerializeField] private bool hasJumped = false;
    [SyncVar(hook = nameof(OnChangeCustomState))] public List<int> customState = new List<int>();

    [Header ("GhostSkill")]
    [SerializeField] private GameObject ghostSkillEffect;
    [SerializeField] private float ghostSkillCoolDown = 5f;
    [SerializeField] private float curGhostSkillCoolDown;
    [SerializeField] private float ghostSkillRadius = 3f;
    [SerializeField] private float ghostSkillDelay = 3f;
    [SerializeField] private float ghostSkillForce = 5f;
    [SyncVar] public bool isGhostSkllCasting = false;

    #region UnityEventFunc

    private void Awake()
    {
        SoundSource = GetComponent<AudioSource>();
        Smanager.AddAudioSource(SoundSource);
    }

    // Initialize states
    private void Start()
    {
        CustomObjects.Add(playerObject.transform.Find("Head").GetChild(0).GetComponent<SpriteRenderer>());
        CustomObjects.Add(playerObject.transform.Find("Body").GetChild(0).GetComponent<SpriteRenderer>());
        
        if(isLocalPlayer) 
        {
            CmdSetNickName(PlayerSetting.playerNickname);
            CmdSetCustomState(PlayerSetting.customState);
        }
        else
        {
            SpriteRenderer[] rend = playerObject.GetComponentsInChildren<SpriteRenderer>();
            foreach(var render in rend)
            {
                render.color = new Color(1f, 0f, 0f, 1f);
            }
            ApplyCustom(customState);
        }
        
        // 게임 매니저에 해당 플레이어 추가
        GameManager.Instance.AddPlayer(this);

        IState idle = new PlayerIdle(this);
        IState run = new PlayerRun(this);
        IState jump = new PlayerJump(this);
        IState stun = new PlayerStun(this);
        IState dead = new PlayerDead(this);
        IState cast = new PlayerCast(this);
        IState drop = new PlayerDrop(this);
        IState push = new PlayerPush(this);

        dicState.Add(PlayerState.Idle, idle);
        dicState.Add(PlayerState.Run, run);
        dicState.Add(PlayerState.Jump, jump);
        dicState.Add(PlayerState.Stun, stun);
        dicState.Add(PlayerState.Dead, dead);
        dicState.Add(PlayerState.Cast, cast);
        dicState.Add(PlayerState.Drop, drop);
        dicState.Add(PlayerState.Push, push);

        // 시작 상태를 Idle로 설정
        stateMachine = new StateMachine(dicState[PlayerState.Idle]);

        spriteRenderer = GetComponent<SpriteRenderer>();
        rigid2d = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        VFXanim = explosionVFX.GetComponent<Animator>();
        curGhostSkillCount = GameRuleStore.Instance.CurGameRule.ghostSkillCount;
        curGhostSkillCoolDown = ghostSkillCoolDown;
        
        curPushCoolDown = pushCoolDown;

        curItemImage = Instantiate(curItemImagePrefab, Vector3.zero, Quaternion.identity);
        curItemImage.GetComponent<ItemImage>().AddPlayer(this);

        curItemImage.SetActive(false);

        Smanager.PlayBGM(AudioType.GameSceneBGM);

    }

    // 키보드 입력 받기 및 State 갱신
    private void Update()
    {
        // 로컬 플레이어가 아닐 경우 작동 X
        if (!isLocalPlayer) return;
        //게임매니저의 이동가능 플래그가 true일때만 이동 가능
        if(!GameManager.Instance.isPlayerMovable) return;
        KeyboardInput();
        stateMachine.DoOperateUpdate();
        // dash 속도 감소
        if (isCasting)
        {
            rigid2d.velocity = new Vector2((isHeadingRight ? 1 : -1) * Mathf.Lerp(0, dashVel, lerpT), rigid2d.velocity.y);
            curDashTime -= Time.deltaTime;
            lerpT = curDashTime / dashTime;
        }
    }

    private void FixedUpdate()
    {
        RaycastHit2D raycastHitLeft = Physics2D.Raycast(coll.bounds.center + new Vector3(coll.bounds.extents.x - .1f, -coll.bounds.extents.y, 0), Vector2.down, .1f, LayerMask.GetMask("Ground"));
        RaycastHit2D raycastHitMid = Physics2D.Raycast(coll.bounds.center + new Vector3(0, -coll.bounds.extents.y, 0), Vector2.down, .04f, LayerMask.GetMask("Ground"));
        RaycastHit2D raycastHitRight = Physics2D.Raycast(coll.bounds.center + new Vector3(-coll.bounds.extents.x + .1f, -coll.bounds.extents.y, 0), Vector2.down, .1f, LayerMask.GetMask("Ground"));
        if((raycastHitLeft.collider != null && raycastHitMid.collider != null) || (raycastHitMid.collider != null && raycastHitRight.collider != null))
        {
            if(onGround)
            {
                //if(rigid2d.velocity.y < .2f)
                isGround = true;
                isWallJumpable = true;
            }
        }
        else
        {
            isGround = false;
        }

        if(raycastHitMid.collider != null)
        {
            RaycastHit2D raycastHitRef = Physics2D.Raycast(coll.bounds.center + new Vector3(.01f, -coll.bounds.extents.y, 0), Vector2.down, .04f, LayerMask.GetMask("Ground"));
            if(raycastHitRef.collider != null)
            {
                if(raycastHitMid.distance == raycastHitRef.distance)
                {
                    curGroundAngle = 0;
                } 
                else
                {
                    float Ydiff = Mathf.Abs(raycastHitMid.distance - raycastHitRef.distance);
                    curGroundAngle = Mathf.Atan2(.01f, Ydiff);
                }
            }
        }

        RaycastHit2D raycastHitWall = Physics2D.Raycast(coll.bounds.center + new Vector3(coll.bounds.extents.x * (isHeadingRight ? 1 : -1), -coll.bounds.extents.y/2,0), Vector2.right * (isHeadingRight ? 1 : -1), .04f, LayerMask.GetMask("Ground"));
        if(raycastHitWall.collider != null)
        {
            isWallAttached = true;
        }
        else
        {
            isWallAttached = false;
        }

        if(hasJumped && rigid2d.velocity.y < 0f)
        {
            CmdSetATriggerJump();
            hasJumped = false;
            rigid2d.gravityScale = descendGravityScale;
        }

        if(lastIsGround)
        {

            Vector2 vec = Vector2.zero;
            if(Input.GetAxisRaw("Horizontal") == 0)
            {
                rigid2d.velocity = Vector2.SmoothDamp(rigid2d.velocity, Vector2.zero, ref vec, .1f);
            }
            else
            {
                //마찰 적용
                float XFriction = 0;
                float YFriction = 0;
                float value = .1f * 9.81f * rigid2d.gravityScale;
                if(curGroundAngle == 0)
                {
                    XFriction = value * Mathf.Cos(curGroundAngle) * Mathf.Cos(curGroundAngle) * (rigid2d.velocity.x > 0 ? -1 : 1);
                }
                else
                {
                    YFriction = -value * Mathf.Cos(curGroundAngle) * Mathf.Sin(curGroundAngle);
                    XFriction = value * rigid2d.gravityScale * Mathf.Cos(curGroundAngle) * Mathf.Cos(curGroundAngle) * (curGroundAngle > 0 ? -1 : 1);    
                } 
                //Debug.Log($"curGround Angle : {curGroundAngle} XFriction : {XFriction} YFriction : {YFriction}");
                rigid2d.AddForce(new Vector2(XFriction, YFriction), ForceMode2D.Force);
            }
        }
        lastIsGround = isGround;

        playerObject.transform.localScale = new Vector3((isHeadingRight ? -1 : 1), 1, 1);
        ItemVFX[0].transform.localScale = new Vector3((!isHeadingRight ? -1 : 1), 1, 1);
        ItemVFX[0].transform.localPosition = new Vector3(0.44f * (isHeadingRight ? -1 : 1), 0, 0);
        ghostSprite.flipX = isHeadingRight;
        coll.offset = new Vector2(isHeadingRight ? 0.03f : -0.03f,0); 
    }

    public void OnDrawGizmos()
    {
        // Gizmos.DrawRay(coll.bounds.center + new Vector3(coll.bounds.extents.x - .1f, -coll.bounds.extents.y, 0), Vector2.down * .1f);
        // Gizmos.DrawRay(coll.bounds.center + new Vector3(0, -coll.bounds.extents.y, 0), Vector2.down * .04f);
        // Gizmos.DrawRay(coll.bounds.center + new Vector3(-coll.bounds.extents.x + .1f, -coll.bounds.extents.y, 0), Vector2.down * .1f);
        // Gizmos.DrawRay(coll.bounds.center + new Vector3(coll.bounds.extents.x * (isHeadingRight ? 1 : -1), -coll.bounds.extents.y/2, 0), Vector2.right * (isHeadingRight ? 1 : -1) * .08f);
    }

    // 다른 플레이어 충돌
    private void OnCollisionEnter2D(Collision2D other)
    {
        if(!hasAuthority) return;

        //폭탄을 가지고 충돌하는 경우
        if (other.transform.CompareTag("Player") && hasBomb && isTransferable)
        {
            var targetPSM = other.transform.GetComponent<PlayerStateManager>();
            if (targetPSM.hasBomb == false)
            {   
                GameManager.Instance.UI_Play.CmdAddLogTransition(this, targetPSM);
                StartCoroutine(_TransitionDone());
                float time = 3*((float)GameManager.Instance.bombGlobalTimeLeft / GameManager.Instance.bombGlobalTime);
                StartCoroutine(Stunned(Mathf.Max(time, .25f)));
                Vector2 dir = (Vector3.Scale(transform.position - other.transform.position,new Vector3(2,1,1))).normalized * power;
                if(dir.y == 0) dir.y = 0.1f * power;
                rigid2d.velocity = dir;
                CmdBombTransition(targetPSM.netId, dir * (-1));
            }
        }
        else if(stateMachine.CurruentState != dicState[PlayerState.Stun] && other.transform.CompareTag("Player") && transform.position.y + coll.bounds.size.y * .75f < other.transform.position.y)
        {
            CmdAddForce(new Vector2(other.transform.GetComponent<Rigidbody2D>().velocity.x,jumpForce), other.transform.GetComponent<PlayerStateManager>());
            CmdSetStun(1f);
        }

        if(other.transform.CompareTag("Ground"))
        {
            for(int i=0; i<other.contactCount; i++)
            {
                if(other.GetContact(i).point.y < transform.position.y)
                {
                    onGround = true;
                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other) 
    {
        if(!hasAuthority) return;

        if(other.transform.CompareTag("Ground"))
        {
            onGround = false;
        }    
    }

    // 다른 아이템 충돌
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!hasAuthority) return;
        // 아이템인 경우, 현재 아이템을 가지고 있지 않은 상태여야 한다
        if (other.transform.CompareTag("Item") && curItem == null)
        {
            Item _item = other.GetComponent<Item>();
            _item.player = this;
            curItemObj = _item.itemObj;
            CmdAddItem(_item);
        }
        // Stone에 맞았을 때
        if (other.transform.CompareTag("Projectile") && other.GetComponent<StoneProjectile>().player != this)
        {
            Vector2 dir = (transform.position - other.transform.position).normalized * other.GetComponent<StoneProjectile>().force;
            CmdHitStone(other.GetComponent<StoneProjectile>().StunTime, dir, this);
            CmdDestroy(other.GetComponent<StoneProjectile>().netId);
        }
        // 서버에 로그 전송
    }

    [Command]
    public void CmdDestroy(uint netId)
    {
        NetworkServer.Destroy(NetworkServer.spawned[netId].gameObject);
    }

    #endregion UnityEventFunc

    // 키보드 입력 제어
    private void KeyboardInput()
    {
        // Stun, Dead상태가 아니거나 돌진 중이 아닐 때 행동 가능
        if (stateMachine.CurruentState != dicState[PlayerState.Stun] && stateMachine.CurruentState != dicState[PlayerState.Dead] && !isCasting)
        {
            // Run State
            if(!hasJumped && isGround)
            {
                if (Input.GetAxisRaw("Horizontal") != 0)
                {
                    stateMachine.SetState(dicState[PlayerState.Run]);
                }
                else
                {
                    stateMachine.SetState(dicState[PlayerState.Idle]);
                }
            }
            
            // Jump State
            if(isGround || (isWallJumpable&&isWallAttached)) 
            {
                hangTimeCnt = hangTime;
            }
            else
            {
                hangTimeCnt -= Time.deltaTime;
            }
            
            if(Input.GetKeyDown(PlayerSetting.keyList[(int)PlayerSetting.BindKeys.Jump]))
            {
                jumpBufferTimeCnt = jumpBufferTime;
            }
            else
            {
                jumpBufferTimeCnt -= Time.deltaTime;
            }

            if (jumpBufferTimeCnt > 0f && hangTimeCnt > 0f)
            {
                rigid2d.velocity = new Vector2(rigid2d.velocity.x, jumpForce);
                rigid2d.gravityScale = normalGravityScale;
                hasJumped = true;
                stateMachine.SetState(dicState[PlayerState.Jump]);
                isWallJumpable = false;
                jumpBufferTimeCnt = 0f;
            }   

            // Cast State
            if (Input.GetKeyDown(PlayerSetting.keyList[(int)PlayerSetting.BindKeys.Cast]) && stateMachine.CurruentState != dicState[PlayerState.Cast])
            {
                stateMachine.SetState(dicState[PlayerState.Cast]);
            }

            // Drop State
            if(Input.GetKeyDown(PlayerSetting.keyList[(int)PlayerSetting.BindKeys.Drop]) && !isGround && stateMachine.CurruentState != dicState[PlayerState.Drop])
            {
                stateMachine.SetState(dicState[PlayerState.Drop]);
                StartCoroutine(DropRoutine());
            }

            // Push State
            if(Input.GetKeyDown(PlayerSetting.keyList[(int)PlayerSetting.BindKeys.Push]) && curPushCoolDown > pushCoolDown)
            {
                curPushCoolDown = 0f;
                stateMachine.SetState(dicState[PlayerState.Push]);
                Push();
            }
            curPushCoolDown += Time.deltaTime;
        }

        if (stateMachine.CurruentState == dicState[PlayerState.Dead])
        {
            if(Input.GetKeyDown(PlayerSetting.keyList[(int)PlayerSetting.BindKeys.Cast]))
            {
                if(curGhostSkillCount > 0 && curGhostSkillCoolDown > ghostSkillCoolDown)
                {
                    curGhostSkillCoolDown = 0f;
                    curGhostSkillCount--;
                    CmdGhostSkill();
                    StartCoroutine(GhostSkillRoutine());
                }
            }
            curGhostSkillCoolDown += Time.deltaTime;
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.UI_Play.ActivateESC();
        }
    }

    // 아이템 사용
    public void UseItem()
    {
        // 아이템이 없으면 작동 안함
        if (curItem != null)
        {
            curItem.OnUse();
            CmdSetItem();
        }
    }

    public void GetBomb(Vector2 dir)
    {
        StartCoroutine(_TransitionDone());
        RpcAddDirVec(dir);
        RpcStunSync(Mathf.Max(3*((float)GameManager.Instance.bombGlobalTimeLeft / GameManager.Instance.bombGlobalTime), .25f));
    }

    //Dash후 감속
    public void DashDone(float time)
    {
        dashTime = time;
        curDashTime = time;
        lerpT = 1f;
        StartCoroutine(_DashDone());
    }

    public void DiscardItem()
    {
        if(curItem != null)
        {
            curItem.DiscardItem();
            curItem = null;
        }
    }

    public void CheckBombState()
    {
        if(!hasBomb)
        {
            bombState = 0;
            return;
        }
        float bombGlobalTime = GameManager.Instance.bombGlobalTime;
        float bombGlobalTimeLeft = GameManager.Instance.bombGlobalTimeLeft;
        if(Mathf.Min(bombGlobalTimeLeft, playerLocalBombTime) < 2f)
        {
            bombState = 3;
        }
        else if(playerLocalBombTime > Mathf.Round(bombGlobalTime/5)/2 && bombGlobalTimeLeft > bombGlobalTime/2)
        {
            bombState = 1;
        }
        else
        {
            bombState = 2;
        }
    }

    public void Push()
    {
        RaycastHit2D[] check = Physics2D.BoxCastAll(coll.bounds.center, coll.bounds.size, 0f, Vector2.right * (isHeadingRight ? 1 : -1), coll.bounds.size.x * 2, LayerMask.GetMask("Player"));

        foreach(var player in check)
        {
            if(player.collider != null && player.transform != this.transform)
            {
                PlayerStateManager target = player.transform.GetComponent<PlayerStateManager>();
                CmdAddForce(new Vector2((isHeadingRight ? .5f : -.5f), .5f) * 5f, target);
                CmdApplyStun(target, .25f); 
            }
        }
    }

    public void ApplyCustom(List<int> customState)
    {
        for(int i=0; i<customState.Count; i++)
        {
            if(customState[i] < CustomManager.Instance.listDic[i].Count)
            {
                CustomObjects[i].sprite = CustomManager.Instance.listDic[i][customState[i]];
                Debug.Log(CustomObjects[i].sprite.name + ", " + playerNickname);
            }
        }
    }

    #region IEnumerators

    //감속 종료 후 gravityScale 정상화, Casting 종료
    private IEnumerator _DashDone()
    {
        yield return new WaitForSeconds(dashTime);
        isCasting = false;
        rigid2d.gravityScale = normalGravityScale;
    }

    private IEnumerator Stunned(float stunTime)
    {
        stateMachine.SetState(dicState[PlayerState.Stun]);
        yield return new WaitForSeconds(stunTime);
        stateMachine.SetState(dicState[PlayerState.Run]);
    }

    // 폭탄 전달시 약간의 딜레이 부여
    private IEnumerator _TransitionDone()
    {
        isTransferable = false;
        yield return new WaitForSeconds(0.1f);
        isTransferable = true;
    }
    
    private IEnumerator TimeDescend()
    {
        if(!hasAuthority) yield break;
        while(hasBomb)
        {
            if(playerLocalBombTime <= 0f && hasBomb)
            {
                CmdPlayAudio(AudioType.Explosion);
                CmdPlayerDead();
                yield break;
            }
            CmdLocalTimeReduced(Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator BombFlickering()
    {
        while(isFlickering)
        {
            bombStateImage.sprite = bombSpriteList[3];
            yield return new WaitForSeconds(0.1f);
            if(!isFlickering) yield break;
            bombStateImage.sprite = bombSpriteList[4];
            yield return new WaitForSeconds(0.1f);
            if(!isFlickering) yield break;
        }
    }

    private IEnumerator GhostSkillRoutine()
    {
        isGhostSkllCasting = true;
        yield return StartCoroutine(GhostSkillEffect());
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, ghostSkillRadius, LayerMask.GetMask("Player"));
        foreach(var target in targets)
        {
            if(target == this.coll) continue;
            CmdAddForce((target.transform.position - transform.position).normalized * ghostSkillForce, target.GetComponent<PlayerStateManager>());
            //Debug.Log(target.GetComponent<PlayerStateManager>().playerNickname);
        }
        isGhostSkllCasting = false;
    }

    [Command]
    public void CmdAddForce(Vector2 dir, PlayerStateManager target)
    {
        target.RpcAddDirVec(dir);
    }

    private IEnumerator GhostSkillEffect()
    {
        ghostSkillEffect.SetActive(true);
        float curTime = 0f;
        while(curTime < ghostSkillDelay)
        {
            ghostSkillEffect.transform.localScale = Vector3.one * (curTime / ghostSkillDelay) * ghostSkillRadius;
            yield return null;
            curTime += Time.deltaTime;
        }
        ghostSkillEffect.SetActive(false);
    }

    private IEnumerator DropRoutine()
    {
        rigid2d.gravityScale = 0f;
        rigid2d.velocity = Vector2.zero;
        yield return new WaitForSeconds(.2f);
        rigid2d.gravityScale = normalGravityScale;
        rigid2d.velocity = Vector2.down * 15f;
    }

    #endregion IEnumerators

    #region CommandFunc

    [Command]
    private void CmdLocalTimeReduced(float time)
    {
        if(GameManager.Instance.isBombDecreasable)
        {
            playerLocalBombTime -= time;
            GameManager.Instance.bombGlobalTimeLeft -= Time.deltaTime;
            CheckBombState();
        }
    }

    [Command]
    private void CmdSetTimer(float time)
    {
        RpcSetTimer((int)time);
    }

    // 아이템 획득 상태 동기화
    [Command]
    private void CmdAddItem(Item item)
    {
        curItem = item;
        curItemObj = item.itemObj;
        item.player = this;
        item.spawner.isSpawnable = true;
        item.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
        RpcItemSync(item.netId);
    }

    [Command]
    public void CmdHitStone(float time, Vector2 dir, PlayerStateManager target)
    {
        target.RpcAddDirVec(dir);
        RpcStunSync(time);
    }

    [Command]
    public void CmdBombTransition(uint targetNetId, Vector3 dir)
    {
        PlayerStateManager target = null;
        target = NetworkServer.spawned[targetNetId].GetComponent<PlayerStateManager>();
        if (target != null)
        {
            target.playerLocalBombTime = Mathf.Max(2f, target.playerLocalBombTime);
            target.GetBomb(dir);
        }
        hasBomb = !hasBomb;
        target.hasBomb = !target.hasBomb;
    }

    [Command]
    public void CmdIsHeadingSync(bool isHeading)
    {
        isHeadingRight = isHeading;
    }

    [Command]
    private void CmdPlayerDead()
    {
        hasBomb = false;
        isFlickering = false;
        RpcDead();
        GameManager.Instance.bombExplode(this);
    }

    [Command]
    public void CmdSetNickName(string nick)
    {
        playerNickname = nick;
    }
    
    [Command]
    public void CmdSetItem()
    {
        RpcSetItem();
    }

    [Command(requiresAuthority = false)]
    public void CmdSetBombStete(int value)
    {
        bombState = value;
    }

    [Command]
    public void CmdSetGhostSprite(bool value)
    {
        RpcSetGhostSprite(value);
    }

    [Command]
    public void CmdSetItemAnim(int idx)
    {
        RpcSetItemAnim(idx);
    }

    [Command]
    public void CmdSetAisRunning(bool value)
    {
        AisRunning = value;
    }

    [Command]
    public void CmdSetAisStunned(bool value)
    {
        AisStunned = value;
    }

    [Command]
    public void CmdSetAisJumping(bool value)
    {
        AisJumping = value;
    }

    [Command]
    public void CmdSetATriggerJump()
    {
        RpcSetTriggerJump();
    }
    
    [Command]
    public void CmdSetAisTurning(bool value)
    {
        AisTurning = value;
    }

    [Command]
    public void CmdGhostSkill()
    {
        RpcGhostSKillRoutine();
    }

    [Command]
    public void CmdSetIsGhostSkillCasting(bool value)
    {
        isGhostSkllCasting = value;
    }

    [Command]
    public void CmdPlayAudio(AudioType type)
    {
        RpcPlayAudio(type, Smanager.SourceIdx(SoundSource));
    }

    [ClientRpc]
    public void RpcPlayAudio(AudioType type, int idx)
    {
        Smanager.PlayAudio(type, idx);
    }

    [Command]
    public void CmdApplyStun(PlayerStateManager target, float time)
    {   
        target.RpcStunSync(time);
    }

    [Command]
    public void CmdSetStun(float time)
    {
        RpcStunSync(time);
    }

    [Command]
    public void CmdSetCustomState(int[] _customState)
    {
        List<int> temp = new List<int>(_customState);
        customState = temp;
    }

    #endregion CommandFunc
    
    #region ClientRpcFunc

    [ClientRpc]
    public void RpcAddDirVec(Vector2 dir)
    {
        if(hasAuthority)
        {
            //Debug.Log("Hitted__" + playerNickname);
            this.rigid2d.velocity = dir;
        }
    }

    [ClientRpc]
    public void RpcStunSync(float time)
    {
        if (hasAuthority)
        {
            //Debug.Log("Stunned__" + playerNickname);
            this.StartCoroutine(Stunned(time));
        }
    }

    // 폭탄 폭발
    [ClientRpc]
    public void RpcDead()
    {
        VFXanim.SetTrigger("Explode");
        if (hasAuthority)
        {
            stateMachine.SetState(dicState[PlayerState.Dead]);
        }
        else
        {
            // 죽으면 모습 안보이게 (임시)
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            this.gameObject.layer = LayerMask.NameToLayer("GhostPlayer");
        }
    }

    //획득된 아이템의 collider와 renderer의 비활성화 상태 동기화
    [ClientRpc]
    public void RpcItemSync(uint netId)
    {
        GameObject obj = NetworkClient.spawned[netId].gameObject;
        obj.GetComponent<Collider2D>().enabled = false;
        obj.GetComponent<SpriteRenderer>().enabled = false;
    }

    //Destroy되는 아이템의 상태 동기화
    [ClientRpc]
    public void RpcItemDestroy(uint netId)
    {
        Destroy(NetworkClient.spawned[netId].gameObject);
    }

    [ClientRpc]
    public void RpcTeleport(Vector3 position)
    {
        transform.position = position;
    }

    [ClientRpc]
    public void RpcSetTimer(float time)
    {
        if(hasBomb)
        {
            timer.text = time.ToString();
        }
        else
        {
            timer.text = "";
        }
    }

    [ClientRpc]
    public void RpcPlayerRoundReset(){
        rigid2d.velocity = Vector2.zero;
        stateMachine.SetState(dicState[PlayerState.Idle]);
        curGhostSkillCount = GameRuleStore.Instance.CurGameRule.ghostSkillCount;
        curGhostSkillCoolDown = ghostSkillCoolDown;
        spriteRenderer.color = new Color(1f,1f,1f,1f);
        this.gameObject.layer = LayerMask.NameToLayer("Player");
    }

    [ClientRpc]
    public void RpcSetItem()
    {
        curItem = null;
        curItemImage.SetActive(false);
    }

    [ClientRpc]
    public void RpcSetGhostSprite(bool value)
    {
        ghostSprite.enabled = value;
        playerObject.SetActive(!value);
    }

    [ClientRpc]
    public void RpcSetItemAnim(int idx)
    {
        if(idx == 1) return;
        ItemVFX[idx].SetTrigger("Trigger");
    }

    [ClientRpc]
    public void RpcSetTriggerJump()
    {
        anim.SetTrigger("TriggerJump");
    }

    [ClientRpc]
    public void RpcGhostSKillRoutine()
    {
        StartCoroutine(GhostSkillEffect());
    }

    #endregion ClientRpcFunc

    #region SyncVarHookFunc

    //hasBomb hook함수
    void OnChangeHasBomb(bool oldBool, bool newbool)
    {
        
        if(!hasAuthority) return;
        if(newbool)
        {
            StartCoroutine(TimeDescend());
        }
        else
        {
            CmdSetBombStete(0);
            //CmdSetTimer(0);
        }
    }

    //playerLocalbombTime hook함수
    void OnChangePlayerLocalBombTime(float oldfloat, float newfloat)
    {
        if(hasAuthority)
        {
            //CmdSetTimer(newfloat);
        } 
    }

    public void OnChangeNickName(string _, string value)
    {
        this.playerNickname = value;
        nickNameText.text = value;
    }

    public void OnChangeItem(Item _, Item value)
    {
        curItemImage.SetActive(false);
        if(value != null)
        {
            curItemImage.SetActive(true);
            curItemImage.transform.position = transform.position + new Vector3(.5f, .2f, 0f);
            curItemImage.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.itemSprites[(int)value.Type];
        } 
    }

    public void OnChangeBombState(int _, int value)
    {
        isFlickering = false;
        if(value == 3)
        {
            isFlickering = true;
            StartCoroutine(BombFlickering());
        }
        else
        {
            bombStateImage.sprite = bombSpriteList[value];
        } 
    }

    public void OnChangeAisRunning(bool _, bool value)
    {
        anim.SetBool("isRunning", value);
    }

    public void OnChangeAisStunned(bool _, bool value)
    {
        anim.SetBool("isStunned", value);
    }

    public void OnChangeAisJumping(bool _, bool value)
    {
        anim.SetBool("isJumping", value);
        anim.ResetTrigger("TriggerJump");
    }

    public void OnChangeAisTurning(bool _, bool value)
    {
        anim.SetBool("isTurning", value);
    }

    public void OnChangeCustomState(List<int> _, List<int> value)
    {
        ApplyCustom(value);
    }

    #endregion SyncVarHookFunc

}