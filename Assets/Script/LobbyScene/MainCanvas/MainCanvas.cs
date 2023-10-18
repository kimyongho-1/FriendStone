using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainCanvas : LobbyPopup
{
    public TextMeshProUGUI Play, Option,Exit;
    AudioSource audioPlayer;
    private void Awake()
    {
        // tmp텍스트 버튼들에 이벤트 연결
        GAME.Manager.UM.BindTMPInteraction(Play ,Color.green, Color.red, EnterPlayCanvas);
        GAME.Manager.UM.BindTMPInteraction(Option, Color.green, Color.red, EnterOptionCanvas);
        GAME.Manager.UM.BindTMPInteraction(Exit, Color.green, Color.red, ExitGame);
        audioPlayer = gameObject.GetComponent<AudioSource>();
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
