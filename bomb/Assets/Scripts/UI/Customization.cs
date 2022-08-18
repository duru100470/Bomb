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
        EyeGlass,
        Crown,
        None
    }

    public enum Body_Parts
    {
        None
    }

    [SerializeField] GameObject PreviewObject;
    [SerializeField] RectTransform Panel_Customize;
    [SerializeField] RectTransform Panel_head_parts;
    [SerializeField] RectTransform Panel_body_parts;
    [SerializeField] List<GameObject> Head_partsList;
    [SerializeField] List<GameObject> Body_partsList;

    [SerializeField] Button Button_head;
    [SerializeField] Button Button_body;

    [SerializeField] GameObject selectionButtonPrefab;

    private Dictionary<int, GameObject> idxToParent = new Dictionary<int, GameObject>();
    private Dictionary<int, List<GameObject>> idxToTargetObject = new Dictionary<int, List<GameObject>>();
    [SerializeField] GameObject PreviewHead;
    public int curPartsIdx;

    private void Start()
    {
        idxToParent.Add(0, PreviewHead);
        idxToTargetObject.Add(0, Head_partsList);

        SetupCustomButton(Head_partsList, Panel_head_parts);
        SetupCustomButton(Body_partsList, Panel_body_parts);

        Button_head.onClick.AddListener(OnClickHeadButton);
        Button_body.onClick.AddListener(OnClickBodyButton);
    }

    public void EnterCustomization()
    {
        Panel_Customize.gameObject.SetActive(true);
        PreviewObject.SetActive(true);
    }

    public void SetupCustomButton(List<GameObject> list, RectTransform parent)
    {

        foreach(var item in list)
        {
            GameObject obj = Instantiate(selectionButtonPrefab, Vector3.zero, Quaternion.identity, parent);
            obj.GetComponent<Image>().sprite = item.GetComponent<SpriteRenderer>().sprite;
            obj.GetComponent<ButtonCustomization>().manager = this;
            obj.GetComponent<ButtonCustomization>().index = list.IndexOf(item);
        }
        
        GameObject temp = Instantiate(selectionButtonPrefab, Vector3.zero, Quaternion.identity, parent);
        temp.GetComponent<ButtonCustomization>().manager = this;
        temp.GetComponent<ButtonCustomization>().index = list.Count + 1;
    }

    public void UpdatePreviewModel()
    {
        //PreviewHead = transform.Find("Head").gameObject;
    }
    
    public void ApplyCustom(int index)
    {
        ResetCurCustomObj();
        PlayerSetting.customState[curPartsIdx] = index;
        if(index > idxToTargetObject[curPartsIdx].Count)
        {
            return;
        } 
        GameObject obj = idxToTargetObject[curPartsIdx][index];
        obj.SetActive(true);
    }

    public void ResetCurButton()
    {
        Panel_head_parts.gameObject.SetActive(false);
        Panel_body_parts.gameObject.SetActive(false);
    }

    public void ResetCurCustomObj()
    {
        foreach(var item in idxToTargetObject[curPartsIdx])
        {
            item.SetActive(false);
        }
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
