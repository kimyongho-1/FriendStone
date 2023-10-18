using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class LobbyManager : MonoBehaviour
{
    public LobbyPopup main, play, edit, option;
    public Stack<LobbyPopup> popupIndex = new Stack<LobbyPopup>();
    AudioSource audioPlayer;
    public Dictionary<Define.OtherSound, AudioClip> sceneAudio = new Dictionary<Define.OtherSound, AudioClip>();

    // 뒤로 갈떄, 이전팝업 찾기
    public LobbyPopup GetExPopup 
    {
        get 
        {
            if (popupIndex.Count > 0)
            { return popupIndex.Pop(); }
            else { return main; }
        } 
    } 
    private void Awake()
    {
        GAME.Manager.LM = this;

        #region audio클립 모아두기 
        // 인게임씬에서 자주 사용할 클립들은, 매니저내부에서 관리하여 사용하기로 결정
        audioPlayer = GetComponent<AudioSource>();
        sceneAudio.Add(Define.OtherSound.Enter, Resources.Load<AudioClip>("Sound/LoginNLobby/Enter"));
        sceneAudio.Add(Define.OtherSound.Back, Resources.Load<AudioClip>("Sound/LoginNLobby/Back"));
        sceneAudio.Add(Define.OtherSound.Flip, Resources.Load<AudioClip>("Sound/LoginNLobby/Flip"));
        sceneAudio.Add(Define.OtherSound.Info, Resources.Load<AudioClip>("Sound/LoginNLobby/InfoSound"));
        sceneAudio.Add(Define.OtherSound.HotSelect, Resources.Load<AudioClip>("Sound/LoginNLobby/HotSelect")); 

        #endregion
    }
    // 타 컴포넌트에서, 주로 사용할 클립소스를 사용할떄, 매니저에게서 가져와 사용하기
    public AudioClip GetClip(Define.OtherSound s) { return sceneAudio[s]; }
    
    // 클립소스 참조와 재생 한번에
    public void Play(ref AudioSource audio, Define.OtherSound s)
    { audio.clip = GetClip(s); audio.Play(); }

    // 코루틴으로 만든 캔버스간 전환 효과 , Stack 인덱스 사용 버전
    public IEnumerator CanvasTransition(LobbyPopup ex, LobbyPopup next)
    {
        GAME.Manager.Evt.enabled = false;
        next.cg.alpha = 0;
        next.gameObject.SetActive(true);
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;

            ex.cg.alpha = Mathf.Lerp(1,0, t);
            next.cg.alpha = Mathf.Lerp(0,1,t);
            yield return null;
        }
        GAME.Manager.Evt.enabled = true;
        ex.gameObject.SetActive(false);

        // 지난 팝업창 다시 켤수있도록 stack에 기록
        popupIndex.Push(ex);
    }

    // 이전 팝업 호출하는 버전
    public IEnumerator CanvasTransition(LobbyPopup ex)
    {
        GAME.Manager.Evt.enabled = false;
        LobbyPopup next = popupIndex.Pop();
        next.cg.alpha = 0;
        next.gameObject.SetActive(true);
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;

            ex.cg.alpha = Mathf.Lerp(1, 0, t);
            next.cg.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }
        GAME.Manager.Evt.enabled = true;
        ex.gameObject.SetActive(false);

    }
}
