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
        // Enter�� TabŰ ���������� ȣ�� �̺�Ʈ ����
        CheckEnter -= PressKey;
        CheckEnter += PressKey;

        GAME.Manager.UM.BindUIPopup(accountBtn.gameObject, 1f, new Vector3(100f,20f,0),
            Define.PopupScale.Medium, "������ �����ϼ���\n���� ������ϴ�!");

        // ��������ĭ�� �� ��ǲ�ʵ� Ŀ�� ������ ��� �ȳ�â ȣ��
        GAME.Manager.UM.BindUIPopup(accountID.gameObject, 1f, new Vector3(-130f, -50f, 0),
            Define.PopupScale.Medium, "<color=green>ID<color=black>\n�α��ο� ����� ID�Դϴ�.\n������ �����˴ϴ�\n�ִ� 12����");
        GAME.Manager.UM.BindUIPopup(accountPW.gameObject, 1f, new Vector3(-130f, -120f, 0),
            Define.PopupScale.Medium, "<color=green>PW<color=black>\n����� �α��ν� �� PW�Դϴ�.\n<color=red>�������� �Ұ�\n�ִ� 12����");
        GAME.Manager.UM.BindUIPopup(accountNickName.gameObject, 1f, new Vector3(-130f, 76f, 0),
            Define.PopupScale.Medium, "<color=green>�г���<color=black>\n���ӳ�����, ���濡�� ������\n�̸��Դϴ�.10~12���ڰ� �ִ��Դϴ�.");
    }
    private void Update()
    {
        if (GAME.Manager.Evt == null) { return; }
        CheckEnter?.Invoke();
    }

    #region �α����г�

    // �α��� ��ư Ŭ����
    public void OnClickedLogin()
    {
        // tab + enter ����Ű ��� ����
        CheckEnter = null;

        #region Temp Area

        // ���⼭���� TEMP
        StartCoroutine(Temp());
        loginBtn.enabled = false;
        IEnumerator Temp()
        {
            yield return GAME.Manager.StartCoroutine(GAME.Manager.PM.PunConnect());
            yield return new WaitForSeconds(1f);
            // TEST ���� ,no php
            GAME.Manager.NM.playerInfo = new PlayerInfo() { ID = "123", NickName = "test" };
            GAME.Manager.RM.GameDeck = new DeckData();
            for (int i = 33; i < 43; i++)
            {
                // ���ҽ� �Ŵ����� ��θ� ��ȯ �޴� ��ųʸ� ���� ī��Ÿ�԰� ī�嵥���� ã��
                Define.cardType type = GAME.Manager.RM.PathFinder.Dic[i].type;
                string jsonFile = GAME.Manager.RM.PathFinder.Dic[i].GetJson();
                CardData card = null;
                // Ȯ�ε� ī��Ÿ������, ���� ī��Ÿ������ Ŭ����ȭ
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
         
            // ������Ī ����
            Photon.Pun.PhotonNetwork.JoinRandomRoom();

        }

        #endregion
        // �α��� �õ�
        // GAME.Manager.StartCoroutine(GAME.Manager.NM.TryLogin
        //     (loginID.text, loginPW.text, this));
    }
    // �������� ��ư Ŭ����
    public void OnClickedOpenAccount() 
    {
        // ��ư ��ȣ�ۿ�� �г� ���� ����
        accountBtn.interactable = false;
        LoginPanel.gameObject.SetActive(false);
        AccountPanel.gameObject.SetActive(true);
        accountBtn.interactable = true;
        // �Է��ʵ� ���� ����
        loginID.text = loginPW.text = string.Empty;
    }
    #endregion

    #region ���������г�
    // ���� ���� ��ư Ŭ���� ȣ��
    public void OnClickedMakeAccount()
    {
        // tab + enter ����Ű ��� ����
        CheckEnter = null;

        #region ����ó��, ��ĭ �˻�
        string result = accountID.text + accountPW.text;

        // ID PW ��� �˻�
        if (string.IsNullOrEmpty(result) || result.Contains(" "))
        {
            WarningText.text =
            "<color=red>ID<color=black> �Ǵ� <color=red>PW<color=black>��\n��ȿ���� �ʽ��ϴ�\n (���� ���ԺҰ�)";
            WarningPanel.gameObject.SetActive(true);
            CheckEnter = PressKey;
            return;
        }

        // NickNameĭ �˻�
        if (string.IsNullOrEmpty(accountNickName.text) || accountNickName.text.Contains(" "))
        {
            WarningText.text = "<color=red>�г���<color=black> �Է��� �ʼ��̸�\n ���� ���ԺҰ��Դϴ�";
            WarningPanel.gameObject.SetActive(true);
            CheckEnter = PressKey;
            return;
        }
        #endregion

        // �� ���ܻ��� ������ ���� �õ�
        GAME.Manager.StartCoroutine(GAME.Manager.NM.TryRegisterAccount
            (accountID.text, accountPW.text, accountNickName.text, this));
    }

    // �ڷΰ��� ��ư Ŭ���� ȣ��
    public void OnClickedBackBtn()
    {
        // ��ư ��ȣ�ۿ�� �г� ���� ����
        backBtn.interactable = false;
        AccountPanel.gameObject.SetActive(false);
        LoginPanel.gameObject.SetActive(true);
        backBtn.interactable = true;
        // �Է��ʵ� ���� ����
        accountID.text = accountPW.text = accountNickName.text = string.Empty;
    }
    #endregion

    // ����˾�â���� ��ư Ŭ����
    public void OnClickedAcceptBtn()
    {
        WarningPanel.gameObject.SetActive(false);
    }
    // Enter�� Tab��ư �Է½� ȣ��
    public void PressKey()
    {
        // Ű�Է� ���ų� ���� ���â ������ Ű�Է��� ���
        if (!Input.anyKey || WarningPanel.gameObject.activeSelf ==true) { return; }

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            // ���� �α����г� �Ǵ� ���������г� �����ִ� ȭ�鿡 �°� �ڵ���ư Ŭ�� ����
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
            // ���� �ƹ� �Է�ĭ���� �Է�Ŭ�� �����ٸ�
            if (GAME.Manager.Evt.currentSelectedGameObject == null)
            {
                // id�Է�â���� �����̵�
                GAME.Manager.Evt.SetSelectedGameObject
                    ((LoginPanel.gameObject.activeSelf == true) ? loginID.gameObject : accountID.gameObject);
                return;
            }

            // �α���â���� TabŰ �Է½�
            else if (LoginPanel.gameObject.activeSelf == true)
            {
                int idx = GAME.Manager.Evt.currentSelectedGameObject.transform.GetSiblingIndex();

                if (idx == 0)
                {
                    GAME.Manager.Evt.SetSelectedGameObject(loginPW.gameObject);
                }
                // pw �Է�â���� �ѹ��� tabŬ���� �Է�â �����
                else
                {
                    GAME.Manager.Evt.SetSelectedGameObject(null);
                }
            }

            // ��������â���� TabŰ �Է½�
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
