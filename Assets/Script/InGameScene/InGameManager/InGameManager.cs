using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class CustomList : List<CardField>
{
    public new void Insert(int idx ,CardField ele)
    {
        // 기본 Insert 함수 실행후
        base.Insert(idx, ele);
        Debug.Log($"AllBody List에 {ele.data.cardName},[{ele.PunId}] 가 추가 됨");

        // 총 관리 목적의 LIST에도 추가
        GAME.IGM.allIBody.Add(ele);
    }

    public new void Remove(CardField ele) 
    {
        base.Remove(ele);
        GAME.IGM.allIBody.Remove(ele);
    }
}

public class InGameManager : MonoBehaviour
{
    // 게임내 체력 상호작용을 이루는 영웅들, 미니언들을 여기에서 쉽게 찾기위해 생성
    public List<IBody> allIBody = new List<IBody>();

    public int GameTurn = 0;
    public GameObject cardPopup;
    public TextMeshPro cardName, Description, Stat, Type, cost;
    public SpriteRenderer cardImage;

    #region 참조
    public FindEvtHolder FindEvt { get; set; }
    public HeroManager Hero { get; set; }
    public HandManager Hand { get; set; }
    public SpawnManager Spawn { get; set; }
    public TargetingCamera TC { get; set; }
    public BattleManager Battle { get; set; }
    public TurnEndBtn Turn { get; set; }
    public PacketManager Packet { get; set; }
    private void Awake()
    {
        GAME.IGM = this;
    }
    #endregion


    // BattleManager의 액션큐 빠르게 접근용도
    public void AddAction(IEnumerator co) { Battle.ActionQueue.Enqueue(co); }

    // 영웅 능력 팝업창 호출
    public void ShowHeroSkill(Vector3 pos, SkillData skill)
    {
        cardPopup.transform.position = pos;
        cardName.text = skill.Name;
        Description.text = skill.Desc;
        Stat.gameObject.SetActive(false);
        Type.text = "skill";
        cost.text = "2";
        cardImage.sprite = skill.Image;
        cardPopup.gameObject.SetActive(true);
    }

    // 카드팝업 호출
    public void ShowMinionPopup(MinionCardData data, Vector3 pos, Sprite sprite)
    {
        Stat.gameObject.SetActive(true);
        cardPopup.transform.position= pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        // 만약 글자가 25글자 이상이면 폰트크기를 약간 줄이기
        Debug.Log("글자 길이 : "+data.cardDescription.Length);
        Description.fontSize = (data.cardDescription.Length > 39) ? 15f : 18f;
        Stat.text = $"<color=yellow>ATT {data.att} <color=red>HP {data.hp} <color=black>몬스터";
        Type.text = data.cardType.ToString();
        cost.text = data.cost.ToString();
        cardImage.sprite = sprite;
        cardPopup.gameObject.SetActive(true);
    }
    public void ShowSpellPopup(SpellCardData data, Vector3 pos)
    {
        cardPopup.transform.position = pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = $"<color=black>주문";
        Type.text = data.cardType.ToString();
        cost.text = data.cost.ToString();
        cardImage.sprite = null;
        cardPopup.gameObject.SetActive(true);
    }

    public void ShowCardPopup(ref WeaponCardData data, Vector3 pos)
    {
        Stat.gameObject.SetActive(true);
        cardPopup.transform.position = pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = "";
        Type.text = data.cardType.ToString();
        cost.text = data.cost.ToString();
        cardImage.sprite = null;
        cardPopup.gameObject.SetActive(true);
    }
}
