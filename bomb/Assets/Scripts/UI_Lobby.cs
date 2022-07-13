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

    public void Start(){
        if(isServer)
        {
            hostIP = PlayerSetting.hostIP;
        }
        text.text = hostIP;
    }
}