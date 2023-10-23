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
        // tmp텍스트 버튼들에 이벤트 연결
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

    // 옵션창 설정
    public void EnterOptionCanvas(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Enter);
        GAME.Manager.StartCoroutine(GAME.Manager.LM.CanvasTransition(this, GAME.Manager.LM.option));
    }

    // 게임종료
    public void ExitGame(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Back);
        Application.Quit();
    }
}
