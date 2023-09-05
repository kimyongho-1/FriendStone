using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling.Memory.Experimental;

public class EditCanvas : LobbyPopup
{
    // ó������� ĳ���ͽ�����������,  ���������̸� �ٷ� ��������������
    public CharacterSelect characterStage;
    public CardSelect cardStage;

    // �����˾�����, �� ĵ���������� �̵�
    public IEnumerator Transition(LobbyPopup ex, LobbyPopup next, int idx)
    {
        // cardStage������ ���� �ε����� �´�, ȯ�� ���߱�

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

    // PlayCanvas������, �ڽ��� �������� Ŭ����
    // �ش� ���� �����ϴ� ��Ȳ ����
    public void StartEditMode(DeckData data)
    {
        cardStage.deckView.EnterUserDeck(data);
        cardStage.cardView.EnterEmptyDeck(data.ownerClass);
        cardStage.cg.alpha = 1;
        characterStage.gameObject.SetActive(false);
        cardStage.gameObject.SetActive(true);
    }

    // PlayCanvas������, ���ο� ������⸦ Ŭ���� �� ���� ȯ�� ����
    public void StartMakingMode()
    {
        characterStage.cg.alpha = 1;
        // ���� ó�� ���鋚�� ĳ���� ����â���� ����
        cardStage.gameObject.SetActive(false);
        characterStage.gameObject.SetActive(true);  
    }
}