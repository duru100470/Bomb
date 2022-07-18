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
    RoomManager manager = NetworkManager.singleton as RoomManager;
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
        if(player.readyToBegin)
        {
            player.CmdChangeReadyState(false);
        }
        else
        {
            player.CmdChangeReadyState(true);
        }
    }
}