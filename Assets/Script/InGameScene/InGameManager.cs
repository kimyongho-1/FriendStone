using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    public GameObject cardPopup;
    public TextMeshPro cardName, Description, Stat, Type, cost;
    public SpriteRenderer cardImage;

    public HeroManager Hero { get; set; }
    public HandManager Hand { get; set; }
    public SpawnManager Spawn { get; set; }
    public TargetingCamera TC { get; set; } 
    
    private void Awake()
    {
        GAME.Manager.IGM = this;
    }

    // 카드팝업 호출
    public void ShowCardPopup(ref CardData data, Vector3 pos)
    {
        cardPopup.transform.position= pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = "";
        Type.text = data.cardType.ToString();
        cost.text = data.cost.ToString();
        cardImage.sprite = null;
        cardPopup.gameObject.SetActive(true);
    }
    public void ShowCardPopup(ref HeroSkill data, Vector3 pos)
    {
        cardPopup.transform.position = pos;
       // cardName.text = data.cardName;
       // Description.text = data.cardDescription;
       // Stat.text = "";
       // Type.text = data.cardType.ToString();
       // cost.text = data.cost.ToString();
       // cardImage.sprite = null;
       // cardPopup.gameObject.SetActive(true);
    }
    public void ShowCardPopup(ref WeaponCardData data, Vector3 pos)
    {
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
