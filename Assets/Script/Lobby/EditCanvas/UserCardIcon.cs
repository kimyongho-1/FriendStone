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

    // X버튼을 눌러 덱에서 카드 삭제
    public void RemoveCard(GameObject go)
    {
        deckView.currDeck.cards.TryGetValue(data, out int count);
        
        // 예외처리
        if (count == 0) { return; }

        count -= 1;
        // 덱에서 완전히 삭제
        if (count == 0)
        {
            // 자신을 제외한 나머지 프리팹들 위치 재정렬
            deckView.currDeck.cards.Remove(this.data);
            deckView.visualList.Remove(this);
            deckView.visualList = deckView.visualList.OrderBy(x => x.data.cost).ToList();
            for (int i = 0; i < deckView.visualList.Count; i++)
            {
                deckView.visualList[i].rt.anchoredPosition =
                    new Vector3(0, -5f + (-50f * i), 0);
            }
            deckView.cardCount.text = deckView.currDeck.GetCount().ToString() + "/20";

            // 마지막 삭제
            GameObject.Destroy(this.gameObject);
        }
        // 단순 갯수만 감소
        else
        {
            // 갯수 수정
            deckView.currDeck.cards[data] = count;
            deckView.cardCount.text = deckView.currDeck.GetCount().ToString()+"/20";
            deckView.visualList.Find(x => x.data.cardIdNum == data.cardIdNum).
                cardCount.text = $"{count}/2";
        } 
        
        // 만약 현재 편집중인 카드샘플리스트에 덱에서 삭제한 카드가 있고
          // 현재 잠김 상태면 바로 해제해주기
        SampleCardIcon sc = deckView.cardView.sampleList.Find(x => x.data.cardIdNum == data.cardIdNum);
        if (sc != null)
        { sc.lockedImage.gameObject.SetActive(false); }
        // 덱뷰포트의 컨텐츠 오브젝트 길이 줄이기
        deckView.content.sizeDelta = new Vector2(0, deckView.currDeck.cards.Keys.Count() * 50f);

        // php에서도 삭제 감소 진행
        GAME.Manager.StartCoroutine(GAME.Manager.NM.ChangeDeckCard(deckView.currDeck.deckCode, data.cardIdNum, "true"));
    }

    // 덱에 존재하는 유저 카드 아이콘 우클릭시 확대카드 실행
    public void CallEnLargedCard(GameObject go)
    {
        // 현재 덱아이콘이 참조중인 카드데이터 그대로 복붙
        EnLargedCard.cardName.text = cardName.text;
        EnLargedCard.cardDescription.text = data.cardDescription;
        EnLargedCard.cardCost.text = data.cost.ToString();
        // 카드 타입 표시
        switch (data.cardType)
        {
            case Define.cardType.minion:
                MinionCardData cardData = data as MinionCardData;
                EnLargedCard.cardStat.text = $"<color=yellow>ATT {cardData.att} <color=red>HP {cardData.hp} <color=black>몬스터";
                break;
            case Define.cardType.spell:
                EnLargedCard.cardStat.text = "<color=black>주문";
                break;
            case Define.cardType.weapon:
                WeaponCardData wData = (WeaponCardData)data;
                EnLargedCard.cardStat.text = $"<color=yellow>ATT {wData.att} <color=red>dur {wData.durability} <color=black>무기";
                break;
        }
        EnLargedCard.type.text = data.cardRarity.ToString();
        EnLargedCard.cardImage.sprite =cardIcon.sprite;
        EnLargedCard.gameObject.SetActive(true);
    }
}


