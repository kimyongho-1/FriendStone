using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;

public class LoginCanvas : MonoBehaviour
{
    public GameObject LoadingGO, LoginPanel, AccountPanel, WarningPanel;
    public TMP_InputField loginID, loginPW, accountID, accountPW, accountNickName;
    public Button loginBtn, accountBtn, createBtn, backBtn, acceptBtn;
    public TextMeshProUGUI WarningText;
    public System.Action CheckEnter;

    private void Awake()
    {
        GAME.Manager.LC = this;
        // Enter와 Tab키 누를떄마다 호출 이벤트 연결
        CheckEnter -= PressKey;
        CheckEnter += PressKey;

        GAME.Manager.UM.BindUIPopup(accountBtn.gameObject, 1f, new Vector3(100f,20f,0),
            Define.PopupScale.Medium, "계정을 생성하세요\n무려 무료랍니다!");

        // 계정생성칸의 각 인풋필드 커서 가져다 댈시 안내창 호출
        GAME.Manager.UM.BindUIPopup(accountID.gameObject, 1f, new Vector3(-130f, -50f, 0),
            Define.PopupScale.Medium, "<color=green>ID<color=black>\n로그인에 사용할 ID입니다.\n공란은 삭제됩니다\n최대 12글자");
        GAME.Manager.UM.BindUIPopup(accountPW.gameObject, 1f, new Vector3(-130f, -120f, 0),
            Define.PopupScale.Medium, "<color=green>PW<color=black>\n당신이 로그인시 쓸 PW입니다.\n<color=red>공란포함 불가\n최대 12글자");
        GAME.Manager.UM.BindUIPopup(accountNickName.gameObject, 1f, new Vector3(-130f, 76f, 0),
            Define.PopupScale.Medium, "<color=green>닉네임<color=black>\n게임내에서, 상대방에게 보여질\n이름입니다.10~12글자가 최대입니다.");
    }
    private void Update()
    {
        if (GAME.Manager.Evt == null) { return; }
        CheckEnter?.Invoke();
    }

    #region 로그인패널

    // 로그인 버튼 클릭시
    public void OnClickedLogin()
    {
        // tab + enter 단축키 잠시 해제
        CheckEnter = null;

        #region Temp Area

        // 여기서부터 TEMP
        StartCoroutine(Temp());
        loginBtn.enabled = false;
        IEnumerator Temp()
        {
            yield return GAME.Manager.StartCoroutine(GAME.Manager.PM.PunConnect());
            yield return new WaitForSeconds(1f);
            // TEST 버전 ,no php
            GAME.Manager.NM.playerInfo = new PlayerInfo() { ID = "123", NickName = "test" };
            GAME.Manager.RM.GameDeck = new DeckData();
            for (int i = 33; i < 43; i++)
            {
                // 리소스 매니저의 경로를 반환 받는 딕셔너리 통해 카드타입과 카드데이터 찾기
                Define.cardType type = GAME.Manager.RM.PathFinder.Dic[i].type;
                string jsonFile = GAME.Manager.RM.PathFinder.Dic[i].GetJson();
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
                GAME.Manager.RM.GameDeck.cards.Add(card, 2);

            }
         
            // 랜덤매칭 시작
            Photon.Pun.PhotonNetwork.JoinRandomRoom();

        }

        #endregion
        // 로그인 시도
        // GAME.Manager.StartCoroutine(GAME.Manager.NM.TryLogin
        //     (loginID.text, loginPW.text, this));
    }
    // 계정생성 버튼 클릭시
    public void OnClickedOpenAccount() 
    {
        // 버튼 상호작용과 패널 끄기 끄기
        accountBtn.interactable = false;
        LoginPanel.gameObject.SetActive(false);
        AccountPanel.gameObject.SetActive(true);
        accountBtn.interactable = true;
        // 입력필드 내용 비우기
        loginID.text = loginPW.text = string.Empty;
    }
    #endregion

