using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCast : IState 
{
    private PlayerStateManager player;

    public PlayerCast(PlayerStateManager player){
        this.player = player;
    }
    
    public void OperateEnter(){
        // 아이템 사용
        player.UseItem();
    }
    public void OperateExit(){

    }
    public void OperateUpdate(){

    }
}