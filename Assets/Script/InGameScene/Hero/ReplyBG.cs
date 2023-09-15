using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplyBG : MonoBehaviour
{
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
        yield return new WaitForSeconds(2f);
        // �ڽ��� �θ�� Speech������Ʈ
        // �ڽ��� �θ� ��ü�� �����Ͽ� �ڽŵ� �����
        // �ٽ� ��ȭâ �̺�Ʈ ������ ��ȯ�ϵ��� ����
        transform.parent.gameObject.SetActive(false);
    }
}
