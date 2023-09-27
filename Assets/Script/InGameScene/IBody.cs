using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBody // 공격을 받을수 있는 모든 객체에 부착
{
    public bool IsMine { get; set; }
    public int PunId { get; set; }
    public Define.BodyType bodyType { get; }
    
    #region 공통 컴포넌트
    public Transform TR { get; } // 트랜스폼
    public Vector3 Pos { get { return TR.position; } } // 현재 포지션값
    public Vector3 OriginPos { get; set; } // 고유위치

    
    public int OriginHp { get; set; }
    public int Att { get; set;  }
    public int HP { get; set; }
    #endregion


    #region 충돌체와 활성여부
    public Collider2D Col { get; set; }
    public bool Ray { set; }
    #endregion

    public IEnumerator onDead { get; set; }
}
