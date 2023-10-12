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
        // 윈도우의 최소 크기를 설정
        ew.minSize = new Vector2(900, 600);

    }

    // 에디터창을 닫을때마다 호출
    public void OnDisable()
    {
        // 에디터창 닫을떄, 현재 참조중인 제이슨파일있을시 바뀐 사항 강제저장
        if (cardData != null && jsonFile != null)
        {
            string json = JsonConvert.SerializeObject(cardData, Formatting.Indented
                           , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

            // 기존 제이슨파일 바뀐 내역 상태로 저장
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }
        if (heroData != null && heroJsonFile != null)
        {
            string json = JsonConvert.SerializeObject(heroData, Formatting.Indented
                           , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

            // 기존 제이슨파일 바뀐 내역 상태로 저장
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }
    }

    TextAsset jsonFile; // 기존 파일을 재참조하여 편집할경우
    CardData cardData; // 참조중인 제이슨파일을 다시 클래스화하여 사용할떄 참조중인 변수
    Define.cardType type;
    string path;

    int selectedIdx; // 현재 참조 및 편집중인 이벤트데이터 클래스 인덱스

    private int tabIndex = 0;
    private string[] tabs = { "카드 데이타 생성편집기", "영웅 데이타 생성편집기" };

    void OnGUI()
    {
        int clickedIDX  = tabIndex;
        clickedIDX = GUILayout.Toolbar(clickedIDX, tabs);

        if (clickedIDX != tabIndex)
        {
            tabIndex = clickedIDX;
            if (tabIndex == 0) 
            {
                if (heroData != null && heroJsonFile != null)
                {
                    string json = JsonConvert.SerializeObject(heroData, Formatting.Indented
                                   , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

                    // 기존 제이슨파일 바뀐 내역 상태로 저장
                    File.WriteAllText(path, json);
                    AssetDatabase.Refresh();
                    heroData = null; heroJsonFile = null;
                }
            }
            else if (tabIndex == 1)
            {
                // 에디터창 바뀔떄, 현재 참조중인 제이슨파일있을시 바뀐 사항 강제저장
                if (cardData != null && jsonFile != null)
                {
                    string json = JsonConvert.SerializeObject(cardData, Formatting.Indented
                                   , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

                    // 기존 제이슨파일 바뀐 내역 상태로 저장
                    File.WriteAllText(path, json);
                    AssetDatabase.Refresh();
                    cardData = null; jsonFile = null;
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        switch (tabIndex)
        {
            case 0:
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(position.width / 2));
                GUILayout.Label("카드 데이터 편집", EditorStyles.boldLabel);
                CardDataEditor();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(position.width / 2));
                GUILayout.Label("이벤트 데이터 편집", EditorStyles.boldLabel);
                EvtDataEditor();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                break;
            case 1:
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(position.width / 2));
                GUILayout.Label("영웅 데이터 편집", EditorStyles.boldLabel);
                HeroDataEditor();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(position.width / 2));
                GUILayout.Label("영웅 대사 편집", EditorStyles.boldLabel);
                HeroSpeechEditor();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                break;
        }
      
    }
    void EvtDataEditor()
    {
        #region CODE
        // 현재 참조된 데이터 없으면 안되기에 return
        if (jsonFile == null || cardData == null) { selectedIdx = -1; return; }

        // 이벤트 추가 버튼
        if (GUILayout.Button("이벤트 추가"))
        {
            // 상세한 이벤트 내부는 모르겠지만, 새 데이타클래스 생성
            cardData.evtDatas.Add(new BuffHandler());
            // 초기화
            selectedIdx = -1;
        }

        if (selectedIdx == -1)
        {
            // 이벤트 리스트 표시
            for (int i = 0; i < cardData.evtDatas.Count; i++)
            {
                GUILayout.BeginHorizontal();
                // 해당 이벤트 클릭시 편집 버튼
                if (GUILayout.Button($"{i}번 : {cardData.evtDatas[i].type} [클릭시 편집]"))
                {
                    selectedIdx = i;
                }

                // 해당 이벤트 삭제 버튼
                if (GUILayout.Button($"{i}번 {cardData.evtDatas[i].type} 이벤트 삭제"))
                {
                    selectedIdx = -1;
                    cardData.evtDatas.RemoveAt(i);
                    GUILayout.EndHorizontal();
                    return;
                }
                GUILayout.EndHorizontal();
            }

        }


        // 선택된 이벤트의 상세 정보 표시
        else
        {
            if (GUILayout.Button("뒤로 가기"))
            {
                selectedIdx = -1;
                return;
            }

            // 현재 이벤트의 타입이 바뀌었는지 확인
            Define.evtType evtType = (Define.evtType)EditorGUILayout.EnumPopup("어떤 이벤트를 ", cardData.evtDatas[selectedIdx].type);

            // 이벤트 타입 자체가 바뀌었다면 해당 이벤트 클래스를 새로 만들어 기존것과 바꾸기
            switch (evtType)
            {
                case Define.evtType.buff:
                    BuffHandler bh = null;
                    if (cardData.evtDatas[selectedIdx].type is evtType.buff)
                    {
                        // 이벤트 타입이 바뀌지 않았다면 그대로 참조 시작
                        bh = (BuffHandler)cardData.evtDatas[selectedIdx];
                    }
                    else
                    {
                        // 기존 이벤트타입과 다르면 새로 생성하여 기존 데이터 이용하여 생성시작
                        bh = new BuffHandler();
                    }
                    #region BuffHandler 속성 그리기
                    if (bh.targeting == evtTargeting.Auto)
                    { bh.buffAutoMode = (Define.buffAutoMode)EditorGUILayout.EnumPopup("타겟팅 타입 ", bh.buffAutoMode); }
                    bh.buffType = (Define.buffType)EditorGUILayout.EnumPopup("어떤 버프 ", bh.buffType);
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
                    bh.buffAtt = EditorGUILayout.IntField("공격증가량", bh.buffAtt);
                    bh.buffHp = EditorGUILayout.IntField("체력증가량", bh.buffHp);
                    bh.costCount = EditorGUILayout.IntField("드로우 수", bh.costCount);
                    #endregion
                    SetProperty(bh);
                    // 현재 참조중인 데이터를 바뀐 타입클래스로 초기화
                    cardData.evtDatas[selectedIdx] = bh;
                    break;
                case Define.evtType.attack:
                    AttackHandler ah = null;
                    if (cardData.evtDatas[selectedIdx].type is evtType.attack)
                    {
                        // 이벤트 타입이 바뀌지 않았다면 그대로 참조 시작
                        ah = (AttackHandler)cardData.evtDatas[selectedIdx];
                    }
                    else
                    {
                        // 기존 이벤트타입과 다르면 새로 생성하여 기존 데이터 이용하여 생성시작
                        ah = new AttackHandler();
                    }
                    #region AttackHandler 속성 그리기
                    ah.attType = (Define.attType)EditorGUILayout.EnumPopup("어떤 종류 ", ah.attType);
                    ah.attFX = (Define.attFX)EditorGUILayout.EnumPopup("발사할 효과 ", ah.attFX);
                    ah.attAmount = EditorGUILayout.IntField("공격량", ah.attAmount);
                    #endregion
                    SetProperty(ah);
                    cardData.evtDatas[selectedIdx] = ah;
                    break;
                case Define.evtType.restore:
                    RestoreHandler rh = null;
                    if (cardData.evtDatas[selectedIdx].type is evtType.restore)
                    {
                        // 이벤트 타입이 바뀌지 않았다면 그대로 참조 시작
                        rh = (RestoreHandler)cardData.evtDatas[selectedIdx];
                    }
                    else
                    {
                        // 기존 이벤트타입과 다르면 새로 생성하여 기존 데이터 이용하여 생성시작
                        rh = new RestoreHandler();
                    }
                    #region RestoreHandler 속성 그리기
                    if (rh.targeting == evtTargeting.Auto)
                    { rh.restoreAutoMode = (Define.restoreAutoMode)EditorGUILayout.EnumPopup("자동 타입", rh.restoreAutoMode ); }
                    rh.restoreFX = (Define.restoreFX)EditorGUILayout.EnumPopup("재생할 효과", rh.restoreFX);
                    rh.restoreAmount = EditorGUILayout.IntField("회복량", rh.restoreAmount);
                    #endregion
                    SetProperty(rh);
                    cardData.evtDatas[selectedIdx] = rh;
                    break;
                case Define.evtType.utill:
                    UtillHandler uh = null;
                    if (cardData.evtDatas[selectedIdx].type is evtType.utill)
                    {
                        // 이벤트 타입이 바뀌지 않았다면 그대로 참조 시작
                        uh = (UtillHandler)cardData.evtDatas[selectedIdx];
                    }
                    else
                    {
                        // 기존 이벤트타입과 다르면 새로 생성하여 기존 데이터 이용하여 생성시작
                        uh = new UtillHandler();
                    }
                    #region UtillHandler 속성 그리기
                    uh.utillType = (Define.utillType)EditorGUILayout.EnumPopup("어떤 종류 ", uh.utillType);

                    // 단순 드로우 이벤트라면 , 몇장 뽑을지만 에디터에 그리기
                    if (uh.utillType == Define.utillType.draw)
                    {
                        uh.utillAmount = EditorGUILayout.IntField("드로우수 : ", uh.utillAmount);
                    }

                    // 카드풀에서 발견이벤트 | 관련카드 획득 이벤트
                    else
                    {
                        EditorGUI.BeginChangeCheck();

                        // 발견이벤트의 경우 항상 3장으로 고정, 그외 획득이벤트는 내가 설정하는 양으로 카드배열 설정
                        uh.utillAmount = (uh.utillType == Define.utillType.find) ? 3 : EditorGUILayout.IntField("획득할 카드의 범위", uh.utillAmount);

                        // 배열 설정한 크기 확정시, 배열 크기 변경
                        if (EditorGUI.EndChangeCheck())
                        { Array.Resize(ref uh.relatedCards, uh.utillAmount); }

                        for (int i = 0; i < uh.relatedCards.Length; i++)
                        {
                            uh.relatedCards[i] = EditorGUILayout.IntField($"{i}번 고유번호 : ", uh.relatedCards[i]);
                        }
                    }

                    #endregion
                    SetProperty(uh);
                    cardData.evtDatas[selectedIdx] = uh;
                    break;
            }

            // 공통 변수 전달 (CardBaseEvtData.cs 최상위 부모)
            void SetProperty(CardBaseEvtData evtData)
            {
                evtData.type = evtType; // 최상단 오브젝트 필드 변수
                evtData.when = (Define.evtWhen)EditorGUILayout.EnumPopup("언제 발동 : ", cardData.evtDatas[selectedIdx].when);
                evtData.area = (Define.evtArea)EditorGUILayout.EnumPopup("타겟 진영 : ", cardData.evtDatas[selectedIdx].area);
                evtData.faction = (Define.evtFaction)EditorGUILayout.EnumPopup("타겟타입 : ", cardData.evtDatas[selectedIdx].faction);
                evtData.targeting = (Define.evtTargeting)EditorGUILayout.EnumPopup("타겟팅방식 : ", cardData.evtDatas[selectedIdx].targeting);
            }

            // 바뀐 값 또는 참조로 실제 데이터 변경
            cardData.evtDatas[selectedIdx] = cardData.evtDatas[selectedIdx];
        }
        #endregion
    }
    void CardDataEditor()
    {
        #region CODE
        // 현재 편집중이던 아니든, 새 파일 생성시 클릭
        if (GUILayout.Button("새 데이타 파일 만들기"))
        {
            // 새 데이터 생성 및 저장 => 디폴트 미니언카드로 선택
            cardData = new MinionCardData();
            cardData.cardName = "새로운 카드 데이터";
            string newFile = JsonConvert.SerializeObject(cardData, Formatting.Indented
                     , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            path = $"Assets/Resources/{cardData.cardName}.json";
            File.WriteAllText(path, newFile);
            jsonFile = new TextAsset(newFile);
            AssetDatabase.Refresh();
        }

        // 현재 참조(작업)중인 데이터 파일 보여주는 오브젝트 필드 생성
        TextAsset file = (TextAsset)EditorGUILayout.
            ObjectField("기존 카드 편집(편집할것 드래그) ", jsonFile, typeof(TextAsset), false);

        // 현재 작업중인, 참조 하는 데이터 없으면 에디터 더이상 그리지 말기
        if (file == null)
        { return; }

        // 참조파일이 변경되었다면, 현재 에디터창을 변경된 파일로 바꾸기 위해 작업
        if (file != jsonFile)
        {
            // 참조파일 변경시 기존 참조값이 바뀐경우를 대비해,
            // 먼저 기존 참조파일을 저장하고 변경작업 시작
            if (jsonFile != null && cardData != null)
            {
                string json = JsonConvert.SerializeObject(cardData, Formatting.Indented
                     , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                // 기존 제이슨파일 바뀐 내역 상태로 저장
                File.WriteAllText(path, json);
                AssetDatabase.Refresh();
                cardData = null; path = null;
            }


            // 바뀐 파일로 참조 변경 및 클래스화 다시 시작
            jsonFile = file;

            // 현재 파일의 경로로 초기화
            path = AssetDatabase.GetAssetPath(file);

            // 제이슨 파일 클래스화 및 변수 cardData 초기화 (실패시 cardData가 null참조)
            JsonToClass();

            // 위함수에서 어떠한 이유로 클래스화가 실패할시 다시 처음부터 진행하도록 유도
            if (cardData == null || jsonFile == null)
            {
                cardData = null;
                jsonFile = null;
                path = null;
                return;
            }
        }

        // 현재 참조중인 JsonFile이 있을시
        // 그것을 에디터창에 그리기
        DrawCurrentCardData();

        // 클릭시 , Path경로로 이름포함하여 저장
        if (GUILayout.Button("현재 저장"))
        {
            string json = JsonConvert.SerializeObject(cardData, Formatting.Indented
                   , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            // 기존 제이슨파일 바뀐 내역 상태로 저장
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }

        #endregion
    }

    // 제이슨파일을 클래스화 
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
            EditorGUILayout.HelpBox("올바르지 않은 JSON 파일입니다. CardData 클래스로 생성된 파일을 사용해 주세요.", MessageType.Warning);
            jsonFile = null;
            cardData = null;
        }
    }

    // 현재 참조중인 제이슨파일을 클래스화 한 변수 cardData가 참조하는것을 에디터창에 그리기
    public void DrawCurrentCardData()
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
                }
                mc.att = EditorGUILayout.IntField("공격력 : ", mc.att);
                mc.hp = EditorGUILayout.IntField("체력 : ", mc.hp);
                mc.isTaunt = EditorGUILayout.Toggle("도발 유닛 : ", mc.isTaunt);
                mc.isCharge = EditorGUILayout.Toggle("돌진 유닛 : ", mc.isCharge);
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
                wc.att = EditorGUILayout.IntField("공격력 : ", wc.att);
                wc.durability = EditorGUILayout.IntField("내구도 : ", wc.durability);
                SetProperty(wc);
                cardData = wc;
                break;
        }

        // 타입 자체가 바뀌면 클래스가 바뀌기에, 기존 공통 CardData.cs 내부 변수들 적용
        void SetProperty(CardData cd)
        {
            // 소유 직업 설정
            cd.cardClass = (Define.classType)EditorGUILayout.EnumPopup("직업카드 : ", cardData.cardClass);
            cd.cardType = type;
            // 공통 변수 목록
            cd.cardRarity = (Define.cardRarity)EditorGUILayout.EnumPopup("희귀도 : ", cardData.cardRarity);
            cd.cardName = EditorGUILayout.TextField("Card Name: ", cardData.cardName);
            cd.cardDescription = EditorGUILayout.TextField("설명 : ", cardData.cardDescription);
            cd.cardIdNum = EditorGUILayout.IntField("고유ID: ", cardData.cardIdNum);
            cd.cost = EditorGUILayout.IntField("Cost: ", cardData.cost);
           
            cd.evtDatas = cardData.evtDatas;
        }

        path = EditorGUILayout.TextField("Path ", path);
    }

    HeroData heroData;
    TextAsset heroJsonFile;
    void HeroDataEditor()
    {
        if (GUILayout.Button("새 영웅 파일 만들기"))
        {
            // 새 데이터 생성 및 저장 => 디폴트 HJData로 선택
            heroData = new HeroData();
            string newFile = JsonConvert.SerializeObject(heroData, Formatting.Indented
                     , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            path = $"Assets/Resources/Data/HeroData/{"신규"}.json";
            File.WriteAllText(path, newFile);
            heroJsonFile = new TextAsset(newFile);
            AssetDatabase.Refresh();
        }

        // 현재 참조(작업)중인 데이터 파일 보여주는 오브젝트 필드 생성
        TextAsset file = (TextAsset)EditorGUILayout.
            ObjectField("기존 영웅데이터 편집(편집할것 드래그) ", heroJsonFile, typeof(TextAsset), false);

        // 현재 작업중인, 참조 하는 데이터 없으면 에디터 더이상 그리지 말기
        if (file == null)
        { return; }

        // 참조파일이 변경되었다면, 현재 에디터창을 변경된 파일로 바꾸기 위해 작업
        if (file != heroJsonFile)
        {
            // 참조파일 변경시 기존 참조값이 바뀐경우를 대비해,
            // 먼저 기존 참조파일을 저장하고 변경작업 시작
            if (heroJsonFile != null && heroData != null)
            {
                string json = JsonConvert.SerializeObject(heroData, Formatting.Indented
                     , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                // 기존 제이슨파일 바뀐 내역 상태로 저장
                File.WriteAllText(path, json);
                AssetDatabase.Refresh();
                heroData = null; path = null;
            }


            // 바뀐 파일로 참조 변경 및 클래스화 다시 시작
            heroJsonFile = file;
            // 현재 파일의 경로로 초기화
            path = AssetDatabase.GetAssetPath(file);

            #region HeroData 제이슨파일 클래스화
            // 제이슨 파일 클래스화 및 변수 heroData 초기화 (실패시 heroData가 null참조)
            try
            {
                HeroData hero = JsonConvert.DeserializeObject<HeroData>
                    (heroJsonFile.text, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                heroData = hero;
            }
            catch
            {
                EditorGUILayout.HelpBox("올바르지 않은 JSON 파일입니다. CardData 클래스로 생성된 파일을 사용해 주세요.", MessageType.Warning);
                heroJsonFile = null;
                heroData = null;
            }
            #endregion

            // 위함수에서 어떠한 이유로 클래스화가 실패할시 다시 처음부터 진행하도록 유도
            if (heroData == null || heroJsonFile == null)
            {
                heroData = null;
                heroJsonFile = null;
                 path = null;
                return;
            }
        }

        #region 프로퍼티 드로우어
        heroData.classType = (Define.classType)EditorGUILayout.EnumPopup("직업 : ", heroData.classType);
        heroData.skillName = EditorGUILayout.TextField("스킬이름: ", heroData.skillName);
        heroData.skillDesc = EditorGUILayout.TextField("스킬설명: ", heroData.skillDesc);
        GUILayoutOption[] options = { GUILayout.Width(200), GUILayout.Height(50) };
       // heroData.skillImagePath = EditorGUILayout.TextField("스킬아이콘경로",heroData.skillImagePath);
       // heroData.heroImagePath = EditorGUILayout.TextField("영웅이미지", heroData.heroImagePath);
        heroData.skillCost = EditorGUILayout.IntField("스킬 비용 : ", heroData.skillCost);
        string descSkillAmount = null;
        switch (heroData.classType)
        {
            case classType.HJ:
                descSkillAmount = "피해량 : "; break;
            case classType.HZ:
                descSkillAmount = "드로우량 : "; break;
            default:
                descSkillAmount = "치료량 : "; break;
        }
        heroData.skillAmount = EditorGUILayout.IntField(descSkillAmount, heroData.skillAmount);
        path = EditorGUILayout.TextField("Path ", path);
        #endregion

        // 클릭시 , Path경로로 이름포함하여 저장
        if (GUILayout.Button("현재 저장"))
        {
            string json = JsonConvert.SerializeObject(heroData, Formatting.Indented
                   , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            // 기존 제이슨파일 바뀐 내역 상태로 저장
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }
    }

    void HeroSpeechEditor()
    {
        if (heroData == null || heroJsonFile == null)
        { return; }
        GUILayout.Label("감정표현", EditorStyles.boldLabel);

        heroData.outSpeech[Define.Emotion.Hello] = EditorGUILayout.TextField("인사: ", heroData.outSpeech[Define.Emotion.Hello]);
        heroData.outSpeech[Define.Emotion.WellPlayed] = EditorGUILayout.TextField("칭찬: ", heroData.outSpeech[Define.Emotion.WellPlayed]);
        heroData.outSpeech[Define.Emotion.Thanks] = EditorGUILayout.TextField("감사: ", heroData.outSpeech[Define.Emotion.Thanks]);
        heroData.outSpeech[Define.Emotion.Wow] = EditorGUILayout.TextField("감탄: ", heroData.outSpeech[Define.Emotion.Wow]);
        heroData.outSpeech[Define.Emotion.Oops] = EditorGUILayout.TextField("이런: ", heroData.outSpeech[Define.Emotion.Oops]);
        heroData.outSpeech[Define.Emotion.Threat] = EditorGUILayout.TextField("위협: ", heroData.outSpeech[Define.Emotion.Threat]);

        EditorGUILayout.Space();

        GUILayout.Label("상호작용대사", EditorStyles.boldLabel);
        heroData.outSpeech[Define.Emotion.AlreadyAttacked] = EditorGUILayout.TextField("이미 공격한 미니언 클릭시: ", heroData.outSpeech[Define.Emotion.AlreadyAttacked]);
        heroData.outSpeech[Define.Emotion.CantAttack] = EditorGUILayout.TextField("공격불가 상태 미니언 클릭시 : ", heroData.outSpeech[Define.Emotion.CantAttack]);
        heroData.outSpeech[Define.Emotion.NotReady] = EditorGUILayout.TextField("지금 소환된 하수인 공격클릭 : ", heroData.outSpeech[Define.Emotion.NotReady]);
        heroData.outSpeech[Define.Emotion.ThereTaunt] = EditorGUILayout.TextField("도발 하수인 알릴떄 : ", heroData.outSpeech[Define.Emotion.ThereTaunt]);
        heroData.outSpeech[Define.Emotion.TimeLimitStart] = EditorGUILayout.TextField("시간제한 시작 : ", heroData.outSpeech[Define.Emotion.TimeLimitStart]);
        heroData.outSpeech[Define.Emotion.TimeLess] = EditorGUILayout.TextField("시간마감 임박 : ", heroData.outSpeech[Define.Emotion.TimeLess]);
        heroData.outSpeech[Define.Emotion.AlreadtHeroAttacked] = EditorGUILayout.TextField("영웅이 공격을 한 상황 : ", heroData.outSpeech[Define.Emotion.AlreadtHeroAttacked]);
        heroData.outSpeech[Define.Emotion.HandOverFlow] = EditorGUILayout.TextField("10장에서 드로우시 : ", heroData.outSpeech[Define.Emotion.HandOverFlow]);
    }

}