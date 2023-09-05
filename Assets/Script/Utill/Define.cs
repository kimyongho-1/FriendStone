using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define 
{
    #region 카드 정보 데이터
    // 카드 종류
    public enum cardType { minion, spell, weapon }

    // 직업 종류
    public enum classType { HJ, HZ, KH, Netural }
    // 카드 희귀도
    public enum cardRarity { normal, rare , legend}
    #endregion

    #region 카드 이벤트 데이터
    public enum evtWhen { onPlayed, onDead, onHand, } // 카드의 어느순간에 이벤트 실행할지 : 낼떄, 미니언죽을떄, 손에있을떄
    public enum evtArea // 이벤트 타겟팅의범위 : None => 자동실행건, 그외 유저가 선택할수잇는 범위 또는 자동실행의 범위
    { None, All, enemy, enemyHero, allEnemy, player, playerHero, allPlayer, } 
    public enum evtType { buff, attack, restore, utill , } // 어떤 이벤트인지 : 버프효과, 공격, 회복, 드로우 등

    // 위의 evtType별로 사용할 변수들

    #region 공격 이벤트 데이터 프로퍼티
    public enum attTargeting { auto , userSelect, randomOnEvtArea } // 자동실행, 유저가 선택, 이벤트범위에서 랜덤타겟
    public enum attType { Damage, Kill } // 공격이벤트 타입 : 일반공격 , 죽이기
    public enum attExtraArea // 추가 공격범위 : 없음, 타겟의 양옆 대상까지 , 공격대상의 영웅까지, 카드의 소유자
    { None, withTargetsBothSide , withTargetsHero, ownerHero }
    public enum attFX // 재생할 효과 프리팹
    { None, arrow, }
    #endregion

    #region 드로우 이벤트 데이터 프로퍼티
    public enum utillType { draw, find, acquisition ,sendItem } //  드로우, 발견이벤트, 획득, 상대에게 아이템전달
    
    #endregion

    #region 버프 이벤트 데이터 프로퍼티
    public enum buffTargeting { auto, userSelect, randomOnEvtArea } // 자동실행, 유저가 선택, 이벤트범위에서 랜덤타겟
    public enum buffType { att, hp, atthp, cost } // 어떤 버프 이벤트인지
    public enum buffExtraArea { None, withOwnerHero, withBothSide,onlyBothSide, someId } // 버프이벤트의 추가 대상여부 : 없음, 카드의 주인포함, 양옆 하수인, 특정id 하수인기준으로
    public enum buffFX { None, }
    #endregion

    #region 회복 이벤트 데이터 프로퍼티
    public enum restoreTargeting { auto, userSelect, randomOnEvtArea } // 자동실행, 유저가 선택, 이벤트범위에서 랜덤타겟
    public enum restoreExtraArea { None, addOwnerHero , BothSide } // 추가대상 여부 : 없음, 영웅까지, 양옆 미니언까지
    public enum restoreFX { None, Blink}
    #endregion



    #endregion

    public enum Scene { Login, Lobby, InGame }
    #region UI 관련
    public enum Mouse { ClickL, ClickR, Enter, Exit , StartDrag , Dragging, EndDrag}

    // UI매니저의 안내팝업창 크기
    public enum PopupScale { Small, Medium, Big}
    #endregion
}
