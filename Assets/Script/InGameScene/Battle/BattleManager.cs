using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    // 공격 이벤트 실행
    public void AttackHandler(AttackHandler ah, IBody attacker)
    {
        // 타겟의 범위
        List<IBody> targets = new List<IBody>();

        // 유저가 직접 선택하는 타겟인지, 자동 선택되는 타겟인지 확인
        switch (ah.attTargeting)
        {
            // 유저가 직접 선택일시, 미니언 공격처럼 먼저 타겟팅 실행
            case Define.attTargeting.userSelect:
                GAME.Manager.IGM.TC.TargetCo = GAME.Manager.IGM.TC.TargettingCo
                    (  attacker,

                    // 공격인지 강제로 죽이는 이벤트인지 식별하기
                    (ah.attType == Define.attType.Damage) ? (IBody a, IBody t) => { return AttackEvt(a, t); }
                    : (IBody a, IBody t) => { return KillEvt(a, t); } ,

                    // 타겟범위 레이어 찾기
                    FindLayer()
                    );

                // 타겟팅 카메라 실행 + 만약 타겟팅 성공시 이벤트함수 예약 실행
                GAME.Manager.IGM.TC.StartCoroutine(GAME.Manager.IGM.TC.TargetCo);

                break;
            case Define.attTargeting.randomOnEvtArea:
                // 설정된 이벤트타입별로 타겟범위 찾기
                AutoExecute();
                Debug.Log(targets[0].TR.name);
                for (int i = 0; i < targets.Count; i++)
                {
                    Debug.Log(targets[i].TR.name);
                    ActionQueue.Enqueue(AttackEvt(attacker, targets[i]));
                }
                
                break;
        }

        #region 유저가 선택건
        string[] FindLayer() // 유저가 선택하는건이면, 타겟층의 레이어 찾기
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
            // 투사체 호출
            ParticleSystem pj = FX.GetPJ;

            // 공격자의 위치에서 시작하도록 위치 초기화
            pj.transform.position = attacker.Pos;
            pj.gameObject.SetActive(true);
            Vector3 start = attacker.Pos;
            Vector3 dest = target.Pos;
            Vector3 dir = (dest - start).normalized; // 방향벡터
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

        #region 자동 실행건
        void AutoExecute()
        {
            IBody caster = null;

            // 확인된 진영의 어떤 타입 : 하수인, 영웅 등
            switch (ah.faction) 
            { 
                case Define.evtFaction.All:
                    // 모든 진영의 모든 대상이면 자신을 제외한 모든 대상을 타겟범위로
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
                    }
                    return;
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
                    break;
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
                    break;
            }

        }
        #endregion

    }
}
