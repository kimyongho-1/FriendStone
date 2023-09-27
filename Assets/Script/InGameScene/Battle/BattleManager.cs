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
    // ����Ǵ� �̺�Ʈ�� ���������� ����
    public IEnumerator ProcessigCo()
    {
        while (true)
        {
            // ���� �̺�Ʈ ���������� ���
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
        Debug.Log($"attacker[{attacker.PunId}]�� {evtData.when}�̱⿡ {evtData.type}����");
        if (evtData is UtillHandler) { Debug.Log("Utill"); }

        switch (evtData.type) // �̺�Ʈ Ÿ�Ժ��� �з�
        {
            case Define.evtType.attack: RegisterAttHandler((AttackHandler)evtData , attacker); return;
            case Define.evtType.buff: RegisterBuffHandler((BuffHandler)evtData, attacker); return;
            case Define.evtType.utill: RegisterUtillHandler((UtillHandler)evtData, attacker); break;
            case Define.evtType.restore: RegisterRestoreHandler((RestoreHandler)evtData, attacker); break;
        }
    }

    #region ������ ���ð��Ͻ�, Ÿ�� ���̾� ã��
    string[] FindLayer(CardBaseEvtData ah) // ������ �����ϴ°��̸�, Ÿ������ ���̾� ã��
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

    // ���� �̺�Ʈ ����
    public void RegisterAttHandler(AttackHandler ah, IBody attacker)
    {
        // ������ ���� �����ϴ� Ÿ������, �ڵ� ���õǴ� Ÿ������ Ȯ��
        switch (ah.attTargeting)
        {
            // ������ ���� �����Ͻ�, �̴Ͼ� ����ó�� ���� Ÿ���� ����
            case Define.attTargeting.userSelect:
                StartCoroutine(GAME.IGM.TC.TargettingCo
                    (attacker,

                    // �������� ������ ���̴� �̺�Ʈ���� �ĺ��ϱ�
                    (ah.attType == Define.attType.Damage) ? (IBody a, IBody t) => { return AttackEvt(a, t); }
                     : (IBody a, IBody t) => { return KillEvt(a, t); },
                    // Ÿ�ٹ��� ���̾� ã��
                    FindLayer(ah)
                    ));
                break;

            case Define.attTargeting.randomOnEvtArea:
                // ������ �̺�ƮŸ�Ժ��� Ÿ�ٹ��� ã��
                IBody target = AutoExecute();
                if (target == null) { Debug.Log($"attacker[{attacker.PunId}] �� �̺�Ʈ Ÿ�� ���� X"); return; }
                
                // Ÿ�������� �̺�Ʈ ����
                if (ah.attType == Define.attType.Damage) { ActionQueue.Enqueue(AttackEvt(attacker, target)); }
                else { ActionQueue.Enqueue(KillEvt(attacker, target)); }
                break;
        }

        #region �ڵ� �����
        IBody AutoExecute()
        {
            IBody caster = null;
            // Ÿ���� ����
            List<IBody> targets = new List<IBody>();

            // Ȯ�ε� ������ � Ÿ�� : �ϼ���, ���� ��
            switch (ah.faction) 
            { 
                case Define.evtFaction.All:
                    // ��� ������ ��� ����̸� �ڽ��� ������ ��� ����� �ϳ�
                    if (ah.area == Define.evtArea.All) 
                    {
                        targets.Add(GAME.IGM.Hero.Player);
                        targets.Add(GAME.IGM.Hero.Enemy);
                        targets.AddRange(GAME.IGM.Spawn.playerMinions);
                        targets.AddRange(GAME.IGM.Spawn.enemyMinions);

                        // ��� �������� ������ �ڽ��� ���ܽ�Ű��
                        caster = targets.Find(x => x.PunId == attacker.PunId);
                        if (caster != null)
                        { targets.Remove(caster); }

                        return targets[UnityEngine.Random.Range(0, targets.Count)];
                    }
                    return null;
                case Define.evtFaction.Minion:
                    // �� ������ ��� �ϼ����� ���� ( ������ �ڽ��� ���� )
                    if (ah.area == Define.evtArea.All)
                    {
                        targets.AddRange(GAME.IGM.Spawn.playerMinions);
                        targets.AddRange(GAME.IGM.Spawn.enemyMinions);
                    }
                    // ������ ������ �ϼ��ε�
                    else if (ah.area == Define.evtArea.Player)
                    {
                        targets.AddRange((attacker.IsMine) ?
                            GAME.IGM.Spawn.playerMinions : GAME.IGM.Spawn.enemyMinions);
                    }
                    // �������� �� �ϼ��ε���
                    else
                    {
                        targets.AddRange((attacker.IsMine) ?
                            GAME.IGM.Spawn.enemyMinions : GAME.IGM.Spawn.playerMinions);
                    }
                    // ��� �������� ������ �ڽ��� ���ܽ�Ű��
                    caster = targets.Find(x => x.PunId == attacker.PunId);
                    if (caster != null)
                    { targets.Remove(caster); }
                    return targets[UnityEngine.Random.Range(0, targets.Count)];
                    
                case Define.evtFaction.Hero:
                    // �ο����� Ÿ�� �������
                    if (ah.area == Define.evtArea.All)
                    {
                        targets.Add(GAME.IGM.Hero.Player);
                        targets.Add(GAME.IGM.Hero.Enemy);
                    }
                    // �������� ������
                    else if (ah.area == Define.evtArea.Player)
                    {
                        targets.Add((attacker.IsMine) ? GAME.IGM.Hero.Player : GAME.IGM.Hero.Enemy);
                    }
                    // �������� �� ������
                    else
                    {
                        targets.Add((attacker.IsMine) ? GAME.IGM.Hero.Enemy : GAME.IGM.Hero.Player);
                    }
                    return targets[UnityEngine.Random.Range(0, targets.Count)];
                default: return null;
            }

        }
        #endregion

        #region �̺�Ʈ �ڷ�ƾ
        IEnumerator AttackEvt(IBody attacker, IBody target)
        {
            // ����ü ȣ��
            ParticleSystem pj = FX.GetPJ;

            // �������� ��ġ���� �����ϵ��� ��ġ �ʱ�ȭ
            pj.transform.position = attacker.Pos;
            pj.gameObject.SetActive(true);
            Vector3 start = attacker.Pos;
            Vector3 dest = target.Pos;
            Vector3 dir = (dest - start).normalized; // ���⺤��
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
        { // ����ü ȣ��
            ParticleSystem pj = FX.GetPJ;

            // �������� ��ġ���� �����ϵ��� ��ġ �ʱ�ȭ
            pj.transform.position = attacker.Pos;
            pj.gameObject.SetActive(true);
            Vector3 start = attacker.Pos;
            Vector3 dest = target.Pos;
            Vector3 dir = (dest - start).normalized; // ���⺤��
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
            #region �̺�Ʈ �����ͳ� ������ Ÿ�ٵ鿡�� �̺�Ʈ ����
            case Define.buffTargeting.autoOnEvtArea:
                List<IBody> targetList = FindBaseRange(bh.area);
                if (targetList.Count == 0) { Debug.Log($"caster[{caster.PunId}] �� Ÿ���� ���� "); return; }
                // Ÿ�� ������ŭ, �̺�Ʈ ����
                for (int i = 0; i < targetList.Count; i++)
                {
                    ActionQueue.Enqueue(Buff(caster, targetList[i], bh));
                }
                break;
            #endregion

            #region ������ ���� Ÿ���� �����Ͽ� ����
            case Define.buffTargeting.userSelect:
                StartCoroutine(GAME.IGM.TC.TargettingCo
                    (caster,
                     // �̺�Ʈ ����
                     (IBody a, IBody t) => { return Buff(a, t, bh); },
                    // Ÿ�ٹ��� ���̾� ã��
                    FindLayer(bh)
                    ));
                break;
            #endregion

            #region ���� Ÿ���� �ϳ� �����Ͽ� �̺�Ʈ ����
            case Define.buffTargeting.randomOnEvtArea:
                targetList = FindBaseRange(bh.area);
                if (targetList.Count == 0) { Debug.Log($"caster[{caster.PunId}] �� Ÿ���� ���� "); return; }
                ActionQueue.Enqueue(Buff(caster, targetList[UnityEngine.Random.Range(0, targetList.Count)], bh));
                break;
            #endregion

            #region ���� �ϼ��� �翷���� ����
            case Define.buffTargeting.BothSide:
                // ���� �̺�Ʈ ���� �ϼ����� ��ġã�� (�� �ϼ��μ��� ȥ�ڶ�� ������ �ʿ� X )
                List<CardField> minonList = (caster.IsMine) ? GAME.IGM.Spawn.playerMinions : GAME.IGM.Spawn.enemyMinions;
                if (minonList.Count < 2) { Debug.Log($"������[{caster.PunId}]�� Ÿ���� �������� ����"); return; }

                int idx = minonList.IndexOf(minonList.Find(x => x.PunId == caster.PunId));
                List<IBody> targets = new List<IBody>();
                // �ڽ��� ������ ���� �� ������ ������ �ƴ϶�� Ÿ�� Ȯ��
                if (idx - 1 > -1 && minonList[idx - 1].PunId != caster.PunId)
                {
                    ActionQueue.Enqueue(Buff(caster, minonList[idx - 1], bh));
                }
                // ������ �ϼ����� ������ �����ϴ��� ã��
                if (idx + 1 < minonList.Count && minonList[idx + 1].PunId != caster.PunId)
                {
                    ActionQueue.Enqueue(Buff(caster, minonList[idx + 1], bh));
                }
                break;
            #endregion

            #region Ư�� ī��ID�� �ϼ��� �� ��ŭ, �ϼ��ο��� ����
            case Define.buffTargeting.someID:
                // �տ� ������ ������̶��, ���� Caster�� �ڵ�ī���°��� �˼� �ִ�
                CardHand ch = caster.TR.GetComponent<CardHand>();

                // �տ� ������ ������̶��
                if (bh.when == Define.evtWhen.onHand)
                {
                    // �տ� �ִ� �ڵ�ī���ϋ�  Ư���������� ������ �̺�Ʈ ���� �ǽ�
                    ch.HandCardChanged += BuffHandle(ch, ch.PunId, bh);
                    // �̺�Ʈ ��Ͻ� ���� ����
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

        // �̺�Ʈ ������ �ش� �ϴ� Ÿ�ٵ� ����
        List<IBody> FindBaseRange(Define.evtArea area)
        {
            List<IBody> list = new List<IBody>();
            switch (bh.faction)
            {
                case Define.evtFaction.All:
                    // ��ü ������ ��ü Ÿ���̶��
                    if (area == Define.evtArea.All)
                    {
                        list.AddRange(GAME.IGM.Spawn.playerMinions);
                        list.AddRange(GAME.IGM.Spawn.enemyMinions);
                        list.Add(GAME.IGM.Hero.Player);
                        list.Add(GAME.IGM.Hero.Enemy);
                    }

                    // �Ʊ� ��ü �������
                    else if (area == Define.evtArea.Player)
                    { list.Add(GAME.IGM.Hero.Player); list.AddRange(GAME.IGM.Spawn.playerMinions); }

                    // ���� ��ü �������
                    else
                    { list.Add(GAME.IGM.Hero.Enemy); list.AddRange(GAME.IGM.Spawn.playerMinions); }
                    break;
                case Define.evtFaction.Minion:
                    // ��ü ������ ��ü Ÿ���̶��
                    if (area == Define.evtArea.All)
                    {
                        list.AddRange(GAME.IGM.Spawn.playerMinions);
                        list.AddRange(GAME.IGM.Spawn.enemyMinions);
                    }

                    // �Ʊ� ��ü �������
                    else if (area == Define.evtArea.Player)
                    { list.AddRange(GAME.IGM.Spawn.playerMinions); }

                    // ���� ��ü �������
                    else
                    { list.AddRange(GAME.IGM.Spawn.playerMinions); }
                    break;
                case Define.evtFaction.Hero:
                    // ��ü ������ �������̶��
                    if (area == Define.evtArea.All)
                    {
                        list.Add(GAME.IGM.Hero.Player);
                        list.Add(GAME.IGM.Hero.Enemy);
                    }

                    // �Ʊ� ������
                    else if (area == Define.evtArea.Player)
                    { list.Add(GAME.IGM.Hero.Player); }

                    // �� ������
                    else
                    { list.Add(GAME.IGM.Hero.Enemy); }
                    break;
            }

            return list;
        }

        // ���� �̺�Ʈ �������ֱ�
        Action<int,bool> BuffHandle(CardHand target,int ownerPunID,  BuffHandler bh)
        {
            // ��� �ο� ȿ������
            switch (bh.buffType)
            {
                case Define.buffType.att:
                    return (int id, bool IsMine) =>
                    {
                        // ����ó��, Ÿ���ϼ����� ������ �����ϴµ� ���� ��ȯ�ϴ� �ϼ����� �������ο� ����ġ�� �̺�Ʈ ���� X
                        if (IsMine == true && bh.area == Define.evtArea.Enemy) { return; }
                        if (IsMine == false && bh.area == Define.evtArea.Player) { return; }

                        int origin = target.OriginAtt;
                        int[] relatedID = bh.relatedIds;

                        // �̴Ͼ�ī�� ��ȯ������ ��Ȯ�� ī��Ÿ�� ������ �ƴ϶�� �̺�Ʈ ���� ���
                        if (bh.relatedIds.Length > 0 && !relatedID.Contains(id)) { return; }

                        int count = 0;
                        // ���� ī��ѹ��� ������, �ʵ��� ���� �ѹ� �ϼ��δ� ���� �����ֱ�
                        if (bh.relatedIds.Length > 0)
                        {
                            for (int i = 0; i < relatedID.Length; i++)
                            {
                                count += GAME.IGM.allIBody.FindAll(x => x.PunId == relatedID[i]).Count();
                            }
                        }

                        // count �� 0 �̸�, �̺�Ʈ ������ ��� �ϼ������� ����
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
                                target.Stat.text = $"<color=yellow>ATT {mc.att} <color=red>HP {mc.hp} <color=black>����";
                                break;
                            case Define.cardType.spell: break;
                            case Define.cardType.weapon:
                                WeaponCardData wData = (WeaponCardData)target.data;
                                wData.att = origin + bh.buffAtt * count;
                                target.Stat.text = $"<color=yellow>ATT {wData.att} <color=red>dur {wData.durability} <color=black>����";
                                break;
                        }
                    };

                case Define.buffType.hp:
                    return (int id,bool IsMine) =>
                    {
                        // ����ó��, Ÿ���ϼ����� ������ �����ϴµ� ���� ��ȯ�ϴ� �ϼ����� �������ο� ����ġ�� �̺�Ʈ ���� X
                        if (IsMine == true && bh.area == Define.evtArea.Enemy) { return; }
                        if (IsMine == false && bh.area == Define.evtArea.Player) { return; }
                        int origin = target.OriginHp;
                        int[] relatedID = bh.relatedIds;

                        // �̴Ͼ�ī�� ��ȯ������ ��Ȯ�� ī��Ÿ�� ������ �ƴ϶�� �̺�Ʈ ���� ���
                        if (bh.relatedIds.Length > 0 && !relatedID.Contains(id)) { return; }

                        int count = 0;
                        // ���� ī��ѹ��� ������, �ʵ��� ���� �ѹ� �ϼ��δ� ���� �����ֱ�
                        if (bh.relatedIds.Length > 0 )
                        {
                            for (int i = 0; i < relatedID.Length; i++)
                            {
                                count += GAME.IGM.allIBody.FindAll(x => x.PunId == relatedID[i]).Count();
                            }
                        }

                        // count �� 0 �̸�, �̺�Ʈ ������ ��� �ϼ������� ����
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
                                target.Stat.text = $"<color=yellow>ATT {mc.att} <color=red>HP {mc.hp} <color=black>����";
                                break;
                            case Define.cardType.spell: break;
                            case Define.cardType.weapon:
                                WeaponCardData wData = (WeaponCardData)target.data;
                                wData.durability = origin + bh.buffHp * count;
                                target.Stat.text = $"<color=yellow>ATT {wData.att} <color=red>dur {wData.durability} <color=black>����";
                                break;
                        }
                    };

                case Define.buffType.atthp:
                    return (int id, bool IsMine) =>
                    {
                        // ����ó��, Ÿ���ϼ����� ������ �����ϴµ� ���� ��ȯ�ϴ� �ϼ����� �������ο� ����ġ�� �̺�Ʈ ���� X
                        if (IsMine == true && bh.area == Define.evtArea.Enemy) { return; }
                        if (IsMine == false && bh.area == Define.evtArea.Player) { return; }
                        int[] relatedID = bh.relatedIds;
                        int count = 0;
                        
                        // �̴Ͼ�ī�� ��ȯ������ ��Ȯ�� ī��Ÿ�� ������ �ƴ϶�� �̺�Ʈ ���� ���
                        if (bh.relatedIds.Length > 0 && !relatedID.Contains(id)) { return; }

                        // ���� ī��ѹ��� ������, �ʵ��� ���� �ѹ� �ϼ��δ� ���� �����ֱ�
                        if (bh.relatedIds.Length > 0 )
                        {
                            for (int i = 0; i < relatedID.Length; i++)
                            {
                                count += GAME.IGM.allIBody.FindAll(x => x.PunId == relatedID[i]).Count();
                            }
                        }

                        // count �� 0 �̸�, �̺�Ʈ ������ ��� �ϼ������� ����
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
                                target.Stat.text = $"<color=yellow>ATT {mc.att} <color=red>HP {mc.hp} <color=black>����";
                                break;
                            case Define.cardType.spell: break;
                            case Define.cardType.weapon:
                                WeaponCardData wData = (WeaponCardData)target.data;
                                wData.att = target.OriginAtt + bh.buffAtt * count; 
                                wData.durability = target.OriginHp + bh.buffHp * count;
                                target.Stat.text = $"<color=yellow>ATT {wData.att} <color=red>dur {wData.durability} <color=black>����";
                                break;
                        }
                    };

                case Define.buffType.cost:
                    return (int id, bool IsMine) =>
                    {
                        // ����ó��, Ÿ���ϼ����� ������ �����ϴµ� ���� ��ȯ�ϴ� �ϼ����� �������ο� ����ġ�� �̺�Ʈ ���� X
                        if (IsMine == true && bh.area == Define.evtArea.Enemy) { return; }
                        if (IsMine == false && bh.area == Define.evtArea.Player) { return; }
                        int[] relatedID = bh.relatedIds;
                        int count = 0;
                        
                        // �̴Ͼ�ī�� ��ȯ������ ��Ȯ�� ī��Ÿ�� ������ �ƴ϶�� �̺�Ʈ ���� ���
                        if (bh.relatedIds.Length > 0 && !relatedID.Contains(id)) { return; }

                        // ���� ī��ѹ��� ������, �ʵ��� ���� �ѹ� �ϼ��δ� ���� �����ֱ�
                        if (bh.relatedIds.Length > 0 )
                        {
                            for (int i = 0; i < relatedID.Length; i++)
                            {
                                count += GAME.IGM.allIBody.FindAll(x => x.PunId == relatedID[i]).Count();
                            }
                        }

                        // related �� 0 �̸�, �̺�Ʈ ������ ��� �ϼ������� ����
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

        // ������ ���� �ڷ�ƾ
        IEnumerator Buff(IBody caster, IBody target, BuffHandler bh )
        {
            // ��� �ο� ȿ������
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
        switch (uh.utillType) // ���Ǽ� ��Ÿ �̺�Ʈ�� ���� �������� ���� �ڵ�����ǵ�
        {
            case Define.utillType.draw: // ��ο� �̺�Ʈ
                DrawEvt(uh);
                break;
            case Define.utillType.find: // �߰� �̺�Ʈ
                Debug.Log("�߰� �̺�Ʈ");
                ActionQueue.Enqueue(FindEvt(uh));
                break;
            case Define.utillType.acquisition: // ȹ�� �̺�Ʈ
                Debug.Log("ȹ�� �̺�Ʈ");
                ActionQueue.Enqueue(AcquisitionEvt(uh,caster));
                break;
        }

        // ��ο� �̺�Ʈ
        void DrawEvt(UtillHandler uh)
        {
            switch (uh.area)
            {
                #region ��븦 ��ο� ��Ű�� �̺�Ʈ
                case Define.evtArea.Enemy:
                    GAME.IGM.Packet.SendDoDraw(uh.utillAmount, false);
                    return;
                #endregion

                #region ���� ��ο��ϴ� �̺�Ʈ
                case Define.evtArea.Player: 
                    ActionQueue.Enqueue(GAME.IGM.Hand.CardDrawing(uh.utillAmount)); return;
                #endregion

                #region ���� �� ��� ��ο� �ϴ� �̺�Ʈ
                case Define.evtArea.All:
                    GAME.IGM.Packet.SendDoDraw(uh.utillAmount, true);
                    return;
                #endregion
            }
        }

        // �߰� �̺�Ʈ
        IEnumerator FindEvt(UtillHandler uh)
        {
            // �̹� 10���̸�, ī�带 ������ ���⿡ ���� ���
            if (GAME.IGM.Hand.PlayerHand.Count == 10) { yield break; }

            // �߰� �̺�Ʈ �����, ���濡�� ���� �߰��̺�Ʈ ������ ����ȭ�Ͽ� �����ֱ�
            GAME.IGM.Packet.SendFindEvt();

            // �߰� �̺�Ʈ �غ� �� ���� (�ڷ�ƾ�� ������Ʈ�� ���������� ������ �ȵǾ�, �Լ��� ���� ������ �غ� �� �ڷ�ƾ ����)
            GAME.IGM.FindEvt.ReadyFindEvt(uh.relatedCards);

            // �߰� �ڷ�ƾ ���������� (������ �߰�ī����� �ϳ��� �����ҋ����� ���)
            yield return new WaitUntil(() => (GAME.IGM.FindEvt.CurrSelected == true));
        }
        
        // ȹ�� �̺�Ʈ
        IEnumerator AcquisitionEvt(UtillHandler uh, IBody caster )
        {
            // ȹ���ϴ� ��ġ��, �ִ� 10���� �Ѿ���� Ȯ�� (������ ������ ����ŭ�� ȹ��)
            int count = (GAME.IGM.Hand.PlayerHand.Count + uh.relatedCards.Length <= 10) 
                ? uh.relatedCards.Length : (10 - (GAME.IGM.Hand.PlayerHand.Count));

            // ��뿡�� ���� ������ �ݳѹ��� ī����̵�迭
            int casterPunID = caster.PunId;
            int[] sendArray = new int[count];
            int[] punArray = new int[count];
            // ����ī��� ����
            for (int i = 0; i < count; i++)
            {
                // ���ҽ� �Ŵ����� ��θ� ��ȯ �޴� ��ųʸ� ���� ī��Ÿ�԰� ī�嵥���� ã��
                Define.cardType type = GAME.Manager.RM.PathFinder.Dic[uh.relatedCards[i]].type;
                string jsonFile = GAME.Manager.RM.PathFinder.Dic[uh.relatedCards[i]].GetJson();
                CardData card = null;  
                // Ȯ�ε� ī��Ÿ������, ���� ī��Ÿ������ Ŭ����ȭ
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

                // �ش� ī���ȣ�� ������ ����
                CardHand ch =
                GameObject.Instantiate(Resources.Load<CardHand>("Prefab/InGamePrefab/CardHand"), GAME.IGM.Hand.PlayerHandGO.transform);
                ch.Init(card, caster.IsMine);
                ch.PunId = GAME.IGM.Hand.CreatePunNumber();

                // ��뿡�� ������ ī��ĺ��ڿ� �ݽĺ��ڵ� �����ϱ�
                sendArray[i] = uh.relatedCards[i];
                punArray[i] = ch.PunId;

                ch.transform.localScale = Vector3.one;
                ch.transform.localPosition = caster.TR.position;
                GAME.IGM.Hand.PlayerHand.Add(ch);
                yield return null;
            }

            // ��뿡�� ���� ȹ���̺�Ʈ�� �����ϱ�
            GAME.IGM.Packet.SendAcquisition(casterPunID,sendArray,punArray);

            // ī�� ����
            yield return StartCoroutine(GAME.IGM.Hand.CardAllignment(true));
        }
    }

    public void RegisterRestoreHandler(RestoreHandler rh, IBody caster)
    {
        // ġ���̺�Ʈ�� ���� ��Ŀ� ���� ������ ó��
        switch (rh.restoreTargeting)
        {
            #region ������ ���� �����Ͽ� ġ���̺�Ʈ ����
            case Define.restoreTargeting.userSelect:
                StartCoroutine(GAME.IGM.TC.TargettingCo
                    (caster,
                     // �̺�Ʈ ����
                     (IBody a, IBody t) => { return Restore(a, t, rh.restoreAmount); },
                    // Ÿ�ٹ��� ���̾� ã��
                    FindLayer(rh)
                    ));
                break;
            #endregion

            #region �̺�Ʈ������ Ÿ�ٵ��� ��� ã�� ����
            case Define.restoreTargeting.AutoOnEvtArea:
                List<IBody> list = FindBaseTarget();
                if (list.Count == 0) { Debug.Log($"������[{caster.PunId}]�� Ÿ���� �������� ����"); return; }
                for (int i = 0; i < list.Count; i++)
                {
                    ActionQueue.Enqueue(Restore(caster, list[i], rh.restoreAmount));
                }
                break;
            #endregion

            #region �̺�Ʈ ������ Ÿ������ ����Ÿ�� �ϳ��� ã�� ����
            case Define.restoreTargeting.RandomOnEvtArea:
                list = FindBaseTarget();
                if (list.Count == 0) { Debug.Log($"������[{caster.PunId}]�� Ÿ���� �������� ����"); return; }
                for (int i = 0; i < list.Count ; i++)
                {
                    ActionQueue.Enqueue(Restore(caster, list[UnityEngine.Random.Range(0, list.Count)] , rh.restoreAmount));
                }
                break;
            #endregion

            #region ��ȯ�� ���� �̴Ͼ��� �翷�� Ÿ������
            case Define.restoreTargeting.BothSide:
                // ���� �̺�Ʈ ���� �ϼ����� ��ġã�� (�� �ϼ��μ��� ȥ�ڶ�� ������ �ʿ� X )
                List<CardField> minonList = (caster.IsMine) ? GAME.IGM.Spawn.playerMinions : GAME.IGM.Spawn.enemyMinions;
                if (minonList.Count < 2) { Debug.Log($"������[{caster.PunId}]�� Ÿ���� �������� ����"); return; }

                int idx = minonList.IndexOf(minonList.Find(x => x.PunId == caster.PunId));
                List<IBody> targets = new List<IBody>();
                // �ڽ��� ������ ���� �� ������ ������ �ƴ϶�� Ÿ�� Ȯ��
                if (idx - 1 > -1 && minonList[idx - 1].PunId != caster.PunId) 
                {
                    ActionQueue.Enqueue(Restore(caster, minonList[idx - 1], rh.restoreAmount)); 
                }
                // ������ �ϼ����� ������ �����ϴ��� ã��
                if (idx + 1 < minonList.Count && minonList[idx + 1].PunId != caster.PunId) 
                {
                    ActionQueue.Enqueue(Restore(caster, minonList[idx + 1], rh.restoreAmount));
                }
                break;
            #endregion
        }

        // ������ �̺�Ʈ�� �������� �⺻���� ã��
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

            // ������ �ڽ��� ġ�� X
            if (targets.Find(x => x.PunId == caster.PunId) != null)
            { targets.Remove(targets.Find(x => x.PunId == caster.PunId)); }

            return targets;
        }

        // ġ����� �̺�Ʈ �ڷ�ƾ
        IEnumerator Restore(IBody caster , IBody target , int amount)
        {
            target.HP = Mathf.Clamp(target.HP + amount, 0, target.OriginHp);
            yield break;
        }
    }
}