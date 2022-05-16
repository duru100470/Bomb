using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
public class PlayerStateManager : NetworkBehaviour
{
    private enum PlayerState{
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
    [SyncVar][SerializeField]
    private Item curItem;
    public SpriteRenderer spriteRenderer {set; get;}
    public Rigidbody2D rigid2d {set; get;}
    public Collider2D coll {set; get;}
    public GameObject curItemObj {set; get;}
    public PhysicsMaterial2D idlePhysicsMat {set; get;}
    public PhysicsMaterial2D stunPhysicsMat {set; get;}
    // 폭탄 글로벌 타이머 (For Debugging)
    [SerializeField]
    private Text timer;
   
    [SerializeField]
    private float moveSpeed = 0f;
    [SerializeField]
    private float jumpForce = 5.0f;
    [SerializeField]
    private float maxSpeed = 10f;
    [SerializeField]
    private float minSpeed = 2f;
    [SerializeField]
    private float accelaration = 10f;
    [SerializeField]
    private float ghostSpeed = 10f;
    
    public float MoveSpeed => moveSpeed;
    public float JumpForce => jumpForce;
    [SerializeField]
    private float power = 30f;
    public float MaxSpeed => maxSpeed;
    public float MinSpeed => minSpeed;
    public float Accelaration => accelaration;
    public float GhostSpeed => ghostSpeed;
    private float refVelocity = 0f;
    private float dashTime = 0f;

    ///
    [SerializeField]
    private bool isGround = false;
    [SyncVar]
    public bool isHeadingRight = false;
    public bool isCasting {set; get;} = false;
    [SyncVar]
    private bool isTransferable = true;
    public bool IsTransferable => isTransferable;
    [SyncVar]
    public bool hasBomb = false;

    // Initialize states
    private void Start() {
        // 게임 매니저에 해당 플레이어 추가
        GameManager.Instance.AddPlayer(this);

        // 타이머 초기화 (For Debugging)
        timer.text = GameManager.Instance.bombGlobalTime.ToString();

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
    }
    // 키보드 입력 받기 및 State 갱신
    private void Update() {
        // 로컬 플레이어가 아닐 경우 작동 X
        if(!isLocalPlayer) return;
        KeyboardInput();
        stateMachine.DoOperateUpdate();
        // dash 속도 감소
        if(isCasting) rigid2d.velocity = new Vector2(Mathf.SmoothDamp(rigid2d.velocity.x, 0f, ref refVelocity, dashTime), rigid2d.velocity.y);
    }

    private void FixedUpdate(){
        RaycastHit2D raycastHit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.02f, LayerMask.GetMask("Ground"));
        if(raycastHit.collider != null) isGround = true;
        else isGround = false;
        spriteRenderer.flipX = !isHeadingRight;
    }
    // 키보드 입력 제어
    private void KeyboardInput()
    {
        // Stun, Dead상태가 아니거나 돌진 중이 아닐 때 행동 가능
        if (stateMachine.CurruentState != dicState[PlayerState.Stun] && stateMachine.CurruentState != dicState[PlayerState.Dead] && !isCasting){
            // Run State
            if (Input.GetAxisRaw("Horizontal") != 0){
                stateMachine.SetState(dicState[PlayerState.Run]);
            }else{
                stateMachine.SetState(dicState[PlayerState.Idle]);
            }

            // Jump State
            if (Input.GetKeyDown(KeyCode.Space) && this.isGround){
                stateMachine.SetState(dicState[PlayerState.Jump]);
            }

            // Cast State
            if (Input.GetKeyDown(KeyCode.Q) && stateMachine.CurruentState != dicState[PlayerState.Cast]){
                stateMachine.SetState(dicState[PlayerState.Cast]);
            }
        }
        if(stateMachine.CurruentState == dicState[PlayerState.Dead]){
            //고스트 상태에서 하는 무언가


        }
    }
    // 아이템 사용
    public void UseItem(){
        // 아이템이 없으면 작동 안함
        if(curItem != null){
            curItem.OnUse();
            curItem = null;
        }
    }

    private IEnumerator Stunned(float stunTime){
        stateMachine.SetState(dicState[PlayerState.Stun]);
        yield return new WaitForSeconds(stunTime);
        stateMachine.SetState(dicState[PlayerState.Idle]);
    }

    // 다른 플레이어 충돌
    private void OnCollisionEnter2D(Collision2D other) {
        if (!hasAuthority) return;

        if(other.transform.CompareTag("Player") && hasBomb == true && isTransferable == true){
            var targetPSM = other.transform.GetComponent<PlayerStateManager>();

            if (targetPSM.hasBomb == false && hasAuthority){
                StartCoroutine(_TransitionDone());
                StartCoroutine(Stunned(2f));
                Vector2 dir = ( transform.position - other.transform.position ).normalized * power;

                rigid2d.AddForce(dir);
                CmdBombTransition(targetPSM.netId, dir * (-1));
            }
        }
    }

