using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static Define;
using System.Linq;
using System;
using Unity.VisualScripting;

public class CardField : CardEle,IBody
{
    public TextMeshPro AttTmp, HpTmp;
    public SpriteRenderer cardImage,attIcon, hpIcon, tauntIcon;
    public ParticleSystem sleep;
    public SpriteMask mask;
    [SerializeField] int currAtt, currHp;
    public bool waitDeathRattleEnd = false;
    #region IBODY
    [field: SerializeField] public bool Attackable { get; set; }
    [field: SerializeField] public int PunId { get; set; }
    [field: SerializeField] public bool IsMine { get; set; }
    public Transform TR { get { return this.transform; } }

    [field: SerializeField] public Collider2D Col { get; set; }
    public bool Ray { set { if (Col == null) { Col = TR.GetComponent<Collider2D>(); } Col.enabled = value; } }

    [field: SerializeField] public Vector3 OriginPos { get; set; }
    public IEnumerator onDead { get; set; }
    public Define.ObjType objType { get; set; }
    [field : SerializeField] public int OriginAtt { get; set; }
    [field: SerializeField] public int OriginHp { get; set; }

    public int Att
    {
        get
        { return currAtt; }
        set
        {
            currAtt = value;
            AttTmp.text = currAtt.ToString();
        }
    }

    public int HP
    {
        get
        { return currHp; }
        set
        {
            currHp = value;
            HpTmp.text = currHp.ToString();
        }
    }
    #endregion

    // 미니언 카드가 공격을 한다 가정시, 부딪힐떄 위치가 겹치는 순간
    // 소팅 레이어가 동일하면 이미지가 겹치거나 꺠질 위험이 있어, 공격자가 최상단에 위치하도록 레이어 변경
    public void ChangeSortingLayer(bool isOn)
    {
        SortingLayer[] layers = SortingLayer.layers;
        SortingLayer layer = Array.Find(layers, x => x.name == ((isOn) ? "Attacker" : "None"));
        
        mask.frontSortingLayerID = layer.id;
        tauntIcon.sortingLayerID = cardImage.sortingLayerID = 
        attIcon.sortingLayerID = hpIcon.sortingLayerID = layer.id;
        AttTmp.sortingLayerID = HpTmp.sortingLayerID = layer.id;
        
    }

