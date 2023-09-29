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
    public CardPopupEvtHolder cardPopup;
    public TextMeshPro cardName, Description, Stat, Type, Cost;
    public SpriteRenderer cardImage,cardBackground;

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
        Cost.text = "2";
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
        Cost.text = data.cost.ToString();
        cardImage.sprite = sprite;
        cardPopup.gameObject.SetActive(true);
    }
    public void ShowSpellPopup(SpellCardData data, Vector3 pos)
    {
        // ���� ��������ī�尡 �ִٰ� ������, �ٸ� ī�� ���� �̺�Ʈ ����
        cardPopup.isEnmeySpawning = true;

        cardPopup.transform.position = pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = $"<color=black>�ֹ�";
        Type.text = data.cardType.ToString();
        Cost.text = data.cost.ToString();
        cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum );
        StartCoroutine(FadeIn());
        IEnumerator FadeIn()
        {
            float t = 0;
            // ����ȭ ���� ��� TMP�� SR�� ����
            List<TextMeshPro> tmpList = new List<TextMeshPro>() { cardName, Description, Cost, Stat, Type };
            List<SpriteRenderer> imageList = new List<SpriteRenderer>() { cardImage, cardBackground };

            cardPopup.gameObject.SetActive(true);

            tmpList.ForEach(x => x.alpha = 0);
            imageList.ForEach(x => x.color = new Color(1, 1, 1, 0));
            Color tempColor = Color.white;
            while (t < 1f)
            {
                // ���İ� ���� 1���� ��ȯ
                t += Time.deltaTime;
                tempColor.a = t;
                tmpList.ForEach(x => x.alpha = t);
                imageList.ForEach(x => x.color = tempColor);
                yield return null;
            }
        }
    }

    public void ShowCardPopup(ref WeaponCardData data, Vector3 pos)
    {
        Stat.gameObject.SetActive(true);
        cardPopup.transform.position = pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = "";
        Type.text = data.cardType.ToString();
        Cost.text = data.cost.ToString();
        cardImage.sprite = null;
        cardPopup.gameObject.SetActive(true);
    }
}
