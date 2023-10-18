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

        // ������ ������ ������ ���� �ִ��� Ȯ���� ������ �°� ����.
        GAME.Manager.waitQueue.Enqueue(this.gameObject);
        audioPlayer = gameObject.GetComponent<AudioSource>();
    }

    // Ȱ��ȭ�ø��� : ���� ������ ���� �°� ������ �������ܵ� �ʱ�ȭ
    public void OnEnable()
    {
        int deckMax = GAME.Manager.RM.userDecks.Count;

        // ������ �������� ���� NetworkManager�� ������Ÿ�� �� �������ܿ� ����
        for (int i = 0; i < userDeckIcons.Count; i++)
        {
            if (i < deckMax)
            {
                userDeckIcons[i].Init(GAME.Manager.RM.userDecks[i]);
            }
            else
            {
                // ������ ���������� ���� ���� �� �̻� ������, ���� ������ִ� ���˾� ����
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
