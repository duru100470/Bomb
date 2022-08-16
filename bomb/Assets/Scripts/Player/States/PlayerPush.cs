using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPush : IState
{
    private PlayerStateManager player;

    public PlayerPush(PlayerStateManager player)
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
