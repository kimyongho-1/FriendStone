using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSkill : MonoBehaviour, IBody
{
    Hero owenrUser;
    Func<IBody, IBody, IEnumerator> skillFunc;
    # region
    public bool IsMine { get; set; }
    public int PunId { get; set; }

    public Define.BodyType bodyType { get; set; }

    public Transform TR { get; set; }

    public Vector3 OriginPos { get; set; }
    public Collider2D Col { get; set; }
    public bool Ray { set { Col.enabled = value; } }

#endregion

    public void InitSkill(Hero user)
    {
        bodyType = Define.BodyType.HeroSkill;
        TR = transform;
        Col = GetComponent<CircleCollider2D>();

        // 스킬 아이콘 클릭시, 타겟팅 이벤트 실행 연결
        GAME.Manager.UM.BindEvent(this.gameObject, ClickedOnSkill, Define.Mouse.ClickL, Define.Sound.None);
        // 로비에서 인게임씬으로 진입후
        // ResourcesManager에서 사용할 유저의 덱 프로퍼티를 통해
        // 직업을 찾고 맞는 영웅 능력으로 초기화
        owenrUser = user;   
        switch (GAME.Manager.RM.GameDeck.ownerClass)
        {
            case Define.classType.HJ: skillFunc = (IBody a, IBody t) => { return HJClick(a, t); }; break;
            case Define.classType.HZ: skillFunc = (IBody a, IBody t) => { return HZClick(a, t); }; break;
            case Define.classType.KH: skillFunc = (IBody a, IBody t) => { return KHClick(a, t); }; break;
        }
    }

    public void ClickedOnSkill(GameObject go)
    {
        // 레이 비활성화 : 이미 사용하였기에 다음 자신의 턴이 오기까지 해당 이벤트 실행 불가
        // 타겟팅 코루틴이 Null이 아닐시 : 현재 다른 드래깅클릭이 스킬컴포넌트를 클릭한 경우
        if (Col.enabled == false || GAME.Manager.IGM.TC.TargetCo != null)
        { return; }

        // 공격자 자신과, 스폰영역 레이 비활성화
        GAME.Manager.IGM.Spawn.SpawnRay = Ray = false;

        // 현재 타겟팅 코루틴 등록 및 실행
        GAME.Manager.IGM.TC.TargetCo = GAME.Manager.IGM.TC.TargettingCo(this, skillFunc);

        // 타겟팅 카메라 실행 + 만약 타겟팅 성공시 공격함수 예약 실행
        GAME.Manager.IGM.TC.StartCoroutine(GAME.Manager.IGM.TC.TargetCo);
    }


    public IEnumerator HJClick(IBody attacker, IBody target)
    {
        // 대상에게 2 피해주기
        yield return null;
    }     
    public IEnumerator HZClick(IBody attacker, IBody target)
    {
        // 무작위 아군에게 +1/+1 버프
        yield return null; 
    }   
    public IEnumerator KHClick(IBody attacker, IBody target)
    { 
        // 대상을 2치료
        yield return null; 
    }
}
