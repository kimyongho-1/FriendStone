using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UtillHandler : CardBaseEvtData
{
    public Define.utillType utillType; // � �����̺�Ʈ����
    public int[] relatedCards = new int[] { }; // ȹ���ϰų� �߰� ī����� ������ȣ �����, �Ŀ� ��ȣ ���ؼ� ã�� ���
    public int utillAmount; // ��Ÿ �Ǵ� ��ο� ��ġ
}
