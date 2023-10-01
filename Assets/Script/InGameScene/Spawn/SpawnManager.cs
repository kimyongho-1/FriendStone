﻿using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SpawnManager : MonoBehaviour
{
    public Transform Players, Enemies;
    public CustomList enemyMinions = new CustomList();
    public CustomList playerMinions = new CustomList();
    public List<Vector3> spawnPoint = new List<Vector3>();
    public CardField prefab;

    public int idx = 0; // 소환될 미니언의 인덱싱 위치
    public bool SpawnRay { get { return SpawnArea.enabled; } set { SpawnArea.enabled = value; } }
    BoxCollider2D SpawnArea;
    private void Awake()
    {
        GAME.IGM.Spawn = this;
        SpawnArea = transform.GetComponentInChildren<BoxCollider2D>();

        spawnPoint.Add(new Vector3(0.5f, 0.5f, -0.1f));
    }

    // 다음 미니언이 놓일 위치를 미리 계산
    public void CalcSpawnPoint() 
    {
        // 현재 드래깅커서가 필드범위에 없는것으로 초기화
        idx = -1000;

        // 최대 미니언의 갯수가 7이면 더 이상 공간 만들 필요 X
        if (playerMinions.Count == 7 || spawnPoint.Count == playerMinions.Count+1) { return; }

        // 현재 미니언이 없으면 언제나 고정된 위치로 다음 미니언 소환 위치 결정
        if (playerMinions.Count == 0) { spawnPoint[0] = new Vector3(0.5f, 0.5f, -0.1f); return; }

        // 미니언이 1개 이상부터는 SpawnPoint요소들이 변경되기전 위치 값으로 결정
        else
        {
            for (int i = 0; i < playerMinions.Count; i++)
            {
                playerMinions[i].OriginPos = spawnPoint[i];
                playerMinions[i].transform.localPosition = playerMinions[i].OriginPos;
            }
        }

        // 현재 미니언의 갯수 +1 한 여유 공간 만들기 (다음 미니언 소환될 공간을 미리 마련하는 개념)
        spawnPoint.Add(new Vector3(0, 0, 0));
        int count = spawnPoint.Count;
        for (int i = 0; i < spawnPoint.Count; i++)
        {
            // i - (count - 1) / 2 는 중심을 기준으로 -3, -2, -1, 0, 1, 2, 3 같은 변환을 유도
            float x = 0.5f + 1.25f * (i - (count - 1) / 2.0f);
            spawnPoint[i] = new Vector3(x, 0.35f, -0.1f);
        }
    }

    // 내 미니언이 죽고 재정렬
    public IEnumerator AllPlayersAlignment()
    {
        spawnPoint.Clear();

        if (playerMinions.Count == 0) { spawnPoint.Add(new Vector3(0.5f, 0.5f, -0.1f)); yield break; }

        // ��� �ʵ� �̴Ͼ�� ��ġ ������
        for (int i = 0; i < playerMinions.Count + 1; i++)
        {
            // �ִ��� ��� �ε����� ���ص� ���·�, �¿�� �������� ��ġ ����
            float x = 0.5f + 1.25f * (i - (spawnPoint.Count - 1) / 2.0f);
            spawnPoint.Add(new Vector3(x, 0.5f, -0.1f));
        }
        // ��� �ʵ� �̴Ͼ�� ��ġ ������
        for (int i = 0; i < playerMinions.Count; i++)
        {
            playerMinions[i].OriginPos = spawnPoint[i];
            StartCoroutine(move(playerMinions[i].transform, i));
            yield return null;
        }

        // �̵� �ִϸ��̼� �ڷ�ƾ
        IEnumerator move(Transform tr, int idx)
        {
            float t = (tr.transform.localPosition == spawnPoint[idx]) ? 1f : 0f;
            Vector3 startPos = tr.transform.localPosition;
            while (t < 1f)
            {
                t += Time.deltaTime * 2.5f;
                tr.transform.localPosition =
                    Vector3.Lerp(startPos, spawnPoint[idx], t);
                yield return null;
            }
        }
    }
    // 현재 유저의 미니언핸드카드가, 필드 범위에 들어왔는지 확인하는 함수
    public bool CheckInBox(Vector3 worldPos)
    {
        SpawnArea.enabled = true;
        return SpawnArea.OverlapPoint(new Vector3(worldPos.x, worldPos.y));
    }

    // 유저가 핸드카드중 미니언 카드를 드래그하여 필드위에서
    // 움직일떄, 미니언들 간의 자리 사이를 미리 움직여서 보여주는 함수
    public void MinionAlignment(CardHand ch , Vector3 worldPos)
    {
        // 미니언이 하나도 없다면 판별할 필요가 X
        if (spawnPoint.Count == 1) { idx = 0; return; }

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
            Debug.Log("원래의 위치로 재정렬");
            return;
        }
        
        Debug.Log("IN BOX LINE");
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
        for (int i = 0; i < spawnPoint.Count; i++)
        {
            if (idx < i)
            { StartCoroutine(move(playerMinions[i-1].transform, spawnPoint[i])); }

            else if (idx == i)
            { continue; }

            else
            { StartCoroutine(move(playerMinions[i].transform, spawnPoint[i])); }
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

    // 미니언들의 이동 애니메이션코루틴 카운트
    Queue<GameObject> queue = new Queue<GameObject>();

    
    // 미니언 카드 스폰 실행 함수
    public void StartSpawn(CardHand card)
    {
        // 현재 핸드카드 (미니언) 정보 토대로 스폰 시작
        CardField cf = GameObject.Instantiate(prefab, Players);
        //cf.Init(card.data,true);
        cf.PunId = card.PunId;
        cf.transform.localPosition = card.transform.localPosition;
        playerMinions.Insert(idx, cf);

        // 상대에게 내 미니언 소환 이벤트 전파 [카드객체 식별자, 몇번쨰 필드에 소환인지, 실제 카드 데이터 식별자 , 원본이 아닌 현재 공체비용]
        GAME.IGM.Packet.SendMinionSpawn(cf.PunId, idx, card.data.cardIdNum , card.Att, card.HP , card.data.cost);

        // 필드 하수인들의 레이를 잠시 끄기
        playerMinions.ForEach(x=>x.Ray = false);

        CardHand tempch = card;
        int ourIDX = GAME.IGM.Hand.PlayerHand.IndexOf(card);
        Debug.Log($"idx : {idx} , ourIDX : {0}");

        // 핸드 카드는 이제 필요없기에 소멸애니메이션 코루틴 실행 (삭제도 내부에서 진행)
        GAME.Manager.StartCoroutine(card.FadeOutCo());

        // 소환 효과음 재생
        GAME.Manager.SM.PlaySound(Define.Sound.Summon);

        // 모든 필드 미니언들 위치 재정렬
        for (int i = 0; i < playerMinions.Count; i++)
        {
            queue.Enqueue(playerMinions[i].gameObject);
            playerMinions[i].OriginPos = spawnPoint[i];
            StartCoroutine(move(playerMinions[i].transform, i));
        }
        GAME.IGM.AddAction(waitPos(cf));
        cf.Init(card.data, true);
        cf.OriginAtt = card.OriginAtt;
        cf.OriginHp = card.OriginHp;
        cf.minionCardData.cost = card.data.cost;

        IEnumerator waitPos(CardField cf)
        {
            // 현재 소환된 카드가, 정해진 위치로 이동을 끝맞출떄까지 대기
            yield return new WaitUntil(() => (cf.transform.position == cf.OriginPos));
        }
        
        // 이동 애니메이션 코루틴
        IEnumerator move(Transform tr, int idx)
        {
            float t = (tr.transform.localPosition == spawnPoint[idx]) ? 1f : 0f;
            Vector3 startPos = tr.transform.localPosition;
            while (t < 1f)
            {
                t += Time.deltaTime * 2.5f;
                tr.transform.localPosition =
                    Vector3.Lerp(startPos, spawnPoint[idx], t);
                yield return null;
            }
            queue.Dequeue();
        }

        StartCoroutine(wait());
        IEnumerator wait()
        {
            // 모든 이동 코루틴 끝났을지, 다음 스폰포인트 위치값 초기화
            // 이동 코루틴을 모아둔 queue의 갯수가 0 => 현재 이동코루틴 모두 실행 완료
            yield return new WaitUntil(() => (queue.Count == 0));
            // 필드 하수인들의 레이를 다시 켜기
            playerMinions.ForEach(x => x.Ray = true);
            // 새로운 위치 구하기
            CalcSpawnPoint();

            // 하수인들이 소환될떄마다, 손에서 실행해야할 이벤트가 있는 카드들은 현재 소환된 하수인의 넘버를 인자로 이벤트 실행
            GAME.IGM.Hand.PlayerHand.FindAll(x => x.HandCardChanged != null).ForEach(x=>x.HandCardChanged.Invoke(cf.data.cardIdNum, cf.IsMine));
        }
        
    }

  
}
