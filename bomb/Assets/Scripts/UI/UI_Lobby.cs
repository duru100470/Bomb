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
    [SerializeField] Button button_resume;
    [SerializeField] Button button_setting;
    [SerializeField] Button button_backtomain;
    RoomManager manager = NetworkManager.singleton as RoomManager;
    public RoomPlayer player;

    [Header ("GameRule")]

    [SyncVar(hook = nameof(OnChangeMaxBombTime))] private int maxBombTime;
    [SerializeField] private Text maxBombTimeText;
    [SyncVar(hook = nameof(OnChangeMinBombTime))] private int minBombTime;
    [SerializeField] private Text minBombTimeText;
    [SyncVar(hook = nameof(OnChangeScorePerRound))] private int scorePerRound;
    [SerializeField] private Text scorePerRoundText;
    [SyncVar(hook = nameof(OnChangeGhostSkilCount))] private int ghostSkillCount;
    [SerializeField] private Text ghostSKillCountText;

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
        UpdateRule();
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

    public void UpdateRule()
    {
        maxBombTimeText.text = GameRuleStore.Instance.CurGameRule.maxBombTime.ToString();
        minBombTimeText.text = GameRuleStore.Instance.CurGameRule.minBombTime.ToString();
        scorePerRoundText.text = GameRuleStore.Instance.CurGameRule.roundWinningPoint.ToString();
        ghostSKillCountText.text = GameRuleStore.Instance.CurGameRule.ghostSkillCount.ToString();
    }

    public void OnChangeMaxBombTime(int _, int value)
    {
        maxBombTimeText.text = value.ToString();
        GameRuleStore.Instance.SetMaxBombTime(value);
    }

    public void OnMaxBombTime(bool isPlus)
    {
        maxBombTime = Mathf.Clamp(maxBombTime + (isPlus ? 5 : -5), 80, 100);
    }

    public void OnChangeMinBombTime(int _, int value)
    {
        minBombTimeText.text = value.ToString();
        GameRuleStore.Instance.SetMinBombTime(value);
    }

    public void OnMinBombTime(bool isPlus)
    {
        minBombTime = Mathf.Clamp(minBombTime + (isPlus ? 5 : -5), 60, 80);
    }

    public void OnChangeScorePerRound(int _, int value)
    {
        scorePerRoundText.text = value.ToString();
        GameRuleStore.Instance.SetScorePerRound(value);
    }

    public void OnScorePerRound(bool isPlus)
    {
        scorePerRound = Mathf.Clamp(scorePerRound + (isPlus ? 1 : -1), 3, 6);
    }

    public void OnChangeGhostSkilCount(int _, int value)
    {
        ghostSKillCountText.text = value.ToString();
        GameRuleStore.Instance.SetGhostSkillCount(value);
    }

    public void OnGhostSKillCount(bool isPlus)
    {
        ghostSkillCount = Mathf.Clamp(ghostSkillCount + (isPlus ? 1 : -1), 0, 3);
    }
}