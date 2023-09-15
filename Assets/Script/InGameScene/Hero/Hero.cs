using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Define;

// to do list
// 1. 음성파일 재요구..
// 2. 스피치 부분 이벤트함수 완성
// 3. 영웅 능력 구현 마무리 필요

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
    public Transform TR { get { return this.transform; } }

    public Vector3 OriginPos { get; set; }
    #endregion


    // 카메라의 피직스레이캐스터 필요, 객체에 Collider필요
    public void Awake()
    {
        OriginPos = transform.localPosition;
        Col = PlayerIcon.GetComponent<Collider2D>();
        Debug.Log(Col);
        IsMine = (this.gameObject.name.Contains("Player")) ? true : false;

        // 내 영웅만 필요한 클릭 이벤트들 
        if (IsMine == true)
        {
            // 영웅 아이콘 좌클릭 => 무기공격
            GAME.Manager.UM.BindEvent(PlayerIcon, HeroAttack, Define.Mouse.ClickL, Define.Sound.Ready);

            // 영웅 아이콘 우클릭 => 감정표현
            GAME.Manager.UM.BindEvent(PlayerIcon, HeroSpeech, Define.Mouse.ClickR, Define.Sound.None);

            // 각각의 말풍선에 이벤트 연결
            for (int i = 0; i < Select.gameObject.transform.childCount; i++)
            {
                GAME.Manager.UM.BindEvent(Select.transform.GetChild(i).gameObject,
                    SelectedSpeech, Define.Mouse.ClickL, Define.Sound.None);
            }
            
            // 선택대사 나올떄, 배경클릭시 대사 즉각 종료
            GAME.Manager.UM.BindEvent(Reply.gameObject, OffReply, Mouse.ClickL, Sound.None);
        }

        // 카드팝업 이벤트 연결 (마우스 엔터로 구현)
        GAME.Manager.UM.BindCardPopupEvent(WpIcon, ShowWeaponIcon, 0.75f);
        GAME.Manager.UM.BindCardPopupEvent(skillIcon, ShowSkillIcon, 0.75f);

        // 영웅 스킬란 초기화
        heroSkill.InitSkill(this);
        // 영웅 대화지 초기화

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
        GAME.Manager.IGM.ShowCardPopup(ref heroSkill, new Vector3(4f, -1.3f, 0));
    }
    #endregion

    #region 영웅 이벤트

    // 영웅 좌클릭시 공격
    public void HeroAttack(GameObject go)
    {
        // 만약 대화선택지 이벤트 선택 또는 실행중이었따면, 해당 이벤트 종료로 실행
        if (Speech.gameObject.activeSelf == true)
        {
            Speech.gameObject.SetActive(false);
            return;
        }

        // 대화 이벤트 도중이 아니면 무기 여부 확인후 공격 실행
        Debug.Log("Test");
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

    
}
