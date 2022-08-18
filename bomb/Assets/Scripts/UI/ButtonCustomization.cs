using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonCustomization : MonoBehaviour
{
    Button button;
    public Customization manager;
    public int index;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        manager.ApplyCustom(index);
    }
}
