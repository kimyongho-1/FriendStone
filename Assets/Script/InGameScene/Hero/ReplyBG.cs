using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplyBG : MonoBehaviour
{
    public float textWaitTime =2f;
    private void OnEnable()
    {
        StartCoroutine(DisableWait());          
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    public IEnumerator DisableWait()
    {
        textWaitTime = 2f;
        while (textWaitTime > 0)
        {
            textWaitTime -= Time.deltaTime;
            yield return null;
        }
           
        // �ڽ��� �θ�� Speech������Ʈ
        // �ڽ��� �θ� ��ü�� �����Ͽ� �ڽŵ� �����
        // �ٽ� ��ȭâ �̺�Ʈ ������ ��ȯ�ϵ��� ����
        transform.parent.gameObject.SetActive(false);
    }
}
