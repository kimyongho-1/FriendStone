using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public partial class PacketManager
{
    // 21 ~ 40
    const byte EnemySpawn = 21;
    const byte EnemyAtt = 22;
    const byte WaitDeathRattleEnd = 23;

    const byte DeathRattleRestoreEvt = 24;
    const byte DeathRattleAttEvt = 25;
    const byte DeathRattleBuffEvt = 26;
    public void InitMinionDictionary()
    {
        dic.Add(EnemySpawn, ReceivedMinionSpawn);
        dic.Add(EnemyAtt, ReceivedMinionAttack);
        dic.Add(WaitDeathRattleEnd , DeathRattleEnd );
        dic.Add(DeathRattleRestoreEvt , ReceivedDeathRestoreEvt);
        dic.Add(DeathRattleAttEvt , ReceivedDeathAttEvt);
        dic.Add(DeathRattleBuffEvt, ReceivedDeathBuffEvt);
    }

    #region 미니언 소환 이벤트 전파 받기

    public void SendMinionSpawn(int punID, int fieldIdx, int cardID , int currAtt, int currHp, int currCost ) // 현재 att hp cost
    {
        object[] data = new object[] { punID , fieldIdx , cardID, currAtt, currHp, currCost};
        PhotonNetwork.RaiseEvent(EnemySpawn, data, Other, SendOptions.SendReliable);
    }

    // 상대의 미니언 소환 이벤트를 전달 받아 동기화 실행
    public void ReceivedMinionSpawn(object[] data)
    {
        int punID = (int)data[0]; // 게임 화면내 어떤 카드 객체인지 식별자
        int fieldIdx = (int)data[1]; // 필드내 몇번째 위치에 소환될건지
        int cardID = (int)data[2]; // 해당 카드가 실제 어떤 카드인지 , 카드데이터

        // 변경되었을지 모를 현재의 공체비 받기
        int currAtt = (int)data[3];
        int currHp = (int)data[4];
        int currCost = (int)data[5];
        Debug.Log($"미니언 소환 전달 받기 성공, punID {punID}");
        // 데이터가 존재하기에, 소환 이벤트 예약
        GAME.IGM.AddAction(MakeEnemyData(punID, fieldIdx, cardID, currAtt, currHp, currCost));

        IEnumerator MakeEnemyData(int punID, int fieldIdx, int cardID, int currAtt, int currHp, int currCost)
        {
            // 리소스 매니저의 경로를 반환 받는 딕셔너리 통해 카드타입과 카드데이터 찾기
            Define.cardType type = GAME.Manager.RM.PathFinder.Dic[cardID].type;
            string jsonFile = GAME.Manager.RM.PathFinder.Dic[cardID].GetJson();

            // 카드 데이터 생성
            MinionCardData mc = JsonConvert.DeserializeObject<MinionCardData>
                (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

            // 비용감소
            GAME.IGM.Hero.Enemy.MP -= currCost;

            // 상대가 소환한 미니언의 현재 데이터 표기
            GAME.IGM.ShowEnemyMinionPopup(mc, currAtt, currHp, currCost);

            // 미니언 소환전, 실제 카드데이터를 찾아 적용해주기
            GAME.IGM.Hand.EnemyHand.Find(punID).MC = mc;
            yield return GAME.IGM.StartCoroutine(GAME.IGM.Spawn.EnemySpawn(punID, fieldIdx, currAtt, currHp, currCost));
        }

    }
    #endregion

    #region 미니언 공격 이벤트

    // 내 미니언 공격 사실을 전파하기 ( 공격자의id, 타겟의 id )
    public void SendMinionAttack(int attackerID, int targetID)
    {
        object[] data = new object[] { attackerID, targetID };
        PhotonNetwork.RaiseEvent(EnemyAtt, data , Other, SendOptions.SendReliable);
    }

    // 상대로 부터 상대 미니언 공격 이벤트 전파 받기
    public void ReceivedMinionAttack(object[] data)
    {
        // 공격자와 타겟의 식별 변수 확인
        int attackerID = (int)data[0];
        int targetID = (int)data[1];

        // 공격자와 타겟 찾기
        IBody attacker = GAME.IGM.allIBody.Find(x => x.PunId == attackerID);
        IBody target = GAME.IGM.allIBody.Find(x => x.PunId == targetID);

        // 현재 이벤트는 미니언 공격을 기준으로 하기에 , CardField컴포넌트 찾기
        CardField minion = attacker.TR.GetComponent<CardField>();

        // 그리고 공격 이벤트 예약
        GAME.IGM.AddAction(minion.AttackCo(attacker, target));
    }
    #endregion

    #region 미니언이 죽을떄 실행할 버프 이벤트 전달 및 받기
    public void SendDeathBuffEvt(int targetPunID, Define.buffType type, int att, int hp)
    {
        object[] data = new object[] { targetPunID, (int)type, att, hp };
        // 전파 실행
        PhotonNetwork.RaiseEvent(DeathRattleBuffEvt, data, Other, SendOptions.SendReliable);
    }
    public void ReceivedDeathBuffEvt(object[] data)
    {
        int targetPunID = (int)data[0];
        Define.buffType type = (Define.buffType)data[1];
        int att = (int)data[2];
        int hp = (int)data[3];

        GAME.IGM.AddDeathAction(DelayedBuff(targetPunID, att, hp, type));
        IEnumerator DelayedBuff(int targetPunID, int att, int hp, Define.buffType type)
        {
            CardField cf1 = GAME.IGM.Spawn.enemyMinions.Find(x => x.PunId == targetPunID);
            CardHand cf2 = GAME.IGM.Hand.EnemyHand.Find(targetPunID);
            Debug.Log($"cardField : {cf1}, cardHand : {cf2}");
            yield return GAME.IGM.StartCoroutine(
                GAME.IGM.Battle.ReceivedBuff(type, GAME.IGM.Spawn.enemyMinions.Find(x => x.PunId == targetPunID)
                , att, hp));
        }
    }
    #endregion

    #region 미니언이 죽을떄 실행할 치료 이벤트 전파 및 전달받기
    public void SendDeathRestoreEvt(int casterID, int targetID, int amount, bool casterIsHandCard)
    {
        object[] data = new object[] { casterID, targetID, amount, (object)casterIsHandCard };
        PhotonNetwork.RaiseEvent(DeathRattleRestoreEvt, data, Other, SendOptions.SendReliable);
    }
    public void ReceivedDeathRestoreEvt(object[] data)
    {
        int casterPunID = (int)data[0];
        int targetPunID = (int)data[1];
        int amount = (int)data[2];
        bool casterIsHand = (bool)data[3];

        // 시전자와 대상 찾기 (주문카드는 적 영웅 위치를 고정)
        IBody caster = (casterIsHand == true) ?
            GAME.IGM.Hero.Enemy
            : GAME.IGM.allIBody.Find(x => x.PunId == casterPunID);
        IBody target = GAME.IGM.allIBody.Find(x => x.PunId == targetPunID);

        // 상대로부터 받은 치료이벤트 예약후 실행
        GAME.IGM.AddDeathAction(GAME.IGM.Battle.Restore(caster, target, amount, true));
    }
    #endregion


    #region 미니언이 죽을떄 실행할 공격 이벤트 전달 및 받기
    public void SendDeathAttEvt(int attackerPunID, int targetPunID, Define.attType type, int attAmount, Define.ObjType objType)
    {
        object[] data = new object[] { attackerPunID, targetPunID, (int)type, attAmount, (int)objType };
        PhotonNetwork.RaiseEvent(DeathRattleAttEvt, data, Other, SendOptions.SendReliable);
    }
    public void ReceivedDeathAttEvt(object[] data)
    {
        int attackerPunID = (int)data[0];
        int targetPunID = (int)data[1];
        Define.attType attType = (Define.attType)data[2];
        int attAmount = (int)data[3];
        Define.ObjType objType = (Define.ObjType)data[4];

        IBody target = GAME.IGM.allIBody.Find(x => x.PunId == targetPunID);
        // 공격 이벤트를 받을시, 공격자의 obj타입이 미니언이라면 미니언을 찾고
        // 그외에는 적 영웅을 공격자로 지정 (공격 이펙트의 시작위치가 적 영웅에서 시작하길 원하기에)
        IBody attacker = (objType == Define.ObjType.Minion) ?
            GAME.IGM.allIBody.Find(x => x.PunId == attackerPunID)
            : GAME.IGM.Hero.Enemy;

        // 상대로부터 받은 공격이벤트 예약후 실행
        GAME.IGM.AddDeathAction(
            GAME.IGM.Battle.AttackEvt(attacker, target, attAmount, attType, true)
            );
    }

    #endregion

    #region 죽음의 메아리 종료를 기다리기
    public void SendDeathRattleEnd(int punID)
    {
        Debug.Log("Send EndOfDeathRattle");
        PhotonNetwork.RaiseEvent(WaitDeathRattleEnd, new object[] { punID}, Other, SendOptions.SendReliable);
    }
    public void DeathRattleEnd(object[] data)
    {
        Debug.Log("Received EndOfDeathRattle");
        int punID = (int)data[0];
        // 이 이벤트를 받을떄쯤, 순차적으로 먼저 죽을떄 실행할 이벤트를 받아 실행중일것이므로
        // 마지막으로 모든 죽을때 이벤트 실행 완료 신호를 전달받아 동기화해주기
        GAME.IGM.AddDeathAction(FinishingDeathRattle(punID));
        IEnumerator FinishingDeathRattle(int punID)
        {
            yield return null;
            IBody deadMan = GAME.IGM.allIBody.Find(x=>x.PunId == punID);

            // 현재 미니언만 죽을떄 이벤트 실행이 가능하기에
            deadMan.TR.GetComponent<CardField>().waitDeathRattleEnd = true;
        }
    }
    #endregion
}