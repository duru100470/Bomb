using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomManager : NetworkRoomManager
{
    public string hostIP;
    [SerializeField] private List<RoomPlayer> roomPlayerList = new List<RoomPlayer>();

    public override void Start()
    {
        base.Start();
        SoundManager.Instance.SetCamSource(Camera.main.GetComponents<AudioSource>());
    }

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
    
    public override void OnStartHost()
    {
        hostIP = PlayerSetting.hostIP;
    }

    public override void OnRoomServerPlayersReady()
    {
        PlayerSetting.playerNum = roomSlots.Count;
        base.OnRoomServerPlayersReady();
    }
}