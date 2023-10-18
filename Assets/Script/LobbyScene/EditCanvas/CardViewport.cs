using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Linq;
public class CardViewport : MonoBehaviour
{
    public DeckViewport deckView;
    public TextMeshProUGUI LeftBtn, RightBtn, classBtn, neturalBtn, acceptBtn, deleteBtn;
    public List<SampleCardIcon> sampleList = new List<SampleCardIcon>();
    public List<CardData> classList = new List<CardData>();
    public List<CardData> neturalList = new List<CardData>();
    List<CardData> HjList = new List<CardData>();
    List<CardData> HzList = new List<CardData>();
    List<CardData> KhList = new List<CardData>();
    int page;
    AudioSource audioPlayer;
    private void Awake()
    {
        // 현재 모든 버튼 역할을 TMPUGUI글자텍스트로 진행중 + 클릭시 실행할 이벤트함수 연결
        GAME.Manager.UM.BindTMPInteraction(LeftBtn, Color.green, Color.gray, LeftBtnClicked );
        GAME.Manager.UM.BindTMPInteraction(RightBtn, Color.green, Color.gray, RightBtnClicked);
        GAME.Manager.UM.BindTMPInteraction(classBtn, Color.green, Color.red, ShowClassCards);
        GAME.Manager.UM.BindTMPInteraction(neturalBtn, Color.green, Color.red, ShowNeutralCards);

        GAME.Manager.UM.BindTMPInteraction(acceptBtn, Color.green, Color.red, SaveDeckBtn);
        GAME.Manager.UM.BindTMPInteraction(deleteBtn, Color.green, Color.red, DeleteDeckBtn);

        // 각종 안내팝업창 호출 이벤트 연결
        GAME.Manager.UM.BindUIPopup(
            classBtn.gameObject, 0.75f, new Vector3(-225f,60f,0), Define.PopupScale.Small,
            $"{((classBtn.gameObject.activeSelf == true) ? "현재 당신의 직업카드를\n보고 있습니다\n중립카드를 누르면\n중립카드로 바꿉니다": "현재 중립카드를\n보고 있습니다\n직업카드를 누르면\n직업카드로 바꿉니다")}");
        GAME.Manager.UM.BindUIPopup(
            neturalBtn.gameObject, 0.75f, new Vector3(-75f,60f,0), Define.PopupScale.Small,
            $"{((classBtn.gameObject.activeSelf == true) ? "현재 당신의 직업카드를\n보고 있습니다\n중립카드를 누르면\n중립카드로 바꿉니다" : "현재 중립카드를\n보고 있습니다\n직업카드를 누르면\n직업카드로 바꿉니다")}");
        GAME.Manager.UM.BindUIPopup(
            acceptBtn.gameObject, 0.25f, new Vector3(-322f, -123f,0),
            Define.PopupScale.Small, "편집을 중단하지만\n20장 미만의 덱은\n게임에 사용할수가 없습니다");
        GAME.Manager.UM.BindUIPopup(
            deleteBtn.gameObject, 0.25f, new Vector3(131f, -123f, 0),
            Define.PopupScale.Medium, "클릭시\n현재 덱을 <color=red>완전히 삭제합니다.");
        audioPlayer = GetComponent<AudioSource>();
    }
    // 사용할 데이터 파일들 준비
    public void ReadyData()
    {
        // 각진영별 카드 데이터들을 로딩
        neturalList.AddRange(LoadCards("Assets/Resources/Data/Neturaldata"));
        HjList.AddRange(LoadCards("Assets/Resources/Data/HJdata"));
        HzList.AddRange(LoadCards("Assets/Resources/Data/HZdata"));
        KhList.AddRange(LoadCards("Assets/Resources/Data/Khdata"));

        // 준비완료 신호 보내기
        GAME.Manager.waitQueue.Enqueue(GAME.Manager.LM.edit.gameObject);
    }
    
