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
// 현재 모든 카드데이터에 접근할수있는 경로추적의 제이슨파일 구조
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

        // 이벤트데이터, 유저선택 타겟팅을 항상 0번쨰 순위로 옮겨주기
        evtDatas = evtDatas.OrderByDescending(x => x.targeting == Define.evtTargeting.Select).ToList();
    }

    #region GPT도움
    // 사유 : php로부터 기존 유저가만든 덱데이터를 형성할??
    // 덱 내부의 딕셔너리에 들어가는 카드데이터를 올바르게 형변환을 분명하였음에도
    // DeckViewPort.cs내 딕셔너리의 Contains함수를 사용하여도
    // 플레이모드 종료후 재접속시, 동일한 카드도 다른 카드로 인식하는 현상을 발견
    // gpt통해서 딕셔너리가 아래 GetHashCode와 Equals 메서드 두함수를 사용해 포함여부를 결정한다고 한다
    // 따라서 오버라이딩하여서 , cardIdNum을 통해 비교하여서 현재 올바르게 작동되는것 확인
    public override bool Equals(object obj)
    {
        if (obj is CardData other)
        {
            // 필요한 필드를 모두 비교
            return this.cardIdNum == other.cardIdNum;
        }
        return false;
    }

    public override int GetHashCode()
    {
        // 필요한 필드로부터 해시 코드를 생성
        return (this.cardIdNum).GetHashCode();
    }
    #endregion

}

[Serializable]
public class MinionCardData : CardData
{
    public int att, hp;
    public bool isTaunt; // 도발 유닛인지
    public bool isCharge; // 돌진 유닛인지

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