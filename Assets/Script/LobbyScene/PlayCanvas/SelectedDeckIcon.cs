using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class SelectedDeckIcon : MonoBehaviour
{
    public TextMeshProUGUI deckName, deckCount, classType , editBtn, playBtn, backBtn , cancelBtn, matchingState;
    public GameObject loadingPopup;
    public Image classIcon, transitionPanel;
    public DeckData currDeck;
    PlayCanvas playCanvas;
    AudioSource audioPlayer;
    public RotationBar rotate;
    private void OnDisable()
    {
        this.gameObject.SetActive(false);
        cancelBtn.gameObject.SetActive(false); 
        matchingState.text = "<color=green>상대를 찾고 있는중";
    }

    private void Awake()
    {
        GAME.Manager.PM.sdi = this;
        playCanvas = GetComponentInParent<PlayCanvas>();
        GAME.Manager.UM.BindTMPInteraction(editBtn, Color.green, Color.magenta, StartEditDeck );
        GAME.Manager.UM.BindTMPInteraction(backBtn, Color.green, Color.magenta, BackBtn);
        GAME.Manager.UM.BindTMPInteraction(playBtn, Color.green, Color.magenta, StartGame);
        GAME.Manager.UM.BindTMPInteraction(cancelBtn, Color.green, Color.magenta,CancelMatching);
        loadingPopup.gameObject.SetActive(false);
        audioPlayer = playCanvas.audioPlayer;
    }

    // 유저가 자신의 덱을 클릭시, 이 팝업창을 호출 및 선택덱에 맞게 내용 모두 초기화
    public void OpenSelectedDeckIcon(DeckData data)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.HotSelect);
        int cardCount = data.cards.Values.Sum();
        // 선택한 덱정보로 초기화
        currDeck = data; 
        deckName.text = data.deckName;
        deckCount.text = $"{cardCount}/20 {((cardCount == 20) ? "<color=blue>\n게임 가능" : $" <color=red>\n{20-cardCount}장 부족!")}";
        classType.text = $"{data.ownerClass}";
        classIcon.sprite = GAME.Manager.RM.GetHeroImage(data.ownerClass);

        // 덱의 카드수 20장 미만일시, 시작버튼 못 누르게 설정
        playBtn.raycastTarget = (cardCount == 20);
        playBtn.color = (cardCount == 20) ? Color.black : Color.red ;
        this.gameObject.SetActive(true);
    }


    // 편집버튼 클릭시 자신의 덱을 편집 시작
    public void StartEditDeck(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Enter);
        // 편집캔버스로 전환
        GAME.Manager.LM.edit.GetComponent<EditCanvas>().StartEditMode(currDeck);
        GAME.Manager.StartCoroutine(GAME.Manager.LM.CanvasTransition(playCanvas, GAME.Manager.LM.edit));
    }

    // 게임시작 누를시 랜덤매칭 시작
    public void StartGame(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.HotSelect);
        // 사용덱 확정
        GAME.Manager.RM.GameDeck =currDeck;

        playBtn.raycastTarget = false;
        // 매칭을 표현하는 애니메이션 코루틴으로 실행
        GAME.Manager.StartCoroutine(RandomSlotAnim());
        IEnumerator RandomSlotAnim()
        {
            // 시작 스케일 초기화
            loadingPopup.transform.localScale = Vector3.zero;
            loadingPopup.gameObject.SetActive(true);
            float t = 0;
            while (t < 0.4f)
            { 
                t += Time.deltaTime;
                float ratio = t * 2.5f;
                loadingPopup.transform.localScale =
                    Vector3.Lerp(Vector3.zero , new Vector3(0.3f,0.7f,1) , ratio);
                yield return null;
            }
        }

        // 랜덤매칭 시작
        GAME.Manager.PM.StartRandomMatching();

    }
    public void CancelMatching(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Back);
        // 만약 랜덤매칭이 잡혀버렸다면, 취소
        if (!GAME.Manager.PM.CanCancel) { return; }
        // 매칭 잡혓을시 stop변수 true로 변경되기에 취소
        if (rotate.stop == true) { return; }

        StopAllCoroutines();
        // 애니메이션 진행되는동안 이벤트 클릭방지
        GAME.Manager.Evt.enabled = false;
        // pun매니저의 게임룸 퇴장 및 대기코루틴등 모두 중지
        GAME.Manager.PM.CancelRandomMatching(); // 펀매니저 내부에서 리브룸 실행 (방에 입성대기후)
    }

    public void BackBtn(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Back);
        this.gameObject.SetActive(false);
    }
}
