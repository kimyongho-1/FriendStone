using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class cardInfo
{
    public string path;
    public Define.cardType type;
    public string GetJson() { return Resources.Load<TextAsset>(path).ToString(); }
}
// ���� ��� ī�嵥���Ϳ� �����Ҽ��ִ� ��������� ���̽����� ����
public class CardPath
{
    [JsonProperty("Dic")]
    public Dictionary<int, cardInfo> Dic { get; set; }
}
[Serializable]
public class CardData
{
    public string cardName;
    public string cardDescription;
    public int cardIdNum;
    public int cost;
    public Define.classType cardClass;
    public Define.cardType cardType;
    public Define.cardRarity cardRarity;
    public List<CardBaseEvtData> evtDatas = new List<CardBaseEvtData>();

    public CardData() { }
    public CardData(CardData cd)
    {
        cardName = cd.cardName;
        cardDescription = cd.cardDescription;
        cardIdNum = cd.cardIdNum;
        cost =cd.cost;
        cardClass = cd.cardClass;
        cardType = cd.cardType;
        cardRarity = cd.cardRarity;
        evtDatas = cd.evtDatas;

        // �̺�Ʈ������, �������� Ÿ������ �׻� 0���� ������ �Ű��ֱ�
        evtDatas = evtDatas.OrderByDescending(x => x.targeting == Define.evtTargeting.Select).ToList();
    }

    #region GPT����
    // ���� : php�κ��� ���� ���������� �������͸� ������??
    // �� ������ ��ųʸ��� ���� ī�嵥���͸� �ùٸ��� ����ȯ�� �и��Ͽ�������
    // DeckViewPort.cs�� ��ųʸ��� Contains�Լ��� ����Ͽ���
    // �÷��̸�� ������ �����ӽ�, ������ ī�嵵 �ٸ� ī��� �ν��ϴ� ������ �߰�
    // gpt���ؼ� ��ųʸ��� �Ʒ� GetHashCode�� Equals �޼��� ���Լ��� ����� ���Կ��θ� �����Ѵٰ� �Ѵ�
    // ���� �������̵��Ͽ��� , cardIdNum�� ���� ���Ͽ��� ���� �ùٸ��� �۵��Ǵ°� Ȯ��
    public override bool Equals(object obj)
    {
        if (obj is CardData other)
        {
            // �ʿ��� �ʵ带 ��� ��
            return this.cardIdNum == other.cardIdNum;
        }
        return false;
    }

    public override int GetHashCode()
    {
        // �ʿ��� �ʵ�κ��� �ؽ� �ڵ带 ����
        return (this.cardIdNum).GetHashCode();
    }
    #endregion

}

[Serializable]
public class MinionCardData : CardData
{
    public int att, hp;
    public bool isTaunt; // ���� ��������
    public bool isCharge; // ���� ��������

    public MinionCardData() { }
    public MinionCardData(CardData cd) : base(cd)
    {
        MinionCardData mc = (MinionCardData)cd;
        att = mc.att;
        hp = mc.hp;
        isTaunt = mc.isTaunt;
        isCharge = mc.isCharge;
    }
}

[Serializable]
public class SpellCardData : CardData
{
    public SpellCardData() { }
    public SpellCardData(CardData cd) : base(cd)
    {
    }
}

[Serializable]
public class WeaponCardData : CardData
{
    public int att, durability;

    public WeaponCardData() { }
    public WeaponCardData(CardData cd) : base(cd)
    {
        WeaponCardData wc = (WeaponCardData)cd;
        att = wc.att;
        durability = wc.durability; 
    }
}