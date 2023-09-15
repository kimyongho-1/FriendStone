using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

// to do list
// 1. Ż���� 10�� �ʰ��� �ڷ�ƾ �ִϸ��̼� �ʿ�


public class HandManager : MonoBehaviour
{
    public GameObject PlayerHandGO, EnemyHandGO;
    public Transform PlayerDeck, EnemyDeck, PlayerDrawingCard, EnemyDrawingCard;
    public List<CardHand> PlayerHand, EnemyHand;
    public CardHand prefab;
    public int punConsist = 0;
    Queue<CardData> deckCards = new Queue<CardData>();
    private void Awake()
    {
        GAME.Manager.IGM.Hand = this;
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
        //CardDrawing(new int[] { 1,1,1,1,1,1,1});
        StartCoroutine(CardDrawing(10));
    }

    
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
            ch.Init(ref cd);
            ch.IsMine = true;

            // ���� �ĺ��� �ѹ����ϱ� + ���� �̴Ͼ�ī�尡 ��ȯ�ɽ� �ڵ�ī���� �ݳѹ� �Ѱܹ޾� ���
            ch.PunId = (Photon.Pun.PhotonNetwork.IsMasterClient ? 1000 : 2000) + punConsist;
            // ���� �ڵ�ī�忡 ���Խ�Ű��
            PlayerHand.Add(ch);
            yield return new WaitUntil(() => (DrawingCo == null)) ;

            // ������ ī��� ���� ������, ������ ��ο�
            yield return StartCoroutine(CardAllignment(true));
        }

    }

    // ��밡 ī�� ������ ����
    public IEnumerator CardDrawing(int[] puns)
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
                t += Time.deltaTime *2f;
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
      

        // ���� ī��� ������ Ȯ��
        for (int i = 0; i < puns.Length; i++)
        {    
            // ������ ������ ���� �ڷ�ƾ ���� ����
            DrawingCo = DeckAnimCo();
            StartCoroutine(DrawingCo);

            // ������ ���������� ���� ī���� ������
            CardData cd = deckCards.Dequeue();
            // �ΰ��� ī�� ������ ����
            CardHand ch = GameObject.Instantiate(prefab, EnemyHandGO.transform);
            // �ΰ��� ī�� �ʱ�ȭ
            ch.Init(ref cd);

            // ���� �ĺ��� �ѹ����ϱ� + ���� �̴Ͼ�ī�尡 ��ȯ�ɽ� �ڵ�ī���� �ݳѹ� �Ѱܹ޾� ���
            ch.PunId = puns[i];

            // ���� �ڵ�ī�忡 ���Խ�Ű��
            EnemyHand.Add(ch);

            // ������ ī��� ���� ������, ������ ��ο�
            yield return StartCoroutine(CardAllignment(false));
        }
    }

    int playerMoveCount = 0;
    int enemyMoveCount = 0;
    // �ڵ� ī�� ����
    public IEnumerator CardAllignment(bool isMine = true)
    {
        // ���� �� �Ǵ� ���� �ڵ��� �������� Ȯ��
        List<CardHand> hand = (isMine) ? PlayerHand : EnemyHand;

        // ī�尡 ������ ������ ������ �ʿ䰡 �����Ƿ� �ٷ� ���
        if (hand.Count == 0) { yield break; }

        // ���� �ڷ�ƾ ��� �������� Ȯ�� �� �ڷ�ƾ �ܿ� Ȯ�ο뵵
        if (isMine)
        { playerMoveCount = hand.Count; }
        else
        { enemyMoveCount = hand.Count; }

        // Left�� Right �� �ڵ�ī���� �¿� ���������� ����
        // interval�� ī�尡 �������� ī�尣�� ������ ������ ����
        float interval = Mathf.Lerp(0.5f, 0, (hand.Count) / 10f);
        float Left = -4f + Mathf.Lerp(0, -2f, (hand.Count) / 10f) - interval;
        float Right = -4f + Mathf.Lerp(0, 2f, (hand.Count) / 10f) + interval;

        // ī�尣 ����
        float Height = 0.3f * (hand.Count / 10f);
        // ī�� ������ ����Ͽ� z�� ȸ�� ����
        float angle = 18f * ((hand.Count - 1) / 10f);
        int max = Mathf.Max(hand.Count - 1,1);
        for (int i = 0; i < hand.Count; i++)
        {
            // 0���� ������ �ȵǱ⿡
            float ratio = ((float)i / max );

            // ��ġ �� ���ϱ�
            float x = Mathf.Lerp(Left, Right, ratio);
            float y = -2.5f + Mathf.Sin(ratio * Mathf.PI) * Height;

            // ���� ��ġ�� ȸ���� ���� 
            hand[i].OriginPos = new Vector3(x, y, -0.5f);
            hand[i].originRot = new Vector3(0, 0, Mathf.Cos(ratio * Mathf.PI) * angle);

            // ��ġ�� �̵���Ű��
            StartCoroutine(HandCardMove(hand[i]));

            if (isMine)
            {
                // ���ÿ��� ���� ����
                hand[i].originOrder = i + 1;
                hand[i].SetOrder(i + 1);
            }
            
        }

        // ���� �ڷ�ƾ ���� ��������� Ȯ�� �� ���
        yield return new WaitUntil(() => (isMine ? playerMoveCount == 0 : enemyMoveCount == 0));
    }

    // ���� �ִϸ��̼� �ڷ�ƾ
    public IEnumerator HandCardMove(CardHand ch)
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
        //Debug.Log(ch.transform.rotation.eulerAngles);
        float t = 0;
        while (t < 1f) 
        {
            t += Time.deltaTime * 1.5f;
            ch.transform.localRotation =
                Quaternion.Euler(Vector3.Lerp(startRpt, ch.originRot, t));
            ch.transform.localPosition =
                Vector3.Lerp(start, ch.OriginPos, t);
            yield return null;
        }
        ch.Ray = true;

        // �������ο� ����, ���� �ڵ����� �ڷ�ƾ �������� ���ҷ� ǥ�� => ���� ī��Ʈ�� 0�̵Ǹ� ������ ��ο� ���� , �ݺ�
        if (ch.IsMine == true)
        { 
            playerMoveCount -= 1; 
        }
        else
        {
            enemyMoveCount -= 1;
        }
    }
}
