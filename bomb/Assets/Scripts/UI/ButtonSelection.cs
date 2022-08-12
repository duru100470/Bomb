using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class ButtonSelection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private RectTransform Button;
    private Animator anim;
    private void Start()
    {
        Button = GetComponent<RectTransform>();
        anim = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        anim.Play("HoverOn");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        anim.Play("HoverOut");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance.PlayUISound(AudioType.ButtonClick);
        Button.localScale = new Vector3(1f,1f,1f);
    }
}
