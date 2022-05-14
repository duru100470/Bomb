using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStun : IState 
{
    private Player player;

    public PlayerStun(Player player){
        this.player = player;
    }
    
    public void OperateEnter(){
        //Debug.Log("stunned" + player.name);
        player.coll.sharedMaterial = player.stunPhysicsMat;
    }
    public void OperateExit(){
        player.coll.sharedMaterial = player.idlePhysicsMat;
    }
    public void OperateUpdate(){

    }
}