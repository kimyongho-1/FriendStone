using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

// 핸드매니저에서 해야할것
// 1. 탈진 표시
// 2. 10장 초과시 카드삭제 코루틴
// 3. 위치 재정렬 이쁘게 손보기

public class HandManager : MonoBehaviour
{
    public GameObject playerHand, enemyHand;
    public List<CardHand> PlayerHand, EnemyHand;
    public CardHand prefab;

    Queue<CardData> deckCards = new Queue<CardData>();
    private void Awake()
    {
        GAME.Manager.IGM.Hand = this;
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
        CardDrawing(new int[] { 1,1,1,1,1,1,1});

    }

    // 덱에서 카드 뽑기
    public void CardDrawing(int[] puns)
    {
        // 덱에 카드수 남는지 확인
        for (int i = 0; i < puns.Length; i++)
        {
            CardData cd = deckCards.Dequeue();
            CardHand ch = GameObject.Instantiate(prefab, playerHand.transform);
            ch.Init(ref cd);
            PlayerHand.Add(ch);
            ch.PunId = 100+i;
            ch.gameObject.SetActive(true);
        }
        CardAllignment();
    }

    

    // 핸드 카드 정렬
    public void CardAllignment()
    { 
        for (int i = 0; i < PlayerHand.Count; i++)
        {
            float ratio = 0;
            if (PlayerHand.Count - 1 == 0)
            { ratio = 0; }
            else
            { ratio = ((float)i / (PlayerHand.Count - 1)); }
            // 위치 값 구하기
            PlayerHand[i].OriginPos =
                new Vector3(Mathf.Lerp(-6f, -2f, ratio) ,
                  -2.5f + Mathf.Sin(ratio * Mathf.PI) * 0.3f,
                 -0.5f);
            PlayerHand[i].originRot =
                new Vector3(0,0, Mathf.Cos(ratio * Mathf.PI ) * 18f);
            
            // 위치로 이동시키기
            StartCoroutine(HandCardMove(PlayerHand[i]));
            // 소팅오더 정렬 시작
            PlayerHand[i].originOrder = i;
            PlayerHand[i].SetOrder(i);
        }
        for (int i = 0; i < EnemyHand.Count; i++)
        {
            float ratio = ((float)i / (PlayerHand.Count - 1));

            // 위치 값 구하기
            EnemyHand[i].OriginPos =
                new Vector3(Mathf.Lerp(-6f, -2f, ratio),
                 -0.3f + Mathf.Sin(ratio * Mathf.PI) * -0.5f,
                 -0.5f);
            EnemyHand[i].originRot =
                new Vector3(0, 0, Mathf.Cos(ratio * Mathf.PI) * -18f);

            // 위치로 이동시키기
            StartCoroutine(HandCardMove(EnemyHand[i]));
            // 소팅오더 정렬 시작
            EnemyHand[i].SetOrder(i);
        }
    }

    // 정렬 애니메이션 코루틴
    public IEnumerator HandCardMove(CardHand ch)
    {
        Vector3 euler = transform.rotation.eulerAngles;
        if (euler.y > 180)
        { euler.y -= 360f; }
        if (euler.x > 180)
        { euler.x -= 360f; }
        ch.Ray = false;
        Vector3 start = ch.transform.position;
        Vector3 startRpt = euler;
        Debug.Log(ch.transform.rotation.eulerAngles);
        float t = 0;
        while (t < 1f) 
        {
            t += Time.deltaTime;
            ch.transform.rotation =
                Quaternion.Euler(Vector3.Lerp(startRpt, ch.originRot, t));
            ch.transform.localPosition =
                Vector3.Lerp(start, ch.OriginPos, t);
            yield return null;
        }
        ch.Ray = true;
    }
}
