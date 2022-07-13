using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UI_Lobby : MonoBehaviour
{
    [SerializeField] Text hostIP;

    public void Start(){
        DontDestroyOnLoad(this);
    }

    public void SetIPPoint(){
        hostIP.text = PlayerSetting.hostIP;
    }

}
