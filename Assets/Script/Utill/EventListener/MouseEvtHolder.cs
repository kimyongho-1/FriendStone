using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class MouseEvtHolder : MonoBehaviour, IPointerDownHandler 
    , IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public Action<GameObject> mClickL, mClickR, mStartDrag, mEnter, mExit,mDrag, mEndDrag;

    public void OnPointerDown(PointerEventData eventData)
    {
        
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                GAME.Manager.SM.PlayBtn(Define.Mouse.ClickL);
                mClickL?.Invoke(this.gameObject);  break;
            case PointerEventData.InputButton.Right:
                GAME.Manager.SM.PlayBtn(Define.Mouse.ClickR);
                mClickR?.Invoke(this.gameObject); break;
            default: break;
        }
    }

  
    public void OnBeginDrag(PointerEventData eventData)
    {
      //  Debug.Log("Begin Drag");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
     //   Debug.Log("End Drag");
    }

    public void OnDrag(PointerEventData eventData)
    {
     //   Debug.Log("Dragging");
    }
}
