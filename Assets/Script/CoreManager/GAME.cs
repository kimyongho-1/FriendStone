using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;

public class GAME : MonoBehaviour
{
    public Define.Scene CurrScene;
    public AudioSource BGM, FX, Speech;
    private void Awake()
    {
        DontDestroyOnLoad(this);
        ui = new UIManager(GameObject.Find("@UI_Popup").GetComponent<RectTransform>(),
            GameObject.Find("UIText").GetComponent<TextMeshProUGUI>());
        rm = new ResourceManager(Resources.Load<TextAsset>("DataPath").ToString());
        sm = new SoundManager(BGM, FX, Speech);
        pm = GetComponent<PunManager>();
        CurrScene = Define.Scene.Login;
        sm.PlayBGM();
        SceneManager.sceneLoaded += OnLobbyLoad;
        Screen.SetResolution(600,400,false);
        Debug.Log("DataPath : " + Application.dataPath);
    }

    private void OnApplicationQuit()
    {
        PhotonNetwork.Disconnect();
    }

    #region ALL MANAGER

    static GAME gmInstance;
    public static GAME Manager
    {
        get
        {
            if (gmInstance == null)
            {
                gmInstance = GameObject.Find("@GameManager").GetComponent<GAME>();
            }
            return gmInstance;
        }
    }
    public EventSystem Evt 
    { 
        get 
        {
            if (evt == null)
            { evt = transform.GetComponentInChildren<EventSystem>(); }
            return evt; 
        }
    }
    UIManager ui;
    ResourceManager rm;
    NetworkManager nm = new NetworkManager();
    SoundManager sm ;
    PunManager pm;
    EventSystem evt;
    public UIManager UM { get { return ui; } }
    public ResourceManager RM { get { return rm; } }
    public SoundManager SM { get { return sm; } }
    public NetworkManager NM { get { return nm; } }
    public PunManager PM { get { return pm; } }
    #endregion

    #region InGameScene 관리
    public static InGameManager IGM { get; set; }
    #endregion

    #region LobbyScene 관리
    public LobbyManager LM { get; set; }
    #endregion


    #region LoginScene관리
    public LoginCanvas LC { get; set; }
    public Queue<GameObject> waitQueue = new Queue<GameObject>();
    // 로그인 => 로비 씬전환함수
    public void OnLobbyLoad(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name) 
        {
            case "Lobby":
                Camera.main.aspect = 16f / 9f;
                // 로비씬 전환시 실행할 초기화 모두 끝날떄
                // 로그인씬 끝내기
                // 새씬의 이벤트시스템 삭제
                GameObject es = GameObject.Find("EventSystem");
                GameObject.Destroy(es);
                StartCoroutine(ToLobbyScene());
                IEnumerator ToLobbyScene()
                {
                    // 로비씬 총 관리자 LobbyManager 초기화 대기
                    yield return new WaitUntil(() => (LM != null));
                    // 제이슨파일들 모두 클래스화
                    LM.edit.GetComponentInChildren<CardSelect>().cardView.ReadyData();
                   
                    yield return new WaitUntil(() => (waitQueue.Count == 2));
                    for (int i = 0; i < 2; i++)
                    {
                        // 로비씬이 시작될떄, 초기세팅에 맞게 꺼져야할 팝업창들은 모두 끄기
                        waitQueue.Dequeue().gameObject.SetActive(false);
                    }
                    if (CurrScene == Define.Scene.Login)
                    {
                        // 로비씬 모두 끝날시, 그때 로그인씬 해제
                        SceneManager.UnloadSceneAsync("Login").completed
                        += (AsyncOperation op) => { StartCoroutine(LM.main.GetComponent<MainCanvas>().PlayLobbyIntro()); };
                    }
                    else
                    { 
                        // 로비씬 모두 끝날시, 그때 로그인씬 해제
                        SceneManager.UnloadSceneAsync("InGame").completed
                        += (AsyncOperation op) => 
                        {
                            // 재접속
                            // StartCoroutine(PM.PunConnect());
                            Destroy(GAME.Manager.GetComponent<PhotonView>());
                            GAME.Manager.AddComponent<PhotonView>();
                            StartCoroutine(LM.main.GetComponent<MainCanvas>().PlayLobbyIntro());
                        };
                    }
                    Camera.main.aspect = 16f / 9f;
                    CurrScene = Define.Scene.Lobby;
                    sm.PlayBGM();
                    GAME.Manager.Evt.gameObject.SetActive(true);
                }
                break;

            case "InGame":
                Camera.main.aspect = 16f / 9f;
                Debug.Log("GM에서 호출");
                sm.PlayBGM();

                break;
        }
    }

    #endregion
}
