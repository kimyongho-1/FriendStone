using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using static Define;
using WebSocketSharp;

public class CardHand : CardEle, IBody
{
    AudioSource audioPlayer;
    public Vector3  originRot, originScale;
    public int originOrder;
    public TextMeshPro cardName, Description, Stat, Type, Cost;
    public SpriteRenderer cardBackGround, cardImage;
    public GameObject TMPgo;
    [SerializeField] int currAtt, currHp, currCost;

    #region IBODY
    public bool Attackable { get; set; }
    [field: SerializeField] public int PunId { get; set; }
    [field: SerializeField] public bool IsMine { get; set; }
    public Transform TR { get { return this.transform; } }
    public Define.ObjType objType { get; set; }
    [field: SerializeField] public Collider2D Col { get; set; }
    public bool Ray 
    {
        set 
        { 
            if (Col == null) 
            {
                Col = TR.GetComponent<Collider2D>(); 
            }
            Col.enabled = value; 
        } 
    }

    public Vector3 OriginPos { get; set; }
    public IEnumerator onDead { get; set; }

    [field : SerializeField] public int OriginAtt { get; set; }
    [field: SerializeField] public int OriginHp { get; set; }
    public int Att
    {
        get
        { return currAtt; }
        set
        {
            if (CardEleType == cardType.spell) { return; }
            currAtt = value;
            DrawVar(); // tmp 조정
        }
    }
    public int HP
    {
        get
        { return currHp; }
        set
        {
            if (CardEleType == cardType.spell) { return; }
            currHp = Mathf.Clamp(value,0, value) ;
            DrawVar(); // tmp 조정
        }
    }
    #endregion

    [field: SerializeField] public int OriginCost { get; set; }
    public int CurrCost
    {
        get
        { return currCost; }
        set
        {
            currCost = Mathf.Clamp(value, 0, value);
            Cost.text = currCost.ToString();
        }
    }

    // 손의 핸드 제자리 위치 + 잠그기 (드로우 & 상대턴일떄 사용불가)
    public void rewindHand() { StopAllCoroutines(); DragCo = null; Exit(null); }
    // OnHand이벤트가 존재시, 특정순간마다 이벤트를 실행 (이벤트 구독은 BattleManager.cs에서 등록)
    public Action<int,bool> HandCardChanged;
    public void DrawVar() // 벨류 변경뒤, tmp자동 변경
    {
        switch (CardEleType)
        {
            case Define.cardType.minion:
                Stat.text = $"<color=green>ATT {Att} <color=red>HP {HP} <color=black>몬스터";return;
            case Define.cardType.spell:
                Stat.text = "<color=black>주문";return;
            case Define.cardType.weapon:
                Stat.text = $"<color=green>ATT {Att} <color=red>dur {HP} <color=black>무기"; return;
        }
    }
    public void Awake()
    {
        originScale = 0.3f * Vector3.one;
        Col = GetComponent<BoxCollider2D>();
    }
    public void Init(CardData data, bool isMine = true)
    {
        audioPlayer = GetComponent<AudioSource>();
        base.Init(data);
        IsMine = isMine;
        cardBackGround.sprite = GAME.Manager.RM.GetCardSprite(IsMine);
        originScale = Vector3.one * 0.3f;
        transform.localScale = originScale;
        objType = ObjType.HandCard;
        
        // 나의 카드라면 모든 데이터 초기화
        if (IsMine)
        {
            cardName.text = data.cardName;
            Description.text = data.cardDescription;
            Description.fontSize = ( data.cardDescription == null || data.cardDescription.Length > 25) ? 18 : 15;
            // 카드 희귀도 표시
            switch (data.cardRarity)
            {
                case Define.cardRarity.rare:
                    Type.text = "<color=blue>희귀"; break;
                case Define.cardRarity.legend:
                    Type.text = "<color=red>전설"; break;
                default: Type.text = "<color=black>일반"; break;
            }
            // 카드 타입 표시
            switch (data.cardType)
            {
                case Define.cardType.minion:
                    Stat.text = $"<color=green>ATT {MC.att} <color=red>HP {MC.hp} <color=black>몬스터";
                    Att = OriginAtt = MC.att; HP = OriginHp = MC.hp;
                    break;
                case Define.cardType.spell:
                    Stat.text = "<color=black>주문";
                    break;
                case Define.cardType.weapon:
                    Stat.text = $"<color=green>ATT {WC.att} <color=red>dur {WC.durability} <color=black>무기";
                    Att = OriginAtt = WC.att; HP = OriginHp = WC.durability; 
                    break;
            }

            CurrCost = OriginCost = data.cost;
            cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);

            // 드래그 이벤트 연결
            GAME.Manager.UM.BindEvent(this.gameObject, StartDrag, Define.Mouse.StartDrag);
            GAME.Manager.UM.BindEvent(this.gameObject, Dragging, Define.Mouse.Dragging);
            GAME.Manager.UM.BindEvent(this.gameObject, EndDrag, Define.Mouse.EndDrag);
            // 커서 가져다 댈시 이벤트
            GAME.Manager.UM.BindEvent(this.gameObject, Enter, Define.Mouse.Enter);
            GAME.Manager.UM.BindEvent(this.gameObject, Exit, Define.Mouse.Exit);

        }

