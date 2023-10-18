using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class PlayCanvas : LobbyPopup
{
    public TextMeshProUGUI  backBtn;
    public List<DeckIcon> userDeckIcons = new List<DeckIcon>();
    public SelectedDeckIcon selectedDeckIcon; 
    public AudioSource audioPlayer;
    private void Awake()
    {
        GAME.Manager.UM.BindTMPInteraction(backBtn, Color.yellow, Color.red, BackBtn);

        // 유저가 기존에 만들어둔 덱이 있는지 확인후 설정에 맞게 세팅.
        GAME.Manager.waitQueue.Enqueue(this.gameObject);
        audioPlayer = gameObject.GetComponent<AudioSource>();
    }

    // 활성화시마다 : 현재 유저의 덱에 맞게 프리팹 덱아이콘들 초기화
    public void OnEnable()
    {
        int deckMax = GAME.Manager.RM.userDecks.Count;

        // 유저의 덱정보를 지닌 NetworkManager의 덱데이타를 각 덱아이콘에 전달
        for (int i = 0; i < userDeckIcons.Count; i++)
        {
            if (i < deckMax)
            {
                userDeckIcons[i].Init(GAME.Manager.RM.userDecks[i]);
            }
            else
            {
                // 유저가 실질적으로 지닌 덱이 더 이상 없으면, 덱을 만들수있는 빈팝업 띄우기
                userDeckIcons[i].Clear(); 
            }
        }
    }


    public void BackBtn(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Back);
        StartCoroutine(GAME.Manager.LM.CanvasTransition(this ));
    }
}
