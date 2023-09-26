using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

public class ResourceManager
{
    public CardPath PathFinder;
    public ResourceManager(string pathFinder)
    {
        // 모든 제이슨 파일에 접근할수있도록, 데이터 경로를 반환하는 제이슨파일 
        PathFinder = JsonConvert.DeserializeObject<CardPath>(pathFinder);
    }

    // 게임 애플리케이션이 실행된후부터
    // 유저가 새로만들거나, 기존에 만든덱을 다시 로드할떄 모든 상황에서
    // 언제든지 접근해서 사용할수있도록 만든 유저의 모든 덱 List
    public List<DeckData> userDecks = new List<DeckData>();
    public DeckData GameDeck;
    // 유저의 기존덱들 모두 가져오기
    public void LoadUsersDeck()
    { }
    

    // 영웅 이미지 가져오기
    public Sprite GetHeroImage(Define.classType type)
    { return Resources.Load<Sprite>($"Texture/CardImage/heroImage/{type.ToString()}"); }

    // 카드직업과 고유번호 통해서 이미지 찾기
    public Sprite GetImage(Define.classType type, int id)
    { return Resources.Load<Sprite>($"Texture/CardImage/{type.ToString()}/{type.ToString()}{id}"); }

    // 카드가 앞면인지 뒷면인지 확인후 이미지 가져오기
    public Sprite GetCardSprite(bool isFront)
    {
        return Resources.Load<Sprite>($"Texture/CardImage/{((isFront) ? "CardFront" : "CardBack")}");
    }
}
