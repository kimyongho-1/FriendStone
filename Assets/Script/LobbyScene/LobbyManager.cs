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

    // �ڷ� ����, �����˾� ã��
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

        #region audioŬ�� ��Ƶα� 
        // �ΰ��Ӿ����� ���� ����� Ŭ������, �Ŵ������ο��� �����Ͽ� ����ϱ�� ����
        audioPlayer = GetComponent<AudioSource>();
        sceneAudio.Add(Define.OtherSound.Enter, Resources.Load<AudioClip>("Sound/LoginNLobby/Enter"));
        sceneAudio.Add(Define.OtherSound.Back, Resources.Load<AudioClip>("Sound/LoginNLobby/Back"));
        sceneAudio.Add(Define.OtherSound.Flip, Resources.Load<AudioClip>("Sound/LoginNLobby/Flip"));
        sceneAudio.Add(Define.OtherSound.Info, Resources.Load<AudioClip>("Sound/LoginNLobby/InfoSound"));
        sceneAudio.Add(Define.OtherSound.HotSelect, Resources.Load<AudioClip>("Sound/LoginNLobby/HotSelect")); 

        #endregion
    }
    // Ÿ ������Ʈ����, �ַ� ����� Ŭ���ҽ��� ����ҋ�, �Ŵ������Լ� ������ ����ϱ�
    public AudioClip GetClip(Define.OtherSound s) { return sceneAudio[s]; }
    
    // Ŭ���ҽ� ������ ��� �ѹ���
    public void Play(ref AudioSource audio, Define.OtherSound s)
    { audio.clip = GetClip(s); audio.Play(); }

    // �ڷ�ƾ���� ���� ĵ������ ��ȯ ȿ�� , Stack �ε��� ��� ����
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

        // ���� �˾�â �ٽ� �Ӽ��ֵ��� stack�� ���
        popupIndex.Push(ex);
    }

    // ���� �˾� ȣ���ϴ� ����
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
