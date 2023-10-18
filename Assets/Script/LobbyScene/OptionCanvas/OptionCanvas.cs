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
        GAME.Manager.UM.BindTMPInteraction(backBtn, Color.green, Color.red, BackBtn);
        td.onValueChanged.AddListener(ChangedFrame);
        this.gameObject.SetActive(false);
        Vol.onValueChanged.AddListener(GAME.Manager.SM.ChangedVol);
        FX.onValueChanged.AddListener(GAME.Manager.SM.FXVol);
        BGM.onValueChanged.AddListener(GAME.Manager.SM.BGMVol);
        audioPlayer = GetComponent<AudioSource>();
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