using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPlayerDrop : IState
{
    private RoomPlayer player;

    public RPlayerDrop(RoomPlayer player)
    {
        this.player = player;
    }

    public void OperateEnter()
    {
        //Debug.Log("Drop enter");
    }

    public void OperateExit()
    {
        //Debug.Log("Drop Exit");
        if(player.IsGround)
        {
            player.CmdSetStun(.5f);
        }
    }
    public void OperateUpdate()
    {
        
    }
}
