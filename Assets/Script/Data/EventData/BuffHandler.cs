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
    public int[] relatedIds= new int[] { }; // ���õ� �ϼ���ID
    public int buffAtt, buffHp, drawCount; // ���ݷ�,ü��,��ο�� 


}
