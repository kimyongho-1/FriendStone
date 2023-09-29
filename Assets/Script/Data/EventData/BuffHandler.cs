using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuffHandler : CardBaseEvtData
{
    public Define.buffAutoMode buffAutoMode;
    public Define.buffType buffType;
    public Define.buffFX buffFX;
    public int[] relatedIds= new int[] { }; // 관련된 하수인ID
    public int buffAtt, buffHp, costCount; // 공격력,체력,비용 순 


}
