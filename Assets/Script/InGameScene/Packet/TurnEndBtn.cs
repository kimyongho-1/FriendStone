using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class TurnEndBtn : MonoBehaviour
{
    public Collider2D Col;
    public TextMeshPro btnTmp;
    public TextMeshProUGUI infoTmp;
    public float baseTimeLimit = 10;
    public Material blinkMat;
    AudioSource audioPlayer;
    private void Awake()
    {
        audioPlayer = GetComponent<AudioSource>();
        Col.enabled = false;
        btnTmp.text = "상대 턴";
        GAME.IGM.Turn = this;
        GAME.Manager.UM.BindEvent( this.gameObject , ClickedOnTurnEnd , Define.Mouse.ClickL);
    }

    // 턴종료 버튼 누를시 호출
    public void ClickedOnTurnEnd(GameObject go)
    {
        // 턴 버튼 클릭음 재생
        audioPlayer.clip = GAME.IGM.GetClip(Define.IGMsound.ClickTurnBtn);
        audioPlayer.Play();

        // 시간임박 타이머중이었다면 강제 끄기
        if (GAME.IGM.TimeLimiter.gameObject.activeSelf == true)
        { GAME.IGM.TimeLimiter.gameObject.SetActive(false); }

        Col.enabled = false;
        btnTmp.text = "상대 턴";
        blinkMat.SetColor("_Color", new Color(1,0,0,1) );
        // 턴 시간 멈추기
        if (turnTimer != null)
        {
            StopCoroutine(turnTimer);
            turnTimer = null;
        }
        Debug.Log("TurnEnd!!");

        // 강제로 Exit함수로 초기화 실행
        GAME.IGM.Hand.PlayerHand.ForEach(x => x.rewindHand());

        // 내 차례 종료 예약 (아직 안끝난 행동이 있을수도 있기에)
        GAME.IGM.AddAction(EndMyTurn());
    }

    // 턴종료 버튼 누른뒤, 모든 등록된 함수 실행후에 턴종료 시작
    public IEnumerator EndMyTurn()
    {
        // 필드 하수인 모두 공격중지
        GAME.IGM.Spawn.playerMinions.ForEach(x => x.Attackable = false);
        // 내 턴 종료
        GAME.IGM.Packet.isMyTurn = false;
        // 상대에게 내 턴 종료 전파
        GAME.IGM.Packet.SendTurnEnd();

        for (int i = 0; i < GAME.IGM.Spawn.enemyMinions.Count; i++)
        {
            GAME.IGM.Spawn.enemyMinions[i].Attackable = true;
            GAME.IGM.Spawn.enemyMinions[i].sleep.gameObject.SetActive(false);
        }

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

        // 내 턴 시작이면, 내 턴 시작음 재생
        if (isMyTurn)
        {
            // 턴 시작 효과음 재생
            audioPlayer.clip = GAME.IGM.GetClip(Define.IGMsound.TurnStart);
            audioPlayer.Play();
        }
        
        // UI 글씨 알파 감소
        while (t > 0)
        {
            t -= Time.deltaTime * 3f;
            c.a = t;
            infoTmp.color = c;
            yield return null;
        }

        if (isMyTurn) // 나의턴이 시작될떄마다 영웅의 공격권과 스킬공격권 초기화
        {
            // 영웅의 공격상태 초기화
            GAME.IGM.Hero.Player.heroSkill.Attackable = GAME.IGM.Hero.Player.Attackable = true;
        }
        else // 상대도 동일
        {
            // 상대의 초기화
            GAME.IGM.Hero.Enemy.heroSkill.Attackable = true;
        }
    }

    // 상대의 턴종료 이벤트 받을시, 나의 턴 시작
    public IEnumerator StartMyTurn()
    {
        // 내 턴 시작 알림 텍스트 시작
        StartCoroutine(ShowTurnMSG(true));
        // 턴종료 버튼 머테리얼 색상 내 턴일떄로 초기화
        blinkMat.SetColor("_Color", new Color(0, 1, 0, 1));
        for (int i = 0; i < GAME.IGM.Spawn.playerMinions.Count; i++)
        {
            GAME.IGM.Spawn.playerMinions[i].Attackable = true;
            GAME.IGM.Spawn.playerMinions[i].sleep.gameObject.SetActive(false);
        }
        // 상대의 화면에 내 턴 시작 띄우기 이벤트 전파
        GAME.IGM.Packet.SendMyTurnMSG();

        // 현재 나의 턴 시작, 마나 초기화 (최대치는 10, 서로의 턴이 지날떄마다 하나씩 최대 마나 증가 like 하스스톤)
        GAME.IGM.Hero.Player.MP = Mathf.Min(10, GAME.IGM.GameTurn);

        // 나 드로우 시작
        yield return StartCoroutine(GAME.IGM.Hand.CardDrawing(1));
        // 내 턴 시작
        GAME.IGM.Packet.isMyTurn = true;
        // 턴종료 버튼 누를수 있도록 레이 활성화
        Col.enabled = true;
        // 턴 타이머 시작
        turnTimer = UserTurnTimer();
        StartCoroutine(turnTimer);
    }
    IEnumerator turnTimer;
    // 나의 턴이 시작될떄마다 실행되는 턴타이머
    public IEnumerator UserTurnTimer()
    {
        // 30초간 유저의 턴 동안 시간 계산
        float startTime = Time.time; // 정확한 시간개념을 위해 time사용
        while ((Time.time - startTime) < baseTimeLimit)
        {
            yield return null;
        }

        // 시간이 지난 영웅 대사 실행
        GAME.IGM.Hero.Player.HeroSaying(Define.Emotion.TimeLimitStart);

        startTime = Time.time;
        while ((Time.time - startTime) < baseTimeLimit)
        {
            yield return null;
        }

        // 턴 시간제한이 됬음에도, 턴종료가 눌리지 않았다면
        if (Col.enabled == true)
        {
            GAME.IGM.TimeLimiter.StartTimeLimit();
        }

        turnTimer = null;
    }
}
