using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SpawnManager : MonoBehaviour
{
    AudioSource audioPlayer;
    public Transform Players, Enemies;
    public CustomList enemyMinions = new CustomList();
    public CustomList playerMinions = new CustomList();
    public List<Vector3> spawnPoint = new List<Vector3>();
    public CardField prefab;
    
    public int idx = 0; // 소환될 미니언의 인덱싱 위치
    BoxCollider2D SpawnArea;
    private void Awake()
    {
        audioPlayer = GetComponent<AudioSource>();
        GAME.IGM.Spawn = this;
        SpawnArea = transform.GetComponentInChildren<BoxCollider2D>();

        spawnPoint.Add(new Vector3(0.5f, 0.5f, -0.1f));
    }

    // 미니언이 소환되거나 죽을떄 , 스폰포인트 재계산 및 잔존 미니언들 위치 재정렬
    public IEnumerator CalcSpawnPoint()
    {
        // 최대 미니언의 갯수가 7이면 더 이상 공간 만들 필요 X
        if (playerMinions.Count == 7) {  yield break; }

        // 현재 미니언이 없으면 언제나 고정된 위치로 다음 미니언 소환 위치 결정
        if (playerMinions.Count == 0)
        {
            spawnPoint.Clear();
            spawnPoint.Add(new Vector3(0.5f, 0.5f, -0.1f));
            yield break;
        }
        StopAllCoroutines();
        StartCoroutine(AlingTest(true));

        spawnPoint.Clear();
        // 스폰포인트 재정렬
        for (int i = 0; i < playerMinions.Count + 1; i++)
        {
            spawnPoint.Add(default);
            // i - (count - 1) / 2 는 중심을 기준으로 -3, -2, -1, 0, 1, 2, 3 같은 변환을 유도
            float x = 0.5f + 1.25f * (i - (playerMinions.Count) / 2.0f);
            spawnPoint[i] = new Vector3(x, 0.5f, -0.1f);
        }

        // 현재 드래깅커서가 필드범위에 없는것으로 초기화
        idx = -1000;

    }

    // 현재 유저의 미니언핸드카드가, 필드 범위에 들어왔는지 확인하는 함수
    public bool CheckInBox(Vector3 worldPos)
    {
        float left = -4.68f;
        float right = 5.32f;
        float top = 1.14f;
        float bottom = -0.26f;
        return (worldPos.x > left && worldPos.x < right
            && worldPos.y > bottom && worldPos.y < top);
    }

    // 유저가 핸드카드중 미니언 카드를 드래그하여 필드위에서
    // 움직일떄, 미니언들 간의 자리 사이를 미리 움직여서 보여주는 함수
    public void MinionAlignment(CardHand ch , Vector3 worldPos)
    {
        // 미니언이 하나도 없다면 판별할 필요가 X
        if (playerMinions.Count == 0) { idx = 0; return; }

        // 먼저 유저 카드가 필드범위에 안들어와있으면 이동할 필요 X
        if (CheckInBox(worldPos) == false)
        {
            StopAllCoroutines();
            // 예상되는 idx가 음수값은, 현재 마우스포인터가 필드위가 아닌 곳에 있다.
            // 그러면 필드의 미니언들은 원래의 위치값으로 재이동
            idx = -1000; 
            for (int i = 0; i < playerMinions.Count; i++)
            {
                // 본래의 위치로 돌아가기
                StartCoroutine(move(playerMinions[i].transform, playerMinions[i].OriginPos));
            }
            //Debug.Log("원래의 위치로 재정렬");
            return;
        }
        //Debug.Log("IN BOX LINE");
        int currIdx = 0;
        
        // 현재 스폰포인트와 가장 가까운 마우스 위치 값 찾기
        float minDist = 10f;
        for (int i = 0; i < spawnPoint.Count; i++)
        {
            float dist = Mathf.Abs(spawnPoint[i].x - worldPos.x);
            if (minDist > dist)
            {
                minDist = dist;
                currIdx = i;
            }
        }
        // 만약 현재 필드위 드래깅 인덱스가 기존 인덱스랑 같다면, 미니언들 움직일 필요가 X
        if (currIdx == idx) { return; }
        // 바뀐 예상 위치인덱스로 갱신
        idx = currIdx;
        // 움직여야할 인덱싱이 바뀌었다면 기존 움직임있을지도 모르니 모두 중지시키고 새로 이동 코루틴 실행
        StopAllCoroutines();

        // 현재 커서가 필드의 몬스터들 위치 사이 어느곳에 존재
        // 마우스 커서랑 가장 가까운 스폰포인트 지점에 몬스터가 놓인다고 예상한채로
        // 현재의 필드 미니언들을 미리 이동 시켜두기
        int count = playerMinions.Count + 1;
        Debug.Log($"idx : {idx}, playersCount : {playerMinions.Count}");
        for (int i = 0; i < count; i++)
        {
            // 최대한 가운데 인덱싱을 정해둔 상태로, 좌우로 퍼지도록 위치 선정
            float x = 0.5f + 1.25f * (i - (count - 1) / 2.0f);

            if (idx == i)
            { continue; }

            else if (idx < i)
            {
                Debug.Log($"195Line : {i - 1}번");
                StartCoroutine(move(playerMinions[i-1].transform, new Vector3(x, 0.5f, -0.1f))); }
        
            else
            {
                Debug.Log($"200Line : {i}번");
                StartCoroutine(move(playerMinions[i].transform, new Vector3(x, 0.5f, -0.1f))); 
            }
        }
        
        // 각각의 이동 코루틴 애니메이션
        IEnumerator move(Transform tr, Vector3 dest)
        {
            float t = 0;
            Vector3 startPos = tr.transform.localPosition;
            while (t < 1f) 
            {
                t += Time.deltaTime * 2.5f;
                tr.transform.localPosition =
                    Vector3.Lerp(startPos , dest ,t);
                yield return null;
            }
        }
    }

    // 미니언 카드 스폰 실행 함수
    public void StartSpawn(CardHand card)
    {
        // 카드 사용 : 마나소모
        GAME.IGM.Hero.Player.MP -= card.CurrCost;

        // 핸드카드도 정렬 시작
        GAME.IGM.StartCoroutine(GAME.IGM.Hand.CardAllignment(true));
        // idx에 우선 소환 및 배치만 시키고 정렬을 addAction으로?
        MinionCardData mc = new MinionCardData(card.MC);
        int cachidx = idx;

        // 전투중, 다중소환등의 상황이 있을수있어 예약하여 실행
        GAME.IGM.AddAction(StartSpawn(mc, idx, card.transform.localPosition));
        // 핸드 카드는 이제 필요없기에 소멸애니메이션 코루틴 실행 (삭제도 내부에서 진행)
        GAME.Manager.StartCoroutine(card.FadeOutCo());

        IEnumerator StartSpawn(MinionCardData mc, int cacheIdx, Vector3 dest )
        {
            // 현재 핸드카드 (미니언) 정보 토대로 스폰 시작
            CardField cf = GameObject.Instantiate(prefab, Players);
            cf.PunId = card.PunId;
            cf.transform.localPosition = dest;
            playerMinions.Insert(cacheIdx, cf);
            // 미니언카드 데이터 초기화
            cf.Init(card.MC, true);

            // 소환 효과음 재생
            audioPlayer.clip = GAME.IGM.GetClip(Define.IGMsound.Summon);
            audioPlayer.Play();

            // 필드 하수인들의 레이를 잠시 끄기
            playerMinions.ForEach(x => x.Ray = false);

            // 모든 이동 코루틴 끝났을지, 다음 스폰포인트 위치값 초기화
            // 이동 코루틴을 모아둔 queue의 갯수가 0 => 현재 이동코루틴 모두 실행 완료
            yield return GAME.IGM.StartCoroutine(AlingTest(true));
            Debug.Log("정렬완료 From SpawnManager");
            // 필드 하수인들의 레이를 다시 켜기
            playerMinions.ForEach(x => x.Ray = true);
            // 새로운 위치 구하기
            yield return StartCoroutine(CalcSpawnPoint());

            // 하수인들이 소환될떄마다, 손에서 실행해야할 이벤트가 있는 카드들은 현재 소환된 하수인의 넘버를 인자로 이벤트 실행
            GAME.IGM.Hand.PlayerHand.FindAll(x => x.HandCardChanged != null && x.PunId != cf.PunId).
                ForEach(x => x.HandCardChanged.Invoke(cf.MC.cardIdNum, cf.IsMine));
            int idx = playerMinions.IndexOf(cf);
            Debug.Log("spawnIDX : " + idx);
            // 상대에게 내 미니언 소환 이벤트 전파 [카드객체 식별자, 몇번쨰 필드에 소환인지, 실제 카드 데이터 , 원본이 아닌 현재 공체비용]
            GAME.IGM.Packet.SendMinionSpawn(cf.PunId, idx, card.MC.cardIdNum, card.Att, card.HP, card.CurrCost);

            // 그후 손에서 낼떄 이벤트 있을시 실행
            yield return GAME.IGM.StartCoroutine(cf.InvokeEvt());
        }

        // 미니언이 위치를 잡을떄까지 대기 예약 (이벤트가 있고 곧바로 이벤트 실행시 부자연스럽기에)
       
    }
    public IEnumerator AlingTest(bool isPlayers)
    {
        List<CardField> cfs = (isPlayers) ? playerMinions: enemyMinions;

        int count = cfs.Count + 1;
        if (cfs.Count == 1)
        {
            cfs[0].OriginPos = new Vector3(0.5f, 0.5f, -0.1f);
            yield return StartCoroutine(move(cfs[0]));
            yield break;
        }
        // 모든 필드 미니언들 위치 재정렬
        for (int i = 0; i < cfs.Count; i++)
        {
            // 최대한 가운데 인덱싱을 정해둔 상태로, 좌우로 퍼지도록 위치 선정
            float x = 0.5f + 1.25f * (i - (cfs.Count-1) / 2.0f);

            cfs[i].OriginPos = new Vector3(x, 
                (isPlayers) ? 0.5f : 2.25f
                , -0.1f);
            StartCoroutine(move(cfs[i]));
            yield return null;
        }
        // 이동 애니메이션 코루틴
        IEnumerator move(CardField cf)
        {
            float t = (cf.transform.localPosition == cf.OriginPos) ? 1f : 0f;
            Vector3 startPos = cf.transform.localPosition;
            while (t < 1f)
            {
                t += Time.deltaTime * 2.5f;
                cf.transform.localPosition =
                    Vector3.Lerp(startPos, cf.OriginPos, t);
                yield return null;
            }
        }
    }
}
