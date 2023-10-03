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
// 1. Ż���� 10�� �ʰ��� �ڷ�ƾ �ִϸ��̼� �ʿ�
public class CustomCardHand : List<CardHand>
{
    public new void Add(CardHand ch)
    {
        base.Add(ch);
        GAME.IGM.Hand.AllCardHand.Add(ch);
    }
    public new void Remove(CardHand ch)
    {
        base.Remove(ch);
    }
}

public class HandManager : MonoBehaviour
{
    public GameObject PlayerHandGO, EnemyHandGO;
    public Transform PlayerDeck, EnemyDeck, PlayerDrawingCard, EnemyDrawingCard;
    public CustomCardHand PlayerHand = new CustomCardHand();
    public CustomCardHand EnemyHand = new CustomCardHand();

    public List<CardHand> AllCardHand = new List<CardHand>();
    public CardHand prefab;
    public int punConsist = 1;
    Queue<CardData> deckCards = new Queue<CardData>();
    private void Awake()
    {
        GAME.IGM.Hand = this;
        // ���� ���۽�, ����� �� ��� ����Ʈ�� Ǯ��
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
        
        // ���� ī��� ��� ���� (������ ������ ���ؼ�)
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
    
    // ���� ������ ī�� �̱�
    public IEnumerator CardDrawing(int count)
    {

        // �������� �� �������� ī�� ������ ���� �ڷ�ƾ
        IEnumerator DrawingCo;
        IEnumerator DeckAnimCo()
        {
            // ������ ī�� ��ο�ø��� �� ������ ������ ���ҽ�Ű��
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


            // ���� ī�尡 ���� ���ٸ�, �������� �����ī�� ��������
            if (PlayerDeck.transform.localScale.x <= 0) 
            {
                PlayerDeck.gameObject.SetActive(false); 
                PlayerDrawingCard.gameObject.SetActive(false);
            }
            PlayerDrawingCard.transform.position = new Vector3(startPos.x, startPos.y, -0.03f);
            DrawingCo = null;
            yield break;
        }

        // ���� ī��� ������ Ȯ��
        for (int i = 0; i < count; i++)
        {
            // ������ ������ ���� �ڷ�ƾ ���� ����
            DrawingCo = DeckAnimCo();
            StartCoroutine(DrawingCo);

            // ������ ���������� ���� ī���� ������
            CardData cd = deckCards.Dequeue();
            // �ΰ��� ī�� ������ ����
            CardHand ch = GameObject.Instantiate(prefab, PlayerHandGO.transform);
            // �ΰ��� ī�� �ʱ�ȭ
            ch.Init(cd, true);

            // ���� �ĺ��� �ѹ����ϱ� + ���� �̴Ͼ�ī�尡 ��ȯ�ɽ� �ڵ�ī���� �ݳѹ� �Ѱܹ޾� ���
            ch.PunId = CreatePunNumber();

            // OnHand �̺�Ʈ ���� Ȯ�� �� ����
            List<CardBaseEvtData> evtList = cd.evtDatas.FindAll(x => x.when == Define.evtWhen.onHand);
            if (evtList != null)
            {
                for (int j = 0; j < evtList.Count; j++)
                {
                    GAME.IGM.Battle.Evt(evtList[j],ch);
                }
            }

            // ��뿡�� �� ��ο� ���� ����
            GAME.IGM.Packet.SendDrawInfo(ch.PunId);

            // ���� �ڵ�ī�忡 ���Խ�Ű��
            PlayerHand.Add(ch);
            yield return new WaitUntil(() => (DrawingCo == null)) ;

            // ������ ī��� ���� ������, ������ ��ο�
            yield return StartCoroutine(CardAllignment(true));
        }

    }

    // ��밡 ī�� ������ ����
    public IEnumerator EnemyCardDrawing(int punID)
    {
        // �������� �� �������� ī�� ������ ���� �ڷ�ƾ
        IEnumerator DrawingCo;
        IEnumerator DeckAnimCo()
        {
            // ������ ī�� ��ο�ø��� �� ������ ������ ���ҽ�Ű��
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


            // ���� ī�尡 ���� ���ٸ�, �������� �����ī�� ��������
            if (EnemyDeck.transform.localScale.x <= 0)
            {
                EnemyDeck.gameObject.SetActive(false);
                EnemyDrawingCard.gameObject.SetActive(false);
            }
            EnemyDrawingCard.transform.position = new Vector3(startPos.x, startPos.y, -0.03f);
            DrawingCo = null;
            yield break;
        }

        // ������ ������ ���� �ڷ�ƾ ���� ����
        DrawingCo = DeckAnimCo();
        StartCoroutine(DrawingCo);

        // ������ ���������� ���� ī���� ������
        //CardData cd = deckCards.Dequeue();
        // �ΰ��� ī�� ������ ����
        CardHand ch = GameObject.Instantiate(prefab, EnemyHandGO.transform);
        ch.transform.position = new Vector3(9.45f, 3.26f, 0);
        // �ΰ��� ī�� �ʱ�ȭ
        ch.Init(null, false);
        
        // ���� �ĺ��� �ѹ����ϱ� + ���� �̴Ͼ�ī�尡 ��ȯ�ɽ� �ڵ�ī���� �ݳѹ� �Ѱܹ޾� ���
        ch.PunId = punID;

        // ���� �ڵ�ī�忡 ���Խ�Ű��
        EnemyHand.Add(ch);
        yield return new WaitUntil(() => (DrawingCo == null));
        // ������ ī��� ���� ������, ������ ��ο�
        yield return StartCoroutine(CardAllignment(false));
    }
  
    // �ڵ� ī�� ����
    public IEnumerator CardAllignment(bool isMine = true)
    {
        // ���� �� �Ǵ� ���� �ڵ��� �������� Ȯ��
        List<CardHand> hand = (isMine) ? PlayerHand : EnemyHand;
        // ī�尡 ������ ������ ������ �ʿ䰡 �����Ƿ� �ٷ� ���
        if (hand.Count == 0) { yield break; }

        // ī���� ���� �ڷ�ƾ �����ų��, ��� �ڵ�ī����� ������ �������� Ȯ���ϴ� ī���� ť
        Queue<Coroutine> co = new Queue<Coroutine>();
        // Left�� Right �� �ڵ�ī���� �¿� ���������� ����
        // interval�� ī�尡 �������� ī�尣�� ������ ������ ����
        float interval = Mathf.Lerp(0.5f, 0, (hand.Count) / 10f);
        float Left = -4f + Mathf.Lerp(0, -2f, (hand.Count) / 10f) - interval;
        float Right = -4f + Mathf.Lerp(0, 2f, (hand.Count) / 10f) + interval;

        // ī�尣 ����
        float Height = 0.3f * (hand.Count / 10f);
        // ī�� ������ ����Ͽ� z�� ȸ�� ����
        float angle = 18f * ((hand.Count - 1) / 10f);
        int max = Mathf.Max(hand.Count - 1, 1);
        for (int i = 0; i < hand.Count; i++)
        {
            // 0���� ������ �ȵǱ⿡
            float ratio = ((float)i / max);

            // ��ġ �� ���ϱ�
            float x = Mathf.Lerp(Left, Right, ratio);
            float y = (isMine) ? -2.5f + Mathf.Sin(ratio * Mathf.PI) * Height
                : 4.7f - Mathf.Sin(ratio * Mathf.PI) * Height ;

            // ���� ��ġ�� ȸ���� ���� 
            hand[i].OriginPos = new Vector3(x, y, -0.5f);
            hand[i].originRot = new Vector3(0, 0,
                (isMine) ? Mathf.Cos(ratio * Mathf.PI) * angle
                : Mathf.Cos(ratio * Mathf.PI) * -angle);

            // ��ġ�� �̵���Ű��
            co.Enqueue(StartCoroutine(HandCardMove(hand[i])));
           
            if (isMine)
            {
                // ���ÿ��� ���� ����
                hand[i].originOrder = i + 1;
                hand[i].SetOrder(i + 1);
            }
            
        }

        // ���� �ڷ�ƾ ���� ��������� Ȯ�� �� ���
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
            // ť ���ҽ�Ű�� ( ť ���� 0 �Ͻ� , ��� �ڵ�ī�� ���� �ڷ�ƾ �������� �Ͻ�)
            co.Dequeue();
        }
    }

    // ���� �ִϸ��̼� �ڷ�ƾ
    
}
