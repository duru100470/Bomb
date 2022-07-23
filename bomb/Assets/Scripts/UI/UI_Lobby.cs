using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.EventSystems;

public class UI_Lobby : NetworkBehaviour
{
    [SyncVar] public string hostIP;
    [SerializeField] Text text;
    [SerializeField] RectTransform GameRule;
    [SerializeField] Button button_Play;
    [SerializeField] Text buttonPlay_text;
    [SerializeField] Text playerStatus_text;
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

    public void Update()
    {
        int cnt = 0;
        foreach(var player in manager.roomSlots) 
        {
            if(player.readyToBegin) cnt++;
        }
        playerStatus_text.text = cnt + " / " + (manager.roomSlots.Count-1);
    }

    public void OnClickButtonPlay()
    {
        if(isServer)
        {
            int cnt = 0;
            foreach(var cur in manager.roomSlots)
            {
                if(cur.readyToBegin)
                {
                    cnt++;
                }
            }
            if(cnt == manager.roomSlots.Count-1)
            {
                player.CmdChangeReadyState(true);
            }
        }
        else
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

}