using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buffFX : MonoBehaviour, IFx
{
    public Define.fxType FXtype { get { return Define.fxType.Buff; } }
    public Transform tr;
    public ParticleSystem ps,psArm, psAura; 
    public bool currUsing { get; set; }
    public Transform Tr { get { return this.transform; } }
    public IEnumerator Invoke(IBody attacker, IBody target)
    {
        return PlayBuffFX(target.TR);
    }
    public IEnumerator PlayBuffFX(Transform target)
    {
        currUsing = true;
        // 잠시 타겟의 자식으로 이 FX를 넣어서 타겟에 달라붙은 상태로 만들기로 결정
        this.transform.SetParent(target);
        Material mat1 = psArm.GetComponent<Renderer>().sharedMaterial;
        Material mat2 = psAura.GetComponent<Renderer>().sharedMaterial;
        transform.localPosition = Vector3.zero;

        Vector4 m1Start = new Vector4(1, 1, 1, 1);
        Vector4 m2Start = new Vector4(1, 0, 0, 1);
        Vector4 m1End = new Vector4(1, 1, 1, 0);
        Vector4 m2End = new Vector4(1, 0, 0, 0);
        mat1.SetColor("_BaseColor", m1Start);
        mat2.SetColor("_BaseColor", m2Start);
        ps.Play();
        float t = 0;
        while (t < 1.5f)
        {
            // 전체를 점점 투명화
            mat1.SetColor( "_BaseColor", Vector4.Lerp( m1Start, m1End , t ));
            mat2.SetColor( "_BaseColor", Vector4.Lerp( m2Start, m2End,  t ));
            t += Time.deltaTime;
            yield return null;
        }
        // 전체를 점점 투명화
        mat1.SetColor("_BaseColor", m1End );
        mat2.SetColor("_BaseColor", m2End );
        ps.Stop();
        // 원상복귀
        currUsing = false;
        transform.SetParent(GAME.IGM.Battle.transform);
        transform.localPosition = Vector3.zero;
    }
}
