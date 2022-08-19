using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Customization : MonoBehaviour
{
    public enum Parts
    {
        Head,
        Body,
        Length
    }

    public enum Head_Parts
    {
        Crown,
        EyeGlass,
        None
    }

    public enum Body_Parts
    {
        Crown,
        None
    }

    [SerializeField] GameObject PreviewObject;
    [SerializeField] RectTransform Panel_Customize;
    [SerializeField] RectTransform Panel_head_parts;
    [SerializeField] RectTransform Panel_body_parts;
    [SerializeField] List<Sprite> Head_partsList;
    [SerializeField] List<Sprite> Body_partsList;

    [SerializeField] Button Button_head;
    [SerializeField] Button Button_body;

    [SerializeField] GameObject selectionButtonPrefab;

    private Dictionary<int, SpriteRenderer> idxToParent = new Dictionary<int, SpriteRenderer>();
    private Dictionary<int, List<Sprite>> idxToTargetSprite = new Dictionary<int, List<Sprite>>();
    [SerializeField] SpriteRenderer PreviewHead;
    [SerializeField] SpriteRenderer PreviewBody;
    public int curPartsIdx;

    private void Start()
    {
        idxToParent.Add(0, PreviewHead);
        idxToParent.Add(1, PreviewBody);

        idxToTargetSprite.Add(0, Head_partsList);
        idxToTargetSprite.Add(1, Body_partsList);

        SetupCustomButton(Head_partsList, Panel_head_parts);
        SetupCustomButton(Body_partsList, Panel_body_parts);

        Button_head.onClick.AddListener(OnClickHeadButton);
        Button_body.onClick.AddListener(OnClickBodyButton);

        PlayerSetting.customState[0] = (int)Head_Parts.None;
        PlayerSetting.customState[1] = (int)Body_Parts.None;
    }

    public void EnterCustomization()
    {
        Panel_Customize.gameObject.SetActive(true);
        PreviewObject.SetActive(true);
    }

    public void SetupCustomButton(List<Sprite> list, RectTransform parent)
    {
        foreach(var item in list)
        {
            GameObject obj = Instantiate(selectionButtonPrefab, Vector3.zero, Quaternion.identity, parent);
            obj.GetComponent<Image>().sprite = item;
            obj.GetComponent<ButtonCustomization>().manager = this;
            obj.GetComponent<ButtonCustomization>().index = list.IndexOf(item);
        }
        
        GameObject temp = Instantiate(selectionButtonPrefab, Vector3.zero, Quaternion.identity, parent);
        temp.GetComponent<ButtonCustomization>().manager = this;
        temp.GetComponent<ButtonCustomization>().index = list.Count;
    }

    public void UpdatePreviewModel()
    {
        //PreviewHead = transform.Find("Head").gameObject;
    }
    
    public void ApplyCustom(int index)
    {
        PlayerSetting.customState[curPartsIdx] = index;
        idxToParent[curPartsIdx].sprite = null;
        if(index == idxToTargetSprite[curPartsIdx].Count)
        {
            return;
        } 
        idxToParent[curPartsIdx].sprite = idxToTargetSprite[curPartsIdx][index];
    }

    public void ResetCurButton()
    {
        Panel_head_parts.gameObject.SetActive(false);
        Panel_body_parts.gameObject.SetActive(false);
    }

    public void OnClickHeadButton()
    {
        ResetCurButton();
        Panel_head_parts.gameObject.SetActive(true);
        curPartsIdx = (int)Parts.Head;
    }

    public void OnClickBodyButton()
    {
        ResetCurButton();
        Panel_body_parts.gameObject.SetActive(true);
        curPartsIdx = (int)Parts.Body;
    }

}
