using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuffHandler : CardBaseEvtData
{
    public Define.buffAutoMode buffAutoMode;
    public Define.buffType buffType;
    public Define.buffFX buffFX;
    public int[] relatedIds= new int[] { }; // ���õ� �ϼ���ID
    public int buffAtt, buffHp, costCount; // ���ݷ�,ü��,��� �� 


}
