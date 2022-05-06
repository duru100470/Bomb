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
        player.transform.position += new Vector3(Input.GetAxisRaw("Horizontal") * player.MoveSpeed * Time.deltaTime, Input.GetAxisRaw("Vertical") * player.MoveSpeed * Time.deltaTime, 0.0f);
    }
}