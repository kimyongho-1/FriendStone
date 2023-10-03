﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static Define;
using System.Linq;
using System;

public class CardField : CardEle,IBody
{
    public TextMeshPro AttTmp, HpTmp;
    public SpriteRenderer cardImage,attIcon, hpIcon;
    public bool attackable = true;
    public ParticleSystem sleep;
    public SpriteMask mask;
    [field: SerializeField] public MinionCardData minionCardData { get; set; }

    #region IBODY

    [field: SerializeField] public int PunId { get; set; }
    [field: SerializeField] public bool IsMine { get; set; }
    public Transform TR { get { return this.transform; } }

    [field: SerializeField] public Collider2D Col { get; set; }
    public bool Ray { set { if (Col == null) { Col = TR.GetComponent<Collider2D>(); } Col.enabled = value; } }

    public Vector3 OriginPos { get; set; }
    public IEnumerator onDead { get; set; }
    public Define.ObjType objType { get; set; }
    [field : SerializeField] public int OriginAtt { get; set; }
    [field: SerializeField] public int OriginHp { get; set; }

    public int Att
    {
        get { AttTmp.text = minionCardData.att.ToString(); return minionCardData.att; }
        set { minionCardData.att = value; AttTmp.text = minionCardData.att.ToString(); }
    }

    public int HP
    {
        get { HpTmp.text = minionCardData.hp.ToString(); return minionCardData.hp; }
        set { minionCardData.hp = value; HpTmp.text = minionCardData.hp.ToString(); }
    }
    #endregion
    
    // 미니언 카드가 공격을 한다 가정시, 부딪힐떄 위치가 겹치는 순간
    // 소팅 레이어가 동일하면 이미지가 겹치거나 꺠질 위험이 있어, 공격자가 최상단에 위치하도록 레이어 변경
    public void ChangeSortingLayer(bool isOn)
    {
        SortingLayer[] layers = SortingLayer.layers;
        SortingLayer layer = Array.Find(layers, x => x.name == ((isOn) ? "Attacker" : "None"));
        
        mask.frontSortingLayerID = layer.id;
        cardImage.sortingLayerID = layer.id;
        AttTmp.sortingLayerID = HpTmp.sortingLayerID = layer.id;
    }
    public void Init(CardData datapar, bool isMine)
    {
        data = datapar;
        IsMine = isMine;
        
        gameObject.layer = LayerMask.NameToLayer((IsMine == true) ? "ally" : "foe") ;
        
        Col = GetComponent<CircleCollider2D>();
        minionCardData = (MinionCardData)data;
        cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);
        
        GAME.Manager.UM.BindCardPopupEvent(this.gameObject, CallPopup, 0.75f);
        
        //attackable = (minionCardData.isCharge) ? true : false;
        //sleep.gameObject.SetActive((attackable)? true : false);
      
