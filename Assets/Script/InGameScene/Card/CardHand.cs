using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using static Define;

public class CardHand : CardEle, IBody
{
    public Vector3  originRot, originScale;
    public int originOrder, originCost;
    public TextMeshPro cardName, Description, Stat, Type, Cost;
    public SpriteRenderer cardBackGround, cardImage;
    public GameObject TMPgo;
    public SpellCardData spellCardData { get; set; }

    #region IBODY
    [field: SerializeField] public int PunId { get; set; }
    [field: SerializeField] public bool IsMine { get; set; }
    public Transform TR { get { return this.transform; } }
    public Define.ObjType objType { get; set; }
    [field: SerializeField] public Collider2D Col { get; set; }
    public bool Ray { set { if (Col == null) { Col = TR.GetComponent<Collider2D>(); } Col.enabled = value; } }

    public Vector3 OriginPos { get; set; }
    public IEnumerator onDead { get; set; }

    [field : SerializeField] public int OriginAtt { get; set; }
    [field: SerializeField] public int OriginHp { get; set; }

    public int Att
    {
        get
        {
            if (data.cardType == cardType.spell) { return 0; }
            else if (data.cardType == cardType.minion)
            {
                MinionCardData mc = (MinionCardData)data;
                return mc.att;
            }
            else
            {
                WeaponCardData wc = (WeaponCardData)data;
                return wc.att;
            }
        }
        set
        {
            if (data.cardType == cardType.spell) { return; ; }
            else if (data.cardType == cardType.minion)
            {
                MinionCardData mc = (MinionCardData)data;
                mc.att = value;
            }
            else
            {
                WeaponCardData wc = (WeaponCardData)data;
                wc.att = value;
            }
        }
    }

    public int HP
    {
        get
        {
            if (data.cardType == cardType.spell) { return 0; }
            else if (data.cardType == cardType.minion)
            {
                MinionCardData mc = (MinionCardData)data;
                return mc.hp;
            }
            else
            {
                WeaponCardData wc = (WeaponCardData)data;
                return wc.durability;
            }
        }
        set
        {
            if (data.cardType == cardType.spell) { return; ; }
            else if (data.cardType == cardType.minion)
            {
                MinionCardData mc = (MinionCardData)data;
                mc.hp = value;
            }
            else
            {
                WeaponCardData wc = (WeaponCardData)data;
                wc.durability = value;
            }
        }
    }

    #endregion

    // OnHand이벤트가 존재시, 특정순간마다 이벤트를 실행 (이벤트 구독은 BattleManager.cs에서 등록)
    public Action<int,bool> HandCardChanged;

    public void Awake()
    {
        originScale = 0.3f * Vector3.one;
        Col = GetComponent<BoxCollider2D>();
    }
    public void Init(CardData dataParam , bool isMine = true)
    {
        IsMine = isMine;
        cardBackGround.sprite = GAME.Manager.RM.GetCardSprite(IsMine);
        originScale = Vector3.one * 0.3f;
        transform.localScale = originScale;

        // 나의 카드라면 모든 데이터 초기화
        if (IsMine)
        {
            data = dataParam;
            cardName.text = data.cardName;
            Description.text = data.cardDescription;
            Description.fontSize = (data.cardDescription.Length > 25) ? 18 : 15;
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
                    MinionCardData cardData = data as MinionCardData;
                    Stat.text = $"<color=green>ATT {cardData.att} <color=red>HP {cardData.hp} <color=black>몬스터";
                    Att = OriginAtt = cardData.att; HP = OriginHp = cardData.hp;
                    break;
                case Define.cardType.spell:
                    Stat.text = "<color=black>주문";
                    break;
                case Define.cardType.weapon:
                    WeaponCardData wData = (WeaponCardData)data;
                    Stat.text = $"<color=green>ATT {wData.att} <color=red>dur {wData.durability} <color=black>무기";
                    Att = OriginAtt = wData.att; HP = OriginHp = wData.durability; 
                    break;
            }
            originCost = data.cost;
            Cost.text = originCost.ToString();
            cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);

