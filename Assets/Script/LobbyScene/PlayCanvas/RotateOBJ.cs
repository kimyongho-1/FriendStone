using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
public class RotateOBJ : MonoBehaviour
{
    public Transform tr;
    TextMeshProUGUI text;
    public float rotate, startRotate;
    private void Awake()
    {
        tr = GetComponent<Transform>();
        // ��Ī ������ ������ ������ �г���
        text = gameObject.GetComponent<TextMeshProUGUI>();  
    }

    // ��Ȱ��ȭ�ø��� �ʱ�ȭ (��ȸ���ø��� ó������ ����״�� ������ؼ�)
    private void OnDisable()
    {
        rotate = startRotate;
    }


}
