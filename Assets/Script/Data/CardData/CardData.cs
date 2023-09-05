using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class cardInfo
{ 
    public string  path;
    public Define.cardType type;
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
    public string associatedHandler; // ��Ÿ �̺�Ʈ�� ���, ������Ʈ�� �ٿ� �����ϱ�� ����
    public List<CardBaseEvtData> evtDatas = new List<CardBaseEvtData>();

    #region GPT����
    // ���� : php�κ��� ���� ���������� �������͸� �����ҋ�
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
}

[Serializable]
public class SpellCardData : CardData
{

}

[Serializable]
public class WeaponCardData : CardData
{
    public int att, durability;
}