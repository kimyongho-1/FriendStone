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

// ���̽� �Ľ̿� Ŭ����
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
        // ��ȯ�� �� ����Ÿ
        DeckData deckData = new DeckData();
        deckData.deckCode = deckCode;   
        deckData.deckName = deckName;
        deckData.ownerClass = (Define.classType)classType;
        for (int i = 0; i < protoList.Count; i++) 
        {
            // ���������� ��ο� �������� Ÿ��
            cardInfo info = GAME.Manager.RM.PathFinder.Dic[protoList[i].CardID];
            CardData cd = null;
            switch (info.type)
            {
                case Define.cardType.minion:
                    // ī��ID�� Ű���̸�, ���̽����� ��θ� ��ȯ�ϴ� ��ųʸ� ���
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
            
            
              // id�ĺ���ȣ ���ؼ� ī�嵥���Ϳ� ����� ã�� ����
             deckData.cards.Add(cd, protoList[i].CardAmount);
        }

        return deckData;
    }
}