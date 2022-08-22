using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

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
    [SerializeField] Button Button_back;

    [SerializeField] GameObject selectionButtonPrefab;

    private Dictionary<int, SpriteRenderer> idxToParent = new Dictionary<int, SpriteRenderer>();
    private Dictionary<int, List<Sprite>> idxToTargetSprite = new Dictionary<int, List<Sprite>>();
    [SerializeField] SpriteRenderer PreviewFace;
    [SerializeField] SpriteRenderer PreviewHead;
    [SerializeField] SpriteRenderer PreviewBody;
    [SerializeField] SpriteRenderer PreviewLeftArm;
    [SerializeField] SpriteRenderer PreviewRightLeg;
    [SerializeField] SpriteRenderer PreviewLeftLeg;
    [SerializeField] SpriteRenderer PreviewRightArm;
    [SerializeField] SpriteRenderer PreviewTail;

    [SerializeField] SpriteRenderer HeadCustom; 
    [SerializeField] SpriteRenderer BodyCustom;

    public int curPartsIdx;
    public bool isActive = false;
    public RoomPlayer localPlayer;

    private void Start()
    {
        idxToParent.Add(0, HeadCustom);
        idxToParent.Add(1, BodyCustom);

        idxToTargetSprite.Add(0, Head_partsList);
        idxToTargetSprite.Add(1, Body_partsList);

        SetupCustomButton(Head_partsList, Panel_head_parts);
        SetupCustomButton(Body_partsList, Panel_body_parts);

        Button_head.onClick.AddListener(OnClickHeadButton);
        Button_body.onClick.AddListener(OnClickBodyButton);
        Button_back.onClick.AddListener(OnClickBackButton);

        PlayerSetting.customState[0] = (int)Head_Parts.None;
        PlayerSetting.customState[1] = (int)Body_Parts.None;
    }

    public void EnterCustomization()
    {
        Panel_Customize.gameObject.SetActive(true);
        PreviewObject.SetActive(true);
        isActive = true;
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

    public void UpdatePreviewModel()
    {
        PreviewFace.sprite = localPlayer.transform.GetChild(0).Find("Face").GetComponent<SpriteRenderer>().sprite;
        PreviewHead.sprite = localPlayer.transform.GetChild(0).Find("Head").GetComponent<SpriteRenderer>().sprite;
        PreviewBody.sprite = localPlayer.transform.GetChild(0).Find("Body").GetComponent<SpriteRenderer>().sprite;
        PreviewLeftArm.sprite = localPlayer.transform.GetChild(0).Find("Left arm").GetComponent<SpriteRenderer>().sprite;
        PreviewRightLeg.sprite = localPlayer.transform.GetChild(0).Find("Right leg").GetComponent<SpriteRenderer>().sprite;
        PreviewLeftLeg.sprite = localPlayer.transform.GetChild(0).Find("Left leg").GetComponent<SpriteRenderer>().sprite;
        PreviewRightArm.sprite = localPlayer.transform.GetChild(0).Find("Right arm").GetComponent<SpriteRenderer>().sprite;
        PreviewTail.sprite = localPlayer.transform.GetChild(0).Find("Tail").GetComponent<SpriteRenderer>().sprite;
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

    public void OnClickBackButton()
    {
        isActive = false;
        PreviewObject.SetActive(false);
        Panel_Customize.gameObject.SetActive(false);
    }
}
