using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class TextButtonHolder : MonoBehaviour, IPointerDownHandler, IPointerClickHandler ,IPointerEnterHandler, IPointerExitHandler
{
    public Action<TextMeshProUGUI> mClick;
    public Color enterColor,pressedColor;
    TextMeshProUGUI tmp;
    Color baseColor;
    private void OnEnable()
    {
        if (tmp != null)
        tmp.color = Color.black;
    }
    public void Init(ref TextMeshProUGUI tmpEle, Color enter, Color pressed, Action<TextMeshProUGUI> func)
    {
        mClick = func;
        tmp = tmpEle;
        baseColor = Color.black;
        enterColor = enter;
        pressedColor =pressed;  
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        GAME.Manager.SM.PlaySound(Define.Sound.Click);
        tmp.color = pressedColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        mClick?.Invoke(tmp);
        tmp.color = baseColor;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        tmp.color = enterColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tmp.color = baseColor;
    }

}
