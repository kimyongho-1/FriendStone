using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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


    #region LobbyScene ����
    public LobbyManager LM { get; set; }
    #endregion


    #region LoginScene����
    public LoginCanvas LC { get; set; }
    public Queue<GameObject> waitQueue = new Queue<GameObject>();
    // �α��� => �κ� ����ȯ�Լ�
    public void OnLobbyLoad(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name) 
        {
            case "Lobby":
                // �κ�� ��ȯ�� ������ �ʱ�ȭ ��� ������
                // �α��ξ� ������
                // ������ �̺�Ʈ�ý��� ����
                GameObject es = GameObject.Find("EventSystem");
                GameObject.Destroy(es);
                CurrScene = Define.Scene.Lobby;
                StartCoroutine(ToLobbyScene());
                IEnumerator ToLobbyScene()
                {
                    // �κ�� �� ������ LobbyManager �ʱ�ȭ ���
                    yield return new WaitUntil(() => (LM != null));
                    // ���̽����ϵ� ��� Ŭ����ȭ
                    LM.edit.GetComponentInChildren<CardSelect>().cardView.ReadyData();
                   
                    yield return new WaitUntil(() => (waitQueue.Count == 2));
                    for (int i = 0; i < 2; i++)
                    {
                        // �κ���� ���۵ɋ�, �ʱ⼼�ÿ� �°� �������� �˾�â���� ��� ����
                        waitQueue.Dequeue().gameObject.SetActive(false);
                    }
                    yield return new WaitForSeconds(2f);
                    // �κ�� ��� ������, �׶� �α��ξ� ����
                    SceneManager.UnloadSceneAsync("Login");
                    GAME.Manager.Evt.gameObject.SetActive(true);
                }
                break;
            case "InGame": 
                break;
        }
    }

    #endregion
}
