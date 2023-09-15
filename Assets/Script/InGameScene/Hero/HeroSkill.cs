using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSkill : MonoBehaviour, IBody
{
    Hero owenrUser;
    Func<IBody, IBody, IEnumerator> skillFunc;
    # region
    public bool IsMine { get; set; }
    public int PunId { get; set; }

    public Define.BodyType bodyType { get; set; }

    public Transform TR { get; set; }

    public Vector3 OriginPos { get; set; }
    public Collider2D Col { get; set; }
    public bool Ray { set { Col.enabled = value; } }

#endregion

    public void InitSkill(Hero user)
    {
        bodyType = Define.BodyType.HeroSkill;
        TR = transform;
        Col = GetComponent<CircleCollider2D>();

        // ��ų ������ Ŭ����, Ÿ���� �̺�Ʈ ���� ����
        GAME.Manager.UM.BindEvent(this.gameObject, ClickedOnSkill, Define.Mouse.ClickL, Define.Sound.None);
        // �κ񿡼� �ΰ��Ӿ����� ������
        // ResourcesManager���� ����� ������ �� ������Ƽ�� ����
        // ������ ã�� �´� ���� �ɷ����� �ʱ�ȭ
        owenrUser = user;   
        switch (GAME.Manager.RM.GameDeck.ownerClass)
        {
            case Define.classType.HJ: skillFunc = (IBody a, IBody t) => { return HJClick(a, t); }; break;
            case Define.classType.HZ: skillFunc = (IBody a, IBody t) => { return HZClick(a, t); }; break;
            case Define.classType.KH: skillFunc = (IBody a, IBody t) => { return KHClick(a, t); }; break;
        }
    }

    public void ClickedOnSkill(GameObject go)
    {
        // ���� ��Ȱ��ȭ : �̹� ����Ͽ��⿡ ���� �ڽ��� ���� ������� �ش� �̺�Ʈ ���� �Ұ�
        // Ÿ���� �ڷ�ƾ�� Null�� �ƴҽ� : ���� �ٸ� �巡��Ŭ���� ��ų������Ʈ�� Ŭ���� ���
        if (Col.enabled == false || GAME.Manager.IGM.TC.TargetCo != null)
        { return; }

        // ������ �ڽŰ�, �������� ���� ��Ȱ��ȭ
        GAME.Manager.IGM.Spawn.SpawnRay = Ray = false;

        // ���� Ÿ���� �ڷ�ƾ ��� �� ����
        GAME.Manager.IGM.TC.TargetCo = GAME.Manager.IGM.TC.TargettingCo(this, skillFunc);

        // Ÿ���� ī�޶� ���� + ���� Ÿ���� ������ �����Լ� ���� ����
        GAME.Manager.IGM.TC.StartCoroutine(GAME.Manager.IGM.TC.TargetCo);
    }


    public IEnumerator HJClick(IBody attacker, IBody target)
    {
        // ��󿡰� 2 �����ֱ�
        yield return null;
    }     
    public IEnumerator HZClick(IBody attacker, IBody target)
    {
        // ������ �Ʊ����� +1/+1 ����
        yield return null; 
    }   
    public IEnumerator KHClick(IBody attacker, IBody target)
    { 
        // ����� 2ġ��
        yield return null; 
    }
}
