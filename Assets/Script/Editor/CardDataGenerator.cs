using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Diagnostics.Eventing.Reader;

public class CardDataGenerator : EditorWindow
{

    [MenuItem("Window/Card Data Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CardDataGenerator));
    }

    TextAsset jsonFile; // ���� ������ �������Ͽ� �����Ұ��
    CardData cardData;
    Define.cardType type;
    string path;

    public void Parse(TextAsset asset )
    {
        // �巡�� �� ����� ���� JSON ������ �޴� �κ�
        jsonFile = asset;
        path =  AssetDatabase.GetAssetPath(asset);
        // ���� CardData.cs�� ���̽����� ��ȯ�� ���ϸ� �ǵ�����ϱ⿡ Ȯ��
        if (jsonFile != null)
        {
            try
            {
                CardData cd = JsonConvert.DeserializeObject<MinionCardData>(jsonFile.text, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

                switch (cd.cardType)
                {
                    case Define.cardType.minion:
                        cardData = JsonConvert.DeserializeObject<MinionCardData>
                            (jsonFile.text, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }); ;
                        break;
                    case Define.cardType.spell:
                        cardData = JsonConvert.DeserializeObject<SpellCardData>
                            (jsonFile.text, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }); ;
                        break;
                    case Define.cardType.weapon:
                        cardData = JsonConvert.DeserializeObject<WeaponCardData>
                            (jsonFile.text, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }); ;
                        break;
                }

            }
            catch
            {
                EditorGUILayout.HelpBox("�ùٸ��� ���� JSON �����Դϴ�. CardData Ŭ������ ������ ������ ����� �ּ���.", MessageType.Warning);
                jsonFile = null;
                cardData = null;
            }
        }

    }
    void OnGUI()
    {
        GUILayout.Label("ī�� ������", EditorStyles.boldLabel);

        // �߰��� ���̽����� �ٲ���, �ٽ� ó������ �ٲ����Ϸ� �����ͱ׸���
        TextAsset file = (TextAsset)EditorGUILayout.ObjectField("���� ī�� ����(�����Ұ� �巡��) ", jsonFile, typeof(TextAsset), false);
        if (file != jsonFile)
        {
            Parse(file);
            return;
        }

        #region ���� ī�嵥���͸� �����ҋ�
        if (jsonFile == null && cardData == null)
        {
            TextAsset a = (TextAsset)EditorGUILayout.ObjectField("���� ī�� ����(�����Ұ� �巡��) ",
                jsonFile, typeof(TextAsset), false);
            Parse(a);
        }

        if (jsonFile != null)
        {
            // ���� � ī�带 ������ ����
            type = (Define.cardType)EditorGUILayout.EnumPopup("ī�� Ÿ�� : ", cardData.cardType);
            // ������ ī�� Ÿ�Կ� �°� �߰� ������Ƽ �����ֱ�
            switch (type)
            {
                case Define.cardType.minion:
                    MinionCardData mc = null;
                    // ī��Ÿ�� (�ֹ�,�̴Ͼ�,������)�� �ٲ�ٸ� Ŭ���� ��ü�� �ٲپ���ϴ� ��Ȳ
                    if (cardData is MinionCardData)
                    {
                       mc = (MinionCardData)cardData;
                    }
                    else
                    { 
                        mc = new MinionCardData();
                        mc.evtDatas = cardData.evtDatas;
                        cardData = mc;
                    }
                    mc.att = EditorGUILayout.IntField("���ݷ� : ", mc.att);
                    mc.hp = EditorGUILayout.IntField("ü�� : ", mc.hp);
                    mc.isTaunt = EditorGUILayout.Toggle("���� ���� : ", mc.isTaunt);
                    mc.isCharge = EditorGUILayout.Toggle("���� ���� : ", mc.isCharge);
                    break;
                case Define.cardType.spell:
                    SpellCardData sc = null;
                    if (cardData is SpellCardData)
                    { sc = (SpellCardData)cardData; }
                    else 
                    { 
                        sc = new SpellCardData();
                        sc.evtDatas = cardData.evtDatas;
                        cardData = sc;
                    }
                    break;
                case Define.cardType.weapon:
                    WeaponCardData wc = null;
                    if (cardData is WeaponCardData)
                    { wc = (WeaponCardData)cardData; }
                    else
                    {
                        wc = new WeaponCardData();
                        wc.evtDatas = cardData.evtDatas;
                        cardData = wc;
                    }
                    wc.att = EditorGUILayout.IntField("���ݷ� : ", wc.att);
                    wc.durability = EditorGUILayout.IntField("������ : ", wc.durability);
                    break;
            }
            // ���� ���� ����
            cardData.cardClass = (Define.classType)EditorGUILayout.EnumPopup("����ī�� : ", cardData.cardClass);
            cardData.cardType = type;
            // ���� ���� ���
            cardData.cardRarity = (Define.cardRarity)EditorGUILayout.EnumPopup("��͵� : ", cardData.cardRarity);
            cardData.cardName = EditorGUILayout.TextField("Card Name: ", cardData.cardName);
            cardData.cardDescription = EditorGUILayout.TextField("���� : ", cardData.cardDescription);
            cardData.cardIdNum = EditorGUILayout.IntField("����ID: ", cardData.cardIdNum);
            cardData.cost = EditorGUILayout.IntField("Cost: ", cardData.cost);
            cardData.associatedHandler = EditorGUILayout.TextField("��ũ��Ʈ�� : ", cardData.associatedHandler);
            path = EditorGUILayout.TextField("Path ", path);

            // ��ư ���� �� ���̽����Ϸ� ��������
            if (GUILayout.Button("Json ��������"))
            {
                SaveCardData();
            }
        }

        #endregion

        #region ���ο� ī�嵥���� �����ҋ�
        else
        { 
            // ������ ���� �ʱ�
            if (cardData == null)
            {
                 // ���� � ī�带 ������ ����
                 type = (Define.cardType)EditorGUILayout.EnumPopup("ī�� Ÿ�� : ", type);

                 // ����� ����
                 if (GUILayout.Button("�����"))
                 {
                     // ������ ī�� Ÿ�Կ� �°� �߰� ������Ƽ �����ֱ�
                     switch (type)
                     {
                         case Define.cardType.minion:
                             cardData = new MinionCardData();
                             break;
                         case Define.cardType.spell:
                             cardData = new SpellCardData();
                             break;
                         case Define.cardType.weapon:
                             cardData = new WeaponCardData();
                             break;
                     }

                 }
                return;
            }

            // ������ ���� ����
            else
            {
                // ī���� Ÿ�� ���� Ȯ�� �� ���� ������ ���� ����
                cardData.cardType = type;
                // ���� ���� ����
                cardData.cardClass = (Define.classType)EditorGUILayout.EnumPopup("����ī�� : ", cardData.cardClass);

                // ���� ���� ���
                cardData.cardName = EditorGUILayout.TextField("Card Name: ", cardData.cardName);
                cardData.cardRarity = (Define.cardRarity)EditorGUILayout.EnumPopup("��͵� : ", cardData.cardRarity);
                cardData.cardDescription = EditorGUILayout.TextField("���� : ", cardData.cardDescription);
                cardData.cardIdNum = EditorGUILayout.IntField("����ID: ", cardData.cardIdNum);
                cardData.cost = EditorGUILayout.IntField("Cost: ", cardData.cost);
                cardData.associatedHandler = EditorGUILayout.TextField("��ũ��Ʈ�� : ", cardData.associatedHandler);
                // ������ ī�� Ÿ�Կ� �°� �߰� ������Ƽ �����ֱ�
                switch (type)
                {
                    case Define.cardType.minion:
                        if (cardData == null)
                        { cardData = new MinionCardData(); }
                        MinionCardData mc = (MinionCardData)cardData;
                        mc.att = EditorGUILayout.IntField("���ݷ� : ", mc.att);
                        mc.hp = EditorGUILayout.IntField("ü�� : ", mc.hp);
                        mc.isTaunt = EditorGUILayout.Toggle("���� ���� : ",mc.isTaunt);
                        mc.isCharge = EditorGUILayout.Toggle("���� ���� : ", mc.isCharge);
                        break;
                    case Define.cardType.spell:
                        if (cardData == null)
                        { cardData = new SpellCardData(); }
                        SpellCardData sc = (SpellCardData)cardData;

                        break;
                    case Define.cardType.weapon:
                        if (cardData == null)
                        { cardData = new WeaponCardData(); }

                        WeaponCardData wc = (WeaponCardData)cardData;
                        wc.att = EditorGUILayout.IntField("���ݷ� : ", wc.att);
                        wc.durability = EditorGUILayout.IntField("������ : ", wc.durability);
                        break;
                }

                path = EditorGUILayout.TextField("Path ", path);
                // ��ư ���� �� ���̽����Ϸ� ��������
                if (GUILayout.Button("Json ��������"))
                {
                    SaveCardData();
                }
            }
        }

        #endregion


        // �ٽ� �ʱ�ȭ������ �ʱ�ȭ��ư
        if (GUILayout.Button("�ٽ� ó������"))
        {
            cardData = null;
            jsonFile = null;
        }
    }

    void SaveCardData()
    {
        string json = JsonConvert.SerializeObject(cardData, Formatting.Indented
         , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
    }
}
