using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Define;
using static System.Net.WebRequestMethods;
using System;

// to do list
// 1. �������� ��䱸..
// 2. ����ġ �κ� �̺�Ʈ�Լ� �ϼ�
// 3. ���� �ɷ� ���� ������ �ʿ�

public class Hero : MonoBehaviour, IBody
{
    public GameObject skillIcon, WpIcon;
    public SpriteMask playerMask;
    public HeroSkill heroSkill;
    public WeaponCardData weaponData;
    public int hp, att, dur, mp;
    public SpriteRenderer wpImg, skillImg, playerImg, AttIcon, HpIcon;
    public TextMeshPro hpTmp, weaponAttTmp, durTmp, replyTmp , mpTmp, attTmp;
    public GameObject Select, Reply, Speech;
    public bool attackable = true;
    public bool CanAttack { get { return (weaponData != null) && attackable == true; } }
    #region IBODY
    public Collider2D Col { get; set; }
    public bool Ray { set { Col.enabled = value; } }
    public bool IsMine { get; set; }
    public int PunId { get; set; }
    public Define.BodyType bodyType { get { return Define.BodyType.Meele; } }
    public Transform TR { get { return playerMask.transform; } }

    public Vector3 OriginPos { get; set; }
    public int OriginHp { get; set; }
    public int OriginAtt { get; set; }
    public int Att
    {
        get { return (weaponData != null) ? att + weaponData.att : att; }
        set 
        {
            att += value;    
        } 
    }
    public int HP
    { get; set; }
    #endregion


    // ī�޶��� ����������ĳ���� �ʿ�, ��ü�� Collider�ʿ�
    public void Awake()
    {
        OriginPos = playerMask.transform.position;
        Col = GetComponent<Collider2D>();
        IsMine = (this.gameObject.name.Contains("Player")) ? true : false;
        gameObject.layer = LayerMask.NameToLayer((IsMine == true) ? "allyHero" : "foeHero");
        this.PunId = (Photon.Pun.PhotonNetwork.IsMasterClient ? 1000 : 2000);
        attackable = true;
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
        Att = 0;
        HP = OriginHp = 30;
        // ī���˾� �̺�Ʈ ���� (���콺 ���ͷ� ����)
        GAME.Manager.UM.BindCardPopupEvent(WpIcon, ShowWeaponIcon, 0.75f);
        GAME.Manager.UM.BindCardPopupEvent(skillIcon, ShowSkillIcon, 0.75f);
        WpIcon.transform.localScale = Vector3.zero;
        // ���� ��ų�� �ʱ�ȭ
        heroSkill.InitSkill(this);
        // ���� ��ȭ�� �ʱ�ȭ

    }
    public void ChangeSortingLayer(bool isOn)
    {
        SortingLayer[] layers = SortingLayer.layers;
        SortingLayer layer = Array.Find(layers, x => x.name == ((isOn) ? "Attacker" : "None"));

        playerMask.frontSortingLayerID = layer.id;
        playerImg.sortingLayerID = layer.id;
        AttIcon.sortingLayerID = HpIcon.sortingLayerID = layer.id;
        attTmp.sortingLayerID = hpTmp.sortingLayerID = layer.id;
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
        if (Speech.gameObject.activeSelf == true )
        {
            Speech.gameObject.SetActive(false);
            return;
        }
        // ���� ������ ���°� �ƴϰų�
        // �̹� �ٸ� ��ü�� Ÿ���� ���ε� �� ��ü�� Ŭ���� ���
        if (GAME.Manager.IGM.TC.LR.gameObject.activeSelf == true || !CanAttack)
        { return;  }

        // ������ �ڽŰ�, �������� ���� ��Ȱ��ȭ
        GAME.Manager.IGM.Spawn.SpawnRay = Ray = false;

        // Ÿ���� ī�޶� ���� + ���� Ÿ���� ������ �����Լ� ���� ����
        GAME.Manager.StartCoroutine(GAME.Manager.IGM.TC.TargettingCo
            (this,
            (IBody a, IBody t) => { return AttackCo(a, t); },
            new string[] { "foe", "foeHero" }
            ));
        
        IEnumerator AttackCo(IBody attacker, IBody target)
        {
            #region ���� �ڷ�ƾ : ��뿡�� ��ġ��
            ChangeSortingLayer(true); // ������ ���÷��̾�� �Ű� �ֻ�ܿ� ��ġ�ϱ�
            float t = 0;
            Vector3 start = playerMask.transform.position;
            Vector3 dest = target.Pos;
            while (t < 1f)
            {
                t += Time.deltaTime * 1f;
                playerMask.transform.position = Vector3.Lerp(start, dest, t);
                yield return null;
            }
            #endregion

            #region ī�޶� ���� ����Ʈ
            // 0~PI ������ ���̸� ���ѵ� ������ ����ϸ�
            // 0 ~ 1 ��, 1 ~ 0 ���� �ǵ��� ���⿡ Z�� ȸ�� �ڷ�ƾ���� �̿��ϱ�� ����
            StartCoroutine(GAME.Manager.IGM.TC.ShakeCo());
            yield return null;

            #endregion

            #region ���ڸ��� ����
            t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 1f;
                playerMask.transform.localPosition = Vector3.Lerp(dest, OriginPos, t);
                yield return null;
            }
            ChangeSortingLayer(false); // ���÷��̾� �ʱ�ȭ
            #endregion

        }
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

    #region ���� ����
    public IEnumerator EquipWeapon(CardHand card)
    {
        // ���� ������ �ʱ�ȭ �� ���� ���� ����
        weaponData = (WeaponCardData)card.data;
        wpImg.sprite = card.cardImage.sprite;
        Att += weaponData.att;
        attTmp.text = weaponData.att.ToString();
        durTmp.text = weaponData.durability.ToString();

        // ���� ���� �ִϸ��̼� ����
        yield return StartCoroutine(WearingCo(card));
    }

    // ���� ���� �̺�Ʈ
    public IEnumerator WearingCo(CardHand card)
    {
        float t = 0;
        // ���� �Ҹ� �ڷ�ƾ ����
        GAME.Manager.StartCoroutine(card.FadeOutCo(card.IsMine));

        // ���Ⱑ ���� Ŀ���鼭 �����ϴ� �ڷ�ƾ ����
        while (t < 1f)
        {
            t+= Time.deltaTime *2f; 
            WpIcon.transform.localScale =
                Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }
    }
    #endregion
}
