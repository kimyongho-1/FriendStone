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
        // ��� Ÿ���� �ڽ����� �� FX�� �־ Ÿ�ٿ� �޶���� ���·� ������ ����
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
            // ��ü�� ���� ����ȭ
            mat1.SetColor( "_BaseColor", Vector4.Lerp( m1Start, m1End , t ));
            mat2.SetColor( "_BaseColor", Vector4.Lerp( m2Start, m2End,  t ));
            t += Time.deltaTime;
            yield return null;
        }
        // ��ü�� ���� ����ȭ
        mat1.SetColor("_BaseColor", m1End );
        mat2.SetColor("_BaseColor", m2End );
        ps.Stop();
        // ���󺹱�
        currUsing = false;
        transform.SetParent(GAME.IGM.Battle.transform);
        transform.localPosition = Vector3.zero;
    }
}
