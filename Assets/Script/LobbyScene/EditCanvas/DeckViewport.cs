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
    public UserCardIcon Prefab; // 유저가 카드뷰포트에서 카드를 클릭시마다 그걸 시각적으로 보여줄 프리팹
    public RectTransform content; // 프리팹들의 위치 부모값 + 카드수에 따라 사이즈변환용도
    public CardViewport cardView;
    public TextMeshProUGUI cardCount ;
    public TMP_InputField deckNameInputField; // tmp 인풋필드
    public Image classIcon;
    public DeckData currDeck; // 현재 편집중인 덱데이타 참조변수1
    public List<UserCardIcon> visualList = new List<UserCardIcon>(); // 보이는 프리팹 리스트
    public SampleCardIcon EnLargedCard;
    public AudioSource audioPlayer;
    private void Awake()
    {
        GAME.Manager.UM.BindUIPopup(
            deckNameInputField.gameObject, 
            0.3f, new Vector3(75f,170f,0),Define.PopupScale.Small, 
            "오른쪽 이름을 클릭시\n" +
            "당신의 덱이름을 바꿀수가 있습니다");
        audioPlayer = GetComponent<AudioSource>();
    }

    public void OnDisable()
    {
        // 비활성화 마다 : 현재 편집모드를 벗어나는 상황
        // 참조 덱 연결 끊기 + 만들어진 덱의 카드프리팹들 삭제
        currDeck = null;
        
        for (int i = 0; i < visualList.Count; i++)
        {
            GameObject.Destroy(visualList[i].gameObject);
        }
        visualList.Clear();
    }

    // 유저가 기존 자신의 덱을 수정하는경우, 처음 들어올시 기존과 동일한 화면으로 만들기
    public void EnterUserDeck(DeckData data)
    {
        currDeck = data;
        deckNameInputField.text = data.deckName.ToString(); // $"새로운 {data.ownerClass}덱";
        cardCount.text = $"{data.cards.Values.Sum()}/20";
        classIcon.sprite = GAME.Manager.RM.GetHeroImage(data.ownerClass);

        // 현재 기존에 만든덱 설정에 맞게, 다시 프리팹들 불러와
        // 시각적으로 일치시키기
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

            // 프리팹 위치 정하기
            visualList[i].rt.anchoredPosition =
                    new Vector3(0, -5f + (-50f * i), 0);

            // 시작화면에서, 카드뷰포트에서 최대한도 도달한것있는지 확인 및 잠금 이미지 설정
            SampleCardIcon sci = cardView.sampleList.Find(x => x.data.cardIdNum == card[i].cardIdNum);
            if (sci != null
                && (card[i].cardRarity == Define.cardRarity.legend || count[i] == 2))
            { 
                sci.lockedImage.gameObject.SetActive(true);
            }
        }
        
        

        content.sizeDelta = new Vector2(0, currDeck.cards.Keys.Count() * 50f);
    }

    // 유저가 새로운 덱을 만들떄 시작
    public void EnterEmptyDeck(Define.classType type)
    {
        // 새로운덱이기에 새로운 덱데이터 생성
        currDeck = new DeckData();

        currDeck.deckName = $"새로운 {type}덱";
        // 고유한 덱코드를 만들기 위해, guid로 고유번호 및 만든날짜를 문자열로 바꾸어 초기화
        currDeck.deckCode = Guid.NewGuid().ToString() + DateTime.UtcNow.Ticks.ToString();
        Debug.Log("deckCode Len : "+currDeck.deckCode.Length);
        currDeck.ownerClass = type;
        currDeck.deckName = deckNameInputField.text = $"새로운 {type}덱";
        cardCount.text = "0/20";
        classIcon.sprite = GAME.Manager.RM.GetHeroImage(type);

        // 유저의 기본정보 관리하는 NetWorkMgr에서 덱데이타 신규 삽입
        GAME.Manager.RM.userDecks.Add(currDeck);
        // NM으로 php로 새로운덱 생성 정보 보내기
        GAME.Manager.StartCoroutine(GAME.Manager.NM.MakeDeck(
            GAME.Manager.NM.playerInfo.ID, currDeck.deckName,
            currDeck.deckCode, currDeck.ownerClass ));

        // 컨텐트 사이즈 초기화
        content.sizeDelta = new Vector2(0, currDeck.cards.Keys.Count() * 50f);
    }

    // 덱에 포함된 카드수를 통해서 잠금화면 켜야할지 꺼야할지 결정하는 함수
    public bool isOn(ref CardData data)
    {
        // 덱에 존재하지도 않는 카드라면 잠금은 꺼두기
        if (!currDeck.cards.ContainsKey(data)) { return false; }

        // 전설카드라면, 덱에 한장만 있어도 잠궈야하는 상황 (위 조건문 통과하였기에 덱에 존재하는 카드)
        if (data.cardRarity == Define.cardRarity.legend)
        { return true; }
        
        // 그외 카드들은 2장이 덱에 있으면 잠그기
        else
        {
            // 2장미만이면 잠금 비활성화
            if (currDeck.cards.TryGetValue(data, out int amount) && amount < 2)
            { return false; }
            else
            { return true; }
        }
    }

    // 카드뷰포트에서, 샘플카드를 좌클릭시, 덱에 삽입 시도
    public void AddCard(CardData data, SampleCardIcon icon)
    {
        // 현재 덱에 이미 포함된 카드라면
        if (currDeck.cards.ContainsKey(data))//
        {
            Debug.Log("이미!");
            // 덱에 카드 삽입
            currDeck.cards[data] += 1;
            // 덱에 카드 삽입후 
            if (currDeck.cards[data] == 2)
            {
                // 한도2장 이기에 잠그기
                icon.lockedImage.gameObject.SetActive(true);
            }
            // 보이는 프리팹의 영역에서의 갯수텍스트도 설정
            visualList.Find(x => x.data.cardIdNum == data.cardIdNum).cardCount.text =
                currDeck.cards[data].ToString() + "/ 2";

        }
        // 덱에 넣는 새로운 카드라면
        else
        {
            Debug.Log("처음!");
            currDeck.cards.Add(data, 1);
            // 만약 전설카드면 한장이라도 바로 잠금 설정
            if (data.cardRarity == Define.cardRarity.legend)
            {
                icon.lockedImage.gameObject.SetActive(true);
            }

            // 카드를 추가하였기에, 프리팹생성하여 보이는 이미지도 동일하게 만들기
            UserCardIcon deckCard = GameObject.Instantiate(Prefab, content.transform);
            deckCard.data = data;
            visualList.Add(deckCard);
            deckCard.cardName.text = data.cardName;
            deckCard.cardIcon.sprite = Resources.Load<Sprite>
                ($"Texture/CardImage/{data.cardClass}/{data.cardClass}{data.cardIdNum}");
            deckCard.cardCount.text = $"1/{((data.cardRarity == Define.cardRarity.legend) ? 1 : 2)}";
                
            // 비용순으로 재정렬
            visualList = visualList.OrderBy(x => x.data.cost).ToList();

            // 모든프리팹들 위치 재정렬
            for (int i = 0; i < visualList.Count; i++)
            {
                // 카드수에 맞게 , 프리팹들 위치 지정하여주기
                visualList[i].rt.anchoredPosition =
                    new Vector3(0, -5f + (-50f * i), 0);
            }
        }
        
        // 현재 편집중인 덱의 한도수 보여주기
        cardCount.text =$"{currDeck.GetCount()}/20";
        content.sizeDelta = new Vector2(0, currDeck.cards.Keys.Count() * 50f);

    }

    // deckNameText (TMP_inputField)의 변화가 생길떄마다 호출 (인스펙터에 연결한 이벤트함수)
    public void ChangeDeckName(TMP_InputField input)
    {
        // 띄어쓰기, 빈칸등 모두 삭제
        input.text.Trim();

        // 빈 공간뿐이면 강제 취소
        if (string.IsNullOrEmpty(input.text))
        { return; }

        // 아닐시 덱 이름 수정
        else
        { currDeck.deckName = input.text; }

        // PHP에서도 해당 바뀐 덱이름으로 수정 요구하기
        GAME.Manager.StartCoroutine(GAME.Manager.NM.ChangeDeckName(currDeck.deckName, currDeck.deckCode));
    }
}
