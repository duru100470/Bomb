using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeySelector : MonoBehaviour
{
    [SerializeField] OptionControl optionControl;
    [SerializeField] Text KeyName;
    [SerializeField] Text CurKeyCode;
    [SerializeField] public KeyCode matchKey;
    [SerializeField] PlayerSetting.BindKeys thisKey;

    public void Start()
    {
        UpdateCurKey();
    }

    public void UpdateKeyBinds()
    {
        PlayerSetting.keyDict[thisKey] = matchKey;
    }

    public void UpdateCurKey()
    {
        KeyName.text = thisKey.ToString();
        CurKeyCode.text = matchKey.ToString();
    }

    public void OnClickKeyBind()
    {
        optionControl.Panel_KeySelecting.gameObject.SetActive(true);
        optionControl.curKey = thisKey;
        optionControl.curKeySelector = this;
    }

}
