using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEle : MonoBehaviour, IBody
{
    public CardData data;
    [field: SerializeField] public int PunId { get; set; }
    [field: SerializeField] public bool IsMine { get; set; }
    public Transform TR { get { return this.transform; } }
    public Define.BodyType bodyType 
    { 
        get 
        {
            switch (data.cardType)
            {
                case Define.cardType.minion:
                    return Define.BodyType.Meele;
                default : return Define.BodyType.None;
            }
            
        } 
    }
    [field: SerializeField] public Collider2D Col { get; set; }
    public bool Ray { set { Col.enabled = value; } }

    public Vector3 OriginPos { get; set; }
    [field: SerializeField] public int OriginAtt { get; set; }
    [field: SerializeField] public int OriginHp { get; set; }
    [field: SerializeField] public int Att { get; set; }
    [field: SerializeField] public int HP { get; set; }
}
