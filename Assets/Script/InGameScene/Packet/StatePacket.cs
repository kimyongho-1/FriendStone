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
    const byte StartIntro = 1; // 인게임씬 시작시 화면이 밝아지는 인트로 동시실행 전파
    const byte InitDraw = 2; //  초기 4장 드로우 상태로 게임시작 동시실행 전파
    const byte IsOffensive = 3; // 선후공 결정 결과 동시실행 전파
    const byte UserDraw = 4; // 상대가 카드를 뽑을떄마다 자동으로 내게도 이벤트가 전파
    const byte OtherTurnEnd = 5;
    const byte OtherTurnStartMSG = 6;
    const byte DoDraw = 7; // 드로우를 실행 전파

    const byte FindEvt = 10; // 발견 이벤트 시작 전파
    const byte FindEvtResult = 11; // 발견 이벤트 실행자가 카드를 고르면 그 결과 전파
    const byte AcqusitionEvt = 12; // 단순 획득 이벤트 실행시 전파

    const byte BuffEvt = 14; // 버프 이벤트 전달받을시 동기화하기
    const byte AttEvt = 15; // 공격 이벤트 전달받을시 동기화하기
    const byte RestoreEvt = 16; // 치료이벤트
    const byte FatigueEvt = 17; // 상대가 덱에 카드가 없어 피해입는 탈진 이벤트
    const byte OverDrawEvt = 18; // 상대가 카드가 10장인데 추가 드로우시, 뽑는 카드 없애는 이벤트
    const byte GameEnd = 19; // 게임결과를 상대방이 먼저 볼경우, 나에게 전파하기에 받아서 동기화
    public void InitStateDictionary()
    {
        dic.Add(UserInfo, ReceivedUserInfo);
        dic.Add(StartIntro, (object[] o)=> { GAME.IGM.AddAction(GAME.IGM.TC.StartIntro()); }); 
        dic.Add(InitDraw, (object[] o) => { GAME.IGM.AddAction(GAME.IGM.Hand.CardDrawing(4)); });
        dic.Add(IsOffensive, ReceivedOffensiveResult);
        dic.Add( UserDraw , ReceivedOtherDraw );
        dic.Add(OtherTurnEnd, ReceivedTurnEnd);
        dic.Add(OtherTurnStartMSG, ShowOtherTurn);
        dic.Add(DoDraw, ReceivedDoDraw);
        dic.Add(FindEvt, ReceivedFindEvt);
        dic.Add(FindEvtResult, ReceivedResultFindEvt);
        dic.Add(AcqusitionEvt, ReceivedAcquisition );
        dic.Add(BuffEvt, ReceivedBuffEvt);
        dic.Add(AttEvt, ReceivedAttEvt );
        dic.Add(RestoreEvt, ReceivedRestoreEvt );
        dic.Add(FatigueEvt, ReceivedFatigueEvt);
        dic.Add(OverDrawEvt, ReceivedOverDrawEvt);
        dic.Add(GameEnd, GameEndStart);
    }

    // 내 턴 시작 전송
    public void SendMyTurnMSG()
    {
        // 상대에게 내 턴 시작한다고 알리기
        PhotonNetwork.RaiseEvent(OtherTurnStartMSG, null, Other, SendOptions.SendReliable);
    }
    // 상대의 턴 시작시, 상대의 턴 시작 메시지 출력 
    public void ShowOtherTurn(object[] data)
    {
        // 상대 턴 시작, 상대 마나 초기화
        GAME.IGM.Hero.Enemy.MP = Mathf.Min(10, GAME.IGM.GameTurn+1);
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
        Debug.Log("정보 전달 받기 성공");
        string nickName = (string)data[0];
        Define.classType classType = (Define.classType)(data[1]);

        // 적 닉네임 초기화
        GAME.IGM.Hero.Enemy.nickTmp.text = nickName;
        // 적 영웅 캔버스 초기화
        GAME.IGM.Hero.Enemy.heroData = GAME.Manager.RM.GetHeroData(classType);
        GAME.IGM.Hero.Enemy.heroData.Init(GAME.IGM.Hero.Enemy.playerImg, GAME.IGM.Hero.Enemy.skillImg,false);
        GAME.IGM.Hero.Enemy.heroSkill.InitSkill(false);
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
            GAME.IGM.Turn.ClickedOnTurnEnd(null);
            //GAME.IGM.AddAction(GAME.IGM.Turn.EndMyTurn());
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
        Debug.Log($"상대로부터 드로우 이벤트 전달받음 {(int)data[0]}");
        if (GAME.IGM.GameTurn < 2f)
        {
            // 상대의 드로우 진행
            GAME.IGM.StartCoroutine(GAME.IGM.Hand.EnemyCardDrawing((int)data[0]));
        }
        else
        { // 상대의 드로우 진행
            GAME.IGM.AddAction(GAME.IGM.Hand.EnemyCardDrawing((int)data[0]));
        }
        
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
            new object[] { GAME.IGM.GameTurn+1 },
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
        IBody caster = (isMinionAct) ? GAME.IGM.allIBody.Find(x=>x.PunId == punID) : GAME.IGM.Hand.EnemyHand.Find(punID);
        
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

            yield return GAME.IGM.StartCoroutine(GAME.IGM.Hand.CardAllignment(false));
        }
    }

    #endregion

    #region 버프 이벤트 전달 및 받기
    public void SendBuffEvt(int targetPunID, Define.buffType type,int att, int hp)
    {
        object[] data = new object[] {targetPunID, (int)type , att,hp };
        // 전파 실행
        PhotonNetwork.RaiseEvent(BuffEvt, data, Other, SendOptions.SendReliable);
    }
    public void ReceivedBuffEvt(object[] data)
    {
        int targetPunID = (int)data[0];
        Define.buffType type = (Define.buffType)data[1];
        int att = (int)data[2];
        int hp = (int)data[3];
        
        GAME.IGM.AddAction( DelayedBuff(targetPunID, att, hp, type));
        IEnumerator DelayedBuff(int targetPunID, int att, int hp , Define.buffType type)
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

    #region 공격 이벤트 전달 및 받기
    public void SendAttEvt(int attackerPunID ,int targetPunID, Define.attType type, int attAmount , Define.ObjType objType)
    {
        object[] data = new object[] { attackerPunID, targetPunID, (int)type, attAmount ,(int)objType };
        PhotonNetwork.RaiseEvent(AttEvt, data, Other, SendOptions.SendReliable);
    }
    public void ReceivedAttEvt(object[] data)
    {
        int attackerPunID = (int)data[0];
        int targetPunID = (int)data[1];
        Define.attType attType = (Define.attType)data[2];
        int attAmount = (int)data[3];
        Define.ObjType objType = (Define.ObjType)data[4];

        GAME.IGM.AddAction(MakeAttEvt(attackerPunID, targetPunID, attType, attAmount, objType));

        IEnumerator MakeAttEvt(int attackerPunID, int targetPunID, Define.attType attType, int attAmount, Define.ObjType objType)
        {
            IBody target = GAME.IGM.allIBody.Find(x => x.PunId == targetPunID);
            // 공격 이벤트를 받을시, 공격자의 obj타입이 미니언이라면 미니언을 찾고
            // 그외에는 적 영웅을 공격자로 지정 (공격 이펙트의 시작위치가 적 영웅에서 시작하길 원하기에)
            IBody attacker = (objType == Define.ObjType.Minion) ?
                GAME.IGM.allIBody.Find(x => x.PunId == attackerPunID)
                : GAME.IGM.Hero.Enemy;
            Debug.Log($"공격 이벤트 공격자:{attacker.PunId}, 타겟 : {target.PunId}");
            // 상대로부터 받은 공격이벤트 예약후 실행
            yield return GAME.IGM.StartCoroutine(GAME.IGM.Battle.AttackEvt(attacker, target, attAmount, attType));
        }
       
    }

    #endregion

    #region 치료 이벤트 전파 및 전달받기
    public void SendRestoreEvt(int casterID, int targetID, int amount, bool casterIsHandCard)
    {
        object[] data = new object[] { casterID, targetID, amount, (object)casterIsHandCard };
        PhotonNetwork.RaiseEvent(RestoreEvt, data, Other, SendOptions.SendReliable);
    }
    public void ReceivedRestoreEvt(object[] data)
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
        GAME.IGM.AddAction(GAME.IGM.Battle.Restore(caster, target, amount));
    }
    #endregion

    #region 탈진 이벤트 전파 및 받기
    public void SendFatigue(int dmg)
    {
        PhotonNetwork.RaiseEvent(FatigueEvt, new object[] { dmg }, Other, SendOptions.SendReliable);
    }
    public void ReceivedFatigueEvt(object[] data)
    {
        int dmg = (int)data[0];
        // 적을 기준으로, 탈진피해량과 함께 이벤트 예약 실행
        GAME.IGM.AddAction(GAME.IGM.Hand.Fatigue.DeckExhausted(false, dmg));
    }
    #endregion

    #region 핸드가 10장일떄 추가드로우한 카드는 소멸되는 이벤트 전달&받기
    public void SendOverDrawInfo(int cardID)
    { PhotonNetwork.RaiseEvent(OverDrawEvt , new object[] { cardID} , Other, SendOptions.SendReliable); }
    public void ReceivedOverDrawEvt(object[] data)
    {
        // 현재 소멸되는 상대 카드가 어떤 카드인지, 카드식별자로 찾기
        int cardID = (int)data[0];

        // 카드식별자 넘겨주며, 상대의 드로우카드 소멸 이벤트 예약
        GAME.IGM.AddAction(GAME.IGM.Hand.EnemyHandOverFlow(cardID));
    }
    #endregion

    #region 게임엔딩 시작
    public void SendGameEnd(bool isPlayerWin)
    { PhotonNetwork.RaiseEvent(GameEnd, new object[] { isPlayerWin }, Other, SendOptions.SendReliable); }
    public void GameEndStart(object[] data)
    {
        GAME.IGM.AddAction(GAME.IGM.EndingGame((bool)data[0], false));
    }
    #endregion
}
