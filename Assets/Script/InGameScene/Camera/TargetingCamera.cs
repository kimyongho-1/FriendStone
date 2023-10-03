using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static Define;

public class TargetingCamera : MonoBehaviour
{
    public LineRenderer LR;
    public SpriteRenderer Arrow;
    public Image transitionPanel;
    private void Awake()
    {
        // Ÿ���� ī�޶� ����
        GAME.IGM.TC = this;
        LR.positionCount = 0;
    }
    public float dist = 7;
    public float ShakeDuration = 2;  // ShakeDuration�� �� ȸ���� �ð�
    public float maxAngle = 1f; // �ִ� ������ (z���� �ִ� ȸ��ġ)
    // ��� �ӵ�
    public float playTime = 2f; // ShakeDuration���� ����� �ݺ�����

    // ���ݵ��� ����ɋ�, ī�޶� �ѵ�� �ڷ�ƾ
    public IEnumerator ShakeCo()
    {
        // 0 ~ 2PI��  �����Լ����� ���ֱ�
        float t = 0;
        float frequency = 2 * Mathf.PI / ShakeDuration; // ShakeDur���� ���ֱ⸦ ���� ��ġ
        frequency *= playTime; // �߰��� ����� ��鸱�� playTime��ŭ ���ϱ�
        Transform tr = Camera.main.transform;
        while (t < ShakeDuration)
        {
            t += Time.deltaTime;
            //Debug.Log($" Mahtf.Sin( {t} * {frequency} : {t * frequency}) : {Mathf.Sin(t * frequency)}");
            float currAngle = Mathf.Sin(t * frequency) * maxAngle;
            tr.rotation = Quaternion.Euler(0, 0, currAngle);
            yield return null;
        }
        tr.rotation = Quaternion.identity; // ȸ���� �ʱ� ���·� ����
    }

    // ������ ���콺 �巡�� ���� ���� �׸���
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

    // ������ ���콺 Ÿ���� ���� 
    public IEnumerator TargettingCo(IBody attacker, Func<IBody, IBody, IEnumerator> RegisterCo, string[] filter)
    {
        // �ణ�� �����̸� �־ �ٷ� ���� ���� ���� ����
        yield return new WaitForSeconds(0.15f);

        // ���ݱ����� �׸��� ���η����� ����, ���� ��ġ�� ���ڷ�
        // �̴Ͼ�ī��� �ڽ��� ��ġ, �׿� �ֹ�ī�峪 ������ �ڽ��� ���� ������ġ���� ����
        Vector3 startPos = (attacker.objType == ObjType.Minion) ? attacker.Pos : new Vector3(GAME.IGM.Hero.Player.OriginPos.x, -0.9f, GAME.IGM.Hero.Player.OriginPos.z);
        StartCoroutine(DrawLine(startPos));

        // �����ڰ� �̴Ͼ��̳� �����̸� �����ڼ� ���ϱ�
        if (attacker.objType != Define.ObjType.HandCard)
        {
            float t = 0;
            Vector3 dest = attacker.OriginPos + new Vector3(0, -0.25f, -0.5f);
            while (t < 1f)
            {
                t += Time.deltaTime * 2.5f;
                attacker.TR.position = Vector3.Lerp(attacker.OriginPos, dest, t);
                yield return null;
            }
        }
        // Ÿ���� �����ڰ� �ڵ�ī���� 
        if (attacker.objType == Define.ObjType.HandCard)
        {
            float t = 0;
            Vector3 dest = attacker.OriginPos + new Vector3(0, 0.5f, -0.5f);
            while (t < 1f)
            {
                t += Time.deltaTime * 2.5f;
                attacker.TR.position = Vector3.Lerp(attacker.OriginPos, dest, t);
                yield return null;
            }
        }

        // ����Ȯ��
        if (filter == null) { Debug.Log("Filter is NUll"); }

        Vector3 camPos = Camera.main.transform.position;
        Func<bool> waitInput = (attacker.objType != Define.ObjType.HandCard) ?
            () => { return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1); }
        :
            () => { return Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1); };

        while (true)
        {
            // ���� ������ Ŀ������Ʈ ����������
            Vector3 CursorPos = Camera.main.ScreenToWorldPoint
               (new Vector3(Input.mousePosition.x, Input.mousePosition.y, -camPos.z));

            // ���콺 Ŭ���� �浹ü Ȯ�ν� ����
            if (waitInput.Invoke()) //Input.GetMouseButtonDown(0)
            {
                Collider2D hit = Physics2D.OverlapPoint(CursorPos, LayerMask.GetMask(filter)); ;
                for (int i = 0; i < filter.Length; i++)
                {
                    Debug.Log(filter[i]);
                }
                LR.positionCount = 0;
                if (hit != null)
                {
                    // ���� �ڷ�ƾ ���� ����
                    GAME.IGM.AddAction(RegisterCo(attacker, hit.transform.GetComponent<IBody>()));
                    // ���� ����Ŀ�� � �༮���� : �̴Ͼ� �Ϲݰ���, ������������, ������ų��� ���
                    
                    Debug.Log("�浹ü �̸� : " + hit.name);
                }
                else
                {
                    Debug.Log("�浹ü ������ ��");
                    // �����ڰ� �̴Ͼ��̾����� �����ڼ� �����ϱ�
                    if (attacker.objType != Define.ObjType.HandCard)
                    {
                        float t = 0;
                        Vector3 start = attacker.Pos;
                        Vector3 dest = attacker.OriginPos ;
                        while (t < 1f)
                        {
                            t += Time.deltaTime * 2.5f;
                            attacker.TR.position = Vector3.Lerp(start , dest, t);
                            yield return null;
                        }
                    }  
                    // Ÿ���� �����ڰ� �ڵ�ī���� 
                    if (attacker.objType == Define.ObjType.HandCard)
                    {
                        float t = 0;
                        Vector3 start = attacker.Pos;
                        Vector3 dest = attacker.OriginPos;
                        while (t < 1f)
                        {
                            t += Time.deltaTime * 2.5f;
                            attacker.TR.position = Vector3.Lerp(start, dest, t);
                            yield return null;
                        }
                    }
                }
                
                break;
            }

            yield return null;
        }
        // ���α��� �׸��� �ڷ�ƾ �������� ���� �ʱ�ȭ
        LR.positionCount = 0;
        // ���� ��� �ٽ� Ȱ��ȭ
        GAME.IGM.Spawn.SpawnRay = attacker.Ray = true;
        yield break;
    }

    // �� ���� �ΰ��Ӿ� ���� + ���� �⺻���� ���� Ȯ�ν�
    // ������ Ŭ���̾�Ʈ�� ���ӳ� Ʈ������ ĵ������ ���İ��� ���̸鼭 ���� ���� ���� �ڷ�ƾ
    public IEnumerator StartIntro()
    {
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * 0.5f;
            transitionPanel.color = new Color(0,0,0,1-t);
            yield return null;
        }
    }
}
