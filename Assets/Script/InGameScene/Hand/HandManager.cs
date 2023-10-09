using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;
// to do list
// 1. 탈진과 10장 초과시 코루틴 애니메이션 필요
public class CustomCardHand
{
    [field: SerializeField] private List<CardHand> cardHand = new List<CardHand>();
    Dictionary< int, CardHand> cardHandDic = new Dictionary<int,CardHand>();
    public void Add(CardHand ch)
    {
        cardHand.Add(ch);
        cardHandDic.Add(ch.PunId,ch);
        GAME.IGM.Hand.AllCardHand.Add(ch);
    }
    public void Remove(CardHand ch)
    {
        cardHand.Remove(ch);
        cardHandDic.Remove(ch.PunId);
    }
    public CardHand Find(int punID)
    {
        cardHandDic.TryGetValue(punID, out CardHand FoundedCard);
        return FoundedCard;
    }
    public CustomCardHand FindAll(System.Func<CardHand,bool> act )
    {
        CustomCardHand tempList = new CustomCardHand();
        for (int i = 0; i < cardHand.Count; i++)
        {
            if (act.Invoke(cardHand[i]) == true)
            { tempList.Add(cardHand[i]); }
        }

        return tempList;
    }

    public void ForEach(System.Action<CardHand> act)
    {
        for (int i = 0; i < cardHand.Count; i++)
        {
            act.Invoke(cardHand[i]);
        }
    }
    public List<CardHand> ToList()
    { return cardHand; }
    public int IndexOf(CardHand ch)
    { return cardHand.IndexOf(ch); }
    public int Count { get { return cardHand.Count(); } }
}

public class HandManager : MonoBehaviour
{
    public GameObject PlayerHandGO, EnemyHandGO;
    public Transform PlayerDeck, EnemyDeck, PlayerDrawingCard, EnemyDrawingCard;
    public CustomCardHand PlayerHand = new CustomCardHand();
    public CustomCardHand EnemyHand = new CustomCardHand();
    public List<CardHand> AllCardHand = new List<CardHand>();
    public FatigueIcon Fatigue;
    public CardHand prefab;
    public int punConsist = 1;
    int fatigueStack = 0; // 탈진스택 : 덱에 카드가 없을떄 탈진코루틴을 실행하며 실행시 1씩 증가한 피해량을 내 영웅에게 부여하는 이벤트
    Queue<CardData> deckCards = new Queue<CardData>();
    private void Awake()
    {
        GAME.IGM.Hand = this;
        // 게임 시작시, 사용할 덱 모두 리스트로 풀기
        List<CardData> cards = GAME.Manager.RM.GameDeck.cards.Keys.ToList();
        List<int> counts = GAME.Manager.RM.GameDeck.cards.Values.ToList();
        List < CardData > list = new List<CardData>();
        for (int i = 0; i < cards.Count; i++)
        {
            for (int j = 0; j < counts[i]; j++)
            {
                // 새로운 원본을 생성하여 덱에 추가해주기 ( 모든 데이터가 원본으로 지정하여 데이터 겹침 현상 방지)
                list.Add(cards[i]);
            }
        }
        
        // 덱의 카드들 모두 섞기 (랜덤한 순서를 위해서)
        for (int i = 0; i < list.Count; i++)
        {
            CardData temp = list[i];
            int rand = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[rand];
            list[rand] = temp;
            deckCards.Enqueue(list[i]);
        }
    }

    public int CreatePunNumber()
    { return (Photon.Pun.PhotonNetwork.IsMasterClient ? 1000 : 2000) + punConsist++; }

    // 핸드가 이미 10장일떄 카드를 드로우시, 해당 드로우카드 소멸 이벤트
    public IEnumerator HandOverFlow(int cardIdNum, CardHand ch)
    {
        #region 소멸할 카드 이동
        ch.transform.localScale = Vector3.one * 0.6f; // 크기 확대
        Vector3 start = new Vector3(8.5f, -0.5f, -0.5f); 
        Vector3 dest = new Vector3(4.5f, -0.5f, -0.5f); // 3.2f
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            ch.transform.position =
                Vector3.Lerp(start, dest, t);
            yield return null;
        }
        #endregion

