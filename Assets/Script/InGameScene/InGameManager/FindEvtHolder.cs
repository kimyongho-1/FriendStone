using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class FindEvtHolder : MonoBehaviour
{
    public CardHand prefab;
    public Collider2D left, center, right;
    public List<CardHand> list = new List<CardHand>();
    private void Awake()
    {
        GAME.IGM.FindEvt = this;

        // �߰� �̺�Ʈ��, ī�� ������ Ȯ���ϴ� Ŭ�� �̺�Ʈ ����
        GAME.Manager.UM.BindEvent(left.gameObject, ClickedCard, Define.Mouse.ClickL, Define.Sound.Click);
        GAME.Manager.UM.BindEvent(center.gameObject, ClickedCard, Define.Mouse.ClickL, Define.Sound.Click);
        GAME.Manager.UM.BindEvent(right.gameObject, ClickedCard, Define.Mouse.ClickL, Define.Sound.Click);

        gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        int start = Mathf.Clamp(list.Count,0,3);
        for (int i = start; i < 3; i++) 
        {
            // �ΰ��� ī�� ������ ����
            CardHand ch = GameObject.Instantiate(prefab, this.transform);
            ch.transform.localScale = Vector3.one * 0.75f;
            ch.transform.localPosition = new Vector3( (i-1) * 4f, 1.25f, -1);
            list.Add(ch);
        }
    }

    public bool CurrSelected = false;

    // �߰� �̺�Ʈ ���۽�, ������ �غ�
    public void ReadyFindEvt(int[] puns)
    {
        CurrSelected = false;
        // �߰� ī��Ǯ ������ �ʱ�ȭ
        for (int i = 0; i < 3; i++)
        {
            // ShowEnemyFindEvt�Լ� �����ĸ� ����ؼ� �� ����
            list[i].cardImage.gameObject.SetActive(true);
            list[i].TMPgo.gameObject.SetActive(true);
            list[i].cardBackGround.sprite = GAME.Manager.RM.GetCardSprite(true);
            list[i].transform.localScale = Vector3.one * 0.3f;

            // ���ҽ� �Ŵ����� ��θ� ��ȯ �޴� ��ųʸ� ���� ī��Ÿ�԰� ī�嵥���� ã��
            Define.cardType type = GAME.Manager.RM.PathFinder.Dic[puns[i]].type;
            string jsonFile = GAME.Manager.RM.PathFinder.Dic[puns[i]].GetJson();

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

            CardHand ch = list[i];
            
            ch.Init(card);
            ch.PunId = GAME.IGM.Hand.CreatePunNumber();
            ch.SetOrder(1000);
        }
        // ���� ����� ī�� ��ȣ�ۿ� �ϸ� �ȵǱ⿡ ���� ����
        left.enabled = right.enabled = center.enabled = true;
        this.gameObject.SetActive(true);
    }

    public void ClickedCard(GameObject go)
    {
        // Ŭ�� ������Ʈ �̸��� ���� ����Ʈ�� �ε��� ã��
        string name = go.name;
        CardHand ch;
        int findIdx = 0;
        if (name.StartsWith("L")) 
        {
            ch = list[0];
            findIdx = 0;
        }

        else if (name.StartsWith("C")) 
        {
            ch = list[1];
            findIdx = 1;
        }

        else 
        {
            ch = list[2];
            findIdx = 2;
        }

        // �߰� ����Ʈ���� ����
        list.Remove(ch);
        // �ĺ��� �ʱ�ȭ
        ch.PunId = GAME.IGM.Hand.CreatePunNumber();

        // ���� ���ؼ� ������ �ڵ�� �̵�
        GAME.IGM.Hand.PlayerHand.Add(ch);
        ch.transform.SetParent(GAME.IGM.Hand.PlayerHandGO.transform);
        CurrSelected = true;

        // ������ ī�尡 �ڵ忡 �ڸ����������� ���
        GAME.IGM.StartCoroutine(GAME.IGM.Hand.CardAllignment());

        // ��뿡�� �� �߰� �̺�Ʈ ����� �����ϱ� [ ���� ����� ī�带 �����ߴ��� + �� ī���� �ݳѹ��� �������� ]
        GAME.IGM.Packet.SendResultFindEvt( findIdx , ch.PunId);

        // �ݱ�
        this.gameObject.SetActive(false) ;
    }   
    
    // ��밡 �߰� �ڷ�ƾ�� �����ҋ�, �� ȭ�鿡�� �Ȱ��� ǥ�����ֱ�
    public IEnumerator ShowEnemyFindEvt()
    {
        CurrSelected = false;
        for (int i = 0; i < 3; i++)
        {
            // ��� ī�� �޸����� ǥ��
            list[i].cardBackGround.sprite = GAME.Manager.RM.GetCardSprite(false);
            list[i].cardBackGround.sortingOrder = 1000;
            // �޸鸸 ������ �Ǳ⿡, ������ ���ֱ�
            list[i].cardImage.gameObject.SetActive(false);
            list[i].TMPgo.gameObject.SetActive(false);
            list[i].transform.localScale = Vector3.one * 0.4f;
        }
        // ���� ����� ī�� ��ȣ�ۿ� �ϸ� �ȵǱ⿡ ���� ����
        left.enabled = right.enabled = center.enabled = false;
        this.gameObject.SetActive(true);
        yield return null;
    }

    // ��밡 �߰��̺�Ʈ�� �����Ͽ� ������, �� ȭ�鿡�� �� ī�带 ���� ���� �ڷ�ƾ
    public IEnumerator ShowEnemyFindEvtResult(int idx, int punID)
    {
        // ī�� ã�� + �ĺ��� �ʱ�ȭ
        CardHand ch = list[idx];
        ch.PunId = punID;
        ch.IsMine = false;
        ch.originScale = (ch.IsMine) ? Vector3.one * 0.3f : new Vector3(0.16f, 0.17f, 0.3f);
        // �� ����Ʈ���� ����
        list.Remove(ch);
        // ���� ���ؼ� ���� �ڵ�� �̵�
        GAME.IGM.Hand.EnemyHand.Add(ch);
        ch.transform.SetParent(GAME.IGM.Hand.EnemyHandGO.transform);

        // �ش� ī�� �� �ڵ�� ���� ���� ���ֱ� 
        GAME.IGM.StartCoroutine(GAME.IGM.Hand.CardAllignment());
        yield return null;

        // �߰� �̺�Ʈ â ����
        this.gameObject.SetActive(false);
    }
}
