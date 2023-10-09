using Newtonsoft.Json;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class BattleManager : MonoBehaviour
{
    public FXManager FX;
    public Queue<IEnumerator> ActionQueue = new Queue<IEnumerator>();
    public Queue<IEnumerator> DeathRattleQueue = new Queue<IEnumerator>();  
    public IEnumerator currCo;
    // 예약되는 이벤트를 순차적으로 실행
    public IEnumerator ProcessigCo()
    {
        while (true)
        {
            // 예약 이벤트 있을떄까지 대기
            yield return new WaitUntil(()=>(ActionQueue.Count > 0));
            currCo = ActionQueue.Dequeue();
            yield return StartCoroutine(currCo);

            // 현재 타겟팅 중이면 , 타겟팅이 끝날떄까지 대기
            yield return new WaitUntil(() => (GAME.IGM.TC.LR.gameObject.activeSelf == false));
            // 현재 죽을떄 실행 이벤트가 진행중이면 대기
            yield return new WaitUntil(() => (DeathRattleQueue.Count() == 0));
            currCo = null;
        }
    }
    public void PlayDeathRattle(IEnumerator co)
    {
        DeathRattleQueue.Enqueue(co);
        
        if (DeathRattleQueue.Count == 1)
        {
            StartCoroutine(DeathRattleCo());
        }

        IEnumerator DeathRattleCo()
        {
            while (DeathRattleQueue.Count > 0)
            {
                IEnumerator co = DeathRattleQueue.Dequeue(); 
                if (co == null) { Debug.Log("co is null"); continue; }
                Debug.Log($"received DeathRattle Co : {co}");
                yield return StartCoroutine(co);
            }
        }
    }

    private void Awake()
    {
        FX = GetComponent<FXManager>();
        GAME.IGM.Battle = this;
        StartCoroutine(ProcessigCo());
    }

    // 실행할 이벤트 코루틴을 반환하는 함수
    public IEnumerator Evt(CardBaseEvtData evtData, IBody attacker, IBody searchedTarget = null)
    {
        Debug.Log($"attacker[{attacker.PunId}]가 {evtData.when}이기에 {evtData.type}실행");
        if (evtData is UtillHandler) { Debug.Log("Utill"); }
        switch (evtData.type) // 이벤트 타입별로 분류
        {
            case Define.evtType.attack:  return RegisterAttHandler((AttackHandler)evtData, attacker, searchedTarget);
            case Define.evtType.buff: return RegisterBuffHandler((BuffHandler)evtData, attacker, searchedTarget); 
            case Define.evtType.utill: return RegisterUtillHandler((UtillHandler)evtData, attacker);
            case Define.evtType.restore: return RegisterRestoreHandler((RestoreHandler)evtData, attacker, searchedTarget);
            default: return null;
        }
    }

    #region 유저가 선택건일시, 타겟 레이어 찾기
    public string[] FindLayer(CardBaseEvtData ah) // 유저가 선택하는건이면, 타겟층의 레이어 찾기
    {
        switch (ah.area)
        {
            case Define.evtArea.All:
                if (ah.faction == Define.evtFaction.All)
                { return new string[] { "allyHero", "ally", "foeHero", "foe" }; }
                else if (ah.faction == Define.evtFaction.Minion)
                { return new string[] { "foe", "ally" }; }
                else
                { return new string[] { "foeHero", "allyHero" }; }
            case Define.evtArea.Player:
                if (ah.faction == Define.evtFaction.All)
                { return new string[] { "allyHero", "ally" }; }
                else if (ah.faction == Define.evtFaction.Minion)
                { return new string[] { "ally" }; }
                else
                { return new string[] { "allyHero" }; }
            case Define.evtArea.Enemy:
                if (ah.faction == Define.evtFaction.All)
                { return new string[] { "foeHero", "foe" }; }
                else if (ah.faction == Define.evtFaction.Minion)
                { return new string[] { "foe" }; }
                else
                { return new string[] { "foeHero" }; }
        }
        return null;
    }
    #endregion

    // 공격 이벤트 실행
    public IEnumerator RegisterAttHandler(AttackHandler ah, IBody attacker, IBody searchedTarget)
    {
        // 유저가 직접 선택하는 타겟인지, 자동 선택되는 타겟인지 확인
        // 주문카드의 경우 직접 타겟팅일시 자신의 영웅위치에서 시작하도록 실행
        // 유저가 직접 선택일시, 미니언 공격처럼 먼저 타겟팅 실행
        if (ah.targeting == Define.evtTargeting.Select)
        {
            return AttackEvt((attacker.objType == Define.ObjType.Minion) ?
                attacker : GAME.IGM.Hero.Player
                , searchedTarget, ah.attAmount, ah.attType);
        }

        else
        {
            IBody target = AutoExecute();
            if (target == null) { Debug.Log($"attacker[{attacker.PunId}] 의 이벤트 타겟 존재 X"); return null; }

            // 타겟있을시 이벤트 예약
            return AttackEvt((attacker.objType == Define.ObjType.Minion) ?
                attacker : GAME.IGM.Hero.Player, target, ah.attAmount, ah.attType, (ah.when == Define.evtWhen.onDead));
        }

        #region 자동 실행건
        IBody AutoExecute()
        {
            // 타겟의 범위
            List<IBody> targets = new List<IBody>();

            // 확인된 진영의 어떤 타입 : 하수인, 영웅 등
            switch (ah.faction) 
            { 
                case Define.evtFaction.All:
                    // 모든 진영의 모든 대상이면 자신을 제외한 모든 대상중 하나
                    if (ah.area == Define.evtArea.All)
                    {
                        targets.AddRange(GAME.IGM.allIBody.FindAll(x=>x.PunId != attacker.PunId && x.HP > 0));
                        return targets[UnityEngine.Random.Range(0, targets.Count)];
                    }
                    else if (ah.area == Define.evtArea.Enemy)
                    {
                        targets.AddRange(GAME.IGM.allIBody.FindAll(x => x.PunId != attacker.PunId && x.HP > 0
                        && x.IsMine == ( (attacker.IsMine) ? false : true ) ));

                        return targets[UnityEngine.Random.Range(0, targets.Count)];
                    }
                    else
                    {
                        targets.AddRange(GAME.IGM.allIBody.FindAll(x => x.PunId != attacker.PunId && x.HP > 0
                        && x.IsMine == ((attacker.IsMine) ? true : false )));

                        return targets[UnityEngine.Random.Range(0, targets.Count)];
                    }
                case Define.evtFaction.Minion:
                    // 두 진영의 모든 하수인이 범위 ( 시전자 자신은 제외 )
                    if (ah.area == Define.evtArea.All)
                    {
                        targets.AddRange(GAME.IGM.Spawn.playerMinions);
                        targets.AddRange(GAME.IGM.Spawn.enemyMinions);
                    }
                    // 시전자 진영의 하수인들
                    else if (ah.area == Define.evtArea.Player)
                    {
                        targets.AddRange((attacker.IsMine) ?
                            GAME.IGM.Spawn.playerMinions : GAME.IGM.Spawn.enemyMinions);
                    }
                    // 시전자의 적 하수인들중
                    else
                    {
                        targets.AddRange((attacker.IsMine) ?
                            GAME.IGM.Spawn.enemyMinions : GAME.IGM.Spawn.playerMinions);
                    }
                    // 대상 범위에서 시전자 자신은 제외시키기
                    attacker = targets.Find(x => x.PunId == attacker.PunId);
                    if (attacker != null)
                    { targets.Remove(attacker); }
                    return targets[UnityEngine.Random.Range(0, targets.Count)];
                    
                case Define.evtFaction.Hero:
                    // 두영웅이 타겟 범위라면
                    if (ah.area == Define.evtArea.All)
                    {
                        targets.Add(GAME.IGM.Hero.Player);
                        targets.Add(GAME.IGM.Hero.Enemy);
                    }
                    // 시전자의 영웅만
                    else if (ah.area == Define.evtArea.Player)
                    {
                        targets.Add((attacker.IsMine) ? GAME.IGM.Hero.Player : GAME.IGM.Hero.Enemy);
                    }
                    // 시전자의 적 영웅만
                    else
                    {
                        targets.Add((attacker.IsMine) ? GAME.IGM.Hero.Enemy : GAME.IGM.Hero.Player);
                    }
                    return targets[UnityEngine.Random.Range(0, targets.Count)];
                default: return null;
            }

        }
        #endregion

    }

    public IEnumerator RegisterBuffHandler(BuffHandler bh, IBody caster, IBody searchedTarget)
    {
        #region 유저가 직접 타겟을 선택하여 지정
        if (bh.targeting == Define.evtTargeting.Select)
        {
            return Buff(caster, searchedTarget, bh);
           
        }
        #endregion

        #region 그외 손에서 낼떄 자동실행이면 자동실행이면, 분류에 맞게 실행
        else
        {
            IEnumerator MoBuff(List<IBody> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    yield return GAME.IGM.StartCoroutine(Buff(caster, list[i], bh));
                }
            }

            switch (bh.buffAutoMode)
            {
                #region 이벤트 데이터내 범위의 타겟들에게 이벤트 실행
                case Define.buffAutoMode.autoOnEvtArea:
                    List<IBody> targetList = FindBaseRange(bh.area);
                    if (targetList.Count == 0) { Debug.Log($"caster[{caster.PunId}] 의 타겟이 없음 "); return null; }
                    
                    return MoBuff(targetList);
                  
                #endregion

                #region 랜덤 타겟을 하나 추출하여 이벤트 실행
                case Define.buffAutoMode.randomOnEvtArea:
                    targetList = FindBaseRange(bh.area);
                    if (targetList.Count == 0) { Debug.Log($"caster[{caster.PunId}] 의 타겟이 없음 "); return null; }
                    return Buff(caster, targetList[UnityEngine.Random.Range(0, targetList.Count)], bh);
                #endregion

                #region 시전 하수인 양옆에게 실행
                case Define.buffAutoMode.BothSide:
                    // 현재 이벤트 실행 하수인의 위치찾기 (단 하수인수가 혼자라면 실행할 필요 X )
                    List<CardField> minonList = (caster.IsMine) ? GAME.IGM.Spawn.playerMinions : GAME.IGM.Spawn.enemyMinions;
                    if (minonList.Count < 2) { Debug.Log($"시전자[{caster.PunId}]의 타겟이 존재하지 않음"); return null; }
                    List<IBody> list = new List<IBody>();
                    int idx = minonList.IndexOf(minonList.Find(x => x.PunId == caster.PunId));
                    // 자신의 왼쪽이 존재 및 시전자 본인이 아니라면 타겟 확정
                    if (idx - 1 > -1 && minonList[idx - 1].PunId != caster.PunId )
                    {
                        list.Add(minonList[idx - 1]);
                        Debug.Log($"양옆 하수인, 왼쪽에게 실행 타겟 {minonList[idx - 1]}[{minonList[idx - 1].PunId}]");
                        //ActionQueue.Enqueue(Buff(caster, minonList[idx - 1], bh));
                    }
                    // 시전자 하수인의 오른쪽 존재하는지 찾기
                    if (idx + 1 < minonList.Count && minonList[idx + 1].PunId != caster.PunId)
                    {
                        list.Add(minonList[idx + 1]);
                        Debug.Log($"양옆 하수인, 오른쪽에게 실행 타겟 {minonList[idx + 1]}[{minonList[idx+1].PunId}]");
                        //ActionQueue.Enqueue(Buff(caster, minonList[idx + 1], bh));
                    }
                    if (list.Count == 0) { return null; }
                    return MoBuff(list);
                #endregion

                #region 특정 카드ID의 하수인 수 만큼, 하수인에게 시전
                case Define.buffAutoMode.someID:
                    
                    // 손에 있을떄 실행건이라면
                    if (bh.when == Define.evtWhen.onHand)
                    {
                        // 손에 있을떄 실행건이라면, 현재 Caster가 핸드카드라는것을 알수 있다
                        CardHand ch = caster.TR.GetComponent<CardHand>();

                        // 손에 있는 핸드카드일떄  특정순간마다 실행할 이벤트 예약 실시
                        ch.HandCardChanged += BuffHandle(ch, ch.PunId, bh);
                        // 이벤트 등록시 최초 실행
                        ch.HandCardChanged.Invoke(ch.Data.cardIdNum, true);
                        return null;
                    }

                    // 손에서 낼떄, 즉 필드 미니언이 객체
                    else
                    {
                        CardField cf = caster.TR.GetComponent<CardField>();
                        int count = 0;
                        // 연관 카드넘버가 있으면, 필드의 동일 넘버 하수인당 벨류 곱해주기
                        if (bh.relatedIds.Length > 0)
                        {
                            for (int i = 0; i < bh.relatedIds.Length; i++)
                            {
                                count += GAME.IGM.allIBody.FindAll(x => x.PunId == bh.relatedIds[i] && x.HP > 0).Count();
                            }
                        }

                        // count 가 0 이면, 이벤트 범위의 모든 하수인으로 함축
                        else
                        {
                            switch (bh.area)
                            {
                                case Define.evtArea.Enemy:
                                    count = GAME.IGM.allIBody.FindAll(x => x.IsMine == false &&
                                    x.objType == Define.ObjType.Minion && x.HP > 0).Count(); break;
                                case Define.evtArea.Player:
                                    count = GAME.IGM.allIBody.FindAll(x => x.IsMine == true && x.PunId != caster.PunId 
                                    && x.objType == Define.ObjType.Minion && x.HP > 0).Count(); break;
                                case Define.evtArea.All:
                                    count = GAME.IGM.allIBody.FindAll(x=>x.objType == Define.ObjType.Minion 
                                    && x.PunId != caster.PunId && x.HP > 0).Count(); break;
                            }
                        }

                        // 이벤트 예약
                        return Buff(caster, caster, bh, count);
                    }
                #endregion

                default: return null;
            }
        }
        #endregion

        // 이벤트 범위내 해당 하는 타겟들 추적
        List<IBody> FindBaseRange(Define.evtArea area)
        {
            List<IBody> list = new List<IBody>();
            switch (bh.faction)
            {
                case Define.evtFaction.All:
                    // 전체 범위의 전체 타입이라면
                    if (area == Define.evtArea.All)
                    {
                        list.AddRange(GAME.IGM.Spawn.playerMinions);
                        list.AddRange(GAME.IGM.Spawn.enemyMinions);
                        list.Add(GAME.IGM.Hero.Player);
                        list.Add(GAME.IGM.Hero.Enemy);
                    }

                    // 아군 전체 범위라면
                    else if (area == Define.evtArea.Player)
                    { list.Add(GAME.IGM.Hero.Player); list.AddRange(GAME.IGM.Spawn.playerMinions); }

                    // 적군 전체 범위라면
                    else
                    { list.Add(GAME.IGM.Hero.Enemy); list.AddRange(GAME.IGM.Spawn.playerMinions); }
                    break;
                case Define.evtFaction.Minion:
                    // 전체 범위의 전체 타입이라면
                    if (area == Define.evtArea.All)
                    {
                        list.AddRange(GAME.IGM.Spawn.playerMinions);
                        list.AddRange(GAME.IGM.Spawn.enemyMinions);
                    }

                    // 아군 전체 범위라면
                    else if (area == Define.evtArea.Player)
                    { list.AddRange(GAME.IGM.Spawn.playerMinions); }

                    // 적군 전체 범위라면
                    else
                    { list.AddRange(GAME.IGM.Spawn.playerMinions); }
                    break;
                case Define.evtFaction.Hero:
                    // 전체 범위의 영웅만이라면
                    if (area == Define.evtArea.All)
                    {
                        list.Add(GAME.IGM.Hero.Player);
                        list.Add(GAME.IGM.Hero.Enemy);
                    }

                    // 아군 영웅만
                    else if (area == Define.evtArea.Player)
                    { list.Add(GAME.IGM.Hero.Player); }

                    // 적 영웅만
                    else
                    { list.Add(GAME.IGM.Hero.Enemy); }
                    break;
            }
            list.RemoveAll(x => x.HP <= 0);
            return list;
        }

        // 버프 이벤트 예약해주기
        Action<int, bool> BuffHandle(CardHand target, int ownerPunID, BuffHandler bh)
        {
            // 어떠한 부여 효과인지
            switch (bh.buffType)
            {
                case Define.buffType.att:
                    return (int id, bool IsMine) =>
                    {
                        int count = SortHandEvt(ref IsMine, ref bh, ref target, ref id);
                        target.Att = target.Att + bh.buffAtt * count;
                    };

                case Define.buffType.hp:
                    return (int id, bool IsMine) =>
                    {
                        int count = SortHandEvt(ref IsMine, ref bh, ref target, ref id);
                        target.HP = target.HP + bh.buffHp * count;
                    };

                case Define.buffType.atthp:
                    return (int id, bool IsMine) =>
                    {
                        int count = SortHandEvt(ref IsMine, ref bh, ref target, ref id);
                        target.Att = target.Att + bh.buffAtt * count;
                        target.HP = target.HP + bh.buffHp * count;
                    };

                case Define.buffType.cost:
                    return (int id, bool IsMine) =>
                    {
                        int count = SortHandEvt(ref IsMine, ref bh, ref target, ref id);
                        target.CurrCost = target.OriginCost - bh.costCount * count;
                    };

                default: return null;
            }
        }

        // 손에 있을떄 실행할 버프이벤트 분류함수
        int SortHandEvt(ref bool IsMine, ref BuffHandler bh, ref CardHand target, ref int id)
        {
            if (IsMine == true && bh.area == Define.evtArea.Enemy) { return 0; }
            if (IsMine == false && bh.area == Define.evtArea.Player) { return 0; }
            int origin = target.OriginAtt;
            int[] relatedID = bh.relatedIds;

            // 미니언카드 소환이지만 명확한 카드타겟 범위가 아니라면 이벤트 실행 취소
            if (bh.relatedIds.Length > 0 && !relatedID.Contains(id)) { return 0; }

            int count = 0;
            // 연관 카드넘버가 있으면, 필드의 동일 넘버 하수인당 벨류 곱해주기
            if (bh.relatedIds.Length > 0)
            {
                for (int i = 0; i < relatedID.Length; i++)
                {
                    count += GAME.IGM.allIBody.FindAll(x => x.PunId == relatedID[i]).Count();
                }
            }

            // count 가 0 이면, 이벤트 범위의 모든 하수인으로 함축
            else
            {
                switch (bh.area)
                {
                    case Define.evtArea.Enemy:
                        count = GAME.IGM.allIBody.FindAll(x => x.IsMine == false && x.HP > 0).Count() - 1; break;
                    case Define.evtArea.Player:
                        count = GAME.IGM.allIBody.FindAll(x => x.IsMine == true && x.HP > 0).Count() - 1; break;
                    case Define.evtArea.All:
                        count = GAME.IGM.allIBody.FindAll(x=> x.HP > 0).Count() - 2; break;
                }
            }
            return count;
        }
    }

    public IEnumerator RegisterUtillHandler(UtillHandler uh, IBody caster)
    {
        switch (uh.utillType) // 편의성 기타 이벤트는 유저 직접선택 없이 자동실행건들
        {
            case Define.utillType.draw: // 드로우 이벤트
                return DrawEvt(uh);
            case Define.utillType.find: // 발견 이벤트
                Debug.Log("발견 이벤트");
                return FindEvt(uh);
            case Define.utillType.acquisition: // 획득 이벤트
                Debug.Log("획득 이벤트");
                return AcquisitionEvt(uh, caster);
            default: return null;
        }

        // 드로우 이벤트
        IEnumerator DrawEvt(UtillHandler uh)
        {
            switch (uh.area)
            {
                #region 상대를 드로우 시키는 이벤트
                case Define.evtArea.Enemy:
                    return enemyDraw();
                    IEnumerator enemyDraw()
                    { yield return null; GAME.IGM.Packet.SendDoDraw(uh.utillAmount, false); };
                #endregion

                #region 나만 드로우하는 이벤트
                case Define.evtArea.Player:
                    return playerDraw();
                    IEnumerator playerDraw()
                    { yield return null; GAME.IGM.Hand.CardDrawing(uh.utillAmount); }
                #endregion

                #region 상대와 나 모두 드로우 하는 이벤트
                case Define.evtArea.All:
                    return bothDraw();
                    IEnumerator bothDraw()
                    { yield return null; GAME.IGM.Packet.SendDoDraw(uh.utillAmount, true); }
                #endregion
                default:return null;
            }
        }

        // 발견 이벤트
        IEnumerator FindEvt(UtillHandler uh)
        {
            // 이미 10장이면, 카드를 얻을수 없기에 강제 취소
            if (GAME.IGM.Hand.PlayerHand.Count == 10) { yield break; }

            // 발견 이벤트 실행시, 상대방에게 나의 발견이벤트 시작을 동기화하여 보여주기
            GAME.IGM.Packet.SendFindEvt();

            // 발견 이벤트 준비 및 시작 (코루틴은 오브젝트가 꺼져있을떄 실행이 안되어, 함수로 먼저 데이터 준비 후 코루틴 실행)
            GAME.IGM.FindEvt.ReadyFindEvt(uh.relatedCards);

            // 발견 코루틴 끝날떄까지 (유저가 발견카드들중 하나를 선택할떄까지 대기)
            yield return new WaitUntil(() => (GAME.IGM.FindEvt.CurrSelected == true));
        }
        
        // 획득 이벤트
        IEnumerator AcquisitionEvt(UtillHandler uh, IBody caster )
        {
            // 획득하는 수치가, 최대 10장을 넘어서는지 확인 (넘을시 가능한 수만큼만 획득)
            int count = (GAME.IGM.Hand.PlayerHand.Count + uh.relatedCards.Length <= 10) 
                ? uh.relatedCards.Length : (10 - (GAME.IGM.Hand.PlayerHand.Count));

            // 상대에게 보낼 시전자 펀넘버와 카드아이디배열
            int casterPunID = caster.PunId;
            int[] sendArray = new int[count];
            int[] punArray = new int[count];
            // 관련카드들 생성
            for (int i = 0; i < count; i++)
            {
                // 리소스 매니저의 경로를 반환 받는 딕셔너리 통해 카드타입과 카드데이터 찾기
                Define.cardType type = GAME.Manager.RM.PathFinder.Dic[uh.relatedCards[i]].type;
                string jsonFile = GAME.Manager.RM.PathFinder.Dic[uh.relatedCards[i]].GetJson();
                
                CardData card = null;  
                // 확인된 카드타입으로, 실제 카드타입으로 클래스화
                switch (type)
                {
                    case Define.cardType.minion:
                        card = JsonConvert.DeserializeObject<MinionCardData>
                    (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                        break;
                    case Define.cardType.spell:
                        card = JsonConvert.DeserializeObject<SpellCardData>
                    (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                        break;
                    case Define.cardType.weapon:
                        card = JsonConvert.DeserializeObject<WeaponCardData>
                    (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                        break;
                    default: break;
                }

                // 해당 카드번호의 프리팹 생성
                CardHand ch =
                GameObject.Instantiate(Resources.Load<CardHand>("Prefab/InGamePrefab/CardHand"), GAME.IGM.Hand.PlayerHandGO.transform);
                ch.Init(card, caster.IsMine);
                ch.PunId = GAME.IGM.Hand.CreatePunNumber();

                // 상대에게 전달할 카드식별자와 펀식별자도 생성하기
                sendArray[i] = uh.relatedCards[i];
                punArray[i] = ch.PunId;

                ch.transform.localScale = Vector3.zero;
                ch.transform.localPosition = caster.TR.position;
                
                GAME.IGM.Hand.PlayerHand.Add(ch);
                yield return null;
            }

            // 상대에게 나의 획득이벤트를 전파하기
            GAME.IGM.Packet.SendAcquisition(caster.objType , casterPunID, sendArray, punArray);

        }
    }

    public IEnumerator RegisterRestoreHandler(RestoreHandler rh, IBody caster, IBody searchedTarget)
    {

        #region 유저가 직접 선택하여 치료이벤트 실행
        if (rh.targeting == Define.evtTargeting.Select)
        {
            return Restore(caster, searchedTarget, rh.restoreAmount);
        }
        #endregion

        #region 자동실행건이면, 분류에 맞게 실행
        else
        {
            IEnumerator MoRestore(List<IBody> list, bool isDeathRattle = false)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    yield return StartCoroutine(Restore(caster, list[i], rh.restoreAmount, isDeathRattle));
                }
            }

            // 치료이벤트의 동작 방식에 따라 나누어 처리
            switch (rh.restoreAutoMode)
            {
                #region 이벤트범위내 타겟들을 모두 찾아 실행
                case Define.restoreAutoMode.AutoOnEvtArea:
                    List<IBody> list = FindBaseTarget();
                    if (list.Count == 0) { Debug.Log($"시전자[{caster.PunId}]의 타겟이 존재하지 않음"); return null; }
                    return MoRestore(list, (rh.when == Define.evtWhen.onDead));
                #endregion

                #region 이벤트 범위의 타겟팅중 랜덤타겟 하나를 찾아 실행
                case Define.restoreAutoMode.RandomOnEvtArea:
                    list = FindBaseTarget();
                    if (list.Count == 0) { Debug.Log($"시전자[{caster.PunId}]의 타겟이 존재하지 않음"); return null; }
                    return MoRestore(list, (rh.when == Define.evtWhen.onDead));
                #endregion

                #region 소환된 시전 미니언의 양옆을 타겟지정
                case Define.restoreAutoMode.BothSide:
                    // 현재 이벤트 실행 하수인의 위치찾기 (단 하수인수가 혼자라면 실행할 필요 X )
                    List<CardField> minonList = (caster.IsMine) ? GAME.IGM.Spawn.playerMinions : GAME.IGM.Spawn.enemyMinions;
                    if (minonList.Count < 2) { Debug.Log($"시전자[{caster.PunId}]의 타겟이 존재하지 않음"); return null; }
                    list = new List<IBody>();
                    int idx = minonList.IndexOf(minonList.Find(x => x.PunId == caster.PunId));
                    List<IBody> targets = new List<IBody>();
                    // 자신의 왼쪽이 존재 및 시전자 본인이 아니라면 타겟 확정
                    if (idx - 1 > -1 && minonList[idx - 1].PunId != caster.PunId)
                    {
                        list.Add(minonList[idx - 1]);
                    }
                    // 시전자 하수인의 오른쪽 존재하는지 찾기
                    if (idx + 1 < minonList.Count && minonList[idx + 1].PunId != caster.PunId)
                    {
                        list.Add(minonList[idx + 1]);
                    }
                    if (list.Count == 0) { return null; }
                    return MoRestore(list, (rh.when == Define.evtWhen.onDead));
                #endregion
                default:return null;
            }
        }
        #endregion

        // 설정한 이벤트의 범위에서 기본범위 찾기
        List<IBody> FindBaseTarget()
        {
            List<IBody> targets = new List<IBody>();
            switch (rh.faction)
            {
                case Define.evtFaction.All:
                    if (rh.area == Define.evtArea.All)
                    {
                        targets.Add(GAME.IGM.Hero.Enemy);
                        targets.AddRange(GAME.IGM.Spawn.enemyMinions);
                        targets.AddRange(GAME.IGM.Spawn.playerMinions);
                        targets.Add(GAME.IGM.Hero.Player);
                    }

                    else if (rh.area == Define.evtArea.Player)
                    {
                        targets.AddRange(GAME.IGM.Spawn.playerMinions);
                        targets.Add(GAME.IGM.Hero.Player);
                    }

                    else
                    {
                        targets.Add(GAME.IGM.Hero.Enemy);
                        targets.AddRange(GAME.IGM.Spawn.enemyMinions);
                    }
                    break;

                case Define.evtFaction.Minion:
                    if (rh.area == Define.evtArea.All)
                    {
                        targets.AddRange(GAME.IGM.Spawn.enemyMinions);
                        targets.AddRange(GAME.IGM.Spawn.playerMinions);
                    }

                    else if (rh.area == Define.evtArea.Player)
                    {
                        targets.AddRange((caster.IsMine) ? GAME.IGM.Spawn.playerMinions : GAME.IGM.Spawn.enemyMinions);
                    }

                    else
                    {
                        targets.AddRange((caster.IsMine) ? GAME.IGM.Spawn.enemyMinions : GAME.IGM.Spawn.playerMinions);
                    }
                    break;

                case Define.evtFaction.Hero:
                    if (rh.area == Define.evtArea.All)
                    {
                        targets.Add(GAME.IGM.Hero.Enemy);
                        targets.Add(GAME.IGM.Hero.Player);
                    }

                    else if (rh.area == Define.evtArea.Player)
                    {
                        targets.Add((caster.IsMine)? GAME.IGM.Hero.Player : GAME.IGM.Hero.Enemy);
                    }

                    else
                    {
                        targets.Add((caster.IsMine) ? GAME.IGM.Hero.Enemy : GAME.IGM.Hero.Player);
                    }
                    break;
            }

            // 시전자 자신은 치료 X
            if (targets.Find(x => x.PunId == caster.PunId) != null)
            { targets.Remove(targets.Find(x => x.PunId == caster.PunId)); }
            targets.RemoveAll(x=>x.HP <= 0);
            return targets;
        }

    }


    // 실행할 버프 코루틴
    public IEnumerator Buff(IBody caster, IBody target, BuffHandler bh, int multiPly = 1)
    {
        // 어떠한 부여 효과인지
        switch (bh.buffType)
        {
            case Define.buffType.att:
                target.Att += bh.buffAtt* multiPly;
                break;
            case Define.buffType.hp:
                target.HP += bh.buffHp * multiPly;
                break;
            case Define.buffType.atthp:
                target.Att += bh.buffAtt * multiPly;
                target.HP += bh.buffHp * multiPly;
                break;
            case Define.buffType.cost:
                break;
        }
        Debug.Log($"타겟 : {target.PunId}, 시전자 : {caster.PunId}");

        // 버프 이벤트 동기화를 위해 전달 (죽을떄 실행인지, 일반 실행인지 구분해서 실행)
        if (bh.when == Define.evtWhen.onDead)
        { GAME.IGM.Packet.SendDeathBuffEvt(target.PunId, bh.buffType, bh.buffAtt * multiPly, bh.buffHp * multiPly); }
        else
        { GAME.IGM.Packet.SendBuffEvt(target.PunId, bh.buffType, bh.buffAtt * multiPly, bh.buffHp * multiPly); }
        
        yield break;
    }
    public IEnumerator ReceivedBuff(Define.buffType type, IBody target,int att, int hp )
    {
        // 어떠한 부여 효과인지
        switch (type)
        {
            case Define.buffType.att:
                target.Att += att;
                break;
            case Define.buffType.hp:
                target.HP += hp;
                break;
            case Define.buffType.atthp:
                target.Att += att;
                target.HP += hp;
                break;
            case Define.buffType.cost:
                break;
        }
        yield break;
    }

    // 치료 코루틴
    public IEnumerator Restore(IBody caster, IBody target, int amount, bool isDeathRattle = false)
    {
        Debug.Log($"치료이벤트 실행, target : {target}[{target.PunId}]");
        target.HP = Mathf.Clamp(target.HP + amount, 0, (target.objType == Define.ObjType.Minion) ? target.OriginHp : 30);

        // 내턴이며 시전자가 내가 낸 하수인일떄만 상대에게 이벤트 전파
        if (GAME.IGM.Packet.isMyTurn)
        {
            // 죽을떄 실행할 이벤트라면 (독립적인 데스액션큐로 실행 예정이라)
            if (isDeathRattle == true)
            {
                GAME.IGM.Packet.SendDeathRestoreEvt(caster.PunId, target.PunId, amount,
                    (caster.objType == Define.ObjType.HandCard));
            }
            // 일반 이벤트
            else
            {
                GAME.IGM.Packet.SendRestoreEvt(caster.PunId, target.PunId, 
                    amount, (caster.objType == Define.ObjType.HandCard));
            }
        }

        yield break;
    }
   
    // 공격 코루틴
    public IEnumerator AttackEvt(IBody attacker, IBody target, int attAmount, Define.attType attType , bool isDeathRattle = false )
    {
        #region 투사체 준비 및 투사체 이동 코루틴
        // 죽을떄 실행 이벤트라면, 공격자 피해자 모두 제자리 복귀떄까지 대기
        if (isDeathRattle == true && GAME.IGM.Packet.isMyTurn == false)
        {
            yield return new WaitForSeconds(0.5f);
        }
        // 투사체 호출
        ParticleSystem pj = FX.GetPJ;
        // 공격자의 위치에서 시작하도록 위치 초기화
        pj.transform.position = attacker.Pos;
        pj.gameObject.SetActive(true);
        Vector3 start = attacker.Pos;
        Vector3 dest = target.Pos;
        Vector3 dir = (dest - start).normalized; // 방향벡터
        float angle = Vector3.Angle(attacker.TR.up, dir);
        Vector3 cross = Vector3.Cross(attacker.TR.up, dir);
        if (cross.y < 0) { angle *= -1; }

        // 투사체 선형보간으로 타겟으로 향하며 이동
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            pj.transform.rotation =
                Quaternion.Euler(new Vector3(0, 0, 90f + Mathf.Lerp(0, angle, t)));
            pj.transform.position =
                Vector3.Lerp(start, dest, t);

            yield return null;
        }

        // 투사체 끄기
        pj.gameObject.SetActive(false);
        #endregion

        // 나의 턴 + 나의 하수인 공격이벤트는 , 전파해야할 이벤트
        if (GAME.IGM.Packet.isMyTurn == true )
        {
            if (isDeathRattle == true)
            {
                GAME.IGM.Packet.SendDeathAttEvt(attacker.PunId, target.PunId, attType, attAmount, attacker.objType);
            }
            // 일반 공격이벤트
            else 
            {
                GAME.IGM.Packet.SendAttEvt(attacker.PunId, target.PunId, attType, attAmount, attacker.objType);
            }
        }

        Debug.Log($"attacker : {attacker}[{attacker.PunId}], target : {target}[{target.PunId}]");
        target.HP -=  (attType == Define.attType.Damage) ? attAmount : 1000;
        if (target.HP <= 0) 
        { 
            yield return StartCoroutine(target.onDead); 
        }
    }
   
}