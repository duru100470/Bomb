using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.EventSystems;
using System;

public class UI_Lobby : NetworkBehaviour
{
    [SyncVar] public string hostIP;
    [SerializeField] Text text;
    [SerializeField] Button button_Play;
    [SerializeField] Text buttonPlay_text;
    [SerializeField] Text playerStatus_text;
    [SerializeField] RectTransform Panel_ESC;
    [SerializeField] RectTransform Panel_option;
    [SerializeField] Button button_resume;
    [SerializeField] Button button_backtomain;
    RoomManager manager = NetworkManager.singleton as RoomManager;
    public RoomPlayer player;



    public void Start()
    {
        if(isServer)
        {
            hostIP = Encrypt(PlayerSetting.hostIP);
            buttonPlay_text.text = "PLAY";
        }
        else
        {
            buttonPlay_text.text = "READY";
        }
        text.text = hostIP;

        button_resume.onClick.AddListener(OnClickButtonResume);
        button_backtomain.onClick.AddListener(OnClickButtonBackToMain);
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

    
    public void ActivateESC()
    {
        Panel_ESC.gameObject.SetActive(true);
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

    public void OnClickButtonResume()
    {
        Panel_ESC.gameObject.SetActive(false);
    }

    public void OnClickButtonBackToMain()
    {
        
    }

    public string Encrypt(string str)
    {
        string ret = String.Empty;
        string[] strings = str.Split('.');
        foreach(var strng in strings)
        {
            int cur = Int32.Parse(strng);
            if(cur < 17) ret += "0";
            ret += Int32.Parse(strng).ToString("X");
        }
        return ret;
    }

    
}