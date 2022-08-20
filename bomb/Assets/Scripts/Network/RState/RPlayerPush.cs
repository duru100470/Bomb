using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPlayerPush : IState
{
    private RoomPlayer player;

    public RPlayerPush(RoomPlayer player)
    {
        this.player = player;
    }

    public void OperateEnter()
    {
        //Debug.Log("push enter");
    }

    public void OperateExit()
    {
        
    }

    public void OperateUpdate()
    {
        
    }
}
