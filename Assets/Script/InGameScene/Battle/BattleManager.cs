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
    
    // ����Ǵ� �̺�Ʈ�� ���������� ����
    public IEnumerator ProcessigCo()
    {
        while (true)
        {
            // ���� �̺�Ʈ ���������� ���
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
                StartCoroutine(GAME.Manager.IGM.TC.TargettingCo
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
                if (target == null) { return; }
                
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
                        targets.Add(GAME.Manager.IGM.Hero.Player);
                        targets.Add(GAME.Manager.IGM.Hero.Enemy);
                        targets.AddRange(GAME.Manager.IGM.Spawn.playerMinions);
                        targets.AddRange(GAME.Manager.IGM.Spawn.enemyMinions);

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
                        targets.AddRange(GAME.Manager.IGM.Spawn.playerMinions);
                        targets.AddRange(GAME.Manager.IGM.Spawn.enemyMinions);
                    }
                    // ������ ������ �ϼ��ε�
                    else if (ah.area == Define.evtArea.Player)
                    {
                        targets.AddRange((attacker.IsMine) ?
                            GAME.Manager.IGM.Spawn.playerMinions : GAME.Manager.IGM.Spawn.enemyMinions);
                    }
                    // �������� �� �ϼ��ε���
                    else
                    {
                        targets.AddRange((attacker.IsMine) ?
                            GAME.Manager.IGM.Spawn.enemyMinions : GAME.Manager.IGM.Spawn.playerMinions);
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
                        targets.Add(GAME.Manager.IGM.Hero.Player);
                        targets.Add(GAME.Manager.IGM.Hero.Enemy);
                    }
                    // �������� ������
                    else if (ah.area == Define.evtArea.Player)
                    {
                        targets.Add((attacker.IsMine) ? GAME.Manager.IGM.Hero.Player : GAME.Manager.IGM.Hero.Enemy);
                    }
                    // �������� �� ������
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
                     // �̺�Ʈ ����
                     (IBody a, IBody t) => { return Buff(a, t, bh); },
                    // Ÿ�ٹ��� ���̾� ã��
                    FindLayer(bh)
                    ));
                break;
            case Define.buffTargeting.randomOnEvtArea:
                OnFindRand();
                break;

        }

        #region �ڵ� ���� ����
        // �ڵ� ���� ����
        void AutoBuff()
        {
            // �߰� ��� ���� Ȯ�� �� ã��
            List<IBody> FindExtraTarget() 
            {
                // �߰� ��� ���� Ȯ��
                List<CardField> list = (caster.IsMine) ? GAME.Manager.IGM.Spawn.playerMinions : GAME.Manager.IGM.Spawn.enemyMinions;
                // ������ ������ ������ ������ ������ �ڽ��� ���ֱ�
                if (bh.buffExtraArea != Define.buffExtraArea.withBothSide)
                { list.Remove(list.Find(x => x.PunId == caster.PunId)); }

                List<IBody> targets = new List<IBody>();
                // �߰� ��� Ȯ��
                switch (bh.buffExtraArea)
                {
                    // �߰� ��� ������ �ٷ� ������
                    case Define.buffExtraArea.None: return null;

                    // ���� �翷�� + �翷 ������ ��� , �翷�� ���ϱ�
                    case Define.buffExtraArea.withBothSide:
                    case Define.buffExtraArea.onlyBothSide:
                        int idx = list.IndexOf(list.Find(x => x.PunId == caster.PunId));

                        // �ڽ��� ������ ���� �� ������ ������ �ƴ϶�� Ÿ�� Ȯ��
                        if (idx - 1 > -1) { targets.Add(list[idx - 1]); }
                        if (idx + 1 < list.Count ) { targets.Add(list[idx + 1]); }
                        break;

                    // Ư�� �ѹ����� ���� ��ü�鸸 �߸���
                    case Define.buffExtraArea.someId:
                        // ���� ���� �ѹ����� ������ ���
                        if (bh.relatedIds.Length == 0) { return null; }

                        List<CardField> rangeList = new List<CardField>();
                        // �� �ϼ��ε��� Ư�� �ѹ����� ���� ����ŭ
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

                        // ���� �ѹ����� ��ġ�� �̴Ͼ�� ��� ã��
                        for (int i = 0; i < bh.relatedIds.Length; i++)
                        {
                            int currPunId = bh.relatedIds[i];
                            targets.AddRange(rangeList.FindAll(x => x.PunId == currPunId));
                        }
                        break;
                }


                // Ÿ���� �ϳ��� ���ٸ� ���
                if (targets.Count == 0) { return null; }

                return targets;
            }

            // �̺�Ʈ ������ Ÿ�ٵ� ã��
            List<IBody> targetList = FindExtraTarget();

            if (targetList == null) { return; }

            // Ÿ�� ������ŭ, �̺�Ʈ ����
            for (int i = 0; i < targetList.Count; i++)
            {
                ActionQueue.Enqueue(Buff(caster, targetList[i], bh));
            }
            
        }
        #endregion


        #region ������ Ÿ���� ã�� ����
        void OnFindRand()
        {
            List<IBody> list = new List<IBody>();

            GetRange(bh.area);

            // Ÿ�ٵ� ã��
            void GetRange(Define.evtArea area)
            {
                switch (bh.faction)
                {
                    case Define.evtFaction.All:
                        // ��ü ������ ��ü Ÿ���̶��
                        if (area == Define.evtArea.All)
                        {
                            list.AddRange(GAME.Manager.IGM.Spawn.playerMinions);
                            list.AddRange(GAME.Manager.IGM.Spawn.enemyMinions);
                            list.Add(GAME.Manager.IGM.Hero.Player);
                            list.Add(GAME.Manager.IGM.Hero.Enemy);
                        }

                        // �Ʊ� ��ü �������
                        else if (area == Define.evtArea.Player)
                        { list.Add(GAME.Manager.IGM.Hero.Player); list.AddRange(GAME.Manager.IGM.Spawn.playerMinions); }

                        // ���� ��ü �������
                        else
                        { list.Add(GAME.Manager.IGM.Hero.Enemy); list.AddRange(GAME.Manager.IGM.Spawn.playerMinions); }
                        break;

                    case Define.evtFaction.Minion:
                        // ��ü ������ ��ü Ÿ���̶��
                        if (area == Define.evtArea.All)
                        {
                            list.AddRange(GAME.Manager.IGM.Spawn.playerMinions);
                            list.AddRange(GAME.Manager.IGM.Spawn.enemyMinions);
                        }

                        // �Ʊ� ��ü �������
                        else if (area == Define.evtArea.Player)
                        { list.AddRange(GAME.Manager.IGM.Spawn.playerMinions); }

                        // ���� ��ü �������
                        else
                        {  list.AddRange(GAME.Manager.IGM.Spawn.playerMinions); }
                        break;
                    case Define.evtFaction.Hero:    
                        // ��ü ������ �������̶��
                        if (area == Define.evtArea.All)
                        {
                            list.Add(GAME.Manager.IGM.Hero.Player);
                            list.Add(GAME.Manager.IGM.Hero.Enemy);
                        }

                        // �Ʊ� ������
                        else if (area == Define.evtArea.Player)
                        { list.Add(GAME.Manager.IGM.Hero.Player); }

                        // �� ������
                        else
                        { list.Add(GAME.Manager.IGM.Hero.Enemy);}
                        break;
                }
            }

            // Ÿ�� Ȯ�� �� �̺�Ʈ ����
            ActionQueue.Enqueue(Buff(caster,
                list[UnityEngine.Random.Range(0, list.Count)], bh));
        }
        #endregion

        // ������ ���� �ڷ�ƾ
        IEnumerator Buff(IBody caster, IBody target, BuffHandler bh)
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
            }
        }
    }

    public void RegisterUtillHandler(UtillHandler uh, IBody caster)
    {
        switch (uh.utillType) // ���Ǽ� ��Ÿ �̺�Ʈ�� ���� �������� ���� �ڵ�����ǵ�
        {
            case Define.utillType.draw: // ��ο� �̺�Ʈ
                ActionQueue.Enqueue(DrawEvt(uh));
                break;
            case Define.utillType.find: // �߰� �̺�Ʈ
                Debug.Log("�߰� �̺�Ʈ");
                ActionQueue.Enqueue(FindEvt(uh));
                break;
            case Define.utillType.acquisition: // ȹ�� �̺�Ʈ
                Debug.Log("ȹ�� �̺�Ʈ");
                ActionQueue.Enqueue(AcquisitionEvt(uh));
                break;
        }

        // ��ο� �̺�Ʈ
        IEnumerator DrawEvt(UtillHandler uh)
        {
            switch (uh.area)
            {
                // ����� ��ο� �̺�Ʈ�� ��밡 ����
                case Define.evtArea.Enemy: yield break;

                // ��� �Ǵ� ���� ��ο��� ���, ���� ��ο� ���� (��뿡�� �̺�Ʈ�� ���޵Ǳ⿡)
                case Define.evtArea.All:
                case Define.evtArea.Player:
                    // ��ο� ����
                    yield return StartCoroutine(GAME.Manager.IGM.Hand.CardDrawing(uh.utillAmount));
                    break;
            }
        }

        // �߰� �̺�Ʈ
        IEnumerator FindEvt(UtillHandler uh)
        {
            // �̹� 10���̸� ���� ���
            if (GAME.Manager.IGM.Hand.PlayerHand.Count == 10) { yield break; }

            // �߰� �̺�Ʈ �غ� �� ���� (�ڷ�ƾ�� ������Ʈ�� ���������� ������ �ȵǾ�, �Լ��� ���� ������ �غ� �� �ڷ�ƾ ����)
            GAME.Manager.IGM.FindEvt.ReadyFindEvt(uh.relatedCards);

            // �߰� �ڷ�ƾ ����������(������ �����ҋ����� ���)
            yield return new WaitUntil(() => (GAME.Manager.IGM.FindEvt.CurrSelected == true));
        }
        
        // ȹ�� �̺�Ʈ
        IEnumerator AcquisitionEvt(UtillHandler uh)
        {
            // ȹ���ϴ� ��ġ��, �ִ� 10���� �Ѿ���� Ȯ�� (������ ������ ����ŭ�� ȹ��)
            int count = (GAME.Manager.IGM.Hand.PlayerHand.Count + uh.relatedCards.Length <= 10) 
                ? uh.relatedCards.Length : (10 - (GAME.Manager.IGM.Hand.PlayerHand.Count));

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
                GameObject.Instantiate(Resources.Load<CardHand>("Prefab/InGamePrefab/CardHand"), GAME.Manager.IGM.Hand.PlayerHandGO.transform);
                ch.Init(ref card);
                ch.PunId = (Photon.Pun.PhotonNetwork.IsMasterClient ? 1000 : 2000) 
                    + GAME.Manager.IGM.Hand.punConsist++;
                ch.transform.localScale = Vector3.one;
                ch.transform.localPosition = caster.TR.position;
                GAME.Manager.IGM.Hand.PlayerHand.Add(ch);
                yield return null;
            }

            // �� �� ����
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
                     // �̺�Ʈ ����
                     (IBody a, IBody t) => { return Restore(a, t, rh.restoreAmount); },
                    // Ÿ�ٹ��� ���̾� ã��
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
            // �߰� ��� ���� Ȯ�� �� ã��
            List<IBody> FindExtraTarget()
            {
                // �߰� ��� ������ �ٷ� ������
                if (rh.restoreExtraArea == Define.restoreExtraArea.None)
                { return null; }
                
                // �߰� ��� ���� Ȯ��
                List<CardField> list = (caster.IsMine) ? GAME.Manager.IGM.Spawn.playerMinions : GAME.Manager.IGM.Spawn.enemyMinions;
                // ������ ������ ������ ������ ������ �ڽ��� ���ֱ�
                
                List<IBody> targets = new List<IBody>();
                // �߰� ��� Ȯ��
                switch (rh.restoreExtraArea)
                {
                    // ���� �翷�� + �翷 ������ ��� , �翷�� ���ϱ�
                    case Define.restoreExtraArea.BothSide:
                        int idx = list.IndexOf(list.Find(x => x.PunId == caster.PunId));

                        // �ڽ��� ������ ���� �� ������ ������ �ƴ϶�� Ÿ�� Ȯ��
                        if (idx - 1 > -1 && list[idx-1].PunId != caster.PunId) { targets.Add(list[idx - 1]); }
                        if (idx + 1 < list.Count && list[idx + 1].PunId != caster.PunId) { targets.Add(list[idx + 1]); }
                        break;

                    // Ư�� �ѹ����� ���� ��ü�鸸 �߸���
                    case Define.restoreExtraArea.addOwnerHero:
                        targets.Add((caster.IsMine) ? GAME.Manager.IGM.Hero.Player : GAME.Manager.IGM.Hero.Enemy);
                        break;
                }


                // Ÿ���� �ϳ��� ���ٸ� ���
                if (targets.Count == 0) { return null; }

                return targets;
            }

            // �̺�Ʈ ������ Ÿ�ٵ� ã��
            List<IBody> targetList = FindExtraTarget();

            if (targetList == null) { return; }

            // Ÿ�� ������ŭ, �̺�Ʈ ����
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
            
            // ������ �ڽ��� ġ�� X
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