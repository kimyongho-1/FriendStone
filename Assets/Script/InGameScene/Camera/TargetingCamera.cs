using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static Define;

public class TargetingCamera : MonoBehaviour
{
    public LineRenderer LR;
    public SpriteRenderer Arrow;
    public Image transitionPanel;
    public AudioSource targetingSound;
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
        targetingSound.Play();
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
        targetingSound.Stop();
        LR.gameObject.SetActive(false);
    }

    // 미니언과 영웅의 근접공격에 사용
    public IEnumerator MeeleTargettingCo(IBody attacker, Func<IBody, IBody, IEnumerator> RegisterCo )
    {
        GAME.IGM.Turn.Col.enabled = false;
        // 약간의 딜레이를 주어서 바로 다음 구문 실행 방지
        yield return new WaitForSeconds(0.15f);

        // 공격궤적을 그리는 라인렌더러 실행, 시작 위치를 인자로
        // 미니언카드는 자신의 위치, 그외 주문카드나 영웅은 자신의 영웅 고유위치에서 시작
        Vector3 startPos = attacker.Pos;
        StartCoroutine(DrawLine(startPos));

        // 공격자가 미니언이나 영웅이면 공격자세 취하기
        float t = 0;
        Vector3 dest = attacker.OriginPos + new Vector3(0, -0.25f, -0.5f);
        while (t < 1f)
        {
            t += Time.deltaTime * 2.5f;
            attacker.TR.position = Vector3.Lerp(attacker.OriginPos, dest, t);
            yield return null;
        }
        string[] filter = new string[] {"foe", "foeHero" };
        Vector3 camPos = Camera.main.transform.position;
        Func<bool> waitInput =  () => { return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1); } ;

        while (true)
        {
            // 현재 유저의 커서포인트 월드포지션
            Vector3 CursorPos = Camera.main.ScreenToWorldPoint
               (new Vector3(Input.mousePosition.x, Input.mousePosition.y, -camPos.z));

            // 마우스 클릭과 충돌체 확인시 성공
            if (waitInput.Invoke())
            {
                Collider2D hit = Physics2D.OverlapPoint(CursorPos, LayerMask.GetMask(filter)); 
                if (hit != null)
                {
                    // 근접 공격 타겟팅은 미니언과 영웅들만이 타겟범위
                    IBody targetBody  = hit.transform.GetComponent<IBody>();

                    #region 상대 도발 하수인 존재 + 선택한 타겟이 도발하수인이 아닌 경우 : 공격취소로 간주                   
                    // 먼저 도발 하수인들이 상대에게 있는지 확인
                    List<CardField> FindTauntList = GAME.IGM.Spawn.enemyMinions.FindAll(x => x.MC.isTaunt == true);
                    Debug.Log("적 도발수 : " + FindTauntList.Count);
                    Debug.Log("타겟은 도발 ? : "+ FindTauntList.Contains(targetBody));
                    // 상대에게 도발하수인이 존재하며, 현재 공격하기로한 타겟이 도발하수인이 아니라면 공격 불가능 (언제나 도발 하수인 먼저 공격)
                    if (FindTauntList.Count > 0 &&
                        FindTauntList.Contains(targetBody) == false)
                    {
                        // 라인렌더러 그리는 궤적코루틴 중지위해 라인렌더러 갯수 줄이기
                        LR.positionCount = 0;
                        // 상대에게 도발하수인이 있는데, 도발하수인이 아닌 다른 타겟을 공격할수없기에
                        // 공격 강제 취소
                        Debug.Log("충돌체는 있으나, 도발하수인이 존재해서 직접공격 불가");
                        t = 0;
                        Vector3 start = attacker.Pos;
                        dest = attacker.OriginPos;
                        while (t < 1f)
                        {
                            t += Time.deltaTime * 2.5f;
                            attacker.TR.position = Vector3.Lerp(start, dest, t);
                            yield return null;
                        }
                        // 플레이어에게 현재 도발하수인떄문에 공격할수 없다 알리기 ( 이벤트 전파 X )
                        GAME.IGM.Hero.Player.HeroSaying(Define.Emotion.ThereTaunt);

                        // 결과적으로 공격을 안하였기에 attackable을 다시 복구
                        attacker.Attackable = true;
                    }
                    #endregion

                    #region 상대 도발하수인 없음 + 선택한 타겟 존재 : 평범한 공격코루틴 Queue에 이벤트 예약실행
                    // 타겟을 찾았고, 상대에게 도발하수인도 없다면
                    else
                    {
                        // 라인렌더러 그리는 궤적코루틴 중지위해 라인렌더러 갯수 줄이기
                        LR.positionCount = 0;
                        // 실행 코루틴 예약 실행
                        GAME.IGM.AddAction(RegisterCo(attacker, targetBody));
                        // 현재 어택커가 어떤 녀석인지 : 미니언 일반공격, 영웅웨폰공격, 영웅스킬사용 등등

                        Debug.Log("충돌체 이름 : " + hit.name);
                    }
                    #endregion

                    break; // 반복문 빠져나가기
                }
                
                // 잘못된 타겟 클릭시 
                else
                {
                    // 라인궤적 그리는 코루틴 종료위해 갯수 초기화
                    LR.positionCount = 0;
                    // 잘못된 곳에 클릭시 공격 취소 및 제자리 복귀 애니메이션
                    Debug.Log("충돌체 없으면 끝");
                    t = 0;
                    Vector3 start = attacker.Pos;
                    dest = attacker.OriginPos;
                    while (t < 1f)
                    {
                        t += Time.deltaTime * 2.5f;
                        attacker.TR.position = Vector3.Lerp(start, dest, t);
                        yield return null;
                    }
                    // 공격을 안하였기에 attackable을 다시 복구
                    attacker.Attackable = true;
                    break;
                }
            }

            yield return null;
        }
        GAME.IGM.Turn.Col.enabled = attacker.Col.enabled = true;
        yield break;
    }

    // 주문카드, 미니언의 이벤트 타겟팅 등에 사용
    public IEnumerator TargettingCo(IBody attacker, Func<IBody, IBody, IEnumerator> RegisterCo, string[] filter )
    {
        // 핸드 못건드리게 잠그기
        GAME.IGM.Hand.PlayerHand.ForEach(x=>x.Col.enabled = false);
        // 이벤트의 공격 범위에 맞게 , 포스트프로세싱과 제외대상을 나누어 시각적으로 강조하기
        GAME.IGM.Post.StartMaskingArea(filter);
        Debug.Log("공격자 충돌체 여부" + attacker.Col);
        attacker.Col.enabled = false;
        GAME.IGM.Turn.Col.enabled = false;

        // 약간의 딜레이를 주어서 바로 다음 구문 실행 방지
        yield return new WaitForSeconds(0.15f);

        // 공격궤적을 그리는 라인렌더러 시작 위치를 찾기
        // 미니언카드는 자신의 고유 위치, 그외 주문카드나 영웅은 자신의 영웅 고유위치에서 시작
        Vector3 startPos = (attacker.objType == ObjType.Minion)
            ? attacker.OriginPos : 
            new Vector3(GAME.IGM.Hero.Player.OriginPos.x, -0.9f, GAME.IGM.Hero.Player.OriginPos.z);

        // 공격 모션 그리기
        StartCoroutine(DrawLine(startPos));

        // 필터확인
        if (filter == null) { Debug.Log("Filter is NUll"); }

        // 카메라 위치와, 입력확인 초기화
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
                if (hit != null)
                {
                    // 라인궤적 그리는 코루틴 종료위해 갯수 초기화
                    LR.positionCount = 0;
                    // 실행 코루틴 예약 실행
                    GAME.IGM.AddAction(RegisterCo(attacker, hit.transform.GetComponent<IBody>()));
                    Debug.Log("충돌체 이름 : " + hit.name);
                    break;
                }
                else
                {
                    Debug.Log("타겟 없음");
                    // 손에서 사용하는, 주문카드의 경우 잘못된 곳을 클릭시 취소하도록 변경
                    if (attacker.objType == ObjType.HandCard)
                    {
                        // 라인궤적 그리는 코루틴 종료위해 갯수 초기화
                        LR.positionCount = 0; break; 
                    }
                }
            }

            yield return null;
        }

        GAME.IGM.Post.ExitMaskingArea();
        // 레이 모두 다시 활성화
        attacker.Ray = true;
        GAME.IGM.Turn.Col.enabled = attacker.Col.enabled = true;
        GAME.IGM.Hand.PlayerHand.ForEach(x => x.Col.enabled = true);
        yield break;
    }
    public IEnumerator TargettingHeroSkillCo(IBody attacker)
    {
        Hero player = GAME.IGM.Hero.Player;
        // 핸드 못건드리게 잠그기
        GAME.IGM.Hand.PlayerHand.ForEach(x => x.Col.enabled = false);
        attacker.Col.enabled = false;
        GAME.IGM.Turn.Col.enabled = false;
        // 약간의 딜레이를 주어서 바로 다음 구문 실행 방지
        yield return new WaitForSeconds(0.15f);

        // 공격 모션 그리기
        StartCoroutine(DrawLine(attacker.OriginPos));

        
        // 카메라 위치와, 입력확인 초기화
        Vector3 camPos = Camera.main.transform.position;
        Func<bool> waitInput = (attacker.objType != Define.ObjType.HandCard) ?
            () => { return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1); }
        :
            () => { return Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1); };
        string[] filter = new string[] { "ally", "allyHero", "foe", "foeHero", };
        while (true)
        {
            // 현재 유저의 커서포인트 월드포지션
            Vector3 CursorPos = Camera.main.ScreenToWorldPoint
               (new Vector3(Input.mousePosition.x, Input.mousePosition.y, -camPos.z));

            // 마우스 클릭과 충돌체 확인시 성공
            if (waitInput.Invoke()) //Input.GetMouseButtonDown(0)
            {
                Collider2D hit = Physics2D.OverlapPoint(CursorPos, LayerMask.GetMask(filter)); ;
                if (hit != null && hit.TryGetComponent<IBody>(out IBody targetFounded))
                {
                    // 라인궤적 그리는 코루틴 종료위해 갯수 초기화
                    LR.positionCount = 0;
                    attacker.Attackable = false;
                    // 실행 코루틴 예약 실행
                    GAME.IGM.AddAction(player.heroData.SkillCo(player.heroSkill, targetFounded));
                    // 현재 어택커가 어떤 녀석인지 : 미니언 일반공격, 영웅웨폰공격, 영웅스킬사용 등등

                    Debug.Log("충돌체 이름 : " + targetFounded.TR.name);
                    break;
                }
                else
                {
                    // 라인궤적 그리는 코루틴 종료위해 갯수 초기화
                    LR.positionCount = 0;
                    Debug.Log("충돌체 찾기 실패");
                    // 타겟을 잘못 하였기에, 다시 공격상태로 바꿔주고 타겟팅 레이 취소하기
                    attacker.Attackable = true;
                    break;
                }
            }
            yield return null;
        }
        GAME.IGM.Turn.Col.enabled = attacker.Col.enabled = true;
        GAME.IGM.Hand.PlayerHand.ForEach(x => x.Col.enabled = true);
        yield break;
    }
    // 두 유저 인게임씬 진입 + 서로 기본정보 공유 확인시
    // 마스터 클라이언트가 게임내 트랜지션 캔버스의 알파값을 줄이면서 게임 시작 연출 코루틴
    public IEnumerator StartIntro()
    {
        Debug.Log("t시작");
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * 0.5f;
            transitionPanel.color = new Color(0,0,0,1-t);
            yield return null;
        }
        Debug.Log("끝");
    }
}
