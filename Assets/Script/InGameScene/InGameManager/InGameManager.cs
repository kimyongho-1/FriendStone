using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class CustomList : List<CardField>
{
    public new void Insert(int idx ,CardField ele)
    {
        // �⺻ Insert �Լ� ������
        base.Insert(idx, ele);
        Debug.Log($"AllBody List�� {ele.data.cardName},[{ele.PunId}] �� �߰� ��");

        // �� ���� ������ LIST���� �߰�
        GAME.IGM.allIBody.Add(ele);
    }

    public new void Remove(CardField ele) 
    {
        base.Remove(ele);
        GAME.IGM.allIBody.Remove(ele);
    }
}

public class InGameManager : MonoBehaviour
{
    // ���ӳ� ü�� ��ȣ�ۿ��� �̷�� ������, �̴Ͼ���� ���⿡�� ���� ã������ ����
    public List<IBody> allIBody = new List<IBody>();

    public int GameTurn = 0;
    public GameObject cardPopup;
    public TextMeshPro cardName, Description, Stat, Type, cost;
    public SpriteRenderer cardImage;

    #region ����
    public FindEvtHolder FindEvt { get; set; }
    public HeroManager Hero { get; set; }
    public HandManager Hand { get; set; }
    public SpawnManager Spawn { get; set; }
    public TargetingCamera TC { get; set; }
    public BattleManager Battle { get; set; }
    public TurnEndBtn Turn { get; set; }
    public PacketManager Packet { get; set; }
    private void Awake()
    {
        GAME.IGM = this;
    }
    #endregion


    // BattleManager�� �׼�ť ������ ���ٿ뵵
    public void AddAction(IEnumerator co) { Battle.ActionQueue.Enqueue(co); }

    // ���� �ɷ� �˾�â ȣ��
    public void ShowHeroSkill(Vector3 pos, SkillData skill)
    {
        cardPopup.transform.position = pos;
        cardName.text = skill.Name;
        Description.text = skill.Desc;
        Stat.gameObject.SetActive(false);
        Type.text = "skill";
        cost.text = "2";
        cardImage.sprite = skill.Image;
        cardPopup.gameObject.SetActive(true);
    }

    // ī���˾� ȣ��
    public void ShowMinionPopup(MinionCardData data, Vector3 pos, Sprite sprite)
    {
        Stat.gameObject.SetActive(true);
        cardPopup.transform.position= pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        // ���� ���ڰ� 25���� �̻��̸� ��Ʈũ�⸦ �ణ ���̱�
        Debug.Log("���� ���� : "+data.cardDescription.Length);
        Description.fontSize = (data.cardDescription.Length > 39) ? 15f : 18f;
        Stat.text = $"<color=yellow>ATT {data.att} <color=red>HP {data.hp} <color=black>����";
        Type.text = data.cardType.ToString();
        cost.text = data.cost.ToString();
        cardImage.sprite = sprite;
        cardPopup.gameObject.SetActive(true);
    }
    public void ShowSpellPopup(SpellCardData data, Vector3 pos)
    {
        cardPopup.transform.position = pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = $"<color=black>�ֹ�";
        Type.text = data.cardType.ToString();
        cost.text = data.cost.ToString();
        cardImage.sprite = null;
        cardPopup.gameObject.SetActive(true);
    }

    public void ShowCardPopup(ref WeaponCardData data, Vector3 pos)
    {
        Stat.gameObject.SetActive(true);
        cardPopup.transform.position = pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = "";
        Type.text = data.cardType.ToString();
        cost.text = data.cost.ToString();
        cardImage.sprite = null;
        cardPopup.gameObject.SetActive(true);
    }
}
