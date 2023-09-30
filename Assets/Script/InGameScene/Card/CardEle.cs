using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEle : MonoBehaviour, IBody
{
    public CardData data;
    [field: SerializeField] public int PunId { get; set; }
    [field: SerializeField] public bool IsMine { get; set; }
    public Transform TR { get { return this.transform; } }
  
    [field: SerializeField] public Collider2D Col { get; set; }
    public bool Ray { set { if (Col == null) { Col = TR.GetComponent<Collider2D>(); } Col.enabled = value; } }

    public Vector3 OriginPos { get; set; }
    [field: SerializeField] public int OriginAtt { get; set; }
    [field: SerializeField] public int OriginHp { get; set; }
    [field: SerializeField] public virtual int Att { get; set; }
    [field: SerializeField] public virtual int HP { get; set; }
    public IEnumerator onDead { get; set; }


    public Define.ObjType objType { get; set; }
}
