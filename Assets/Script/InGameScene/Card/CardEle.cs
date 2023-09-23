using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEle : MonoBehaviour, IBody
{
    public CardData data;
    public int PunId { get; set; }
    public bool IsMine { get; set; }
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
    public Collider2D Col { get; set; }
    public bool Ray { set { Col.enabled = value; } }

    public Vector3 OriginPos { get; set; }
    public int OriginAtt { get; set; }
    public int OriginHp { get; set; }
    public int Att { get; set; }
    public int HP { get; set; }
}
