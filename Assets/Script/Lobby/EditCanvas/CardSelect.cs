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

    // editCanvas���� ĳ���ͼ���â���� ĳ���� ���ý�
    // ������Ʈ �� ī�����Ʈ ��� ������ ĳ���Ϳ� ���ο���� ����
    public void MakeNewDeck(Define.classType type)
    {
        deckView.EnterEmptyDeck(type);
        cardView.EnterEmptyDeck(type);
    }
}