            // 드래그 이벤트 연결
            GAME.Manager.UM.BindEvent(this.gameObject, StartDrag, Define.Mouse.StartDrag, Define.Sound.Pick);
            GAME.Manager.UM.BindEvent(this.gameObject, Dragging, Define.Mouse.Dragging);
            GAME.Manager.UM.BindEvent(this.gameObject, EndDrag, Define.Mouse.EndDrag);
            // 커서 가져다 댈시 이벤트
            GAME.Manager.UM.BindEvent(this.gameObject, Enter, Define.Mouse.Enter, Define.Sound.None);
            GAME.Manager.UM.BindEvent(this.gameObject, Exit, Define.Mouse.Exit, Define.Sound.None);

            // 주문카드라면 그에 맞게 바인딩 ( 미니언카드와 무기카드는 실제로 필드에소환 하거나 착용시 데이타 캐스팅)
            if (dataParam is SpellCardData)
            {
                spellCardData = (SpellCardData)dataParam;
                objType = Define.ObjType.HandCard;
            }
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
        // 드래그 중일떄 취소
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

    IEnumerator DragCo = null;
    public void StartDrag(Vector3 v)
    {
        // 주문카드이며 직접 타겟팅하는 경우, 카드를 움직이지않고 현재 핸드카드에서 타겟팅 화살표 실행
        if (spellCardData != null &&
            spellCardData.evtDatas.Find(x => x.targeting == Define.evtTargeting.Select) != null)
        {
            // 이벤트중에 선택 이벤트 찾기
            CardBaseEvtData selectEvt = spellCardData.evtDatas.Find(x => x.targeting == Define.evtTargeting.Select);

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

                // 상대에게 내가 주문 카드를 사용한걸 알리기
                GAME.IGM.Packet.SendUseSpellCard(this.PunId, spellCardData.cardIdNum);

                // 유저 선택건 , 자동 순서로 변경 [true,true,false,false]
                data.evtDatas =  data.evtDatas.OrderByDescending(x => x.targeting == Define.evtTargeting.Select).ToList();
              
                for (int i = 0; i < data.evtDatas.Count; i++)
                {
                    GAME.IGM.Battle.Evt(data.evtDatas[i], this, t);
                    yield return null;
                }

                // 나머지 핸드 레이 풀어주기
                GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);

                //핸드매니저에서 핸드카드들 재정렬 시작
                GAME.IGM.AddAction(GAME.IGM.Hand.CardAllignment(this.IsMine));

                // 상대에게 내 주문카드 사용 끝을 알리기
                GAME.IGM.AddAction(EndSpell());
                IEnumerator EndSpell()
                {
                    // 주문카드 사용 완료시, 모든 핸드카드 레이활성 초기화
                    GAME.IGM.StartCoroutine(FadeOutCo(IsMine));
                    GAME.IGM.Packet.SendEndingSpellCard(this.PunId);
                    yield return null;

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
                Vector3 currRot = transform.localRotation.eulerAngles;
                while (t < 1f)
                {
                    t += Time.deltaTime * 2.5F;
                    transform.localScale = Vector3.Lerp(currScale, originScale, t);
                    transform.localRotation = Quaternion.Euler(Vector3.Lerp(currRot, Vector3.zero, t));
                    yield return null;
                }
            }

            // 드래그동안에 상하좌우등의 이동에 맞게 회전 코루틴 실행
            StartCoroutine(Rotate());
            IEnumerator Rotate()
            {
                while (true)
                {
                    // 현재 회전값이 0.1 이하일시, 강제로 1로 벨류를 고정
                    float angle = Quaternion.Angle(transform.localRotation, Quaternion.identity);
                    float val = (angle < 0.1f) ? 1f : 0.05f;
                    transform.localRotation
                    = Quaternion.Lerp(transform.localRotation, Quaternion.identity, val);
                    yield return null;
                }
            }
        }

    }
    public void Dragging(Vector3 worldPos)
    {
        if (spellCardData != null && spellCardData.evtDatas.Find(x => x.targeting == Define.evtTargeting.Select) != null) { return; }
        // 미니언 카드들만 정렬 실행
        if (data.cardType == Define.cardType.minion)
        {
            GAME.IGM.Spawn.MinionAlignment(this, worldPos);
        }

        Vector3 euler = transform.rotation.eulerAngles;
        if (euler.y > 180)
        { euler.y -= 360f; }
        if (euler.x > 180)
        { euler.x -= 360f; }

        if ((transform.position.x - worldPos.x) > 0.01f)
        { euler.y = Mathf.Clamp(euler.y + 7f, -45f, 45f); }
        else if ((transform.position.x - worldPos.x) < -0.01f)
        { euler.y = Mathf.Clamp(euler.y - 7f, -45f, 45f); }
        
        if ((transform.position.y - worldPos.y) > 0.01f)
        { euler.x = Mathf.Clamp(euler.x - 4f, -25f, 25f); }
        else if ((transform.position.y - worldPos.y) < -0.01f)
        { euler.x = Mathf.Clamp(euler.x + 4f, -25f, 25f); }

        this.transform.localRotation = Quaternion.Euler(euler.x, euler.y, euler.z);
        this.transform.localPosition = worldPos;
    }

    // 카드 투명화로 소멸 코루틴 : 주로 카드 삭제 또는 미니언카드를 필드로 소환할떄 사용
    public IEnumerator FadeOutCo(bool isMine = true)
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
        GameObject.Destroy(this.gameObject);
    }
    public void EndDrag(Vector3 v)
    {
        Ray = false; // Ray를 비활성화시 Exit가 호출되지만, DragCo를 뒤에서 null로 초기화하여서 Exit 먼저 실행을 막기
        StopAllCoroutines();
     
        switch (data.cardType)
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
                    //핸드매니저에서 핸드카드들 재정렬 시작
                    GAME.IGM.AddAction(GAME.IGM.Hand.CardAllignment(this.IsMine));
                    return;
                }
                break;
            case Define.cardType.spell:
                if (GAME.IGM.Spawn.CheckInBox(
                 new Vector2(this.transform.localPosition.x, this.transform.localPosition.y))
                    && spellCardData.evtDatas.Find(x=>x.targeting == Define.evtTargeting.Select) == null)
                {
                    GAME.IGM.Hand.PlayerHand.Remove(this);

                    // 상대에게 내가 주문 카드를 사용한걸 알리기
                    GAME.IGM.Packet.SendUseSpellCard(this.PunId, spellCardData.cardIdNum);
                    // 이벤트 실행
                    for (int i = 0; i < data.evtDatas.Count; i++)
                    {
                        GAME.IGM.Battle.Evt(data.evtDatas[i], this);
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
                    // 현재 핸드목록에서 카드 제거
                    GAME.IGM.Hand.PlayerHand.Remove(this);
                    // a무기 착용, 모든 핸드카드 레이활성 초기화
                    GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);
                    // 영웅 무기 착용 시작 ( 카드 소멸 애니메이션 코루틴은 함수 내부에서 실행)
                    GAME.IGM.AddAction(GAME.IGM.Hero.Player.EquipWeapon(this));

                    // 상대에게 내 무기 착용 이벤트 전파
                    GAME.IGM.Packet.SendWeapon(PunId, data.cardIdNum, this.transform.position.x
                        , this.transform.position.y, this.transform.position.z);

                    //핸드매니저에서 핸드카드들 재정렬 시작
                    GAME.IGM.AddAction(GAME.IGM.Hand.CardAllignment(this.IsMine));

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
            // SR의 오더 초기화 원래대로
            SetOrder(originOrder);
            // 크기등 모두 초기화
            float t = 0;
            Vector3 currScale = transform.localScale;
            Vector3 currRot = transform.localRotation.eulerAngles;
            currRot.z = 0;
            Vector3 currPos = transform.localPosition;
            while (t < 1f)
            {
                t += Time.deltaTime * 5f;
                transform.localScale = Vector3.Lerp(currScale, originScale, t);
                transform.localRotation = Quaternion.Euler(Vector3.Lerp(currRot, originRot, t));
                transform.localPosition = Vector3.Lerp(currPos, OriginPos, t);
                yield return null;
            }
            GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);

            DragCo = null;
        }
    }
    #endregion
}
