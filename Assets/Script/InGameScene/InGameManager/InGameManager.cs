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
        int newIdx = Mathf.Clamp(idx, 0, this.Count);
        Debug.Log($"리스트삽입, 원본idx:{idx},  변경idx:{newIdx}");
        // 기본 Insert 함수 실행후
        base.Insert(newIdx, ele);
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
    public CardPopupEvtHolder cardPopup;
    public TextMeshPro cardName, Description, Stat, Type, Cost;
    public SpriteRenderer cardImage,cardBackground;

    #region 참조
    public FindEvtHolder FindEvt { get; set; }
    public HeroManager Hero { get; set; }
    public HandManager Hand { get; set; }
    public SpawnManager Spawn { get; set; }
    public TargetingCamera TC { get; set; }
    public BattleManager Battle { get; set; }
    public TurnEndBtn Turn { get; set; }
    public PacketManager Packet { get; set; }
    public PostCamera Post { get; set; }
    private void Awake()
    {
        GAME.IGM = this;
    }
    #endregion


    // BattleManager의 액션큐 빠르게 접근용도
    public void AddAction(IEnumerator co) { if (co != null) { Battle.ActionQueue.Enqueue(co); } }
    public void AddDeathAction(IEnumerator co) { if (co != null) { Battle.PlayDeathRattle(co); } }
    // 영웅 능력 팝업창 호출
    public void ShowHeroSkill(Vector3 pos, HeroData skill)
    {
        cardPopup.transform.position = pos;
        cardName.text = skill.skillName;
        Description.text = skill.skillDesc;
        Stat.gameObject.SetActive(false);
        Type.text = "skill";
        Cost.text = skill.skillCost.ToString();
        cardImage.sprite = (skill.IsMine) ? GAME.IGM.Hero.Player.skillImg.sprite : GAME.IGM.Hero.Enemy.skillImg.sprite;
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
        Stat.text = $"<color=green>ATT {data.att} <color=red>HP {data.hp} <color=black>몬스터";
        Type.text = data.cardType.ToString();
        Cost.text = data.cost.ToString();
        cardImage.sprite = sprite;
        cardPopup.gameObject.SetActive(true);
    }
    public void ShowSpellPopup(SpellCardData data, Vector3 pos)
    {
        // 현재 스폰중인카드가 있다고 설정해, 다른 카드 엔터 이벤트 방지
        cardPopup.isEnmeySpawning = true;

        cardPopup.transform.position = pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = $"<color=black>주문";
        Type.text = data.cardType.ToString();
        Cost.text = data.cost.ToString();
        cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum );
        StartCoroutine(FadeIn());
        IEnumerator FadeIn()
        {
            float t = 0;
            // 투명화 위해 모든 TMP와 SR을 묶기
            List<TextMeshPro> tmpList = new List<TextMeshPro>() { cardName, Description, Cost, Stat, Type };
            List<SpriteRenderer> imageList = new List<SpriteRenderer>() { cardImage, cardBackground };

            cardPopup.gameObject.SetActive(true);

            tmpList.ForEach(x => x.alpha = 0);
            imageList.ForEach(x => x.color = new Color(1, 1, 1, 0));
            Color tempColor = Color.white;
            while (t < 1f)
            {
                // 알파값 점차 1으로 변환
                t += Time.deltaTime;
                tempColor.a = t;
                tmpList.ForEach(x => x.alpha = t);
                imageList.ForEach(x => x.color = tempColor);
                yield return null;
            }
        }
    }
    public void ShowSpawningMinionPopup(MinionCardData data, int att, int hp, int cost)
    {
        // 현재 스폰중인카드가 있다고 설정해, 다른 카드 엔터 이벤트 방지
        cardPopup.isEnmeySpawning = true;

        cardPopup.transform.position = new Vector3(3.5f, 2.8f, -0.5f);
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = $"<color=green>ATT {att} <color=red>HP {hp} <color=black>몬스터";
        Type.text = data.cardType.ToString();
        Cost.text = cost.ToString();
        cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);
        StartCoroutine(FadeIn());
        IEnumerator FadeIn()
        {
            float t = 0;
            // 투명화 위해 모든 TMP와 SR을 묶기
            List<TextMeshPro> tmpList = new List<TextMeshPro>() { cardName, Description, Cost, Stat, Type };
            List<SpriteRenderer> imageList = new List<SpriteRenderer>() { cardImage, cardBackground };

            cardPopup.gameObject.SetActive(true);

            tmpList.ForEach(x => x.alpha = 0);
            imageList.ForEach(x => x.color = new Color(1, 1, 1, 0));
            Color tempColor = Color.white;
            while (t < 1f)
            {
                // 알파값 점차 1으로 변환
                t += Time.deltaTime;
                tempColor.a = t;
                tmpList.ForEach(x => x.alpha = t);
                imageList.ForEach(x => x.color = tempColor);
                yield return null;
            }

            yield return new WaitForSeconds(1.5f);

            cardPopup.isEnmeySpawning = false;
            cardPopup.gameObject.SetActive(false);
        }
    }
    public void ShowCardPopup(ref WeaponCardData data, Vector3 pos)
    {
        Stat.gameObject.SetActive(true);
        cardPopup.transform.position = pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = "";
        Type.text = data.cardType.ToString();
        Cost.text = data.cost.ToString();
        cardImage.sprite = null;
        cardPopup.gameObject.SetActive(true);
    }
}
