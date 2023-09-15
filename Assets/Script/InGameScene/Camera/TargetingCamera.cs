using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class TargetingCamera : MonoBehaviour
{
    public LineRenderer LR;
    public SpriteRenderer Arrow;
    private void Awake()
    {
        // 타겟팅 카메라 참조
        GAME.Manager.IGM.TC = this;
        LR.positionCount = 0;
    }
    public float dist = 7;
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

    public IEnumerator TargetCo = null;
    public void StartTargetting(IBody attacker, Func<IBody, IBody, IEnumerator> RegisterCo)
    {
        Debug.Log(attacker.TR.name+"의 타겟팅 시도");

        // 현재 타겟팅 코루틴 등록 및 실행
        TargetCo = TargettingCo(attacker, RegisterCo);
        StartCoroutine(TargetCo);
    }
    
    public IEnumerator TargettingCo(IBody attacker, Func<IBody, IBody, IEnumerator> RegisterCo)
    {
        // 약간의 딜레이를 주어서 바로 다음 구문 실행 방지
        yield return new WaitForSeconds(0.15f);
        // 공격궤적을 그리는 라인렌더러 실행
        StartCoroutine(DrawLine(attacker.Pos));

        // 미니언의 경우 선택되었단 표시를 코루틴 애니메이션 실행
        if (attacker.bodyType == BodyType.Minion)
        { StartCoroutine(attacker.StartReadyCoAnimation()); }

        Vector3 camPos = Camera.main.transform.position;
        while (true)
        {
            // 현재 유저의 커서포인트 월드포지션
            Vector3 CursorPos = Camera.main.ScreenToWorldPoint
               (new Vector3(Input.mousePosition.x, Input.mousePosition.y, -camPos.z));

            // 현재 마우스 위치에 충돌체있는지 검사
            RaycastHit2D hit = Physics2D.Raycast(CursorPos,Vector2.zero);

            // 마우스 클릭과 충돌체 확인시 성공
            if (Input.GetMouseButtonDown(0))
            {
                LR.positionCount = 0;
                if (hit.collider != null)
                {
                    if (attacker.bodyType == BodyType.Minion)
                    { StartCoroutine(attacker.ExitReadyCoAnimation()); }
                       
                    // 현재 어택커가 어떤 녀석인지 : 미니언 일반공격, 영웅웨폰공격, 영웅스킬사용 등등
                    
                    Debug.Log("충돌체 이름 : " + hit.collider.name);
                }
                else
                {
                    if (attacker.bodyType == BodyType.Minion)
                    { StartCoroutine(attacker.ExitReadyCoAnimation()); }
                    Debug.Log("충돌체 없으면 끝");
                }
                
                break;
            }

            yield return null;
        }
        // 레이 모두 다시 활성화
        GAME.Manager.IGM.Spawn.SpawnRay = attacker.Ray = true;
        TargetCo = null;
        yield break;
    }
}
