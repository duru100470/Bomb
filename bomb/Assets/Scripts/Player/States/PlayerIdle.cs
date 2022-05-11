using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdle : IState 
{
    private Player player;

    public PlayerIdle(Player player){
        this.player = player;
    }

    public void OperateEnter(){
        //Debug.Log("Idle Enter");
    }

    public void OperateExit(){
        //Debug.Log("Idle Exit");
    }
    public void OperateUpdate(){

    }
}
