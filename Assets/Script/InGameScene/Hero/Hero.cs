using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Define;

// to do list
// 1. �������� ��䱸..
// 2. ����ġ �κ� �̺�Ʈ�Լ� �ϼ�
// 3. ���� �ɷ� ���� ������ �ʿ�

public class Hero : MonoBehaviour, IBody
{
    public GameObject skillIcon, WpIcon, PlayerIcon;
    public HeroSkill heroSkill;
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
    public Transform TR { get { return PlayerIcon.transform; } }

    public Vector3 OriginPos { get; set; }
    #endregion


    // ī�޶��� ����������ĳ���� �ʿ�, ��ü�� Collider�ʿ�
    public void Awake()
    {
        OriginPos = transform.localPosition;
        Col = GetComponent<Collider2D>();
        IsMine = (this.gameObject.name.Contains("Player")) ? true : false;
        gameObject.layer = LayerMask.NameToLayer((IsMine == true) ? "allyHero" : "foeHero");
        this.PunId = (Photon.Pun.PhotonNetwork.IsMasterClient ? 1000 : 2000);

        // �� ������ �ʿ��� Ŭ�� �̺�Ʈ�� 
        if (IsMine == true)
        {
            // ���� ������ ��Ŭ�� => �������
            GAME.Manager.UM.BindEvent(this.gameObject, HeroAttack, Define.Mouse.ClickL, Define.Sound.Ready);

            // ���� ������ ��Ŭ�� => ����ǥ��
            GAME.Manager.UM.BindEvent(this.gameObject, HeroSpeech, Define.Mouse.ClickR, Define.Sound.None);

            // ������ ��ǳ���� �̺�Ʈ ����
            for (int i = 0; i < Select.gameObject.transform.childCount; i++)
            {
                GAME.Manager.UM.BindEvent(Select.transform.GetChild(i).gameObject,
                    SelectedSpeech, Define.Mouse.ClickL, Define.Sound.None);
            }
            
            // ���ô�� ���Ë�, ���Ŭ���� ��� �ﰢ ����
            GAME.Manager.UM.BindEvent(Reply.gameObject, OffReply, Mouse.ClickL, Sound.None);
        }

        // ī���˾� �̺�Ʈ ���� (���콺 ���ͷ� ����)
        GAME.Manager.UM.BindCardPopupEvent(WpIcon, ShowWeaponIcon, 0.75f);
        GAME.Manager.UM.BindCardPopupEvent(skillIcon, ShowSkillIcon, 0.75f);

        // ���� ��ų�� �ʱ�ȭ
        heroSkill.InitSkill(this);
        // ���� ��ȭ�� �ʱ�ȭ

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
        GAME.Manager.IGM.ShowHeroSkill(new Vector3(4f, -1.3f, 0), heroSkill.data);
    }
    #endregion

    #region ���� �̺�Ʈ

    // ���� ��Ŭ���� ����
    public void HeroAttack(GameObject go)
    {
        // ���� ��ȭ������ �̺�Ʈ ���� �Ǵ� �������̾�����, �ش� �̺�Ʈ ����� ����
        if (Speech.gameObject.activeSelf == true)
        {
            Speech.gameObject.SetActive(false);
            return;
        }

        // ��ȭ �̺�Ʈ ������ �ƴϸ� ���� ���� Ȯ���� ���� ����
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
        Speech.gameObject.SetActive(true);
    }

    // ������ �����ϴ� ��ǳ���� �ϳ��� Ŭ���Ͽ�����
    public void SelectedSpeech(GameObject go)
    {
        // Ŭ���� ��ü�� �ڽ� ������ �ľ�
        switch (go.transform.GetSiblingIndex())
        {
            // 0 ���ڽ� ���
            case 0:
                Debug.Log("���� ȣ��");
                Speech.gameObject.SetActive(false);
                return;
            case 1:
                replyTmp.text = "�ȳ�!";
                break;
            case 2:
                replyTmp.text = "������";
                break;
            case 3:
                replyTmp.text = "����";
                break;
            case 4:
                replyTmp.text = "�̾�!";
                break;
            case 5:
                replyTmp.text = "�̷�..";
                break;
            case 6:
                replyTmp.text = "������?";
                break;
        }
        Select.gameObject.SetActive(false);
        Reply.gameObject.SetActive(true);   
    }
    public void OffReply(GameObject go)
    {
        Speech.gameObject.SetActive(false);
    }
    #endregion

    
}
