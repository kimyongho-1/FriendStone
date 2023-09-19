using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SpawnManager 
{
    public IEnumerator EnemySpawn(int handIdx, int spawnIdx)
    {
        #region ��밡 ��ȯ�Ұ��̱⿡, �߰� �ڸ��� �����Ͽ� �� �̴Ͼ�� ��ġ ���
        // ������ ���� �ڵ�ī�� ��ü
        CardHand card = GAME.Manager.IGM.Hand.EnemyHand[handIdx];
        // �� ���ʹ� �̴Ͼ� ���� (�þ���� �߰��Ͽ� ���)
        int count = enemyMinions.Count + 1;
        List<Vector3> pointList = new List<Vector3>();
        for (int i = 0; i < count; i++)
        {
            // �ִ��� ��� �ε����� ���ص� ���·�, �¿�� �������� ��ġ ����
            float x = 0.5f + 1.25f * (i - (count - 1) / 2.0f);

            pointList.Add(new Vector3(x, 2.25f, -0.1f));
        }
        #endregion

        #region ����� �ڵ�ī�尡 ��ȯ�� ��ġ�� �̵��ϴ� ���
        yield return StartCoroutine(handCardMove());
        // ���� ���� �ڵ�ī�尡 ��ȯ�� ��ġ�� �̵��ϴ� ��� �ڷ�ƾ
        IEnumerator handCardMove()
        {
            Vector3 start = card.transform.localPosition;
            Vector3 dest = pointList[spawnIdx]; // ��ȯ�� ��ġ�� �ڵ�ī�尡 �̵� ����
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime;
                card.transform.localPosition =
                    Vector3.Lerp(start, dest, t);
                yield return null;
            }
        }

        #endregion

        #region ����� �ڵ� ī��Ҹ� �ڷ�ƾ �� �ʵ�ī�� ������ ����
        // ���� �ڵ�ī�� (�̴Ͼ�) ���� ���� ���� ����
        CardField cf = GameObject.Instantiate(prefab, Enemies);
        cf.Init(card.data,false);
        cf.PunId = card.PunId;
        cf.transform.localPosition = card.transform.localPosition;
        enemyMinions.Insert(0, cf);
        // ���� �� �ڵ��Ͽ��� ��ȯ�� �� �̴Ͼ�ī�� ����
        GAME.Manager.IGM.Hand.EnemyHand.Remove(GAME.Manager.IGM.Hand.EnemyHand[handIdx]);
        // �ڵ� ī��� ���� �ʿ���⿡ �Ҹ�ִϸ��̼� �ڷ�ƾ ���� (������ ���ο��� ����)
        GAME.Manager.StartCoroutine(card.FadeOutCo(false));
        #endregion

        // ��ȯ ȿ���� ���
        GAME.Manager.SM.PlaySound(Define.Sound.Summon);

        // ��� �ʵ� �̴Ͼ�� ��ġ ������
        for (int i = 0; i < enemyMinions.Count; i++)
        {
            // �ִ��� ��� �ε����� ���ص� ���·�, �¿�� �������� ��ġ ����
            float x = 0.5f + 1.25f * (i - (count - 1) / 2.0f);

            enemyMinions[i].OriginPos = new Vector3(x,2.25f, -0.1f);
            StartCoroutine(move(enemyMinions[i].transform, i));
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
                    Vector3.Lerp(startPos, enemyMinions[idx].OriginPos, t);
                yield return null;
            }
        }
    }
}
