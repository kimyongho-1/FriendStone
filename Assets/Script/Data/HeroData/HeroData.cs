using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
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
    };
    public string skillName;
    public string skillDesc;
    public int skillCost;
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
                SkillCo = (IBody owner , IBody target) => { return GAME.IGM.Battle.AttackEvt(owner, target, 2, Define.attType.Damage, false); };
                break;
            case Define.classType.HZ:
                skillTargeting = Define.evtTargeting.Auto;
                SkillCo = (IBody owner, IBody target) =>  { return GAME.IGM.Hand.CardDrawing(1); };
                break;
            case Define.classType.KH:
                skillTargeting = Define.evtTargeting.Select;
                SkillCo = (IBody owner, IBody target) => { return GAME.IGM.Battle.Restore(owner, target, 2, false); };
                break;
            default:
                break;
        }
    }

}
