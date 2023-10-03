using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static Define;

public class TargetingCamera : MonoBehaviour
{
    public LineRenderer LR;
    public SpriteRenderer Arrow;
    public Image transitionPanel;
    private void Awake()
    {
        // 타겟팅 카메라 참조
        GAME.IGM.TC = this;
        LR.positionCount = 0;
    }
    public float dist = 7;
    public float ShakeDuration = 2;  // ShakeDuration은 총 회전할 시간
    public float maxAngle = 1f; // 최대 각도량 (z축의 최대 회전치)
    // 재생 속도
    public float playTime = 2f; // ShakeDuration동안 몇번을 반복할지

    // 공격등이 실행될떄, 카메라 한들기 코루틴
    public IEnumerator ShakeCo()
    {
        // 0 ~ 2PI가  사인함수에서 한주기
        float t = 0;
        float frequency = 2 * Mathf.PI / ShakeDuration; // ShakeDur동안 한주기를 도는 수치
        frequency *= playTime; // 추가로 몇번을 흔들릴지 playTime만큼 곱하기
        Transform tr = Camera.main.transform;
        while (t < ShakeDuration)
        {
            t += Time.deltaTime;
            //Debug.Log($" Mahtf.Sin( {t} * {frequency} : {t * frequency}) : {Mathf.Sin(t * frequency)}");
            float currAngle = Mathf.Sin(t * frequency) * maxAngle;
            tr.rotation = Quaternion.Euler(0, 0, currAngle);
            yield return null;
        }
        tr.rotation = Quaternion.identity; // 회전을 초기 상태로 리셋
    }

    // 유저의 마우스 드래그 공격 궤적 그리기
    public IEnumerator DrawLine(Vector3 startPos)
    {
        // 라인렌더러 실행
        LR.gameObject.SetActive(true);
        LR.positionCount = 6;
        while (LR.positionCount > 0)
        {
            // 현재 유저의 커서포인트 월드포지션
            Vector3 CursorPos = Camera.main.ScreenToWorldPoint
               (new Vector3(Input.mousePosition.x, Input.mousePosition.y, dist));

            // 방향벡터 와 길이 구하기
            Vector3 dir = CursorPos - startPos;
            float distance = Vector3.Distance(startPos, CursorPos);
            // 방향 벡터 단위 구해서 이후에 사용
            Vector3 oneGrid = dir.normalized;

            // (길이 / 2)만큼 LR의 머테리얼속 텍스처 타일링 쪼개기
            LR.material.mainTextureScale = new Vector2(distance * .5f, 1);

            // 커서위치에서 약간 뒤로 떨어진만큼을 거리로 조정 (화살표를 최상단으로 위치시키고 싶어서)
            Vector3 dest = CursorPos - (oneGrid * 0.5f);
            // LR포지션 위치조정
            for (int i = 0; i < LR.positionCount; i++)
            {
                Vector3 pos = Vector3.Lerp(startPos, dest, (float)i / (LR.positionCount));
                LR.SetPosition(i, pos);
                yield return null;
            }

            // 각도량 구하기
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            // 회전과 위치 시키기
            Arrow.transform.localRotation = Quaternion.Euler(0, 0, angle - 90f);
            Arrow.transform.localPosition = CursorPos - oneGrid * .5f;
        }

        LR.gameObject.SetActive(false);
    }

