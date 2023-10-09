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
    // ����Ǵ� �̺�Ʈ�� ���������� ����
    public IEnumerator ProcessigCo()
    {
        while (true)
        {
            // ���� �̺�Ʈ ���������� ���
            yield return new WaitUntil(()=>(ActionQueue.Count > 0));
            currCo = ActionQueue.Dequeue();
            yield return StartCoroutine(currCo);

            // ���� Ÿ���� ���̸� , Ÿ������ ���������� ���
            yield return new WaitUntil(() => (GAME.IGM.TC.LR.gameObject.activeSelf == false));
            // ���� ������ ���� �̺�Ʈ�� �������̸� ���
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

    // ������ �̺�Ʈ �ڷ�ƾ�� ��ȯ�ϴ� �Լ�
    public IEnumerator Evt(CardBaseEvtData evtData, IBody attacker, IBody searchedTarget = null)
    {
        Debug.Log($"attacker[{attacker.PunId}]�� {evtData.when}�̱⿡ {evtData.type}����");
        if (evtData is UtillHandler) { Debug.Log("Utill"); }
        switch (evtData.type) // �̺�Ʈ Ÿ�Ժ��� �з�
        {
            case Define.evtType.attack:  return RegisterAttHandler((AttackHandler)evtData, attacker, searchedTarget);
            case Define.evtType.buff: return RegisterBuffHandler((BuffHandler)evtData, attacker, searchedTarget); 
            case Define.evtType.utill: return RegisterUtillHandler((UtillHandler)evtData, attacker);
            case Define.evtType.restore: return RegisterRestoreHandler((RestoreHandler)evtData, attacker, searchedTarget);
            default: return null;
        }
    }

    #region ������ ���ð��Ͻ�, Ÿ�� ���̾� ã��
    public string[] FindLayer(CardBaseEvtData ah) // ������ �����ϴ°��̸�, Ÿ������ ���̾� ã��
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
    public IEnumerator RegisterAttHandler(AttackHandler ah, IBody attacker, IBody searchedTarget)
    {
        // ������ ���� �����ϴ� Ÿ������, �ڵ� ���õǴ� Ÿ������ Ȯ��
        // �ֹ�ī���� ��� ���� Ÿ�����Ͻ� �ڽ��� ������ġ���� �����ϵ��� ����
        // ������ ���� �����Ͻ�, �̴Ͼ� ����ó�� ���� Ÿ���� ����
        if (ah.targeting == Define.evtTargeting.Select)
        {
            return AttackEvt((attacker.objType == Define.ObjType.Minion) ?
                attacker : GAME.IGM.Hero.Player
                , searchedTarget, ah.attAmount, ah.attType);
        }

        else
        {
            IBody target = AutoExecute();
            if (target == null) { Debug.Log($"attacker[{attacker.PunId}] �� �̺�Ʈ Ÿ�� ���� X"); return null; }

            // Ÿ�������� �̺�Ʈ ����
            return AttackEvt((attacker.objType == Define.ObjType.Minion) ?
                attacker : GAME.IGM.Hero.Player, target, ah.attAmount, ah.attType, (ah.when == Define.evtWhen.onDead));
        }

        #region �ڵ� �����
        IBody AutoExecute()
        {
            // Ÿ���� ����
            List<IBody> targets = new List<IBody>();

            // Ȯ�ε� ������ � Ÿ�� : �ϼ���, ���� ��
            switch (ah.faction) 
            { 
                case Define.evtFaction.All:
                    // ��� ������ ��� ����̸� �ڽ��� ������ ��� ����� �ϳ�
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
                    attacker = targets.Find(x => x.PunId == attacker.PunId);
                    if (attacker != null)
                    { targets.Remove(attacker); }
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

    }

    public IEnumerator RegisterBuffHandler(BuffHandler bh, IBody caster, IBody searchedTarget)
    {
        #region ������ ���� Ÿ���� �����Ͽ� ����
        if (bh.targeting == Define.evtTargeting.Select)
        {
            return Buff(caster, searchedTarget, bh);
           
        }
        #endregion

        #region �׿� �տ��� ���� �ڵ������̸� �ڵ������̸�, �з��� �°� ����
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
                #region �̺�Ʈ �����ͳ� ������ Ÿ�ٵ鿡�� �̺�Ʈ ����
                case Define.buffAutoMode.autoOnEvtArea:
                    List<IBody> targetList = FindBaseRange(bh.area);
                    if (targetList.Count == 0) { Debug.Log($"caster[{caster.PunId}] �� Ÿ���� ���� "); return null; }
                    
                    return MoBuff(targetList);
                  
                #endregion

                #region ���� Ÿ���� �ϳ� �����Ͽ� �̺�Ʈ ����
                case Define.buffAutoMode.randomOnEvtArea:
                    targetList = FindBaseRange(bh.area);
                    if (targetList.Count == 0) { Debug.Log($"caster[{caster.PunId}] �� Ÿ���� ���� "); return null; }
                    return Buff(caster, targetList[UnityEngine.Random.Range(0, targetList.Count)], bh);
                #endregion

                #region ���� �ϼ��� �翷���� ����
                case Define.buffAutoMode.BothSide:
                    // ���� �̺�Ʈ ���� �ϼ����� ��ġã�� (�� �ϼ��μ��� ȥ�ڶ�� ������ �ʿ� X )
                    List<CardField> minonList = (caster.IsMine) ? GAME.IGM.Spawn.playerMinions : GAME.IGM.Spawn.enemyMinions;
                    if (minonList.Count < 2) { Debug.Log($"������[{caster.PunId}]�� Ÿ���� �������� ����"); return null; }
                    List<IBody> list = new List<IBody>();
                    int idx = minonList.IndexOf(minonList.Find(x => x.PunId == caster.PunId));
                    // �ڽ��� ������ ���� �� ������ ������ �ƴ϶�� Ÿ�� Ȯ��
                    if (idx - 1 > -1 && minonList[idx - 1].PunId != caster.PunId )
                    {
                        list.Add(minonList[idx - 1]);
                        Debug.Log($"�翷 �ϼ���, ���ʿ��� ���� Ÿ�� {minonList[idx - 1]}[{minonList[idx - 1].PunId}]");
                        //ActionQueue.Enqueue(Buff(caster, minonList[idx - 1], bh));
                    }
                    // ������ �ϼ����� ������ �����ϴ��� ã��
                    if (idx + 1 < minonList.Count && minonList[idx + 1].PunId != caster.PunId)
                    {
                        list.Add(minonList[idx + 1]);
                        Debug.Log($"�翷 �ϼ���, �����ʿ��� ���� Ÿ�� {minonList[idx + 1]}[{minonList[idx+1].PunId}]");
                        //ActionQueue.Enqueue(Buff(caster, minonList[idx + 1], bh));
                    }
                    if (list.Count == 0) { return null; }
                    return MoBuff(list);
                #endregion

                #region Ư�� ī��ID�� �ϼ��� �� ��ŭ, �ϼ��ο��� ����
                case Define.buffAutoMode.someID:
                    
                    // �տ� ������ ������̶��
                    if (bh.when == Define.evtWhen.onHand)
                    {
                        // �տ� ������ ������̶��, ���� Caster�� �ڵ�ī���°��� �˼� �ִ�
                        CardHand ch = caster.TR.GetComponent<CardHand>();

                        // �տ� �ִ� �ڵ�ī���ϋ�  Ư���������� ������ �̺�Ʈ ���� �ǽ�
                        ch.HandCardChanged += BuffHandle(ch, ch.PunId, bh);
                        // �̺�Ʈ ��Ͻ� ���� ����
                        ch.HandCardChanged.Invoke(ch.Data.cardIdNum, true);
                        return null;
                    }

                    // �տ��� ����, �� �ʵ� �̴Ͼ��� ��ü
                    else
                    {
                        CardField cf = caster.TR.GetComponent<CardField>();
                        int count = 0;
                        // ���� ī��ѹ��� ������, �ʵ��� ���� �ѹ� �ϼ��δ� ���� �����ֱ�
                        if (bh.relatedIds.Length > 0)
                        {
                            for (int i = 0; i < bh.relatedIds.Length; i++)
                            {
                                count += GAME.IGM.allIBody.FindAll(x => x.PunId == bh.relatedIds[i] && x.HP > 0).Count();
                            }
                        }

                        // count �� 0 �̸�, �̺�Ʈ ������ ��� �ϼ������� ����
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

                        // �̺�Ʈ ����
                        return Buff(caster, caster, bh, count);
                    }
                #endregion

                default: return null;
            }
        }
        #endregion

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
            list.RemoveAll(x => x.HP <= 0);
            return list;
        }

        // ���� �̺�Ʈ �������ֱ�
        Action<int, bool> BuffHandle(CardHand target, int ownerPunID, BuffHandler bh)
        {
            // ��� �ο� ȿ������
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

        // �տ� ������ ������ �����̺�Ʈ �з��Լ�
        int SortHandEvt(ref bool IsMine, ref BuffHandler bh, ref CardHand target, ref int id)
        {
            if (IsMine == true && bh.area == Define.evtArea.Enemy) { return 0; }
            if (IsMine == false && bh.area == Define.evtArea.Player) { return 0; }
            int origin = target.OriginAtt;
            int[] relatedID = bh.relatedIds;

            // �̴Ͼ�ī�� ��ȯ������ ��Ȯ�� ī��Ÿ�� ������ �ƴ϶�� �̺�Ʈ ���� ���
            if (bh.relatedIds.Length > 0 && !relatedID.Contains(id)) { return 0; }

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
        switch (uh.utillType) // ���Ǽ� ��Ÿ �̺�Ʈ�� ���� �������� ���� �ڵ�����ǵ�
        {
            case Define.utillType.draw: // ��ο� �̺�Ʈ
                return DrawEvt(uh);
            case Define.utillType.find: // �߰� �̺�Ʈ
                Debug.Log("�߰� �̺�Ʈ");
                return FindEvt(uh);
            case Define.utillType.acquisition: // ȹ�� �̺�Ʈ
                Debug.Log("ȹ�� �̺�Ʈ");
                return AcquisitionEvt(uh, caster);
            default: return null;
        }

        // ��ο� �̺�Ʈ
        IEnumerator DrawEvt(UtillHandler uh)
        {
            switch (uh.area)
            {
                #region ��븦 ��ο� ��Ű�� �̺�Ʈ
                case Define.evtArea.Enemy:
                    return enemyDraw();
                    IEnumerator enemyDraw()
                    { yield return null; GAME.IGM.Packet.SendDoDraw(uh.utillAmount, false); };
                #endregion

                #region ���� ��ο��ϴ� �̺�Ʈ
                case Define.evtArea.Player:
                    return playerDraw();
                    IEnumerator playerDraw()
                    { yield return null; GAME.IGM.Hand.CardDrawing(uh.utillAmount); }
                #endregion

                #region ���� �� ��� ��ο� �ϴ� �̺�Ʈ
                case Define.evtArea.All:
                    return bothDraw();
                    IEnumerator bothDraw()
                    { yield return null; GAME.IGM.Packet.SendDoDraw(uh.utillAmount, true); }
                #endregion
                default:return null;
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

                ch.transform.localScale = Vector3.zero;
                ch.transform.localPosition = caster.TR.position;
                
                GAME.IGM.Hand.PlayerHand.Add(ch);
                yield return null;
            }

            // ��뿡�� ���� ȹ���̺�Ʈ�� �����ϱ�
            GAME.IGM.Packet.SendAcquisition(caster.objType , casterPunID, sendArray, punArray);

        }
    }

    public IEnumerator RegisterRestoreHandler(RestoreHandler rh, IBody caster, IBody searchedTarget)
    {

        #region ������ ���� �����Ͽ� ġ���̺�Ʈ ����
        if (rh.targeting == Define.evtTargeting.Select)
        {
            return Restore(caster, searchedTarget, rh.restoreAmount);
        }
        #endregion

        #region �ڵ�������̸�, �з��� �°� ����
        else
        {
            IEnumerator MoRestore(List<IBody> list, bool isDeathRattle = false)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    yield return StartCoroutine(Restore(caster, list[i], rh.restoreAmount, isDeathRattle));
                }
            }

            // ġ���̺�Ʈ�� ���� ��Ŀ� ���� ������ ó��
            switch (rh.restoreAutoMode)
            {
                #region �̺�Ʈ������ Ÿ�ٵ��� ��� ã�� ����
                case Define.restoreAutoMode.AutoOnEvtArea:
                    List<IBody> list = FindBaseTarget();
                    if (list.Count == 0) { Debug.Log($"������[{caster.PunId}]�� Ÿ���� �������� ����"); return null; }
                    return MoRestore(list, (rh.when == Define.evtWhen.onDead));
                #endregion

                #region �̺�Ʈ ������ Ÿ������ ����Ÿ�� �ϳ��� ã�� ����
                case Define.restoreAutoMode.RandomOnEvtArea:
                    list = FindBaseTarget();
                    if (list.Count == 0) { Debug.Log($"������[{caster.PunId}]�� Ÿ���� �������� ����"); return null; }
                    return MoRestore(list, (rh.when == Define.evtWhen.onDead));
                #endregion

                #region ��ȯ�� ���� �̴Ͼ��� �翷�� Ÿ������
                case Define.restoreAutoMode.BothSide:
                    // ���� �̺�Ʈ ���� �ϼ����� ��ġã�� (�� �ϼ��μ��� ȥ�ڶ�� ������ �ʿ� X )
                    List<CardField> minonList = (caster.IsMine) ? GAME.IGM.Spawn.playerMinions : GAME.IGM.Spawn.enemyMinions;
                    if (minonList.Count < 2) { Debug.Log($"������[{caster.PunId}]�� Ÿ���� �������� ����"); return null; }
                    list = new List<IBody>();
                    int idx = minonList.IndexOf(minonList.Find(x => x.PunId == caster.PunId));
                    List<IBody> targets = new List<IBody>();
                    // �ڽ��� ������ ���� �� ������ ������ �ƴ϶�� Ÿ�� Ȯ��
                    if (idx - 1 > -1 && minonList[idx - 1].PunId != caster.PunId)
                    {
                        list.Add(minonList[idx - 1]);
                    }
                    // ������ �ϼ����� ������ �����ϴ��� ã��
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

            // ������ �ڽ��� ġ�� X
            if (targets.Find(x => x.PunId == caster.PunId) != null)
            { targets.Remove(targets.Find(x => x.PunId == caster.PunId)); }
            targets.RemoveAll(x=>x.HP <= 0);
            return targets;
        }

    }


    // ������ ���� �ڷ�ƾ
    public IEnumerator Buff(IBody caster, IBody target, BuffHandler bh, int multiPly = 1)
    {
        // ��� �ο� ȿ������
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
        Debug.Log($"Ÿ�� : {target.PunId}, ������ : {caster.PunId}");

        // ���� �̺�Ʈ ����ȭ�� ���� ���� (������ ��������, �Ϲ� �������� �����ؼ� ����)
        if (bh.when == Define.evtWhen.onDead)
        { GAME.IGM.Packet.SendDeathBuffEvt(target.PunId, bh.buffType, bh.buffAtt * multiPly, bh.buffHp * multiPly); }
        else
        { GAME.IGM.Packet.SendBuffEvt(target.PunId, bh.buffType, bh.buffAtt * multiPly, bh.buffHp * multiPly); }
        
        yield break;
    }
    public IEnumerator ReceivedBuff(Define.buffType type, IBody target,int att, int hp )
    {
        // ��� �ο� ȿ������
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

    // ġ�� �ڷ�ƾ
    public IEnumerator Restore(IBody caster, IBody target, int amount, bool isDeathRattle = false)
    {
        Debug.Log($"ġ���̺�Ʈ ����, target : {target}[{target.PunId}]");
        target.HP = Mathf.Clamp(target.HP + amount, 0, (target.objType == Define.ObjType.Minion) ? target.OriginHp : 30);

        // �����̸� �����ڰ� ���� �� �ϼ����ϋ��� ��뿡�� �̺�Ʈ ����
        if (GAME.IGM.Packet.isMyTurn)
        {
            // ������ ������ �̺�Ʈ��� (�������� �����׼�ť�� ���� �����̶�)
            if (isDeathRattle == true)
            {
                GAME.IGM.Packet.SendDeathRestoreEvt(caster.PunId, target.PunId, amount,
                    (caster.objType == Define.ObjType.HandCard));
            }
            // �Ϲ� �̺�Ʈ
            else
            {
                GAME.IGM.Packet.SendRestoreEvt(caster.PunId, target.PunId, 
                    amount, (caster.objType == Define.ObjType.HandCard));
            }
        }

        yield break;
    }
   
    // ���� �ڷ�ƾ
    public IEnumerator AttackEvt(IBody attacker, IBody target, int attAmount, Define.attType attType , bool isDeathRattle = false )
    {
        #region ����ü �غ� �� ����ü �̵� �ڷ�ƾ
        // ������ ���� �̺�Ʈ���, ������ ������ ��� ���ڸ� ���͋����� ���
        if (isDeathRattle == true && GAME.IGM.Packet.isMyTurn == false)
        {
            yield return new WaitForSeconds(0.5f);
        }
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

        // ����ü ������������ Ÿ������ ���ϸ� �̵�
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

        // ����ü ����
        pj.gameObject.SetActive(false);
        #endregion

        // ���� �� + ���� �ϼ��� �����̺�Ʈ�� , �����ؾ��� �̺�Ʈ
        if (GAME.IGM.Packet.isMyTurn == true )
        {
            if (isDeathRattle == true)
            {
                GAME.IGM.Packet.SendDeathAttEvt(attacker.PunId, target.PunId, attType, attAmount, attacker.objType);
            }
            // �Ϲ� �����̺�Ʈ
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