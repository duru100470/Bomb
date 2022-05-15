using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStun : IState 
{
    private PlayerStateManager player;

    public PlayerStun(PlayerStateManager player){
        this.player = player;
    }
    
    public void OperateEnter(){
        //Debug.Log("stunned" + player.name);
        player.spriteRenderer.color = new Color(1, 1, 1, 0.5f);
        player.coll.sharedMaterial = player.stunPhysicsMat;
    }
    public void OperateExit(){
        player.coll.sharedMaterial = player.idlePhysicsMat;
        player.spriteRenderer.color = new Color(1, 1, 1, 1f);
    }
    public void OperateUpdate(){

    }
}