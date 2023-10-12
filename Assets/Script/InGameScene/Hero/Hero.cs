using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Define;
using static System.Net.WebRequestMethods;
using System;

public class Hero : MonoBehaviour, IBody
{
    AudioSource audioPlayer;
    public HeroData heroData;
    public GameObject skillIcon, WpIcon;
    public SpriteMask playerMask;
    public HeroSkill heroSkill;
    public WeaponCardData weaponData = null;
    public int hp, att, dur, mp;
    public SpriteRenderer wpImg, skillImg, playerImg, AttIcon, HpIcon;
    public TextMeshPro hpTmp, weaponAttTmp, durTmp, replyTmp , mpTmp, attTmp, nickTmp;
    public GameObject Select, Speech;
    public ReplyBG Reply;
    public bool CanAttack { get { return att > 0 && Attackable == true; } }
    public int MP 
    {
        get { return mp; }
        set 
        {
            mp = value;
            mpTmp.text = "X" + mp.ToString();
        }
    }
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

    public int OriginHp { get { return hp; } set { hp = value; } } // 원본 체력
    public int OriginAtt { get { return att; } set { att = value; } } // 원본 공격력
    public int Att // 현재 공격력 [무기+원본 공격력]
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

    // 카메라의 피직스레이캐스터 필요, 객체에 Collider필요
    public void Awake()
    {
        audioPlayer = GetComponent<AudioSource>();
        weaponData = null;
        OriginPos = playerMask.transform.position;
        Col = GetComponent<Collider2D>();
        IsMine = (this.gameObject.name.Contains("Player")) ? true : false;
        gameObject.layer = LayerMask.NameToLayer((IsMine == true) ? "allyHero" : "foeHero");
        Attackable = true;
        att = 0; hp = 30;
        GAME.IGM.allIBody.Add(this);
        // 내 영웅만 필요한 클릭 이벤트들 
        if (IsMine == true)
        {
            // 감정표현 자연감소 시키는 코루틴, 게임 끝날떄까지 계속 실행 ( 감정표현 전송 남발사용시에, 잠시 사용을 잠그는 코루틴 실행)
            GAME.IGM.StartCoroutine(ReduceEmoCount());

            // 내 닉네임 표기
            nickTmp.text = GAME.Manager.NM.playerInfo.NickName;

            heroData = GAME.Manager.RM.GetHeroData(GAME.Manager.RM.GameDeck.ownerClass);
            heroData.Init(playerImg, skillImg, IsMine);
            // 영웅 이미지 초기화

            // 영웅 스킬란 초기화 및 이미지 적용
            heroSkill.InitSkill(IsMine);

            // 영웅 아이콘 좌클릭 => 무기공격
            GAME.Manager.UM.BindEvent(this.gameObject, HeroAttack, Define.Mouse.ClickL, Define.Sound.Ready);

            // 영웅 아이콘 우클릭 => 감정표현
            GAME.Manager.UM.BindEvent(this.gameObject, HeroSpeech, Define.Mouse.ClickR, Define.Sound.None);
            // 스킬 아이콘 클릭시, 타겟팅 이벤트 실행 연결
            GAME.Manager.UM.BindEvent(heroSkill.Col.gameObject, ClickedOnSkill, Define.Mouse.ClickL, Define.Sound.None);

            // 각각의 말풍선에 이벤트 연결
            for (int i = 0; i < Select.gameObject.transform.childCount; i++)
            {
                GAME.Manager.UM.BindEvent(Select.transform.GetChild(i).gameObject,
                    SelectedSpeech, Define.Mouse.ClickL, Define.Sound.None);
            }
            // 선택대사 나올떄, 배경클릭시 대사 즉각 종료
            GAME.Manager.UM.BindEvent(Reply.gameObject, OffReply, Mouse.ClickL, Sound.None);

        }
        else
        {
            Select.gameObject.SetActive(false);
            Reply.gameObject.SetActive(true);
            Speech.gameObject.SetActive(false);
        }
        
        // 카드팝업 이벤트 연결 (마우스 엔터로 구현)
        GAME.Manager.UM.BindCardPopupEvent(WpIcon, ShowWeaponIcon, 0.75f);
        GAME.Manager.UM.BindCardPopupEvent(skillIcon, ShowSkillIcon, 0.75f);
        // 무기 아이콘 초기화
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

    #region 감정표현 관련 코루틴
    public int EmoCount = 0; // 감정표현을 제한없이 사용하는건 문제있다 생각하여, 한계점을 만들고 사용하기
    IEnumerator EmoFlushCo;

    // 감정표현 남발시, 쌓인수만큼 랜덤한 초만큼 사용못하게 막기
    IEnumerator CalculateEmoCount()
    {
        // 시작할떄의 갯수 기억
        int currCount = EmoCount;
        // 랜덤한 초만큼 대기후 다시 감정표현 사용할수있도록 하기
        WaitForSeconds wait = new WaitForSeconds(EmoCount * UnityEngine.Random.Range(0.5f, 1.5f));
        for (int i = 0; i < currCount; i++)
        {
            yield return wait;
        }
        // 참조하였떤 코루틴을 비워서, 다시 감정표현 사용가능한지 조건문에 적용
        EmoFlushCo = null;
    }

    // 쌓이는 감정표현 카운트 수 자연감소 실행 코루틴
    IEnumerator ReduceEmoCount()
    {
        while (true)
        {
            yield return new WaitUntil(() => (EmoCount > 0));
            yield return new WaitForSeconds(5f);
            EmoCount--;
        }
    }
    #endregion

    #region EnterExit이벤트
    public void ShowWeaponIcon() // 무기아이콘에 커서를 일정시간 가져다 댈시
    {
        // 카드팝업 호출 + 보여질 데이터와 위치값 함께
        GAME.IGM.ShowCardPopup(ref weaponData, (IsMine == true) ? new Vector3(-3f, -1.1f, 0) : new Vector3(4f, 3.3f, 0));
    }
    public void ShowSkillIcon() // 영웅능력 아이콘에 커서를 일정시간 가져다 댈시
    {
        // 카드팝업 호출 + 보여질 데이터와 위치값 함께
        GAME.IGM.ShowHeroSkill((IsMine == true) ? new Vector3(4f, -1.3f, 0) : new Vector3(4f, 3.3f, 0), heroData);
    }
    #endregion

    #region 영웅 이벤트

    // 적의 영웅 능력 사용을 내 화면에서 동기화할떄, 강제 호출하여 사용
    public void CallHeroSkillAttack(IBody target)
    {
        // 사용으로 검게 만들어주기
        IEnumerator co = heroData.SkillCo(heroSkill, target);
        GAME.IGM.AddAction(EnemySkillUse(co));
        
        IEnumerator EnemySkillUse(IEnumerator co)
        {
            // 사용으로 검게 만들어주기
            heroSkill.Attackable = false;
            if (co != null)
            { yield return GAME.IGM.StartCoroutine(co); }
        }
    }
    // 영웅의 스킬 아이콘 클릭시, 스킬이벤트 시작
    public void ClickedOnSkill(GameObject go)
    {
        // 나의 턴에서만 가능한 상태
        if (GAME.IGM.Packet.isMyTurn == false) { return; }

        if (heroSkill.Attackable == false || GAME.IGM.TC.LR.gameObject.activeSelf == true)
        { 
            return;
        }

        // 레이 잠시끄기
        heroSkill.Attackable = false;
        // 영웅 능력이 선택타겟팅이라면 : 타겟이 성공할떄만 사용으로 간주
        if (heroData.skillTargeting == evtTargeting.Select)
        {
            // 타겟팅 카메라 실행 + 만약 타겟팅 성공시 공격함수 예약 실행
            GAME.IGM.TC.StartCoroutine(GAME.IGM.TC.TargettingHeroSkillCo(heroSkill));
        }
        // 영웅 능력이 자동실행건이라면 : 자동 실행 + 영능 잠그기
        else
        {
            // 영웅능력 잠그기 예약 실행
            GAME.IGM.AddAction (heroData.SkillCo(heroSkill, null)) ;
        }
    }

    // 영웅 좌클릭시 공격
    public void HeroAttack(GameObject go)
    {
        #region 예외상황들 확인
        // 나의 턴에서만 가능한 상태
        if (GAME.IGM.Packet.isMyTurn == false) { return; }

        // 만약 대화선택지 이벤트 선택 또는 실행중이었따면, 대화창 닫는 이벤트로 변경 실행
        if (Speech.gameObject.activeSelf == true)
        {
            Speech.gameObject.SetActive(false);
            return;
        }

        // 공격 가능한 상태가 아니거나
        // 다른 객체의 타겟팅 이벤트 실행중에 객체를 클릭시 취소
        if (GAME.IGM.TC.LR.gameObject.activeSelf == true || !CanAttack)
        { return; }

        // 공격력이 존재하지만, 공격 가능상태가 아니면, 이미 공격한것이므로 공격했다는 텍스트를 보여주기 실행
        if (Attackable == false && Att > 0 )
        {
            // 지금 대화창이 켜져있다면,
            // 유저 자신에게 난 이미 공격했다는 텍스트를 현재 재생중일거기에, 중복 실행 할필요없어 강제 취소
            if (Speech.gameObject.activeSelf == true) { return; }

            // 유저에게 자신의 영웅이 이미 공격했다는걸 알리는 대사창 활성화 실행
            HeroSaying(Define.Emotion.AlreadtHeroAttacked);
            return;
        }
        #endregion

        #region 예외 사항 모두 통과시 타겟팅 이벤트 실행 => 적절한 타겟을 클릭시 , 공격함수 예약 실행

        // 공격자 공격한것으로 변경 (이후 타겟팅 실패시, 아래 함수내부에서 다시 true로 변경예정 )
        Ray = Attackable = false;
        // 타겟팅 카메라 실행 + 만약 타겟팅 성공시 공격함수 예약 실행
        GAME.Manager.StartCoroutine(GAME.IGM.TC.MeeleTargettingCo
            (this,
            (IBody a, IBody t) => { return AttackCo(a, t); }
            ));

        #endregion
    }
    public IEnumerator AttackCo(IBody attacker, IBody target)
    {
        // 타겟이 현재 없다면 , 제자리 위치후 끄기
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
            // 공격권 다시 복구
            Attackable = true;
            yield break;
        }

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
        StartCoroutine(GAME.IGM.TC.ShakeCo());
        yield return null;

        #endregion

        #region 제자리로 복귀
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 1f;
            playerMask.transform.position = Vector3.Lerp(dest, OriginPos, t);
            yield return null;
        }
        ChangeSortingLayer(false); // 소팅레이어 초기화
        #endregion
        // 데미지 교환
        attacker.HP -= target.Att;
        target.HP -= attacker.Att;
        // 나의 턴이고, 내 영웅이 공격했다면
        // 현재 내가 조종한 행동으로 확인 및 공격 이벤트 상대에게 전파
        if (GAME.IGM.Packet.isMyTurn && attacker.IsMine)
        {
            GAME.IGM.Packet.SendHeroAttack(attacker.PunId, target.PunId);
        }

