using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SpawnManager : MonoBehaviour
{
    public Transform Players, Enemies;
    public List<CardField> enemyMinions = new List<CardField>();
    public List<CardField> playerMinions = new List<CardField>();  
    public List<Vector3> spawnPoint = new List<Vector3>();
    public CardField prefab;

    public int idx = 0; // ��ȯ�� �̴Ͼ��� �ε��� ��ġ
    public bool SpawnRay { get { return SpawnArea.enabled; } set { SpawnArea.enabled = value; } }
    BoxCollider2D SpawnArea;
    private void Awake()
    {
        GAME.Manager.IGM.Spawn = this;
        SpawnArea = transform.GetComponentInChildren<BoxCollider2D>();
        Debug.Log("SpawnArea : "+SpawnArea);

        spawnPoint.Add(new Vector3(0.5f, 0.5f, -0.1f));
    }

    // ���� �̴Ͼ��� ���� ��ġ�� �̸� ���
    public void CalcSpawnPoint() 
    {
        // ���� �巡��Ŀ���� �ʵ������ ���°����� �ʱ�ȭ
        idx = -1000;

        // �ִ� �̴Ͼ��� ������ 7�̸� �� �̻� ���� ���� �ʿ� X
        if (playerMinions.Count == 7 || spawnPoint.Count == playerMinions.Count+1) { return; }

        // ���� �̴Ͼ��� ������ ������ ������ ��ġ�� ���� �̴Ͼ� ��ȯ ��ġ ����
        if (playerMinions.Count == 0) { spawnPoint[0] = new Vector3(0.5f, 0.5f, -0.1f); return; }

        // �̴Ͼ��� 1�� �̻���ʹ� SpawnPoint��ҵ��� ����Ǳ��� ��ġ ������ ����
        else
        {
            for (int i = 0; i < playerMinions.Count; i++)
            {
                playerMinions[i].OriginPos = spawnPoint[i];
                playerMinions[i].transform.localPosition = playerMinions[i].OriginPos;
            }
        }

        // ���� �̴Ͼ��� ���� +1 �� ���� ���� ����� (���� �̴Ͼ� ��ȯ�� ������ �̸� �����ϴ� ����)
        spawnPoint.Add(new Vector3(0, 0, 0));
        int count = spawnPoint.Count;
        for (int i = 0; i < spawnPoint.Count; i++)
        {
            // i - (count - 1) / 2 �� �߽��� �������� -3, -2, -1, 0, 1, 2, 3 ���� ��ȯ�� ����
            float x = 0.5f + 1.25f * (i - (count - 1) / 2.0f);
            spawnPoint[i] = new Vector3(x, 0.5f, -0.1f);
        }
    }

    // ���� ������ �̴Ͼ��ڵ�ī�尡, �ʵ� ������ ���Դ��� Ȯ���ϴ� �Լ�
    public bool CheckInBox(Vector3 worldPos)
    {
        SpawnArea.enabled = true;
        return SpawnArea.OverlapPoint(new Vector3(worldPos.x, worldPos.y));
    }

    // ������ �ڵ�ī���� �̴Ͼ� ī�带 �巡���Ͽ� �ʵ�������
    // �����ϋ�, �̴Ͼ�� ���� �ڸ� ���̸� �̸� �������� �����ִ� �Լ�
    public void MinionAlignment(CardHand ch , Vector3 worldPos)
    {
        // �̴Ͼ��� �ϳ��� ���ٸ� �Ǻ��� �ʿ䰡 X
        if (spawnPoint.Count == 1) { idx = 0; return; }

        // ���� ���� ī�尡 �ʵ������ �ȵ��������� �̵��� �ʿ� X
        if (CheckInBox(worldPos) == false)
        {
            StopAllCoroutines();
            // ����Ǵ� idx�� ��������, ���� ���콺�����Ͱ� �ʵ����� �ƴ� ���� �ִ�.
            // �׷��� �ʵ��� �̴Ͼ���� ������ ��ġ������ ���̵�
            idx = -1000; 
            for (int i = 0; i < playerMinions.Count; i++)
            {
                // ������ ��ġ�� ���ư���
                StartCoroutine(move(playerMinions[i].transform, playerMinions[i].OriginPos));
            }
            Debug.Log("������ ��ġ�� ������");
            return;
        }
        
        Debug.Log("IN BOX LINE");
        int currIdx = 0;
        
        // ���� ��������Ʈ�� ���� ����� ���콺 ��ġ �� ã��
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
        // ���� ���� �ʵ��� �巡�� �ε����� ���� �ε����� ���ٸ�, �̴Ͼ�� ������ �ʿ䰡 X
        if (currIdx == idx) { return; }
        // �ٲ� ���� ��ġ�ε����� ����
        idx = currIdx;
        // ���������� �ε����� �ٲ���ٸ� ���� �������������� �𸣴� ��� ������Ű�� ���� �̵� �ڷ�ƾ ����
        StopAllCoroutines();

        // ���� Ŀ���� �ʵ��� ���͵� ��ġ ���� ������� ����
        // ���콺 Ŀ���� ���� ����� ��������Ʈ ������ ���Ͱ� ���δٰ� ������ä��
        // ������ �ʵ� �̴Ͼ���� �̸� �̵� ���ѵα�
        for (int i = 0; i < spawnPoint.Count; i++)
        {
            if (idx < i)
            { StartCoroutine(move(playerMinions[i-1].transform, spawnPoint[i])); }

            else if (idx == i)
            { continue; }

            else
            { StartCoroutine(move(playerMinions[i].transform, spawnPoint[i])); }
        }

        // ������ �̵� �ڷ�ƾ �ִϸ��̼�
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

    // �̴Ͼ���� �̵� �ִϸ��̼��ڷ�ƾ ī��Ʈ
    Queue<GameObject> queue = new Queue<GameObject>();
    // �̴Ͼ� ī�� ���� ���� �Լ�
    public void StartSpawn(CardHand card)
    {
        // ���� �ڵ�ī�� (�̴Ͼ�) ���� ���� ���� ����
        CardField cf = GameObject.Instantiate(prefab, Players);
        cf.Init(card.data);
        cf.transform.localPosition = card.transform.localPosition;
        playerMinions.Insert(idx, cf);
        
        // �ڵ� ī��� ���� �ʿ���⿡ �Ҹ�ִϸ��̼� �ڷ�ƾ ���� (������ ���ο��� ����)
        GAME.Manager.StartCoroutine(card.FadeOutCo());

        // ��ȯ ȿ���� ���
        GAME.Manager.SM.PlaySound(Define.Sound.Summon);

        // ��� �ʵ� �̴Ͼ�� ��ġ ������
        for (int i = 0; i < playerMinions.Count; i++)
        {
            queue.Enqueue(playerMinions[i].gameObject);
            playerMinions[i].OriginPos = spawnPoint[i];
            StartCoroutine(move(playerMinions[i].transform, i));
        }

        // ��� �̵� �ڷ�ƾ ��������, ���� ��������Ʈ ��ġ�� �ʱ�ȭ
        StartCoroutine(waitCo()); 
        IEnumerator waitCo()
        {
            // �̵� �ڷ�ƾ�� ��Ƶ� queue�� ������ 0 => ���� �̵��ڷ�ƾ ��� ���� �Ϸ�
            yield return new WaitUntil(() => (queue.Count == 0));
            // ��ȯ�� �̴Ͼ��� ���ڴ� ��� ���ֱ�
            cf.sleep.gameObject.SetActive(true);
            // ���ο� ��ġ ���ϱ�
            CalcSpawnPoint();
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
            queue.Dequeue();
        }
    }

  
}
