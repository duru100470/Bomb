using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UI_MainScene : MonoBehaviour
{
    [SerializeField] Button playButton;
    [SerializeField] Button tutorialButton;
    [SerializeField] Button optionButton;
    [SerializeField] Button exitButton;
    [SerializeField] RectTransform playPanel;
    [SerializeField] RectTransform tutorialPanel;
    [SerializeField] RectTransform optionPanel;
    [SerializeField] InputField joinMatchInput;
    [SerializeField] Button joinButton;
    [SerializeField] Button hostButton;
    [SerializeField] Button playQuitButton;
    [SerializeField] Button tutorialBeforeButton;
    [SerializeField] Button tutorialAfterButton;
    [SerializeField] Button tutorialQuitButton;
    [SerializeField] Image tutorialImage;
    [SerializeField] Button optionQuitButton;
    [SerializeField] private List<Sprite> tutorialList = new List<Sprite>();
    private int tutorialIdx = 0;

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
        NetworkManager.singleton.StartHost();
    }

    public void Join(){
        NetworkManager.singleton.networkAddress = joinMatchInput.text;
        NetworkManager.singleton.StartClient();
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
}
