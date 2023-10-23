using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using static Define;
using Unity.VisualScripting;

public class UIpopupHolder: MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{ 
    IEnumerator Co;
    float time; // 커서 대기시간
    Vector3 pos, scale;
    string text;
    public void Init(float t1, Define.PopupScale s , string t2, Vector3 p)
    {
        time = t1;
        switch (s)
        {
            case PopupScale.Small:
                scale = GAME.Manager.UM.Size ;
                break;
            case PopupScale.Medium:
                scale = GAME.Manager.UM.Size * 2f;
                break;
            case PopupScale.Big:
                scale = GAME.Manager.UM.Size * 3f;
                break;
        }
        
        pos = p;
        text = t2;
    }

    public void OnDisable()
    { StopAllCoroutines(); GAME.Manager.UM.popup.gameObject.SetActive(false); }

    // 대기후에 안내팝업창 호출
    IEnumerator waitCo()
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            yield return null;
        }

        GAME.Manager.UM.ShowPopup(pos, scale, text);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Co != null) { StopCoroutine(Co); Co = null; }
        Co = waitCo();
        StartCoroutine(Co);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GAME.Manager.UM.popup.gameObject.SetActive(false);
        if (Co != null) { StopCoroutine(Co); Co = null; }
    }

    // 클릭입력시 안내창은 강제 종료 (대기코루틴또한 강제종료)
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Downw");
        OnPointerExit(eventData);
    }
}
