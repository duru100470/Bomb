using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionControl : MonoBehaviour
{
    [SerializeField] Toggle fullScreenToggle;
    [SerializeField] Slider MasterVolume;
    [SerializeField] Slider BGMVolume;
    [SerializeField] Slider VFXVolume;
    
    private void Start()
    {
        MasterVolume.onValueChanged.AddListener( delegate {OnChangeMasterVolume();});
        BGMVolume.onValueChanged.AddListener( delegate {OnChangeBGMVolume();});
        VFXVolume.onValueChanged.AddListener( delegate {OnChangeVFXVolume();});
        fullScreenToggle.onValueChanged.AddListener(OnChangeFullScreen);
    }

    public void OnChangeMasterVolume()
    {
        SoundManager.Instance.SetMasterVolume(MasterVolume.value);
    }

    public void OnChangeBGMVolume()
    {
        SoundManager.Instance.SetBGMVolume(BGMVolume.value);
    }

    public void OnChangeVFXVolume()
    {
        SoundManager.Instance.SetVFXVolume(VFXVolume.value);
    }

    public void OnChangeFullScreen(bool value)
    {
        Screen.fullScreen = value;
    }
}

