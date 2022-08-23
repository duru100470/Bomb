using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;
using System.Net;
using System.Net.Sockets;
using System;
public class UI_MainScene : MonoBehaviour
{
    RoomManager manager;

    [Header("Main Buttons")]
    [SerializeField] GameObject MainButtons;
    [SerializeField] Button playButton;
    [SerializeField] Button tutorialButton;
    [SerializeField] Button optionButton;
    [SerializeField] Button exitButton;

    [Header ("Play")]
    [SerializeField] RectTransform playPanel;
    [SerializeField] Button joinButton;
    [SerializeField] Button hostButton;
    [SerializeField] Button playQuitButton;
    [SerializeField] InputField joinMatchInput;
    [SerializeField] InputField playerNickname;

    [Header("Tutorial")]
    [SerializeField] RectTransform tutorialPanel;
    [SerializeField] Button tutorialBeforeButton;
    [SerializeField] Button tutorialAfterButton;
    [SerializeField] Button tutorialQuitButton;
    [SerializeField] Image tutorialImage;
    [SerializeField] private List<Sprite> tutorialList = new List<Sprite>();

    [Header("Option")]
    [SerializeField] RectTransform optionPanel;
    [SerializeField] Button optionQuitButton;

    [Header("ETC")]
    [SerializeField] RectTransform nicknameEmptyCaution;
    [SerializeField] Image transitionPanel;
    [SerializeField] private float transitionTime = 1f;
    
    [SerializeField] private int tutorialIdx = 0;
    private bool transitionFlag = true;

    void Start()
    {
        
        manager = NetworkManager.singleton as RoomManager;
        //joinMatchInput.text = "localhost";

        playButton.onClick.AddListener(Play);
        tutorialButton.onClick.AddListener(Tutorial);
        optionButton.onClick.AddListener(Option);
        exitButton.onClick.AddListener(Exit);

        joinButton.onClick.AddListener(Join);
        hostButton.onClick.AddListener(Host);
        playQuitButton.onClick.AddListener(PlayQuit);
        tutorialBeforeButton.onClick.AddListener(Tuto_Before);
        tutorialAfterButton.onClick.AddListener(Tuto_After);
        tutorialQuitButton.onClick.AddListener(TutoQuit);
        optionQuitButton.onClick.AddListener(OptionQuit);

        SoundManager.Instance.PlayBGM(AudioType.MainBGM);
    }

    #region Main

    public void Play()
    {
        StartCoroutine(Transition());
        playPanel.gameObject.SetActive(true);
        MainButtons.SetActive(false);
    }

    public void Tutorial()
    {
        StartCoroutine(Transition());
        tutorialPanel.gameObject.SetActive(true);
        MainButtons.SetActive(false);
        if(tutorialList.Count > 0)
        {
            tutorialIdx = 0;
            tutorialImage.sprite = tutorialList[tutorialIdx];
        }
    }

    public void Option()
    {
        StartCoroutine(Transition());
        optionPanel.gameObject.SetActive(true);
        MainButtons.SetActive(false);
    }

    public void Exit()
    {
        Application.Quit();
    }

    #endregion Main

    #region Panel_Play
    public void Host()
    {
        if(!playerNickname.text.Equals(string.Empty))
        {

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    PlayerSetting.hostIP = ip.ToString();
            }
            
            PlayerSetting.playerNickname = playerNickname.text;
            NetworkManager.singleton.StartHost();
        }
        else
        {
            StartCoroutine(EmptyNicknameCaution());
        }
    }

    public void Join()
    {
        if(!playerNickname.text.Equals(string.Empty))
        {
            PlayerSetting.playerNickname = playerNickname.text;
            NetworkManager.singleton.networkAddress = Decrypt(joinMatchInput.text);
            NetworkManager.singleton.StartClient();
        }
        else
        {
            StartCoroutine(EmptyNicknameCaution());
        }
    }

    public void PlayQuit()
    {
        StartCoroutine(Transition());
        playPanel.gameObject.SetActive(false);
        MainButtons.SetActive(true);
    }
    #endregion Panel_Play

    #region Panel_Tutorial

    public void TutoQuit()
    {
        StartCoroutine(Transition());
        tutorialPanel.gameObject.SetActive(false);
        MainButtons.SetActive(true);
    }

    public void Tuto_Before()
    {
        if(tutorialIdx > 0){
            tutorialIdx--;
            tutorialImage.sprite = tutorialList[tutorialIdx];
        }
    }

    public void Tuto_After()
    {
        if(tutorialIdx < tutorialList.Count-1){
            tutorialIdx++;
            tutorialImage.sprite = tutorialList[tutorialIdx];
        }
    }

    #endregion Panel_Tutorial

    #region Panel_Option

    public void OptionQuit()
    {
        StartCoroutine(Transition());
        optionPanel.gameObject.SetActive(false);
        MainButtons.SetActive(true);
    }

    #endregion Panel_Option

    #region Caution

    private IEnumerator EmptyNicknameCaution()
    {
        nicknameEmptyCaution.gameObject.SetActive(true);
        Text cautionText = nicknameEmptyCaution.GetComponentInChildren<Text>();
        Image cautionImage = nicknameEmptyCaution.GetComponent<Image>();
        float time = 1f;
        cautionText.color = new Color(0f, 0f, 0f, time);
        cautionImage.color = new Color(1f, 1f, 1f, time);       
        yield return new WaitForSeconds(.5f);
        while(time > 0f){
            cautionText.color = new Color(0f, 0f, 0f, time);
            cautionImage.color = new Color(1f, 1f, 1f, time);
            time -= Time.deltaTime;
            yield return null;
        }
        nicknameEmptyCaution.gameObject.SetActive(false);
    }

    #endregion Caution

    private IEnumerator Transition()
    {
        transitionFlag = false;
        yield return null;
        transitionFlag = true;
        float curTime = 0f;
        transitionPanel.color = new Color(0f, 0f, 0f, 1f);
        while(curTime < transitionTime && transitionFlag)
        {
            transitionPanel.color = new Color(0f, 0f, 0f, 1 - curTime/transitionTime);
            curTime += Time.deltaTime;
            yield return null;
        }
        yield break;
    }

     public string Decrypt(string input)
    {
        string ret = String.Empty;
        string str = CheckCapital(input);
        for(int i=0; i< str.Length/2; i++)
        {
            string cur = str.Substring(i*2, 2);
            int first = cur[0] >= 'A' ? cur[0] - 'A' + 10 : cur[0] - '0';
            int second = cur[1] >= 'A' ? cur[1] - 'A' + 10 : cur[1] - '0';
            int intValue = first * 16 + second;
            ret += intValue;
            if(i != str.Length/2 -1) ret += ".";
        }
        return ret;
    }

    public string CheckCapital(string str)
    {
        string ret = String.Empty;
        for(int i=0; i<str.Length; i++)
        {
            char cur = str[i];
            if(cur <= 'z' && cur >= 'a') cur = (char)(cur + 'A' - 'a');
            ret += cur.ToString();
        }
        return ret;
    }
}
