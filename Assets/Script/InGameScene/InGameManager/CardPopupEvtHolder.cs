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
    public float time; // Ŀ�� ���ð�

    // ����Ŀ� �ȳ��˾�â ȣ��
    IEnumerator waitCo()
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            yield return null;
        }
        // Exitȣ�� ������, ���ð� �����Ŀ� ī���˾�â ȣ��
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
