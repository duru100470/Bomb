using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCast : IState 
{
    private Player player;

    public PlayerCast(Player player){
        this.player = player;
    }
    
    public void OperateEnter(){
        // 아이템 사용
        player.UseItem();
        //Debug.Log("Cast enter");
    }
    public void OperateExit(){

    }
    public void OperateUpdate(){

    }
}