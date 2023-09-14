using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSkill : MonoBehaviour, IBody
{
    Hero owenrUser;
    Action<GameObject> func;
    # region
    public IEnumerator Do { get; set; }
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
        Col = Col.GetComponent<CircleCollider2D>();   

        owenrUser = user;   
        switch (GAME.Manager.RM.GameDeck.ownerClass)
        { 
            
        }

        // ��ų ������ Ŭ����, Ÿ���� �� ��ų �̺�Ʈ ���� ����
        GAME.Manager.UM.BindEvent(this.gameObject, ClickedOnSkill, Define.Mouse.ClickL, Define.Sound.None);
    }

    public void ClickedOnSkill(GameObject go)
    {
        // ���� ��Ȱ��ȭ : �̹� ��� Ȯ��
        // �ڷ�ƾ ����� : ���� �ٸ� �巡��Ŭ���� ��ų������Ʈ�� Ŭ���� ���
        if (Col.enabled == false || GAME.Manager.IGM.TC.TargetCo != null)
        { return; }

        // ������ �ڽŰ�, �������� ���� ��Ȱ��ȭ
        GAME.Manager.IGM.Spawn.SpawnRay = Ray = false;

        // Ÿ���� ī�޶� ����
        GAME.Manager.IGM.TC.StartTargetting(this);
    }


    public IEnumerator HJClick()
    {
        yield return null;
    }     
    public IEnumerator HZClick()
    { yield return null; }   
    public IEnumerator KHClick()
    { yield return null; }
}