        if (IsMine)
        { 
            // 좌클릭시 카메라로 레이활성화하여 타겟팅 이벤트 시작
            GAME.Manager.UM.BindEvent(this.gameObject, StartAttack, Define.Mouse.ClickL, Define.Sound.Ready);
            
            // 소환시 , 손에서 낼떄 실행할 이벤트 있는지 확인
            List<CardBaseEvtData> list = data.evtDatas.FindAll(x => x.when == Define.evtWhen.onPlayed);

            // 이벤트 없으면 자동 취소
            if (list == null) { return; }

            // 먼저 유저가 직접 타겟팅하는 이벤트가 존재하는지 확인
            CardBaseEvtData selectEvt = list.Find(x => x.targeting == evtTargeting.Select);

            // 유저가 직접 찾는 타겟팅 방식이 있다면
            if (selectEvt != null)
            {
                // 전체 이벤트에서 직접 수동으로 타겟팅하는 이벤트만 제거
                list.Remove(selectEvt);

                // 타겟팅 카메라 실행 + 만약 타겟팅 성공시 공격함수 예약 실행
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
                    GAME.IGM.Battle.Evt(selectEvt, a,t);
                    yield return null;
                    for (int i = 0; i < list.Count; i++)
                    {
                        GAME.IGM.Battle.Evt(list[i], this);
                        yield return null;
                    }
                }
            }

            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    GAME.IGM.Battle.Evt(list[i], this);
                }
            }
            
        }
        onDead = Dead(IsMine);
        IEnumerator Dead(bool isMine)
        {
            // ���� ����������
            StartCoroutine(FadeOut());
            IEnumerator FadeOut()
            {
                float t = 1;
                while (t > 0f)
                {
                    // ����ȭ ����
                    t -= Time.deltaTime;
                    Color tempColor = new Color(1, 1, 1, t);
                    cardImage.color = attIcon.color = hpIcon.color = tempColor;
                    AttTmp.alpha = HpTmp.alpha = t;
                    yield return null;
                }
                mask.enabled = false;
            }

            // �¿�� �Դٰ��ٷ� �״� �ڷ�ƾ �ִ�
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

            // ���� ���� �ϼ����� ������ �������� ���� �������� �����ϱ�
            if (isMine)
            {
                // �� �ϼ��� �׾�����, ����Ʈ���� ���� �� ������ġ�� ������
                GAME.IGM.Spawn.playerMinions.Remove(GAME.IGM.Spawn.playerMinions.Find(x => x.PunId == this.PunId));
                // �ʵ� ������
                GAME.IGM.Spawn.CalcSpawnPoint(false);
                yield return new WaitForSeconds(1.5f);
                //yield return StartCoroutine(GAME.IGM.Spawn.AllPlayersAlignment());
            }
            // �� �ϼ��ε� �� ����
            else
            {
                GAME.IGM.Spawn.enemyMinions.Remove(GAME.IGM.Spawn.enemyMinions.Find(x => x.PunId == this.PunId));
                yield return StartCoroutine(GAME.IGM.Spawn.AllEnemiesAlignment());
            }

            Destroy(this.gameObject);
        }
        AttTmp.text = minionCardData.att.ToString(); HpTmp.text = minionCardData.hp.ToString();
    }

    // 미니언 카드의 경우, 일정초 동안 커서를 가져다 댈시 카드의 정보를 보여주는 카드팝업 호출 이벤트
    public void CallPopup()
    {
        Debug.Log(data.cardType.ToString());

        if (this.data is MinionCardData == false)
        {
            Debug.Log("ERROR : 왜 데이터가 미니언타입이 아닌지");
        }

        else
        {
            // 몇번쨰 인지 인덱스 찾기
            int idx = (this.IsMine) ? GAME.IGM.Spawn.playerMinions.IndexOf(this)
                : GAME.IGM.Spawn.enemyMinions.IndexOf(this) ;

            // 우측으로 너무 밀린 미니언의 경우 왼쪽으로 카드팝업을 띄어주기
            Vector3 pos = transform.position + Vector3.right * 2f;
            if (this.transform.position.x > 3f)
            { pos = transform.position - Vector3.right * 2f; }
            
            // 보여질 데이터와 위치 구해지면, 카드팝업 띄우기
            GAME.IGM.ShowMinionPopup((MinionCardData)data, pos, cardImage.sprite); 
        }
        
    }

    // 미니언 공격시도 함수 (좌클릭 마우스 이벤트)
    public void StartAttack(GameObject go)
    {
        // 공격 가능한 상태가 아니거나
        // 이미 다른 객체가 타겟팅 중인데 이 객체를 클릭시 취소
        if (!attackable || GAME.IGM.TC.LR.gameObject.activeSelf == true
            || sleep.gameObject.activeSelf == true)
        { return; }

        // 공격자 자신과, 스폰영역 레이 비활성화
        GAME.IGM.Spawn.SpawnRay = Ray = false;

        // 타겟팅 카메라 실행 + 만약 타겟팅 성공시 공격함수 예약 실행
        GAME.Manager.StartCoroutine(GAME.IGM.TC.TargettingCo
            (this,
            (IBody a, IBody t) => { return AttackCo(a, t); },
            new string[] { "foe", "foeHero" }
            ));
    }

    public IEnumerator AttackCo(IBody attacker, IBody target)
    {
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
        if (GAME.IGM.Packet.isMyTurn && attacker.IsMine)
        {
            GAME.IGM.Packet.SendMinionAttack(attacker.PunId, target.PunId);
        }

        if (target.HP <= 0) { yield return StartCoroutine(target.onDead); }
        if (attacker.HP <= 0) { yield return StartCoroutine(attacker.onDead); }
    }

}