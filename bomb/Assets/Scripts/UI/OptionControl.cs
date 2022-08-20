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
    [SerializeField] public RectTransform Panel_KeySelecting;
    [SerializeField] Button Button_JumpKey;
    [SerializeField] Button Button_CastKey;
    [SerializeField] Button Button_DropKey;
    [SerializeField] Button Button_PushKey;
    public PlayerSetting.BindKeys curKey;
    public KeyCode curKeyCode;
    public KeySelector curKeySelector;
    private void Start()
    {
        MasterVolume.onValueChanged.AddListener( delegate {OnChangeMasterVolume();});
        BGMVolume.onValueChanged.AddListener( delegate {OnChangeBGMVolume();});
        VFXVolume.onValueChanged.AddListener( delegate {OnChangeVFXVolume();});
        fullScreenToggle.onValueChanged.AddListener(OnChangeFullScreen);

        MasterVolume.value = SoundManager.Instance.MasterVolume;
        BGMVolume.value = SoundManager.Instance.BGMVolume;
        VFXVolume.value = SoundManager.Instance.VFXVolume;

        PlayerSetting.keyDict.Add(PlayerSetting.BindKeys.Jump, PlayerSetting.JumpKey);
        PlayerSetting.keyDict.Add(PlayerSetting.BindKeys.Cast, PlayerSetting.CastKey);
        PlayerSetting.keyDict.Add(PlayerSetting.BindKeys.Drop, PlayerSetting.DropKey);
        PlayerSetting.keyDict.Add(PlayerSetting.BindKeys.Push, PlayerSetting.PushKey);

        Button_JumpKey.onClick.AddListener(OnClickKeySetButton);
        Button_CastKey.onClick.AddListener(OnClickKeySetButton);
        Button_DropKey.onClick.AddListener(OnClickKeySetButton);
        Button_PushKey.onClick.AddListener(OnClickKeySetButton);

        PlayerSetting.AvailKeys.Add(KeyCode.Q);
        PlayerSetting.AvailKeys.Add(KeyCode.W);
        PlayerSetting.AvailKeys.Add(KeyCode.E);
        PlayerSetting.AvailKeys.Add(KeyCode.R);
        PlayerSetting.AvailKeys.Add(KeyCode.T);
        PlayerSetting.AvailKeys.Add(KeyCode.S);
        PlayerSetting.AvailKeys.Add(KeyCode.F);
        PlayerSetting.AvailKeys.Add(KeyCode.Space);
        PlayerSetting.AvailKeys.Add(KeyCode.G);
    }

    public void Update()
    {
        if(Panel_KeySelecting.gameObject.activeInHierarchy)
        {
            foreach(var input in PlayerSetting.AvailKeys)
            {
                if(Input.GetKeyDown(input) && !PlayerSetting.keyDict.ContainsValue(input))
                {
                    curKeySelector.matchKey = input;
                    curKeySelector.UpdateCurKey();
                    curKeySelector.UpdateKeyBinds();
                    Panel_KeySelecting.gameObject.SetActive(false);
                    break;
                }
            }
        }
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

    public void OnClickKeySetButton()
    {
        Panel_KeySelecting.gameObject.SetActive(true);
    }

}

