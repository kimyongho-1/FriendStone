using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEle : MonoBehaviour
{
    public Define.cardType CardEleType;
    public CardData Data { get; set; }
    public void Init(CardData data)
    {
        if (data == null) { return; }
        Data = data;
        switch (data.cardType)
        {
            case Define.cardType.minion:
                CardEleType = Define.cardType.minion;  
                MC = (MinionCardData)data;
                SC = null; WC = null;
                break;
            case Define.cardType.spell:
                CardEleType = Define.cardType.spell; SC = (SpellCardData)data; MC = null; WC = null; break;
            case Define.cardType.weapon:
                CardEleType = Define.cardType.weapon; WC = (WeaponCardData)data; SC = null; MC = null; break;
        }
    }

    [field: SerializeField] public MinionCardData MC { get; set; }
    [field: SerializeField] public WeaponCardData WC { get; set; }
    [field: SerializeField] public SpellCardData SC { get; set; }

    
    public int GetDataAtt 
    {
        get 
        {
            switch(CardEleType)
            {
                case Define.cardType.minion:
                    return MC.att;
                case Define.cardType.spell:
                    return 0;
                case Define.cardType.weapon:
                    return WC.att;
                    default: return 0;  
            }
        }
    }
    public int GetDataHp
    {
        get
        {
            switch (CardEleType)
            {
                case Define.cardType.minion:
                    return MC.hp;
                case Define.cardType.spell:
                    return 0;
                case Define.cardType.weapon:
                    return WC.durability;
                default: return 0;
            }
        }
    }
}
