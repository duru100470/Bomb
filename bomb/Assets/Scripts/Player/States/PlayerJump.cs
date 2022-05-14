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
        player.rigid2d.AddForce(new Vector2(0,1)*player.JumpForce, ForceMode2D.Impulse);
    }
    public void OperateExit(){
        //Debug.Log("Exit Jump");
    }
    public void OperateUpdate(){

    }
}