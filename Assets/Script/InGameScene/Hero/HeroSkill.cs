using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSkill : MonoBehaviour, IBody
{
    Func<IBody, IBody, IEnumerator> skillFunc;
    public SkillData data ;
    # region
    public IEnumerator onDead { get; set; }
    public bool IsMine { get; set; }
    public int PunId { get; set; }

    public Define.AttType AttType { get; set; }
    public Define.ObjType objType { get; set; }
    public Transform TR { get; set; }

    public Vector3 OriginPos { get; set; }
    public Collider2D Col { get; set; }
    public bool Ray { set { Col.enabled = value; } }
    public int OriginAtt { get; set; }
    public int OriginHp { get; set; }
    public int Att { get; set; }
    public int HP { get; set; }

    #endregion
    public void InitEnemySkill(Hero user, Define.classType type)
    {
        AttType = Define.AttType.None;
        TR = transform;
        Col = GetComponent<CircleCollider2D>();
        user.playerImg.sprite = GAME.Manager.RM.GetHeroImage(type);
        switch (type)
        {
            case Define.classType.HJ:
                data = new HJskill();
                data.Name = "���� ���̱�";
                data.Desc = "���콺�� �� ����� ������ ���ظ� 2 �ݴϴ�";
                GAME.IGM.Hero.Player.skillImg.sprite =
                data.Image = GAME.Manager.RM.GetImage(Define.classType.HJ, 4);
                break;
            case Define.classType.HZ:
                data = new HZskill();
                data.Name = "���� ��Ű��";
                data.Desc = "���콺�� ����� ������ +1/+1�� �ο��մϴ�";
                GAME.IGM.Hero.Player.skillImg.sprite =
                data.Image = GAME.Manager.RM.GetImage(Define.classType.HZ, 15);
                break;
            case Define.classType.KH:
                data = new KHskill();
                data.Name = "������ ���̱�";
                data.Desc = "����� ������ 2ġ���մϴ�";
                GAME.IGM.Hero.Player.skillImg.sprite =
                data.Image = GAME.Manager.RM.GetImage(Define.classType.KH, 28);
                break;
        }
        skillFunc = data.SkillClickEvt;
    }
    public void InitSkill(Hero user)
    {
        AttType = Define.AttType.None;
        TR = transform;
        Col = GetComponent<CircleCollider2D>();

        // ��ų ������ Ŭ����, Ÿ���� �̺�Ʈ ���� ����
        GAME.Manager.UM.BindEvent(this.gameObject, ClickedOnSkill, Define.Mouse.ClickL, Define.Sound.None);
        // �κ񿡼� �ΰ��Ӿ����� ������
        // ResourcesManager���� ����� ������ �� ������Ƽ�� ����
        // ������ ã�� �´� ���� �ɷ����� �ʱ�ȭ
        switch (GAME.Manager.RM.GameDeck.ownerClass)
        {
            case Define.classType.HJ:
                data = new HJskill();
                data.Name = "���� ���̱�";
                data.Desc = "���콺�� �� ����� ������ ���ظ� 2 �ݴϴ�";
                GAME.IGM.Hero.Player.skillImg.sprite = 
                data.Image = GAME.Manager.RM.GetImage(Define.classType.HJ,4);
                break;
            case Define.classType.HZ:
                data = new HZskill();
                data.Name = "���� ��Ű��";
                data.Desc = "���콺�� ����� ������ +1/+1�� �ο��մϴ�";
                GAME.IGM.Hero.Player.skillImg.sprite =
                data.Image = GAME.Manager.RM.GetImage(Define.classType.HZ, 15);
                break;
            case Define.classType.KH:
                data = new KHskill();
                data.Name = "������ ���̱�";
                data.Desc = "����� ������ 2ġ���մϴ�";
                GAME.IGM.Hero.Player.skillImg.sprite =
                data.Image = GAME.Manager.RM.GetImage(Define.classType.KH, 28);
                break;
        }
        skillFunc = data.SkillClickEvt;
    }

    public void ClickedOnSkill(GameObject go)
    {
        // ���� ��Ȱ��ȭ : �̹� ����Ͽ��⿡ ���� �ڽ��� ���� ������� �ش� �̺�Ʈ ���� �Ұ�
        // Ÿ���� �ڷ�ƾ�� Null�� �ƴҽ� : ���� �ٸ� �巡��Ŭ���� ��ų������Ʈ�� Ŭ���� ���
        if (Col.enabled == false || GAME.IGM.TC.Arrow.gameObject.activeSelf == true)
        { return; }

        // ������ �ڽŰ�, �������� ���� ��Ȱ��ȭ
        GAME.IGM.Spawn.SpawnRay = Ray = false;

        // Ÿ���� ī�޶� ���� + ���� Ÿ���� ������ �����Լ� ���� ����
        GAME.IGM.TC.StartCoroutine(GAME.IGM.TC.TargettingCo(this, skillFunc, new string[] { "foe" }));
    }


}

public abstract class SkillData
{
    public abstract IEnumerator SkillClickEvt(IBody attacker, IBody target);
    public string Desc,Name;
    public Sprite Image;
}

public class HJskill : SkillData
{
    public override IEnumerator SkillClickEvt(IBody attacker, IBody target)
    {
        // ��󿡰� 2 �����ֱ�
        yield return null;
    }
}
public class HZskill : SkillData
{
    public override IEnumerator SkillClickEvt(IBody attacker, IBody target)
    {
        // ������ �Ʊ����� +1/+1 ����
        yield return null;
    }
}
public class KHskill : SkillData
{
    public override IEnumerator SkillClickEvt(IBody attacker, IBody target)
    { 
        // ����� 2ġ��
        yield return null;
    }
}