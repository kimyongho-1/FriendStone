using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EditCanvas : LobbyPopup
{
    // 처음만들면 캐릭터스테이지부터,  편집수정이면 바로 편집스테이지로
    public CharacterSelect characterStage;
    public CardSelect cardStage;
    public AudioSource audioPlayer;

    private void Awake()
    {
        audioPlayer = GetComponent<AudioSource>();  
    }
    // 이전팝업없이, 한 캔버스내에서 이동
    public IEnumerator Transition(LobbyPopup ex, LobbyPopup next, int idx)
    {
        // cardStage에서는 현재 인덱스에 맞는, 환경 갖추기

        GAME.Manager.Evt.enabled = false;
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

    // PlayCanvas내에서, 자신의 기존덱을 클릭시
    // 해당 덱을 편집하는 상황 실행
    public void StartEditMode(DeckData data)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Enter);
        cardStage.deckView.EnterUserDeck(data);
        cardStage.cardView.EnterEmptyDeck(data.ownerClass);
        cardStage.cg.alpha = 1;
        characterStage.gameObject.SetActive(false);
        cardStage.gameObject.SetActive(true);
    }

    // PlayCanvas내에서, 새로운 덱만들기를 클릭시 덱 제작 환경 시작
    public void StartMakingMode()
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Enter);
        characterStage.cg.alpha = 1;
        // 덱을 처음 만들떄는 캐릭터 선택창부터 실행
        cardStage.gameObject.SetActive(false);
        characterStage.gameObject.SetActive(true);  
    }
}