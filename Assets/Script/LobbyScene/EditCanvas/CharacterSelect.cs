using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelect :LobbyPopup
{
    public Image HJ, HZ, KH; // 각 캐릭터 선택아이콘
    public TextMeshProUGUI backBtn;
    EditCanvas editCanvas;
    AudioSource audioPlayer;
    private void Awake()
    {
        // 각 버튼과, 이미지 클릭시 호출할 이벤트함수 연결
        GAME.Manager.UM.BindTMPInteraction(backBtn, Color.yellow, Color.red, BackBtn);
        GAME.Manager.UM.BindEvent(HJ.gameObject, StartMakeDeck, Define.Mouse.ClickL);
        GAME.Manager.UM.BindEvent(HZ.gameObject, StartMakeDeck, Define.Mouse.ClickL);
        GAME.Manager.UM.BindEvent(KH.gameObject, StartMakeDeck, Define.Mouse.ClickL);

        editCanvas = GetComponentInParent<EditCanvas>();
        audioPlayer = editCanvas.audioPlayer;
    }
    // 뒤로가기 버튼 클릭시, 전환실행
    public void BackBtn(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Back);
        StartCoroutine(GAME.Manager.LM.CanvasTransition(GAME.Manager.LM.edit)) ;
    }

    // 캐릭터 아이콘을 선택시, 해당 아이콘 환경으로의 덱제작 페이지로 이동
    public void StartMakeDeck(GameObject go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.HotSelect);
        // 인덱스 순서로 어떤 직업을 선택했는지 확인하여 덱 편집창으로 이동
        int idx = go.transform.GetSiblingIndex();
        // 덱 만들 준비시작
        editCanvas.cardStage.MakeNewDeck((Define.classType)idx);
        // 현재 선택한 아이콘 환경의 덱 제작 이동 코루틴
        GAME.Manager.StartCoroutine(editCanvas.Transition(this, editCanvas.cardStage, idx));
    }

}
