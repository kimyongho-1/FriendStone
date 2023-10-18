using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelect :LobbyPopup
{
    public Image HJ, HZ, KH; // �� ĳ���� ���þ�����
    public TextMeshProUGUI backBtn;
    EditCanvas editCanvas;
    AudioSource audioPlayer;
    private void Awake()
    {
        // �� ��ư��, �̹��� Ŭ���� ȣ���� �̺�Ʈ�Լ� ����
        GAME.Manager.UM.BindTMPInteraction(backBtn, Color.yellow, Color.red, BackBtn);
        GAME.Manager.UM.BindEvent(HJ.gameObject, StartMakeDeck, Define.Mouse.ClickL);
        GAME.Manager.UM.BindEvent(HZ.gameObject, StartMakeDeck, Define.Mouse.ClickL);
        GAME.Manager.UM.BindEvent(KH.gameObject, StartMakeDeck, Define.Mouse.ClickL);

        editCanvas = GetComponentInParent<EditCanvas>();
        audioPlayer = editCanvas.audioPlayer;
    }
    // �ڷΰ��� ��ư Ŭ����, ��ȯ����
    public void BackBtn(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Back);
        StartCoroutine(GAME.Manager.LM.CanvasTransition(GAME.Manager.LM.edit)) ;
    }

    // ĳ���� �������� ���ý�, �ش� ������ ȯ�������� ������ �������� �̵�
    public void StartMakeDeck(GameObject go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.HotSelect);
        // �ε��� ������ � ������ �����ߴ��� Ȯ���Ͽ� �� ����â���� �̵�
        int idx = go.transform.GetSiblingIndex();
        // �� ���� �غ����
        editCanvas.cardStage.MakeNewDeck((Define.classType)idx);
        // ���� ������ ������ ȯ���� �� ���� �̵� �ڷ�ƾ
        GAME.Manager.StartCoroutine(editCanvas.Transition(this, editCanvas.cardStage, idx));
    }

}
