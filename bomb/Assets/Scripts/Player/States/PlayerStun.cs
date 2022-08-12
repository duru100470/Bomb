using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStun : IState
{
    private PlayerStateManager player;

    public PlayerStun(PlayerStateManager player)
    {
        this.player = player;
    }

    public void OperateEnter()
    {
        player.CmdSetAisStunned(true);
        player.spriteRenderer.color = new Color(1, 1, 1, 0.5f);
        player.coll.sharedMaterial = player.stunPhysicsMat;
    }
    public void OperateExit()
    {
        player.CmdSetAisStunned(false);
        player.coll.sharedMaterial = player.idlePhysicsMat;
        player.rigid2d.velocity = new Vector2(Mathf.Clamp(player.rigid2d.velocity.x, -player.MaxSpeed, player.MaxSpeed), player.rigid2d.velocity.y);
        player.spriteRenderer.color = new Color(1, 1, 1, 1f);
    }
    public void OperateUpdate()
    {

    }
}