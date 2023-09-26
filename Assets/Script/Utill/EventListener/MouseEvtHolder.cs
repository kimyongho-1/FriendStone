using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class MouseEvtHolder : MonoBehaviour, IPointerDownHandler 
    , IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler ,IPointerExitHandler
{
    public Action<GameObject> mClickL, mClickR, mEnter, mExit;
    public Action<Vector3> mStartDrag, mDrag, mEndDrag;
    public Define.Sound sound, dragSound;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (sound != Define.Sound.None)
        { GAME.Manager.SM.PlaySound(sound); }
        
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                mClickL?.Invoke(this.gameObject);  break;
            case PointerEventData.InputButton.Right:
                mClickR?.Invoke(this.gameObject); break;
            default: break;
        }
    }

  
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (dragSound != Define.Sound.None)
        { GAME.Manager.SM.PlaySound(dragSound); }
        mStartDrag?.Invoke(default);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        mEndDrag?.Invoke(default); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 newPosition = Camera.main.ScreenToWorldPoint
            (new Vector3(Input.mousePosition.x, Input.mousePosition.y, +6.5f));
        mDrag?.Invoke(newPosition);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mEnter?.Invoke(this.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mExit?.Invoke(this.gameObject);
    }
}