    // 다른 아이템 충돌
    private void OnTriggerEnter2D(Collider2D other) {

        // 아이템인 경우, 현재 아이템을 가지고 있지 않은 상태여야 한다
        if(other.transform.CompareTag("Item") && curItem == null && hasAuthority){
            Item _item = other.GetComponent<Item>();
            _item.player = this;
            curItemObj = _item.itemObj;
            CmdAddItem(_item);
        }
        // Stone에 맞았을 때
        if(other.transform.CompareTag("Projectile")){
            Debug.Log(netId);
            CmdHitStone(netId, other.GetComponent<StoneProjectile>().stunTime);
            NetworkServer.Destroy(other.gameObject);
        }
        // 서버에 로그 전송
    }

    // 아이템 획득 상태 동기화
    [Command]
    private void CmdAddItem(Item item){
        curItem = item;
        curItemObj = item.itemObj;
        item.player = this;
        item.spawner.isSpawnable = true;
        item.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
        RpcItemSync(item.netId);
    }

    //Dash후 감속
    public void DashDone(float time){
        dashTime = time;
        StartCoroutine(_DashDone());
    }

    //감속 종료 후 gravityScale 정상화, Casting 종료
    private IEnumerator _DashDone(){
        yield return new WaitForSeconds(dashTime);
        isCasting = false;
        rigid2d.velocity = Vector2.zero;
        rigid2d.gravityScale = 1f;
    }

    // 폭탄 전달시 약간의 딜레이 부여
    private IEnumerator _TransitionDone(){
        isTransferable = false;
        yield return new WaitForSeconds(0.1f);
        isTransferable = true;
    }

    [Command]
    public void CmdBombTransition(uint targetNetId, Vector3 dir){
        PlayerStateManager target = null;

        foreach (var player in GameManager.Instance.GetPlayerList()){
            if (player.netId == targetNetId){
                target = player;
            }
        }

        if (target != null){
            Debug.Log("CmdBombTransition Called!!");
            GameManager.Instance.bombLocalTime = Mathf.Max( Mathf.Round( GameManager.Instance.bombGlobalTime / 5 ), 2f);
            hasBomb = false;
            target.GetBomb(dir);
        }
    }

    [Command (requiresAuthority = false)]
    public void CmdHitStone(uint targetNetId, float time){
        PlayerStateManager target = null;
        foreach (var player in GameManager.Instance.GetPlayerList()){
            if (player.netId == targetNetId){
                target = player;
                Debug.Log("Setted by target : " + target.netId);
                target.RpcStunSync(time);
            }
        }
    }
    
    public void GetBomb(Vector2 dir){
        StartCoroutine(_TransitionDone());
        hasBomb = true;
        RpcGetBomb(dir);
        RpcStunSync(2f);
    }

    [ClientRpc]
    public void RpcGetBomb(Vector2 dir){
        if (hasAuthority){
            Debug.Log("Stunned");
            Debug.Log(dir);
            rigid2d.AddForce(dir);
        }
    }

    //Stun상태 sync용 ClientRpc
    [ClientRpc]
    public void RpcStunSync(float time){
        if(hasAuthority){
            StartCoroutine(Stunned(time));
        }
    }

    // 폭탄 폭발
    [ClientRpc]
    public void RpcDead(){
        if (hasAuthority){
            StopAllCoroutines();
            stateMachine.SetState(dicState[PlayerState.Dead]);
        }else{
            // 죽으면 모습 안보이게 (임시)
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.1f);
            this.gameObject.layer = LayerMask.NameToLayer("GhostPlayer");    
        }
    }

    //획득된 아이템의 collider와 renderer의 비활성화 상태 동기화
    [ClientRpc]
    public void RpcItemSync(uint netId){
        GameObject obj = NetworkClient.spawned[netId].gameObject; 
        obj.GetComponent<Collider2D>().enabled = false;
        obj.GetComponent<SpriteRenderer>().enabled = false;
    }

    //Destroy되는 아이템의 상태 동기화
    [ClientRpc]
    public void RpcItemDestroy(uint netId){
        NetworkServer.Destroy(NetworkClient.spawned[netId].gameObject);
    }

    [ClientRpc]
    public void RpcTeleport(Vector3 position){
        transform.position = position;
    }

    [ClientRpc]
    public void RpcSetTimer(float time){
        timer.text = time.ToString();
    }
}