    // 소유여부로 자신과 자식 모두 레이어 통일 (포스트 프로세싱 작업위해)
    public void SetLayerRecursive(int layers)
    {
        this.gameObject.layer =
        mask.gameObject.layer =
        cardImage.gameObject.layer =
        attIcon.gameObject.layer =
        AttTmp.gameObject.layer =
        hpIcon.gameObject.layer =
        HpTmp.gameObject.layer = 
        tauntIcon.gameObject.layer =layers;
    }
    public void Init(CardData datapar, bool isMine)
    {
        base.Init(datapar);
        // 소환시, 이벤트 타겟에 포함안되도록 체력을 0으로 설정후, 밑 InvokeEvt에서 다시 원래의 체력으로 복구
        HP = 0;
        HpTmp.text = MC.hp.ToString();
        // 손에 있을때 실행해야할 이벤트들 제거
        MC.evtDatas.RemoveAll(x=>x.when == evtWhen.onHand);

        Col = GetComponent<CircleCollider2D>();
        IsMine = isMine;
        // 죽었을시 이벤트 재생
        onDead = Dead(IsMine);
        objType = ObjType.Minion;
        SetLayerRecursive(LayerMask.NameToLayer((IsMine == true) ? "ally" : "foe"));
        cardImage.sprite = GAME.Manager.RM.GetImage(MC.cardClass, MC.cardIdNum);

        // 커서를 가져다 댈시 나올 팝업 이벤트 연결
        GAME.Manager.UM.BindCardPopupEvent(this.gameObject, CallPopup, 0.75f);
        
        Attackable = (MC.isCharge) ? true : false;
        sleep.gameObject.SetActive((MC.isCharge) ? false : true);
        tauntIcon.gameObject.SetActive(MC.isTaunt);
        Att = OriginAtt = MC.att;
        if (IsMine)
        {
            // 좌클릭시 공격 타겟팅 이벤트 시작을 연결
            GAME.Manager.UM.BindEvent(this.gameObject, StartAttack, Define.Mouse.ClickL, Define.Sound.Ready);
        }
        IEnumerator Dead(bool isMine)
        {
            #region 죽는 하수인이 죽을떄 실행할 이벤트 있는지 확인 및 실행
            // 만약 죽는 이 하수인에게 죽을때 실행할 이벤트가 있다면
            if (MC != null && MC.evtDatas.Find(x => x.when == evtWhen.onDead) != null)
            {
                // 현재 나의 턴이고 미니언이 죽었으며 죽을떄 실행할 이벤트가 있다면
                if (GAME.IGM.Packet.isMyTurn == true)
                {
                  //  GAME.IGM.Hand.PlayerHand.ForEach(x=>x.Ray =false);
                  //  GAME.IGM.Turn.Col .enabled= false;
                    // 죽을떄 실행할 이벤트를 모두 찾아
                    // 예약이 아닌 이 자리에서 모두 실행
                    List<CardBaseEvtData> list =
                        MC.evtDatas.FindAll(x => x.when == evtWhen.onDead);
                    for (int i = 0; i < list.Count; i++)
                    {
                        IEnumerator co = GAME.IGM.Battle.Evt(list[i], this);
                        if (co == null) { continue; }
                        yield return GAME.IGM.StartCoroutine(co);
                    }

                    // 죽을떄 실행할 이벤트가 모두 끝났음을 전파 (상대 입장에선 몬스터가 죽으면서 이벤트들이 모두 실행된후 몬스터가 소멸되야하기에)
                    GAME.IGM.Packet.SendDeathRattleEnd(this.PunId);
                   
                     Debug.Log("죽메 전송 끝");
                     //GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);
                     //GAME.IGM.Turn.Col.enabled = true;
                }

                // 나의 턴이 아닐시, 상대방이 보내는 이벤트를 계속 받아 실행하며 끝나는 신호를 받을떄까지 대기
                else
                {
                    // 위의 if문은 현재 공격자가 자신의 턴에 미니언이 죽을시,
                    // 해당 미니언이 죽을떄 이벤트를 순차적으로 실행후
                    // 마지막으로 모든 죽음 이벤트가 끝난 신호를 전파 => " GAME.IGM.Packet.SendDeathRattleEnd(this.PunId) "
                    // 위 이벤트를 전파 받은 타클라이언트는 waitDeathRattleEnd를 true로 변경하여 동기화를 진행
                    yield return new WaitUntil(() => (this.waitDeathRattleEnd == true));
                    Debug.Log("죽메 받기 성공");
                }
            }
            #endregion

            #region 코루틴 애니메이션 (점점 투명화 + 흔들기)
            // 투명도를 증가시켜 점차 사라지는 코루틴 애니메이션
            StartCoroutine(FadeOut());
            IEnumerator FadeOut()
            {
                float t = 1;
                while (t > 0f)
                {
                    // ����ȭ ����
                    t -= Time.deltaTime;
                    Color tempColor = new Color(1, 1, 1, t);
                    tauntIcon.color = cardImage.color = attIcon.color = hpIcon.color = tempColor;
                    AttTmp.alpha = HpTmp.alpha = t;
                    yield return null;
                }
                mask.enabled = false;
            }

            // 좌우로 죽는 코루틴 애니메이션
            yield return StartCoroutine(Wiggle());
            IEnumerator Wiggle()
            {
                float t = 0;
                float min = this.transform.position.x - 0.25f;
                float max = this.transform.position.x + 0.25f;
                while (t < 1f)
                {
                    t += Time.deltaTime;
                    float x = Mathf.Lerp(min, max, MathF.Sin(t * MathF.PI));
                    transform.position = new Vector3(x, transform.position.y, transform.position.z);
                    yield return null;
                }
            }
            #endregion

            // 소유여부로 게임내에서 완전제거
            if (isMine)
            {
                GAME.IGM.Spawn.playerMinions.Remove(this);
                // 핸드 관련된 이벤트를 실행해야하는 카드가 있는지 확인후 실행
                GAME.IGM.Hand.AllCardHand.FindAll(x => x.HandCardChanged != null).ForEach(x => x.HandCardChanged.Invoke(PunId, IsMine));

                yield return StartCoroutine(GAME.IGM.Spawn.CalcSpawnPoint());
            }
            // �� �ϼ��ε� �� ����
            else
            {
                GAME.IGM.Spawn.enemyMinions.Remove(this);
                // 핸드 관련된 이벤트를 실행해야하는 카드가 있는지 확인후 실행
                GAME.IGM.Hand.AllCardHand.FindAll(x => x.HandCardChanged != null).ForEach(x => x.HandCardChanged.Invoke(PunId, IsMine));
                yield return StartCoroutine(GAME.IGM.Spawn.AllEnemiesAlignment());
            }
            Debug.Log("삭제도 실행");
            Destroy(this.gameObject);
        }
    }
    public IEnumerator InvokeEvt()
    {
        HP = OriginHp = MC.hp;
        #region 이벤트 보유 여부 확인 
        // 먼저 유저가 직접 타겟팅하는 이벤트가 존재하는지 확인
        List<CardBaseEvtData> evtList = MC.evtDatas.FindAll(x => x.when == evtWhen.onPlayed);

        // 이벤트 자체가 없으면 취소
        if (evtList.Count == 0) { yield break; }
        #endregion

        #region 직접 타겟팅하는 이벤트 존재 여부 확인 및 나눠 실행

        // 존재하는 이벤트중, 유저가 직접 선택하는 이벤트포함 확인
        CardBaseEvtData selectEvt = evtList.Find(x => x.targeting == evtTargeting.Select);

        // 선택 이벤트 없으면 순서대로 자동실행
        if (selectEvt == null)
        {
            PlayEvt();
            yield break;
        }

        // 유저가 직접 선택하는 이벤트 존재 확인시
        // 적 미니언만이 타겟대상이지만, 적 미니언이 없는 상황 확인
        string[] layers = GAME.IGM.Battle.FindLayer(selectEvt);
        if (layers.Length == 1 && layers[0] == "foe"
            && GAME.IGM.Spawn.enemyMinions.Count == 0)
        {
            // 그러면 선택 이벤트 삭제후, 나머지 기존처럼 순서대로 자동실행
            MC.evtDatas.Remove(selectEvt);
            PlayEvt();
        }

        // 유저가 직접 찾는 타겟팅 방식이 있다면
        else
        {
            // 타겟팅 카메라 실행 + 만약 타겟팅 성공시 공격함수 예약 실행
            GAME.Manager.StartCoroutine(GAME.IGM.TC.TargettingCo
                (this,

                (IBody a, IBody t) =>
                {
                    return evt(a, t);
                },

                layers // 현재 선택 타겟 이벤트의 레이어 설정에 맞게 변경

                ));

            // 유저가 타겟을 고르면 해당 이벤트 먼저 실행후, 나머지 기타 이벤트 순서대로 실행
            IEnumerator evt(IBody a, IBody t)
            {
                List<CardBaseEvtData> list =
                MC.evtDatas.FindAll(x => x.when == evtWhen.onPlayed).OrderByDescending(x => x.targeting == evtTargeting.Select).ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    GAME.IGM.AddAction(GAME.IGM.Battle.Evt(list[i], this, t));
                    yield return null;
                }
                GAME.IGM.AddAction(DeleteOnPlayedEvts());
                IEnumerator DeleteOnPlayedEvts()
                {
                    yield return null;
                    // 등장시 실행할 이벤트들은 모두 제거
                    MC.evtDatas.RemoveAll(x => x.when == evtWhen.onPlayed);
                }
            }
        }

