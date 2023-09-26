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
        // 확대카드만 사용
        if (CancelBtn != null)
        {
            // 확대취소 또는 주변 배경 클릭시 확대 기능 취소
            GAME.Manager.UM.BindTMPInteraction(CancelBtn, Color.grey, Color.red,
                (tmp) => { this.gameObject.SetActive(false); } );
            GAME.Manager.UM.BindEvent(this.gameObject ,
                (GameObject tmp) => { this.gameObject.SetActive(false); } , Define.Mouse.ClickL, Define.Sound.None);
            // 확대 카드는 초기에 꺼두기
            this.gameObject.SetActive(false);
        }

        // 기본적인 샘플카드들이 사용하는 클릭이벤트
        else
        {
            // 좌클릭 : 덱에 카드삽입
            GAME.Manager.UM.BindEvent(this.gameObject, CardLeftClicked, Define.Mouse.ClickL, Define.Sound.Click);
            // 우클릭 : 카드 확대보기
            GAME.Manager.UM.BindEvent(this.gameObject, CardRightClicked, Define.Mouse.ClickR, Define.Sound.Back);

            // 커서 닿을시 안내팝업 호출
            // 커서 가져다 댈시 기본 안내문구 안내
            GAME.Manager.UM.BindUIPopup(this.gameObject, 0.2f, new Vector3(-300f,0,0) , Define.PopupScale.Small,
                "\r\n<color=red><size=25>좌클릭시" +
                "<color=black>덱에 삽입\r\n\r\n<color=red>우클릭시 <color=black>카드 확대");
        }
    }
    
    // cardSlot.cs에서 직업카드,중립카드등 진영 선택시마다 카드의 내용 초기화
    public void CardInit(ref CardData input)
    {
        // 카드를 현재 덱에 포함된 갯수와 희귀도통해서 잠글지 말지 설정
        lockedImage.gameObject.SetActive
            (deckView.isOn(ref input));

        data = input;
        cardName.text = data.cardName;
        cardDescription.text = data.cardDescription;
        cardCost.text = data.cost.ToString();
        
        Debug.Log($"{data.cardName} : {data.cardClass}{data.cardIdNum}");
        cardImage.sprite = Resources.Load<Sprite>($"Texture/CardImage/{data.cardClass}/{data.cardClass}{data.cardIdNum}");

        // 카드 희귀도 표시
        switch (data.cardRarity)
        {
            case Define.cardRarity.rare:
                type.text = "<color=blue>희귀"; break;
            case Define.cardRarity.legend:
                type.text = "<color=red>전설"; break;
            default: type.text = "<color=black>일반"; break;
        }

        // 카드 타입 표시
        switch (data.cardType)
        {
            case Define.cardType.minion:
                MinionCardData cardData = data as MinionCardData;
                cardStat.text = $"<color=yellow>ATT {cardData.att} <color=red>HP {cardData.hp} <color=black>몬스터";
                break;
            case Define.cardType.spell:
                cardStat.text = "<color=black>주문";
                break;
            case Define.cardType.weapon:
                WeaponCardData wData = (WeaponCardData)data;
                cardStat.text = $"<color=yellow>ATT {wData.att} <color=red>dur {wData.durability} <color=black>무기";
                break;
        }
    }

    // 샘플카드 좌클릭시, 덱에 해당 카드 삽입
    public void CardLeftClicked(GameObject go) 
    {
        // 현재 잠긴 이미지 활성화 상태라면, 한도문제로 덱에 못넣는 상태
        // 또는 현재 편집중인 덱에 포함된 카드수가 20장이면 최대한도로 강제 취소
        if (lockedImage.gameObject.activeSelf
            || deckView.currDeck.GetCount() == 20)
        { return; } // 덱 삽입은 강제 취소

        // 현재 편집중인 덱을 보여주는 덱 뷰포트에 현재 카드ID 보내기
        deckView.AddCard(data, this);

        // php의 보유 카드사항 변경하기
        GAME.Manager.StartCoroutine(GAME.Manager.NM.ChangeDeckCard(deckView.currDeck.deckCode, data.cardIdNum, "false"));
    }

    // 우클릭 : 카드확대기능 => 확대용 카드 오브젝트 활성화 및 데이터 대입
    public void CardRightClicked(GameObject go)
    {
        // 현재 덱아이콘이 참조중인 카드데이터 그대로 복붙
        EnLargedCard.cardName.text = cardName.text;
        EnLargedCard.cardDescription.text = cardDescription.text;
        EnLargedCard.cardCost.text = cardCost.text;
        EnLargedCard.cardStat.text = cardStat.text;
        EnLargedCard.type.text = type.text;
        EnLargedCard.cardImage.sprite = cardImage.sprite;
        EnLargedCard.gameObject.SetActive(true);
    }
}