    // 제이슨파일 가져오기
    public List<CardData> LoadCards(string folder)
    {
        // json확장자만 가져오기
        string[] filePath = Directory.GetFiles(folder, "*.json");
        List<CardData> allCards = new List<CardData>();

        for (int i = 0; i < filePath.Length; i++)
        {
            string content = File.ReadAllText(filePath[i]);
            // 여기서 카드 타입을 확인
            CardData c = JsonConvert.DeserializeObject<CardData>
                (content, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });


            CardData card = null;
            // 확인된 카드타입으로, 실제 카드타입으로 클래스화
            switch (c.cardType)
            {
                case Define.cardType.minion:
                    card = JsonConvert.DeserializeObject<MinionCardData>
                (content, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    break;
                case Define.cardType.spell:
                    card = JsonConvert.DeserializeObject<SpellCardData>
                (content, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    break;
                case Define.cardType.weapon:
                    card = JsonConvert.DeserializeObject<WeaponCardData>
                (content, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    break;
                default: break;
            }

            Debug.Log(card);
            allCards.Add(card);
        }

        // 비용순으로 재정렬
        allCards = allCards.OrderBy(x => x.cost).ToList();

        return allCards;
    }
 
    // 처음 만드는 덱의 경우
    public void EnterEmptyDeck(Define.classType type)
    {
        // 시작시 초기화
        classBtn.raycastTarget = false;
        neturalBtn.raycastTarget = true;
        page = 0;
        LeftBtn.gameObject.SetActive(false);
        RightBtn.gameObject.SetActive(true);
        // 인자의 캐릭터 타입에 맞게 설정하도록
        // 카드뷰포트에서 직업데이타List 변경
        switch (type)
        {
            case Define.classType.HJ:
                classBtn.text = "HJ 카드";
                classList = HjList; break;
            case Define.classType.HZ:
                classBtn.text = "HZ 카드";
                classList = HzList; break;
            case Define.classType.KH:
                classBtn.text = "KH 카드";
                classList = KhList; break;
        }

        // 뷰포트에 보일 카드샘플리스트도 변경
        UpdateCardSheet();

    }

    #region 뷰포트 사용 함수
    // 나의 직업카드 버튼 클릭 호출
    public void ShowClassCards(TextMeshProUGUI go)
    {
        // 레이가 꺼진 상태면, 현재 해당 진영카드 임을 확인
        if (go.raycastTarget == false) { return; }
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Enter);
        // 현재 진영임을 알려주기위해 레이끄기
        go.raycastTarget = false;
        go.color = Color.red;
        neturalBtn.raycastTarget = true;
        neturalBtn.color = Color.black;

        // 직업카드 목록으로 바꿔주기
        UpdateCardSheet();
    }

    // 중립카드 버튼 클릭시 호출
    public void ShowNeutralCards(TextMeshProUGUI go)
    {
        // 레이가 꺼진 상태면, 현재 해당 진영카드 임을 확인
        if (go.raycastTarget == false) { return; }
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Enter);
        // 현재 진영임을 알려주기위해 레이끄기
        go.raycastTarget = false;
        classBtn.raycastTarget = true;
        classBtn.color = Color.black;

        // 중립카드 목록으로 바꿔주기
        UpdateCardSheet();
    }

    // 왼쪽 오른쪽 화살표 누를떄마다, 카드뷰포트 다시 설정
    public void LeftBtnClicked(TextMeshProUGUI go)
    {
        // 이미 최소 페이지면 취소
        if (page == 0)
        {
            return;
        }
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Flip);

        // 왼쪽을 눌렀다면 강제로 켜기
        RightBtn.gameObject.SetActive(true);
        // 아니면, 페이지 줄인상태로 다시 세팅
        page--;
        
        // 카드 리스트 재정렬
        UpdateCardSheet();
        // 페이지 0일시 꺼두기
        LeftBtn.gameObject.SetActive(page > 0);
        
    }
    public void RightBtnClicked(TextMeshProUGUI go)
    {
        // 현재 선택된 진영 데이타를 페이지수로 구하기 (인덱스의 8 나누기)
        int maxPage = (classBtn.raycastTarget == false) ? CalculateMax(neturalList) : CalculateMax(classList);
        if (page >= maxPage) { return; }
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Flip);
        page++;
        // 오른쪽을 눌렀다면, 강제로 왼쪽 켜기
        LeftBtn.gameObject.SetActive(true);
        if (page == maxPage)
        {
            RightBtn.gameObject.SetActive(false);
        }
        UpdateCardSheet();
    }

    // 덱제작페이지내에서 완료 버튼 클릭시 호출
    public void SaveDeckBtn(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Info);
        // 다시 이전 팝업창으로 캔버스화면 전환
        GAME.Manager.StartCoroutine(GAME.Manager.LM.CanvasTransition(GAME.Manager.LM.edit));
    }

    // 덱 자체를 삭제 버튼을 클릭시 호출
    public void DeleteDeckBtn(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Back);
        // php에서도 덱 삭제 진행
        GAME.Manager.StartCoroutine(GAME.Manager.NM.DeleteDeck(deckView.currDeck.deckName, deckView.currDeck.deckCode));
        // nm에서 관리하는 덱리스트에서도 제거
        GAME.Manager.RM.userDecks.Remove(deckView.currDeck);

        // 다시 이전 팝업창으로 캔버스화면 전환
        GAME.Manager.StartCoroutine(GAME.Manager.LM.CanvasTransition(GAME.Manager.LM.edit));
    }

    // 현재 선택된 진영데이터 List의 최대치 한계 구하기
    public int CalculateMax(List<CardData> list)
    {
        // 최대 페이지수 구하기
        return (int)Mathf.Ceil(list.Count / 8.0f) -1;
    }

    // 카드뷰포트에서 유저가 선택할수있는 카드 데이터들을 객체로 보이도록 만들기
    public void UpdateCardSheet()
    {
        // 현재 어떤 진영의 카드세트를 보던중인지 확인
        List<CardData> currentList = (classBtn.raycastTarget == false) ?
            classList : neturalList;

        for (int i = 0; i < sampleList.Count; i++)
        {
            // 인덱스 최대를 초과하면 애초에 보일 필요가 없기에 끄기
            sampleList[i].gameObject.SetActive(page * 8 + i < currentList.Count);

            // 켜져있다면, 보여야할 카드
            if (sampleList[i].gameObject.activeSelf)
            {
                CardData cd = currentList[page * 8 + i];
                sampleList[i].CardInit(ref cd);
            }
        }

    }
    #endregion
}
