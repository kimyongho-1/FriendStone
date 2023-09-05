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
        // 매칭 잡힐시 정해줄 상대방의 닉네임
        text = gameObject.GetComponent<TextMeshProUGUI>();  
    }

    // 비활성화시마다 초기화 (재회전시마다 처음보던 모습그대로 재생위해서)
    private void OnDisable()
    {
        rotate = startRotate;
    }


}