        // 내구도 감소 및 내구도 0 도달시 무기 부서지는 애니메이션 코루틴 실행
        weaponData.durability -= 1;
        durTmp.text = weaponData.durability.ToString();
        if (weaponData != null && weaponData.durability <= 0)
        {
            StartCoroutine(BrokenWeaponCo());
        }

        if (target.HP <= 0) { yield return StartCoroutine(target.onDead); }
        if (attacker.HP <= 0) { yield return StartCoroutine(attacker.onDead); }
    }

    // 영웅 우클릭시 대화 이벤트
    public void HeroSpeech(GameObject go)
    {
        // 감정표현이 쌓이면, EmoFlushCo가 실행되기에 , Null참조가 아니면 현재 감정표현을 남발한 상태로 강제 취소
        if (EmoFlushCo != null)
        { return;  }

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
        // 몇번쨰 선택패널 골랐는지 자식순서로 찾기
        int index = go.transform.GetSiblingIndex();

        // 0번은 배경 , 배경 클릭시 감정표현 이벤트 강제 종료
        if (index == 0)
        { Speech.gameObject.SetActive(false); }
        else
        {
            // Emotion이넘 인덱스 0부터 시작하기위해 -1한 값을 적용
            int emoIndex = index - 1;
            // 오디오 찾기
            AudioClip clip = GAME.Manager.RM.GetClip(heroData.classType, (Define.Emotion)emoIndex);
            Reply.textWaitTime = clip.length + 0.5f; // 대화창 재생시간 다시 초기화
            // 음성오디오 재생
            audioPlayer.Stop();
            audioPlayer.clip = clip;
            audioPlayer.Play();
            replyTmp.text = heroData.outSpeech[(Define.Emotion)(emoIndex)];
            // 내 감정표현 전달
            GAME.IGM.Packet.SendHeroEmotion(emoIndex);
            EmoCount++;
            Select.gameObject.SetActive(false);
            Reply.gameObject.SetActive(true);

        }


        // 감정표현이 5개 이상이나 쌓였따면, 잠시 제한걸기 (코루틴내에서 스스로 제한 풀예정)
        if (EmoCount > 5 && EmoFlushCo == null)
        {
            EmoFlushCo = CalculateEmoCount();
            GAME.IGM.StartCoroutine(EmoFlushCo); 
        }
        return;
    }

    // 적 영웅일떄는, 전달 받은 감정표현 대사 사용
    public void PlayEnemyEmotion(int idx)
    {
        audioPlayer.Stop();
        audioPlayer.clip = GAME.Manager.RM.GetClip(heroData.classType, (Define.Emotion)idx);
        audioPlayer.Play();

        // 현재 말풍선이 켜져있으면 대사재생시간 초기화 및 바뀐 대사로 진행
        if (Speech.gameObject.activeSelf == true)
        {
            // 텍스트 재생시간 초기화 및 새 텍스트로 변경하여 대사 진행
            Reply.textWaitTime = 2.75f;
            replyTmp.text = heroData.outSpeech[(Define.Emotion)idx];
        }
        else 
        {
            replyTmp.text = heroData.outSpeech[(Define.Emotion)idx];
            Speech.gameObject.SetActive(true);
        }
        
    }
    // 유저가 선택한 대사에 맞는 대화창을 한번더 클릭시 강제 종료
    public void OffReply(GameObject go)
    {
        Speech.gameObject.SetActive(false);
    }

    // 단순 감정표현 외, 특정상황에 재생해야할 대사가 있다면 상황별Enum으로 찾아서 재생하기
    public void HeroSaying(Define.Emotion e)
    {
        // 이미 대사를 진행중이었다면
        if (Speech.gameObject.activeSelf == true)
        {
            // 강제 종료후, 다시 세팅
            Speech.gameObject.SetActive(false);
        }

        // 말풍선 선택 이벤트시, 선택창만 켜져있도록 Defalut상태로 변경
        Select.gameObject.SetActive(false);
        replyTmp.text = heroData.outSpeech[e];
        Reply.gameObject.SetActive(true);
        Speech.gameObject.SetActive(true);
    }
    #endregion

    #region 무기 착용
    public IEnumerator EquipWeapon(CardHand card)
    {
        // 무기 데이터 초기화 및 공격 가능 설정
        weaponData = card.WC;
        wpImg.sprite = card.cardImage.sprite;
        Att += weaponData.att;
        durTmp.text = weaponData.durability.ToString();
        weaponAttTmp.text  = weaponData.att.ToString();
        // 무기 착용 애니메이션 시작
        yield return StartCoroutine(WearingCo(card));
    }
    public IEnumerator EquipWeapon(CardHand card ,CardData data)
    {
        // 적 영웅이 무기를 차고있다면, 팝업으로 내게 보여주기
        if (IsMine == false)
        {
            GAME.IGM.ShowEnemyWeaponPopup(card.WC);
        }
        

        // 무기 데이터 초기화 및 공격 가능 설정
        weaponData = (WeaponCardData)data;
        wpImg.sprite = GAME.Manager.RM.GetImage(data.cardClass,data.cardIdNum);
        Att += weaponData.att;
        durTmp.text = weaponData.durability.ToString();
        weaponAttTmp.text = weaponData.att.ToString();

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
    public IEnumerator BrokenWeaponCo()
    {
        float t = 0;
        Att -= weaponData.att;
        Vector3 start = WpIcon.transform.localScale;
        if (start != Vector3.zero)
        {
            // 무기가 점차 작아지면서 없어지는 코루틴 실행
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

    #region 마나상황과 영웅 인포창
    //public void 
    #endregion
}