    #region 계정생성패널
    // 계정 생성 버튼 클릭시 호출
    public void OnClickedMakeAccount()
    {
        // tab + enter 단축키 잠시 해제
        CheckEnter = null;

        #region 예외처리, 빈칸 검사
        string result = accountID.text + accountPW.text;

        // ID PW 모두 검사
        if (string.IsNullOrEmpty(result) || result.Contains(" "))
        {
            WarningText.text =
            "<color=red>ID<color=black> 또는 <color=red>PW<color=black>가\n유효하지 않습니다\n (띄어쓰기 포함불가)";
            WarningPanel.gameObject.SetActive(true);
            CheckEnter = PressKey;
            return;
        }

        // NickName칸 검사
        if (string.IsNullOrEmpty(accountNickName.text) || accountNickName.text.Contains(" "))
        {
            WarningText.text = "<color=red>닉네임<color=black> 입력은 필수이며\n 띄어쓰기 포함불가입니다";
            WarningPanel.gameObject.SetActive(true);
            CheckEnter = PressKey;
            return;
        }
        #endregion

        // 위 예외사항 없을시 생성 시도
        GAME.Manager.StartCoroutine(GAME.Manager.NM.TryRegisterAccount
            (accountID.text, accountPW.text, accountNickName.text, this));
    }

    // 뒤로가기 버튼 클릭시 호출
    public void OnClickedBackBtn()
    {
        // 버튼 상호작용과 패널 끄기 끄기
        backBtn.interactable = false;
        AccountPanel.gameObject.SetActive(false);
        LoginPanel.gameObject.SetActive(true);
        backBtn.interactable = true;
        // 입력필드 내용 비우기
        accountID.text = accountPW.text = accountNickName.text = string.Empty;
    }
    #endregion

    // 경고팝업창에서 버튼 클릭시
    public void OnClickedAcceptBtn()
    {
        WarningPanel.gameObject.SetActive(false);
    }
    // Enter와 Tab버튼 입력시 호출
    public void PressKey()
    {
        // 키입력 없거나 현재 경고창 있을시 키입력은 취소
        if (!Input.anyKey || WarningPanel.gameObject.activeSelf ==true) { return; }

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            // 현재 로그인패널 또는 계정생성패널 켜져있는 화면에 맞게 자동버튼 클릭 실행
            if (LoginPanel.gameObject.activeSelf == true)
            {
                OnClickedLogin();
                return;
            }

            else
            {
                OnClickedMakeAccount();
                return;
            }
        }

        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            // 현재 아무 입력칸에도 입력클릭 없었다면
            if (GAME.Manager.Evt.currentSelectedGameObject == null)
            {
                // id입력창으로 강제이동
                GAME.Manager.Evt.SetSelectedGameObject
                    ((LoginPanel.gameObject.activeSelf == true) ? loginID.gameObject : accountID.gameObject);
                return;
            }

            // 로그인창에서 Tab키 입력시
            else if (LoginPanel.gameObject.activeSelf == true)
            {
                int idx = GAME.Manager.Evt.currentSelectedGameObject.transform.GetSiblingIndex();

                if (idx == 0)
                {
                    GAME.Manager.Evt.SetSelectedGameObject(loginPW.gameObject);
                }
                // pw 입력창에서 한번더 tab클릭시 입력창 벗어나기
                else
                {
                    GAME.Manager.Evt.SetSelectedGameObject(null);
                }
            }

            // 계정생성창에서 Tab키 입력시
            else
            {
                int idx = GAME.Manager.Evt.currentSelectedGameObject.transform.GetSiblingIndex();
                switch (idx)
                {
                    case 0:
                        GAME.Manager.Evt.SetSelectedGameObject(accountPW.gameObject); break;
                    case 1:
                        GAME.Manager.Evt.SetSelectedGameObject(accountNickName.gameObject); break;
                    default:
                        GAME.Manager.Evt.SetSelectedGameObject(null); break;
                }
            }
            return;
        }

    }
}
