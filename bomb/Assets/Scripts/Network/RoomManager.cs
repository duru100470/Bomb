using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomManager : NetworkRoomManager
{
    [SerializeField] UI_Lobby UI_Lobby;

    public override void OnRoomStartHost()
    {   
        UI_Lobby.gameObject.SetActive(true);
        UI_Lobby.SetIPPoint();
    }
}