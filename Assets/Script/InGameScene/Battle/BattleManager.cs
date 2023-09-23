using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleManager : MonoBehaviour
{
    FXManager FX;
    public Queue<IEnumerator> ActionQueue = new Queue<IEnumerator>();
    
    // 예약되는 이벤트를 순차적으로 실행
    public IEnumerator ProcessigCo()
    {
        while (true)
        {
            // 예약 이벤트 있을떄까지 대기
            yield return new WaitUntil(()=>(ActionQueue.Count > 0));

            yield return StartCoroutine(ActionQueue.Dequeue());
        }
    }
    private void Update()
    {
        Debug.Log(ActionQueue.Count());
    }
    private void Awake()
    {
        FX = GetComponent<FXManager>();
        GAME.Manager.IGM.Battle = this;
        StartCoroutine(ProcessigCo());
    }
    
    public void Evt(CardBaseEvtData evtData, IBody attacker)
    {
        Debug.Log("StartEvt");
        if (evtData is UtillHandler) { Debug.Log("Utill"); }

        switch (evtData.type) // 이벤트 타입별로 분류
        {
            case Define.evtType.attack: RegisterAttHandler((AttackHandler)evtData , attacker); return;
            case Define.evtType.buff: RegisterBuffHandler((BuffHandler)evtData, attacker); return;
            case Define.evtType.utill: RegisterUtillHandler((UtillHandler)evtData, attacker); break;
            case Define.evtType.restore: RegisterRestoreHandler((RestoreHandler)evtData, attacker); break;
        }
    }
    #region 유저가 선택건일시, 타겟 레이어 찾기
    string[] FindLayer(CardBaseEvtData ah) // 유저가 선택하는건이면, 타겟층의 레이어 찾기
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
    public void RegisterAttHandler(AttackHandler ah, IBody attacker)
    {
        // 유저가 직접 선택하는 타겟인지, 자동 선택되는 타겟인지 확인
        switch (ah.attTargeting)
        {
            // 유저가 직접 선택일시, 미니언 공격처럼 먼저 타겟팅 실행
            case Define.attTargeting.userSelect:
                StartCoroutine(GAME.Manager.IGM.TC.TargettingCo
                    (attacker,

                    // 공격인지 강제로 죽이는 이벤트인지 식별하기
                    (ah.attType == Define.attType.Damage) ? (IBody a, IBody t) => { return AttackEvt(a, t); }
                : (IBody a, IBody t) => { return KillEvt(a, t); },

                    // 타겟범위 레이어 찾기
                    FindLayer(ah)
                    ));
                break;
            case Define.attTargeting.randomOnEvtArea:
                // 설정된 이벤트타입별로 타겟범위 찾기
                IBody target = AutoExecute();
                if (target == null) { return; }
                
                // 타겟있을시 이벤트 예약
                if (ah.attType == Define.attType.Damage) { ActionQueue.Enqueue(AttackEvt(attacker, target)); }
                else { ActionQueue.Enqueue(KillEvt(attacker, target)); }
                break;
        }

    

        #region 자동 실행건
        IBody AutoExecute()
        {
            IBody caster = null;
            // 타겟의 범위
            List<IBody> targets = new List<IBody>();

            // 확인된 진영의 어떤 타입 : 하수인, 영웅 등
            switch (ah.faction) 
            { 
                case Define.evtFaction.All:
                    // 모든 진영의 모든 대상이면 자신을 제외한 모든 대상중 하나
                    if (ah.area == Define.evtArea.All) 
                    {
                        targets.Add(GAME.Manager.IGM.Hero.Player);
                        targets.Add(GAME.Manager.IGM.Hero.Enemy);
                        targets.AddRange(GAME.Manager.IGM.Spawn.playerMinions);
                        targets.AddRange(GAME.Manager.IGM.Spawn.enemyMinions);

                        // 대상 범위에서 시전자 자신은 제외시키기
                        caster = targets.Find(x => x.PunId == attacker.PunId);
                        if (caster != null)
                        { targets.Remove(caster); }

                        return targets[UnityEngine.Random.Range(0, targets.Count)];
                    }
                    return null;
                case Define.evtFaction.Minion:
                    // 두 진영의 모든 하수인이 범위 ( 시전자 자신은 제외 )
                    if (ah.area == Define.evtArea.All)
                    {
                        targets.AddRange(GAME.Manager.IGM.Spawn.playerMinions);
                        targets.AddRange(GAME.Manager.IGM.Spawn.enemyMinions);
                    }
                    // 시전자 진영의 하수인들
                    else if (ah.area == Define.evtArea.Player)
                    {
                        targets.AddRange((attacker.IsMine) ?
                            GAME.Manager.IGM.Spawn.playerMinions : GAME.Manager.IGM.Spawn.enemyMinions);
                    }
                    // 시전자의 적 하수인들중
                    else
                    {
                        targets.AddRange((attacker.IsMine) ?
                            GAME.Manager.IGM.Spawn.enemyMinions : GAME.Manager.IGM.Spawn.playerMinions);
                    }
                    // 대상 범위에서 시전자 자신은 제외시키기
                    caster = targets.Find(x => x.PunId == attacker.PunId);
                    if (caster != null)
                    { targets.Remove(caster); }
                    return targets[UnityEngine.Random.Range(0, targets.Count)];
                    
                case Define.evtFaction.Hero:
                    // 두영웅이 타겟 범위라면
                    if (ah.area == Define.evtArea.All)
                    {
                        targets.Add(GAME.Manager.IGM.Hero.Player);
                        targets.Add(GAME.Manager.IGM.Hero.Enemy);
                    }
                    // 시전자의 영웅만
                    else if (ah.area == Define.evtArea.Player)
                    {
                        targets.Add((attacker.IsMine) ? GAME.Manager.IGM.Hero.Player : GAME.Manager.IGM.Hero.Enemy);
                    }
                    // 시전자의 적 영웅만
                    else
                    {
                        targets.Add((attacker.IsMine) ? GAME.Manager.IGM.Hero.Enemy : GAME.Manager.IGM.Hero.Player);
                    }
                    return targets[UnityEngine.Random.Range(0, targets.Count)];
                default: return null;
            }

        }
        #endregion


        IEnumerator AttackEvt(IBody attacker, IBody target)
        {
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
            pj.gameObject.SetActive(false);
        }
        IEnumerator KillEvt(IBody attacker, IBody target)
        { // 투사체 호출
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
            pj.gameObject.SetActive(false);
        }
    }

    public void RegisterBuffHandler(BuffHandler bh, IBody caster)
    {
        switch (bh.buffTargeting)
        {
            case Define.buffTargeting.auto: AutoBuff();
                break;
            case Define.buffTargeting.userSelect:
                StartCoroutine(GAME.Manager.IGM.TC.TargettingCo
                    (caster,
                     // 이벤트 전달
                     (IBody a, IBody t) => { return Buff(a, t, bh); },
                    // 타겟범위 레이어 찾기
                    FindLayer(bh)
                    ));
                break;
            case Define.buffTargeting.randomOnEvtArea:
                OnFindRand();
                break;

        }

        #region 자동 실행 버프
        // 자동 버프 실행
        void AutoBuff()
        {
            // 추가 대상 여부 확인 및 찾기
            List<IBody> FindExtraTarget() 
            {
                // 추가 대상 범위 확인
                List<CardField> list = (caster.IsMine) ? GAME.Manager.IGM.Spawn.playerMinions : GAME.Manager.IGM.Spawn.enemyMinions;
                // 시전자 본인이 범위에 없을시 시전자 자신은 없애기
                if (bh.buffExtraArea != Define.buffExtraArea.withBothSide)
                { list.Remove(list.Find(x => x.PunId == caster.PunId)); }

                List<IBody> targets = new List<IBody>();
                // 추가 대상 확인
                switch (bh.buffExtraArea)
                {
                    // 추가 대상 없으면 바로 빠지기
                    case Define.buffExtraArea.None: return null;

                    // 오직 양옆만 + 양옆 포함의 경우 , 양옆만 구하기
                    case Define.buffExtraArea.withBothSide:
                    case Define.buffExtraArea.onlyBothSide:
                        int idx = list.IndexOf(list.Find(x => x.PunId == caster.PunId));

                        // 자신의 왼쪽이 존재 및 시전자 본인이 아니라면 타겟 확정
                        if (idx - 1 > -1) { targets.Add(list[idx - 1]); }
                        if (idx + 1 < list.Count ) { targets.Add(list[idx + 1]); }
                        break;

                    // 특정 넘버링을 가진 객체들만 추리기
                    case Define.buffExtraArea.someId:
                        // 만약 범위 넘버링이 없으면 취소
                        if (bh.relatedIds.Length == 0) { return null; }

                        List<CardField> rangeList = new List<CardField>();
                        // 적 하수인들중 특정 넘버링을 가진 수만큼
                        if (bh.area == Define.evtArea.Enemy)
                        {
                            rangeList = GAME.Manager.IGM.Spawn.enemyMinions;
                        }
                        else if (bh.area == Define.evtArea.Player) { rangeList = GAME.Manager.IGM.Spawn.playerMinions; }
                        else
                        {
                            rangeList = GAME.Manager.IGM.Spawn.playerMinions;
                            rangeList.AddRange(GAME.Manager.IGM.Spawn.enemyMinions);
                        }

                        // 연관 넘버링에 일치한 미니언들 모두 찾기
                        for (int i = 0; i < bh.relatedIds.Length; i++)
                        {
                            int currPunId = bh.relatedIds[i];
                            targets.AddRange(rangeList.FindAll(x => x.PunId == currPunId));
                        }
                        break;
                }


                // 타겟이 하나도 없다면 취소
                if (targets.Count == 0) { return null; }

                return targets;
            }

            // 이벤트 실행할 타겟들 찾기
            List<IBody> targetList = FindExtraTarget();

            if (targetList == null) { return; }

            // 타겟 범위만큼, 이벤트 예약
            for (int i = 0; i < targetList.Count; i++)
            {
                ActionQueue.Enqueue(Buff(caster, targetList[i], bh));
            }
            
        }
        #endregion


        #region 무작위 타겟을 찾아 적용
        void OnFindRand()
        {
            List<IBody> list = new List<IBody>();

            GetRange(bh.area);

            // 타겟들 찾기
            void GetRange(Define.evtArea area)
            {
                switch (bh.faction)
                {
                    case Define.evtFaction.All:
                        // 전체 범위의 전체 타입이라면
                        if (area == Define.evtArea.All)
                        {
                            list.AddRange(GAME.Manager.IGM.Spawn.playerMinions);
                            list.AddRange(GAME.Manager.IGM.Spawn.enemyMinions);
                            list.Add(GAME.Manager.IGM.Hero.Player);
                            list.Add(GAME.Manager.IGM.Hero.Enemy);
                        }

                        // 아군 전체 범위라면
                        else if (area == Define.evtArea.Player)
                        { list.Add(GAME.Manager.IGM.Hero.Player); list.AddRange(GAME.Manager.IGM.Spawn.playerMinions); }

                        // 적군 전체 범위라면
                        else
                        { list.Add(GAME.Manager.IGM.Hero.Enemy); list.AddRange(GAME.Manager.IGM.Spawn.playerMinions); }
                        break;

                    case Define.evtFaction.Minion:
                        // 전체 범위의 전체 타입이라면
                        if (area == Define.evtArea.All)
                        {
                            list.AddRange(GAME.Manager.IGM.Spawn.playerMinions);
                            list.AddRange(GAME.Manager.IGM.Spawn.enemyMinions);
                        }

                        // 아군 전체 범위라면
                        else if (area == Define.evtArea.Player)
                        { list.AddRange(GAME.Manager.IGM.Spawn.playerMinions); }

                        // 적군 전체 범위라면
                        else
                        {  list.AddRange(GAME.Manager.IGM.Spawn.playerMinions); }
                        break;
                    case Define.evtFaction.Hero:    
                        // 전체 범위의 영웅만이라면
                        if (area == Define.evtArea.All)
                        {
                            list.Add(GAME.Manager.IGM.Hero.Player);
                            list.Add(GAME.Manager.IGM.Hero.Enemy);
                        }

                        // 아군 영웅만
                        else if (area == Define.evtArea.Player)
                        { list.Add(GAME.Manager.IGM.Hero.Player); }

                        // 적 영웅만
                        else
                        { list.Add(GAME.Manager.IGM.Hero.Enemy);}
                        break;
                }
            }

            // 타겟 확정 및 이벤트 전달
            ActionQueue.Enqueue(Buff(caster,
                list[UnityEngine.Random.Range(0, list.Count)], bh));
        }
        #endregion

        // 실행할 버프 코루틴
        IEnumerator Buff(IBody caster, IBody target, BuffHandler bh)
        {
            // 어떠한 부여 효과인지
            switch (bh.buffType)
            {
                case Define.buffType.att:
                    target.Att += bh.buffAtt;
                    yield break;
                case Define.buffType.hp:
                    target.HP += bh.buffHp;
                    yield break;
                case Define.buffType.atthp:
                    target.Att += bh.buffAtt;
                    target.HP += bh.buffHp;
                    yield break;
            }
        }
    }

    public void RegisterUtillHandler(UtillHandler uh, IBody caster)
    {
        switch (uh.utillType) // 편의성 기타 이벤트는 유저 직접선택 없이 자동실행건들
        {
            case Define.utillType.draw: // 드로우 이벤트
                ActionQueue.Enqueue(DrawEvt(uh));
                break;
            case Define.utillType.find: // 발견 이벤트
                Debug.Log("발견 이벤트");
                ActionQueue.Enqueue(FindEvt(uh));
                break;
            case Define.utillType.acquisition: // 획득 이벤트
                Debug.Log("획득 이벤트");
                ActionQueue.Enqueue(AcquisitionEvt(uh));
                break;
        }

        // 드로우 이벤트
        IEnumerator DrawEvt(UtillHandler uh)
        {
            switch (uh.area)
            {
                // 상대의 드로우 이벤트는 상대가 실행
                case Define.evtArea.Enemy: yield break;

                // 모두 또는 나만 드로우의 경우, 내가 드로우 실행 (상대에게 이벤트는 전달되기에)
                case Define.evtArea.All:
                case Define.evtArea.Player:
                    // 드로우 실행
                    yield return StartCoroutine(GAME.Manager.IGM.Hand.CardDrawing(uh.utillAmount));
                    break;
            }
        }

        // 발견 이벤트
        IEnumerator FindEvt(UtillHandler uh)
        {
            // 이미 10장이면 강제 취소
            if (GAME.Manager.IGM.Hand.PlayerHand.Count == 10) { yield break; }

            // 발견 이벤트 준비 및 시작 (코루틴은 오브젝트가 꺼져있을떄 실행이 안되어, 함수로 먼저 데이터 준비 후 코루틴 실행)
            GAME.Manager.IGM.FindEvt.ReadyFindEvt(uh.relatedCards);

            // 발견 코루틴 끝날떄까지(유저가 선택할떄까지 대기)
            yield return new WaitUntil(() => (GAME.Manager.IGM.FindEvt.CurrSelected == true));
        }
        
        // 획득 이벤트
        IEnumerator AcquisitionEvt(UtillHandler uh)
        {
            // 획득하는 수치가, 최대 10장을 넘어서는지 확인 (넘을시 가능한 수만큼만 획득)
            int count = (GAME.Manager.IGM.Hand.PlayerHand.Count + uh.relatedCards.Length <= 10) 
                ? uh.relatedCards.Length : (10 - (GAME.Manager.IGM.Hand.PlayerHand.Count));

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
                GameObject.Instantiate(Resources.Load<CardHand>("Prefab/InGamePrefab/CardHand"), GAME.Manager.IGM.Hand.PlayerHandGO.transform);
                ch.Init(ref card);
                ch.PunId = (Photon.Pun.PhotonNetwork.IsMasterClient ? 1000 : 2000) 
                    + GAME.Manager.IGM.Hand.punConsist++;
                ch.transform.localScale = Vector3.one;
                ch.transform.localPosition = caster.TR.position;
                GAME.Manager.IGM.Hand.PlayerHand.Add(ch);
                yield return null;
            }

            // 그 후 정렬
            GAME.Manager.IGM.Hand.StopAllCoroutines();
            yield return StartCoroutine(GAME.Manager.IGM.Hand.CardAllignment(true));
        }


    }

    public void RegisterRestoreHandler(RestoreHandler rh, IBody caster)
    {
        switch (rh.restoreTargeting)
        {
            case Define.restoreTargeting.userSelect:
                StartCoroutine(GAME.Manager.IGM.TC.TargettingCo
                    (caster,
                     // 이벤트 전달
                     (IBody a, IBody t) => { return Restore(a, t, rh.restoreAmount); },
                    // 타겟범위 레이어 찾기
                    FindLayer(rh)
                    ));
                break;

            case Define.restoreTargeting.auto:
                AutoRestore();
                break;

            case Define.restoreTargeting.randomOnEvtArea:
                List<IBody> list = FindTarget();
                for (int i = 0; i < list.Count ; i++)
                {
                    ActionQueue.Enqueue(Restore(caster, list[i], rh.restoreAmount));
                }
                
                break;
        }

        void AutoRestore()
        {
            // 추가 대상 여부 확인 및 찾기
            List<IBody> FindExtraTarget()
            {
                // 추가 대상 없으면 바로 빠지기
                if (rh.restoreExtraArea == Define.restoreExtraArea.None)
                { return null; }
                
                // 추가 대상 범위 확인
                List<CardField> list = (caster.IsMine) ? GAME.Manager.IGM.Spawn.playerMinions : GAME.Manager.IGM.Spawn.enemyMinions;
                // 시전자 본인이 범위에 없을시 시전자 자신은 없애기
                
                List<IBody> targets = new List<IBody>();
                // 추가 대상 확인
                switch (rh.restoreExtraArea)
                {
                    // 오직 양옆만 + 양옆 포함의 경우 , 양옆만 구하기
                    case Define.restoreExtraArea.BothSide:
                        int idx = list.IndexOf(list.Find(x => x.PunId == caster.PunId));

                        // 자신의 왼쪽이 존재 및 시전자 본인이 아니라면 타겟 확정
                        if (idx - 1 > -1 && list[idx-1].PunId != caster.PunId) { targets.Add(list[idx - 1]); }
                        if (idx + 1 < list.Count && list[idx + 1].PunId != caster.PunId) { targets.Add(list[idx + 1]); }
                        break;

                    // 특정 넘버링을 가진 객체들만 추리기
                    case Define.restoreExtraArea.addOwnerHero:
                        targets.Add((caster.IsMine) ? GAME.Manager.IGM.Hero.Player : GAME.Manager.IGM.Hero.Enemy);
                        break;
                }


                // 타겟이 하나도 없다면 취소
                if (targets.Count == 0) { return null; }

                return targets;
            }

            // 이벤트 실행할 타겟들 찾기
            List<IBody> targetList = FindExtraTarget();

            if (targetList == null) { return; }

            // 타겟 범위만큼, 이벤트 예약
            for (int i = 0; i < targetList.Count; i++)
            {
                ActionQueue.Enqueue(Restore(caster, targetList[i], rh.restoreAmount));
            }

        }

        List<IBody> FindTarget()
        {
            List<IBody> targets = new List<IBody>();
            switch (rh.faction)
            {
                case Define.evtFaction.All:
                    if (rh.area == Define.evtArea.All)
                    {
                        targets.Add(GAME.Manager.IGM.Hero.Enemy);
                        targets.AddRange(GAME.Manager.IGM.Spawn.enemyMinions);
                        targets.AddRange(GAME.Manager.IGM.Spawn.playerMinions); 
                        targets.Add(GAME.Manager.IGM.Hero.Player);
                    }

                    else if (rh.area == Define.evtArea.Player) 
                    {
                        targets.AddRange(GAME.Manager.IGM.Spawn.playerMinions);
                        targets.Add(GAME.Manager.IGM.Hero.Player);
                    }

                    else
                    {
                        targets.Add(GAME.Manager.IGM.Hero.Enemy);
                        targets.AddRange(GAME.Manager.IGM.Spawn.enemyMinions);
                    }
                    break;

                case Define.evtFaction.Minion:
                    if (rh.area == Define.evtArea.All)
                    {
                        targets.AddRange(GAME.Manager.IGM.Spawn.enemyMinions);
                        targets.AddRange(GAME.Manager.IGM.Spawn.playerMinions);
                    }

                    else if (rh.area == Define.evtArea.Player)
                    {
                        targets.AddRange(GAME.Manager.IGM.Spawn.playerMinions);
                    }

                    else
                    {
                        targets.AddRange(GAME.Manager.IGM.Spawn.enemyMinions);
                    }
                    break;

                case Define.evtFaction.Hero:
                    if (rh.area == Define.evtArea.All)
                    {
                        targets.Add(GAME.Manager.IGM.Hero.Enemy);
                        targets.Add(GAME.Manager.IGM.Hero.Player);
                    }

                    else if (rh.area == Define.evtArea.Player)
                    {
                        targets.Add(GAME.Manager.IGM.Hero.Player);
                    }

                    else
                    {
                        targets.Add(GAME.Manager.IGM.Hero.Enemy);
                    }
                    break;
            }
            
            // 시전자 자신은 치료 X
            if (targets.Find(x => x.PunId == caster.PunId) != null)
            { targets.Remove(targets.Find(x => x.PunId == caster.PunId)); }

            return targets;
        }

        IEnumerator Restore(IBody caster , IBody target , int amount)
        {
            target.HP = Mathf.Clamp(target.HP + amount, 0, target.OriginHp);
            yield break;
        }
    }
}