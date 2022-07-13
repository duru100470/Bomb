using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomManager : NetworkRoomManager
{
    public string hostIP;

    public override void OnStartHost(){
        hostIP = PlayerSetting.hostIP;
    }
}