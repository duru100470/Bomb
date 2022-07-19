using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRun : IState
{
    private PlayerStateManager player;

    public PlayerRun(PlayerStateManager player)
    {
        this.player = player;
    }

    public void OperateEnter()
    {
        //Debug.Log("Run Enter");
    }
    public void OperateExit()
    {
        //Debug.Log("Run Exit");
    }
    public void OperateUpdate()
    {
        float direction = Input.GetAxisRaw("Horizontal");
        //isHeadingRight는 현재 누르고 있는 방향을 가리킴, 중립 상태에서는 가장 마지막으로 눌렀던 방향을 가리킴
        if (direction != 0) player.CmdIsHeadingSync(direction > 0 ? true : false);
        Rigidbody2D rbody = player.rigid2d;
        float curAccel = Mathf.Abs(rbody.velocity.x) < player.MinSpeed ? player.Accelaration * 2 : player.Accelaration;
        float xVelocity = rbody.velocity.x + direction * curAccel * Time.deltaTime;
        rbody.velocity = new Vector2(Mathf.Clamp(xVelocity, -player.MaxSpeed, player.MaxSpeed), rbody.velocity.y);
    }
}