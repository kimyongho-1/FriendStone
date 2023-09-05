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

    string path = "Assets/";// 배출경로

    TextAsset jsonFile; // 참조 제이슨파일 (카드데이터파일)
    CardData currData; // 편집할 데이터클래스

    CardBaseEvtData evtData; // 집어넣을, 카드가 사용할 이벤트데이터
    Define.evtType evtType; // 현재 선택한 이벤트타입 참조

    private void OnGUI()
    {
        if (jsonFile == null || currData == null)
        {
            // 드래그 앤 드롭을 통해 JSON 파일을 받는 부분
            jsonFile = (TextAsset)EditorGUILayout.ObjectField("카드데이터 JSON 파일: ", jsonFile, typeof(TextAsset), false);
            path = AssetDatabase.GetAssetPath(jsonFile);
            // 현재 CardData.cs을 제이슨으로 변환한 파일만 건드려야하기에 확인
            if (jsonFile != null)
            {
                try
                {
                    // 주의 : 직렬화, 역직렬화 모두 Auto설정해서 타입 설정 확인
                    // cardData.cs로 다시 변환하여 확인하기
                    currData = JsonConvert.DeserializeObject<CardData>(jsonFile.text
                        , new JsonSerializerSettings
                        {
                            // 객체 계층구조 명확히
                            TypeNameHandling = TypeNameHandling.Auto
                        }
                        );

                }
                catch
                {
                    EditorGUILayout.HelpBox("올바르지 않은 JSON 파일입니다. CardData 클래스로 생성된 파일을 사용해 주세요.", MessageType.Warning);
                    jsonFile = null;
                    currData = null;
                }
            }

        }

        // 편집할 카드데이터 제이슨파일과 편집할 카드데이터 하나라도 비었을시 강제 취소
        if (jsonFile == null || currData == null) { return; }

        // 제이슨파일이 CardData.cs인게 맞다면,
        // 이벤트 타입 설정창 실행
        evtType = (Define.evtType)EditorGUILayout.EnumPopup("어떤 이벤트를 ", evtType);

        // 위의 evtType을 변경시, 당장 편집중이던 evtData가 존재할시 바로 초기화
        if (evtData != null && evtData.type != evtType) { evtData = null; }

        // 현재 이벤트데이터 생성을 지금 시작했다면
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

        // evtData생성중이었다면 => 나머지 프로퍼티 설정 하도록
        else
        {
            evtData.type = evtType;
            evtData.when = (Define.evtWhen)EditorGUILayout.EnumPopup("언제 발동 : ", evtData.when);
            evtData.area = (Define.evtArea)EditorGUILayout.EnumPopup("영역 설정 : ", evtData.area); 
            switch (evtType)
            {
                case Define.evtType.buff:
                    BuffHandler bh = (BuffHandler)evtData;
                    bh.buffTargeting = (Define.buffTargeting)EditorGUILayout.EnumPopup("타겟팅 타입 ", bh.buffTargeting);
                    bh.buffType = (Define.buffType)EditorGUILayout.EnumPopup("어떤 버프 ", bh.buffType);
                    bh.buffExtraArea = (Define.buffExtraArea)EditorGUILayout.EnumPopup("추가 대상 ", bh.buffExtraArea);
                    bh.buffFX = (Define.buffFX)EditorGUILayout.EnumPopup("재생할 효과 ", bh.buffFX);
                    EditorGUI.BeginChangeCheck();
                    int len = EditorGUILayout.IntField("관련 카드 수", bh.relatedIds.Length);
                    // 배열 설정한 크기 확정시, 배열 크기 변경
                    if (EditorGUI.EndChangeCheck())
                    { Array.Resize(ref bh.relatedIds, len); }

                    for (int i = 0; i < bh.relatedIds.Length; i++)
                    {
                        bh.relatedIds[i] = EditorGUILayout.IntField($"{i}번 고유번호 : ", bh.relatedIds[i]);
                    }
                    bh.buffAtt = EditorGUILayout.IntField("공격증가량",bh.buffAtt);
                    bh.buffHp = EditorGUILayout.IntField("체력증가량", bh.buffHp);
                    bh.drawCount = EditorGUILayout.IntField("드로우 수", bh.drawCount);
                    break;
                case Define.evtType.attack:
                    AttackHandler ah = (AttackHandler)evtData;
                    ah.attTargeting = (Define.attTargeting)EditorGUILayout.EnumPopup("타겟팅 타입 ", ah.attTargeting);
                    ah.attType = (Define.attType)EditorGUILayout.EnumPopup("어떤 종류 ", ah.attType);
                    ah.attExtraArea = (Define.attExtraArea)EditorGUILayout.EnumPopup("추가 대상 ", ah.attExtraArea);
                    ah.attFX = (Define.attFX)EditorGUILayout.EnumPopup("발사할 효과 ", ah.attFX);
                    ah.attAmount = EditorGUILayout.IntField("공격량",ah.attAmount);
                    break;
                case Define.evtType.restore:
                    RestoreHandler rh = (RestoreHandler)evtData;
                    rh.restoreTargeting = (Define.restoreTargeting)EditorGUILayout.EnumPopup("타겟팅 타입", rh.restoreTargeting);
                    rh.restoreExtraArea = (Define.restoreExtraArea)EditorGUILayout.EnumPopup("추가 대상", rh.restoreExtraArea);
                    rh.restoreFX = (Define.restoreFX)EditorGUILayout.EnumPopup("재생할 효과", rh.restoreFX);
                    rh.restoreAmount = EditorGUILayout.IntField("회복량", rh.restoreAmount);
                    break;
                case Define.evtType.utill:
                    UtillHandler dh = (UtillHandler)evtData;
                    dh.utillType = (Define.utillType)EditorGUILayout.EnumPopup("어떤 종류 ", dh.utillType);
                    EditorGUI.BeginChangeCheck();
                    len = EditorGUILayout.IntField("관련 카드 수", dh.relatedCards.Length);
                    // 배열 설정한 크기 확정시, 배열 크기 변경
                    if (EditorGUI.EndChangeCheck())
                    { Array.Resize(ref dh.relatedCards, len); }

                    for (int i = 0; i < dh.relatedCards.Length; i++)
                    {
                        dh.relatedCards[i] = EditorGUILayout.IntField($"{i}번 고유번호 : ", dh.relatedCards[i]);
                    }
                    dh.utillAmount = EditorGUILayout.IntField("드로우수 : ",dh.utillAmount);
                    break;
            }
            path = EditorGUILayout.TextField("path경로 ", path);
            // 제이슨파일에 현재 이벤트를 추가할건지 여부
            if (GUILayout.Button("이벤트 추가"))
            {
                addEvt(evtData);
            }
            if (GUILayout.Button("다른 제이슨파일 가져오기"))
            {
                Clear();
            }
            if (GUILayout.Button("현재 이벤트 데이터 모두 지우기"))
            { EvtDataClear(); }
        }

    }

    // 카드 데이터에서 보관 및 사용할 이벤트 삽입후 다시 제이슨파일로 변환
    public void addEvt(CardBaseEvtData data)
    {
        currData.evtDatas.Add(data);
        string json = JsonConvert.SerializeObject(currData, Formatting.Indented
            , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
        evtData = null; // 드래그 앤 드롭을 통해 JSON 파일을 받는 부분
        jsonFile = (TextAsset)EditorGUILayout.ObjectField("카드데이터 JSON 파일: ", jsonFile, typeof(TextAsset), false); 
        currData = JsonConvert.DeserializeObject<CardData>(jsonFile.text
        , new JsonSerializerSettings
        {
            // 객체 계층구조 명확히
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