using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define 
{
    #region ī�� ���� ������
    // ī�� ����
    public enum cardType { minion, spell, weapon }

    // ���� ����
    public enum classType { HJ, HZ, KH, Netural }
    // ī�� ��͵�
    public enum cardRarity { normal, rare , legend}
    #endregion

    #region ī�� �̺�Ʈ ������
    public enum evtWhen { onPlayed, onDead, onHand, } // ī���� ��������� �̺�Ʈ �������� : ����, �̴Ͼ�������, �տ�������
    public enum evtArea // �̺�Ʈ Ÿ�����ǹ��� : None => �ڵ������, �׿� ������ �����Ҽ��մ� ���� �Ǵ� �ڵ������� ����
    { None, All, enemy, enemyHero, allEnemy, player, playerHero, allPlayer, } 
    public enum evtType { buff, attack, restore, utill , } // � �̺�Ʈ���� : ����ȿ��, ����, ȸ��, ��ο� ��

    // ���� evtType���� ����� ������

    #region ���� �̺�Ʈ ������ ������Ƽ
    public enum attTargeting { auto , userSelect, randomOnEvtArea } // �ڵ�����, ������ ����, �̺�Ʈ�������� ����Ÿ��
    public enum attType { Damage, Kill } // �����̺�Ʈ Ÿ�� : �Ϲݰ��� , ���̱�
    public enum attExtraArea // �߰� ���ݹ��� : ����, Ÿ���� �翷 ������ , ���ݴ���� ��������, ī���� ������
    { None, withTargetsBothSide , withTargetsHero, ownerHero }
    public enum attFX // ����� ȿ�� ������
    { None, arrow, }
    #endregion

    #region ��ο� �̺�Ʈ ������ ������Ƽ
    public enum utillType { draw, find, acquisition ,sendItem } //  ��ο�, �߰��̺�Ʈ, ȹ��, ��뿡�� ����������
    
    #endregion

    #region ���� �̺�Ʈ ������ ������Ƽ
    public enum buffTargeting { auto, userSelect, randomOnEvtArea } // �ڵ�����, ������ ����, �̺�Ʈ�������� ����Ÿ��
    public enum buffType { att, hp, atthp, cost } // � ���� �̺�Ʈ����
    public enum buffExtraArea { None, withOwnerHero, withBothSide,onlyBothSide, someId } // �����̺�Ʈ�� �߰� ��󿩺� : ����, ī���� ��������, �翷 �ϼ���, Ư��id �ϼ��α�������
    public enum buffFX { None, }
    #endregion

    #region ȸ�� �̺�Ʈ ������ ������Ƽ
    public enum restoreTargeting { auto, userSelect, randomOnEvtArea } // �ڵ�����, ������ ����, �̺�Ʈ�������� ����Ÿ��
    public enum restoreExtraArea { None, addOwnerHero , BothSide } // �߰���� ���� : ����, ��������, �翷 �̴Ͼ����
    public enum restoreFX { None, Blink}
    #endregion



    #endregion

    public enum Scene { Login, Lobby, InGame }
    #region UI ����
    public enum Mouse { ClickL, ClickR, Enter, Exit , StartDrag , Dragging, EndDrag}

    // UI�Ŵ����� �ȳ��˾�â ũ��
    public enum PopupScale { Small, Medium, Big}
    #endregion
}
