using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRun : IState 
{
    private Player player;

    public PlayerRun(Player player){
        this.player = player;
    }
    
    public void OperateEnter(){
        Debug.Log("Run Enter");
    }
    public void OperateExit(){
        Debug.Log("Run Exit");
    }
    public void OperateUpdate(){
        float direction = Input.GetAxis("Horizontal");
        player.transform.position += new Vector3(direction, 0, 0) * player.MoveSpeed * Time.deltaTime;
    }
}