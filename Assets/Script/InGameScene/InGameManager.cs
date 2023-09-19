using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class InGameManager : MonoBehaviour
{
    public GameObject cardPopup;
    public TextMeshPro cardName, Description, Stat, Type, cost;
    public SpriteRenderer cardImage;

    public HeroManager Hero { get; set; }
    public HandManager Hand { get; set; }
    public SpawnManager Spawn { get; set; }
    public TargetingCamera TC { get; set; } 
    public BattleManager Battle { get; set; }
    private void Awake()
    {
        GAME.Manager.IGM = this;
    }

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
