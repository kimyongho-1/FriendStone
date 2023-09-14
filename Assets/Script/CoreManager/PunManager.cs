using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PunManager : MonoBehaviourPunCallbacks
{
    #region 랜덤매칭중, 내가 만든방에 누가 올떄까지 대기코루틴
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
            Debug.Log("대기 끝!");
            waitCool = null;
            // 현재 내가 만든방에서 나가기
            PhotonNetwork.LeaveRoom();
            // 방을 떠나고 다시 마스터서버로 접속까지 대기
            yield return new WaitUntil(()=>(PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer));
            // 처음부터 시작
            // 다시 랜덤방 입장 시작 (다른 유저가 만들었을지 모르니)
            StartRandomMatching();
            yield break;
        }

    }
    #endregion

    // 포톤서버 접속 시작
    public IEnumerator PunConnect()
    {
        // 포톤서버 접속 시도
        PhotonNetwork.ConnectUsingSettings();

        // 현재 접속시도하는 클라이언트가 마스터서버 접속까지 대기
        while (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
        {
            yield return null;
        }
        // 접속 성공시 OnConnectedToMaster()
        // 실패시 OnDisconnected()
    }

    // 접속 성공
    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon server Connected Success");
    }
    // 접속 실패시
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log(cause.ToString());
        switch (GAME.Manager.CurrScene)
        {
            case Define.Scene.Login:
                // 로그인에서 펀 접속 실패
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
                            $"Photon Error\n{count}초후 종료합니다\n\n잠시후 다시 시도해주세요";
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

    #region 로비씬의 랜덤매칭 순환

    // 유저가 게임실행창에서 게임시작을 누를떄 상호작용위해  참조
    public SelectedDeckIcon sdi;

    // LobbyScene의 덱아이콘 클릭후 게임시작 누를시 호출
    public void StartRandomMatching()
    {
        // 마스터서버가 아니라면 랜덤매칭 바로 실행이 불가
        if (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
        {
            // 마스터서버로 재접속하기까지 대기후 랜덤입장 시작
            StartCoroutine(waitMasterServer());
            IEnumerator waitMasterServer()
            {
                yield return new WaitUntil(()=>(PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer));
                PhotonNetwork.JoinRandomRoom();
            }
        }

        // 랜덤매칭 바로 시작
        else
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    // 매칭 취소 가능한지 반환
    public bool CanCancel 
    { get { return (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.PlayerCount < 2); } }

    // 유저가 로비씬에서 매칭 취소를 누를시
    public void CancelRandomMatching()
    {
        Debug.Log("랜덤매칭 취소");
        // 진행중인 코루틴 모두 중지
        StopAllCoroutines();
        waitCool = null;
        StartCoroutine(wait());
        IEnumerator wait()
        {
            if (PhotonNetwork.InRoom)
            {
                Debug.Log("PhotonNetwork.InRoom : " + PhotonNetwork.InRoom);
                Debug.Log("LeaveROOM실행");
                PhotonNetwork.LeaveRoom();
            }

            // 매칭 애니메이션 취소(축소)하는 애니메이션 실행
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

    // 다른 유저가 내가 속한 방에 입장시 호출
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("누군가 방에 입장");
        // 누군가 방에 들어올떄마다 호출되는데
        // 방에 인원2명일시 클라모두 취소버튼 없애기 => 바로 게임시작할것이기에
        if (PhotonNetwork.CurrentRoom.Players.Count == 2)
        {
            // 취소 버튼 비활성화
            sdi.cancelBtn.gameObject.SetActive(false);
            // text로 알려주기
            sdi.matchingState.text = "적합한 상대를 찾았다!";

            // 내가 마스터이며, 현재 누군가 들어와 방인원2명일시 바로 게임시작
            if (PhotonNetwork.IsMasterClient)
            {
                // 현재 클라이언트의 화면에 보일 매칭애니메이션 중지시키기
                RotationBar.stop = true;

                // 사용덱 확정
                GAME.Manager.RM.GameDeck = sdi.currDeck;
                // 씬 동기화 설정
                PhotonNetwork.AutomaticallySyncScene = true;
                // 동시에 씬 로드
                PhotonNetwork.LoadLevel(2);
            }

        }
    }
    // 내가 다른사람의 방에 입장하거나 , 내가 만든방에 입장시 호출
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            // 유저가 취소버튼을 누를수 있게 활성화
            sdi.cancelBtn.gameObject.SetActive(true);
        }
        base.OnJoinedRoom();
        Debug.Log("방입장 성공");

        StopAllCoroutines();
        // 사용덱 확정
        GAME.Manager.RM.GameDeck = sdi.currDeck;
        SceneManager.LoadScene("InGame",LoadSceneMode.Single);

        // 내가 방장이 아니라면, 다른 유저 매칭방에 잡힌것
        // 게임시작은 마스터가 자동으로 시작할것 ( 위의 OnPlayerEnteredRoom함수에서 )
    }
    // 랜덤매칭 실패시 호출
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("방이 없어서 내가 만들기 실행");
        base.OnJoinRandomFailed(returnCode, message);
        // 랜덤매칭 실패시, 내가 방을 만들어서 다른유저가 올떄까지 대기하자
        PhotonNetwork.CreateRoom(
            GAME.Manager.NM.playerInfo.ID.ToString(),// 방제 : 유저ID명 => 중복될일이 없을테니
            new RoomOptions { MaxPlayers = 2} ); // 1vs1게임이라서
    }

    // 랜덤매칭 시작후, 다른 유저와 안잡히면 내가 방생성시작 + 성공시 호출
    public override void OnCreatedRoom()
    {

        base.OnCreatedRoom();
        Debug.Log("내가 방장, 다른 유저가 안잡혀 내가 방 생성후 대기할것");
        // 방을 생성하고 일정시간동안 다른유저가 오기까지 대기
        waitCool = waitCo();
        StartCoroutine(waitCool);
        // 만약 안올시, 내가 방을 없애고 다시 처음부터 시작
    }

    // 내가 만든 대기방에서 유저가 안와서 내방을 나갈떄 호출
    public override void OnLeftRoom()
    {
        Debug.Log("방 떠나기");
        // 내 방을 나간것
        base.OnLeftRoom();
    }
    #endregion
}
