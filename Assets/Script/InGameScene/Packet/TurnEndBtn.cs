using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class TurnEndBtn : MonoBehaviour
{
    public Collider2D Col;
    public TextMeshPro btnTmp;
    public TextMeshProUGUI infoTmp;
    private void Awake()
    {
        Col.enabled = false;
        btnTmp.text = "상대 턴";
        GAME.IGM.Turn = this;
        GAME.Manager.UM.BindEvent( this.gameObject , ClickedOnTurnEnd , Define.Mouse.ClickL, Define.Sound.Click );
    }

    // 턴종료 버튼 누를시 호출
    void ClickedOnTurnEnd(GameObject go)
    {
        Col.enabled = false;
        btnTmp.text = "상대 턴";
        Debug.Log("TurnEnd!!");

        // 내 차례 종료 예약 (아직 안끝난 행동이 있을수도 있기에)
        GAME.IGM.AddAction(EndMyTurn());
    }

    // 턴종료 버튼 누른뒤, 모든 등록된 함수 실행후에 턴종료 시작
    public IEnumerator EndMyTurn()
    {
        // 모든 핸드 위치 크기 레이 초기화
        GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = false);

        // 강제로 Exit함수로 초기화 실행
        GAME.IGM.Hand.PlayerHand.ForEach(x => x.Exit(null));

        // 내 턴 종료
        GAME.IGM.Packet.isMyTurn = false;
        // 상대에게 내 턴 종료 전파
        GAME.IGM.Packet.SendTurnEnd();
        yield break;
    }

    // 누구의 턴이 시작됨을 알리는 텍스트애님 코루틴
    public IEnumerator ShowTurnMSG(bool isMyTurn)
    {
        // 텍스트 표시
        btnTmp.text = (isMyTurn) ? "나의 턴" : "상대 턴";
        infoTmp.text = (isMyTurn) ? "나의 차례" : "상대의 차례";
        infoTmp.color = new Color(1, 1, 1, 0);
        infoTmp.gameObject.SetActive(true);
        float t = 0;
        Color c = Color.white;
        c.a = 0;
        // UI 글씨 알파 증가
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            c.a = t;
            infoTmp.color = c;
            yield return null;
        }
        // UI 글씨 알파 감소
        while (t > 0)
        {
            t -= Time.deltaTime * 3f;
            c.a = t;
            infoTmp.color = c;
            yield return null;
        }
    }

    // 상대의 턴종료 이벤트 받을시, 나의 턴 시작
    public IEnumerator StartMyTurn()
    {
        // 내 턴 시작 알림 텍스트 시작
        StartCoroutine(ShowTurnMSG(true));
        // 내 턴 시작
        GAME.IGM.Packet.isMyTurn = true;
        // 상대의 화면에 내 턴 시작 띄우기 이벤트 전파
        GAME.IGM.Packet.SendMyTurnMSG();

        // 나 드로우 시작
        yield return StartCoroutine(GAME.IGM.Hand.CardDrawing(1));

        // 현재 나의 턴 시작, 마나 초기화 (최대치는 10, 서로의 턴이 지날떄마다 하나씩 최대 마나 증가 like 하스스톤)
        GAME.IGM.Hero.Player.mp = Mathf.Clamp(GAME.IGM.GameTurn, 0, 10);


        // 턴종료 버튼 누를수 있도록 레이 활성화
        Col.enabled = true;


    }
}
