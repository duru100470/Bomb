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
        Cast
    }

    private StateMachine stateMachine;
    // 상태를 저장할 딕셔너리 생성
    private Dictionary<PlayerState, IState> dicState = new Dictionary<PlayerState, IState>();
    private Animator VFXanim;
    [SerializeField] private Animator anim;
    [SerializeField] private Image curItemImage;
    [SerializeField] private Sprite defaultItemImage;
    [SerializeField] private Text nickNameText;
    [SerializeField] private Image bombStateImage;
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private List<Animator> ItemVFX = new List<Animator>();
    public Sprite LeaderBoardIcon;

    public SpriteRenderer spriteRenderer { set; get; }
    [SerializeField] SpriteRenderer ghostSprite;
    public Rigidbody2D rigid2d { set; get; }
    public Collider2D coll { set; get; }
    public GameObject curItemObj { set; get; }
    public PhysicsMaterial2D idlePhysicsMat;
    public PhysicsMaterial2D stunPhysicsMat;
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
    private float jumpBufferTime = 0.2f;
    private float jumpBufferTimeCnt;
    private float hangTime = 0.1f;
    private float hangTimeCnt;

    [Header("Player Current State Value")]

    [SyncVar(hook = nameof(OnChangeItem))][SerializeField] public Item curItem;
    [SyncVar(hook = nameof(OnChangePlayerLocalBombTime))]
    public float playerLocalBombTime;

    [SerializeField] private bool isGround = false;
    [SerializeField] private bool isWallJumpable;
    [SerializeField] public bool isWallAttached;

    [SyncVar] public bool isHeadingRight = false;
    [SyncVar] private bool isTransferable = true;
    [SyncVar] private bool isFlickering = false;
    [SyncVar(hook = nameof(OnChangeAisRunning))] public bool AisRunning = false;
    [SyncVar(hook = nameof(OnChangeAisStunned))] public bool AisStunned = false;
    [SyncVar(hook = nameof(OnChangeHasBomb))]
    public bool hasBomb = false;
    public bool isCasting { set; get; } = false;
    public bool IsTransferable => isTransferable;
    [SyncVar] public int roundScore;
    
    [SyncVar(hook = nameof(OnChangeNickName))]
    public string playerNickname;
    [SyncVar(hook = nameof(OnChangeBombState))] public int bombState;
    [SerializeField] private List<Sprite> bombSpriteList = new List<Sprite>();

    [SerializeField] private GameObject playerObject;

    #region UnityEventFunc

    // Initialize states
    private void Start()
    {
        
        if(isLocalPlayer) 
        {
            CmdSetNickName(PlayerSetting.playerNickname);        
        }
        else
        {
            SpriteRenderer[] rend = playerObject.GetComponentsInChildren<SpriteRenderer>();
            foreach(var render in rend)
            {
                render.color = new Color(1f, 0f, 0f, 1f);
            } 
        }
        
        // 게임 매니저에 해당 플레이어 추가
        GameManager.Instance.AddPlayer(this);

        IState idle = new PlayerIdle(this);
        IState run = new PlayerRun(this);
        IState jump = new PlayerJump(this);
        IState stun = new PlayerStun(this);
        IState dead = new PlayerDead(this);
        IState cast = new PlayerCast(this);

        dicState.Add(PlayerState.Idle, idle);
        dicState.Add(PlayerState.Run, run);
        dicState.Add(PlayerState.Jump, jump);
        dicState.Add(PlayerState.Stun, stun);
        dicState.Add(PlayerState.Dead, dead);
        dicState.Add(PlayerState.Cast, cast);

        // 시작 상태를 Idle로 설정
        stateMachine = new StateMachine(dicState[PlayerState.Idle]);

        spriteRenderer = GetComponent<SpriteRenderer>();
        rigid2d = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        VFXanim = explosionVFX.GetComponent<Animator>();
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
        RaycastHit2D raycastHitRight = Physics2D.Raycast(coll.bounds.center + new Vector3(-coll.bounds.extents.x + .1f, -coll.bounds.extents.y, 0), Vector2.down, .1f, LayerMask.GetMask("Ground"));
        if(raycastHitLeft.collider != null && raycastHitRight.collider != null)
        {
            isGround = true;
            isWallJumpable = true;
        }
        else
        {
            isGround = false;
        }

        RaycastHit2D raycastHitWall = Physics2D.Raycast(coll.bounds.center + new Vector3(coll.bounds.extents.x * (isHeadingRight ? 1 : -1), 0,0), Vector2.right * (isHeadingRight ? 1 : -1), .1f, LayerMask.GetMask("Ground"));
        if(raycastHitWall)
        {
            isWallAttached = true;
        }
        else
        {
            isWallAttached = false;
        }

        playerObject.transform.localScale = new Vector3((isHeadingRight ? -1 : 1), 1, 1);
        ItemVFX[0].transform.localScale = new Vector3((!isHeadingRight ? -1 : 1), 1, 1);
        ItemVFX[0].transform.localPosition = new Vector3(0.44f * (isHeadingRight ? -1 : 1), 0, 0);
        ghostSprite.flipX = isHeadingRight;
        coll.offset = new Vector2(isHeadingRight ? 0.03f : -0.03f,0); 
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawRay(coll.bounds.center + new Vector3(coll.bounds.extents.x - .1f, -coll.bounds.extents.y, 0), Vector2.down * .1f);
        Gizmos.DrawRay(coll.bounds.center + new Vector3(-coll.bounds.extents.x + .1f, -coll.bounds.extents.y, 0), Vector2.down * .1f);
        Gizmos.DrawRay(coll.bounds.center + new Vector3(coll.bounds.extents.x * (isHeadingRight ? 1 : -1), 0, 0), Vector2.right * (isHeadingRight ? 1 : -1) * .1f);
    }

    // 다른 플레이어 충돌
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!hasAuthority) return;

        if (other.transform.CompareTag("Player") && hasBomb && isTransferable)
        {
            var targetPSM = other.transform.GetComponent<PlayerStateManager>();
            if (targetPSM.hasBomb == false)
            {   
                GameManager.Instance.UI_Play.CmdAddLogTransition(this, targetPSM);
                StartCoroutine(_TransitionDone());
                StartCoroutine(Stunned(2f));
                Vector2 dir = (Vector3.Scale(transform.position - other.transform.position,new Vector3(2,1,1))).normalized * power;
                if(dir.y == 0) dir.y = 0.1f * power;
                rigid2d.velocity = dir;
                CmdBombTransition(targetPSM.netId, dir * (-1));
            }
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
            CmdHitStone(other.GetComponent<StoneProjectile>().StunTime, dir);
            NetworkServer.Destroy(other.gameObject);
        }
        // 서버에 로그 전송
    }

    #endregion UnityEventFunc

    // 키보드 입력 제어
    private void KeyboardInput()
    {
        // Stun, Dead상태가 아니거나 돌진 중이 아닐 때 행동 가능
        if (stateMachine.CurruentState != dicState[PlayerState.Stun] && stateMachine.CurruentState != dicState[PlayerState.Dead] && !isCasting)
        {
            // Run State
            if (Input.GetAxisRaw("Horizontal") != 0)
            {
                stateMachine.SetState(dicState[PlayerState.Run]);
            }
            else
            {
                stateMachine.SetState(dicState[PlayerState.Idle]);
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
            
            if(Input.GetKeyDown(KeyCode.Space))
            {
                jumpBufferTimeCnt = jumpBufferTime;
            }
            else
            {
                jumpBufferTimeCnt -= Time.deltaTime;
            }

            if (jumpBufferTimeCnt > 0f && hangTimeCnt > 0f)
            {
                stateMachine.SetState(dicState[PlayerState.Jump]);
                isWallJumpable = false;
                jumpBufferTimeCnt = 0f;
            }   

            // Cast State
            if (Input.GetKeyDown(KeyCode.Q) && stateMachine.CurruentState != dicState[PlayerState.Cast])
            {
                stateMachine.SetState(dicState[PlayerState.Cast]);
            }
        }
        if (stateMachine.CurruentState == dicState[PlayerState.Dead])
        {
            //고스트 상태에서 하는 무언가

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
        RpcStunSync(2f);
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

    #region IEnumerators

    //감속 종료 후 gravityScale 정상화, Casting 종료
    private IEnumerator _DashDone()
    {
        yield return new WaitForSeconds(dashTime);
        isCasting = false;
        rigid2d.gravityScale = 1f;
    }

    private IEnumerator Stunned(float stunTime)
    {
        stateMachine.SetState(dicState[PlayerState.Stun]);
        yield return new WaitForSeconds(stunTime);
        stateMachine.SetState(dicState[PlayerState.Idle]);
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
    public void CmdHitStone(float time, Vector2 dir)
    {
        RpcStunSync(time);
        RpcAddDirVec(dir);
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

    [Command]
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
    public void CmdAnimSetBool(string boolName, bool value)
    {
        RpcAnimSetBool(boolName, value);
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

    #endregion CommandFunc
    
    #region ClientRpcFunc

    [ClientRpc]
    public void RpcAddDirVec(Vector2 dir)
    {
        if(hasAuthority)
        {
            Debug.Log("Hitted__" + playerNickname);
            this.rigid2d.velocity = dir;
        }
    }

    [ClientRpc]
    public void RpcStunSync(float time)
    {
        if (hasAuthority)
        {
            Debug.Log("Stunned__" + playerNickname);
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
        spriteRenderer.color = new Color(1f,1f,1f,1f);
        this.gameObject.layer = LayerMask.NameToLayer("Player");
    }

    [ClientRpc]
    public void RpcSetItem()
    {
        curItem = null;
        curItemImage.sprite = defaultItemImage;
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
        ItemVFX[idx].SetTrigger("Trigger");
    }

    [ClientRpc]
    public void RpcAnimSetBool(string boolName, bool value)
    {
        anim.SetBool(boolName, value);
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
        curItemImage.sprite = defaultItemImage;
        if(value != null)
        {
            curItemImage.sprite = GameManager.Instance.itemSprites[(int)value.Type];
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
        if(!isLocalPlayer) return;
        CmdAnimSetBool("isRunning", value);
    }

    public void OnChangeAisStunned(bool _, bool value)
    {
        if(!isLocalPlayer) return;
        CmdAnimSetBool("isStunned", value);
    }

    #endregion SyncVarHookFunc

}