using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardBaseEvtData 
{
    public Define.evtWhen when; // �̺�Ʈ�� �ߵ��� ����
    public Define.evtArea area; // �ڵ� ��������� ���� ( NONE ), �ƴ϶�� ������ �� ���� ���� ����
    public Define.evtType type; // � �̺�Ʈ���� : ġ�� ���� ��ο�..���
}
