using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class UIManager 
{

    #region 안내팝업창 관련
    public RectTransform popup;
    public TextMeshProUGUI Text;
    public Vector3 Size = new Vector3(1.5f, 1f, 1f); // 안내창 기본 사이즈
    // 생성자
    public UIManager(RectTransform rt, TextMeshProUGUI t)
    {
        //참조
        popup = rt; Text = t;
        rt.gameObject.SetActive(false);
    }
    public void ShowPopup(Vector3 p , Vector3 s, string t)
    {
        popup.localScale = s;
        popup.anchoredPosition = p;
        Text.text = t;

        popup.gameObject.SetActive(true);
    }

    #endregion

    // 커서를 갖다대면 일정 ms후 안내팝업창이 주변에 뜨는 이벤트 연결
    public void BindUIPopup(GameObject go, float t, Vector3 pos, Define.PopupScale scale,
        string text)
    {
        // 띄울 팝업창의 크기, 위치 지정
        UIpopupHolder uh = go.AddComponent<UIpopupHolder>();
            uh.Init(t, scale, text, pos);
    }

    // TMP TEXT를 버튼으로 사용하고, 이벤트함수 호출도 동시에 처리하려 만든 함수
    public void BindTMPInteraction(TextMeshProUGUI tmp , Color enter, Color pressed, Action<TextMeshProUGUI> func)
    {
        tmp.AddComponent<TextButtonHolder>().Init(ref tmp,enter,pressed ,func) ;
    }

    // 기타 ui요소에 마우스 작용시 이벤트 발생시키는 함수
    public void BindEvent(GameObject go, Action<GameObject> func, Define.Mouse mouse)
    {
        if (go.TryGetComponent<MouseEvtHolder>(out MouseEvtHolder meh) == false)
        {
            meh = go.AddComponent<MouseEvtHolder>();
        }
        switch (mouse) 
        {
            case Define.Mouse.ClickL:
                meh.mClickL = func; break;
            case Define.Mouse.ClickR:
                meh.mClickR = func; break;
            case Define.Mouse.Enter:
                meh.mEnter = func; break;
            case Define.Mouse.Exit:
                meh.mExit = func; break;
        }
    }
    public void BindEvent(GameObject go, Action<Vector3> func, Define.Mouse mouse)
    {
        if (go.TryGetComponent<MouseEvtHolder>(out MouseEvtHolder meh) == false)
        {
            meh = go.AddComponent<MouseEvtHolder>();
        }

        switch (mouse)
        {
            case Define.Mouse.StartDrag:
                meh.mStartDrag = func; break;
            case Define.Mouse.Dragging:
                meh.mDrag = func; break;
            case Define.Mouse.EndDrag:
                meh.mEndDrag = func; break;
        }
    }
    // 게임씬에서, 카드들에 커서 일정시간 가져다 댈시 안내팝업창 호출 이벤트 연결
    public void BindCardPopupEvent(GameObject go, Action func, float waitTime)
    {
        CardPopupEvtHolder cpeh = null; ;
        if (go.TryGetComponent<CardPopupEvtHolder>(out CardPopupEvtHolder meh) == false)
        {
            cpeh = go.AddComponent<CardPopupEvtHolder>();
        }
        cpeh.func = func;
        cpeh.time = waitTime;
    }
}
