using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomPlayer : NetworkRoomPlayer
{
    [SyncVar] public string playerNickname = string.Empty;

    public override void OnClientEnterRoom()
    {
        if(isLocalPlayer) 
        {
            CmdSetNickName(PlayerSetting.playerNickname);
        }
    }

    [Command]
    public void CmdSetNickName(string nick)
    {
        playerNickname = nick;
    }
}