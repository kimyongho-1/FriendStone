using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSkill : MonoBehaviour, IBody
{
    Func<IBody, IBody, IEnumerator> skillFunc;
    public SkillData data ;
    # region
    public IEnumerator onDead { get; set; }
    public bool IsMine { get; set; }
    public int PunId { get; set; }

    public Define.AttType AttType { get; set; }
    public Define.ObjType objType { get; set; }
    public Transform TR { get; set; }

    public Vector3 OriginPos { get; set; }
    public Collider2D Col { get; set; }
    public bool Ray { set { Col.enabled = value; } }
    public int OriginAtt { get; set; }
    public int OriginHp { get; set; }
    public int Att { get; set; }
    public int HP { get; set; }

    #endregion
    public void InitEnemySkill(Hero user, Define.classType type)
    {
        AttType = Define.AttType.None;
        TR = transform;
        Col = GetComponent<CircleCollider2D>();
        user.playerImg.sprite = GAME.Manager.RM.GetHeroImage(type);
        switch (type)
        {
            case Define.classType.HJ:
                data = new HJskill();
                data.Name = "소주 맥이기";
                data.Desc = "마우스로 적 대상을 선택해 피해를 2 줍니다";
                GAME.IGM.Hero.Player.skillImg.sprite =
                data.Image = GAME.Manager.RM.GetImage(Define.classType.HJ, 4);
                break;
            case Define.classType.HZ:
                data = new HZskill();
                data.Name = "공부 시키기";
                data.Desc = "마우스로 대상을 선택해 +1/+1을 부여합니다";
                GAME.IGM.Hero.Player.skillImg.sprite =
                data.Image = GAME.Manager.RM.GetImage(Define.classType.HZ, 15);
                break;
            case Define.classType.KH:
                data = new KHskill();
                data.Name = "감자탕 먹이기";
                data.Desc = "대상을 선택해 2치유합니다";
                GAME.IGM.Hero.Player.skillImg.sprite =
                data.Image = GAME.Manager.RM.GetImage(Define.classType.KH, 28);
                break;
        }
        skillFunc = data.SkillClickEvt;
    }
    public void InitSkill(Hero user)
    {
        AttType = Define.AttType.None;
        TR = transform;
        Col = GetComponent<CircleCollider2D>();

        // 스킬 아이콘 클릭시, 타겟팅 이벤트 실행 연결
        GAME.Manager.UM.BindEvent(this.gameObject, ClickedOnSkill, Define.Mouse.ClickL, Define.Sound.None);
        // 로비에서 인게임씬으로 진입후
        // ResourcesManager에서 사용할 유저의 덱 프로퍼티를 통해
        // 직업을 찾고 맞는 영웅 능력으로 초기화
        switch (GAME.Manager.RM.GameDeck.ownerClass)
        {
            case Define.classType.HJ:
                data = new HJskill();
                data.Name = "소주 맥이기";
                data.Desc = "마우스로 적 대상을 선택해 피해를 2 줍니다";
                GAME.IGM.Hero.Player.skillImg.sprite = 
                data.Image = GAME.Manager.RM.GetImage(Define.classType.HJ,4);
                break;
            case Define.classType.HZ:
                data = new HZskill();
                data.Name = "공부 시키기";
                data.Desc = "마우스로 대상을 선택해 +1/+1을 부여합니다";
                GAME.IGM.Hero.Player.skillImg.sprite =
                data.Image = GAME.Manager.RM.GetImage(Define.classType.HZ, 15);
                break;
            case Define.classType.KH:
                data = new KHskill();
                data.Name = "감자탕 먹이기";
                data.Desc = "대상을 선택해 2치유합니다";
                GAME.IGM.Hero.Player.skillImg.sprite =
                data.Image = GAME.Manager.RM.GetImage(Define.classType.KH, 28);
                break;
        }
        skillFunc = data.SkillClickEvt;
    }

    public void ClickedOnSkill(GameObject go)
    {
        // 레이 비활성화 : 이미 사용하였기에 다음 자신의 턴이 오기까지 해당 이벤트 실행 불가
        // 타겟팅 코루틴이 Null이 아닐시 : 현재 다른 드래깅클릭이 스킬컴포넌트를 클릭한 경우
        if (Col.enabled == false || GAME.IGM.TC.Arrow.gameObject.activeSelf == true)
        { return; }

        // 공격자 자신과, 스폰영역 레이 비활성화
        GAME.IGM.Spawn.SpawnRay = Ray = false;

        // 타겟팅 카메라 실행 + 만약 타겟팅 성공시 공격함수 예약 실행
        GAME.IGM.TC.StartCoroutine(GAME.IGM.TC.TargettingCo(this, skillFunc, new string[] { "foe" }));
    }


}

public abstract class SkillData
{
    public abstract IEnumerator SkillClickEvt(IBody attacker, IBody target);
    public string Desc,Name;
    public Sprite Image;
}

public class HJskill : SkillData
{
    public override IEnumerator SkillClickEvt(IBody attacker, IBody target)
    {
        // 대상에게 2 피해주기
        yield return null;
    }
}
public class HZskill : SkillData
{
    public override IEnumerator SkillClickEvt(IBody attacker, IBody target)
    {
        // 무작위 아군에게 +1/+1 버프
        yield return null;
    }
}
public class KHskill : SkillData
{
    public override IEnumerator SkillClickEvt(IBody attacker, IBody target)
    { 
        // 대상을 2치료
        yield return null;
    }
}