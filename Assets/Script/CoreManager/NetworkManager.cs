using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;
using Unity.VisualScripting;

public class PlayerInfo
{
    public string ID;
    public string NickName;
}
public class NetworkManager
{
    // ��������
    public PlayerInfo playerInfo = new PlayerInfo();


    #region JSON
  
    #endregion

    #region LoginScene
    // ������ �α��ξ����� �������� ��ư ������, �����õ� 
    public IEnumerator TryRegisterAccount(string id, string pw, string nickname,LoginCanvas lc)
    {
        WWWForm form = new WWWForm();
        // FORMAT���İ��� php�� ���˰��� ������ �������ֱ�
        form.AddField("id", id.Trim());
        form.AddField("pw", pw.Trim());
        form.AddField("nickname", nickname.Trim());
        
        using (UnityWebRequest www = UnityWebRequest.Post("http://211.205.174.211/KYH/RegisterNewAccount.php", form))
        {
            yield return www.SendWebRequest();

            // PHP �������
            if (www.result != UnityWebRequest.Result.Success)
            {
                lc.WarningText.text = "���� ����, ����� ����";
                lc.WarningPanel.gameObject.SetActive(true);
                Debug.Log(www.error);
                lc.CheckEnter = lc.PressKey;
                yield break;
            }
            
            // ���� ��� ���
            switch (www.downloadHandler.text.Trim())
            {
                case "SameAccount":
                    lc.WarningText.text = "�̹� �����ϴ�\n �����Դϴ�";
                    lc.WarningPanel.gameObject.SetActive(true);
                    lc.accountID.text = lc.accountPW.text = lc.accountNickName.text = string.Empty;
                    lc.CheckEnter = lc.PressKey;
                    break;
                case "AccountCreatedSuccess":
                    lc.WarningText.text = "���� ���� �Ϸ�!\n�ٷ� �α������ּ���";
                    lc.WarningPanel.gameObject.SetActive(true);
                    lc.accountID.text = lc.accountPW.text = lc.accountNickName.text = string.Empty;
                    lc.AccountPanel.gameObject.SetActive(false);
                    lc.LoginPanel.gameObject.SetActive(true);
                    lc.loginID.text = id;
                    lc.CheckEnter = lc.PressKey;
                    break;
                case "SameNickName":
                    lc.WarningText.text = "�̹� �����ϴ� NickName!\n���Է� �ٶ��ϴ�";
                    lc.WarningPanel.gameObject.SetActive(true);
                    lc.accountID.text = lc.accountPW.text = lc.accountNickName.text = string.Empty;
                    lc.CheckEnter = lc.PressKey;
                    break;
                default:
                    lc.WarningText.text = $"�˼����� ERROR\n{www.downloadHandler.text}";
                    lc.WarningPanel.gameObject.SetActive(true);
                    lc.CheckEnter = lc.PressKey;
                    break;
            }

      
        }
    }

    // ������ �α��� ������ �α��μ������� Ȯ�� �� ����ȯ
    public IEnumerator TryLogin(string id, string pw,  LoginCanvas lc)
    {
        WWWForm form = new WWWForm();
        // FORMAT���İ��� php�� ���˰��� ������ �������ֱ�
        form.AddField("id", id);
        form.AddField("pw", pw);

        using (UnityWebRequest www = UnityWebRequest.Post("http://211.205.174.211/KYH/TryLogin.php", form))
        {
            // �ε�â ����
            lc.LoadingGO.SetActive(true);
            yield return www.SendWebRequest();

            // PHP �������
            if (www.result != UnityWebRequest.Result.Success)
            { 
                lc.WarningText.text = "���� ����, ����� ����";
                lc.WarningPanel.gameObject.SetActive(true);
                Debug.Log(www.error);
                lc.CheckEnter = lc.PressKey;
                lc.LoadingGO.SetActive(false);
                yield break ;
            }

            // ���� ��� ���
            string result = www.downloadHandler.text.Trim();
            if (result.StartsWith("LoginSuccess"))
            {
                // �α��� ������ => PunĿ��Ʈ ���� + php�� ���� ������ �ε� ����
                string[] each = www.downloadHandler.text.Trim().Split("|");
                playerInfo.ID = id;
                playerInfo.NickName = each[1];

                // Pun�������
                yield return GAME.Manager.StartCoroutine(GAME.Manager.PM.PunConnect());

                Debug.Log(each[2]);
                string json = each[2];
                // Php�� ������Ÿ ��������
                List<DeckDataPrototype> prototypeList = JsonConvert.DeserializeObject<List<DeckDataPrototype>>(json);
                for (int i = 0; i < prototypeList.Count; i++)
                {
                    Debug.Log($"Deserialize : {prototypeList[i].Deserialize()}");
                    GAME.Manager.RM.userDecks.Add(prototypeList[i].Deserialize());
                    yield return null;
                }

                GAME.Manager.Evt.gameObject.SetActive(false);
                // ������ ���� �� ���� �ҷ�����
                SceneManager.LoadScene("Lobby", LoadSceneMode.Additive);
            }
            else
            {
                // �ε�â ����
                lc.LoadingGO.SetActive(false);
                lc.WarningText.text = "���� �Ǵ� ��й�ȣ��\n����ġ";
                lc.WarningPanel.gameObject.SetActive(true);
                lc.loginID.text = lc.loginPW.text = string.Empty;
                lc.CheckEnter = lc.PressKey;
            }
        }
    }

    
    #endregion

