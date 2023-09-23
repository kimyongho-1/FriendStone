using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Diagnostics.Eventing.Reader;
using ExitGames.Client.Photon;
using static Define;

public class CardDataGenerator : EditorWindow
{

    [MenuItem("Window/Card Data Editor")]
    public static void ShowWindow()
    {
        EditorWindow ew = EditorWindow.GetWindow(typeof(CardDataGenerator));
        // �������� �ּ� ũ�⸦ ����
        ew.minSize = new Vector2(900, 600);

    }

    // ������â�� ���������� ȣ��
    public void OnDisable()
    {
        // ������â ������, ���� �������� ���̽����������� �ٲ� ���� ��������
        if (cardData != null && jsonFile != null)
        {
            string json = JsonConvert.SerializeObject(cardData, Formatting.Indented
                           , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

            // ���� ���̽����� �ٲ� ���� ���·� ����
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }
    }

    TextAsset jsonFile; // ���� ������ �������Ͽ� �����Ұ��
    CardData cardData; // �������� ���̽������� �ٽ� Ŭ����ȭ�Ͽ� ����ҋ� �������� ����
    Define.cardType type;
    string path;

    int selectedIdx; // ���� ���� �� �������� �̺�Ʈ������ Ŭ���� �ε���

   

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(position.width / 2));
        GUILayout.Label("ī�� ������ ����");
        CardDataEditor();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("�̺�Ʈ ������ ����");
        EvtDataEditor();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }
    void EvtDataEditor()
    {
        #region CODE
        // ���� ������ ������ ������ �ȵǱ⿡ return
        if (jsonFile == null || cardData == null) { selectedIdx = -1; return; }

        // �̺�Ʈ �߰� ��ư
        if (GUILayout.Button("�̺�Ʈ �߰�"))
        {
            // ���� �̺�Ʈ ���δ� �𸣰�����, �� ����ŸŬ���� ����
            cardData.evtDatas.Add(new BuffHandler());
            // �ʱ�ȭ
            selectedIdx = -1;
        }

        if (selectedIdx == -1)
        {
            // �̺�Ʈ ����Ʈ ǥ��
            for (int i = 0; i < cardData.evtDatas.Count; i++)
            {
                GUILayout.BeginHorizontal();
                // �ش� �̺�Ʈ Ŭ���� ���� ��ư
                if (GUILayout.Button($"{i}�� : {cardData.evtDatas[i].type} [Ŭ���� ����]"))
                {
                    selectedIdx = i;
                }

                // �ش� �̺�Ʈ ���� ��ư
                if (GUILayout.Button($"{i}�� {cardData.evtDatas[i].type} �̺�Ʈ ����"))
                {
                    selectedIdx = -1;
                    cardData.evtDatas.RemoveAt(i);
                    GUILayout.EndHorizontal();
                    return;
                }
                GUILayout.EndHorizontal();
            }

        }


        // ���õ� �̺�Ʈ�� �� ���� ǥ��
        else
        {
            if (GUILayout.Button("�ڷ� ����"))
            {
                selectedIdx = -1;
                return;
            }

            // ���� �̺�Ʈ�� Ÿ���� �ٲ������ Ȯ��
            Define.evtType evtType = (Define.evtType)EditorGUILayout.EnumPopup("� �̺�Ʈ�� ", cardData.evtDatas[selectedIdx].type);

            // �̺�Ʈ Ÿ�� ��ü�� �ٲ���ٸ� �ش� �̺�Ʈ Ŭ������ ���� ����� �����Ͱ� �ٲٱ�
            switch (evtType)
            {
                case Define.evtType.buff:
                    BuffHandler bh = null;
                    if (cardData.evtDatas[selectedIdx].type is evtType.buff)
                    {
                        // �̺�Ʈ Ÿ���� �ٲ��� �ʾҴٸ� �״�� ���� ����
                        bh = (BuffHandler)cardData.evtDatas[selectedIdx];
                    }
                    else
                    {
                        // ���� �̺�ƮŸ�԰� �ٸ��� ���� �����Ͽ� ���� ������ �̿��Ͽ� ��������
                        bh = new BuffHandler();
                    }
                    #region BuffHandler �Ӽ� �׸���
                    bh.buffTargeting = (Define.buffTargeting)EditorGUILayout.EnumPopup("Ÿ���� Ÿ�� ", bh.buffTargeting);
                    bh.buffType = (Define.buffType)EditorGUILayout.EnumPopup("� ���� ", bh.buffType);
                    bh.buffExtraArea = (Define.buffExtraArea)EditorGUILayout.EnumPopup("�߰� ��� ", bh.buffExtraArea);
                    bh.buffFX = (Define.buffFX)EditorGUILayout.EnumPopup("����� ȿ�� ", bh.buffFX);
                    EditorGUI.BeginChangeCheck();
                    int len = EditorGUILayout.IntField("���� ī�� ��", bh.relatedIds.Length);
                    // �迭 ������ ũ�� Ȯ����, �迭 ũ�� ����
                    if (EditorGUI.EndChangeCheck())
                    { Array.Resize(ref bh.relatedIds, len); }

                    for (int i = 0; i < bh.relatedIds.Length; i++)
                    {
                        bh.relatedIds[i] = EditorGUILayout.IntField($"{i}�� ������ȣ : ", bh.relatedIds[i]);
                    }
                    bh.buffAtt = EditorGUILayout.IntField("����������", bh.buffAtt);
                    bh.buffHp = EditorGUILayout.IntField("ü��������", bh.buffHp);
                    bh.costCount = EditorGUILayout.IntField("��ο� ��", bh.costCount);
                    #endregion
                    SetProperty(bh);
                    // ���� �������� �����͸� �ٲ� Ÿ��Ŭ������ �ʱ�ȭ
                    cardData.evtDatas[selectedIdx] = bh;
                    break;
                case Define.evtType.attack:
                    AttackHandler ah = null;
                    if (cardData.evtDatas[selectedIdx].type is evtType.attack)
                    {
                        // �̺�Ʈ Ÿ���� �ٲ��� �ʾҴٸ� �״�� ���� ����
                        ah = (AttackHandler)cardData.evtDatas[selectedIdx];
                    }
                    else
                    {
                        // ���� �̺�ƮŸ�԰� �ٸ��� ���� �����Ͽ� ���� ������ �̿��Ͽ� ��������
                        ah = new AttackHandler();
                    }
                    #region AttackHandler �Ӽ� �׸���
                    ah.attTargeting = (Define.attTargeting)EditorGUILayout.EnumPopup("Ÿ���� Ÿ�� ", ah.attTargeting);
                    ah.attType = (Define.attType)EditorGUILayout.EnumPopup("� ���� ", ah.attType);
                    ah.attFX = (Define.attFX)EditorGUILayout.EnumPopup("�߻��� ȿ�� ", ah.attFX);
                    ah.attAmount = EditorGUILayout.IntField("���ݷ�", ah.attAmount);
                    #endregion
                    SetProperty(ah);
                    cardData.evtDatas[selectedIdx] = ah;
                    break;
                case Define.evtType.restore:
                    RestoreHandler rh = null;
                    if (cardData.evtDatas[selectedIdx].type is evtType.restore)
                    {
                        // �̺�Ʈ Ÿ���� �ٲ��� �ʾҴٸ� �״�� ���� ����
                        rh = (RestoreHandler)cardData.evtDatas[selectedIdx];
                    }
                    else
                    {
                        // ���� �̺�ƮŸ�԰� �ٸ��� ���� �����Ͽ� ���� ������ �̿��Ͽ� ��������
                        rh = new RestoreHandler();
                    }
                    #region RestoreHandler �Ӽ� �׸���
                    rh.restoreTargeting = (Define.restoreTargeting)EditorGUILayout.EnumPopup("Ÿ���� Ÿ��", rh.restoreTargeting);
                    rh.restoreExtraArea = (Define.restoreExtraArea)EditorGUILayout.EnumPopup("�߰� ���", rh.restoreExtraArea);
                    rh.restoreFX = (Define.restoreFX)EditorGUILayout.EnumPopup("����� ȿ��", rh.restoreFX);
                    rh.restoreAmount = EditorGUILayout.IntField("ȸ����", rh.restoreAmount);
                    #endregion
                    SetProperty(rh);
                    cardData.evtDatas[selectedIdx] = rh;
                    break;
                case Define.evtType.utill:
                    UtillHandler uh = null;
                    if (cardData.evtDatas[selectedIdx].type is evtType.utill)
                    {
                        // �̺�Ʈ Ÿ���� �ٲ��� �ʾҴٸ� �״�� ���� ����
                        uh = (UtillHandler)cardData.evtDatas[selectedIdx];
                    }
                    else
                    {
                        // ���� �̺�ƮŸ�԰� �ٸ��� ���� �����Ͽ� ���� ������ �̿��Ͽ� ��������
                        uh = new UtillHandler();
                    }
                    #region UtillHandler �Ӽ� �׸���
                    uh.utillType = (Define.utillType)EditorGUILayout.EnumPopup("� ���� ", uh.utillType);
                    EditorGUI.BeginChangeCheck();
                    len = EditorGUILayout.IntField("���� ī�� ��", uh.relatedCards.Length);
                    // �迭 ������ ũ�� Ȯ����, �迭 ũ�� ����
                    if (EditorGUI.EndChangeCheck())
                    { Array.Resize(ref uh.relatedCards, len); }

                    for (int i = 0; i < uh.relatedCards.Length; i++)
                    {
                        uh.relatedCards[i] = EditorGUILayout.IntField($"{i}�� ������ȣ : ", uh.relatedCards[i]);
                    }
                    uh.utillAmount = EditorGUILayout.IntField("��ο�� : ", uh.utillAmount);
                    #endregion
                    SetProperty(uh);
                    cardData.evtDatas[selectedIdx] = uh;
                    break;
            }

            // ���� ���� ���� (CardBaseEvtData.cs �ֻ��� �θ�)
            void SetProperty(CardBaseEvtData evtData)
            {
                evtData.type = evtType; // �ֻ�� ������Ʈ �ʵ� ����
                evtData.when = (Define.evtWhen)EditorGUILayout.EnumPopup("���� �ߵ� : ", cardData.evtDatas[selectedIdx].when);
                evtData.area = (Define.evtArea)EditorGUILayout.EnumPopup("Ÿ�� ���� : ", cardData.evtDatas[selectedIdx].area);
                evtData.faction = (Define.evtFaction)EditorGUILayout.EnumPopup("Ÿ��Ÿ�� : ", cardData.evtDatas[selectedIdx].faction);
            }

            // �ٲ� �� �Ǵ� ������ ���� ������ ����
            cardData.evtDatas[selectedIdx] = cardData.evtDatas[selectedIdx];
        }
        #endregion
    }
    void CardDataEditor()
    {
        #region CODE
        GUILayout.Label("ī�� ������", EditorStyles.boldLabel);
        // ���� �������̴� �ƴϵ�, �� ���� ������ Ŭ��
        if (GUILayout.Button("�� ����Ÿ ���� �����"))
        {
            // �� ������ ���� �� ���� => ����Ʈ �̴Ͼ�ī��� ����
            cardData = new MinionCardData();
            cardData.cardName = "���ο� ī�� ������";
            string newFile = JsonConvert.SerializeObject(cardData, Formatting.Indented
                     , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            path = $"Assets/Resources/{cardData.cardName}.json";
            File.WriteAllText(path, newFile);
            jsonFile = new TextAsset(newFile);
            AssetDatabase.Refresh();
        }

        // ���� ����(�۾�)���� ������ ���� �����ִ� ������Ʈ �ʵ� ����
        TextAsset file = (TextAsset)EditorGUILayout.
            ObjectField("���� ī�� ����(�����Ұ� �巡��) ", jsonFile, typeof(TextAsset), false);

        // ���� �۾�����, ���� �ϴ� ������ ������ ������ ���̻� �׸��� ����
        if (file == null)
        { return; }

        // ���������� ����Ǿ��ٸ�, ���� ������â�� ����� ���Ϸ� �ٲٱ� ���� �۾�
        if (file != jsonFile)
        {
            // �������� ����� ���� �������� �ٲ��츦 �����,
            // ���� ���� ���������� �����ϰ� �����۾� ����
            if (jsonFile != null && cardData != null)
            {
                string json = JsonConvert.SerializeObject(cardData, Formatting.Indented
                     , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                // ���� ���̽����� �ٲ� ���� ���·� ����
                File.WriteAllText(path, json);
                AssetDatabase.Refresh();
                cardData = null; path = null;
            }


            // �ٲ� ���Ϸ� ���� ���� �� Ŭ����ȭ �ٽ� ����
            jsonFile = file;

            // ���� ������ ��η� �ʱ�ȭ
            path = AssetDatabase.GetAssetPath(file);

            // ���̽� ���� Ŭ����ȭ �� ���� cardData �ʱ�ȭ (���н� cardData�� null����)
            JsonToClass();

            // ���Լ����� ��� ������ Ŭ����ȭ�� �����ҽ� �ٽ� ó������ �����ϵ��� ����
            if (cardData == null || jsonFile == null)
            {
                cardData = null;
                jsonFile = null;
                path = null;
                return;
            }
        }

        // ���� �������� JsonFile�� ������
        // �װ��� ������â�� �׸���
        DrawCurrentCardData();

        // Ŭ���� , Path��η� �̸������Ͽ� ����
        if (GUILayout.Button("���� ����"))
        {
            string json = JsonConvert.SerializeObject(cardData, Formatting.Indented
                   , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            // ���� ���̽����� �ٲ� ���� ���·� ����
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }

        #endregion
    }

    // ���̽������� Ŭ����ȭ 
    public void JsonToClass()
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

    // ���� �������� ���̽������� Ŭ����ȭ �� ���� cardData�� �����ϴ°��� ������â�� �׸���
    public void DrawCurrentCardData()
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
                }
                mc.att = EditorGUILayout.IntField("���ݷ� : ", mc.att);
                mc.hp = EditorGUILayout.IntField("ü�� : ", mc.hp);
                mc.isTaunt = EditorGUILayout.Toggle("���� ���� : ", mc.isTaunt);
                mc.isCharge = EditorGUILayout.Toggle("���� ���� : ", mc.isCharge);
                SetProperty(mc);
                cardData = mc;
                break;
            case Define.cardType.spell:
                SpellCardData sc = null;
                if (cardData is SpellCardData)
                { sc = (SpellCardData)cardData; }
                else
                {
                    sc = new SpellCardData();
                }
                SetProperty(sc);
                cardData = sc;
                break;
            case Define.cardType.weapon:
                WeaponCardData wc = null;
                if (cardData is WeaponCardData)
                { wc = (WeaponCardData)cardData; }
                else
                {
                    wc = new WeaponCardData();
                }
                wc.att = EditorGUILayout.IntField("���ݷ� : ", wc.att);
                wc.durability = EditorGUILayout.IntField("������ : ", wc.durability);
                SetProperty(wc);
                cardData = wc;
                break;
        }

        // Ÿ�� ��ü�� �ٲ�� Ŭ������ �ٲ�⿡, ���� ���� CardData.cs ���� ������ ����
        void SetProperty(CardData cd)
        {
            // ���� ���� ����
            cd.cardClass = (Define.classType)EditorGUILayout.EnumPopup("����ī�� : ", cardData.cardClass);
            cd.cardType = type;
            // ���� ���� ���
            cd.cardRarity = (Define.cardRarity)EditorGUILayout.EnumPopup("��͵� : ", cardData.cardRarity);
            cd.cardName = EditorGUILayout.TextField("Card Name: ", cardData.cardName);
            cd.cardDescription = EditorGUILayout.TextField("���� : ", cardData.cardDescription);
            cd.cardIdNum = EditorGUILayout.IntField("����ID: ", cardData.cardIdNum);
            cd.cost = EditorGUILayout.IntField("Cost: ", cardData.cost);
            cd.associatedHandler = EditorGUILayout.TextField("��ũ��Ʈ�� : ", cardData.associatedHandler);
            cd.evtDatas = cardData.evtDatas;
        }

        path = EditorGUILayout.TextField("Path ", path);
    }
}