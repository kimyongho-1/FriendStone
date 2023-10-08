using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSkill : MonoBehaviour, IBody
{
    #region IBody
    [field : SerializeField]public bool Attackable { get; set; }
    public IEnumerator onDead { get; set; }
    public bool IsMine { get; set; }
    public int PunId { get; set; }

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

    public void InitSkill(bool isMine)
    {
        this.IsMine = IsMine;
        TR = transform;
        Col = GetComponent<CircleCollider2D>();
    }



}
