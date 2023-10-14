using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attFX : MonoBehaviour, IFx
{
    public TrailRenderer tail1, tail2;
    public ParticleSystem particle;
    public Vector3 start, dest;
    public Define.fxType FXtype { get { return Define.fxType.Projectile; } }

    public bool currUsing { get;set; }
    public Transform Tr { get { return this.transform; }  }

    public IEnumerator Invoke(IBody attacker, IBody target)
    {
        return AttProjectile(attacker.TR.position, target.TR.position) ;
    }

  
    public IEnumerator AttProjectile(Vector3 start, Vector3 dest)
    {
        currUsing = true;
        // 시간이 지나면서 점차 투명해지도록
        tail1.colorGradient.alphaKeys = tail2.colorGradient.alphaKeys = new GradientAlphaKey[]
         { new GradientAlphaKey(1, 0f ), new GradientAlphaKey( 1 , 1f) };

        #region 투사체 준비 및 투사체 이동 코루틴
        // 공격자의 위치에서 시작하도록 위치 초기화
        transform.position = start;
        Vector3 dir = (dest - start).normalized; // 방향벡터
        float angle = Vector3.Angle(start, dir);
        Vector3 cross = Vector3.Cross(start, dir);
        if (cross.y < 0) { angle *= -1; }

        // 투사체 선형보간으로 타겟으로 향하며 이동
        float t = 0;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90f + Mathf.Lerp(0, angle, t)));
            transform.position = Vector3.Lerp(start, dest, t);
            // 시간이 지나면서 점차 투명해지도록
            tail1.colorGradient.alphaKeys = tail2.colorGradient.alphaKeys = new GradientAlphaKey[]
             { new GradientAlphaKey(1.5f -t, 0.0f), new GradientAlphaKey( 1.5f -t , 1f) };
            yield return null;
        }

        #endregion
        currUsing = false;
    }
  
}
