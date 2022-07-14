using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UI_Lobby : NetworkBehaviour
{
    [SyncVar] public string hostIP;
    [SerializeField] Text text;
    [SerializeField] RectTransform GameRule;
    [SerializeField] Button button_Play;
    [SerializeField] Text buttonPlay_text;
    NetworkRoomManager manager = NetworkManager.singleton as NetworkRoomManager;
    public RoomPlayer player;

    public void Start()
    {
        if(isServer)
        {
            hostIP = PlayerSetting.hostIP;
            buttonPlay_text.text = "PLAY";
        }
        else
        {
            buttonPlay_text.text = "READY";
        }
        text.text = hostIP;
    }

    public void OnClickButtonPlay()
    {
        if(isServer)
        {
            manager.CheckReadyToBegin();
            player.CmdChangeReadyState(!player.isReady);
        }
        else
        {
            player.CmdChangeReadyState(!player.isReady);
        }
    }
}