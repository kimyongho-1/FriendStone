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
        // ��� ���̽� ���Ͽ� �����Ҽ��ֵ���, ������ ��θ� ��ȯ�ϴ� ���̽����� 
        PathFinder = JsonConvert.DeserializeObject<CardPath>(pathFinder);
    }

    // ���� ���ø����̼��� ������ĺ���
    // ������ ���θ���ų�, ������ ���給�� �ٽ� �ε��ҋ� ��� ��Ȳ����
    // �������� �����ؼ� ����Ҽ��ֵ��� ���� ������ ��� �� List
    public List<DeckData> userDecks = new List<DeckData>();
    public DeckData GameDeck;

    // InGameScene ���Խ�, ���������� ����
    public HeroData GetHeroData(Define.classType type)
    {
        switch (type)
        {
            case Define.classType.HJ:
                return JsonConvert.DeserializeObject<HeroData>(GAME.Manager.RM.PathFinder.Dic[10000].GetJson()
                       , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            case Define.classType.HZ:
                return JsonConvert.DeserializeObject<HeroData>(GAME.Manager.RM.PathFinder.Dic[10001].GetJson()
                        , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            default:
                return JsonConvert.DeserializeObject<HeroData>(GAME.Manager.RM.PathFinder.Dic[10002].GetJson()
                        , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }

    }

    public AudioClip GetClip(Define.classType heroType, Define.Emotion emo)
    {
        return Resources.Load<AudioClip>($"Sound/HeroEmotion/{heroType.ToString()}/{emo.ToString()}");
    }

    // ���� �̹��� ��������
    public Sprite GetHeroImage(Define.classType type)
    { return Resources.Load<Sprite>($"Texture/CardImage/heroImage/{type.ToString()}"); }
    public Sprite GetHeroSkillIcon(Define.classType type)
    { return Resources.Load<Sprite>($"Texture/CardImage/heroImage/{type.ToString()}SkillIcon"); }
    // ī�������� ������ȣ ���ؼ� �̹��� ã��
    public Sprite GetImage(Define.classType type, int id)
    { return Resources.Load<Sprite>($"Texture/CardImage/{type.ToString()}/{type.ToString()}{id}"); }

    // ī�尡 �ո����� �޸����� Ȯ���� �̹��� ��������
    public Sprite GetCardSprite(bool isFront)
    {
        return Resources.Load<Sprite>($"Texture/CardImage/{((isFront) ? "CardFront" : "CardBack")}");
    }
}
