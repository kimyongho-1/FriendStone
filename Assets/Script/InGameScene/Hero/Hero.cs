using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Define;
using static System.Net.WebRequestMethods;
using System;

// to do list
// 1. 음성파일 재요구..
// 2. 스피치 부분 이벤트함수 완성
// 3. 영웅 능력 구현 마무리 필요

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


    // 카메라의 피직스레이캐스터 필요, 객체에 Collider필요
    public void Awake()
    {
        OriginPos = playerMask.transform.position;
        Col = GetComponent<Collider2D>();
        IsMine = (this.gameObject.name.Contains("Player")) ? true : false;
        gameObject.layer = LayerMask.NameToLayer((IsMine == true) ? "allyHero" : "foeHero");
        this.PunId = (Photon.Pun.PhotonNetwork.IsMasterClient ? 1000 : 2000);
        attackable = true;
        // 내 영웅만 필요한 클릭 이벤트들 
        if (IsMine == true)
        {
            // 영웅 아이콘 좌클릭 => 무기공격
            GAME.Manager.UM.BindEvent(this.gameObject, HeroAttack, Define.Mouse.ClickL, Define.Sound.Ready);

            // 영웅 아이콘 우클릭 => 감정표현
            GAME.Manager.UM.BindEvent(this.gameObject, HeroSpeech, Define.Mouse.ClickR, Define.Sound.None);

            // 각각의 말풍선에 이벤트 연결
            for (int i = 0; i < Select.gameObject.transform.childCount; i++)
            {
                GAME.Manager.UM.BindEvent(Select.transform.GetChild(i).gameObject,
                    SelectedSpeech, Define.Mouse.ClickL, Define.Sound.None);
            }
            
            // 선택대사 나올떄, 배경클릭시 대사 즉각 종료
            GAME.Manager.UM.BindEvent(Reply.gameObject, OffReply, Mouse.ClickL, Sound.None);
        }
        Att = 0;
        HP = OriginHp = 30;
        // 카드팝업 이벤트 연결 (마우스 엔터로 구현)
        GAME.Manager.UM.BindCardPopupEvent(WpIcon, ShowWeaponIcon, 0.75f);
        GAME.Manager.UM.BindCardPopupEvent(skillIcon, ShowSkillIcon, 0.75f);
        WpIcon.transform.localScale = Vector3.zero;
        // 영웅 스킬란 초기화
        heroSkill.InitSkill(this);
        // 영웅 대화지 초기화

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
    #region EnterExit이벤트
    public void ShowWeaponIcon() // 무기아이콘에 커서를 일정시간 가져다 댈시
    {
        // 카드팝업 호출 + 보여질 데이터와 위치값 함께
        GAME.Manager.IGM.ShowCardPopup(ref weaponData, new Vector3(-3f, -1.3f, 0));
    }
    public void ShowSkillIcon() // 영웅능력 아이콘에 커서를 일정시간 가져다 댈시
    {
        // 카드팝업 호출 + 보여질 데이터와 위치값 함께
        GAME.Manager.IGM.ShowHeroSkill(new Vector3(4f, -1.3f, 0), heroSkill.data);
    }
    #endregion

    #region 영웅 이벤트

    // 영웅 좌클릭시 공격
    public void HeroAttack(GameObject go)
    {
        // 만약 대화선택지 이벤트 선택 또는 실행중이었따면, 해당 이벤트 종료로 실행
        if (Speech.gameObject.activeSelf == true )
        {
            Speech.gameObject.SetActive(false);
            return;
        }
        // 공격 가능한 상태가 아니거나
        // 이미 다른 객체가 타겟팅 중인데 이 객체를 클릭시 취소
        if (GAME.Manager.IGM.TC.LR.gameObject.activeSelf == true || !CanAttack)
        { return;  }

        // 공격자 자신과, 스폰영역 레이 비활성화
        GAME.Manager.IGM.Spawn.SpawnRay = Ray = false;

        // 타겟팅 카메라 실행 + 만약 타겟팅 성공시 공격함수 예약 실행
        GAME.Manager.StartCoroutine(GAME.Manager.IGM.TC.TargettingCo
            (this,
            (IBody a, IBody t) => { return AttackCo(a, t); },
            new string[] { "foe", "foeHero" }
            ));
        
        IEnumerator AttackCo(IBody attacker, IBody target)
        {
            #region 공격 코루틴 : 상대에게 박치기
            ChangeSortingLayer(true); // 공격자 소팅레이어로 옮겨 최상단에 위치하기
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

            #region 카메라 흔들기 이펙트
            // 0~PI 까지의 길이를 정한뒤 사인을 사용하면
            // 0 ~ 1 후, 1 ~ 0 으로 되돌아 오기에 Z축 회전 코루틴으로 이용하기로 결정
            StartCoroutine(GAME.Manager.IGM.TC.ShakeCo());
            yield return null;

            #endregion

            #region 제자리로 복귀
            t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 1f;
                playerMask.transform.localPosition = Vector3.Lerp(dest, OriginPos, t);
                yield return null;
            }
            ChangeSortingLayer(false); // 소팅레이어 초기화
            #endregion

        }
    }

    // 영웅 우클릭시 대화 이벤트
    public void HeroSpeech(GameObject go)
    {
        // 현재 말풍선이 켜져있으면 중복 방지로 끄기
        if (Speech.gameObject.activeSelf == true) { return; }
        
        // 말풍선 선택 이벤트시, 선택창만 켜져있도록 Defalut상태로 변경
        Select.gameObject.SetActive(true);
        Reply.gameObject.SetActive(false);
        Speech.gameObject.SetActive(true);
    }

    // 유저가 선택하는 말풍선중 하나를 클릭하였을떄
    public void SelectedSpeech(GameObject go)
    {
        // 클릭된 객체의 자식 순서로 파악
        switch (go.transform.GetSiblingIndex())
        {
            // 0 번자식 취소
            case 0:
                Debug.Log("종료 호출");
                Speech.gameObject.SetActive(false);
                return;
            case 1:
                replyTmp.text = "안녕!";
                break;
            case 2:
                replyTmp.text = "잘헀네";
                break;
            case 3:
                replyTmp.text = "고마워";
                break;
            case 4:
                replyTmp.text = "이야!";
                break;
            case 5:
                replyTmp.text = "이런..";
                break;
            case 6:
                replyTmp.text = "죽을래?";
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

    #region 무기 착용
    public IEnumerator EquipWeapon(CardHand card)
    {
        // 무기 데이터 초기화 및 공격 가능 설정
        weaponData = (WeaponCardData)card.data;
        wpImg.sprite = card.cardImage.sprite;
        Att += weaponData.att;
        attTmp.text = weaponData.att.ToString();
        durTmp.text = weaponData.durability.ToString();

        // 무기 착용 애니메이션 시작
        yield return StartCoroutine(WearingCo(card));
    }

    // 무기 착용 이벤트
    public IEnumerator WearingCo(CardHand card)
    {
        float t = 0;
        // 무기 소멸 코루틴 실행
        GAME.Manager.StartCoroutine(card.FadeOutCo(card.IsMine));

        // 무기가 점차 커지면서 착용하는 코루틴 실행
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
