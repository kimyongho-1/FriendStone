using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public LobbyPopup main, play, edit, option;
    public Stack<LobbyPopup> popupIndex = new Stack<LobbyPopup>();

    // 뒤로 갈떄, 이전팝업 찾기
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

    // 코루틴으로 만든 캔버스간 전환 효과 , Stack 인덱스 사용 버전
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

        // 지난 팝업창 다시 켤수있도록 stack에 기록
        popupIndex.Push(ex);
    }

    // 이전 팝업 호출하는 버전
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