        #region 위치 도달시, 소멸(투명화 시작)
        // 투명화 위해 모든 TMP와 SR을 묶기
        List<TextMeshPro> tmpList = new List<TextMeshPro>() { ch.cardName, ch.Description, ch.Cost, ch.Stat, ch.Type };
        List<SpriteRenderer> imageList = new List<SpriteRenderer>() { ch.cardImage, ch.cardBackGround };
        t = 1;
        Color tempColor = Color.white;
        while (t > 0f)
        {
            // 알파값 점차 0으로 변환
            t -= Time.deltaTime;
            tempColor.a = t;
            tmpList.ForEach(x => x.alpha = t);
            imageList.ForEach(x => x.color = tempColor);
            yield return null;
        }
        // 상대에게 내 오버드로우 이벤트 전달
        GAME.IGM.Packet.SendOverDrawInfo(cardIdNum);
        GameObject.Destroy(ch.gameObject);
        #endregion 
    } 
    public IEnumerator EnemyHandOverFlow(int cardIdNum)
    {
        #region 적의 소멸 이벤트를 그대로 따라하기 위해, 소멸할 카드 데이터 찾기 + 적용
        // 적의 카드소멸 이벤트를 똑같이 따라하여 동기화해주기
        // 리소스 매니저의 경로를 반환 받는 딕셔너리 통해 카드타입과 카드데이터 찾기
        Define.cardType type = GAME.Manager.RM.PathFinder.Dic[cardIdNum].type;
        string jsonFile = GAME.Manager.RM.PathFinder.Dic[cardIdNum].GetJson();
        CardData card = null;
        // 확인된 카드타입으로, 실제 카드타입으로 클래스화
        switch (type)
        {
            case Define.cardType.minion:
                card = JsonConvert.DeserializeObject<MinionCardData>
            (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                break;
            case Define.cardType.spell:
                card = JsonConvert.DeserializeObject<SpellCardData>
            (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                break;
            case Define.cardType.weapon:
                card = JsonConvert.DeserializeObject<WeaponCardData>
            (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                break;
            default: break;
        }
        yield return null;
        // 인게임 카드 프리팹 생성
        CardHand ch = GameObject.Instantiate(prefab, EnemyHandGO.transform);
        ch.Init(card, true);
        #endregion

        #region 소멸할 카드 이동
        ch.transform.localScale = Vector3.one * 0.6f; // 크기 확대
        Vector3 start = new Vector3(8.5f, 3.2f, -0.5f);
        Vector3 dest = new Vector3(4.5f, 3.2f, -0.5f); //  적의 소멸 카드 위치는 좀더 높게
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            ch.transform.position =
                Vector3.Lerp(start, dest, t);
            yield return null;
        }
        #endregion

        #region 위치 도달시, 소멸(투명화 시작)
        // 투명화 위해 모든 TMP와 SR을 묶기
        List<TextMeshPro> tmpList = new List<TextMeshPro>() { ch.cardName, ch.Description, ch.Cost, ch.Stat, ch.Type };
        List<SpriteRenderer> imageList = new List<SpriteRenderer>() { ch.cardImage, ch.cardBackGround };
        t = 1;
        Color tempColor = Color.white;
        while (t > 0f)
        {
            // 알파값 점차 0으로 변환
            t -= Time.deltaTime;
            tempColor.a = t;
            tmpList.ForEach(x => x.alpha = t);
            imageList.ForEach(x => x.color = tempColor);
            yield return null;
        }
        yield return null;
        GameObject.Destroy(ch.gameObject);
        #endregion
    }

    // 내가 덱에서 카드 뽑기
    public IEnumerator CardDrawing(int count)
    {
        // 씬내부의 덱 모형에서 카드 뽑히는 연출 코루틴
        IEnumerator DrawingCo;
        IEnumerator DeckAnimCo()
        {
            // 유저의 카드 드로우시마다 덱 모형도 스케일 감소시키기
            PlayerDeck.transform.localScale = new Vector3(0.3f - (0.015f), 1.4f, 0.4f);

            float t = 0;
            Vector3 startPos = PlayerDrawingCard.transform.position;
            Vector3 destPos = new Vector3(startPos.x, startPos.y, -1f);

            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                PlayerDrawingCard.transform.position =
                    Vector3.Lerp(startPos, destPos, t);
                yield return null;
            }


            // 만약 카드가 이제 없다면, 덱모형과 드로잉카드 꺼버리기
            if (PlayerDeck.transform.localScale.x <= 0) 
            {
                PlayerDeck.gameObject.SetActive(false); 
                PlayerDrawingCard.gameObject.SetActive(false);
            }
            PlayerDrawingCard.transform.position = new Vector3(startPos.x, startPos.y, -0.03f);
            DrawingCo = null;
            yield break;
        }

        // 덱에 카드수 남는지 확인
        for (int i = 0; i < count; i++)
        {
            // 덱에 카드가 없다면
            if (deckCards.Count == 0)
            {
                yield return StartCoroutine(Fatigue.DeckExhausted(true, ++fatigueStack ));
            }

            // 덱에 카드가 있다면
            else
            {
                // 덱에서 뽑히는 연출 코루틴 먼저 실행
                DrawingCo = DeckAnimCo();
                StartCoroutine(DrawingCo);

                // 덱에서 실질적으로 뽑힌 카드의 데이터
                CardData cd = deckCards.Dequeue();
                switch (cd.cardType)
                {
                    case Define.cardType.weapon: cd = new WeaponCardData(cd); break;
                    case Define.cardType.minion: cd = new MinionCardData(cd); break;
                    case Define.cardType.spell: cd = new SpellCardData(cd); break;
                }

                // 인게임 카드 프리팹 생성
                CardHand ch = GameObject.Instantiate(prefab, PlayerHandGO.transform);
                // 인게임 카드 초기화
                ch.Init(cd, true);

                // 만약 손패가 이미 10장인 상태에서 드로우 상황이라면 => 현재 뽑히는 카드 삭제 이벤트 실행
                if (PlayerHand.Count == 10)
                {
                    yield return new WaitUntil(() => (DrawingCo == null));
                    yield return GAME.IGM.StartCoroutine(HandOverFlow(cd.cardIdNum, ch));
                }

                // 덱에 카드를 뽑아, 10장 미만인 핸드로 가져오는 정상적인 드로우 상황
                else 
                {
                    // 포톤 식별자 넘버링하기 + 만약 미니언카드가 소환될시 핸드카드의 펀넘버 넘겨받아 사용
                    ch.PunId = CreatePunNumber();

                    // OnHand 이벤트 존재 확인 및 실행
                    List<CardBaseEvtData> evtList = cd.evtDatas.FindAll(x => x.when == Define.evtWhen.onHand);
                    if (evtList != null)
                    {
                        for (int j = 0; j < evtList.Count; j++)
                        {
                            GAME.IGM.AddAction(GAME.IGM.Battle.Evt(evtList[j], ch));
                        }
                    }
                    // 상대에게 내 드로우 정보 전달
                    GAME.IGM.Packet.SendDrawInfo(ch.PunId);

                    // 나의 핸드카드에 포함시키기
                    PlayerHand.Add(ch);
                    yield return new WaitUntil(() => (DrawingCo == null));

                    // 손패의 카드들 정렬 실행후, 나머지 드로우
                    yield return StartCoroutine(CardAllignment(true));
                }
            }
            
        }
        // 드로우와 정렬 모두 끝났으면 콜라이더작용 위해 다시 활성화
        GAME.IGM.Hand.PlayerHand.ForEach(x=>x.Ray = true);
    }

    // 상대가 카드 뽑을시 실행
    public IEnumerator EnemyCardDrawing(int punID)
    {
        // 씬내부의 덱 모형에서 카드 뽑히는 연출 코루틴
        IEnumerator DrawingCo;
        IEnumerator DeckAnimCo()
        {
            // 유저의 카드 드로우시마다 덱 모형도 스케일 감소시키기
            EnemyDeck.transform.localScale = new Vector3(EnemyDeck.transform.localScale.x - (0.015f), 1.4f, 0.4f);

            float t = 0;
            Vector3 startPos = EnemyDrawingCard.transform.position;
            Vector3 destPos = new Vector3(startPos.x, startPos.y, -1f);

            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                EnemyDrawingCard.transform.position =
                    Vector3.Lerp(startPos, destPos, t);
                yield return null;
            }


            // 만약 카드가 이제 없다면, 덱모형과 드로잉카드 꺼버리기
            if (EnemyDeck.transform.localScale.x <= 0)
            {
                EnemyDeck.gameObject.SetActive(false);
                EnemyDrawingCard.gameObject.SetActive(false);
            }
            EnemyDrawingCard.transform.position = new Vector3(startPos.x, startPos.y, -0.03f);
            DrawingCo = null;
            yield break;
        }

        // 덱에서 뽑히는 연출 코루틴 먼저 실행
        DrawingCo = DeckAnimCo();
        StartCoroutine(DrawingCo);

        // 덱에서 실질적으로 뽑힌 카드의 데이터
        //CardData cd = deckCards.Dequeue();
        // 인게임 카드 프리팹 생성
        CardHand ch = GameObject.Instantiate(prefab, EnemyHandGO.transform);
        ch.transform.position = new Vector3(9.45f, 3.26f, 0);
        // 인게임 카드 초기화
        ch.Init(null, false);
        
        // 포톤 식별자 넘버링하기 + 만약 미니언카드가 소환될시 핸드카드의 펀넘버 넘겨받아 사용
        ch.PunId = punID;

        // 적의 핸드카드에 포함시키기
        EnemyHand.Add(ch);
        yield return new WaitUntil(() => (DrawingCo == null));
        // 손패의 카드들 정렬 실행후, 나머지 드로우
        yield return StartCoroutine(CardAllignment(false));
    }
  
    // 핸드 카드 정렬
    public IEnumerator CardAllignment(bool isMine = true)
    {
        // 현재 나 또는 적의 핸드중 무엇인지 확인
        List<CardHand> hand = (isMine) ? PlayerHand.ToList() : EnemyHand.ToList();
        // 카드가 없으면 정렬을 수행할 필요가 없으므로 바로 취소
        if (hand.Count == 0) { yield break; }

        // 카드의 정렬 코루틴 실행시킬떄, 모든 핸드카드들의 정렬이 끝났는지 확인하는 카운터 큐
        Queue<Coroutine> co = new Queue<Coroutine>();
        // Left와 Right 은 핸드카드의 좌우 최종길이의 범위
        // interval은 카드가 적을수록 카드간의 간격을 벌리는 범위
        float interval = Mathf.Lerp(0.5f, 0, (hand.Count) / 10f);
        float Left = -4f + Mathf.Lerp(0, -2f, (hand.Count) / 10f) - interval;
        float Right = -4f + Mathf.Lerp(0, 2f, (hand.Count) / 10f) + interval;

        // 카드간 높이
        float Height = 0.3f * (hand.Count / 10f);
        // 카드 갯수에 비례하여 z축 회전 적용
        float angle = 18f * ((hand.Count - 1) / 10f);
        int max = Mathf.Max(hand.Count - 1, 1);

        for (int i = 0; i < hand.Count; i++)
        {
            // 0으로 나누면 안되기에
            float ratio = ((float)i / max);

            // 위치 값 구하기
            float x = Mathf.Lerp(Left, Right, ratio);
            float y = (isMine) ? -2.5f + Mathf.Sin(ratio * Mathf.PI) * Height
                : 4.7f - Mathf.Sin(ratio * Mathf.PI) * Height ;

            // 고유 위치와 회전값 지정 
            hand[i].OriginPos = new Vector3(x, y, -0.5f);
            hand[i].originRot = new Vector3(0, 0,
                (isMine) ? Mathf.Cos(ratio * Mathf.PI) * angle
                : Mathf.Cos(ratio * Mathf.PI) * -angle);

            // 위치로 이동시키기
            co.Enqueue(StartCoroutine(HandCardMove(hand[i])));
           
            if (isMine)
            {
                // 소팅오더 정렬 시작
                hand[i].originOrder = i + 1;
                hand[i].SetOrder(i + 1);
            }
            
        }

        // 정렬 코루틴 전부 수행됬는지 확인 및 대기
        yield return new WaitUntil(() => (co.Count() == 0));

        IEnumerator HandCardMove(CardHand ch)
        {
            Vector3 euler = transform.rotation.eulerAngles;

            if (euler.y > 180)
            { euler.y -= 360f; }
            if (euler.x > 180)
            { euler.x -= 360f; }
            if (euler.z > 180)
            { euler.z -= 360f; }
            
            Vector3 start = ch.transform.position;
            Vector3 startRpt = euler;
            Vector3 startScale = ch.transform.localScale;
            //Debug.Log(ch.transform.rotation.eulerAngles);
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 1.5f;
                ch.transform.localScale =
                    Vector3.Lerp(startScale, ch.originScale, t);
                ch.transform.localRotation =
                    Quaternion.Euler(Vector3.Lerp(startRpt, ch.originRot, t));
                ch.transform.localPosition =
                    Vector3.Lerp(start, ch.OriginPos, t);
                yield return null;
            }
            // 큐 감소시키기 ( 큐 갯수 0 일시 , 모든 핸드카드 정렬 코루틴 끝났음을 암시)
            co.Dequeue();
        }
    }

    
}
