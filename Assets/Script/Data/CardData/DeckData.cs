using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

[Serializable]
public class DeckData
{
    public string deckCode;
    public string deckName;
    public Define.classType ownerClass;
    public Dictionary<CardData, int> cards = new Dictionary<CardData, int>();

    public int GetCount() { return cards.Values.Sum(); }

}

// 제이슨 파싱용 클래스
[Serializable]
public class DeckDataPrototype
{
    public string deckCode;
    public string deckName;
    public int classType;

    public List<cardProto> protoList = new List<cardProto>();
    public class cardProto
    {
        public int CardID;
        public int CardAmount;
    }

    public DeckData Deserialize()
    {
        // 반환할 덱 데이타
        DeckData deckData = new DeckData();
        deckData.deckCode = deckCode;   
        deckData.deckName = deckName;
        deckData.ownerClass = (Define.classType)classType;
        for (int i = 0; i < protoList.Count; i++) 
        {
            // 데이터파일 경로와 데이터의 타입
            cardInfo info = GAME.Manager.RM.PathFinder.Dic[protoList[i].CardID];
            CardData cd = null;
            switch (info.type)
            {
                case Define.cardType.minion:
                    // 카드ID가 키값이며, 제이슨파일 경로를 반환하는 딕셔너리 사용
                    cd = JsonConvert.DeserializeObject<MinionCardData>
                        (Resources.Load<TextAsset>(info.path).ToString()
                        , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    break;
                case Define.cardType.spell:
                    cd = JsonConvert.DeserializeObject<SpellCardData>
                        (Resources.Load<TextAsset>(info.path).ToString()
                        , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    break;
                case Define.cardType.weapon:
                    cd = JsonConvert.DeserializeObject<WeaponCardData>
                        (Resources.Load<TextAsset>(info.path).ToString()
                        , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    break;
            }
            
            
              // id식별번호 통해서 카드데이터와 몇개인지 찾아 대입
             deckData.cards.Add(cd, protoList[i].CardAmount);
        }

        return deckData;
    }
}