using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using UnityEditor.VersionControl;

public class CardEventGenerator : EditorWindow
{

    [MenuItem("Window/Card Event Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CardEventGenerator));
    }

    string path = "Assets/";// ������

    TextAsset jsonFile; // ���� ���̽����� (ī�嵥��������)
    CardData currData; // ������ ������Ŭ����

    CardBaseEvtData evtData; // �������, ī�尡 ����� �̺�Ʈ������
    Define.evtType evtType; // ���� ������ �̺�ƮŸ�� ����

    private void OnGUI()
    {
        if (jsonFile == null || currData == null)
        {
            // �巡�� �� ����� ���� JSON ������ �޴� �κ�
            jsonFile = (TextAsset)EditorGUILayout.ObjectField("ī�嵥���� JSON ����: ", jsonFile, typeof(TextAsset), false);
            path = AssetDatabase.GetAssetPath(jsonFile);
            // ���� CardData.cs�� ���̽����� ��ȯ�� ���ϸ� �ǵ�����ϱ⿡ Ȯ��
            if (jsonFile != null)
            {
                try
                {
                    // ���� : ����ȭ, ������ȭ ��� Auto�����ؼ� Ÿ�� ���� Ȯ��
                    // cardData.cs�� �ٽ� ��ȯ�Ͽ� Ȯ���ϱ�
                    currData = JsonConvert.DeserializeObject<CardData>(jsonFile.text
                        , new JsonSerializerSettings
                        {
                            // ��ü �������� ��Ȯ��
                            TypeNameHandling = TypeNameHandling.Auto
                        }
                        );

                }
                catch
                {
                    EditorGUILayout.HelpBox("�ùٸ��� ���� JSON �����Դϴ�. CardData Ŭ������ ������ ������ ����� �ּ���.", MessageType.Warning);
                    jsonFile = null;
                    currData = null;
                }
            }

        }

        // ������ ī�嵥���� ���̽����ϰ� ������ ī�嵥���� �ϳ��� ������� ���� ���
        if (jsonFile == null || currData == null) { return; }

        // ���̽������� CardData.cs�ΰ� �´ٸ�,
        // �̺�Ʈ Ÿ�� ����â ����
        evtType = (Define.evtType)EditorGUILayout.EnumPopup("� �̺�Ʈ�� ", evtType);

        // ���� evtType�� �����, ���� �������̴� evtData�� �����ҽ� �ٷ� �ʱ�ȭ
        if (evtData != null && evtData.type != evtType) { evtData = null; }

        // ���� �̺�Ʈ������ ������ ���� �����ߴٸ�
        if (evtData == null)
        {
            switch (evtType)
            {
                case Define.evtType.buff:
                    evtData = new BuffHandler();
                    break;
                case Define.evtType.attack:
                    evtData = new AttackHandler();
                    break;
                case Define.evtType.restore:
                    evtData = new RestoreHandler();
                    break;
                case Define.evtType.utill:
                    evtData = new UtillHandler();
                    break;
            }
            evtData.type = evtType;
        }

        // evtData�������̾��ٸ� => ������ ������Ƽ ���� �ϵ���
        else
        {
            evtData.type = evtType;
            evtData.when = (Define.evtWhen)EditorGUILayout.EnumPopup("���� �ߵ� : ", evtData.when);
            evtData.area = (Define.evtArea)EditorGUILayout.EnumPopup("���� ���� : ", evtData.area); 
            switch (evtType)
            {
                case Define.evtType.buff:
                    BuffHandler bh = (BuffHandler)evtData;
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
                    bh.buffAtt = EditorGUILayout.IntField("����������",bh.buffAtt);
                    bh.buffHp = EditorGUILayout.IntField("ü��������", bh.buffHp);
                    bh.drawCount = EditorGUILayout.IntField("��ο� ��", bh.drawCount);
                    break;
                case Define.evtType.attack:
                    AttackHandler ah = (AttackHandler)evtData;
                    ah.attTargeting = (Define.attTargeting)EditorGUILayout.EnumPopup("Ÿ���� Ÿ�� ", ah.attTargeting);
                    ah.attType = (Define.attType)EditorGUILayout.EnumPopup("� ���� ", ah.attType);
                    ah.attExtraArea = (Define.attExtraArea)EditorGUILayout.EnumPopup("�߰� ��� ", ah.attExtraArea);
                    ah.attFX = (Define.attFX)EditorGUILayout.EnumPopup("�߻��� ȿ�� ", ah.attFX);
                    ah.attAmount = EditorGUILayout.IntField("���ݷ�",ah.attAmount);
                    break;
                case Define.evtType.restore:
                    RestoreHandler rh = (RestoreHandler)evtData;
                    rh.restoreTargeting = (Define.restoreTargeting)EditorGUILayout.EnumPopup("Ÿ���� Ÿ��", rh.restoreTargeting);
                    rh.restoreExtraArea = (Define.restoreExtraArea)EditorGUILayout.EnumPopup("�߰� ���", rh.restoreExtraArea);
                    rh.restoreFX = (Define.restoreFX)EditorGUILayout.EnumPopup("����� ȿ��", rh.restoreFX);
                    rh.restoreAmount = EditorGUILayout.IntField("ȸ����", rh.restoreAmount);
                    break;
                case Define.evtType.utill:
                    UtillHandler dh = (UtillHandler)evtData;
                    dh.utillType = (Define.utillType)EditorGUILayout.EnumPopup("� ���� ", dh.utillType);
                    EditorGUI.BeginChangeCheck();
                    len = EditorGUILayout.IntField("���� ī�� ��", dh.relatedCards.Length);
                    // �迭 ������ ũ�� Ȯ����, �迭 ũ�� ����
                    if (EditorGUI.EndChangeCheck())
                    { Array.Resize(ref dh.relatedCards, len); }

                    for (int i = 0; i < dh.relatedCards.Length; i++)
                    {
                        dh.relatedCards[i] = EditorGUILayout.IntField($"{i}�� ������ȣ : ", dh.relatedCards[i]);
                    }
                    dh.utillAmount = EditorGUILayout.IntField("��ο�� : ",dh.utillAmount);
                    break;
            }
            path = EditorGUILayout.TextField("path��� ", path);
            // ���̽����Ͽ� ���� �̺�Ʈ�� �߰��Ұ��� ����
            if (GUILayout.Button("�̺�Ʈ �߰�"))
            {
                addEvt(evtData);
            }
            if (GUILayout.Button("�ٸ� ���̽����� ��������"))
            {
                Clear();
            }
            if (GUILayout.Button("���� �̺�Ʈ ������ ��� �����"))
            { EvtDataClear(); }
        }

    }

    // ī�� �����Ϳ��� ���� �� ����� �̺�Ʈ ������ �ٽ� ���̽����Ϸ� ��ȯ
    public void addEvt(CardBaseEvtData data)
    {
        currData.evtDatas.Add(data);
        string json = JsonConvert.SerializeObject(currData, Formatting.Indented
            , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
        evtData = null; // �巡�� �� ����� ���� JSON ������ �޴� �κ�
        jsonFile = (TextAsset)EditorGUILayout.ObjectField("ī�嵥���� JSON ����: ", jsonFile, typeof(TextAsset), false); 
        currData = JsonConvert.DeserializeObject<CardData>(jsonFile.text
        , new JsonSerializerSettings
        {
            // ��ü �������� ��Ȯ��
            TypeNameHandling = TypeNameHandling.Auto
        }
        );
    }
    public void Clear()
    {
        jsonFile = null;
        currData = null;
        evtData = null;
    }
    public void EvtDataClear()
    {
        currData.evtDatas.Clear();
        string json = JsonConvert.SerializeObject(currData, Formatting.Indented
           , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
    }
}