using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayer : NetworkRoomPlayer
{

    private enum RPlayerState
    {
        Idle,
        Run,
        Jump,
        Stun,
        Drop,
        Push
    }

    private StateMachine stateMachine;
    // 상태를 저장할 딕셔너리 생성
    private Dictionary<RPlayerState, IState> dicState = new Dictionary<RPlayerState, IState>();

    [SerializeField] private Animator anim;
    [SerializeField] private GameObject playerObject;
    private List<SpriteRenderer> CustomObjects = new List<SpriteRenderer>();

    public Rigidbody2D rigid2d { set; get; }
    public Collider2D coll { set; get; }
    public GameObject curItemObj { set; get; }
    public PhysicsMaterial2D idlePhysicsMat;
    public PhysicsMaterial2D stunPhysicsMat;
    private SoundManager Smanager = SoundManager.Instance;
    private AudioSource SoundSource;

    [Header("Player Control Value")]

    [SerializeField] private float moveSpeed = 0f;
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float berserkMaxSpeed = 15f;
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float accelaration = 10f;
    [SerializeField] private float ghostSpeed = 10f;
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

    [SerializeField] private float jumpBufferTime = 0.05f;
    private float jumpBufferTimeCnt;
    [SerializeField] private float hangTime = 0.1f;
    private float hangTimeCnt;
    [SerializeField] private int curGhostSkillCount;
    [SerializeField] private float pushCoolDown = .5f;
    private float curPushCoolDown;
    [SerializeField] private float curGroundAngle;

    [Header("Player Current State Value")]
    [SerializeField] private bool isGround = false;
    public bool IsGround => isGround;
    [SerializeField] private bool onGround = false;
    [SerializeField] private bool isWallJumpable;
    //[SerializeField] private int wallJumpCnt = 1;
    private int curWallJumpCnt;
    [SerializeField] public bool isWallAttached;

    private bool lastIsGround = true;
    [SyncVar(hook = nameof(OnChangeHeading))] public bool isHeadingRight = false;
    public bool isCasting { set; get; } = false;

    [SyncVar(hook = nameof(OnSetNickName))] public string playerNickname = string.Empty;
    [SerializeField] private bool hasJumped = false;

    [SyncVar(hook = nameof(OnChangeAisRunning))] public bool AisRunning = false;
    [SyncVar(hook = nameof(OnChangeAisStunned))] public bool AisStunned = false;
    [SyncVar(hook = nameof(OnChangeAisJumping))] public bool AisJumping = false;
    [SyncVar(hook = nameof(OnChangeAisTurning))] public bool AisTurning = false;

    public bool isReady = false;
    UI_Lobby UI_Lobby;
    [SerializeField] Text nameText;
    public static RoomPlayer MyPlayer;
    RoomManager manager = NetworkManager.singleton as RoomManager;

    [SerializeField]private bool inCustom = false;
    [SerializeField]private bool inSetting = false;

    private Customization _Customization;
    private GameRuleSetter _GameRuleSetter;
    private SpriteRenderer[] rends;
    [SyncVar] public int PrefabIndex;
    private Customization custom;

    public override void OnStartClient()
    {   
        if(isLocalPlayer) 
        {
            UI_Lobby = (UI_Lobby)FindObjectOfType(typeof(UI_Lobby));
            UI_Lobby.player = this;
            CmdSetNickName(PlayerSetting.playerNickname);
            MyPlayer = this;

            Smanager.AddAudioSource(GetComponent<AudioSource>());
            Smanager.PlayBGM(AudioType.LobbyBGM);

            custom = FindObjectOfType<Customization>();
            custom.localPlayer = this;
            custom.UpdatePreviewModel();
        }

        IState idle = new RPlayerIdle(this);
        IState run = new RPlayerRun(this);
        IState jump = new RPlayerJump(this);
        IState stun = new RPlayerStun(this);
        IState drop = new RPlayerDrop(this);
        IState push = new RPlayerPush(this);

        dicState.Add(RPlayerState.Idle, idle);
        dicState.Add(RPlayerState.Run, run);
        dicState.Add(RPlayerState.Jump, jump);
        dicState.Add(RPlayerState.Stun, stun);
        dicState.Add(RPlayerState.Drop, drop);
        dicState.Add(RPlayerState.Push, push);

        stateMachine = new StateMachine(dicState[RPlayerState.Idle]);

        rends = GetComponentsInChildren<SpriteRenderer>();
        rigid2d = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();

        _Customization = FindObjectOfType<Customization>();
        _GameRuleSetter = FindObjectOfType<GameRuleSetter>();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        KeyboardInput();
        stateMachine.DoOperateUpdate();
    }

    private void FixedUpdate()
    {
        if(!hasAuthority) return;
        
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

        coll.offset = new Vector2(isHeadingRight ? 0.03f : -0.03f,0); 
    }

    private void OnDestroy() 
    {
        SoundManager.Instance.RemoveAudioSource(GetComponent<AudioSource>());    
    }

    // 키보드 입력 제어
    private void KeyboardInput()
    {
        // Stun, Dead상태가 아니거나 돌진 중이 아닐 때 행동 가능
        if (stateMachine.CurruentState != dicState[RPlayerState.Stun] && !isCasting)
        {
            // Run State
            if(!hasJumped && isGround)
            {
                if (Input.GetAxisRaw("Horizontal") != 0)
                {
                    stateMachine.SetState(dicState[RPlayerState.Run]);
                }
                else
                {
                    stateMachine.SetState(dicState[RPlayerState.Idle]);
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

            if (jumpBufferTimeCnt > 0f && hangTimeCnt > 0f && !hasJumped)
            {
                rigid2d.velocity = new Vector2(rigid2d.velocity.x, jumpForce);
                rigid2d.gravityScale = normalGravityScale;
                hasJumped = true;
                stateMachine.SetState(dicState[RPlayerState.Jump]);
                isWallJumpable = false;
                jumpBufferTimeCnt = 0f;
            }

            if(Input.GetKeyDown(PlayerSetting.keyList[(int)PlayerSetting.BindKeys.Cast]) && inCustom && !_GameRuleSetter.isActive)
            {
                _Customization.EnterCustomization();
            }

            if(Input.GetKeyDown(PlayerSetting.keyList[(int)PlayerSetting.BindKeys.Cast]) && inSetting && !_Customization.isActive)
            {
                _GameRuleSetter.EnterRuleSetting();
            }

            // Drop State
            if(Input.GetKeyDown(PlayerSetting.keyList[(int)PlayerSetting.BindKeys.Drop]) && !isGround && stateMachine.CurruentState != dicState[RPlayerState.Drop])
            {
                stateMachine.SetState(dicState[RPlayerState.Drop]);
                StartCoroutine(DropRoutine());
            }

            // Push State
            if(Input.GetKeyDown(PlayerSetting.keyList[(int)PlayerSetting.BindKeys.Push]) && curPushCoolDown > pushCoolDown)
            {
                curPushCoolDown = 0f;
                stateMachine.SetState(dicState[RPlayerState.Push]);
                Push();
            }
            curPushCoolDown += Time.deltaTime;
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            UI_Lobby.ActivateESC();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!hasAuthority) return;

        if(other.CompareTag("ReadyZone") && manager.roomSlots.Count != 1)
        {
            CmdChangeReadyState(true);
        }

        if(other.transform.GetComponent<GameRuleSetter>() != null)
        {
            inSetting = true;
            other.transform.GetComponent<SpriteRenderer>().enabled = true;
        }
        
        if(other.transform.GetComponent<Customization>() != null)
        {
            inCustom = true;
            other.transform.GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(!hasAuthority) return;

        if(other.CompareTag("ReadyZone"))
        {
            CmdChangeReadyState(false);
        }
    
        if(other.transform.GetComponent<GameRuleSetter>() != null)
        {
            inSetting = false;
            other.transform.GetComponent<SpriteRenderer>().enabled = false;
        }
        
        if(other.transform.GetComponent<Customization>() != null)
        {
            inCustom = false;
            other.transform.GetComponent<SpriteRenderer>().enabled = false;
        }

    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if(!hasAuthority) return;
        
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

    public void Push()
    {
        RaycastHit2D[] check = Physics2D.BoxCastAll(coll.bounds.center, coll.bounds.size, 0f, Vector2.right * (isHeadingRight ? 1 : -1), coll.bounds.size.x * 2, LayerMask.GetMask("Player"));

        foreach(var player in check)
        {
            if(player.collider != null && player.transform != this.transform)
            {
                RoomPlayer target = player.transform.GetComponent<RoomPlayer>();
                CmdAddForce(new Vector2((isHeadingRight ? .5f : -.5f), .5f) * 5f, target);
                CmdApplyStun(target, .25f); 
            }
        }
    }

    [Command]
    public void CmdAddForce(Vector2 dir, RoomPlayer target)
    {
        target.RpcAddDirVec(dir);
    }

    [ClientRpc]
    public void RpcAddDirVec(Vector2 dir)
    {
        if(hasAuthority)
        {
            //Debug.Log("Hitted__" + playerNickname);
            this.rigid2d.velocity = dir;
        }
    }

    [Command]
    public void CmdApplyStun(RoomPlayer target, float time)
    {   
        target.RpcStunSync(time);
    }

    [Command]
    public void CmdSetStun(float time)
    {
        RpcStunSync(time);
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

    [Command]
    public void CmdSetNickName(string nick)
    {
        playerNickname = nick;
    }

    [Command]
    public void CmdIsHeadingSync(bool value)
    {
        isHeadingRight = value;
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

    [ClientRpc]
    public void RpcSetTriggerJump()
    {
        anim.SetTrigger("TriggerJump");
    }

    [Command]
    public void CmdSetRunAnimMultiplier()
    {
        RpcSetRunAnimMultiplier();
    }

    [Command]
    public void CmdSetRunAnimMultiplier(float value)
    {
        RpcSetRunAnimMultiplier(value);
    }

    [ClientRpc]
    public void RpcSetRunAnimMultiplier()
    {
        float multiplier = Mathf.Abs(rigid2d.velocity.x) * 0.2f + 1;
        anim.SetFloat("SpeedMultiplier", multiplier);
    }

    [ClientRpc]
    public void RpcSetRunAnimMultiplier(float value)
    {
        anim.SetFloat("SpeedMultiplier", value);
    }

    public void OnChangeHeading(bool _, bool value)
    {
        playerObject.transform.localScale = new Vector3((isHeadingRight ? -1 : 1), 1f, 1f);
    }
    
    public void OnSetNickName(string _, string value)
    {
        nameText.text = value;
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

    private IEnumerator DropRoutine()
    {
        rigid2d.gravityScale = 0f;
        rigid2d.velocity = Vector2.zero;
        yield return new WaitForSeconds(.2f);
        rigid2d.gravityScale = normalGravityScale;
        rigid2d.velocity = Vector2.down * 15f;
    }

    private IEnumerator Stunned(float stunTime)
    {
        stateMachine.SetState(dicState[RPlayerState.Stun]);
        yield return new WaitForSeconds(stunTime);
        stateMachine.SetState(dicState[RPlayerState.Run]);
    }

    public override void OnClientExitRoom()
    {
        manager.playerPrefabMemory[PrefabIndex] = false;  
    }
}