using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardBaseEvtData 
{
    public Define.evtWhen when; // �̺�Ʈ�� �ߵ��� ���� : ī�带 �տ��� ����, �̴Ͼ�ī���� ������.. ���
    public Define.evtTargeting targeting; // �տ��� ���� �ڵ� ��������, �����ؾ��ϴ���
    public Define.evtArea area; // ��� ������ ������� : ���� | �� | ���
    public Define.evtFaction faction; // ������ � Ÿ�� : ������ | �̴Ͼ� | ���

    public Define.evtType type; // � �̺�Ʈ���� : ġ�� ���� ��ο�..���
}
