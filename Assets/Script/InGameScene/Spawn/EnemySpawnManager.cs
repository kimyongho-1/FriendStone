﻿using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SpawnManager 
{
    public IEnumerator EnemySpawn(int punID, int spawnIdx)
    {
        #region 상대가 소환할것이기에, 추가 자리를 포함하여 적 미니언들 위치 계산
        // 움직일 적의 핸드카드 객체
        CardHand card = GAME.IGM.Hand.EnemyHand.Find(x => x.PunId == punID);
        // 총 에너미 미니언 갯수 (늘어날것을 추가하여 계산)
        int count = enemyMinions.Count + 1;
        List<Vector3> pointList = new List<Vector3>();
        for (int i = 0; i < count; i++)
        {
            // 최대한 가운데 인덱싱을 정해둔 상태로, 좌우로 퍼지도록 위치 선정
            float x = 0.5f + 1.25f * (i - (count - 1) / 2.0f);

            pointList.Add(new Vector3(x, 2.25f, -0.1f));
        }
        #endregion

        #region 상대의 핸드카드가 소환될 위치로 이동하는 모습
        yield return StartCoroutine(handCardMove());
        // 먼저 적의 핸드카드가 소환될 위치로 이동하는 모습 코루틴
        IEnumerator handCardMove()
        {
            Vector3 start = card.transform.localPosition;
            Vector3 dest = pointList[spawnIdx]; // 소환될 위치로 핸드카드가 이동 유도
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

        #region 상대의 핸드 카드소멸 코루틴 및 필드카드 프리팹 생성
        // 현재 핸드카드 (미니언) 정보 토대로 스폰 시작
        CardField cf = GameObject.Instantiate(prefab, Enemies);
        cf.Init(card.data,false);
        cf.PunId = card.PunId;
        cf.transform.localPosition = card.transform.localPosition;
        enemyMinions.Insert(spawnIdx, cf);
        // 현재 적 핸드목록에서 소환할 이 미니언카드 제거
        GAME.IGM.Hand.EnemyHand.Remove(card);
        // 핸드 카드는 이제 필요없기에 소멸애니메이션 코루틴 실행 (삭제도 내부에서 진행)
        GAME.Manager.StartCoroutine(card.FadeOutCo(false));
        #endregion

        // 소환 효과음 재생
        GAME.Manager.SM.PlaySound(Define.Sound.Summon);

        // 모든 필드 미니언들 위치 재정렬
        for (int i = 0; i < enemyMinions.Count; i++)
        {
            // 최대한 가운데 인덱싱을 정해둔 상태로, 좌우로 퍼지도록 위치 선정
            float x = 0.5f + 1.25f * (i - (count - 1) / 2.0f);

            enemyMinions[i].OriginPos = new Vector3(x,2.25f, -0.1f);
            StartCoroutine(move(enemyMinions[i].transform, i));
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
                    Vector3.Lerp(startPos, enemyMinions[idx].OriginPos, t);
                yield return null;
            }
        }

        // 하수인들이 소환될떄마다, 손에서 실행해야할 이벤트가 있는 카드들은 현재 소환된 하수인의 넘버를 인자로 이벤트 실행
        GAME.IGM.Hand.PlayerHand.FindAll(x => x.HandCardChanged != null).ForEach(x => x.HandCardChanged.Invoke(cf.data.cardIdNum, cf.IsMine));

        // 현재 소환된 카드가, 정해진 위치로 이동을 끝맞출떄까지 대기
        yield return new WaitUntil(()=>(cf.transform.position == cf.OriginPos));
        // 연이어 공격 이벤트 전파시, 위치를 다 끝 맞춘 상태에서 진행되는것이 자연스럽기에
    }


    // 몬스터가 죽고나서 재정렬
    public IEnumerator AllEnemiesAlignment()
    {
        int count = enemyMinions.Count + 1;
        for (int i = 0; i < enemyMinions.Count; i++)
        {
            float x = 0.5f + 1.25f * (i - (count - 1) / 2.0f);

            enemyMinions[i].OriginPos = new Vector3(x, 2.25f, -0.1f);
            StartCoroutine(move(enemyMinions[i].transform, i));
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
}
