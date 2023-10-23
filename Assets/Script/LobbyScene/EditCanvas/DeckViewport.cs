using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;
using WebSocketSharp;
using Unity.VisualScripting;

public class DeckViewport : MonoBehaviour
{
    public UserCardIcon Prefab; // ������ ī�����Ʈ���� ī�带 Ŭ���ø��� �װ� �ð������� ������ ������
    public RectTransform content; // �����յ��� ��ġ �θ� + ī����� ���� �����ȯ�뵵
    public CardViewport cardView;
    public TextMeshProUGUI cardCount ;
    public TMP_InputField deckNameInputField; // tmp ��ǲ�ʵ�
    public Image classIcon;
    public DeckData currDeck; // ���� �������� ������Ÿ ��������1
    public List<UserCardIcon> visualList = new List<UserCardIcon>(); // ���̴� ������ ����Ʈ
    public SampleCardIcon EnLargedCard;
    public AudioSource audioPlayer;
    private void Awake()
    {
        GAME.Manager.UM.BindUIPopup(
            deckNameInputField.gameObject, 
            0.3f, new Vector3(75f,170f,0),Define.PopupScale.Small, 
            "������ �̸��� Ŭ����\n" +
            "����� ���̸��� �ٲܼ��� �ֽ��ϴ�");
        audioPlayer = GetComponent<AudioSource>();
    }

    public void OnDisable()
    {
        // ��Ȱ��ȭ ���� : ���� ������带 ����� ��Ȳ
        // ���� �� ���� ���� + ������� ���� ī�������յ� ����
        currDeck = null;
        
        for (int i = 0; i < visualList.Count; i++)
        {
            GameObject.Destroy(visualList[i].gameObject);
        }
        visualList.Clear();
    }

    // ������ ���� �ڽ��� ���� �����ϴ°��, ó�� ���ý� ������ ������ ȭ������ �����
    public void EnterUserDeck(DeckData data)
    {
        currDeck = data;
        deckNameInputField.text = data.deckName.ToString(); // $"���ο� {data.ownerClass}��";
        cardCount.text = $"{data.cards.Values.Sum()}/20";
        classIcon.sprite = GAME.Manager.RM.GetHeroImage(data.ownerClass);

        // ���� ������ ���給 ������ �°�, �ٽ� �����յ� �ҷ���
        // �ð������� ��ġ��Ű��
        List<CardData> card = data.cards.Keys.ToList();
        List<int> count = data.cards.Values.ToList();

        for (int i = 0; i < card.Count; i++)
        {
            UserCardIcon deckCard = GameObject.Instantiate(Prefab, content.transform);
            deckCard.data = card[i];
            visualList.Add(deckCard);
            deckCard.cardName.text = card[i].cardName;
            deckCard.cardIcon.sprite = Resources.Load<Sprite>
                ($"Texture/CardImage/{card[i].cardClass}/{card[i].cardClass}{card[i].cardIdNum}");
            deckCard.cardCount.text = 
                $"{(count[i])}/{((card[i].cardRarity == Define.cardRarity.legend) ? 1 : 2)}";

            // ������ ��ġ ���ϱ�
            visualList[i].rt.anchoredPosition =
                    new Vector3(0, -5f + (-50f * i), 0);

            // ����ȭ�鿡��, ī�����Ʈ���� �ִ��ѵ� �����Ѱ��ִ��� Ȯ�� �� ��� �̹��� ����
            SampleCardIcon sci = cardView.sampleList.Find(x => x.data.cardIdNum == card[i].cardIdNum);
            if (sci != null
                && (card[i].cardRarity == Define.cardRarity.legend || count[i] == 2))
            { 
                sci.lockedImage.gameObject.SetActive(true);
            }
        }
        
        

        content.sizeDelta = new Vector2(0, currDeck.cards.Keys.Count() * 50f);
    }

    // ������ ���ο� ���� ���鋚 ����
    public void EnterEmptyDeck(Define.classType type)
    {
        // ���ο�̱⿡ ���ο� �������� ����
        currDeck = new DeckData();

        currDeck.deckName = $"���ο� {type}��";
        // ������ ���ڵ带 ����� ����, guid�� ������ȣ �� ���糯¥�� ���ڿ��� �ٲپ� �ʱ�ȭ
        currDeck.deckCode = Guid.NewGuid().ToString() + DateTime.UtcNow.Ticks.ToString();
        Debug.Log("deckCode Len : "+currDeck.deckCode.Length);
        currDeck.ownerClass = type;
        currDeck.deckName = deckNameInputField.text = $"���ο� {type}��";
        cardCount.text = "0/20";
        classIcon.sprite = GAME.Manager.RM.GetHeroImage(type);

        // ������ �⺻���� �����ϴ� NetWorkMgr���� ������Ÿ �ű� ����
        GAME.Manager.RM.userDecks.Add(currDeck);
        // NM���� php�� ���ο ���� ���� ������
        GAME.Manager.StartCoroutine(GAME.Manager.NM.MakeDeck(
            GAME.Manager.NM.playerInfo.ID, currDeck.deckName,
            currDeck.deckCode, currDeck.ownerClass ));

        // ����Ʈ ������ �ʱ�ȭ
        content.sizeDelta = new Vector2(0, currDeck.cards.Keys.Count() * 50f);
    }

