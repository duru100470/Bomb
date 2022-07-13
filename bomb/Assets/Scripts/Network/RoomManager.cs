using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomManager : NetworkRoomManager
{
    public string hostIP;
    private List<RoomPlayer> roomPlayerList = new List<RoomPlayer>();

    public void AddPlayer(RoomPlayer player)
    {
        if (!roomPlayerList.Contains(player))
        {
            roomPlayerList.Add(player);
            Debug.Log("roomPlayer Added");
        }
    }

    public List<RoomPlayer> GetPlayerList()
    {
        return roomPlayerList;
    } 
    
    public override void OnStartHost(){
        hostIP = PlayerSetting.hostIP;
    }
    
    public override void ServerChangeScene(string newSceneName)
    {
        base.ServerChangeScene(newSceneName);
        foreach(var player in roomPlayerList)
        {
            player.gameObject.SetActive(false);
        }
    }
}