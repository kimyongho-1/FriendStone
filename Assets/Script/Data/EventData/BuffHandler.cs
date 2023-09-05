using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuffHandler : CardBaseEvtData
{
    public Define.buffTargeting buffTargeting;
    public Define.buffType buffType;
    public Define.buffExtraArea buffExtraArea;
    public Define.buffFX buffFX;
    public int[] relatedIds= new int[] { }; // 관련된 하수인ID
    public int buffAtt, buffHp, drawCount; // 공격력,체력,드로우순 


}
