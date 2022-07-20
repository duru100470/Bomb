using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : IState
{
    private PlayerStateManager player;

    public PlayerJump(PlayerStateManager player)
    {
        this.player = player;
    }

    public void OperateEnter()
    {
        player.rigid2d.velocity = new Vector2(player.rigid2d.velocity.x, player.JumpForce);
    }
    public void OperateExit()
    {

    }
    public void OperateUpdate()
    {

    }
}