        // 상대 카드라면 꺼두기
        else
        {
            cardImage.gameObject.SetActive(false);
            TMPgo.gameObject.SetActive(false);  
        }

    }

    // TMP 소팅오더 정렬
    public void SetOrder(int i)
    {
        cardImage.sortingOrder = i * 10 - 1;
        cardBackGround.sortingOrder = i * 10;
        Description.sortingOrder =
        Stat.sortingOrder =
        Type.sortingOrder =
        Cost.sortingOrder=
        cardName.sortingOrder  = i * 10 + 1;
    }

    
    #region 마우스 이벤트
    public void Enter(GameObject go)
    {
        // 드래그 관련 이벤트가 발동될떄, DragCo에 실행코루틴을 참조 및 실행
        // DragCo가 Null이 아니면, 현재 드래그관련 이벤트중이기에 확대/취소 이벤트는 강제 생략
        if (DragCo != null) { return; }
        
        SetOrder(originOrder * 10);
        transform.localScale = originScale * 1.5f;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(transform.localPosition.x, -1.65f,-0.5f);
    }
    public void Exit(GameObject go)
    { 
        // 드래그 중일떄 취소
        if (DragCo != null) { return; }
        SetOrder(originOrder );
        transform.localScale = originScale;
        transform.localRotation = Quaternion.Euler(originRot);
        transform.localPosition = OriginPos;
    }
    public IEnumerator DragCo = null;
    public float xDegree, yDegree;
    public float rotVelX, rotVelY;
    float X, Y;
    public void StartDrag(Vector3 v)
    {
        // 비용문제로 사용 못하는 카드라면 바로 취소
        if (CurrCost > GAME.IGM.Hero.Player.MP) { return; }

        // 핸드카드 드래그시, 필드의 하수인과 겹치면 안되기에 잠시끄기
        GAME.IGM.Spawn.playerMinions.ForEach(x=>x.Ray = false);

        // 드래깅 시작 효과음 재생
        audioPlayer.clip =  GAME.IGM.GetClip(Define.IGMsound.Pick);
        audioPlayer.Play();

        // 주문카드이며 직접 타겟팅하는 경우, 카드를 움직이지않고 현재 핸드카드에서 타겟팅 화살표 실행
        if (CardEleType == cardType.spell && SC.evtDatas[0].targeting == evtTargeting.Select)
        {
            // 이벤트중에 선택 이벤트 찾기
            CardBaseEvtData selectEvt = SC.evtDatas[0];

            // 만약 직접 타겟팅을 하는 주문이지만
            // 대상이 적 미니언 뿐이고, 적 미니언이 없으면
            // 강제로 드로우 끝 함수를 호출하여 사용 강제취소
            string[] layers = GAME.IGM.Battle.FindLayer(selectEvt);
            if (layers.Length == 1 && layers[0] == "foe"
                && GAME.IGM.Spawn.enemyMinions.Count == 0)
            { EndDrag(default); return; }

            // 타겟팅 카메라 실행 + 만약 타겟팅 성공시 실행함수 예약 실행
            GAME.Manager.StartCoroutine(GAME.IGM.TC.TargettingCo
                (this,

                (IBody a, IBody t) =>
                {
                    return evt(a, t);
                },

                GAME.IGM.Battle.FindLayer(selectEvt) // 현재 선택 타겟 이벤트의 레이어 설정에 맞게 변경
               
                ));
            
            // 유저가 타겟을 고르면 해당 이벤트 먼저 실행후, 나머지 기타 이벤트 순서대로 실행
            IEnumerator evt(IBody a, IBody t)
            {
                //  evt 코루틴이 실행된 의미는 , 유저가 적합한 타겟팅을 선택하였기에 이벤트 실행 및 주문카드 사용으로 간주
                // 현재 핸드목록에서 사용할 주문 카드 제거
                GAME.IGM.Hand.PlayerHand.Remove(this);

                // 완전 삭제가 아닌 투명화만 진행
                GAME.IGM.AddAction(FadeOutCo(true, false));

                // 상대에게 내가 주문 카드를 사용한걸 알리기
                GAME.IGM.Packet.SendUseSpellCard(this.PunId, SC.cardIdNum);

                for (int i = 0; i < SC.evtDatas.Count; i++)
                {
                   GAME.IGM.AddAction(GAME.IGM.Battle.Evt(SC.evtDatas[i], this, t));
                    yield return null;
                }

                // 나머지 핸드 레이 풀어주기
                GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);

                // 상대에게 내 주문카드 사용 끝을 알리기
                GAME.IGM.AddAction(EndSpell());
                IEnumerator EndSpell()
                {
                    //핸드매니저에서 핸드카드들 재정렬 시작
                    yield return  GAME.IGM.Hand.CardAllignment(this.IsMine);
                    // 주문카드 사용 끝을 전달
                    GAME.IGM.Packet.SendEndingSpellCard(this.PunId);
                    // 주문카드 완전 사용하였기에 삭제 진행
                    GAME.IGM.Hand.AllCardHand.Remove(this);
                    GameObject.Destroy(this.gameObject);
                }
            }
        }

        // 그외 모든 카드들은 필드에 드래그를 하여 놓아야 사용하는것으로 판정
        else
        {
            // 드래그 시작 : 현재 드래깅 객체 크기,회전등 초기화
            DragCo = BackToOrigin();
            StartCoroutine(DragCo);

            // 드래그 시작시, 확대와 회전등 초기화
            IEnumerator BackToOrigin()
            {
                // SR의 오더 강조시키기 (제일 앞에 그려지도록)
                SetOrder(1000);
                // 현재 드래그중인 카드 제외, 모든 카드 레이끄기 (타 핸드카드의 엔터 엑시트 같은 이벤트 중복 방지)
                GAME.IGM.Hand.PlayerHand.FindAll(x => x.PunId != this.PunId).ForEach(x => x.Ray = false);
                // 크기등 모두 초기화
                float t = 0;
                Vector3 currScale = transform.localScale;
                Quaternion currRot = transform.localRotation;
                while (t < 1f)
                {
                    t += Time.deltaTime * 2.5F;
                    transform.localScale = Vector3.Lerp(currScale, originScale, t);
                    transform.localRotation = Quaternion.Lerp(currRot , Quaternion.identity,t);
                    yield return null;
                } 
                
                // 드래그동안 회전 0값으로 돌아가려 계속 시도 (움직임에 의한 실제 회전값 적용은 Dragging함수에서 )
                while (true)
                {
                    X = Mathf.SmoothDamp(X, 0, ref rotVelX, 0.3f);
                    Y = Mathf.SmoothDamp(Y, 0, ref rotVelY, 0.3f);

                    transform.rotation = Quaternion.Euler(new Vector3(X, Y, 0));
                    yield return null;
                }
            }

        }

    }
    public void Dragging(Vector3 worldPos)
    {
        if (DragCo == null) { return; }

        if (CardEleType == cardType.spell && SC.evtDatas[0].targeting == evtTargeting.Select) { return; }

        if (MC != null)
        {
            GAME.IGM.Spawn.MinionAlignment(this, worldPos);
        }

        Vector3 euler = transform.rotation.eulerAngles;
        // worldPos는 현재 마우스의 WP
        // dir :  이동 벡터
        Vector3 dir = worldPos - this.transform.position;

        // 현재 이동을 조금이라도 했는지 여부 확인
        if (dir.sqrMagnitude > Mathf.Epsilon)
        {
            // 만약 이동 확인시 이동방향으로 회전값도 적용 (제자리복귀(0도)는 StartDrag함수내부의 코루틴에서 진행중 )
            X += dir.y * 15f;
            X = Mathf.Clamp(X, -40f, 40f);
            Y += -dir.x * 15f;
            Y = Mathf.Clamp(Y, -40f, 40f);
        }

        this.transform.localPosition = worldPos;
    }

    // 카드 투명화로 소멸 코루틴 : 주로 카드 삭제 또는 미니언카드를 필드로 소환할떄 사용
    public IEnumerator FadeOutCo(bool isMine = true, bool bothDelete = true)
    {
        if (IsMine)
        {
            // 투명화 위해 모든 TMP와 SR을 묶기
            List<TextMeshPro> tmpList = new List<TextMeshPro>() { cardName, Description, Cost, Stat, Type };
            List<SpriteRenderer> imageList = new List<SpriteRenderer>() { cardImage, cardBackGround };
            float t = 1;
            Color tempColor = Color.white;
            while (t > 0f)
            {
                // 알파값 점차 0으로 변환
                t -= Time.deltaTime * 2.5f;
                tempColor.a = t;
                tmpList.ForEach(x => x.alpha = t);
                imageList.ForEach(x => x.color = tempColor);
                yield return null;
            }

        }

        // 적의 핸드카드 경우 백그라운드 이미지만 존재
        else
        {
            // 투명화 위해  백그라운드 이미지만 찾기
            float t = 1;
            Color tempColor = Color.white;
            while (t > 0f)
            {
                // 알파값 점차 0으로 변환
                t -= Time.deltaTime * 2.5f;
                tempColor.a = t;
                cardBackGround.color = tempColor;
                yield return null;
            }
        }

        // 삭제도 진행할건지
        if (bothDelete)
        {
            GAME.IGM.Hand.AllCardHand.Remove(this);
            GameObject.Destroy(this.gameObject);
        }
    }
    public void EndDrag(Vector3 v)
    {

        Ray = false; // Ray를 비활성화시 Exit가 호출되지만, DragCo를 뒤에서 null로 초기화하여서 Exit 먼저 실행을 막기
        StopAllCoroutines();
        // 핸드카드 드래그시, 필드의 하수인과 겹치면 안되기에 잠시끄기
        GAME.IGM.Spawn.playerMinions.ForEach(x => x.Ray = true);
        switch (CardEleType)
        {
            case Define.cardType.minion:
                if (GAME.IGM.Spawn.CheckInBox(
                 new Vector2(this.transform.localPosition.x, this.transform.localPosition.y)))
                {
                    // 현재 핸드목록에서 소환할 이 미니언카드 제거
                    GAME.IGM.Hand.PlayerHand.Remove(this);
                    // 미니언 소환 완료시, 모든 핸드카드 레이활성 초기화
                    GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);
                    // 미니언 스폰 시작 (카드 소멸화 애니메이션 및 삭제는 StartSpawn내부에서 실행)
                    GAME.IGM.Spawn.StartSpawn(this);
                    //핸드매니저에서 핸드카드 재정렬은 위 스타트스폰함수 마지막에 실행
                    return;
                }
                break;
            case Define.cardType.spell:
                if (SC.evtDatas[0].targeting != evtTargeting.Select
                     && GAME.IGM.Spawn.CheckInBox( new Vector2(this.transform.localPosition.x, this.transform.localPosition.y)))
                {
                    // 비용 감소 및 핸드리스트에서 제거
                    GAME.IGM.Hero.Player.MP -= this.currCost;
                    GAME.IGM.Hand.PlayerHand.Remove(this);

                    // 상대에게 내가 주문 카드를 사용한걸 알리기
                    GAME.IGM.Packet.SendUseSpellCard(this.PunId, SC.cardIdNum);
                    // 이벤트 실행
                    for (int i = 0; i < SC.evtDatas.Count; i++)
                    {
                        GAME.IGM.AddAction(GAME.IGM.Battle.Evt(SC.evtDatas[i], this)) ;
                    }

                    GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);

                    // 상대에게 내 주문카드 사용 끝을 알리기
                    GAME.IGM.AddAction(EndSpell());
                    IEnumerator EndSpell()
                    {
                        yield return null;
                        GAME.IGM.Packet.SendEndingSpellCard(this.PunId);
                        //핸드매니저에서 핸드카드들 재정렬 시작
                        GAME.IGM.AddAction(GAME.IGM.Hand.CardAllignment(this.IsMine));
                        // 주문카드 사용 완료시, 모든 핸드카드 레이활성 초기화
                        GAME.IGM.AddAction(FadeOutCo(IsMine));
                    }


                    return;
                }
                else
                { GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true); break; }
            case Define.cardType.weapon:
                if (GAME.IGM.Spawn.CheckInBox(
                 new Vector2(this.transform.localPosition.x, this.transform.localPosition.y)))
                {
                    // 비용만큼 마나 감소
                    GAME.IGM.Hero.Player.MP -= CurrCost;

                    // 현재 핸드목록에서 카드 제거
                    GAME.IGM.Hand.PlayerHand.Remove(this);
                    // a무기 착용, 모든 핸드카드 레이활성 초기화
                    GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);

                    //핸드매니저에서 핸드카드들 재정렬 시작
                    GAME.IGM.AddAction(GAME.IGM.Hand.CardAllignment(this.IsMine));

                    // 상대에게 내 무기 착용 이벤트 전파
                    GAME.IGM.Packet.SendWeapon(PunId, WC.cardIdNum, this.transform.position.x
                        , this.transform.position.y, this.transform.position.z);

                    // 영웅 무기 착용 시작 ( 카드 소멸 애니메이션 코루틴은 함수 내부에서 실행)
                    GAME.IGM.AddAction(GAME.IGM.Hero.Player.EquipWeapon(this));
                    return;
                }
                break;
        }
        GAME.IGM.Spawn.idx = -1000;
        // 원위치 코루틴 실행
        DragCo = BackToOrigin();
        StartCoroutine(DragCo);
        IEnumerator BackToOrigin()
        {
            // 드래깅 끝 효과음 재생
            audioPlayer.clip = GAME.IGM.GetClip(Define.IGMsound.Cancel); 
            yield return null;
            audioPlayer.Play();
            // SR의 오더 초기화 원래대로
            SetOrder(originOrder);
            // 크기등 모두 초기화
            float t = 0;
            Vector3 currScale = transform.localScale;
            Quaternion currRot = transform.localRotation;
            Quaternion originEuler = Quaternion.Euler(originRot);
            currRot.z = 0;
            Vector3 currPos = transform.localPosition;
            while (t < 1f)
            {
                t += Time.deltaTime * 5f;
                transform.localScale = Vector3.Lerp(currScale, originScale, t);
                transform.localRotation = Quaternion.Lerp(currRot, originEuler, t);
                transform.localPosition = Vector3.Lerp(currPos, OriginPos, t);
                yield return null;
            }
            GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);

            DragCo = null;
        }
    }
    #endregion
}
