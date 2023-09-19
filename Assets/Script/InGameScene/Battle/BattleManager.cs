using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            yield return new WaitUntil(()=>(ActionQueue.Count > 0));

            yield return StartCoroutine(ActionQueue.Dequeue());
        }

    }
    private void Awake()
    {
        FX = GetComponent<FXManager>();
        GAME.Manager.IGM.Battle = this;
        StartCoroutine(ProcessigCo());
    }
    
    public void Evt(CardBaseEvtData evtData, IBody attacker)
    {
        switch (evtData.type) 
        {
            case Define.evtType.attack: AttackHandler((AttackHandler)evtData , attacker); return;
            case Define.evtType.buff: break;
            case Define.evtType.utill: break;
            case Define.evtType.restore: break;
        }
    }

    // ���� �̺�Ʈ ����
    public void AttackHandler(AttackHandler ah, IBody attacker)
    {
        // Ÿ���� ����
        List<IBody> targets = new List<IBody>();

        // ������ ���� �����ϴ� Ÿ������, �ڵ� ���õǴ� Ÿ������ Ȯ��
        switch (ah.attTargeting)
        {
            // ������ ���� �����Ͻ�, �̴Ͼ� ����ó�� ���� Ÿ���� ����
            case Define.attTargeting.userSelect:
                GAME.Manager.IGM.TC.TargetCo = GAME.Manager.IGM.TC.TargettingCo
                    (  attacker,

                    // �������� ������ ���̴� �̺�Ʈ���� �ĺ��ϱ�
                    (ah.attType == Define.attType.Damage) ? (IBody a, IBody t) => { return AttackEvt(a, t); }
                    : (IBody a, IBody t) => { return KillEvt(a, t); } ,

                    // Ÿ�ٹ��� ���̾� ã��
                    FindLayer()
                    );

                // Ÿ���� ī�޶� ���� + ���� Ÿ���� ������ �̺�Ʈ�Լ� ���� ����
                GAME.Manager.IGM.TC.StartCoroutine(GAME.Manager.IGM.TC.TargetCo);

                break;
            case Define.attTargeting.randomOnEvtArea:
                // ������ �̺�ƮŸ�Ժ��� Ÿ�ٹ��� ã��
                AutoExecute();
                Debug.Log(targets[0].TR.name);
                for (int i = 0; i < targets.Count; i++)
                {
                    Debug.Log(targets[i].TR.name);
                    ActionQueue.Enqueue(AttackEvt(attacker, targets[i]));
                }
                
                break;
        }

        #region ������ ���ð�
        string[] FindLayer() // ������ �����ϴ°��̸�, Ÿ������ ���̾� ã��
        { 
            switch(ah.area) 
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
            float angle = Vector3.Angle(attacker.TR.up , dir);
            Vector3 cross = Vector3.Cross(attacker.TR.up, dir);
            if (cross.y < 0) { angle *= -1; }


            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime;
                pj.transform.rotation =
                    Quaternion.Euler(new Vector3(0,0, 90f + Mathf.Lerp(0, angle, t)));
                pj.transform.position =
                    Vector3.Lerp(start, dest , t);

                yield return null;
            }
            pj.gameObject.SetActive(false);
        }
        IEnumerator KillEvt(IBody attacker, IBody target)
        {
            float t = 0;
            while (t < 1f)
            {
                yield return null;
            }
        }
        #endregion

        #region �ڵ� �����
        void AutoExecute()
        {
            IBody caster = null;

            // Ȯ�ε� ������ � Ÿ�� : �ϼ���, ���� ��
            switch (ah.faction) 
            { 
                case Define.evtFaction.All:
                    // ��� ������ ��� ����̸� �ڽ��� ������ ��� ����� Ÿ�ٹ�����
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
                    }
                    return;
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
                    break;
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
                    break;
            }

        }
        #endregion

    }
}
