using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindEvtHolder : MonoBehaviour
{
    public CardHand prefab;
    public Collider2D left, center, right;
    public List<CardHand> list = new List<CardHand>();
    private void Awake()
    {
        GAME.Manager.IGM.FindEvt = this;

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
            ch.Init(ref card);
            ch.SetOrder(1000);
        }
        this.gameObject.SetActive(true);
    }

    public void ClickedCard(GameObject go)
    {
        // 클릭 오브젝트 이름을 통해 리스트의 인덱스 찾기
        string name = go.name;
        CardHand ch;
        if (name.StartsWith("L")) 
        {
          ch = list[0];
        }

        else if (name.StartsWith("C")) 
        {
            ch = list[1];
        }

        else 
        {
            ch = list[2];
        }

        // 발견 리스트에서 제거
        list.Remove(ch);
        // 식별자 초기화
        ch.PunId = (Photon.Pun.PhotonNetwork.IsMasterClient ? 1000 : 2000)
                    + GAME.Manager.IGM.Hand.punConsist++;

        // 정렬 위해서 유저의 핸드로 이동
        GAME.Manager.IGM.Hand.PlayerHand.Add(ch);
        ch.transform.SetParent(GAME.Manager.IGM.Hand.PlayerHandGO.transform);
        CurrSelected = true;

        //GAME.Manager.IGM.Hand.StopAllCoroutines();
        // 선택한 카드가 핸드에 자리잡힐떄까지 대기
        StartCoroutine(GAME.Manager.IGM.Hand.CardAllignment());
        // 닫기
        this.gameObject.SetActive(false) ;
    }
}
