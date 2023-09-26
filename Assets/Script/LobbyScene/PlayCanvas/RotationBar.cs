using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RotationBar : MonoBehaviour
{
    public List<RotateOBJ> list = new List<RotateOBJ>();
    public float angle = 0;
    public float radius = 5.0f;
    public float speed = 30.0f;
    public float zThick = 0.5f;
    public Vector3 scale;
    public SelectedDeckIcon sdi;
    // 쉬운 참조를 위해서 전역으로 설정
    public static bool stop = false;

    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            list.Add(transform.GetChild(i).GetComponent<RotateOBJ>());
            // 초기 값 설정
            list[i].startRotate = list[i].rotate = 270f - (angle * i);
        }
        
    }
    
    void OnEnable()
    {
        StartCoroutine(rotate());
    }
    void OnDisable()
    {
        StopAllCoroutines();
        stop = false;
        speed = 125f;
    }
    IEnumerator rotate()
    {
        // 펀매니저에서 매칭 잡히면 로테이션의 속도를 낮추는 코루틴
        StartCoroutine(MatchingTimer());
        IEnumerator MatchingTimer()
        {
            float t = 0;
            float m = speed;
            // 펀매니저에서 매칭 잡힐시 stop 변수가 true로 변경
            yield return new WaitUntil(() => (stop == true));
            while (t < 1f)
            {
                t += Time.deltaTime;
                speed = Mathf.Lerp(m, 0, t);
                yield return null;
            }
        }

        // 스피드가 감소하는건, 펀매니저에서 매칭 잡힐시 
        // 위의 매칭타이머 코루틴에서 속도감소 시작
        while (speed > 0)
        {
            // 매칭이 잡힐떄까지 모든 Bar를 구체모형으로 회전시키기
            for (int i = 0; i < list.Count; i++)
            {
                // 현재 270회전값이 최상위 도달을 의미해서 다시 초기화
                if (list[i].rotate > 270f)
                { list[i].rotate = list[i].rotate % 270f + 90f; } // 90F값이 밑에서 시작

                // 회전 값 갱신
                list[i].rotate += Time.deltaTime * speed;
                float z = Mathf.Cos( list[i].rotate * Mathf.Deg2Rad) * radius * zThick;
                float y = -Mathf.Sin(list[i].rotate * Mathf.Deg2Rad) * radius ;
                
                // 뒤로 갈수록 스케일을 0으로 줄여 안보이게 설정
                list[i].tr.localScale =scale * Mathf.Abs(Mathf.Cos(list[i].rotate * Mathf.Deg2Rad));
                list[i].tr.localPosition = new Vector3(0, y, z);// + 400f
            }
            yield return null;
        }

        Debug.Log("RotationBar 정지 시작");
        // 매칭이 잡혔을시 모든 Bar들을 초기 위치로 되돌리기 (list[i]가 startRotate값으로 복귀하기까지 회전)
        for (int i = 0; i < list.Count; i++)
        {
            StartCoroutine(indieRotate(list[i]));
            IEnumerator indieRotate(RotateOBJ r)
            {
                // 소수점등 불일치시 rotate와 startRotate가 일치하지않기 떄문에 강제로 맞추기
                r.rotate = (float)Mathf.CeilToInt(r.rotate);

                // 회전을 하다가 원래의 회전값으로 돌아올시 끝내기 (초기 위치로 온것이기 떄문에)
                while (r.rotate == r.startRotate)
                {
                    // 현재 270회전값이 최상위 도달을 의미해서 다시 초기화
                    if (r.rotate > 270f)
                    { r.rotate = r.rotate % 270f + 90f; } // 90F값이 밑에서 시작
                                                          
                    r.rotate += 1f;// 회전 값 갱신
                    float z = Mathf.Cos(r.rotate * Mathf.Deg2Rad) * radius * zThick;
                    float y = -Mathf.Sin(r.rotate * Mathf.Deg2Rad) * radius;
                    // 뒤로 갈수록 스케일을 0으로 줄여 안보이게 설정
                    r.tr.localScale = scale * Mathf.Abs(Mathf.Cos(r.rotate * Mathf.Deg2Rad));
                    r.tr.localPosition = new Vector3(0, y, z);// + 400f
                    yield return null;
                }
                
            }
            
        }

        yield return new WaitForSeconds(1f);
        // 화면 전환 코루틴 시작 (점차 화면이 검어지는 코루틴)
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            sdi.transitionPanel.color
                = new Color(0,0,0,t);
            yield return null;
        }

        // 위를 모두 통과하여 여기까지 온경우
        // 매칭이 잡힌 상황
        // 연출 시작

        // 씬 전환
        //SceneManager.LoadScene("InGame", LoadSceneMode.Single);
        
        if (PhotonNetwork.IsMasterClient)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log("마스터가 전환 시작");
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.LoadLevel("InGame");
        }
    }
}
