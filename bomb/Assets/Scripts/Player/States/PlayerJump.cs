using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : IState 
{
    private PlayerStateManager player;

    public PlayerJump(PlayerStateManager player){
        this.player = player;
    }
    
    public void OperateEnter(){
        //Debug.Log("Enter jump");
        player.rigid2d.velocity = new Vector2(player.rigid2d.velocity.x, player.JumpForce);
    }
    public void OperateExit(){
        //Debug.Log("Exit Jump");
    }
    public void OperateUpdate(){

    }
}