    // 유저의 마우스 타겟팅 지정 
    public IEnumerator TargettingCo(IBody attacker, Func<IBody, IBody, IEnumerator> RegisterCo, string[] filter)
    {
        // 약간의 딜레이를 주어서 바로 다음 구문 실행 방지
        yield return new WaitForSeconds(0.15f);

        // 공격궤적을 그리는 라인렌더러 실행, 시작 위치를 인자로
        // 미니언카드는 자신의 위치, 그외 주문카드나 영웅은 자신의 영웅 고유위치에서 시작
        Vector3 startPos = (attacker.objType == ObjType.Minion) ? attacker.Pos : new Vector3(GAME.IGM.Hero.Player.OriginPos.x, -0.9f, GAME.IGM.Hero.Player.OriginPos.z);
        StartCoroutine(DrawLine(startPos));

        // 공격자가 미니언이나 영웅이면 공격자세 취하기
        if (attacker.objType != Define.ObjType.HandCard)
        {
            float t = 0;
            Vector3 dest = attacker.OriginPos + new Vector3(0, -0.25f, -0.5f);
            while (t < 1f)
            {
                t += Time.deltaTime * 2.5f;
                attacker.TR.position = Vector3.Lerp(attacker.OriginPos, dest, t);
                yield return null;
            }
        }
        // 타겟팅 실행자가 핸드카드라면 
        if (attacker.objType == Define.ObjType.HandCard)
        {
            float t = 0;
            Vector3 dest = attacker.OriginPos + new Vector3(0, 0.5f, -0.5f);
            while (t < 1f)
            {
                t += Time.deltaTime * 2.5f;
                attacker.TR.position = Vector3.Lerp(attacker.OriginPos, dest, t);
                yield return null;
            }
        }

        // 필터확인
        if (filter == null) { Debug.Log("Filter is NUll"); }

        Vector3 camPos = Camera.main.transform.position;
        Func<bool> waitInput = (attacker.objType != Define.ObjType.HandCard) ?
            () => { return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1); }
        :
            () => { return Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1); };

        while (true)
        {
            // 현재 유저의 커서포인트 월드포지션
            Vector3 CursorPos = Camera.main.ScreenToWorldPoint
               (new Vector3(Input.mousePosition.x, Input.mousePosition.y, -camPos.z));

            // 마우스 클릭과 충돌체 확인시 성공
            if (waitInput.Invoke()) //Input.GetMouseButtonDown(0)
            {
                Collider2D hit = Physics2D.OverlapPoint(CursorPos, LayerMask.GetMask(filter)); ;
                for (int i = 0; i < filter.Length; i++)
                {
                    Debug.Log(filter[i]);
                }
                LR.positionCount = 0;
                if (hit != null)
                {
                    // 실행 코루틴 예약 실행
                    GAME.IGM.AddAction(RegisterCo(attacker, hit.transform.GetComponent<IBody>()));
                    // 현재 어택커가 어떤 녀석인지 : 미니언 일반공격, 영웅웨폰공격, 영웅스킬사용 등등
                    
                    Debug.Log("충돌체 이름 : " + hit.name);
                }
                else
                {
                    Debug.Log("충돌체 없으면 끝");
                    // 공격자가 미니언이었으면 공격자세 해제하기
                    if (attacker.objType != Define.ObjType.HandCard)
                    {
                        float t = 0;
                        Vector3 start = attacker.Pos;
                        Vector3 dest = attacker.OriginPos ;
                        while (t < 1f)
                        {
                            t += Time.deltaTime * 2.5f;
                            attacker.TR.position = Vector3.Lerp(start , dest, t);
                            yield return null;
                        }
                    }  
                    // 타겟팅 실행자가 핸드카드라면 
                    if (attacker.objType == Define.ObjType.HandCard)
                    {
                        float t = 0;
                        Vector3 start = attacker.Pos;
                        Vector3 dest = attacker.OriginPos;
                        while (t < 1f)
                        {
                            t += Time.deltaTime * 2.5f;
                            attacker.TR.position = Vector3.Lerp(start, dest, t);
                            yield return null;
                        }
                    }
                }
                
                break;
            }

            yield return null;
        }
        // 라인궤적 그리는 코루틴 종료위해 갯수 초기화
        LR.positionCount = 0;
        // 레이 모두 다시 활성화
        GAME.IGM.Spawn.SpawnRay = attacker.Ray = true;
        yield break;
    }

    // 두 유저 인게임씬 진입 + 서로 기본정보 공유 확인시
    // 마스터 클라이언트가 게임내 트랜지션 캔버스의 알파값을 줄이면서 게임 시작 연출 코루틴
    public IEnumerator StartIntro()
    {
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * 0.5f;
            transitionPanel.color = new Color(0,0,0,1-t);
            yield return null;
        }
    }
}
