using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Define;

public class Hero : MonoBehaviour, IBody
{
    public GameObject skillIcon, WpIcon, PlayerIcon;
    public HeroSkill skillData;
    public WeaponCardData weaponData;
    public int hp, att, dur, mp;
    public SpriteRenderer wpImg, skillImg;
    public TextMeshPro hpTmp, attTmp, durTmp, replyTmp , mpTmp;
    public GameObject Select, Reply, Speech;

    #region IBODY
    public Collider2D Col { get; set; }
    public bool Ray { set { Col.enabled = value; } }
    public bool IsMine { get; set; }
    public int PunId { get; set; }
    public Define.BodyType bodyType { get { return Define.BodyType.Hero; } }
    public Transform TR { get { return this.transform; } }

    public Vector3 OriginPos { get; set; }
    #endregion


    // ī�޶��� ����������ĳ���� �ʿ�, ��ü�� Collider�ʿ�
    public void Awake()
    {
        OriginPos = transform.localPosition;
        Col = PlayerIcon.GetComponent<Collider2D>();
        Debug.Log(Col);
        IsMine = (this.gameObject.name.Contains("Player")) ? true : false;

        // �� ������ �ʿ��� Ŭ�� �̺�Ʈ�� 
        if (IsMine == true)
        {
            // ���� ������ ��Ŭ�� => �������
            GAME.Manager.UM.BindEvent(PlayerIcon, HeroAttack, Define.Mouse.ClickL, Define.Sound.Ready);

            // ���� ������ ��Ŭ�� => ����ǥ��
            GAME.Manager.UM.BindEvent(PlayerIcon, HeroSpeech, Define.Mouse.ClickR, Define.Sound.None);

            // ������ ��ǳ���� �̺�Ʈ ����
            for (int i = 0; i < Select.gameObject.transform.childCount; i++)
            {
                GAME.Manager.UM.BindEvent(Select.transform.GetChild(i).gameObject,
                    SelectedSpeech, Define.Mouse.ClickL, Define.Sound.None);
            }
        }

        // ī���˾� �̺�Ʈ ���� (���콺 ���ͷ� ����)
        GAME.Manager.UM.BindCardPopupEvent(WpIcon, ShowWeaponIcon, 0.75f);
        GAME.Manager.UM.BindCardPopupEvent(skillIcon, ShowSkillIcon, 0.75f);
    }

    #region EnterExit�̺�Ʈ
    public void ShowWeaponIcon() // ��������ܿ� Ŀ���� �����ð� ������ ���
    {
        // ī���˾� ȣ�� + ������ �����Ϳ� ��ġ�� �Բ�
        GAME.Manager.IGM.ShowCardPopup(ref weaponData, new Vector3(-3f, -1.3f, 0));
    }
    public void ShowSkillIcon() // �����ɷ� �����ܿ� Ŀ���� �����ð� ������ ���
    {
        // ī���˾� ȣ�� + ������ �����Ϳ� ��ġ�� �Բ�
        GAME.Manager.IGM.ShowCardPopup(ref skillData, new Vector3(4f, -1.3f, 0));
    }
    #endregion

    #region ���� �̺�Ʈ

    // ���� ��Ŭ���� ����
    public void HeroAttack(GameObject go)
    {
        Debug.Log("Test");
    }

    // ���� ��Ŭ���� ��ȭ �̺�Ʈ
    public void HeroSpeech(GameObject go)
    {
        // ���� ��ǳ���� ���������� �ߺ� ������ ����
        if (Speech.gameObject.activeSelf == true) { return; }
        
        // ��ǳ�� ���� �̺�Ʈ��, ����â�� �����ֵ��� Defalut���·� ����
        Select.gameObject.SetActive(true);
        Reply.gameObject.SetActive(false);   
    }

    // ������ �����ϴ� ��ǳ���� �ϳ��� Ŭ���Ͽ�����
    public void SelectedSpeech(GameObject go)
    {
        // Ŭ���� ��ü�� �ڽ� ������ �ľ�
        switch (go.transform.GetSiblingIndex())
        {
            // 0 ���ڽ� ���
            case 0: Speech.gameObject.SetActive(false);
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
        }
    }
    
    #endregion

    
}
