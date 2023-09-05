using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class DeckIcon : MonoBehaviour
{
    public TextMeshProUGUI deckName, deckCount, classType;
    public GameObject userDeck, emptyDeck;
    public Image classIcon;
    DeckData ownDeckData;
    PlayCanvas playCanvas;
    private void Awake()
    {
        // ���� ��ư ���콺 �̺�Ʈ ����
        playCanvas = GetComponentInParent<PlayCanvas>();
        GAME.Manager.UM.BindEvent(emptyDeck, MakeNewDeck, Define.Mouse.ClickL);
        GAME.Manager.UM.BindEvent(userDeck, ClickedOnUserDeck, Define.Mouse.ClickL);

        Vector3 anchor = GetComponent<RectTransform>().anchoredPosition;
        float height = (transform.GetSiblingIndex() > 3) ? -110f: 0f;
        
        if (anchor.x > 0)
        {
            GAME.Manager.UM.BindUIPopup(emptyDeck, 0.75f, new Vector3(183f,height,0),
                Define.PopupScale.Small, "<color=red>Ŭ����<color=black>\n\nĳ���� ����â���� �̵��մϴ�");
            GAME.Manager.UM.BindUIPopup(userDeck, 0.75f, new Vector3(183f, height, 0),
            Define.PopupScale.Small, "<color=red>Ŭ����<color=black>\n���ӽ��� �Ǵ� ���� �����մϴ�\n20�̸����� �����Ҽ� �����");
        }
        else if (anchor.x == 0)
        {
            GAME.Manager.UM.BindUIPopup(emptyDeck, 0.75f, new Vector3(0, height, 0),
            Define.PopupScale.Small, "<color=red>Ŭ����<color=black>\n\nĳ���� ����â���� �̵��մϴ�");
            GAME.Manager.UM.BindUIPopup(userDeck, 0.75f, new Vector3(0, height, 0),
            Define.PopupScale.Small, "<color=red>Ŭ����<color=black>\n���ӽ��� �Ǵ� ���� �����մϴ�\n20�̸����� �����Ҽ� �����");
        }
        else
        {
            GAME.Manager.UM.BindUIPopup(userDeck, 0.75f, new Vector3(-183f, height, 0),
            Define.PopupScale.Small, "<color=red>Ŭ����<color=black>\n���ӽ��� �Ǵ� ���� �����մϴ�\n20�̸����� �����Ҽ� �����");
            GAME.Manager.UM.BindUIPopup(emptyDeck, 0.75f, new Vector3(-183f, height, 0),
            Define.PopupScale.Small, "<color=red>Ŭ����<color=black>\n\nĳ���� ����â���� �̵��մϴ�");
        }
        
    }

    // PlayCanvas�� Ȱ��ȭ�ɶ�����, ������ ���������� ������ �� ������ ����
    public void Init(DeckData data)
    {
        // �Ʊ�� ���� ��������, �̸��� �������游 �����ϰ� �ѱ��
        if (ownDeckData == data)
        {
            ownDeckData = data;
            deckName.text = data.deckName;
            deckCount.text = $"{data.cards.Values.Sum()}/20";
        }
        else 
        {
            ownDeckData = data;
            deckName.text = data.deckName;
            deckCount.text = $"{data.cards.Values.Sum()}/20";
            classType.text = $"{data.ownerClass}";
            classIcon.sprite = GAME.Manager.RM.GetHeroImage(data.ownerClass);
            userDeck.gameObject.SetActive(true);
            emptyDeck.gameObject.SetActive(false);
        }
        
    }

    public void Clear() 
    {
        ownDeckData = null;
        emptyDeck.gameObject.SetActive(true);
        userDeck.gameObject.SetActive(false);
    }

    // ������ �ڽ��� ���� Ŭ����, ���ӽ������� ������������ �����ϴ� �˾�â ȣ��
    public void ClickedOnUserDeck(GameObject go)
    {
        // �ش� ������ ���ӽ����� ����, �������� �����ϴ� �˾�â ȣ��
        playCanvas.selectedDeckIcon.OpenSelectedDeckIcon(ownDeckData);
    }

    // �� ����� ��ư�� Ŭ���ߴٸ�
    public void MakeNewDeck(GameObject go) 
    {
        // ���ο� �� �����Ͻ�, editCanvas�� ĳ���� ����â���� ����
        EditCanvas ec = GAME.Manager.LM.edit.GetComponent<EditCanvas>();
        ec.characterStage.gameObject.SetActive(true);
        ec.cardStage.gameObject.SetActive(false);

        // ����ĵ������ ��ȯ ����
        GAME.Manager.LM.edit.GetComponent<EditCanvas>().StartMakingMode();
        GAME.Manager.StartCoroutine(GAME.Manager.LM.CanvasTransition(playCanvas, GAME.Manager.LM.edit));
    }
}
