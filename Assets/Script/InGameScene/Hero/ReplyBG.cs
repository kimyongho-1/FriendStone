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
        // 자신의 부모는 Speech오브젝트
        // 자신의 부모 자체를 종료하여 자신도 종료및
        // 다시 대화창 이벤트 실행을 순환하도록 유도
        transform.parent.gameObject.SetActive(false);
    }
}
