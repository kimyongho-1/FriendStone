using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using System;
using Unity.VisualScripting;
using Photon.Realtime;

public partial class PacketManager : MonoBehaviourPunCallbacks
{
    public bool isMyTurn = false;

    Dictionary<byte, Action<object[]>> dic = new Dictionary<byte, Action<object[]>>();
    RaiseEventOptions Other = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
    RaiseEventOptions Master = new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient };
    RaiseEventOptions Both = new RaiseEventOptions() { Receivers = ReceiverGroup.All };
    private void Awake()
    {
        GAME.IGM.Packet = this;

        InitStateDictionary();
        InitMinionDictionary();
        InitHeroDictionary();
        InitSpellDictionary();

        PhotonNetwork.NetworkingClient.EventReceived -= Received;
        PhotonNetwork.NetworkingClient.EventReceived += Received;

        

        // 서로 기본 정보 전송후, 각자 전달 받을떄까지 대기
        StartCoroutine(GameReady());

        // 1. 서로 정보 전달
        // 2. 화면 밝아지는 인트로 
        // 3. 카드 드로우
        // 4. 알림 텍스트 표시

        IEnumerator GameReady()
        {
            // 만약 내가 호스트가 아닌 일반 클라라면
            if (!PhotonNetwork.IsMasterClient)
            {
                // 호스트가 보내는 영웅정보를 받을떄까지 대기
                yield return new WaitUntil(() => (GAME.IGM.Hero.Enemy.heroData.SkillCo != null));
                // 그후 호스트에게 내 영웅정보 전달, 이후는 호스트가 동시전파로 이벤트 진행
            }
            // 서로 기본 정보 (닉네임 + 내 직업) 전송
            SendUserInfo(GAME.Manager.NM.playerInfo.NickName,
                (int)GAME.Manager.RM.GameDeck.ownerClass);

            //정보 공유가 되었따면, 적 영웅의 스킬데이타가 null이 아닐것이기에
            yield return new WaitUntil(()=>(GAME.IGM.Hero.Enemy.heroData.SkillCo != null));
            yield return new WaitForSeconds(0.5f);

            // 마스터클라이언트 동기화 목적으로 동시전파 실행
            if (PhotonNetwork.IsMasterClient)
            {
                #region 화면이 밝아지는 인트로 시작
                // 화면이 밝아지는 인트로 시작
                // 본격 게임 시작 진행, 검은화면 점차 밝아지기 (동시 전파)
                PhotonNetwork.RaiseEvent(StartIntro, null, Both, SendOptions.SendReliable);
                #endregion

                #region 초기 4장을 뽑는 애님코루틴 시작
                // 초기 게임 세팅 : 서로 4장 드로우 시작 , 동시 전파
                PhotonNetwork.RaiseEvent(InitDraw, null, Both, SendOptions.SendReliable);
                #endregion

                #region 마스터가 선후공 결정 및 전파
                int firstID = UnityEngine.Random.Range(0, 2); // 0 or 1

                // 선후공 결과를 동시 전파
                PhotonNetwork.RaiseEvent(IsOffensive,
                    new object[] { (firstID == 0) ? true : false },
                    Both,
                    SendOptions.SendReliable);

                Debug.Log($"firstID  :  {firstID}");
                #endregion
            }
            yield return null;
        }
    }

    // 이벤트코드를 키값으로 전달받을 이벤트 찾아 적용
    public void Received(EventData photonEvent) 
    {
        
        byte eventCode = photonEvent.Code;
        if (dic.ContainsKey(eventCode))
        {
            if (photonEvent.CustomData is object[])
            {
                Debug.Log("not null Code : "+photonEvent.Code);
                dic[eventCode].Invoke((object[])(photonEvent.CustomData)); 
            }

            else
            {
                Debug.Log("null Code : " + photonEvent.Code);
                dic[eventCode].Invoke(null); 
            }
            
        }
        else
        { Debug.Log($"EvtCode Not {eventCode}"); }
    }
}