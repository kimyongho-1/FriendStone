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

    // 상대에게 내 미니언 소환 전파 
    // punID : 손에서 어떤 카드인지 식별
    // fieldIDX : 몇번쨰 필드에 소환했는지 
    // cardID : 실제 카드 데이터 넘버
    public void SendMinionSpawn(int punID, int fieldIdx, int cardID)
    {
        object[] data = new object[] { punID , fieldIdx , cardID};
        PhotonNetwork.RaiseEvent(EnemySpawn, data, Other, SendOptions.SendReliable);
    }

    // 상대의 미니언 소환 이벤트를 전달 받아 동기화 실행
    public void ReceivedMinionSpawn(object[] data)
    {
        int punID = (int)data[0]; // 게임 화면내 어떤 카드 객체인지 식별자
        int fieldIdx = (int)data[1]; // 필드내 몇번째 위치에 소환될건지
        int cardID = (int)data[2]; // 해당 카드가 실제 어떤 카드인지 , 카드데이터

        // 리소스 매니저의 경로를 반환 받는 딕셔너리 통해 카드타입과 카드데이터 찾기
        Define.cardType type = GAME.Manager.RM.PathFinder.Dic[cardID].type;
        string jsonFile = GAME.Manager.RM.PathFinder.Dic[cardID].GetJson();

        // 카드 데이터 생성
        CardData card = JsonConvert.DeserializeObject<MinionCardData>
            (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

        // 미니언 소환전, 실제 카드데이터를 찾아 적용해주기
        GAME.IGM.Hand.EnemyHand.Find(x => x.PunId == punID).data = card;

        // 데이터가 존재하기에, 소환 이벤트 예약
        GAME.IGM.AddAction(GAME.IGM.Spawn.EnemySpawn(punID, fieldIdx));
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