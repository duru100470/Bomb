using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdle : IState
{
    private PlayerStateManager player;

    public PlayerIdle(PlayerStateManager player)
    {
        this.player = player;
    }

    public void OperateEnter()
    {
        //Debug.Log("Idle Enter");
    }

    public void OperateExit()
    {
        //Debug.Log("Idle Exit");
    }
    public void OperateUpdate()
    {

    }
}
