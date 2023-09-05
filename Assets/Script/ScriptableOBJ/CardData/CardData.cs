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
    public string associatedHandler; // 기타 이벤트의 경우, 컴포넌트를 붙여 실행하기로 결정
    public List<CardBaseEvtData> evtDatas = new List<CardBaseEvtData>();

    #region GPT도움
    // 사유 : php로부터 기존 유저가만든 덱데이터를 형성할떄
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