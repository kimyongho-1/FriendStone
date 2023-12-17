using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionCanvas :LobbyPopup
{
    public TextMeshProUGUI backBtn;
    public TMP_Dropdown td;
    public Scrollbar Vol, FX, BGM;
    AudioSource audioPlayer;
    private void Awake()
    {
        // 커서를 가져다 댈시 글자의 색상 변화
        GAME.Manager.UM.BindTMPInteraction(backBtn, Color.green, Color.red, BackBtn);
        // 드랍다운(프레임변경)창에 이벤트 함수 연결
        td.onValueChanged.AddListener(ChangedFrame);
        this.gameObject.SetActive(false);
        // 각종 볼륨 오디오소스에 이벤트연결
        Vol.onValueChanged.AddListener(GAME.Manager.SM.ChangedVol);
        FX.onValueChanged.AddListener(GAME.Manager.SM.FXVol);
        BGM.onValueChanged.AddListener(GAME.Manager.SM.BGMVol);
        audioPlayer = GetComponent<AudioSource>();
        GetComponent<Canvas>().sortingOrder = 1;
    }
    

    public void ChangedFrame(int val)
    {
        Application.targetFrameRate = (val == 0) ? 30 : 60;
    }

    // 뒤로가기 누를시
    public void BackBtn(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Back);
        // 이전 팝업창으로
        GAME.Manager.StartCoroutine(GAME.Manager.LM.CanvasTransition(this));
    }

}