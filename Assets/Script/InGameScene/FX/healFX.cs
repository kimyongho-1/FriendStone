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
        // ��� Ÿ���� �ڽ����� �� FX�� �־ Ÿ�ٿ� �޶���� ���·� ������ ����
        this.transform.SetParent(target);
        transform.localPosition = Vector3.zero;
        mat1.SetColor("_BaseColor", m1Start);
        ps.Play();
        float t = 0;
        while (t < 1f)
        {
            // ��ü�� ���� ����ȭ
            t += Time.deltaTime;
            mat1.SetColor("_BaseColor", Vector4.Lerp(m1Start, m1End, t));
            yield return null;
        }
        mat1.SetColor("_BaseColor", m1End);
        ps.Stop();
        // ���󺹱�
        currUsing = false;
        transform.SetParent(GAME.IGM.Battle.transform);
        transform.localPosition = Vector3.zero;
    }
}
