using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class TargetingCamera : MonoBehaviour
{
    public LineRenderer LR;
    public SpriteRenderer Arrow;
    private void Awake()
    {
        // Ÿ���� ī�޶� ����
        GAME.Manager.IGM.TC = this;
        LR.positionCount = 0;
    }
    public float dist = 7;
    public IEnumerator DrawLine(Vector3 startPos)
    {
        // ���η����� ����
        LR.gameObject.SetActive(true);
        LR.positionCount = 6;
        while (LR.positionCount > 0)
        {
            // ���� ������ Ŀ������Ʈ ����������
            Vector3 CursorPos = Camera.main.ScreenToWorldPoint
               (new Vector3(Input.mousePosition.x, Input.mousePosition.y, dist));

            // ���⺤�� �� ���� ���ϱ�
            Vector3 dir = CursorPos - startPos;
            float distance = Vector3.Distance(startPos, CursorPos);
            // ���� ���� ���� ���ؼ� ���Ŀ� ���
            Vector3 oneGrid = dir.normalized;

            // (���� / 2)��ŭ LR�� ���׸���� �ؽ�ó Ÿ�ϸ� �ɰ���
            LR.material.mainTextureScale = new Vector2(distance * .5f, 1);

            // Ŀ����ġ���� �ణ �ڷ� ��������ŭ�� �Ÿ��� ���� (ȭ��ǥ�� �ֻ������ ��ġ��Ű�� �;)
            Vector3 dest = CursorPos - (oneGrid * 0.5f);
            // LR������ ��ġ����
            for (int i = 0; i < LR.positionCount; i++)
            {
                Vector3 pos = Vector3.Lerp(startPos, dest, (float)i / (LR.positionCount));
                LR.SetPosition(i, pos);
                yield return null;
            }

            // ������ ���ϱ�
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            // ȸ���� ��ġ ��Ű��
            Arrow.transform.localRotation = Quaternion.Euler(0, 0, angle - 90f);
            Arrow.transform.localPosition = CursorPos - oneGrid * .5f;
        }

        LR.gameObject.SetActive(false);
    }

    public IEnumerator TargetCo = null;
    public void StartTargetting(IBody attacker, Func<IBody, IBody, IEnumerator> RegisterCo)
    {
        Debug.Log(attacker.TR.name+"�� Ÿ���� �õ�");

        // ���� Ÿ���� �ڷ�ƾ ��� �� ����
        TargetCo = TargettingCo(attacker, RegisterCo);
        StartCoroutine(TargetCo);
    }
    
    public IEnumerator TargettingCo(IBody attacker, Func<IBody, IBody, IEnumerator> RegisterCo)
    {
        // �ణ�� �����̸� �־ �ٷ� ���� ���� ���� ����
        yield return new WaitForSeconds(0.15f);
        // ���ݱ����� �׸��� ���η����� ����
        StartCoroutine(DrawLine(attacker.Pos));

        // �̴Ͼ��� ��� ���õǾ��� ǥ�ø� �ڷ�ƾ �ִϸ��̼� ����
        if (attacker.bodyType == BodyType.Minion)
        { StartCoroutine(attacker.StartReadyCoAnimation()); }

        Vector3 camPos = Camera.main.transform.position;
        while (true)
        {
            // ���� ������ Ŀ������Ʈ ����������
            Vector3 CursorPos = Camera.main.ScreenToWorldPoint
               (new Vector3(Input.mousePosition.x, Input.mousePosition.y, -camPos.z));

            // ���� ���콺 ��ġ�� �浹ü�ִ��� �˻�
            RaycastHit2D hit = Physics2D.Raycast(CursorPos,Vector2.zero);

            // ���콺 Ŭ���� �浹ü Ȯ�ν� ����
            if (Input.GetMouseButtonDown(0))
            {
                LR.positionCount = 0;
                if (hit.collider != null)
                {
                    if (attacker.bodyType == BodyType.Minion)
                    { StartCoroutine(attacker.ExitReadyCoAnimation()); }
                       
                    // ���� ����Ŀ�� � �༮���� : �̴Ͼ� �Ϲݰ���, ������������, ������ų��� ���
                    
                    Debug.Log("�浹ü �̸� : " + hit.collider.name);
                }
                else
                {
                    if (attacker.bodyType == BodyType.Minion)
                    { StartCoroutine(attacker.ExitReadyCoAnimation()); }
                    Debug.Log("�浹ü ������ ��");
                }
                
                break;
            }

            yield return null;
        }
        // ���� ��� �ٽ� Ȱ��ȭ
        GAME.Manager.IGM.Spawn.SpawnRay = attacker.Ray = true;
        TargetCo = null;
        yield break;
    }
}
