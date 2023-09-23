using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindEvtHolder : MonoBehaviour
{
    public CardHand prefab;
    public Collider2D left, center, right;
    public List<CardHand> list = new List<CardHand>();
    private void Awake()
    {
        GAME.Manager.IGM.FindEvt = this;

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
            ch.Init(ref card);
            ch.SetOrder(1000);
        }
        this.gameObject.SetActive(true);
    }

    public void ClickedCard(GameObject go)
    {
        // Ŭ�� ������Ʈ �̸��� ���� ����Ʈ�� �ε��� ã��
        string name = go.name;
        CardHand ch;
        if (name.StartsWith("L")) 
        {
          ch = list[0];
        }

        else if (name.StartsWith("C")) 
        {
            ch = list[1];
        }

        else 
        {
            ch = list[2];
        }

        // �߰� ����Ʈ���� ����
        list.Remove(ch);
        // �ĺ��� �ʱ�ȭ
        ch.PunId = (Photon.Pun.PhotonNetwork.IsMasterClient ? 1000 : 2000)
                    + GAME.Manager.IGM.Hand.punConsist++;

        // ���� ���ؼ� ������ �ڵ�� �̵�
        GAME.Manager.IGM.Hand.PlayerHand.Add(ch);
        ch.transform.SetParent(GAME.Manager.IGM.Hand.PlayerHandGO.transform);
        CurrSelected = true;

        //GAME.Manager.IGM.Hand.StopAllCoroutines();
        // ������ ī�尡 �ڵ忡 �ڸ����������� ���
        StartCoroutine(GAME.Manager.IGM.Hand.CardAllignment());
        // �ݱ�
        this.gameObject.SetActive(false) ;
    }
}