    // ���� ���Ե� ī����� ���ؼ� ���ȭ�� �Ѿ����� �������� �����ϴ� �Լ�
    public bool isOn(ref CardData data)
    {
        // ���� ���������� �ʴ� ī���� ����� ���α�
        if (!currDeck.cards.ContainsKey(data)) { return false; }

        // ����ī����, ���� ���常 �־ ��ž��ϴ� ��Ȳ (�� ���ǹ� ����Ͽ��⿡ ���� �����ϴ� ī��)
        if (data.cardRarity == Define.cardRarity.legend)
        { return true; }
        
        // �׿� ī����� 2���� ���� ������ ��ױ�
        else
        {
            // 2��̸��̸� ��� ��Ȱ��ȭ
            if (currDeck.cards.TryGetValue(data, out int amount) && amount < 2)
            { return false; }
            else
            { return true; }
        }
    }

    // ī�����Ʈ����, ����ī�带 ��Ŭ����, ���� ���� �õ�
    public void AddCard(CardData data, SampleCardIcon icon)
    {
        // ���� ���� �̹� ���Ե� ī����
        if (currDeck.cards.ContainsKey(data))//
        {
            Debug.Log("�̹�!");
            // ���� ī�� ����
            currDeck.cards[data] += 1;
            // ���� ī�� ������ 
            if (currDeck.cards[data] == 2)
            {
                // �ѵ�2�� �̱⿡ ��ױ�
                icon.lockedImage.gameObject.SetActive(true);
            }
            // ���̴� �������� ���������� �����ؽ�Ʈ�� ����
            visualList.Find(x => x.data.cardIdNum == data.cardIdNum).cardCount.text =
                currDeck.cards[data].ToString() + "/ 2";

        }
        // ���� �ִ� ���ο� ī����
        else
        {
            Debug.Log("ó��!");
            currDeck.cards.Add(data, 1);
            // ���� ����ī��� �����̶� �ٷ� ��� ����
            if (data.cardRarity == Define.cardRarity.legend)
            {
                icon.lockedImage.gameObject.SetActive(true);
            }

            // ī�带 �߰��Ͽ��⿡, �����ջ����Ͽ� ���̴� �̹����� �����ϰ� �����
            UserCardIcon deckCard = GameObject.Instantiate(Prefab, content.transform);
            deckCard.data = data;
            visualList.Add(deckCard);
            deckCard.cardName.text = data.cardName;
            deckCard.cardIcon.sprite = Resources.Load<Sprite>
                ($"Texture/CardImage/{data.cardClass}/{data.cardClass}{data.cardIdNum}");
            deckCard.cardCount.text = $"1/{((data.cardRarity == Define.cardRarity.legend) ? 1 : 2)}";
                
            // �������� ������
            visualList = visualList.OrderBy(x => x.data.cost).ToList();

            // ��������յ� ��ġ ������
            for (int i = 0; i < visualList.Count; i++)
            {
                // ī����� �°� , �����յ� ��ġ �����Ͽ��ֱ�
                visualList[i].rt.anchoredPosition =
                    new Vector3(0, -5f + (-50f * i), 0);
            }
        }
        
        // ���� �������� ���� �ѵ��� �����ֱ�
        cardCount.text =$"{currDeck.GetCount()}/20";
        content.sizeDelta = new Vector2(0, currDeck.cards.Keys.Count() * 50f);

    }

    // deckNameText (TMP_inputField)�� ��ȭ�� ���拚���� ȣ�� (�ν����Ϳ� ������ �̺�Ʈ�Լ�)
    public void ChangeDeckName(TMP_InputField input)
    {
        // ����, ��ĭ�� ��� ����
        input.text.Trim();

        // �� �������̸� ���� ���
        if (string.IsNullOrEmpty(input.text))
        { return; }

        // �ƴҽ� �� �̸� ����
        else
        { currDeck.deckName = input.text; }

        // PHP������ �ش� �ٲ� ���̸����� ���� �䱸�ϱ�
        GAME.Manager.StartCoroutine(GAME.Manager.NM.ChangeDeckName(currDeck.deckName, currDeck.deckCode));
    }
}
