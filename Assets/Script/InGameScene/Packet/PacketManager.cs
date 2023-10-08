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

        

        // ���� �⺻ ���� ������, ���� ���� ���������� ���
        StartCoroutine(GameReady());

        // 1. ���� ���� ����
        // 2. ȭ�� ������� ��Ʈ�� 
        // 3. ī�� ��ο�
        // 4. �˸� �ؽ�Ʈ ǥ��

        IEnumerator GameReady()
        {
            // ���� ���� ȣ��Ʈ�� �ƴ� �Ϲ� Ŭ����
            if (!PhotonNetwork.IsMasterClient)
            {
                // ȣ��Ʈ�� ������ ���������� ���������� ���
                yield return new WaitUntil(() => (GAME.IGM.Hero.Enemy.heroData.SkillCo != null));
                // ���� ȣ��Ʈ���� �� �������� ����, ���Ĵ� ȣ��Ʈ�� �������ķ� �̺�Ʈ ����
            }
            // ���� �⺻ ���� (�г��� + �� ����) ����
            SendUserInfo(GAME.Manager.NM.playerInfo.NickName,
                (int)GAME.Manager.RM.GameDeck.ownerClass);

            //���� ������ �Ǿ�����, �� ������ ��ų����Ÿ�� null�� �ƴҰ��̱⿡
            yield return new WaitUntil(()=>(GAME.IGM.Hero.Enemy.heroData.SkillCo != null));
            yield return new WaitForSeconds(0.5f);

            // ������Ŭ���̾�Ʈ ����ȭ �������� �������� ����
            if (PhotonNetwork.IsMasterClient)
            {
                #region ȭ���� ������� ��Ʈ�� ����
                // ȭ���� ������� ��Ʈ�� ����
                // ���� ���� ���� ����, ����ȭ�� ���� ������� (���� ����)
                PhotonNetwork.RaiseEvent(StartIntro, null, Both, SendOptions.SendReliable);
                #endregion

                #region �ʱ� 4���� �̴� �ִ��ڷ�ƾ ����
                // �ʱ� ���� ���� : ���� 4�� ��ο� ���� , ���� ����
                PhotonNetwork.RaiseEvent(InitDraw, null, Both, SendOptions.SendReliable);
                #endregion

                #region �����Ͱ� ���İ� ���� �� ����
                int firstID = UnityEngine.Random.Range(0, 2); // 0 or 1

                // ���İ� ����� ���� ����
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

    // �̺�Ʈ�ڵ带 Ű������ ���޹��� �̺�Ʈ ã�� ����
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