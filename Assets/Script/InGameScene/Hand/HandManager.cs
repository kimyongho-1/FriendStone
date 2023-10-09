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
// 1. Ż���� 10�� �ʰ��� �ڷ�ƾ �ִϸ��̼� �ʿ�
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
    int fatigueStack = 0; // Ż������ : ���� ī�尡 ������ Ż���ڷ�ƾ�� �����ϸ� ����� 1�� ������ ���ط��� �� �������� �ο��ϴ� �̺�Ʈ
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
                // ���ο� ������ �����Ͽ� ���� �߰����ֱ� ( ��� �����Ͱ� �������� �����Ͽ� ������ ��ħ ���� ����)
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

    // �ڵ尡 �̹� 10���ϋ� ī�带 ��ο��, �ش� ��ο�ī�� �Ҹ� �̺�Ʈ
    public IEnumerator HandOverFlow(int cardIdNum, CardHand ch)
    {
        #region �Ҹ��� ī�� �̵�
        ch.transform.localScale = Vector3.one * 0.6f; // ũ�� Ȯ��
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

        #region ��ġ ���޽�, �Ҹ�(����ȭ ����)
        // ����ȭ ���� ��� TMP�� SR�� ����
        List<TextMeshPro> tmpList = new List<TextMeshPro>() { ch.cardName, ch.Description, ch.Cost, ch.Stat, ch.Type };
        List<SpriteRenderer> imageList = new List<SpriteRenderer>() { ch.cardImage, ch.cardBackGround };
        t = 1;
        Color tempColor = Color.white;
        while (t > 0f)
        {
            // ���İ� ���� 0���� ��ȯ
            t -= Time.deltaTime;
            tempColor.a = t;
            tmpList.ForEach(x => x.alpha = t);
            imageList.ForEach(x => x.color = tempColor);
            yield return null;
        }
        // ��뿡�� �� ������ο� �̺�Ʈ ����
        GAME.IGM.Packet.SendOverDrawInfo(cardIdNum);
        GameObject.Destroy(ch.gameObject);
        #endregion 
    } 
    public IEnumerator EnemyHandOverFlow(int cardIdNum)
    {
        #region ���� �Ҹ� �̺�Ʈ�� �״�� �����ϱ� ����, �Ҹ��� ī�� ������ ã�� + ����
        // ���� ī��Ҹ� �̺�Ʈ�� �Ȱ��� �����Ͽ� ����ȭ���ֱ�
        // ���ҽ� �Ŵ����� ��θ� ��ȯ �޴� ��ųʸ� ���� ī��Ÿ�԰� ī�嵥���� ã��
        Define.cardType type = GAME.Manager.RM.PathFinder.Dic[cardIdNum].type;
        string jsonFile = GAME.Manager.RM.PathFinder.Dic[cardIdNum].GetJson();
        CardData card = null;
        // Ȯ�ε� ī��Ÿ������, ���� ī��Ÿ������ Ŭ����ȭ
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
        // �ΰ��� ī�� ������ ����
        CardHand ch = GameObject.Instantiate(prefab, EnemyHandGO.transform);
        ch.Init(card, true);
        #endregion

        #region �Ҹ��� ī�� �̵�
        ch.transform.localScale = Vector3.one * 0.6f; // ũ�� Ȯ��
        Vector3 start = new Vector3(8.5f, 3.2f, -0.5f);
        Vector3 dest = new Vector3(4.5f, 3.2f, -0.5f); //  ���� �Ҹ� ī�� ��ġ�� ���� ����
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            ch.transform.position =
                Vector3.Lerp(start, dest, t);
            yield return null;
        }
        #endregion

        #region ��ġ ���޽�, �Ҹ�(����ȭ ����)
        // ����ȭ ���� ��� TMP�� SR�� ����
        List<TextMeshPro> tmpList = new List<TextMeshPro>() { ch.cardName, ch.Description, ch.Cost, ch.Stat, ch.Type };
        List<SpriteRenderer> imageList = new List<SpriteRenderer>() { ch.cardImage, ch.cardBackGround };
        t = 1;
        Color tempColor = Color.white;
        while (t > 0f)
        {
            // ���İ� ���� 0���� ��ȯ
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
            // ���� ī�尡 ���ٸ�
            if (deckCards.Count == 0)
            {
                yield return StartCoroutine(Fatigue.DeckExhausted(true, ++fatigueStack ));
            }

            // ���� ī�尡 �ִٸ�
            else
            {
                // ������ ������ ���� �ڷ�ƾ ���� ����
                DrawingCo = DeckAnimCo();
                StartCoroutine(DrawingCo);

                // ������ ���������� ���� ī���� ������
                CardData cd = deckCards.Dequeue();
                switch (cd.cardType)
                {
                    case Define.cardType.weapon: cd = new WeaponCardData(cd); break;
                    case Define.cardType.minion: cd = new MinionCardData(cd); break;
                    case Define.cardType.spell: cd = new SpellCardData(cd); break;
                }

                // �ΰ��� ī�� ������ ����
                CardHand ch = GameObject.Instantiate(prefab, PlayerHandGO.transform);
                // �ΰ��� ī�� �ʱ�ȭ
                ch.Init(cd, true);

                // ���� ���а� �̹� 10���� ���¿��� ��ο� ��Ȳ�̶�� => ���� ������ ī�� ���� �̺�Ʈ ����
                if (PlayerHand.Count == 10)
                {
                    yield return new WaitUntil(() => (DrawingCo == null));
                    yield return GAME.IGM.StartCoroutine(HandOverFlow(cd.cardIdNum, ch));
                }

                // ���� ī�带 �̾�, 10�� �̸��� �ڵ�� �������� �������� ��ο� ��Ȳ
                else 
                {
                    // ���� �ĺ��� �ѹ����ϱ� + ���� �̴Ͼ�ī�尡 ��ȯ�ɽ� �ڵ�ī���� �ݳѹ� �Ѱܹ޾� ���
                    ch.PunId = CreatePunNumber();

                    // OnHand �̺�Ʈ ���� Ȯ�� �� ����
                    List<CardBaseEvtData> evtList = cd.evtDatas.FindAll(x => x.when == Define.evtWhen.onHand);
                    if (evtList != null)
                    {
                        for (int j = 0; j < evtList.Count; j++)
                        {
                            GAME.IGM.AddAction(GAME.IGM.Battle.Evt(evtList[j], ch));
                        }
                    }
                    // ��뿡�� �� ��ο� ���� ����
                    GAME.IGM.Packet.SendDrawInfo(ch.PunId);

                    // ���� �ڵ�ī�忡 ���Խ�Ű��
                    PlayerHand.Add(ch);
                    yield return new WaitUntil(() => (DrawingCo == null));

                    // ������ ī��� ���� ������, ������ ��ο�
                    yield return StartCoroutine(CardAllignment(true));
                }
            }
            
        }
        // ��ο�� ���� ��� �������� �ݶ��̴��ۿ� ���� �ٽ� Ȱ��ȭ
        GAME.IGM.Hand.PlayerHand.ForEach(x=>x.Ray = true);
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
        List<CardHand> hand = (isMine) ? PlayerHand.ToList() : EnemyHand.ToList();
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
            // ť ���ҽ�Ű�� ( ť ���� 0 �Ͻ� , ��� �ڵ�ī�� ���� �ڷ�ƾ �������� �Ͻ�)
            co.Dequeue();
        }
    }

    
}
