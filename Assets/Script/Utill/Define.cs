using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    #region IBody BASE
    // 게임내 어떤 역할의 오브젝트인지
    public enum ObjType { Hero, Minion, HandCard }
    #endregion

    #region 카드 정보 데이터 BASE
    // 카드 종류
    public enum cardType { minion, spell, weapon }
    // 직업 종류
    public enum classType { HJ, HZ, KH, Netural }
    // 카드 희귀도
    public enum cardRarity { normal, rare , legend}
    #endregion

    #region 카드 이벤트 데이터 BASE
    public enum evtTargeting { Auto, Select } // 손에서 내면 자동실행건인지, 유저가 직접 선택건인지
    public enum evtWhen { onPlayed, onDead, onHand, } // 카드의 어느순간에 이벤트 실행할지 : 낼떄, 미니언죽을떄, 손에있을떄
    public enum evtArea  // 유저가 직접 선택할시엔 포스트프로세싱의 영역이 되며, 자동이나 랜덤 실행시엔 범위가 되어줄 영역
    { Enemy, Player, All } //None, All, enemy, enemyHero, allEnemy, player, playerHero, allPlayer, 
    public enum evtFaction // 위 이벤트범위에서 좀 더 상세한 카테고리범위 역할 : 그래서 미니언이냐 영웅이냐 등등..
    { All, Minion, Hero,   }
    public enum evtType { buff, attack, restore, utill , } // 어떤 이벤트인지 : 버프효과, 공격, 회복, 드로우 등

    // 위의 evtType별로 사용할 변수들

    #region 공격 이벤트 데이터 프로퍼티
    public enum attType { Damage, Kill } // 공격이벤트 타입 : 일반공격 , 죽이기
    public enum attFX // 재생할 효과 프리팹
    { None, arrow, }

    #endregion

    #region 드로우 이벤트 데이터 프로퍼티
    public enum utillType { draw, find, acquisition  } //  드로우, 발견이벤트, 획득
    
    #endregion

    #region 버프 이벤트 데이터 프로퍼티
    public enum buffAutoMode { autoOnEvtArea,  BothSide ,randomOnEvtArea ,someID } // 자동실행, 유저가 선택, 이벤트범위에서 랜덤타겟
    public enum buffType { att, hp, atthp, cost } // 어떤 버프 이벤트인지
    public enum buffFX { None, }
    #endregion

    #region 회복 이벤트 데이터 프로퍼티
    public enum restoreAutoMode // 이벤트범위에서 타겟찾아 자동실행 | 유저가 선택 | 하수인이 소환된 양옆 위치 | 이벤트범위서 랜덤타겟
    { AutoOnEvtArea, userSelect, BothSide ,RandomOnEvtArea} 
    public enum restoreFX { None, Blink}
    #endregion

    #endregion

    #region 영웅 데이터 BASE
    // 영웅의 감정표현 및 특정상황별 대사 카테고리
    public enum Emotion
    { 
        Hello, WellPlayed, Thanks, Wow, Oops, Threat,
        AlreadyAttacked, NotReady, CantAttack, ThereTaunt, TimeLimitStart, TimeLess ,
        AlreadtHeroAttacked
    }
    #endregion

    public enum Scene { Login, Lobby, InGame }

    #region UI 관련
    public enum Mouse { ClickL, ClickR, Enter, Exit , StartDrag , Dragging, EndDrag}

    // UI매니저의 안내팝업창 크기
    public enum PopupScale { Small, Medium, Big}

    // InGameScene의 IGM에서 사용할 변수
    public enum  Pos 
    {
        PlayerWeapon, PlayerSkill, PlayerInfo, PlayerHandCard, PlayerMinion,
        EnemyWeapon, EnemySkill, EnemyInfo, EnemyHandCard, EnemyMinion,
    }
    #endregion

    public enum Sound { None, Pick, Click, Back , Ready ,Summon}
}
