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
    public Vector3 Pos { get { return TR.localPosition; } } // 현재 포지션값
    public Vector3 OriginPos { get; set; } // 고유위치
    #endregion


    #region 충돌체와 활성여부
    public Collider2D Col { get; set; }
    public bool Ray { set; }
    #endregion


    public IEnumerator StartReadyCoAnimation()
    {
        float t = 0;
        Vector3 dest = OriginPos + new Vector3(0, -0.25f, 0.4f);
        while (t < 1f)
        {
            t += Time.deltaTime * 2.5f;
            TR.transform.localPosition = Vector3.Lerp(OriginPos, dest, t);
            yield return null;
        }
    }
    public IEnumerator ExitReadyCoAnimation()
    {
        float t = 0;
        Vector3 start = Pos;
        while (t < 1f)
        {
            t += Time.deltaTime * 2.5f;
            TR.transform.localPosition = Vector3.Lerp(start, OriginPos, t);
            yield return null;
        }
    }
}
