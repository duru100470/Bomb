using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
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
    private List<Item> itemList = new List<Item>();
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigid2d;
   
    [SerializeField]
    private float moveSpeed = 1.0f;
    public float MoveSpeed => moveSpeed;

    // Initialize states
    private void Start() {
        IState idle = new PlayerIdle(this);
        IState run = new PlayerRun(this);
        IState jump = new PlayerJump(this);
        IState stun = new PlayerStun(this);
        IState dead = new PlayerDead(this);

        dicState.Add(PlayerState.Idle, idle);
        dicState.Add(PlayerState.Run, run);
        dicState.Add(PlayerState.Jump, jump);
        dicState.Add(PlayerState.Stun, stun);
        dicState.Add(PlayerState.Dead, dead);

        // 시작 상태를 Idle로 설정
        stateMachine = new StateMachine(dicState[PlayerState.Idle]);

        spriteRenderer = GetComponent<SpriteRenderer>();
        rigid2d = GetComponent<Rigidbody2D>();
    }

    // 키보드 입력 받기 및 CurruentState 실행
    private void Update() {
        // 로컬 플레이어가 아닐 경우 작동 X
        if(!isLocalPlayer) return;
        KeyboardInput();
        stateMachine.DoOperateUpdate();
    }

    // 키보드 입력 제어
    private void KeyboardInput()
    {
        // Stun이나 Dead가 아닐 때 행동 가능
        if (stateMachine.CurruentState != dicState[PlayerState.Stun] && stateMachine.CurruentState != dicState[PlayerState.Dead]){
            // Run State
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0){
                stateMachine.SetState(dicState[PlayerState.Run]);
            }else{
                stateMachine.SetState(dicState[PlayerState.Idle]);
            }

            // Jump State
            if (Input.GetKeyDown(KeyCode.Space) && stateMachine.CurruentState != dicState[PlayerState.Jump]){
                stateMachine.SetState(dicState[PlayerState.Jump]);
            }

            // Cast State
            if (Input.GetKeyDown(KeyCode.Q) && stateMachine.CurruentState != dicState[PlayerState.Cast]){
                stateMachine.SetState(dicState[PlayerState.Cast]);
            }
        }
    }

    // 다른 플레이어 및 아이템 충돌
    private void OnCollisionEnter2D(Collision2D other) {
        // 플레이어인 경우

        // 아이템인 경우

        // 서버에 로그 전송
    }

    // 아이템 획득
    private void AddItem(Item item){
        // 아이템이 1개 이상이면 아무 작동 안함
        if(itemList.Count > 0) return;

        // 아이템 리스트에 추가
        itemList.Add(item);
    }

    // 아이템 사용
    public void UseItem(){
        // 아이템이 0개면 작동 안함
        if(itemList.Count == 0) return;
        
        foreach (var i in itemList){
            i.OnUse();
        }

        itemList.Clear();
    }
}
