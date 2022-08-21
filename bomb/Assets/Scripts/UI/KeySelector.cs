using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeySelector : MonoBehaviour
{
    [SerializeField] OptionControl optionControl;
    [SerializeField] Text KeyName;
    [SerializeField] Text CurKeyCode;
    [SerializeField] KeyCode matchKey;
    [SerializeField] public PlayerSetting.BindKeys thisKey;

    public void Start()
    {
        UpdateCurKey();
    }

    public void UpdateCurKey()
    {
        KeyName.text = thisKey.ToString();
        CurKeyCode.text = PlayerSetting.keyList[(int)thisKey].ToString();
    }

    public void OnClickKeyBind()
    {
        optionControl.Panel_KeySelecting.gameObject.SetActive(true);
        optionControl.curKey = thisKey;
        optionControl.curKeySelector = this;
    }

}
