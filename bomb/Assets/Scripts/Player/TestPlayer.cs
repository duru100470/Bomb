using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TestPlayer : MonoBehaviour
{


    [SerializeField] private GameObject playerObject;

    public SpriteRenderer spriteRenderer { set; get; }
    public Rigidbody2D rigid2d { set; get; }
    public Collider2D coll { set; get; }


    [Header("Player Control Value")]

    [SerializeField] private float CurrentSpeedX;
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float accelaration = 10f;
    [SerializeField] private float pushCoolDown = .5f;
    [SerializeField] private float stepOnForce = 5f;
    private float curPushCoolDown;

    public float MaxSpeed => maxSpeed;
    public float MinSpeed => minSpeed;
    public float Accelaration => accelaration;
    private float dashTime = 0f;
    public float dashVel { get; set; }
    private float curDashTime = 0f;
    private float lerpT = 0f;
    [SerializeField] private float jumpBufferTime = 0.05f;
    private float jumpBufferTimeCnt;
    [SerializeField] private float hangTime = 0.1f;
    private float hangTimeCnt;

    [Header("Player Current State Value")]

    [SerializeField] private bool isGround = false;
    [SerializeField] private bool onGround = false;
    [SerializeField] private bool isWallJumpable;
    [SerializeField] public bool isWallAttached;

    public bool isHeadingRight = false;
    public bool isCasting { set; get; } = false;

    [SerializeField] private bool hasJumped = false;

    [SerializeField] private float curGroundAngle;


    private bool nowCasting;
    private bool nowDrop;
    private bool nowIdle;
    private bool nowJump;
    private bool nowRun;
    private bool nowStun;

    private bool lastIsGrond = true;
    

    // Initialize states
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigid2d = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
    }

    // 키보드 입력 받기 및 State 갱신
    private void Update()
    {
        if(Input.GetAxis("Horizontal") != 0) isHeadingRight = Input.GetAxis("Horizontal") > 0 ? true : false; 
        KeyboardInput();
        // dash 속도 감소
        if (isCasting)
        {
            rigid2d.velocity = new Vector2((isHeadingRight ? 1 : -1) * Mathf.Lerp(0, dashVel, lerpT), rigid2d.velocity.y);
            curDashTime -= Time.deltaTime;
            lerpT = curDashTime / dashTime;
        }
        if(nowRun || nowJump)
        {
            float direction = Input.GetAxisRaw("Horizontal");
            if(isWallAttached && direction * (isHeadingRight ? 1 : -1) > 0) return;
            Rigidbody2D rbody = rigid2d;
            float curAccel = Mathf.Abs(rbody.velocity.x) < MinSpeed ? Accelaration * 2 : Accelaration;
            float xVelocity = rbody.velocity.x + direction * curAccel * Time.deltaTime;
            rbody.velocity = new Vector2(Mathf.Clamp(xVelocity, -MaxSpeed, MaxSpeed), rbody.velocity.y);
        }
        //ClearBool();
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
                if(rigid2d.velocity.y < .2f) isGround = true;
                isWallJumpable = true;
            }
        }
        else
        {
            isGround = false;
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
            hasJumped = false; 
        }

        playerObject.transform.localScale = new Vector3((isHeadingRight ? -1 : 1), 1, 1);
        coll.offset = new Vector2(isHeadingRight ? 0.03f : -0.03f,0);
        CurrentSpeedX = rigid2d.velocity.x;
        
        if(lastIsGrond && rigid2d.velocity.magnitude > .5f)
        {
            float XFriction = 0;
            float YFriction = 0;
            if(curGroundAngle == 0)
            {
                XFriction = 0.2f * 9.81f * rigid2d.gravityScale * Mathf.Cos(Mathf.Deg2Rad * curGroundAngle) * Mathf.Cos(Mathf.Deg2Rad * curGroundAngle) * (rigid2d.velocity.x > 0 ? -1 : 1);
            }
            else
            {
                YFriction = 0.2f * -9.81f * rigid2d.gravityScale * Mathf.Cos(Mathf.Deg2Rad * curGroundAngle) * Mathf.Sin(Mathf.Deg2Rad * curGroundAngle);
                XFriction = 0.2f * 9.81f * rigid2d.gravityScale * Mathf.Cos(Mathf.Deg2Rad * curGroundAngle) * Mathf.Cos(Mathf.Deg2Rad * curGroundAngle) * (curGroundAngle > 0 ? -1 : 1);    
            } 
            //마찰 적용
            Debug.Log($"curGround Angle : {curGroundAngle} XFriction : {XFriction} YFriction : {YFriction}");
            rigid2d.AddForce(new Vector2(XFriction, YFriction), ForceMode2D.Force);
        }
        lastIsGrond = isGround;

    }

    // 다른 플레이어 충돌
    private void OnCollisionEnter2D(Collision2D other)
    {
        if(other.transform.CompareTag("Player") && transform.position.y > other.transform.position.y + coll.bounds.size.y * .75f)
        {
            rigid2d.velocity = new Vector2(rigid2d.velocity.x, stepOnForce);
        }

        if(other.transform.CompareTag("Ground") && other.GetContact(0).point.y < transform.position.y)//other.transform.position.y < transform.position.y)
        {
            onGround = true;
            curGroundAngle = other.transform.rotation.z;
        }
    }

    private void OnCollisionExit2D(Collision2D other) 
    {
        if(other.transform.CompareTag("Ground"))
        {
            onGround = false;
        }    
    }

    // 키보드 입력 제어
    private void KeyboardInput()
    {
        // Stun, Dead상태가 아니거나 돌진 중이 아닐 때 행동 가능
        if (!nowStun && !isCasting)
        {
            // Run State
            if(!hasJumped && isGround)
            {
                if (Input.GetAxisRaw("Horizontal") != 0)
                {
                    nowIdle = true;
                }
                else
                {
                    nowRun = true;
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
                nowJump = true;

                rigid2d.velocity = new Vector2(rigid2d.velocity.x, jumpForce);
                hasJumped = true;
                isWallJumpable = false;
                jumpBufferTimeCnt = 0f;
            }   

            // Cast State
            if (Input.GetKeyDown(KeyCode.Q) && !nowCasting)
            {
                nowCasting = true;
            }

            // Drop State
            if( Input.GetKeyDown(KeyCode.S) && !isGround && !nowDrop)
            {
                nowDrop = true;
                StartCoroutine(DropRoutine());
            }

            // Push State
            if(Input.GetKeyDown(KeyCode.E) && curPushCoolDown > pushCoolDown)
            {
                curPushCoolDown = 0f;
                Push();
            }
            curPushCoolDown += Time.deltaTime;
        }
    }

    //Dash후 감속
    public void DashDone(float time)
    {
        dashTime = time;
        curDashTime = time;
        lerpT = 1f;
        StartCoroutine(_DashDone());
    }

    //감속 종료 후 gravityScale 정상화, Casting 종료
    private IEnumerator _DashDone()
    {
        yield return new WaitForSeconds(dashTime);
        isCasting = false;
        rigid2d.gravityScale = 1f;
    }

    private IEnumerator DropRoutine()
    {
        rigid2d.gravityScale = 0f;
        rigid2d.velocity = Vector2.zero;
        yield return new WaitForSeconds(.2f);
        rigid2d.gravityScale = 1f;
        rigid2d.velocity = Vector2.down * 15f;
    }

    public void Push()
    {
        RaycastHit2D[] check = Physics2D.BoxCastAll(coll.bounds.center, coll.bounds.size, 0f, Vector2.right * (isHeadingRight ? 1 : -1), coll.bounds.size.x * .5f, LayerMask.GetMask("Player"));

        foreach(var player in check)
        {
            if(player.collider != null && player.transform != this.transform)
            {
                player.transform.GetComponent<Rigidbody2D>().velocity = new Vector2((isHeadingRight ? .5f : -.5f), .5f) * 5f;
            }
        }
    }

    public void ClearBool()
    {
        nowCasting = nowDrop = nowIdle = nowRun = nowJump = nowStun = false;
    }

}