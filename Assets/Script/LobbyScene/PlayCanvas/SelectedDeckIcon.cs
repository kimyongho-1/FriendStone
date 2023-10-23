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
        matchingState.text = "<color=green>��븦 ã�� �ִ���";
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

    // ������ �ڽ��� ���� Ŭ����, �� �˾�â�� ȣ�� �� ���õ��� �°� ���� ��� �ʱ�ȭ
    public void OpenSelectedDeckIcon(DeckData data)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.HotSelect);
        int cardCount = data.cards.Values.Sum();
        // ������ �������� �ʱ�ȭ
        currDeck = data; 
        deckName.text = data.deckName;
        deckCount.text = $"{cardCount}/20 {((cardCount == 20) ? "<color=blue>\n���� ����" : $" <color=red>\n{20-cardCount}�� ����!")}";
        classType.text = $"{data.ownerClass}";
        classIcon.sprite = GAME.Manager.RM.GetHeroImage(data.ownerClass);

        // ���� ī��� 20�� �̸��Ͻ�, ���۹�ư �� ������ ����
        playBtn.raycastTarget = (cardCount == 20);
        playBtn.color = (cardCount == 20) ? Color.black : Color.red ;
        this.gameObject.SetActive(true);
    }


    // ������ư Ŭ���� �ڽ��� ���� ���� ����
    public void StartEditDeck(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Enter);
        // ����ĵ������ ��ȯ
        GAME.Manager.LM.edit.GetComponent<EditCanvas>().StartEditMode(currDeck);
        GAME.Manager.StartCoroutine(GAME.Manager.LM.CanvasTransition(playCanvas, GAME.Manager.LM.edit));
    }

    // ���ӽ��� ������ ������Ī ����
    public void StartGame(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.HotSelect);
        // ��뵦 Ȯ��
        GAME.Manager.RM.GameDeck =currDeck;

        playBtn.raycastTarget = false;
        // ��Ī�� ǥ���ϴ� �ִϸ��̼� �ڷ�ƾ���� ����
        GAME.Manager.StartCoroutine(RandomSlotAnim());
        IEnumerator RandomSlotAnim()
        {
            // ���� ������ �ʱ�ȭ
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

        // ������Ī ����
        GAME.Manager.PM.StartRandomMatching();

    }
    public void CancelMatching(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Back);
        // ���� ������Ī�� �������ȴٸ�, ���
        if (!GAME.Manager.PM.CanCancel) { return; }
        // ��Ī �������� stop���� true�� ����Ǳ⿡ ���
        if (rotate.stop == true) { return; }

        StopAllCoroutines();
        // �ִϸ��̼� ����Ǵµ��� �̺�Ʈ Ŭ������
        GAME.Manager.Evt.enabled = false;
        // pun�Ŵ����� ���ӷ� ���� �� ����ڷ�ƾ�� ��� ����
        GAME.Manager.PM.CancelRandomMatching(); // �ݸŴ��� ���ο��� ����� ���� (�濡 �Լ������)
    }

    public void BackBtn(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Back);
        this.gameObject.SetActive(false);
    }
}
