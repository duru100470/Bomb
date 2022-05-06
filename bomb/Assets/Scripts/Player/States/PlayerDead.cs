using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDead : IState 
{
    private Player player;

    public PlayerDead(Player player){
        this.player = player;
    }
    
    public void OperateEnter(){

    }
    public void OperateExit(){

    }
    public void OperateUpdate(){

    }
}