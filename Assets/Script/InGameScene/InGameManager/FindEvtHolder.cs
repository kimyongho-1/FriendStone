using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class FindEvtHolder : MonoBehaviour
{
    public CardHand prefab;
    public Collider2D left, center, right;
    public List<CardHand> list = new List<CardHand>();
    private void Awake()
    {
        GAME.IGM.FindEvt = this;

        // 발견 이벤트시, 카드 선택을 확인하는 클릭 이벤트 연결
        GAME.Manager.UM.BindEvent(left.gameObject, ClickedCard, Define.Mouse.ClickL, Define.Sound.Click);
        GAME.Manager.UM.BindEvent(center.gameObject, ClickedCard, Define.Mouse.ClickL, Define.Sound.Click);
        GAME.Manager.UM.BindEvent(right.gameObject, ClickedCard, Define.Mouse.ClickL, Define.Sound.Click);

        gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        int start = Mathf.Clamp(list.Count,0,3);
        for (int i = start; i < 3; i++) 
        {
            // 인게임 카드 프리팹 생성
            CardHand ch = GameObject.Instantiate(prefab, this.transform);
            ch.transform.localScale = Vector3.one * 0.75f;
            ch.transform.localPosition = new Vector3( (i-1) * 4f, 1.25f, -1);
            list.Add(ch);
        }
    }

    public bool CurrSelected = false;

    // 발견 이벤트 시작시, 데이터 준비
    public void ReadyFindEvt(int[] puns)
    {
        CurrSelected = false;
        // 발견 카드풀 데이터 초기화
        for (int i = 0; i < 3; i++)
        {
            // ShowEnemyFindEvt함수 실행후를 대비해서 재 설정
            list[i].cardImage.gameObject.SetActive(true);
            list[i].TMPgo.gameObject.SetActive(true);
            list[i].cardBackGround.sprite = GAME.Manager.RM.GetCardSprite(true);
            list[i].transform.localScale = Vector3.one * 0.3f;

            // 리소스 매니저의 경로를 반환 받는 딕셔너리 통해 카드타입과 카드데이터 찾기
            Define.cardType type = GAME.Manager.RM.PathFinder.Dic[puns[i]].type;
            string jsonFile = GAME.Manager.RM.PathFinder.Dic[puns[i]].GetJson();

            CardData card = null;
            // 확인된 카드타입으로, 실제 카드타입으로 클래스화
            switch (type)
            {
                case Define.cardType.minion:
                    card = JsonConvert.DeserializeObject<MinionCardData>
                (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    break;
                case Define.cardType.spell:
                    card = JsonConvert.DeserializeObject<SpellCardData>
                (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    break;
                case Define.cardType.weapon:
                    card = JsonConvert.DeserializeObject<WeaponCardData>
                (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    break;
                default: break;
            }

            CardHand ch = list[i];
            
            ch.Init(card);
            ch.PunId = GAME.IGM.Hand.CreatePunNumber();
            ch.SetOrder(1000);
        }
        // 내가 상대의 카드 상호작용 하면 안되기에 레이 끄기
        left.enabled = right.enabled = center.enabled = true;
        this.gameObject.SetActive(true);
    }

    public void ClickedCard(GameObject go)
    {
        // 클릭 오브젝트 이름을 통해 리스트의 인덱스 찾기
        string name = go.name;
        CardHand ch;
        int findIdx = 0;
        if (name.StartsWith("L")) 
        {
            ch = list[0];
            findIdx = 0;
        }

        else if (name.StartsWith("C")) 
        {
            ch = list[1];
            findIdx = 1;
        }

        else 
        {
            ch = list[2];
            findIdx = 2;
        }

        // 발견 리스트에서 제거
        list.Remove(ch);
        // 식별자 초기화
        ch.PunId = GAME.IGM.Hand.CreatePunNumber();

        // 정렬 위해서 유저의 핸드로 이동
        GAME.IGM.Hand.PlayerHand.Add(ch);
        ch.transform.SetParent(GAME.IGM.Hand.PlayerHandGO.transform);
        CurrSelected = true;

        // 선택한 카드가 핸드에 자리잡힐떄까지 대기
        GAME.IGM.StartCoroutine(GAME.IGM.Hand.CardAllignment());

        // 상대에게 내 발견 이벤트 결과를 전달하기 [ 내가 몇번쨰 카드를 선택했는지 + 그 카드의 펀넘버가 무엇인지 ]
        GAME.IGM.Packet.SendResultFindEvt( findIdx , ch.PunId);

        // 닫기
        this.gameObject.SetActive(false) ;
    }   
    
    // 상대가 발견 코루틴을 시작할떄, 내 화면에도 똑같이 표기해주기
    public IEnumerator ShowEnemyFindEvt()
    {
        CurrSelected = false;
        for (int i = 0; i < 3; i++)
        {
            // 모두 카드 뒷면으로 표시
            list[i].cardBackGround.sprite = GAME.Manager.RM.GetCardSprite(false);
            list[i].cardBackGround.sortingOrder = 1000;
            // 뒷면만 나오면 되기에, 나머지 꺼주기
            list[i].cardImage.gameObject.SetActive(false);
            list[i].TMPgo.gameObject.SetActive(false);
            list[i].transform.localScale = Vector3.one * 0.4f;
        }
        // 내가 상대의 카드 상호작용 하면 안되기에 레이 끄기
        left.enabled = right.enabled = center.enabled = false;
        this.gameObject.SetActive(true);
        yield return null;
    }

    // 상대가 발견이벤트를 선택하여 끝날떄, 내 화면에도 그 카드를 고르는 연출 코루틴
    public IEnumerator ShowEnemyFindEvtResult(int idx, int punID)
    {
        // 카드 찾기 + 식별자 초기화
        CardHand ch = list[idx];
        ch.PunId = punID;
        ch.IsMine = false;
        ch.originScale = (ch.IsMine) ? Vector3.one * 0.3f : new Vector3(0.16f, 0.17f, 0.3f);
        // 내 리스트에서 제거
        list.Remove(ch);
        // 정렬 위해서 적의 핸드로 이동
        GAME.IGM.Hand.EnemyHand.Add(ch);
        ch.transform.SetParent(GAME.IGM.Hand.EnemyHandGO.transform);

        // 해당 카드 적 핸드로 들어가는 연출 해주기 
        GAME.IGM.StartCoroutine(GAME.IGM.Hand.CardAllignment());
        yield return null;

        // 발견 이벤트 창 끄기
        this.gameObject.SetActive(false);
    }
}
