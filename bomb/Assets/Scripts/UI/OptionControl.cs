using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
    [SerializeField] Button Button_ResetKeyBind;
    [SerializeField] GameObject KeySelectors;
    private KeySelector[] selectors;

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

        Button_JumpKey.onClick.AddListener(OnClickKeySetButton);
        Button_CastKey.onClick.AddListener(OnClickKeySetButton);
        Button_DropKey.onClick.AddListener(OnClickKeySetButton);
        Button_PushKey.onClick.AddListener(OnClickKeySetButton);

        Button_ResetKeyBind.onClick.AddListener(OnClickResetKeyBind);

        selectors = KeySelectors.GetComponentsInChildren<KeySelector>();
    }

    public void Update()
    {
        if(Panel_KeySelecting.gameObject.activeInHierarchy && !Input.GetKey(KeyCode.Escape))
        {       
            foreach(var input in PlayerSetting.AvailKeys)
            {
                if(Input.GetKeyDown(input) && !PlayerSetting.keyList.Contains(input))
                {
                    PlayerSetting.keyList[(int)curKey] = input;
                    curKeySelector.UpdateCurKey();
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

    public void OnClickResetKeyBind()
    {
        PlayerSetting.keyList = PlayerSetting.originKey.ToList();
        foreach(var item in selectors)
        {
            item.UpdateCurKey();
        }
    }
}

