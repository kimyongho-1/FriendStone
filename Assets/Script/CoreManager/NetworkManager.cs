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
    // 유저정보
    public PlayerInfo playerInfo = new PlayerInfo();


    #region JSON
  
    #endregion

    #region LoginScene
    // 유저가 로그인씬에서 계정생성 버튼 누를시, 생성시도 
    public IEnumerator TryRegisterAccount(string id, string pw, string nickname,LoginCanvas lc)
    {
        WWWForm form = new WWWForm();
        // FORMAT형식같이 php의 포맷공간 변수명에 대입해주기
        form.AddField("id", id.Trim());
        form.AddField("pw", pw.Trim());
        form.AddField("nickname", nickname.Trim());
        
        using (UnityWebRequest www = UnityWebRequest.Post("http://211.205.174.211/KYH/RegisterNewAccount.php", form))
        {
            yield return www.SendWebRequest();

            // PHP 연결실패
            if (www.result != UnityWebRequest.Result.Success)
            {
                lc.WarningText.text = "연결 실패, 재부팅 요함";
                lc.WarningPanel.gameObject.SetActive(true);
                Debug.Log(www.error);
                lc.CheckEnter = lc.PressKey;
                yield break;
            }
            
            // 생성 결과 출력
            switch (www.downloadHandler.text.Trim())
            {
                case "SameAccount":
                    lc.WarningText.text = "이미 존재하는\n 계정입니다";
                    lc.WarningPanel.gameObject.SetActive(true);
                    lc.accountID.text = lc.accountPW.text = lc.accountNickName.text = string.Empty;
                    lc.CheckEnter = lc.PressKey;
                    break;
                case "AccountCreatedSuccess":
                    lc.WarningText.text = "계정 생성 완료!\n바로 로그인해주세요";
                    lc.WarningPanel.gameObject.SetActive(true);
                    lc.accountID.text = lc.accountPW.text = lc.accountNickName.text = string.Empty;
                    lc.AccountPanel.gameObject.SetActive(false);
                    lc.LoginPanel.gameObject.SetActive(true);
                    lc.loginID.text = id;
                    lc.CheckEnter = lc.PressKey;
                    break;
                case "SameNickName":
                    lc.WarningText.text = "이미 존재하는 NickName!\n재입력 바랍니다";
                    lc.WarningPanel.gameObject.SetActive(true);
                    lc.accountID.text = lc.accountPW.text = lc.accountNickName.text = string.Empty;
                    lc.CheckEnter = lc.PressKey;
                    break;
                default:
                    lc.WarningText.text = $"알수없는 ERROR\n{www.downloadHandler.text}";
                    lc.WarningPanel.gameObject.SetActive(true);
                    lc.CheckEnter = lc.PressKey;
                    break;
            }

      
        }
    }

    // 유저가 로그인 누를시 로그인성공여부 확인 및 씬전환
    public IEnumerator TryLogin(string id, string pw,  LoginCanvas lc)
    {
        WWWForm form = new WWWForm();
        // FORMAT형식같이 php의 포맷공간 변수명에 대입해주기
        form.AddField("id", id);
        form.AddField("pw", pw);

        using (UnityWebRequest www = UnityWebRequest.Post("http://211.205.174.211/KYH/TryLogin.php", form))
        {
            // 로딩창 실행
            lc.LoadingGO.SetActive(true);
            yield return www.SendWebRequest();

            // PHP 연결실패
            if (www.result != UnityWebRequest.Result.Success)
            { 
                lc.WarningText.text = "연결 실패, 재부팅 요함";
                lc.WarningPanel.gameObject.SetActive(true);
                Debug.Log(www.error);
                lc.CheckEnter = lc.PressKey;
                lc.LoadingGO.SetActive(false);
                yield break ;
            }

            // 생성 결과 출력
            string result = www.downloadHandler.text.Trim();
            if (result.StartsWith("LoginSuccess"))
            {
                // 로그인 성공시 => Pun커넥트 시작 + php로 기존 덱정보 로드 시작
                string[] each = www.downloadHandler.text.Trim().Split("|");
                playerInfo.ID = id;
                playerInfo.NickName = each[1];

                // Pun연결시작
                yield return GAME.Manager.StartCoroutine(GAME.Manager.PM.PunConnect());

                Debug.Log(each[2]);
                string json = each[2];
                // Php로 덱데이타 가져오기
                List<DeckDataPrototype> prototypeList = JsonConvert.DeserializeObject<List<DeckDataPrototype>>(json);
                for (int i = 0; i < prototypeList.Count; i++)
                {
                    Debug.Log($"Deserialize : {prototypeList[i].Deserialize()}");
                    GAME.Manager.RM.userDecks.Add(prototypeList[i].Deserialize());
                    yield return null;
                }

                GAME.Manager.Evt.gameObject.SetActive(false);
                // 유저의 기존 덱 정보 불러오기
                SceneManager.LoadScene("Lobby", LoadSceneMode.Additive);
            }
            else
            {
                // 로딩창 끄기
                lc.LoadingGO.SetActive(false);
                lc.WarningText.text = "계정 또는 비밀번호가\n불일치";
                lc.WarningPanel.gameObject.SetActive(true);
                lc.loginID.text = lc.loginPW.text = string.Empty;
                lc.CheckEnter = lc.PressKey;
            }
        }
    }

    
    #endregion

    #region LobbyScene
    // PHP로 새로 생성한 덱의 기본정보 전송 (From deckViewPort)
    public IEnumerator MakeDeck(string id, string name, string deckCode, Define.classType type)
    {
        // 혹시나 덱코드 70자 초과시 자르기
        if (deckCode.Length > 70)
        {
            deckCode.Substring(0,70);
        }

        WWWForm form = new WWWForm();
        // FORMAT형식같이 php의 포맷공간 변수명에 대입해주기
        form.AddField("id", id);
        form.AddField("name", name);
        form.AddField("deckCode", deckCode);
        form.AddField("classType", (int)type);
        using (UnityWebRequest www = UnityWebRequest.Post("http://211.205.174.211/KYH/MakeDeck.php", form))
        {
            yield return www.SendWebRequest();

            // PHP 연결실패
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                yield break;
            }

            Debug.Log(www.downloadHandler.text.Trim());
            // 생성 결과 출력
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
        // FORMAT형식같이 php의 포맷공간 변수명에 대입해주기
        form.AddField("id", playerInfo.ID);
        form.AddField("name", name);
        form.AddField("deckCode", deckCode);
        form.AddField("isDelete", "false");
        using (UnityWebRequest www = UnityWebRequest.Post("http://211.205.174.211/KYH/UpdateDeck.php", form))
        {

            yield return www.SendWebRequest();

            // PHP 연결실패
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                yield break;
            }

            // 성공
            if (www.downloadHandler.text.Trim().Contains("Success"))
            {
                Debug.Log(www.downloadHandler.text.Trim());
            }
            // 실패
            else
            { Debug.Log(www.downloadHandler.text.Trim()); }
        }
    }
    public IEnumerator ChangeDeckCard( string deckCode, int cardId, string isDelete)
    {
        WWWForm form = new WWWForm();
        // FORMAT형식같이 php의 포맷공간 변수명에 대입해주기
        form.AddField("id", playerInfo.ID);
        form.AddField("deckCode", deckCode);
        form.AddField("cardID", cardId);
        form.AddField("isDelete", isDelete );

        using (UnityWebRequest www = UnityWebRequest.Post("http://211.205.174.211/KYH/UpdateDeck.php", form))
        {
            yield return www.SendWebRequest();

            // PHP 연결실패
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                yield break;
            }

            Debug.Log(www.downloadHandler.text);

            // 성공
            if (www.downloadHandler.text.Trim().Contains("Success"))
            {
                Debug.Log(www.downloadHandler.text.Trim());
            }
            // 실패
            else
            {
                Debug.Log(www.downloadHandler.text.Trim());
            }

        }
    }
    public IEnumerator DeleteDeck(string name, string deckCode)
    {
        WWWForm form = new WWWForm();
        // FORMAT형식같이 php의 포맷공간 변수명에 대입해주기
        form.AddField("id", playerInfo.ID);
        form.AddField("name", name);
        form.AddField("deckCode", deckCode);
        form.AddField("isDelete", "false");
        using (UnityWebRequest www = UnityWebRequest.Post("http://211.205.174.211/KYH/DeleteDeck.php", form))
        {

            yield return www.SendWebRequest();

            // PHP 연결실패
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                yield break;
            }

            // 성공
            if (www.downloadHandler.text.Trim().Contains("Success"))
            {
                Debug.Log(www.downloadHandler.text.Trim());
            }
            // 실패
            else
            { Debug.Log(www.downloadHandler.text.Trim()); }
        }
    }
    #endregion
}