    #region LobbyScene
    // PHP�� ���� ������ ���� �⺻���� ���� (From deckViewPort)
    public IEnumerator MakeDeck(string id, string name, string deckCode, Define.classType type)
    {
        // Ȥ�ó� ���ڵ� 70�� �ʰ��� �ڸ���
        if (deckCode.Length > 70)
        {
            deckCode.Substring(0,70);
        }

        WWWForm form = new WWWForm();
        // FORMAT���İ��� php�� ���˰��� ������ �������ֱ�
        form.AddField("id", id);
        form.AddField("name", name);
        form.AddField("deckCode", deckCode);
        form.AddField("classType", (int)type);
        using (UnityWebRequest www = UnityWebRequest.Post("http://211.205.174.211/KYH/MakeDeck.php", form))
        {
            yield return www.SendWebRequest();

            // PHP �������
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                yield break;
            }

            Debug.Log(www.downloadHandler.text.Trim());
            // ���� ��� ���
            switch (www.downloadHandler.text.Trim())
            {
                case "Make Success":
                    Debug.Log(www.downloadHandler.text.Trim());
                    break;
                case "Make Faield":
                    Debug.Log(www.downloadHandler.text.Trim());
                    break;
                default:
                    break;
            }

        }
    }
    public IEnumerator ChangeDeckName(string name, string deckCode)
    {
        WWWForm form = new WWWForm();
        // FORMAT���İ��� php�� ���˰��� ������ �������ֱ�
        form.AddField("id", playerInfo.ID);
        form.AddField("name", name);
        form.AddField("deckCode", deckCode);
        form.AddField("isDelete", "false");
        using (UnityWebRequest www = UnityWebRequest.Post("http://211.205.174.211/KYH/UpdateDeck.php", form))
        {

            yield return www.SendWebRequest();

            // PHP �������
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                yield break;
            }

            // ����
            if (www.downloadHandler.text.Trim().Contains("Success"))
            {
                Debug.Log(www.downloadHandler.text.Trim());
            }
            // ����
            else
            { Debug.Log(www.downloadHandler.text.Trim()); }
        }
    }
    public IEnumerator ChangeDeckCard( string deckCode, int cardId, string isDelete)
    {
        WWWForm form = new WWWForm();
        // FORMAT���İ��� php�� ���˰��� ������ �������ֱ�
        form.AddField("id", playerInfo.ID);
        form.AddField("deckCode", deckCode);
        form.AddField("cardID", cardId);
        form.AddField("isDelete", isDelete );

        using (UnityWebRequest www = UnityWebRequest.Post("http://211.205.174.211/KYH/UpdateDeck.php", form))
        {
            yield return www.SendWebRequest();

            // PHP �������
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                yield break;
            }

            Debug.Log(www.downloadHandler.text);

            // ����
            if (www.downloadHandler.text.Trim().Contains("Success"))
            {
                Debug.Log(www.downloadHandler.text.Trim());
            }
            // ����
            else
            {
                Debug.Log(www.downloadHandler.text.Trim());
            }

        }
    }
    public IEnumerator DeleteDeck(string name, string deckCode)
    {
        WWWForm form = new WWWForm();
        // FORMAT���İ��� php�� ���˰��� ������ �������ֱ�
        form.AddField("id", playerInfo.ID);
        form.AddField("name", name);
        form.AddField("deckCode", deckCode);
        form.AddField("isDelete", "false");
        using (UnityWebRequest www = UnityWebRequest.Post("http://211.205.174.211/KYH/DeleteDeck.php", form))
        {

            yield return www.SendWebRequest();

            // PHP �������
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                yield break;
            }

            // ����
            if (www.downloadHandler.text.Trim().Contains("Success"))
            {
                Debug.Log(www.downloadHandler.text.Trim());
            }
            // ����
            else
            { Debug.Log(www.downloadHandler.text.Trim()); }
        }
    }
    #endregion
}
