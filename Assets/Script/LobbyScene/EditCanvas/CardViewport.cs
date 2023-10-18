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
        // ���� ��� ��ư ������ TMPUGUI�����ؽ�Ʈ�� ������ + Ŭ���� ������ �̺�Ʈ�Լ� ����
        GAME.Manager.UM.BindTMPInteraction(LeftBtn, Color.green, Color.gray, LeftBtnClicked );
        GAME.Manager.UM.BindTMPInteraction(RightBtn, Color.green, Color.gray, RightBtnClicked);
        GAME.Manager.UM.BindTMPInteraction(classBtn, Color.green, Color.red, ShowClassCards);
        GAME.Manager.UM.BindTMPInteraction(neturalBtn, Color.green, Color.red, ShowNeutralCards);

        GAME.Manager.UM.BindTMPInteraction(acceptBtn, Color.green, Color.red, SaveDeckBtn);
        GAME.Manager.UM.BindTMPInteraction(deleteBtn, Color.green, Color.red, DeleteDeckBtn);

        // ���� �ȳ��˾�â ȣ�� �̺�Ʈ ����
        GAME.Manager.UM.BindUIPopup(
            classBtn.gameObject, 0.75f, new Vector3(-225f,60f,0), Define.PopupScale.Small,
            $"{((classBtn.gameObject.activeSelf == true) ? "���� ����� ����ī�带\n���� �ֽ��ϴ�\n�߸�ī�带 ������\n�߸�ī��� �ٲߴϴ�": "���� �߸�ī�带\n���� �ֽ��ϴ�\n����ī�带 ������\n����ī��� �ٲߴϴ�")}");
        GAME.Manager.UM.BindUIPopup(
            neturalBtn.gameObject, 0.75f, new Vector3(-75f,60f,0), Define.PopupScale.Small,
            $"{((classBtn.gameObject.activeSelf == true) ? "���� ����� ����ī�带\n���� �ֽ��ϴ�\n�߸�ī�带 ������\n�߸�ī��� �ٲߴϴ�" : "���� �߸�ī�带\n���� �ֽ��ϴ�\n����ī�带 ������\n����ī��� �ٲߴϴ�")}");
        GAME.Manager.UM.BindUIPopup(
            acceptBtn.gameObject, 0.25f, new Vector3(-322f, -123f,0),
            Define.PopupScale.Small, "������ �ߴ�������\n20�� �̸��� ����\n���ӿ� ����Ҽ��� �����ϴ�");
        GAME.Manager.UM.BindUIPopup(
            deleteBtn.gameObject, 0.25f, new Vector3(131f, -123f, 0),
            Define.PopupScale.Medium, "Ŭ����\n���� ���� <color=red>������ �����մϴ�.");
        audioPlayer = GetComponent<AudioSource>();
    }
    // ����� ������ ���ϵ� �غ�
    public void ReadyData()
    {
        // �������� ī�� �����͵��� �ε�
        neturalList.AddRange(LoadCards("Assets/Resources/Data/Neturaldata"));
        HjList.AddRange(LoadCards("Assets/Resources/Data/HJdata"));
        HzList.AddRange(LoadCards("Assets/Resources/Data/HZdata"));
        KhList.AddRange(LoadCards("Assets/Resources/Data/Khdata"));

        // �غ�Ϸ� ��ȣ ������
        GAME.Manager.waitQueue.Enqueue(GAME.Manager.LM.edit.gameObject);
    }
    
    // ���̽����� ��������
    public List<CardData> LoadCards(string folder)
    {
        // jsonȮ���ڸ� ��������
        string[] filePath = Directory.GetFiles(folder, "*.json");
        List<CardData> allCards = new List<CardData>();

        for (int i = 0; i < filePath.Length; i++)
        {
            string content = File.ReadAllText(filePath[i]);
            // ���⼭ ī�� Ÿ���� Ȯ��
            CardData c = JsonConvert.DeserializeObject<CardData>
                (content, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });


            CardData card = null;
            // Ȯ�ε� ī��Ÿ������, ���� ī��Ÿ������ Ŭ����ȭ
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

        // �������� ������
        allCards = allCards.OrderBy(x => x.cost).ToList();

        return allCards;
    }
 
    // ó�� ����� ���� ���
    public void EnterEmptyDeck(Define.classType type)
    {
        // ���۽� �ʱ�ȭ
        classBtn.raycastTarget = false;
        neturalBtn.raycastTarget = true;
        page = 0;
        LeftBtn.gameObject.SetActive(false);
        RightBtn.gameObject.SetActive(true);
        // ������ ĳ���� Ÿ�Կ� �°� �����ϵ���
        // ī�����Ʈ���� ��������ŸList ����
        switch (type)
        {
            case Define.classType.HJ:
                classBtn.text = "HJ ī��";
                classList = HjList; break;
            case Define.classType.HZ:
                classBtn.text = "HZ ī��";
                classList = HzList; break;
            case Define.classType.KH:
                classBtn.text = "KH ī��";
                classList = KhList; break;
        }

        // ����Ʈ�� ���� ī����ø���Ʈ�� ����
        UpdateCardSheet();

    }

    #region ����Ʈ ��� �Լ�
    // ���� ����ī�� ��ư Ŭ�� ȣ��
    public void ShowClassCards(TextMeshProUGUI go)
    {
        // ���̰� ���� ���¸�, ���� �ش� ����ī�� ���� Ȯ��
        if (go.raycastTarget == false) { return; }
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Enter);
        // ���� �������� �˷��ֱ����� ���̲���
        go.raycastTarget = false;
        go.color = Color.red;
        neturalBtn.raycastTarget = true;
        neturalBtn.color = Color.black;

        // ����ī�� ������� �ٲ��ֱ�
        UpdateCardSheet();
    }

    // �߸�ī�� ��ư Ŭ���� ȣ��
    public void ShowNeutralCards(TextMeshProUGUI go)
    {
        // ���̰� ���� ���¸�, ���� �ش� ����ī�� ���� Ȯ��
        if (go.raycastTarget == false) { return; }
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Enter);
        // ���� �������� �˷��ֱ����� ���̲���
        go.raycastTarget = false;
        classBtn.raycastTarget = true;
        classBtn.color = Color.black;

        // �߸�ī�� ������� �ٲ��ֱ�
        UpdateCardSheet();
    }

    // ���� ������ ȭ��ǥ ����������, ī�����Ʈ �ٽ� ����
    public void LeftBtnClicked(TextMeshProUGUI go)
    {
        // �̹� �ּ� �������� ���
        if (page == 0)
        {
            return;
        }
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Flip);

        // ������ �����ٸ� ������ �ѱ�
        RightBtn.gameObject.SetActive(true);
        // �ƴϸ�, ������ ���λ��·� �ٽ� ����
        page--;
        
        // ī�� ����Ʈ ������
        UpdateCardSheet();
        // ������ 0�Ͻ� ���α�
        LeftBtn.gameObject.SetActive(page > 0);
        
    }
    public void RightBtnClicked(TextMeshProUGUI go)
    {
        // ���� ���õ� ���� ����Ÿ�� ���������� ���ϱ� (�ε����� 8 ������)
        int maxPage = (classBtn.raycastTarget == false) ? CalculateMax(neturalList) : CalculateMax(classList);
        if (page >= maxPage) { return; }
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Flip);
        page++;
        // �������� �����ٸ�, ������ ���� �ѱ�
        LeftBtn.gameObject.SetActive(true);
        if (page == maxPage)
        {
            RightBtn.gameObject.SetActive(false);
        }
        UpdateCardSheet();
    }

    // ������������������ �Ϸ� ��ư Ŭ���� ȣ��
    public void SaveDeckBtn(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Info);
        // �ٽ� ���� �˾�â���� ĵ����ȭ�� ��ȯ
        GAME.Manager.StartCoroutine(GAME.Manager.LM.CanvasTransition(GAME.Manager.LM.edit));
    }

    // �� ��ü�� ���� ��ư�� Ŭ���� ȣ��
    public void DeleteDeckBtn(TextMeshProUGUI go)
    {
        GAME.Manager.LM.Play(ref audioPlayer, Define.OtherSound.Back);
        // php������ �� ���� ����
        GAME.Manager.StartCoroutine(GAME.Manager.NM.DeleteDeck(deckView.currDeck.deckName, deckView.currDeck.deckCode));
        // nm���� �����ϴ� ������Ʈ������ ����
        GAME.Manager.RM.userDecks.Remove(deckView.currDeck);

        // �ٽ� ���� �˾�â���� ĵ����ȭ�� ��ȯ
        GAME.Manager.StartCoroutine(GAME.Manager.LM.CanvasTransition(GAME.Manager.LM.edit));
    }

    // ���� ���õ� ���������� List�� �ִ�ġ �Ѱ� ���ϱ�
    public int CalculateMax(List<CardData> list)
    {
        // �ִ� �������� ���ϱ�
        return (int)Mathf.Ceil(list.Count / 8.0f) -1;
    }

    // ī�����Ʈ���� ������ �����Ҽ��ִ� ī�� �����͵��� ��ü�� ���̵��� �����
    public void UpdateCardSheet()
    {
        // ���� � ������ ī�弼Ʈ�� ���������� Ȯ��
        List<CardData> currentList = (classBtn.raycastTarget == false) ?
            classList : neturalList;

        for (int i = 0; i < sampleList.Count; i++)
        {
            // �ε��� �ִ븦 �ʰ��ϸ� ���ʿ� ���� �ʿ䰡 ���⿡ ����
            sampleList[i].gameObject.SetActive(page * 8 + i < currentList.Count);

            // �����ִٸ�, �������� ī��
            if (sampleList[i].gameObject.activeSelf)
            {
                CardData cd = currentList[page * 8 + i];
                sampleList[i].CardInit(ref cd);
            }
        }

    }
    #endregion
}
