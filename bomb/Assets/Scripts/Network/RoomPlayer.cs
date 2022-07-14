using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayer : NetworkRoomPlayer
{
    [SyncVar(hook = nameof(OnSetNickName))] public string playerNickname = string.Empty;
    RoomManager manager = NetworkManager.singleton as RoomManager;
    [SerializeField] Text nameText;

    public SpriteRenderer spriteRenderer { set; get; }
    public Rigidbody2D rigid2d { set; get; }
    public Collider2D coll { set; get; }
    public PhysicsMaterial2D idlePhysicsMat;
    public PhysicsMaterial2D stunPhysicsMat;

    [SerializeField] private float moveSpeed = 0f;
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float accelaration = 10f;
    [SerializeField] private float ghostSpeed = 10f;

    public float MoveSpeed => moveSpeed;
    public float JumpForce => jumpForce;
    public float MaxSpeed => maxSpeed;
    public float MinSpeed => minSpeed;
    public float Accelaration => accelaration;
    public float GhostSpeed => ghostSpeed;
    private float jumpBufferTime = 0.2f;
    private float jumpBufferTimeCnt;
    private float hangTime = 0.1f;
    private float hangTimeCnt;
    [SerializeField] private bool isGround = false;
    [SyncVar] public bool isHeadingRight = false;
    public bool isReady = false;
    UI_Lobby UI_Lobby;

    public override void OnStartClient()
    {   
        if(isLocalPlayer) 
        {
            UI_Lobby = (UI_Lobby)FindObjectOfType(typeof(UI_Lobby));
            UI_Lobby.player = this;
            manager.AddPlayer(this);
            CmdSetNickName(PlayerSetting.playerNickname);
            spriteRenderer = GetComponent<SpriteRenderer>();
            rigid2d = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
        }
    }

    public void OnSetNickName(string _, string value){
        nameText.text = value;
    }

    // 키보드 입력 받기 및 State 갱신
    private void Update()
    {
        // 로컬 플레이어가 아닐 경우 작동 X
        if (!isLocalPlayer) return;
        KeyboardInput();
    }

    private void FixedUpdate()
    {
        if(!isLocalPlayer) return;
        RaycastHit2D raycastHit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.02f, LayerMask.GetMask("Ground"));
        if (raycastHit.collider != null) isGround = true;
        else isGround = false;
        spriteRenderer.flipX = !isHeadingRight;
    }

    // 키보드 입력 제어
    private void KeyboardInput()
    {
        // Run State
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            float direction = Input.GetAxisRaw("Horizontal");
            //isHeadingRight는 현재 누르고 있는 방향을 가리킴, 중립 상태에서는 가장 마지막으로 눌렀던 방향을 가리킴
            if (direction != 0) isHeadingRight = direction > 0 ? true : false;
            float curAccel = Mathf.Abs(rigid2d.velocity.x) < minSpeed ? accelaration * 2 : accelaration;
            float xVelocity = rigid2d.velocity.x + direction * curAccel * Time.deltaTime;
            rigid2d.velocity = new Vector2(Mathf.Clamp(xVelocity, -maxSpeed, maxSpeed), rigid2d.velocity.y);
        }

        // Jump State
        if(isGround) 
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
            rigid2d.velocity = new Vector2(rigid2d.velocity.x, jumpForce);
            jumpBufferTimeCnt = 0f;
        }   
    }

    [Command]
    public void CmdSetNickName(string nick)
    {
        playerNickname = nick;
    }
}