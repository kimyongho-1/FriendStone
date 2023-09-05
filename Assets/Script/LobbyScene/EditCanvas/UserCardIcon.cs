using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
public class UserCardIcon : MonoBehaviour
{
    public TextMeshProUGUI cardName, cardCount;
    public Image deleteBtn, cardIcon;
    public RectTransform rt;
    public CardData data;
    DeckViewport deckView;
    SampleCardIcon EnLargedCard;
    private void Awake()
    {
        deckView = GetComponentInParent<DeckViewport>();
        GAME.Manager.UM.BindEvent(deleteBtn.gameObject, RemoveCard, Define.Mouse.ClickL);
        GAME.Manager.UM.BindEvent(this.gameObject, CallEnLargedCard, Define.Mouse.ClickR);
        EnLargedCard = deckView.EnLargedCard;
    }

    // X��ư�� ���� ������ ī�� ����
    public void RemoveCard(GameObject go)
    {
        deckView.currDeck.cards.TryGetValue(data, out int count);
        
        // ����ó��
        if (count == 0) { return; }

        count -= 1;
        // ������ ������ ����
        if (count == 0)
        {
            // �ڽ��� ������ ������ �����յ� ��ġ ������
            deckView.currDeck.cards.Remove(this.data);
            deckView.visualList.Remove(this);
            deckView.visualList = deckView.visualList.OrderBy(x => x.data.cost).ToList();
            for (int i = 0; i < deckView.visualList.Count; i++)
            {
                deckView.visualList[i].rt.anchoredPosition =
                    new Vector3(0, -5f + (-50f * i), 0);
            }
            deckView.cardCount.text = deckView.currDeck.GetCount().ToString() + "/20";

            // ������ ����
            GameObject.Destroy(this.gameObject);
        }
        // �ܼ� ������ ����
        else
        {
            // ���� ����
            deckView.currDeck.cards[data] = count;
            deckView.cardCount.text = deckView.currDeck.GetCount().ToString()+"/20";
            deckView.visualList.Find(x => x.data.cardIdNum == data.cardIdNum).
                cardCount.text = $"{count}/2";
        } 
        
        // ���� ���� �������� ī����ø���Ʈ�� ������ ������ ī�尡 �ְ�
          // ���� ��� ���¸� �ٷ� �������ֱ�
        SampleCardIcon sc = deckView.cardView.sampleList.Find(x => x.data.cardIdNum == data.cardIdNum);
        if (sc != null)
        { sc.lockedImage.gameObject.SetActive(false); }
        // ������Ʈ�� ������ ������Ʈ ���� ���̱�
        deckView.content.sizeDelta = new Vector2(0, deckView.currDeck.cards.Keys.Count() * 50f);

        // php������ ���� ���� ����
        GAME.Manager.StartCoroutine(GAME.Manager.NM.ChangeDeckCard(deckView.currDeck.deckCode, data.cardIdNum, "true"));
    }

    // ���� �����ϴ� ���� ī�� ������ ��Ŭ���� Ȯ��ī�� ����
    public void CallEnLargedCard(GameObject go)
    {
        // ���� ���������� �������� ī�嵥���� �״�� ����
        EnLargedCard.cardName.text = cardName.text;
        EnLargedCard.cardDescription.text = data.cardDescription;
        EnLargedCard.cardCost.text = data.cost.ToString();
        // ī�� Ÿ�� ǥ��
        switch (data.cardType)
        {
            case Define.cardType.minion:
                MinionCardData cardData = data as MinionCardData;
                EnLargedCard.cardStat.text = $"<color=yellow>ATT {cardData.att} <color=red>HP {cardData.hp} <color=black>����";
                break;
            case Define.cardType.spell:
                EnLargedCard.cardStat.text = "<color=black>�ֹ�";
                break;
            case Define.cardType.weapon:
                WeaponCardData wData = (WeaponCardData)data;
                EnLargedCard.cardStat.text = $"<color=yellow>ATT {wData.att} <color=red>dur {wData.durability} <color=black>����";
                break;
        }
        EnLargedCard.type.text = data.cardRarity.ToString();
        EnLargedCard.cardImage.sprite =cardIcon.sprite;
        EnLargedCard.gameObject.SetActive(true);
    }
}


