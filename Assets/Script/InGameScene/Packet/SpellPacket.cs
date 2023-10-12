using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static Define;

public partial class PacketManager
{
    // 61 ~ 80
    const byte SpellSpawn = 61;
    const byte SpellEnd = 62;

    public void InitSpellDictionary()
    {
        dic.Add(SpellSpawn, ReceivedSpellCard);
        dic.Add(SpellEnd, ReceivedEndingSpellCard);
    }

    #region 주문 카드 사용
    public void SendUseSpellCard(int punID, int cardID)
    {
        object[] data = new object[] { punID, cardID };
        PhotonNetwork.RaiseEvent(SpellSpawn, data, Other, SendOptions.SendReliable);
    }

    public void ReceivedSpellCard(object[] data)
    {
        int punID = (int)data[0]; // 게임 화면내 어떤 카드 객체인지 식별자
        int cardID = (int)data[1]; // 해당 카드가 실제 어떤 카드인지 , 카드데이터

        // 상대의 핸드카드가 어디서 드래그를 끝냈는지 위치 찾기
        Vector3 dest = new Vector3(0.5f, 2.25f, -0.5f);

        // 리소스 매니저의 경로를 반환 받는 딕셔너리 통해 카드타입과 카드데이터 찾기
        Define.cardType type = GAME.Manager.RM.PathFinder.Dic[cardID].type;
        string jsonFile = GAME.Manager.RM.PathFinder.Dic[cardID].GetJson();

        // 카드 데이터 생성
        SpellCardData card = JsonConvert.DeserializeObject<SpellCardData>
            (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

        // 핸드카드, 실제 카드데이터를 찾아 적용해주기
        CardHand ch = GAME.IGM.Hand.EnemyHand.Find(punID);
        GAME.IGM.Hand.EnemyHand.Remove(ch);

        // 어떠 주문카드인지 띄우기 + 상대의 마나 감소
        GAME.IGM.Hero.Enemy.MP -= card.cost;
        GAME.IGM.ShowSpellPopup(card,new Vector3(3.5f, 2.8f, -0.5f) );

        Debug.Log($"{punID}가 현재 {ch}로 {ch.cardName}존재 확인");
        ch.SC = card;
        // 상대가 드래그를 끝낸 위치로 이동하는 애니메이션코루틴 먼저 예약
        GAME.IGM.AddAction(spellHandCardMove(ch, dest));
        // 상대의 핸드카드가 해당 위치로 이동하는 모션 따라하기
        IEnumerator spellHandCardMove(CardHand ch, Vector3 dest)
        {
            float t = 0;
            Vector3 start = ch.transform.position;
            while (t < 1f)
            {
                t += Time.deltaTime;
                ch.transform.position = Vector3.Lerp(start, dest, t);
                yield return null;
            }
            // 완전 삭제가 아닌 투명화만 진행
            yield return GAME.IGM.StartCoroutine(ch.FadeOutCo(false, false));
        }
    }
    #endregion

    #region 주문카드 끝났을떄
    public void SendEndingSpellCard(int punID)
    {
        object[] data = new object[] { punID, };
        PhotonNetwork.RaiseEvent(SpellEnd, data, Other, SendOptions.SendReliable);
    }
    public void ReceivedEndingSpellCard(object[] data)
    {
        int punID = (int)data[0]; // 게임 화면내 어떤 카드 객체인지 식별자

        // 다시 카드 찾기
        CardHand ch = GAME.IGM.Hand.AllCardHand.Find(x => x.PunId == punID);
        GAME.IGM.AddAction(RemoveSpellCard(ch));
        IEnumerator RemoveSpellCard(CardHand ch)
        {
            Debug.Log($"{punID}가 현재 {ch.SC.cardName}로 존재 확인");
            // 핸드매니저에서 적 핸드카드들 재정렬 시작
            yield return StartCoroutine(GAME.IGM.Hand.CardAllignment(false));
            // 상대가 어떤 카드 사용했는지 보여주던 카드팝업 해제
            GAME.IGM.cardPopup.isEnmeySpawning = false;
            GAME.IGM.cardPopup.gameObject.SetActive(false);
            
            // 주문카드 완전 사용하였기에 삭제 시작
            GAME.IGM.Hand.AllCardHand.Remove(ch);
            GameObject.Destroy(ch.gameObject);
        }

    }
    #endregion
}