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
        Dead
    }

    private StateMachine stateMachine;

    // 상태를 저장할 딕셔너리 생성
    private Dictionary<PlayerState, IState> dicState = new Dictionary<PlayerState, IState>();

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

    private void KeyboardInput()
    {
        // 키보드 입력 통한 State 전이 관리
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0){
            stateMachine.SetState(dicState[PlayerState.Run]);
        }else{
            stateMachine.SetState(dicState[PlayerState.Idle]);
        }
    }
}
