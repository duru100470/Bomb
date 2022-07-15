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
        if(isServer)
        {
            List<RoomPlayer> players = manager.GetPlayerList();
            int cnt = 0;
            for(int i = 0; i<players.Count; i++)
            {
                if(players[i].readyToBegin) cnt++;
            }
            if(cnt == players.Count-1)
            {
                manager.ServerChangeScene(manager.GameplayScene);
            }
        }
        else
        {
            player.CmdChangeReadyState(!player.isReady);
        }
    }
}