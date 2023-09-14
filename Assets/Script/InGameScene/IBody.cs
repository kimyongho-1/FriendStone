using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBody // ������ ������ �ִ� ��� ��ü�� ����
{
    public bool IsMine { get; set; }
    public int PunId { get; set; }
    public Define.BodyType bodyType { get; }
    
    #region ���� ������Ʈ
    public Transform TR { get; } // Ʈ������
    public Vector3 Pos { get { return TR.localPosition; } } // ���� �����ǰ�
    public Vector3 OriginPos { get; set; } // ������ġ
    #endregion


    #region �浹ü�� Ȱ������
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
