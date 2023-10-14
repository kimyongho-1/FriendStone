using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healFX : MonoBehaviour, IFx
{
    public ParticleSystem ps;
    public Define.fxType FXtype { get { return Define.fxType.Heal; } }
    public Transform tr;
    public bool currUsing { get; set; }
    public Transform Tr { get { return this.transform; } }
    public IEnumerator Invoke(IBody attacker, IBody target)
    {
        return PlayHealFX(target.TR);
    }
    public IEnumerator PlayHealFX(Transform target)
    {
        currUsing = true; 
        Material mat1 = ps.GetComponent<Renderer>().sharedMaterial;
        Vector4 m1Start = new Vector4(1, 1, 1, 1);
        Vector4 m1End = new Vector4(1, 1, 1, 0);
        // 잠시 타겟의 자식으로 이 FX를 넣어서 타겟에 달라붙은 상태로 만들기로 결정
        this.transform.SetParent(target);
        transform.localPosition = Vector3.zero;
        mat1.SetColor("_BaseColor", m1Start);
        ps.Play();
        float t = 0;
        while (t < 1f)
        {
            // 전체를 점점 투명화
            t += Time.deltaTime;
            mat1.SetColor("_BaseColor", Vector4.Lerp(m1Start, m1End, t));
            yield return null;
        }
        mat1.SetColor("_BaseColor", m1End);
        ps.Stop();
        // 원상복귀
        currUsing = false;
        transform.SetParent(GAME.IGM.Battle.transform);
        transform.localPosition = Vector3.zero;
    }
}
