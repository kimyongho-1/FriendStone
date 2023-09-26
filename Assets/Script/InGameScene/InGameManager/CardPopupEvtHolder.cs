using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class CardPopupEvtHolder : MonoBehaviour
    , IPointerEnterHandler, IPointerExitHandler
{
    public  Action func;
    IEnumerator Co;
    public float time; // 커서 대기시간

    // 대기후에 안내팝업창 호출
    IEnumerator waitCo()
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            yield return null;
        }
        // Exit호출 없을시, 대기시간 지난후에 카드팝업창 호출
        func?.Invoke(); 
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Co != null) { StopCoroutine(Co); Co = null; }
        Co = waitCo();
        StartCoroutine(Co);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Co != null) { StopCoroutine(Co); Co = null; }
        GAME.IGM.cardPopup.gameObject.SetActive(false);
    }

}
