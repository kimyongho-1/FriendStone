using Newtonsoft.Json;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class BattleManager : MonoBehaviour
{
    FXManager FX;
    public Queue<IEnumerator> ActionQueue = new Queue<IEnumerator>();
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
            currCo = null;
        }
    }
    private void Awake()
    {
        FX = GetComponent<FXManager>();
        GAME.IGM.Battle = this;
        StartCoroutine(ProcessigCo());
    }

    public void OnHandEvt(CardBaseEvtData evtData, CardHand ch)
    { 
        
    }

    public void Evt(CardBaseEvtData evtData, IBody attacker)
    {
        Debug.Log($"attacker[{attacker.PunId}]가 {evtData.when}이기에 {evtData.type}실행");
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
                StartCoroutine(GAME.IGM.TC.TargettingCo
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
                if (target == null) { Debug.Log($"attacker[{attacker.PunId}] 의 이벤트 타겟 존재 X"); return; }
                
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
                        targets.Add(GAME.IGM.Hero.Player);
                        targets.Add(GAME.IGM.Hero.Enemy);
                        targets.AddRange(GAME.IGM.Spawn.playerMinions);
                        targets.AddRange(GAME.IGM.Spawn.enemyMinions);

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
                    caster = targets.Find(x => x.PunId == attacker.PunId);
                    if (caster != null)
                    { targets.Remove(caster); }
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

        #region 이벤트 코루틴
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
        #endregion
    }

    public void RegisterBuffHandler(BuffHandler bh, IBody caster)
    {
        switch (bh.buffTargeting)
        {
            #region 이벤트 데이터내 범위의 타겟들에게 이벤트 실행
            case Define.buffTargeting.autoOnEvtArea:
                List<IBody> targetList = FindBaseRange(bh.area);
                if (targetList.Count == 0) { Debug.Log($"caster[{caster.PunId}] 의 타겟이 없음 "); return; }
                // 타겟 범위만큼, 이벤트 예약
                for (int i = 0; i < targetList.Count; i++)
                {
                    ActionQueue.Enqueue(Buff(caster, targetList[i], bh));
                }
                break;
            #endregion

            #region 유저가 직접 타겟을 선택하여 지정
            case Define.buffTargeting.userSelect:
                StartCoroutine(GAME.IGM.TC.TargettingCo
                    (caster,
                     // 이벤트 전달
                     (IBody a, IBody t) => { return Buff(a, t, bh); },
                    // 타겟범위 레이어 찾기
                    FindLayer(bh)
                    ));
                break;
            #endregion

            #region 랜덤 타겟을 하나 추출하여 이벤트 실행
            case Define.buffTargeting.randomOnEvtArea:
                targetList = FindBaseRange(bh.area);
                if (targetList.Count == 0) { Debug.Log($"caster[{caster.PunId}] 의 타겟이 없음 "); return; }
                ActionQueue.Enqueue(Buff(caster, targetList[UnityEngine.Random.Range(0, targetList.Count)], bh));
                break;
            #endregion

            #region 시전 하수인 양옆에게 실행
            case Define.buffTargeting.BothSide:
                // 현재 이벤트 실행 하수인의 위치찾기 (단 하수인수가 혼자라면 실행할 필요 X )
                List<CardField> minonList = (caster.IsMine) ? GAME.IGM.Spawn.playerMinions : GAME.IGM.Spawn.enemyMinions;
                if (minonList.Count < 2) { Debug.Log($"시전자[{caster.PunId}]의 타겟이 존재하지 않음"); return; }

                int idx = minonList.IndexOf(minonList.Find(x => x.PunId == caster.PunId));
                List<IBody> targets = new List<IBody>();
                // 자신의 왼쪽이 존재 및 시전자 본인이 아니라면 타겟 확정
                if (idx - 1 > -1 && minonList[idx - 1].PunId != caster.PunId)
                {
                    ActionQueue.Enqueue(Buff(caster, minonList[idx - 1], bh));
                }
                // 시전자 하수인의 오른쪽 존재하는지 찾기
                if (idx + 1 < minonList.Count && minonList[idx + 1].PunId != caster.PunId)
                {
                    ActionQueue.Enqueue(Buff(caster, minonList[idx + 1], bh));
                }
                break;
            #endregion

            #region 특정 카드ID의 하수인 수 만큼, 하수인에게 시전
            case Define.buffTargeting.someID:
                // 손에 있을떄 실행건이라면, 현재 Caster가 핸드카드라는것을 알수 있다
                CardHand ch = caster.TR.GetComponent<CardHand>();

                // 손에 있을떄 실행건이라면
                if (bh.when == Define.evtWhen.onHand)
                {
                    // 손에 있는 핸드카드일떄  특정순간마다 실행할 이벤트 예약 실시
                    ch.HandCardChanged += BuffHandle(ch, ch.PunId, bh);
                    // 이벤트 등록시 최초 실행
                    ch.HandCardChanged.Invoke(ch.data.cardIdNum, true);
                }
                else 
                {
                    GAME.IGM.AddAction(InvokeCo(ch, ch.PunId, bh)); 
                    IEnumerator InvokeCo(CardHand target, int ownerPunID,  BuffHandler bh)
                    { BuffHandle(ch, ch.PunId, bh); yield return null; }
                }
                break;
                #endregion
        }

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

            return list;
        }

        // 버프 이벤트 예약해주기
        Action<int,bool> BuffHandle(CardHand target,int ownerPunID,  BuffHandler bh)
        {
            // 어떠한 부여 효과인지
            switch (bh.buffType)
            {
                case Define.buffType.att:
                    return (int id, bool IsMine) =>
                    {
                        // 예외처리, 타겟하수인의 영역이 존재하는데 현재 소환하는 하수인의 소유여부와 불일치시 이벤트 실행 X
                        if (IsMine == true && bh.area == Define.evtArea.Enemy) { return; }
                        if (IsMine == false && bh.area == Define.evtArea.Player) { return; }

                        int origin = target.OriginAtt;
                        int[] relatedID = bh.relatedIds;

                        // 미니언카드 소환이지만 명확한 카드타겟 범위가 아니라면 이벤트 실행 취소
                        if (bh.relatedIds.Length > 0 && !relatedID.Contains(id)) { return; }

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
                                    count = GAME.IGM.allIBody.FindAll(x => x.IsMine == false).Count() - 1; break;
                                case Define.evtArea.Player:
                                    count = GAME.IGM.allIBody.FindAll(x => x.IsMine == true).Count() - 1; break;
                                case Define.evtArea.All:
                                    count = GAME.IGM.allIBody.Count() - 2; break;
                            }
                        }

                        switch (target.data.cardType)
                        {
                            case Define.cardType.minion:
                                MinionCardData mc = (MinionCardData)target.data;
                                mc.att = origin + bh.buffAtt * count;
                                target.Stat.text = $"<color=yellow>ATT {mc.att} <color=red>HP {mc.hp} <color=black>몬스터";
                                break;
                            case Define.cardType.spell: break;
                            case Define.cardType.weapon:
                                WeaponCardData wData = (WeaponCardData)target.data;
                                wData.att = origin + bh.buffAtt * count;
                                target.Stat.text = $"<color=yellow>ATT {wData.att} <color=red>dur {wData.durability} <color=black>무기";
                                break;
                        }
                    };

                case Define.buffType.hp:
                    return (int id,bool IsMine) =>
                    {
                        // 예외처리, 타겟하수인의 영역이 존재하는데 현재 소환하는 하수인의 소유여부와 불일치시 이벤트 실행 X
                        if (IsMine == true && bh.area == Define.evtArea.Enemy) { return; }
                        if (IsMine == false && bh.area == Define.evtArea.Player) { return; }
                        int origin = target.OriginHp;
                        int[] relatedID = bh.relatedIds;

                        // 미니언카드 소환이지만 명확한 카드타겟 범위가 아니라면 이벤트 실행 취소
                        if (bh.relatedIds.Length > 0 && !relatedID.Contains(id)) { return; }

                        int count = 0;
                        // 연관 카드넘버가 있으면, 필드의 동일 넘버 하수인당 벨류 곱해주기
                        if (bh.relatedIds.Length > 0 )
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
                                    count = GAME.IGM.allIBody.FindAll(x => x.IsMine == false).Count() - 1; break;
                                case Define.evtArea.Player:
                                    count = GAME.IGM.allIBody.FindAll(x => x.IsMine == true).Count() - 1; break;
                                case Define.evtArea.All:
                                    count = GAME.IGM.allIBody.Count() - 2; break;
                            }
                        }

                        switch (target.data.cardType)
                        {
                            case Define.cardType.minion:
                                MinionCardData mc = (MinionCardData)target.data;
                                mc.hp = origin + bh.buffHp * count;
                                target.Stat.text = $"<color=yellow>ATT {mc.att} <color=red>HP {mc.hp} <color=black>몬스터";
                                break;
                            case Define.cardType.spell: break;
                            case Define.cardType.weapon:
                                WeaponCardData wData = (WeaponCardData)target.data;
                                wData.durability = origin + bh.buffHp * count;
                                target.Stat.text = $"<color=yellow>ATT {wData.att} <color=red>dur {wData.durability} <color=black>무기";
                                break;
                        }
                    };

                case Define.buffType.atthp:
                    return (int id, bool IsMine) =>
                    {
                        // 예외처리, 타겟하수인의 영역이 존재하는데 현재 소환하는 하수인의 소유여부와 불일치시 이벤트 실행 X
                        if (IsMine == true && bh.area == Define.evtArea.Enemy) { return; }
                        if (IsMine == false && bh.area == Define.evtArea.Player) { return; }
                        int[] relatedID = bh.relatedIds;
                        int count = 0;
                        
                        // 미니언카드 소환이지만 명확한 카드타겟 범위가 아니라면 이벤트 실행 취소
                        if (bh.relatedIds.Length > 0 && !relatedID.Contains(id)) { return; }

                        // 연관 카드넘버가 있으면, 필드의 동일 넘버 하수인당 벨류 곱해주기
                        if (bh.relatedIds.Length > 0 )
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
                                    count = GAME.IGM.allIBody.FindAll(x => x.IsMine == false).Count() - 1; break;
                                case Define.evtArea.Player:
                                    count = GAME.IGM.allIBody.FindAll(x => x.IsMine == true).Count() - 1; break;
                                case Define.evtArea.All:
                                    count = GAME.IGM.allIBody.Count() - 2; break;
                            }
                        }

                        switch (target.data.cardType)
                        {
                            case Define.cardType.minion:
                                MinionCardData mc = (MinionCardData)target.data;
                                mc.att = target.OriginAtt + bh.buffAtt * count;
                                mc.hp = target.OriginHp + bh.buffHp * count;
                                target.Stat.text = $"<color=yellow>ATT {mc.att} <color=red>HP {mc.hp} <color=black>몬스터";
                                break;
                            case Define.cardType.spell: break;
                            case Define.cardType.weapon:
                                WeaponCardData wData = (WeaponCardData)target.data;
                                wData.att = target.OriginAtt + bh.buffAtt * count; 
                                wData.durability = target.OriginHp + bh.buffHp * count;
                                target.Stat.text = $"<color=yellow>ATT {wData.att} <color=red>dur {wData.durability} <color=black>무기";
                                break;
                        }
                    };

                case Define.buffType.cost:
                    return (int id, bool IsMine) =>
                    {
                        // 예외처리, 타겟하수인의 영역이 존재하는데 현재 소환하는 하수인의 소유여부와 불일치시 이벤트 실행 X
                        if (IsMine == true && bh.area == Define.evtArea.Enemy) { return; }
                        if (IsMine == false && bh.area == Define.evtArea.Player) { return; }
                        int[] relatedID = bh.relatedIds;
                        int count = 0;
                        
                        // 미니언카드 소환이지만 명확한 카드타겟 범위가 아니라면 이벤트 실행 취소
                        if (bh.relatedIds.Length > 0 && !relatedID.Contains(id)) { return; }

                        // 연관 카드넘버가 있으면, 필드의 동일 넘버 하수인당 벨류 곱해주기
                        if (bh.relatedIds.Length > 0 )
                        {
                            for (int i = 0; i < relatedID.Length; i++)
                            {
                                count += GAME.IGM.allIBody.FindAll(x => x.PunId == relatedID[i]).Count();
                            }
                        }

                        // related 가 0 이면, 이벤트 범위의 모든 하수인으로 함축
                        else
                        {
                            switch (bh.area)
                            {
                                case Define.evtArea.Enemy:
                                    count = GAME.IGM.allIBody.FindAll(x => x.IsMine == false).Count() - 1; break;
                                case Define.evtArea.Player:
                                    count = GAME.IGM.allIBody.FindAll(x => x.IsMine == true).Count() - 1; break;
                                case Define.evtArea.All:
                                    count = GAME.IGM.allIBody.Count() - 2; break;
                            }
                        }

                        target.data.cost = target.originCost + bh.costCount * count;
                        target.Cost.text = target.data.cost.ToString();
                    };

                default: return null;
            }
        }

        // 실행할 버프 코루틴
        IEnumerator Buff(IBody caster, IBody target, BuffHandler bh )
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
                case Define.buffType.cost:
                    
                    yield break;
            }
        }
    }

    public void RegisterUtillHandler(UtillHandler uh, IBody caster)
    {
        switch (uh.utillType) // 편의성 기타 이벤트는 유저 직접선택 없이 자동실행건들
        {
            case Define.utillType.draw: // 드로우 이벤트
                DrawEvt(uh);
                break;
            case Define.utillType.find: // 발견 이벤트
                Debug.Log("발견 이벤트");
                ActionQueue.Enqueue(FindEvt(uh));
                break;
            case Define.utillType.acquisition: // 획득 이벤트
                Debug.Log("획득 이벤트");
                ActionQueue.Enqueue(AcquisitionEvt(uh,caster));
                break;
        }

        // 드로우 이벤트
        void DrawEvt(UtillHandler uh)
        {
            switch (uh.area)
            {
                #region 상대를 드로우 시키는 이벤트
                case Define.evtArea.Enemy:
                    GAME.IGM.Packet.SendDoDraw(uh.utillAmount, false);
                    return;
                #endregion

                #region 나만 드로우하는 이벤트
                case Define.evtArea.Player: 
                    ActionQueue.Enqueue(GAME.IGM.Hand.CardDrawing(uh.utillAmount)); return;
                #endregion

                #region 상대와 나 모두 드로우 하는 이벤트
                case Define.evtArea.All:
                    GAME.IGM.Packet.SendDoDraw(uh.utillAmount, true);
                    return;
                #endregion
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

                ch.transform.localScale = Vector3.one;
                ch.transform.localPosition = caster.TR.position;
                GAME.IGM.Hand.PlayerHand.Add(ch);
                yield return null;
            }

            // 상대에게 나의 획득이벤트를 전파하기
            GAME.IGM.Packet.SendAcquisition(casterPunID,sendArray,punArray);

            // 카드 정렬
            yield return StartCoroutine(GAME.IGM.Hand.CardAllignment(true));
        }
    }

    public void RegisterRestoreHandler(RestoreHandler rh, IBody caster)
    {
        // 치료이벤트의 동작 방식에 따라 나누어 처리
        switch (rh.restoreTargeting)
        {
            #region 유저가 직접 선택하여 치료이벤트 실행
            case Define.restoreTargeting.userSelect:
                StartCoroutine(GAME.IGM.TC.TargettingCo
                    (caster,
                     // 이벤트 전달
                     (IBody a, IBody t) => { return Restore(a, t, rh.restoreAmount); },
                    // 타겟범위 레이어 찾기
                    FindLayer(rh)
                    ));
                break;
            #endregion

            #region 이벤트범위내 타겟들을 모두 찾아 실행
            case Define.restoreTargeting.AutoOnEvtArea:
                List<IBody> list = FindBaseTarget();
                if (list.Count == 0) { Debug.Log($"시전자[{caster.PunId}]의 타겟이 존재하지 않음"); return; }
                for (int i = 0; i < list.Count; i++)
                {
                    ActionQueue.Enqueue(Restore(caster, list[i], rh.restoreAmount));
                }
                break;
            #endregion

            #region 이벤트 범위의 타겟팅중 랜덤타겟 하나를 찾아 실행
            case Define.restoreTargeting.RandomOnEvtArea:
                list = FindBaseTarget();
                if (list.Count == 0) { Debug.Log($"시전자[{caster.PunId}]의 타겟이 존재하지 않음"); return; }
                for (int i = 0; i < list.Count ; i++)
                {
                    ActionQueue.Enqueue(Restore(caster, list[UnityEngine.Random.Range(0, list.Count)] , rh.restoreAmount));
                }
                break;
            #endregion

            #region 소환된 시전 미니언의 양옆을 타겟지정
            case Define.restoreTargeting.BothSide:
                // 현재 이벤트 실행 하수인의 위치찾기 (단 하수인수가 혼자라면 실행할 필요 X )
                List<CardField> minonList = (caster.IsMine) ? GAME.IGM.Spawn.playerMinions : GAME.IGM.Spawn.enemyMinions;
                if (minonList.Count < 2) { Debug.Log($"시전자[{caster.PunId}]의 타겟이 존재하지 않음"); return; }

                int idx = minonList.IndexOf(minonList.Find(x => x.PunId == caster.PunId));
                List<IBody> targets = new List<IBody>();
                // 자신의 왼쪽이 존재 및 시전자 본인이 아니라면 타겟 확정
                if (idx - 1 > -1 && minonList[idx - 1].PunId != caster.PunId) 
                {
                    ActionQueue.Enqueue(Restore(caster, minonList[idx - 1], rh.restoreAmount)); 
                }
                // 시전자 하수인의 오른쪽 존재하는지 찾기
                if (idx + 1 < minonList.Count && minonList[idx + 1].PunId != caster.PunId) 
                {
                    ActionQueue.Enqueue(Restore(caster, minonList[idx + 1], rh.restoreAmount));
                }
                break;
            #endregion
        }

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
                        targets.AddRange(GAME.IGM.Spawn.playerMinions);
                    }

                    else
                    {
                        targets.AddRange(GAME.IGM.Spawn.enemyMinions);
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
                        targets.Add(GAME.IGM.Hero.Player);
                    }

                    else
                    {
                        targets.Add(GAME.IGM.Hero.Enemy);
                    }
                    break;
            }

            // 시전자 자신은 치료 X
            if (targets.Find(x => x.PunId == caster.PunId) != null)
            { targets.Remove(targets.Find(x => x.PunId == caster.PunId)); }

            return targets;
        }

        // 치료실행 이벤트 코루틴
        IEnumerator Restore(IBody caster , IBody target , int amount)
        {
            target.HP = Mathf.Clamp(target.HP + amount, 0, target.OriginHp);
            yield break;
        }
    }
}