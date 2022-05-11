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
    }
    public void OperateExit(){

    }
    public void OperateUpdate(){

    }
}