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

    TextAsset jsonFile; // 기존 파일을 재참조하여 편집할경우
    CardData cardData;
    Define.cardType type;
    string path;

    public void Parse(TextAsset asset )
    {
        // 드래그 앤 드롭을 통해 JSON 파일을 받는 부분
        jsonFile = asset;
        path =  AssetDatabase.GetAssetPath(asset);
        // 현재 CardData.cs을 제이슨으로 변환한 파일만 건드려야하기에 확인
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
                EditorGUILayout.HelpBox("올바르지 않은 JSON 파일입니다. CardData 클래스로 생성된 파일을 사용해 주세요.", MessageType.Warning);
                jsonFile = null;
                cardData = null;
            }
        }

    }
    void OnGUI()
    {
        GUILayout.Label("카드 생성기", EditorStyles.boldLabel);

        // 중간에 제이슨파일 바뀔경우, 다시 처음부터 바뀐파일로 에디터그리기
        TextAsset file = (TextAsset)EditorGUILayout.ObjectField("기존 카드 편집(편집할것 드래그) ", jsonFile, typeof(TextAsset), false);
        if (file != jsonFile)
        {
            Parse(file);
            return;
        }

        #region 기존 카드데이터를 수정할떄
        if (jsonFile == null && cardData == null)
        {
            TextAsset a = (TextAsset)EditorGUILayout.ObjectField("기존 카드 편집(편집할것 드래그) ",
                jsonFile, typeof(TextAsset), false);
            Parse(a);
        }

        if (jsonFile != null)
        {
            // 먼저 어떤 카드를 만들지 선택
            type = (Define.cardType)EditorGUILayout.EnumPopup("카드 타입 : ", cardData.cardType);
            // 선택한 카드 타입에 맞게 추가 프로퍼티 보여주기
            switch (type)
            {
                case Define.cardType.minion:
                    MinionCardData mc = null;
                    // 카드타입 (주문,미니언,무기등등)이 바뀐다면 클래스 자체를 바꾸어야하는 상황
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
                    mc.att = EditorGUILayout.IntField("공격력 : ", mc.att);
                    mc.hp = EditorGUILayout.IntField("체력 : ", mc.hp);
                    mc.isTaunt = EditorGUILayout.Toggle("도발 유닛 : ", mc.isTaunt);
                    mc.isCharge = EditorGUILayout.Toggle("돌진 유닛 : ", mc.isCharge);
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
                    wc.att = EditorGUILayout.IntField("공격력 : ", wc.att);
                    wc.durability = EditorGUILayout.IntField("내구도 : ", wc.durability);
                    break;
            }
            // 소유 직업 설정
            cardData.cardClass = (Define.classType)EditorGUILayout.EnumPopup("직업카드 : ", cardData.cardClass);
            cardData.cardType = type;
            // 공통 변수 목록
            cardData.cardRarity = (Define.cardRarity)EditorGUILayout.EnumPopup("희귀도 : ", cardData.cardRarity);
            cardData.cardName = EditorGUILayout.TextField("Card Name: ", cardData.cardName);
            cardData.cardDescription = EditorGUILayout.TextField("설명 : ", cardData.cardDescription);
            cardData.cardIdNum = EditorGUILayout.IntField("고유ID: ", cardData.cardIdNum);
            cardData.cost = EditorGUILayout.IntField("Cost: ", cardData.cost);
            cardData.associatedHandler = EditorGUILayout.TextField("스크립트명 : ", cardData.associatedHandler);
            path = EditorGUILayout.TextField("Path ", path);

            // 버튼 감지 및 제이슨파일로 생성시작
            if (GUILayout.Button("Json 편집시작"))
            {
                SaveCardData();
            }
        }

        #endregion

        #region 새로운 카드데이터 생성할떄
        else
        { 
            // 데이터 생성 초기
            if (cardData == null)
            {
                 // 먼저 어떤 카드를 만들지 선택
                 type = (Define.cardType)EditorGUILayout.EnumPopup("카드 타입 : ", type);

                 // 만들기 시작
                 if (GUILayout.Button("만들기"))
                 {
                     // 선택한 카드 타입에 맞게 추가 프로퍼티 보여주기
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

            // 데이터 생성 시작
            else
            {
                // 카드의 타입 설정 확인 및 공통 변수들 설정 시작
                cardData.cardType = type;
                // 소유 직업 설정
                cardData.cardClass = (Define.classType)EditorGUILayout.EnumPopup("직업카드 : ", cardData.cardClass);

                // 공통 변수 목록
                cardData.cardName = EditorGUILayout.TextField("Card Name: ", cardData.cardName);
                cardData.cardRarity = (Define.cardRarity)EditorGUILayout.EnumPopup("희귀도 : ", cardData.cardRarity);
                cardData.cardDescription = EditorGUILayout.TextField("설명 : ", cardData.cardDescription);
                cardData.cardIdNum = EditorGUILayout.IntField("고유ID: ", cardData.cardIdNum);
                cardData.cost = EditorGUILayout.IntField("Cost: ", cardData.cost);
                cardData.associatedHandler = EditorGUILayout.TextField("스크립트명 : ", cardData.associatedHandler);
                // 선택한 카드 타입에 맞게 추가 프로퍼티 보여주기
                switch (type)
                {
                    case Define.cardType.minion:
                        if (cardData == null)
                        { cardData = new MinionCardData(); }
                        MinionCardData mc = (MinionCardData)cardData;
                        mc.att = EditorGUILayout.IntField("공격력 : ", mc.att);
                        mc.hp = EditorGUILayout.IntField("체력 : ", mc.hp);
                        mc.isTaunt = EditorGUILayout.Toggle("도발 유닛 : ",mc.isTaunt);
                        mc.isCharge = EditorGUILayout.Toggle("돌진 유닛 : ", mc.isCharge);
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
                        wc.att = EditorGUILayout.IntField("공격력 : ", wc.att);
                        wc.durability = EditorGUILayout.IntField("내구도 : ", wc.durability);
                        break;
                }

                path = EditorGUILayout.TextField("Path ", path);
                // 버튼 감지 및 제이슨파일로 생성시작
                if (GUILayout.Button("Json 생성시작"))
                {
                    SaveCardData();
                }
            }
        }

        #endregion


        // 다시 초기화면으로 초기화버튼
        if (GUILayout.Button("다시 처음부터"))
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