        #endregion

        // 타겟팅없는 이벤트뿐이면 순서대로 이벤트 재생
        void PlayEvt()
        {
            List<CardBaseEvtData> list =
                MC.evtDatas.FindAll(x => x.when == evtWhen.onPlayed);
            for (int i = 0; i < list.Count; i++)
            {
                GAME.IGM.AddAction(GAME.IGM.Battle.Evt(list[i], this));
            }
            GAME.IGM.AddAction(DeleteOnPlayedEvts());
            IEnumerator DeleteOnPlayedEvts()
            {
                yield return null;
                // 등장시 실행할 이벤트들은 모두 제거
                MC.evtDatas.RemoveAll(x => x.when == evtWhen.onPlayed);
            }
        }
    }


    // 미니언 카드의 경우, 일정초 동안 커서를 가져다 댈시 카드의 정보를 보여주는 카드팝업 호출 이벤트
    public void CallPopup()
    {
        if (MC == null)
        {
            Debug.Log("ERROR : 왜 데이터가 미니언타입이 아닌지");
        }

        // 몇번쨰 인지 인덱스 찾기
        int idx = (this.IsMine) ? GAME.IGM.Spawn.playerMinions.IndexOf(this)
            : GAME.IGM.Spawn.enemyMinions.IndexOf(this);

        // 우측으로 너무 밀린 미니언의 경우 왼쪽으로 카드팝업을 띄어주기
        Vector3 pos = transform.position + Vector3.right * 2f;
        if (this.transform.position.x > 3f)
        { pos = transform.position - Vector3.right * 2f; }

        // 보여질 데이터와 위치 구해지면, 카드팝업 띄우기
        GAME.IGM.ShowMinionPopup(MC, pos, cardImage.sprite);
    }

    // 미니언 공격시도 함수 (좌클릭 마우스 이벤트)
    public void StartAttack(GameObject go)
    {
        #region 예외사항 (공격이 정말 가능한지 확인)
        // 나의 턴에서만 가능한 상태
        if (GAME.IGM.Packet.isMyTurn == false) { return; }

        // 현재 타겟팅 카메라가 켜져있으면 현재 공격준비중인 미니언이 있기에
        // 다른 모든 미니언들의 클릭이벤트 무시
        if (GAME.IGM.TC.LR.gameObject.activeSelf == true)
        { return; }

        // 수명상태시, 현재 소환한 하수인 : 공격불가 판정
        if (sleep.gameObject.activeSelf == true)
        {
            // 내 영웅 대사 시작, 공격불가라고 플레이어에게 알리기 (이벤트 전파할 필요는 X )
            GAME.IGM.Hero.Player.HeroSaying(Define.Emotion.NotReady);
            // 그후 공격 취소
            return;
        }

        // 이미 공격을 한 상태라면 : 공격불가 판정
        if (Attackable == false)
        {
            // 해당 하수인이 이미 공격해서 , 공격 불가능하다고 말하기 (이벤트 전파할 필요는 X )
            GAME.IGM.Hero.Player.HeroSaying(Define.Emotion.AlreadyAttacked);
            // 공격 실행 취소
            return;
        }
        #endregion

        // 공격자 자신과, 스폰영역 레이 비활성화
        Ray = false;
        Attackable = false;

        // 타겟팅 카메라 실행 + 만약 타겟팅 성공시 공격함수 예약 실행
        GAME.Manager.StartCoroutine(GAME.IGM.TC.MeeleTargettingCo
            (this,
            (IBody a, IBody t) => { return AttackCo(a, t); }
            ));
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
            yield break;
        }

        GAME.IGM.Hand.PlayerHand.ForEach(x=>x.Ray =false);

        #region 공격 코루틴 : 상대에게 박치기
        ChangeSortingLayer(true); // 공격자 소팅레이어로 옮겨 최상단에 위치하기
        float t = 0;
        Vector3 start = attacker.Pos;
        Vector3 dest = target.Pos;
        while (t < 1f)
        {
            t += Time.deltaTime * 1f;
            this.transform.position = Vector3.Lerp(start, dest, t);
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
        t = 0 ;
        while (t < 1f)
        {
            t += Time.deltaTime * 1f;
            this.transform.localPosition = Vector3.Lerp( dest , OriginPos, t);
            yield return null;
        }
        ChangeSortingLayer(false); // 소팅레이어 초기화
        #endregion

        // 데미지 교환
        attacker.HP -= target.Att;
        target.HP -= attacker.Att;
        // 나의 턴이고, 나의 소유 미니언이 공격했다면
        // 현재 내가 조종한 행동으로 확인 및 공격 이벤트 상대에게 전파
        if (GAME.IGM.Packet.isMyTurn)
        {
            GAME.IGM.Packet.SendMinionAttack(attacker.PunId, target.PunId);
        }
        this.Attackable = false;

        if (attacker.HP <= 0) { yield return GAME.IGM.StartCoroutine(attacker.onDead); }
        if (target.HP <= 0) { yield return GAME.IGM.StartCoroutine(target.onDead); }
        GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);
    }

}