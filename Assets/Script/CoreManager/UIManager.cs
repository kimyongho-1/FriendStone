using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class UIManager 
{

    #region �ȳ��˾�â ����
    public RectTransform popup;
    public TextMeshProUGUI Text;
    public Vector3 Size = new Vector3(1.5f, 1f, 1f); // �ȳ�â �⺻ ������
    // ������
    public UIManager(RectTransform rt, TextMeshProUGUI t)
    {
        //����
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

    // Ŀ���� ���ٴ�� ���� ms�� �ȳ��˾�â�� �ֺ��� �ߴ� �̺�Ʈ ����
    public void BindUIPopup(GameObject go, float t, Vector3 pos, Define.PopupScale scale,
        string text)
    {
        // ��� �˾�â�� ũ��, ��ġ ����
        UIpopupHolder uh = go.AddComponent<UIpopupHolder>();
            uh.Init(t, scale, text, pos);
    }

    // TMP TEXT�� ��ư���� ����ϰ�, �̺�Ʈ�Լ� ȣ�⵵ ���ÿ� ó���Ϸ� ���� �Լ�
    public void BindTMPInteraction(TextMeshProUGUI tmp , Color enter, Color pressed, Action<TextMeshProUGUI> func)
    {
        tmp.AddComponent<TextButtonHolder>().Init(ref tmp,enter,pressed ,func) ;
    }

    // ��Ÿ ui��ҿ� ���콺 �ۿ�� �̺�Ʈ �߻���Ű�� �Լ�
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
    // ���Ӿ�����, ī��鿡 Ŀ�� �����ð� ������ ��� �ȳ��˾�â ȣ�� �̺�Ʈ ����
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
