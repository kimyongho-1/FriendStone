using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainCanvas : LobbyPopup
{
    public TextMeshProUGUI Play, Option,Exit;
    AudioSource audioPlayer;
    public CanvasGroup introCanvas;
    private void Awake()
    {
        // tmp�ؽ�Ʈ ��ư�鿡 �̺�Ʈ ����
        GAME.Manager.UM.BindTMPInteraction(Play ,Color.green, Color.red, EnterPlayCanvas);
        GAME.Manager.UM.BindTMPInteraction(Option, Color.green, Color.red, EnterOptionCanvas);
        GAME.Manager.UM.BindTMPInteraction(Exit, Color.green, Color.red, ExitGame);
        audioPlayer = gameObject.GetComponent<AudioSource>();
    }
    public IEnumerator PlayLobbyIntro() 
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            introCanvas.alpha = t;
            yield return null;
        }
    }

    public void EnterPlayCanvas(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer , Define.OtherSound.Enter);
        GAME.Manager.StartCoroutine(GAME.Manager.LM.CanvasTransition(this, GAME.Manager.LM.play)) ;
    }

    // �ɼ�â ����
    public void EnterOptionCanvas(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Enter);
        GAME.Manager.StartCoroutine(GAME.Manager.LM.CanvasTransition(this, GAME.Manager.LM.option));
    }

    // ��������
    public void ExitGame(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Back);
        Application.Quit();
    }
}
