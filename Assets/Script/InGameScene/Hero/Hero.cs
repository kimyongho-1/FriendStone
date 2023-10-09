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
    public HeroData heroData;
    public GameObject skillIcon, WpIcon;
    public SpriteMask playerMask;
    public HeroSkill heroSkill;
    public WeaponCardData weaponData = null;
    public int hp, att, dur, mp;
    public SpriteRenderer wpImg, skillImg, playerImg, AttIcon, HpIcon;
    public TextMeshPro hpTmp, weaponAttTmp, durTmp, replyTmp , mpTmp, attTmp, nickTmp;
    public GameObject Select, Reply, Speech;
    public bool CanAttack { get { return (weaponData != null) && Attackable == true; } }

    #region IBODY
    [field:SerializeField] public bool Attackable { get; set; }
    public IEnumerator onDead { get; set; }
    public Collider2D Col { get; set; }
    public bool Ray { set { Col.enabled = value; } }
    public bool IsMine { get; set; }
    [field:SerializeField]public int PunId { get; set; }
    public Define.ObjType objType { get; set; }
    public Transform TR { get { return playerMask.transform; } }

    [field: SerializeField] public Vector3 OriginPos { get; set; }

    public int OriginHp { get { return hp; } set { hp = value; } } // ���� ü��
    public int OriginAtt { get { return att; } set { att = value; } } // ���� ���ݷ�
    public int Att // ���� ���ݷ� [����+���� ���ݷ�]
    {
        get {  return att; }
        set { att = value ; attTmp.text = (att).ToString(); }
    }

    public int HP
    {
        get { return hp; }
        set { hp = value; hpTmp.text = hp.ToString(); }
    }
    #endregion

    // ī�޶��� ����������ĳ���� �ʿ�, ��ü�� Collider�ʿ�
    public void Awake()
    {
        OriginPos = playerMask.transform.position;
        Col = GetComponent<Collider2D>();
        IsMine = (this.gameObject.name.Contains("Player")) ? true : false;
        gameObject.layer = LayerMask.NameToLayer((IsMine == true) ? "allyHero" : "foeHero");
        Attackable = true;
        att = 0; hp = 30;
        GAME.IGM.allIBody.Add(this);
        // �� ������ �ʿ��� Ŭ�� �̺�Ʈ�� 
        if (IsMine == true)
        {
            // �� �г��� ǥ��
            nickTmp.text = GAME.Manager.NM.playerInfo.NickName;
            
            heroData = GAME.Manager.RM.GetHeroData(GAME.Manager.RM.GameDeck.ownerClass);
            heroData.Init(playerImg , skillImg, IsMine);
            // ���� �̹��� �ʱ�ȭ
            
            // ���� ��ų�� �ʱ�ȭ �� �̹��� ����
            heroSkill.InitSkill(IsMine);

            // ���� ������ ��Ŭ�� => �������
            GAME.Manager.UM.BindEvent(this.gameObject, HeroAttack, Define.Mouse.ClickL, Define.Sound.Ready);

            // ���� ������ ��Ŭ�� => ����ǥ��
            GAME.Manager.UM.BindEvent(this.gameObject, HeroSpeech, Define.Mouse.ClickR, Define.Sound.None);
            // ��ų ������ Ŭ����, Ÿ���� �̺�Ʈ ���� ����
            GAME.Manager.UM.BindEvent(heroSkill.Col.gameObject, ClickedOnSkill, Define.Mouse.ClickL, Define.Sound.None);

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
        // ���� ������ �ʱ�ȭ
        WpIcon.transform.localScale = Vector3.zero; attTmp.text = (att).ToString();
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
        GAME.IGM.ShowCardPopup(ref weaponData, (IsMine == true) ? new Vector3(-3f, -1.1f, 0) : new Vector3(4f, 3.3f, 0));
    }
    public void ShowSkillIcon() // �����ɷ� �����ܿ� Ŀ���� �����ð� ������ ���
    {
        // ī���˾� ȣ�� + ������ �����Ϳ� ��ġ�� �Բ�
        GAME.IGM.ShowHeroSkill((IsMine == true) ? new Vector3(4f, -1.3f, 0) : new Vector3(4f, 3.3f, 0), heroData);
    }
    #endregion

    #region ���� �̺�Ʈ

    // ���� ���� �ɷ� ����� �� ȭ�鿡�� ����ȭ�ҋ�, ���� ȣ���Ͽ� ���
    public void CallHeroSkillAttack(IBody target)
    {
        // ������� �˰� ������ֱ�
        heroSkill.Attackable = false;
        return;
        IEnumerator co = heroData.SkillCo(heroSkill, target);
        GAME.IGM.AddAction(EnemySkillUse(co));
        
        IEnumerator EnemySkillUse(IEnumerator co)
        {
            // ������� �˰� ������ֱ�
            heroSkill.Attackable = false;
            if (co != null)
            { yield return GAME.IGM.StartCoroutine(co); }
        }
    }
    // ������ ��ų ������ Ŭ����, ��ų�̺�Ʈ ����
    public void ClickedOnSkill(GameObject go)
    {
        // ���� �Ͽ����� ������ ����
        if (GAME.IGM.Packet.isMyTurn == false) { return; }

        if (heroSkill.Attackable == false || GAME.IGM.TC.LR.gameObject.activeSelf == true)
        { 
            return;
        }

        // ���� ��ò���
        heroSkill.Attackable = false;
        // ���� �ɷ��� ����Ÿ�����̶�� : Ÿ���� �����ҋ��� ������� ����
        if (heroData.skillTargeting == evtTargeting.Select)
        {
            // Ÿ���� ī�޶� ���� + ���� Ÿ���� ������ �����Լ� ���� ����
            GAME.IGM.TC.StartCoroutine(GAME.IGM.TC.TargettingHeroSkillCo(heroSkill));
        }
        // ���� �ɷ��� �ڵ�������̶�� : �ڵ� ���� + ���� ��ױ�
        else
        {
            // �����ɷ� ��ױ� ���� ����
            GAME.IGM.AddAction (heroData.SkillCo(heroSkill, null)) ;
        }
    }

    // ���� ��Ŭ���� ����
    public void HeroAttack(GameObject go)
    {
        // ���� �Ͽ����� ������ ����
        if (GAME.IGM.Packet.isMyTurn == false) { return; }

        // ���� ��ȭ������ �̺�Ʈ ���� �Ǵ� �������̾�����, �ش� �̺�Ʈ ����� ����
        if (Speech.gameObject.activeSelf == true )
        {
            Speech.gameObject.SetActive(false);
            return;
        }
        // ���� ������ ���°� �ƴϰų�
        // �̹� �ٸ� ��ü�� Ÿ���� ���ε� �� ��ü�� Ŭ���� ���
        if (GAME.IGM.TC.LR.gameObject.activeSelf == true || !CanAttack)
        { return;  }

        // ������ �ڽŰ�, �������� ���� ��Ȱ��ȭ
        Ray = false;

        // Ÿ���� ī�޶� ���� + ���� Ÿ���� ������ �����Լ� ���� ����
        GAME.Manager.StartCoroutine(GAME.IGM.TC.MeeleTargettingCo
            (this,
            (IBody a, IBody t) => { return AttackCo(a, t); }
            ));
    }
    public IEnumerator AttackCo(IBody attacker, IBody target)
    {
        // Ÿ���� ���� ���ٸ� , ���ڸ� ��ġ�� ����
        if (target == null)
        {
            float time = 0;
            Vector3 currPos = attacker.TR.position;
            while (time < 1f)
            {
                time += Time.deltaTime * 1f;
                this.transform.localPosition = Vector3.Lerp(currPos, OriginPos, time);
                yield return null;
            }
            yield break;
        }

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
        StartCoroutine(GAME.IGM.TC.ShakeCo());
        yield return null;

        #endregion

        #region ���ڸ��� ����
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 1f;
            playerMask.transform.position = Vector3.Lerp(dest, OriginPos, t);
            yield return null;
        }
        ChangeSortingLayer(false); // ���÷��̾� �ʱ�ȭ
        #endregion
        // ������ ��ȯ
        attacker.HP -= target.Att;
        target.HP -= attacker.Att;
        // ���� ���̰�, �� ������ �����ߴٸ�
        // ���� ���� ������ �ൿ���� Ȯ�� �� ���� �̺�Ʈ ��뿡�� ����
        if (GAME.IGM.Packet.isMyTurn && attacker.IsMine)
        {
            GAME.IGM.Packet.SendHeroAttack(attacker.PunId, target.PunId);
        }

        // ������ ���� �� ������ 0 ���޽� ���� �μ����� �ִϸ��̼� �ڷ�ƾ ����
        weaponData.durability -= 1;
        durTmp.text = weaponData.durability.ToString();
        if (weaponData != null && weaponData.durability <= 0)
        {
            StartCoroutine(BrokenWeaponCo());
        }

        if (target.HP <= 0) { yield return StartCoroutine(target.onDead); }
        if (attacker.HP <= 0) { yield return StartCoroutine(attacker.onDead); }

        Attackable = false;
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
        int index = go.transform.GetSiblingIndex();

        // 0���� ��� , ��� Ŭ���� ����ǥ�� �̺�Ʈ ���� ����
        if (index == 0)
        { Speech.gameObject.SetActive(false); }
        else
        {
            // Emotion�̳� �ε��� 0���� �����ϱ����� -1�� ���� ����
            replyTmp.text = heroData.outSpeech[(Define.Emotion)(index-1)];
        }
        Select.gameObject.SetActive(false);
        Reply.gameObject.SetActive(true);
        return;
    }
    public void OffReply(GameObject go)
    {
        Speech.gameObject.SetActive(false);
    }
    public void HeroSaying(Define.Emotion e)
    {
        // �̹� ��縦 �������̾��ٸ�
        if (Speech.gameObject.activeSelf == true)
        {
            // ���� ������, �ٽ� ����
            Speech.gameObject.SetActive(false);
        }

        // ���� ���ʰ� �ƴϸ� �� �ʿ䰡 X
        if (GAME.IGM.Packet.isMyTurn == false) { return; }

        // ��ǳ�� ���� �̺�Ʈ��, ����â�� �����ֵ��� Defalut���·� ����
        Select.gameObject.SetActive(false);
        replyTmp.text = heroData.outSpeech[e];
        Reply.gameObject.SetActive(true);
        Speech.gameObject.SetActive(true);
    }
    #endregion

    #region ���� ����
    public IEnumerator EquipWeapon(CardHand card)
    {
        // ���� ������ �ʱ�ȭ �� ���� ���� ����
        weaponData = card.WC;
        wpImg.sprite = card.cardImage.sprite;
        Att += weaponData.att;
        durTmp.text = weaponData.durability.ToString();
        weaponAttTmp.text  = weaponData.att.ToString();
        // ���� ���� �ִϸ��̼� ����
        yield return StartCoroutine(WearingCo(card));
    }
    public IEnumerator EquipWeapon(CardHand card ,CardData data)
    {
        // ���� ������ �ʱ�ȭ �� ���� ���� ����
        weaponData = (WeaponCardData)data;
        wpImg.sprite = GAME.Manager.RM.GetImage(data.cardClass,data.cardIdNum);
        Att += weaponData.att;
        durTmp.text = weaponData.durability.ToString();
        weaponAttTmp.text = weaponData.att.ToString();

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
    public IEnumerator BrokenWeaponCo()
    {
        float t = 0;
        Att -= weaponData.att;
        Vector3 start = WpIcon.transform.localScale;
        if (start != Vector3.zero)
        {
            // ���Ⱑ ���� �۾����鼭 �������� �ڷ�ƾ ����
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                WpIcon.transform.localScale =
                    Vector3.Lerp(start, Vector3.zero, t);
                yield return null;
            }
        }
       
        weaponData = null; 
    }
    #endregion

    #region ������Ȳ�� ���� ����â
    //public void 
    #endregion
}
