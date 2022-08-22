using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPlayerStun : IState
{
    private RoomPlayer player;

    public RPlayerStun(RoomPlayer player)
    {
        this.player = player;
    }

    public void OperateEnter()
    {
        //player.CmdSetAisStunned(true);
        player.coll.sharedMaterial = player.stunPhysicsMat;
    }
    public void OperateExit()
    {
        //player.CmdSetAisStunned(false);
        player.coll.sharedMaterial = player.idlePhysicsMat;
        player.rigid2d.velocity = new Vector2(Mathf.Clamp(player.rigid2d.velocity.x, -player.MaxSpeed, player.MaxSpeed), player.rigid2d.velocity.y);
    }
    public void OperateUpdate()
    {

    }
}