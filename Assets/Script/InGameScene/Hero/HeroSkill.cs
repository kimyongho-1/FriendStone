using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSkill : MonoBehaviour, IBody
{
    bool attackable = true;
    #region IBody
    public bool Attackable 
    {
        get
        { return attackable; }
        set
        {
            attackable = value;
            if (IsMine) { GAME.IGM.Hero.Player.skillImg.color = (attackable == true)? Color.white : Color.black; }
            else { GAME.IGM.Hero.Enemy.skillImg.color = (attackable == true) ? Color.white : Color.black; }
        }
    }
    public IEnumerator onDead { get; set; }
    [field: SerializeField] public bool IsMine { get; set; }
    [field: SerializeField] public int PunId { get; set; }

    [field: SerializeField] public Define.ObjType objType { get; set; }
    public Transform TR { get; set; }

    [field: SerializeField] public Vector3 OriginPos { get; set; }
    public Collider2D Col { get; set; }
    public bool Ray { set { Col.enabled = value; } }
    [field: SerializeField] public int OriginAtt { get; set; }
    [field: SerializeField] public int OriginHp { get; set; }
    [field: SerializeField] public int Att { get; set; }
    [field: SerializeField] public int HP { get; set; }

    #endregion

    public void InitSkill(bool isMine)
    {
        IsMine = isMine;
        TR = transform;
        Col = GetComponent<CircleCollider2D>();
        objType = Define.ObjType.Minion; // 자신의 위치에서 타겟팅을 시키기위해 미니언으로 설정
        OriginPos = this.transform.position;
        GAME.IGM.allIBody.Add(this);
        gameObject.layer = LayerMask.NameToLayer("Default");
        
    }



}
