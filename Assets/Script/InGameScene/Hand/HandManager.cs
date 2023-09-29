using Newtonsoft.Json;
using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// to do list
// 1. 탈진과 10장 초과시 코루틴 애니메이션 필요


public class HandManager : MonoBehaviour
{
    public GameObject PlayerHandGO, EnemyHandGO;
    public Transform PlayerDeck, EnemyDeck, PlayerDrawingCard, EnemyDrawingCard;
    public List<CardHand> PlayerHand, EnemyHand;
    public CardHand prefab;
    public int punConsist = 1;
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
    
    // 내가 덱에서 카드 뽑기
    public IEnumerator CardDrawing(int count)
    {
        // 상대에게 내 드로우 전파 및 동기화를 위해 펀식별자 먼저 카운팅
        int[] puns = new int[count];

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
            // 덱에서 뽑히는 연출 코루틴 먼저 실행
            DrawingCo = DeckAnimCo();
            StartCoroutine(DrawingCo);

            // 덱에서 실질적으로 뽑힌 카드의 데이터
            CardData cd = deckCards.Dequeue();
            // 인게임 카드 프리팹 생성
            CardHand ch = GameObject.Instantiate(prefab, PlayerHandGO.transform);
            // 인게임 카드 초기화
            ch.Init(cd, true);

            // 포톤 식별자 넘버링하기 + 만약 미니언카드가 소환될시 핸드카드의 펀넘버 넘겨받아 사용
            ch.PunId = CreatePunNumber();

            // OnHand 이벤트 존재 확인 및 실행
            List<CardBaseEvtData> evtList = cd.evtDatas.FindAll(x => x.when == Define.evtWhen.onHand);
            if (evtList != null || evtList.Count > 0)
            {
                for (int j = 0; j < evtList.Count; j++)
                {
                    GAME.IGM.Battle.OnHandEvt(evtList[j],ch);
                }
                
            }

            // 상대에게 내 드로우 정보 전달
            GAME.IGM.Packet.SendDrawInfo(ch.PunId);

            // 나의 핸드카드에 포함시키기
            PlayerHand.Add(ch);
            yield return new WaitUntil(() => (DrawingCo == null)) ;

            // 손패의 카드들 정렬 실행후, 나머지 드로우
            yield return StartCoroutine(CardAllignment(true));
        }

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
    public IEnumerator CardDrawing(int[] puns)
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
                t += Time.deltaTime *2f;
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
      

        // 덱에 카드수 남는지 확인
        for (int i = 0; i < puns.Length; i++)
        {    
            // 덱에서 뽑히는 연출 코루틴 먼저 실행
            DrawingCo = DeckAnimCo();
            StartCoroutine(DrawingCo);

            // 덱에서 실질적으로 뽑힌 카드의 데이터
            CardData cd = deckCards.Dequeue();
            // 인게임 카드 프리팹 생성
            CardHand ch = GameObject.Instantiate(prefab, EnemyHandGO.transform);
            ch.transform.position = new Vector3(9.45f,3.26f,0);
            // 인게임 카드 초기화
            ch.Init(cd , false);
            
            // 포톤 식별자 넘버링하기 + 만약 미니언카드가 소환될시 핸드카드의 펀넘버 넘겨받아 사용
            ch.PunId = puns[i];

            // 적의 핸드카드에 포함시키기
            EnemyHand.Add(ch);
            yield return new WaitUntil(() => (DrawingCo == null));
            // 손패의 카드들 정렬 실행후, 나머지 드로우
            yield return StartCoroutine(CardAllignment(false));
        }
    }
    // 핸드 카드 정렬
    public IEnumerator CardAllignment(bool isMine = true)
    {
        // 현재 나 또는 적의 핸드중 무엇인지 확인
        List<CardHand> hand = (isMine) ? PlayerHand : EnemyHand;

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
            ch.Ray = false;
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
            ch.Ray = true;
            // 큐 감소시키기 ( 큐 갯수 0 일시 , 모든 핸드카드 정렬 코루틴 끝났음을 암시)
            co.Dequeue();
        }
    }

    // 정렬 애니메이션 코루틴
    
}
