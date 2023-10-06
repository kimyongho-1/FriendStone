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

    public void InitMinionDictionary()
    {
        dic.Add(EnemySpawn, ReceivedMinionSpawn);
        dic.Add(EnemyAtt, ReceivedMinionAttack); 
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

            // 상대가 소환한 미니언의 현재 데이터 표기
            GAME.IGM.ShowSpawningMinionPopup(mc, currAtt, currHp, currCost);

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
}