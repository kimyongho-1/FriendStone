using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PunManager : MonoBehaviourPunCallbacks
{
    #region ������Ī��, ���� ����濡 ���� �Ë����� ����ڷ�ƾ
    IEnumerator waitCool;
    IEnumerator waitCo()
    {
        float t = 0;
        while (t < 5f)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (PhotonNetwork.CurrentRoom.Players.Count < 2)
        {
            Debug.Log("��� ��!");
            waitCool = null;
            // ���� ���� ����濡�� ������
            PhotonNetwork.LeaveRoom();
            // ���� ������ �ٽ� �����ͼ����� ���ӱ��� ���
            yield return new WaitUntil(()=>(PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer));
            // ó������ ����
            // �ٽ� ������ ���� ���� (�ٸ� ������ ��������� �𸣴�)
            StartRandomMatching();
            yield break;
        }

    }
    #endregion

    // ���漭�� ���� ����
    public IEnumerator PunConnect()
    {
        // ���漭�� ���� �õ�
        PhotonNetwork.ConnectUsingSettings();

        // ���� ���ӽõ��ϴ� Ŭ���̾�Ʈ�� �����ͼ��� ���ӱ��� ���
        while (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
        {
            yield return null;
        }
        // ���� ������ OnConnectedToMaster()
        // ���н� OnDisconnected()
    }

    // ���� ����
    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon server Connected Success");
    }
    // ���� ���н�
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log(cause.ToString());
        switch (GAME.Manager.CurrScene)
        {
            case Define.Scene.Login:
                // �α��ο��� �� ���� ����
                GAME.Manager.StopAllCoroutines();
                GAME.Manager.StartCoroutine(exit());
                IEnumerator exit()
                {
                    GAME.Manager.LC.WarningPanel.gameObject.SetActive(true);
                    GAME.Manager.LC.acceptBtn.gameObject.SetActive(false);
                    int count = 0;
                    while (count < 3)
                    {
                        GAME.Manager.LC.WarningText.text =
                            $"Photon Error\n{count}���� �����մϴ�\n\n����� �ٽ� �õ����ּ���";
                        yield return new WaitForSeconds(1f);
                        count++;
                    }
                    Application.Quit();
                }
                break;
            case Define.Scene.Lobby:
                break;
            case Define.Scene.InGame:
                break;
        }

    }

    #region �κ���� ������Ī ��ȯ

    // ������ ���ӽ���â���� ���ӽ����� ������ ��ȣ�ۿ�����  ����
    public SelectedDeckIcon sdi;

    // LobbyScene�� �������� Ŭ���� ���ӽ��� ������ ȣ��
    public void StartRandomMatching()
    {
        // �����ͼ����� �ƴ϶�� ������Ī �ٷ� ������ �Ұ�
        if (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
        {
            // �����ͼ����� �������ϱ���� ����� �������� ����
            StartCoroutine(waitMasterServer());
            IEnumerator waitMasterServer()
            {
                yield return new WaitUntil(()=>(PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer));
                PhotonNetwork.JoinRandomRoom();
            }
        }

        // ������Ī �ٷ� ����
        else
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    // ��Ī ��� �������� ��ȯ
    public bool CanCancel 
    { get { return (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.PlayerCount < 2); } }

    // ������ �κ������ ��Ī ��Ҹ� ������
    public void CancelRandomMatching()
    {
        Debug.Log("������Ī ���");
        // �������� �ڷ�ƾ ��� ����
        StopAllCoroutines();
        waitCool = null;
        StartCoroutine(wait());
        IEnumerator wait()
        {
            if (PhotonNetwork.InRoom)
            {
                Debug.Log("PhotonNetwork.InRoom : " + PhotonNetwork.InRoom);
                Debug.Log("LeaveROOM����");
                PhotonNetwork.LeaveRoom();
            }

            // ��Ī �ִϸ��̼� ���(���)�ϴ� �ִϸ��̼� ����
            float t = 0;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                float ratio = t * 4f;
                sdi.loadingPopup.transform.localScale =
                    Vector3.Lerp(new Vector3(0.3f, 0.7f, 1), Vector3.zero, ratio);
                yield return null;
            }
            GAME.Manager.Evt.enabled = true;
            sdi.loadingPopup.gameObject.SetActive(false);
            sdi.cancelBtn.gameObject.SetActive(false);
            sdi.playBtn.raycastTarget = true;
        }
    }

    // �ٸ� ������ ���� ���� �濡 ����� ȣ��
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("������ �濡 ����");
        // ������ �濡 ���Ë����� ȣ��Ǵµ�
        // �濡 �ο�2���Ͻ� Ŭ���� ��ҹ�ư ���ֱ� => �ٷ� ���ӽ����Ұ��̱⿡
        if (PhotonNetwork.CurrentRoom.Players.Count == 2)
        {
            // ��� ��ư ��Ȱ��ȭ
            sdi.cancelBtn.gameObject.SetActive(false);
            // text�� �˷��ֱ�
            sdi.matchingState.text = "������ ��븦 ã�Ҵ�!";

            // ���� �������̸�, ���� ������ ���� ���ο�2���Ͻ� �ٷ� ���ӽ���
            if (PhotonNetwork.IsMasterClient)
            {
                // ���� Ŭ���̾�Ʈ�� ȭ�鿡 ���� ��Ī�ִϸ��̼� ������Ű��
                RotationBar.stop = true;

                // ��뵦 Ȯ��
                GAME.Manager.RM.GameDeck = sdi.currDeck;
                // �� ����ȭ ����
                PhotonNetwork.AutomaticallySyncScene = true;
                // ���ÿ� �� �ε�
                PhotonNetwork.LoadLevel(2);
            }

        }
    }
    // ���� �ٸ������ �濡 �����ϰų� , ���� ����濡 ����� ȣ��
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            // ������ ��ҹ�ư�� ������ �ְ� Ȱ��ȭ
            sdi.cancelBtn.gameObject.SetActive(true);
        }
        base.OnJoinedRoom();
        Debug.Log("������ ����");

        StopAllCoroutines();
        // ��뵦 Ȯ��
        GAME.Manager.RM.GameDeck = sdi.currDeck;
        SceneManager.LoadScene("InGame",LoadSceneMode.Single);

        // ���� ������ �ƴ϶��, �ٸ� ���� ��Ī�濡 ������
        // ���ӽ����� �����Ͱ� �ڵ����� �����Ұ� ( ���� OnPlayerEnteredRoom�Լ����� )
    }
    // ������Ī ���н� ȣ��
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("���� ��� ���� ����� ����");
        base.OnJoinRandomFailed(returnCode, message);
        // ������Ī ���н�, ���� ���� ���� �ٸ������� �Ë����� �������
        PhotonNetwork.CreateRoom(
            GAME.Manager.NM.playerInfo.ID.ToString(),// ���� : ����ID�� => �ߺ������� �����״�
            new RoomOptions { MaxPlayers = 2} ); // 1vs1�����̶�
    }

    // ������Ī ������, �ٸ� ������ �������� ���� ��������� + ������ ȣ��
    public override void OnCreatedRoom()
    {

        base.OnCreatedRoom();
        Debug.Log("���� ����, �ٸ� ������ ������ ���� �� ������ ����Ұ�");
        // ���� �����ϰ� �����ð����� �ٸ������� ������� ���
        waitCool = waitCo();
        StartCoroutine(waitCool);
        // ���� �ȿý�, ���� ���� ���ְ� �ٽ� ó������ ����
    }

    // ���� ���� ���濡�� ������ �ȿͼ� ������ ������ ȣ��
    public override void OnLeftRoom()
    {
        Debug.Log("�� ������");
        // �� ���� ������
        base.OnLeftRoom();
    }
    #endregion
}
