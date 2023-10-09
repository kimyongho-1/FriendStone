using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    #region IBody BASE
    // ���ӳ� � ������ ������Ʈ����
    public enum ObjType { Hero, Minion, HandCard }
    #endregion

    #region ī�� ���� ������ BASE
    // ī�� ����
    public enum cardType { minion, spell, weapon }
    // ���� ����
    public enum classType { HJ, HZ, KH, Netural }
    // ī�� ��͵�
    public enum cardRarity { normal, rare , legend}
    #endregion

    #region ī�� �̺�Ʈ ������ BASE
    public enum evtTargeting { Auto, Select } // �տ��� ���� �ڵ����������, ������ ���� ���ð�����
    public enum evtWhen { onPlayed, onDead, onHand, } // ī���� ��������� �̺�Ʈ �������� : ����, �̴Ͼ�������, �տ�������
    public enum evtArea  // ������ ���� �����ҽÿ� ����Ʈ���μ����� ������ �Ǹ�, �ڵ��̳� ���� ����ÿ� ������ �Ǿ��� ����
    { Enemy, Player, All } //None, All, enemy, enemyHero, allEnemy, player, playerHero, allPlayer, 
    public enum evtFaction // �� �̺�Ʈ�������� �� �� ���� ī�װ����� ���� : �׷��� �̴Ͼ��̳� �����̳� ���..
    { All, Minion, Hero,   }
    public enum evtType { buff, attack, restore, utill , } // � �̺�Ʈ���� : ����ȿ��, ����, ȸ��, ��ο� ��

    // ���� evtType���� ����� ������

    #region ���� �̺�Ʈ ������ ������Ƽ
    public enum attType { Damage, Kill } // �����̺�Ʈ Ÿ�� : �Ϲݰ��� , ���̱�
    public enum attFX // ����� ȿ�� ������
    { None, arrow, }

    #endregion

    #region ��ο� �̺�Ʈ ������ ������Ƽ
    public enum utillType { draw, find, acquisition  } //  ��ο�, �߰��̺�Ʈ, ȹ��
    
    #endregion

    #region ���� �̺�Ʈ ������ ������Ƽ
    public enum buffAutoMode { autoOnEvtArea,  BothSide ,randomOnEvtArea ,someID } // �ڵ�����, ������ ����, �̺�Ʈ�������� ����Ÿ��
    public enum buffType { att, hp, atthp, cost } // � ���� �̺�Ʈ����
    public enum buffFX { None, }
    #endregion

    #region ȸ�� �̺�Ʈ ������ ������Ƽ
    public enum restoreAutoMode // �̺�Ʈ�������� Ÿ��ã�� �ڵ����� | ������ ���� | �ϼ����� ��ȯ�� �翷 ��ġ | �̺�Ʈ������ ����Ÿ��
    { AutoOnEvtArea, userSelect, BothSide ,RandomOnEvtArea} 
    public enum restoreFX { None, Blink}
    #endregion

    #endregion

    #region ���� ������ BASE
    // ������ ����ǥ�� �� Ư����Ȳ�� ��� ī�װ�
    public enum Emotion
    { 
        Hello, WellPlayed, Thanks, Wow, Oops, Threat,
        AlreadyAttacked, NotReady, CantAttack, ThereTaunt, TimeLimitStart, TimeLess ,
        AlreadtHeroAttacked
    }
    #endregion

    public enum Scene { Login, Lobby, InGame }

    #region UI ����
    public enum Mouse { ClickL, ClickR, Enter, Exit , StartDrag , Dragging, EndDrag}

    // UI�Ŵ����� �ȳ��˾�â ũ��
    public enum PopupScale { Small, Medium, Big}

    // InGameScene�� IGM���� ����� ����
    public enum  Pos 
    {
        PlayerWeapon, PlayerSkill, PlayerInfo, PlayerHandCard, PlayerMinion,
        EnemyWeapon, EnemySkill, EnemyInfo, EnemyHandCard, EnemyMinion,
    }
    #endregion

    public enum Sound { None, Pick, Click, Back , Ready ,Summon}
}
