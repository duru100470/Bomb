using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDrop : IState
{
    private PlayerStateManager player;

    public PlayerDrop(PlayerStateManager player)
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
            player.CmdPlayAudio(AudioType.Drop_End);
            player.CmdPlayDropParticle();
            player.CmdSetStun(.5f);
        }
    }
    public void OperateUpdate()
    {
        
    }
}
