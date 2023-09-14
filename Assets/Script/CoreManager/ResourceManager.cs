using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManager
{
    public CardPath PathFinder;
    public ResourceManager(string pathFinder)
    {
        // ��� ���̽� ���Ͽ� �����Ҽ��ֵ���, ������ ��θ� ��ȯ�ϴ� ���̽����� 
        PathFinder = JsonConvert.DeserializeObject<CardPath>(pathFinder);
    }

    // ���� ���ø����̼��� ������ĺ���
    // ������ ���θ���ų�, ������ ���給�� �ٽ� �ε��ҋ� ��� ��Ȳ����
    // �������� �����ؼ� ����Ҽ��ֵ��� ���� ������ ��� �� List
    public List<DeckData> userDecks = new List<DeckData>();
    public DeckData GameDeck;
    // ������ �������� ��� ��������
    public void LoadUsersDeck()
    { }
    

    // ���� �̹��� ��������
    public Sprite GetHeroImage(Define.classType type)
    { return Resources.Load<Sprite>($"Texture/CardImage/heroImage/{type.ToString()}"); }

    // ī�������� ������ȣ ���ؼ� �̹��� ã��
    public Sprite GetImage(Define.classType type, int id)
    { return Resources.Load<Sprite>($"Texture/CardImage/{type.ToString()}/{type.ToString()}{id}"); }
}
