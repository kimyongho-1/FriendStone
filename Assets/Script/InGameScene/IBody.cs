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
    public Vector3 Pos { get { return TR.position; } } // ���� �����ǰ�
    public Vector3 OriginPos { get; set; } // ������ġ

    
    public int OriginHp { get; set; }
    public int Att { get; set;  }
    public int HP { get; set; }
    #endregion


    #region �浹ü�� Ȱ������
    public Collider2D Col { get; set; }
    public bool Ray { set; }
    #endregion

    public IEnumerator onDead { get; set; }
}
