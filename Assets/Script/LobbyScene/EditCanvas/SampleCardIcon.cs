using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SampleCardIcon : MonoBehaviour
{
    public TextMeshProUGUI cardName, cardStat, cardDescription, cardCost, type, CancelBtn;
    public Image cardImage, lockedImage;
    public SampleCardIcon EnLargedCard;
    public DeckViewport deckView;
    public CardData data;
    private void Awake()
    {
        // Ȯ��ī�常 ���
        if (CancelBtn != null)
        {
            // Ȯ����� �Ǵ� �ֺ� ��� Ŭ���� Ȯ�� ��� ���
            GAME.Manager.UM.BindTMPInteraction(CancelBtn, Color.grey, Color.red,
                (tmp) => { this.gameObject.SetActive(false); } );
            GAME.Manager.UM.BindEvent(this.gameObject ,
                (GameObject tmp) => { this.gameObject.SetActive(false); } , Define.Mouse.ClickL, Define.Sound.None);
            // Ȯ�� ī��� �ʱ⿡ ���α�
            this.gameObject.SetActive(false);
        }

        // �⺻���� ����ī����� ����ϴ� Ŭ���̺�Ʈ
        else
        {
            // ��Ŭ�� : ���� ī�����
            GAME.Manager.UM.BindEvent(this.gameObject, CardLeftClicked, Define.Mouse.ClickL, Define.Sound.Click);
            // ��Ŭ�� : ī�� Ȯ�뺸��
            GAME.Manager.UM.BindEvent(this.gameObject, CardRightClicked, Define.Mouse.ClickR, Define.Sound.Back);

            // Ŀ�� ������ �ȳ��˾� ȣ��
            // Ŀ�� ������ ��� �⺻ �ȳ����� �ȳ�
            GAME.Manager.UM.BindUIPopup(this.gameObject, 0.2f, new Vector3(-300f,0,0) , Define.PopupScale.Small,
                "\r\n<color=red><size=25>��Ŭ����" +
                "<color=black>���� ����\r\n\r\n<color=red>��Ŭ���� <color=black>ī�� Ȯ��");
        }
    }
    
    // cardSlot.cs���� ����ī��,�߸�ī��� ���� ���ýø��� ī���� ���� �ʱ�ȭ
    public void CardInit(ref CardData input)
    {
        // ī�带 ���� ���� ���Ե� ������ ��͵����ؼ� ����� ���� ����
        lockedImage.gameObject.SetActive
            (deckView.isOn(ref input));

        data = input;
        cardName.text = data.cardName;
        cardDescription.text = data.cardDescription;
        cardCost.text = data.cost.ToString();
        
        Debug.Log($"{data.cardName} : {data.cardClass}{data.cardIdNum}");
        cardImage.sprite = Resources.Load<Sprite>($"Texture/CardImage/{data.cardClass}/{data.cardClass}{data.cardIdNum}");

        // ī�� ��͵� ǥ��
        switch (data.cardRarity)
        {
            case Define.cardRarity.rare:
                type.text = "<color=blue>���"; break;
            case Define.cardRarity.legend:
                type.text = "<color=red>����"; break;
            default: type.text = "<color=black>�Ϲ�"; break;
        }

        // ī�� Ÿ�� ǥ��
        switch (data.cardType)
        {
            case Define.cardType.minion:
                MinionCardData cardData = data as MinionCardData;
                cardStat.text = $"<color=yellow>ATT {cardData.att} <color=red>HP {cardData.hp} <color=black>����";
                break;
            case Define.cardType.spell:
                cardStat.text = "<color=black>�ֹ�";
                break;
            case Define.cardType.weapon:
                WeaponCardData wData = (WeaponCardData)data;
                cardStat.text = $"<color=yellow>ATT {wData.att} <color=red>dur {wData.durability} <color=black>����";
                break;
        }
    }

    // ����ī�� ��Ŭ����, ���� �ش� ī�� ����
    public void CardLeftClicked(GameObject go) 
    {
        // ���� ��� �̹��� Ȱ��ȭ ���¶��, �ѵ������� ���� ���ִ� ����
        // �Ǵ� ���� �������� ���� ���Ե� ī����� 20���̸� �ִ��ѵ��� ���� ���
        if (lockedImage.gameObject.activeSelf
            || deckView.currDeck.GetCount() == 20)
        { return; } // �� ������ ���� ���

        // ���� �������� ���� �����ִ� �� ����Ʈ�� ���� ī��ID ������
        deckView.AddCard(data, this);

        // php�� ���� ī����� �����ϱ�
        GAME.Manager.StartCoroutine(GAME.Manager.NM.ChangeDeckCard(deckView.currDeck.deckCode, data.cardIdNum, "false"));
    }

    // ��Ŭ�� : ī��Ȯ���� => Ȯ��� ī�� ������Ʈ Ȱ��ȭ �� ������ ����
    public void CardRightClicked(GameObject go)
    {
        // ���� ���������� �������� ī�嵥���� �״�� ����
        EnLargedCard.cardName.text = cardName.text;
        EnLargedCard.cardDescription.text = cardDescription.text;
        EnLargedCard.cardCost.text = cardCost.text;
        EnLargedCard.cardStat.text = cardStat.text;
        EnLargedCard.type.text = type.text;
        EnLargedCard.cardImage.sprite = cardImage.sprite;
        EnLargedCard.gameObject.SetActive(true);
    }
}
