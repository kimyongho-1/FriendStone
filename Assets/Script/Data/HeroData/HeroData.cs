using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using static Define;
using static UnityEngine.GraphicsBuffer;

[Serializable]
public class HeroData
{
    public bool IsMine;
    public Define.evtTargeting skillTargeting; 
    public Define.classType classType;
    // 각 영웅별 선택 감정표현별 대사 
    public Dictionary<Define.Emotion, string> outSpeech = new Dictionary<Define.Emotion, string>() // CardDataEditor스크립트에서 해당Value 할당
    {
        { Define.Emotion.Hello , ""},
        { Define.Emotion.WellPlayed , ""},
        { Define.Emotion.Thanks , ""},
        { Define.Emotion.Wow , ""},
        { Define.Emotion.Oops , ""},
        { Define.Emotion.Threat , ""},
        { Define.Emotion.AlreadyAttacked , ""},
        { Define.Emotion.CantAttack , ""},
        { Define.Emotion.ThereTaunt , ""},
        { Define.Emotion.TimeLimitStart , ""},
        { Define.Emotion.TimeLess , ""},
        { Define.Emotion.NotReady , ""},
        { Define.Emotion.AlreadtHeroAttacked , ""},
        { Define.Emotion.HandOverFlow , ""},
    };
    public Dictionary<Define.Emotion, string> outAudio = new Dictionary<Define.Emotion, string>() // CardDataEditor스크립트에서 해당Value 할당
    {
        { Define.Emotion.Hello ,""},
        { Define.Emotion.WellPlayed , ""},
        { Define.Emotion.Thanks ,""},
        { Define.Emotion.Wow ,""},
        { Define.Emotion.Oops ,""},
        { Define.Emotion.Threat , ""},
    };
    public string skillName;
    public string skillDesc;
    public int skillCost, skillAmount;
    public Func<IBody, IBody, IEnumerator> SkillCo;
    public void Init(SpriteRenderer heroImg, SpriteRenderer skillImg , bool isMine)
    {
        IsMine = isMine;
        heroImg.sprite = GAME.Manager.RM.GetHeroImage(classType);
        skillImg.sprite = GAME.Manager.RM.GetHeroSkillIcon(classType);
        switch (classType)
        {
            case Define.classType.HJ:
                skillTargeting = Define.evtTargeting.Select;
                SkillCo = (IBody owner , IBody target) => {
                    Debug.Log($"attackerPun:{owner.PunId}, attackerTR : {owner.TR.name},{owner.Pos}");
                    return HeroSkillUse(owner, target);
                    IEnumerator HeroSkillUse(IBody owner, IBody target)
                    {
                        yield return GAME.IGM.StartCoroutine(Throw(owner, target, skillAmount));
                        if (GAME.IGM.Packet.isMyTurn == true && IsMine == true)
                        { GAME.IGM.Packet.SendHeroSkillEvt(target.PunId); }
                        yield return null;
                    }
                };
                break;
            case Define.classType.HZ:
                skillTargeting = Define.evtTargeting.Auto;
                SkillCo = (IBody owner, IBody target) =>
                {
                    return HeroSkillUse(owner, target);
                    IEnumerator HeroSkillUse(IBody owner, IBody target)
                    {
                        // 상대방은 실행하지 않도록
                        if (GAME.IGM.Packet.isMyTurn == true && IsMine == true)
                        {
                            yield return GAME.IGM.StartCoroutine(GAME.IGM.Hand.CardDrawing(skillAmount));
                            GAME.IGM.Packet.SendHeroSkillEvt(1000); 
                        }
                        yield return null;
                    }
                };
                break;
            case Define.classType.KH:
                skillTargeting = Define.evtTargeting.Select;
                SkillCo = (IBody owner, IBody target) => {
                    Debug.Log($"attackerPun:{owner.PunId}, attackerTR : {owner.TR.name},{owner.Pos}");
                    return HeroSkillUse(owner, target);
                    IEnumerator HeroSkillUse(IBody owner, IBody target)
                    {
                        yield return GAME.IGM.StartCoroutine(Restore(owner, target, skillAmount));
                        if (GAME.IGM.Packet.isMyTurn == true && IsMine == true)
                        { GAME.IGM.Packet.SendHeroSkillEvt(target.PunId); }
                    }
                };
                break;
            default:
                break;
        };
    }
    IEnumerator Throw(IBody attacker, IBody target, int attAmount)
    {
        // 이펙트객체 호출
        IFx fx = GAME.IGM.Battle. FX.GetFX(Define.fxType.Projectile);

        // 호출한 이펙트 재생
        yield return GAME.IGM.StartCoroutine(fx.Invoke(attacker, target));

        Debug.Log($"attacker : {attacker}[{attacker.PunId}], target : {target}[{target.PunId}]");
        target.HP -= attAmount;
        if (target.HP <= 0)
        {
            yield return GAME.IGM.StartCoroutine(target.onDead);
        }
    }
    IEnumerator Restore(IBody caster, IBody target, int healAmount)
    {  
        // 이펙트객체 호출
        IFx fx = GAME.IGM.Battle.FX.GetFX(Define.fxType.Heal);

        // 호출한 이펙트 재생
        yield return GAME.IGM.StartCoroutine(fx.Invoke(caster, target));

        Debug.Log($"치료이벤트 실행, target : {target}[{target.PunId}]");
        target.HP = Mathf.Clamp(target.HP + healAmount, 0, (target.objType == Define.ObjType.Minion) ? target.OriginHp : 30);

        yield break;
    }
}
