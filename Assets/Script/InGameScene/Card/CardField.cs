using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEngine.GraphicsBuffer;
using UnityEditor.Experimental.GraphView;

public class CardField : CardEle
{
    public TextMeshPro AttTmp, HpTmp;
    public SpriteRenderer  cardImage;
    public bool attackable = true;
    public ParticleSystem sleep;
    
    public void Init(CardData dataParam)
    {
        attackable = true;
        Col = GetComponent<CircleCollider2D>();
        if (dataParam is MinionCardData)
        {
            MinionCardData minionCardData = (MinionCardData)dataParam;
            data = dataParam;
            AttTmp.text = minionCardData.att.ToString();
            HpTmp.text = minionCardData.hp.ToString();
            cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);
        }
        else
        {
            data = dataParam;
            cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);
        }

        // ��Ŭ���� ī�޶�� ����Ȱ��ȭ�Ͽ� Ÿ���� �̺�Ʈ ����
        GAME.Manager.UM.BindEvent(this.gameObject, StartAttack , Define.Mouse.ClickL, Define.Sound.Ready );
    }

    // �̴Ͼ� ���ݽõ� �Լ� (��Ŭ�� ���콺 �̺�Ʈ)
    public void StartAttack(GameObject go)
    {
        // ���� ������ ���°� �ƴϰų�
        // �̹� �ٸ� ��ü�� Ÿ���� ���ε� �� ��ü�� Ŭ���� ���
        if (!attackable || GAME.Manager.IGM.TC.TargetCo != null)
        { return; }

        // ������ �ڽŰ�, �������� ���� ��Ȱ��ȭ
        GAME.Manager.IGM.Spawn.SpawnRay = Ray = false;

        // ���� Ÿ���� �ڷ�ƾ ��� �� ����
        GAME.Manager.IGM.TC.TargetCo = GAME.Manager.IGM.TC.TargettingCo
            (this, (IBody a, IBody t) => { return AttackCo(a, t); });

        // Ÿ���� ī�޶� ���� + ���� Ÿ���� ������ �����Լ� ���� ����
        GAME.Manager.IGM.TC.StartCoroutine(GAME.Manager.IGM.TC.TargetCo);
    }

    public IEnumerator AttackCo(IBody attacker, IBody target)
    {
        yield return null;
    }
}