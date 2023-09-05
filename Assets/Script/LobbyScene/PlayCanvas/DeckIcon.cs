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
        // 각종 버튼 마우스 이벤트 연결
        playCanvas = GetComponentInParent<PlayCanvas>();
        GAME.Manager.UM.BindEvent(emptyDeck, MakeNewDeck, Define.Mouse.ClickL);
        GAME.Manager.UM.BindEvent(userDeck, ClickedOnUserDeck, Define.Mouse.ClickL);

        Vector3 anchor = GetComponent<RectTransform>().anchoredPosition;
        float height = (transform.GetSiblingIndex() > 3) ? -110f: 0f;
        
        if (anchor.x > 0)
        {
            GAME.Manager.UM.BindUIPopup(emptyDeck, 0.75f, new Vector3(183f,height,0),
                Define.PopupScale.Small, "<color=red>클릭시<color=black>\n\n캐릭터 선택창으로 이동합니다");
            GAME.Manager.UM.BindUIPopup(userDeck, 0.75f, new Vector3(183f, height, 0),
            Define.PopupScale.Small, "<color=red>클릭시<color=black>\n게임시작 또는 덱을 편집합니다\n20미만으론 게임할수 없어요");
        }
        else if (anchor.x == 0)
        {
            GAME.Manager.UM.BindUIPopup(emptyDeck, 0.75f, new Vector3(0, height, 0),
            Define.PopupScale.Small, "<color=red>클릭시<color=black>\n\n캐릭터 선택창으로 이동합니다");
            GAME.Manager.UM.BindUIPopup(userDeck, 0.75f, new Vector3(0, height, 0),
            Define.PopupScale.Small, "<color=red>클릭시<color=black>\n게임시작 또는 덱을 편집합니다\n20미만으론 게임할수 없어요");
        }
        else
        {
            GAME.Manager.UM.BindUIPopup(userDeck, 0.75f, new Vector3(-183f, height, 0),
            Define.PopupScale.Small, "<color=red>클릭시<color=black>\n게임시작 또는 덱을 편집합니다\n20미만으론 게임할수 없어요");
            GAME.Manager.UM.BindUIPopup(emptyDeck, 0.75f, new Vector3(-183f, height, 0),
            Define.PopupScale.Small, "<color=red>클릭시<color=black>\n\n캐릭터 선택창으로 이동합니다");
        }
        
    }

    // PlayCanvas가 활성화될때마다, 각각의 덱아이콘이 참조할 덱 정보를 공유
    public void Init(DeckData data)
    {
        // 아까와 같은 덱참조시, 이름과 갯수변경만 주의하고 넘기기
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

    // 유저가 자신의 덱을 클릭시, 게임시작할지 덱을편집할지 선택하는 팝업창 호출
    public void ClickedOnUserDeck(GameObject go)
    {
        // 해당 덱으로 게임시작을 할지, 편집할지 선택하는 팝업창 호출
        playCanvas.selectedDeckIcon.OpenSelectedDeckIcon(ownDeckData);
    }

    // 덱 만들기 버튼을 클릭했다면
    public void MakeNewDeck(GameObject go) 
    {
        // 새로운 덱 제작일시, editCanvas의 캐릭터 선택창부터 시작
        EditCanvas ec = GAME.Manager.LM.edit.GetComponent<EditCanvas>();
        ec.characterStage.gameObject.SetActive(true);
        ec.cardStage.gameObject.SetActive(false);

        // 편집캔버스로 전환 시작
        GAME.Manager.LM.edit.GetComponent<EditCanvas>().StartMakingMode();
        GAME.Manager.StartCoroutine(GAME.Manager.LM.CanvasTransition(playCanvas, GAME.Manager.LM.edit));
    }
}
