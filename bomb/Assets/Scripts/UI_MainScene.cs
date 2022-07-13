using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Net;
using System.Net.Sockets;
public class UI_MainScene : MonoBehaviour
{
    RoomManager manager;

    [SerializeField] Button playButton;
    [SerializeField] Button tutorialButton;
    [SerializeField] Button optionButton;
    [SerializeField] Button exitButton;
    [SerializeField] RectTransform playPanel;
    [SerializeField] RectTransform tutorialPanel;
    [SerializeField] RectTransform optionPanel;
    [SerializeField] InputField joinMatchInput;
    [SerializeField] InputField playerNickname;
    [SerializeField] Button joinButton;
    [SerializeField] Button hostButton;
    [SerializeField] Button playQuitButton;
    [SerializeField] Button tutorialBeforeButton;
    [SerializeField] Button tutorialAfterButton;
    [SerializeField] Button tutorialQuitButton;
    [SerializeField] Image tutorialImage;
    [SerializeField] Button optionQuitButton;
    [SerializeField] private List<Sprite> tutorialList = new List<Sprite>();
    [SerializeField] RectTransform nicknameEmptyCaution;

    private int tutorialIdx = 0;

    void Start(){
        manager = NetworkManager.singleton as RoomManager;
        joinMatchInput.text = "localhost";
    }

    #region Main

    public void Play(){
        playPanel.gameObject.SetActive(true);
    }

    public void Tutorial(){
        tutorialPanel.gameObject.SetActive(true);
        if(tutorialList.Count > 0){
            tutorialIdx = 0;
            tutorialImage.sprite = tutorialList[tutorialIdx];
        }
    }

    public void Option(){
        optionPanel.gameObject.SetActive(true);
    }

    public void Exit(){
        Application.Quit();
    }

    #endregion Main

    #region Panel_Play
    public void Host(){
        if(!playerNickname.text.Equals(string.Empty)){

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    PlayerSetting.hostIP = ip.ToString();
            }
            
            PlayerSetting.playerNickname = playerNickname.text;
            NetworkManager.singleton.StartHost();
        }
        else{
            StartCoroutine(EmptyNicknameCaution());
        }
    }

    public void Join(){
        if(!playerNickname.text.Equals(string.Empty)){
            PlayerSetting.playerNickname = playerNickname.text;
            NetworkManager.singleton.networkAddress = joinMatchInput.text;
            NetworkManager.singleton.StartClient();
        }
        else{
            StartCoroutine(EmptyNicknameCaution());
        }
    }


    public void PlayQuit(){
        playPanel.gameObject.SetActive(false);
    }
    #endregion Panel_Play

    #region Panel_Tutorial

    public void TutoQuit(){
        tutorialPanel.gameObject.SetActive(false);
    }

    public void Tuto_Before(){
        if(tutorialIdx > 0){
            tutorialIdx--;
            tutorialImage.sprite = tutorialList[tutorialIdx];
        }
    }

    public void Tuto_After(){
        if(tutorialIdx < tutorialList.Count-1){
            tutorialIdx++;
            tutorialImage.sprite = tutorialList[tutorialIdx];
        }
    }

    #endregion Panel_Tutorial

    #region Panel_Option

    public void optionQuit(){
        optionPanel.gameObject.SetActive(false);
    }

    #endregion Panel_Option

    #region Caution

    private IEnumerator EmptyNicknameCaution(){
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
}
