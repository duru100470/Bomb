using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : IState 
{
    private Player player;

    public PlayerJump(Player player){
        this.player = player;
    }
    
    public void OperateEnter(){

    }
    public void OperateExit(){

    }
    public void OperateUpdate(){

    }
}