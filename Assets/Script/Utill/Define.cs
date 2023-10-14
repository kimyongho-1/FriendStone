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
    public enum cardRarity { normal, rare, legend }
    #endregion

    #region 카드 이벤트 데이터 BASE
    public enum evtTargeting { Auto, Select } // 손에서 내면 자동실행건인지, 유저가 직접 선택건인지
    public enum evtWhen { onPlayed, onDead, onHand, } // 카드의 어느순간에 이벤트 실행할지 : 낼떄, 미니언죽을떄, 손에있을떄
    public enum evtArea  // 유저가 직접 선택할시엔 포스트프로세싱의 영역이 되며, 자동이나 랜덤 실행시엔 범위가 되어줄 영역
    { Enemy, Player, All } //None, All, enemy, enemyHero, allEnemy, player, playerHero, allPlayer, 
    public enum evtFaction // 위 이벤트범위에서 좀 더 상세한 카테고리범위 역할 : 그래서 미니언이냐 영웅이냐 등등..
    { All, Minion, Hero, }
    public enum evtType { buff, attack, restore, utill, } // 어떤 이벤트인지 : 버프효과, 공격, 회복, 드로우 등
    public enum fxType { None, Projectile , Heal, Buff  }
    // 위의 evtType별로 사용할 변수들

    #region 공격 이벤트 데이터 프로퍼티
    public enum attType { Damage, Kill } // 공격이벤트 타입 : 일반공격 , 죽이기
    #endregion

    #region 드로우 이벤트 데이터 프로퍼티
    public enum utillType { draw, find, acquisition  } //  드로우, 발견이벤트, 획득
    
    #endregion

    #region 버프 이벤트 데이터 프로퍼티
    public enum buffAutoMode { autoOnEvtArea,  BothSide ,randomOnEvtArea ,someID } // 자동실행, 유저가 선택, 이벤트범위에서 랜덤타겟
    public enum buffType { att, hp, atthp, cost } // 어떤 버프 이벤트인지
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
        AlreadtHeroAttacked, HandOverFlow
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

    public enum IGMsound {
    
        Draw , // 덱에서 뽑히는 카드
        Pick , Cancel, // 핸드카드를 드래깅 시작, 드래깅 엔드
        Summon,// 미니언 필드 소환
        Click , // 기본 클릭음 + 공격 가능한 미니언 클릭시
        Punch , // 미니언이나 영웅이 근접공격을 수행후
        Popup, // 상대가 카드를 낼떄, 팝업창 소리음 
        TurnStart, ClickTurnBtn // 내턴이 시작될떄마다와 턴 버튼 클릭시 효과음
        // Enum으로 분류할 필요가 없는것들 (언제나 고정된곳에서 입력받을시 재생만 하면되는것들)
        // 게임승리|종료시 나오는 효과음
        // 턴종료 버튼클릭음 | 턴 시작시 효과음
        // 선택타겟팅 대기음
    }
}
