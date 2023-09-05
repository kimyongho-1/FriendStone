using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public LobbyPopup main, play, edit, option;
    public Stack<LobbyPopup> popupIndex = new Stack<LobbyPopup>();

    // �ڷ� ����, �����˾� ã��
    public LobbyPopup GetExPopup 
    {
        get 
        {
            if (popupIndex.Count > 0)
            { return popupIndex.Pop(); }
            else { return main; }
        } 
    } 
    private void Awake()
    {
        GAME.Manager.LM = this;
    }

    // �ڷ�ƾ���� ���� ĵ������ ��ȯ ȿ�� , Stack �ε��� ��� ����
    public IEnumerator CanvasTransition(LobbyPopup ex, LobbyPopup next)
    {
        GAME.Manager.Evt.enabled = false;
        next.cg.alpha = 0;
        next.gameObject.SetActive(true);
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;

            ex.cg.alpha = Mathf.Lerp(1,0, t);
            next.cg.alpha = Mathf.Lerp(0,1,t);
            yield return null;
        }
        GAME.Manager.Evt.enabled = true;
        ex.gameObject.SetActive(false);

        // ���� �˾�â �ٽ� �Ӽ��ֵ��� stack�� ���
        popupIndex.Push(ex);
    }

    // ���� �˾� ȣ���ϴ� ����
    public IEnumerator CanvasTransition(LobbyPopup ex)
    {
        GAME.Manager.Evt.enabled = false;
        LobbyPopup next = popupIndex.Pop();
        next.cg.alpha = 0;
        next.gameObject.SetActive(true);
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;

            ex.cg.alpha = Mathf.Lerp(1, 0, t);
            next.cg.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }
        GAME.Manager.Evt.enabled = true;
        ex.gameObject.SetActive(false);

    }
}
