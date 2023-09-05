using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
public class CardSelect : LobbyPopup
{
    public CardViewport cardView;
    public DeckViewport deckView;

    // editCanvas내의 캐릭터선택창에서 캐릭터 선택시
    // 덱뷰포트 와 카드뷰포트 모드 선택한 캐릭터와 새로운덱으로 시작
    public void MakeNewDeck(Define.classType type)
    {
        deckView.EnterEmptyDeck(type);
        cardView.EnterEmptyDeck(type);
    }
}
