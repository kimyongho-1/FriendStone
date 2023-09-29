using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class PacketManager
{
    // 0 ~ 20
    const byte UserInfo = 0;
    const byte StartIntro = 1;
    const byte InitDraw = 2;
    const byte IsOffensive = 3;
    const byte UserDraw = 4;
    const byte OtherTurnEnd = 5;
    const byte OtherTurnStartMSG = 6;
    const byte DoDraw = 7;

    const byte FindEvt = 10;
    const byte FindEvtResult = 11;
    const byte AcqusitionEvt = 12;
    public void InitStateDictionary()
    {
        dic.Add(UserInfo, ReceivedUserInfo);
        dic.Add(StartIntro, (object[] o)=> { GAME.IGM.AddAction(GAME.IGM.TC.StartIntro()); } );  // 양 플레이어 준비될시, 인트로 애니메이션 실행
        dic.Add(InitDraw, (object[] o) => { GAME.IGM.AddAction(GAME.IGM.Hand.CardDrawing(4)); });
        dic.Add(IsOffensive, ReceivedOffensiveResult);
        dic.Add( UserDraw , ReceivedOtherDraw );
        dic.Add(OtherTurnEnd, ReceivedTurnEnd);
        dic.Add(OtherTurnStartMSG, ShowOtherTurn);
        dic.Add(DoDraw, ReceivedDoDraw);
        dic.Add(FindEvt, ReceivedFindEvt);
        dic.Add(FindEvtResult, ReceivedResultFindEvt);
        dic.Add(AcqusitionEvt, ReceivedAcquisition );
        
    }

    // 내턴 시작 전송
    public void SendMyTurnMSG()
    {
        // 상대 턴임을 알리기
        PhotonNetwork.RaiseEvent(OtherTurnStartMSG, null, Other, SendOptions.SendReliable);
    }
    // 상대의 턴 시작시, 상대의 턴 시작 메시지 출력 
    public void ShowOtherTurn(object[] data)
    {
        // 상대 턴임을 알리기
        GAME.IGM.AddAction(GAME.IGM.Turn.ShowTurnMSG(false));
    }

    #region 게임시작시, 내 기본정보 전달 (닉네임과 영웅정보) + 선후공 전달 
    public void SendUserInfo(string nick, int ownerClass)
    {
        // 공통
        #region 닉네임과 영웅직업 전송

        // 전송할 데이터
        // 나의 닉네임
        // 나의 영웅직업 타입
        object[] packets = new object[]
        {
            nick,
            ownerClass,
        };

        // 내 정보 전송
        PhotonNetwork.RaiseEvent(UserInfo, packets, Other, SendOptions.SendReliable);
        #endregion

    }

    // 상대로부터 기본정보 전달 받아 처리
    public void ReceivedUserInfo(object[] data)
    {
        string nickName = (string)data[0];
        Define.classType classType = (Define.classType)(data[1]);

        // 적 닉네임 초기화
        GAME.IGM.Hero.Enemy.nickTmp.text = nickName;
        // 적 영웅 캔버스 초기화
        GAME.IGM.Hero.Enemy.heroSkill.InitEnemySkill(GAME.IGM.Hero.Enemy, classType);
    }

    // 선후공 결과 마스터로부터 받기 (마스터는 자신 내부에서 적용)
    public void ReceivedOffensiveResult(object[] data)
    {
        bool result = (bool)data[0];
        Debug.Log($"나의 차례 결과 : {result}");

        // 마스터라면 그대로 적용, 타 클라이언트면 반전시키기
        result = (PhotonNetwork.IsMasterClient) ? result : !result;
        // 내가 후공이라면
        if (result == false)
        {
            // 실제 턴종료는 아니지만, 매끄러운 진행을 위해 턴종료 실행
            GAME.IGM.AddAction(GAME.IGM.Turn.EndMyTurn());
        }

    }

    #endregion

    #region 드로우 전파 및 전달

    // 내가 드로우시, 상대의 화면상 적 핸드에서 드로우를 해야하기에 뽑을 카드 펀넘버를 전달
    public void SendDrawInfo(int id)
    {
        // 내가 현재 뽑은 카드 펀넘버링 전파
        PhotonNetwork.RaiseEvent(UserDraw,
        new object[] { id } ,
        Other, SendOptions.SendReliable);
    }

    // 상대로부터 펀넘버를 전달받아, 드로우 동기화 시작
    public void ReceivedOtherDraw(object[] data)
    {
        // 상대의 드로우 진행
        StartCoroutine(GAME.IGM.Hand.EnemyCardDrawing((int)data[0]));
    }

    // 상대에게 드로우를 하라고 이벤트 전파 [상대만 시킬건지, 같이 드로우하는건지]
    public void SendDoDraw(int count, bool AllClient = false)
    {
        PhotonNetwork.RaiseEvent( DoDraw ,
        new object[] { count },
        (AllClient) ? Both : Other, SendOptions.SendReliable);
    }

    // 상대로부터 드로우를 하라고 이벤트받아 드로우 실행
    public void ReceivedDoDraw(object[] data)
    {
        GAME.IGM.AddAction(GAME.IGM.Hand.CardDrawing((int)data[0]));
    }
    #endregion

    #region  턴 종료 전파와 받기
    public void SendTurnEnd()
    {
        Debug.Log("턴 종료 전송");
        
        // 데이타는 현재 턴종료를 누르는 클라이언트의 턴에 +1을 더한 다음 턴을 전달 (integer)
        PhotonNetwork.RaiseEvent(OtherTurnEnd,
            new object[] { GAME.IGM.GameTurn + 1 },
            Other, SendOptions.SendReliable);
    }

    // 상대의 턴종료 전달 받기
    public void ReceivedTurnEnd(object[] data)
    {
        Debug.Log("상대의 턴 종료를 받음");
        // 상대가 턴을 종료시, 자신의 턴에 +1을 한 새로운 턴넘버를 전달 받기
        GAME.IGM.GameTurn = (int)data[0];

        // 턴종료 버튼 초기화 및 텍스트 애니메이션 예약 + 드로우
        GAME.IGM.AddAction(GAME.IGM.Turn.StartMyTurn());

    }
    #endregion

    #region 발견이벤트를 전달 및 받기

    // 상대로부터 발견이벤트 시작한다는것을 전파
    public void SendFindEvt() 
    { PhotonNetwork.RaiseEvent(FindEvt, null, Other, SendOptions.SendReliable); }

    // 상대로부터 발견이벤트 실행을 전파 받았으면, 동기화 위해 실행
    public void ReceivedFindEvt(object[] data)
    { GAME.IGM.AddAction(GAME.IGM.FindEvt.ShowEnemyFindEvt()); }

    // 발견이벤트를 실행중인 내가 카드를 고를시 그 결과를 상대에게 공유 전파
    public void SendResultFindEvt(int idx, int punID)
    { PhotonNetwork.RaiseEvent(FindEvtResult, new object[] { idx , punID }, Other, SendOptions.SendReliable); }

    // 상대가 발견이벤트 결과를 받아서 동기화 하기
    public void ReceivedResultFindEvt(object[] data)
    { 
        // 상대로부터 몇번째 카드인지 + 그 카드의 고유 넘버가 무엇인지 전달받아 똑같은 화면 그려주기
        int idx = (int)data[0];
        int punID = (int)data[1];

        // 이벤트 예약
        GAME.IGM.AddAction(GAME.IGM.FindEvt.ShowEnemyFindEvtResult(idx, punID));
    }
    #endregion

    #region 획득 이벤트 전달 및 받기

    // 내가 획득할 카드들의 카드식별넘버를 전송하여 상대에게도 동기화시키기
    public void SendAcquisition(Define.ObjType objType,int punID ,int[] nums, int[] puns)
    {
        object[] data = new object[] { (objType == Define.ObjType.Minion) ? true : false, punID, nums, puns};
        // 전파 실행
        PhotonNetwork.RaiseEvent(AcqusitionEvt, data, Other, SendOptions.SendReliable);
    }

    // 상대의 획득 이벤트 전달받았으면, 똑같이 만들어주기
    public void ReceivedAcquisition(object[] data)
    {
        // 시전자가 미니언인지 여부
        bool isMinionAct = (bool)data[0];
        // 시전자 찾기 + 카드넘버 + 식별넘버 
        int punID = (int)data[1];
        Debug.Log($"획득이벤트 시전자 punID : {punID}");
        IBody caster = (isMinionAct) ? GAME.IGM.allIBody.Find(x=>x.PunId == punID) : GAME.IGM.Hand.EnemyHand.Find(x => x.PunId == punID);
        
        int[] cardIDs = (int[])data[2];
        int[] puns = (int[])data[3];

        GAME.IGM.AddAction(SyncAcquisitionEvt(cardIDs, puns));

        // 상대의 획득 이벤트를 내 화면에도 똑같이 동기화 해주기
        IEnumerator SyncAcquisitionEvt(int[] cardIDs, int[] puns)
        {
            // 획득하는 수치가, 최대 10장을 넘어서는지 확인 (넘을시 가능한 수만큼만 획득)
            int count = cardIDs.Length;
            // 관련카드들 생성
            for (int i = 0; i < count; i++)
            {
                // 리소스 매니저의 경로를 반환 받는 딕셔너리 통해 카드타입과 카드데이터 찾기
                Define.cardType type = GAME.Manager.RM.PathFinder.Dic[cardIDs[i]].type;
                string jsonFile = GAME.Manager.RM.PathFinder.Dic[cardIDs[i]].GetJson();
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
                GameObject.Instantiate(Resources.Load<CardHand>("Prefab/InGamePrefab/CardHand"), GAME.IGM.Hand.EnemyHandGO.transform);
                ch.Init(card, false);
                ch.PunId = puns[i];

                ch.transform.localScale = Vector3.zero;
                ch.transform.localPosition = new Vector3(0.45f,3.8f,0);
                GAME.IGM.Hand.EnemyHand.Add(ch);
                yield return null;
            }

        }
    }
    
    #endregion
}
