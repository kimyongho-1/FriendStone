using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static Define;

public class TargetingCamera : MonoBehaviour
{
    public LineRenderer LR;
    public SpriteRenderer Arrow;
    public Image transitionPanel;
    public AudioSource targetingSound;
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
        targetingSound.Play();
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
        targetingSound.Stop();
        LR.gameObject.SetActive(false);
    }

    // �̴Ͼ�� ������ �������ݿ� ���
    public IEnumerator MeeleTargettingCo(IBody attacker, Func<IBody, IBody, IEnumerator> RegisterCo )
    {
        GAME.IGM.Turn.Col.enabled = false;
        // �ణ�� �����̸� �־ �ٷ� ���� ���� ���� ����
        yield return new WaitForSeconds(0.15f);

        // ���ݱ����� �׸��� ���η����� ����, ���� ��ġ�� ���ڷ�
        // �̴Ͼ�ī��� �ڽ��� ��ġ, �׿� �ֹ�ī�峪 ������ �ڽ��� ���� ������ġ���� ����
        Vector3 startPos = attacker.Pos;
        StartCoroutine(DrawLine(startPos));

        // �����ڰ� �̴Ͼ��̳� �����̸� �����ڼ� ���ϱ�
        float t = 0;
        Vector3 dest = attacker.OriginPos + new Vector3(0, -0.25f, -0.5f);
        while (t < 1f)
        {
            t += Time.deltaTime * 2.5f;
            attacker.TR.position = Vector3.Lerp(attacker.OriginPos, dest, t);
            yield return null;
        }
        string[] filter = new string[] {"foe", "foeHero" };
        Vector3 camPos = Camera.main.transform.position;
        Func<bool> waitInput =  () => { return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1); } ;

        while (true)
        {
            // ���� ������ Ŀ������Ʈ ����������
            Vector3 CursorPos = Camera.main.ScreenToWorldPoint
               (new Vector3(Input.mousePosition.x, Input.mousePosition.y, -camPos.z));

            // ���콺 Ŭ���� �浹ü Ȯ�ν� ����
            if (waitInput.Invoke())
            {
                Collider2D hit = Physics2D.OverlapPoint(CursorPos, LayerMask.GetMask(filter)); 
                if (hit != null)
                {
                    // ���� ���� Ÿ������ �̴Ͼ�� �����鸸�� Ÿ�ٹ���
                    IBody targetBody  = hit.transform.GetComponent<IBody>();

                    #region ��� ���� �ϼ��� ���� + ������ Ÿ���� �����ϼ����� �ƴ� ��� : ������ҷ� ����                   
                    // ���� ���� �ϼ��ε��� ��뿡�� �ִ��� Ȯ��
                    List<CardField> FindTauntList = GAME.IGM.Spawn.enemyMinions.FindAll(x => x.MC.isTaunt == true);
                    Debug.Log("�� ���߼� : " + FindTauntList.Count);
                    Debug.Log("Ÿ���� ���� ? : "+ FindTauntList.Contains(targetBody));
                    // ��뿡�� �����ϼ����� �����ϸ�, ���� �����ϱ���� Ÿ���� �����ϼ����� �ƴ϶�� ���� �Ұ��� (������ ���� �ϼ��� ���� ����)
                    if (FindTauntList.Count > 0 &&
                        FindTauntList.Contains(targetBody) == false)
                    {
                        // ���η����� �׸��� �����ڷ�ƾ �������� ���η����� ���� ���̱�
                        LR.positionCount = 0;
                        // ��뿡�� �����ϼ����� �ִµ�, �����ϼ����� �ƴ� �ٸ� Ÿ���� �����Ҽ����⿡
                        // ���� ���� ���
                        Debug.Log("�浹ü�� ������, �����ϼ����� �����ؼ� �������� �Ұ�");
                        t = 0;
                        Vector3 start = attacker.Pos;
                        dest = attacker.OriginPos;
                        while (t < 1f)
                        {
                            t += Time.deltaTime * 2.5f;
                            attacker.TR.position = Vector3.Lerp(start, dest, t);
                            yield return null;
                        }
                        // �÷��̾�� ���� �����ϼ��΋����� �����Ҽ� ���� �˸��� ( �̺�Ʈ ���� X )
                        GAME.IGM.Hero.Player.HeroSaying(Define.Emotion.ThereTaunt);

                        // ��������� ������ ���Ͽ��⿡ attackable�� �ٽ� ����
                        attacker.Attackable = true;
                    }
                    #endregion

                    #region ��� �����ϼ��� ���� + ������ Ÿ�� ���� : ����� �����ڷ�ƾ Queue�� �̺�Ʈ �������
                    // Ÿ���� ã�Ұ�, ��뿡�� �����ϼ��ε� ���ٸ�
                    else
                    {
                        // ���η����� �׸��� �����ڷ�ƾ �������� ���η����� ���� ���̱�
                        LR.positionCount = 0;
                        // ���� �ڷ�ƾ ���� ����
                        GAME.IGM.AddAction(RegisterCo(attacker, targetBody));
                        // ���� ����Ŀ�� � �༮���� : �̴Ͼ� �Ϲݰ���, ������������, ������ų��� ���

                        Debug.Log("�浹ü �̸� : " + hit.name);
                    }
                    #endregion

                    break; // �ݺ��� ����������
                }
                
                // �߸��� Ÿ�� Ŭ���� 
                else
                {
                    // ���α��� �׸��� �ڷ�ƾ �������� ���� �ʱ�ȭ
                    LR.positionCount = 0;
                    // �߸��� ���� Ŭ���� ���� ��� �� ���ڸ� ���� �ִϸ��̼�
                    Debug.Log("�浹ü ������ ��");
                    t = 0;
                    Vector3 start = attacker.Pos;
                    dest = attacker.OriginPos;
                    while (t < 1f)
                    {
                        t += Time.deltaTime * 2.5f;
                        attacker.TR.position = Vector3.Lerp(start, dest, t);
                        yield return null;
                    }
                    // ������ ���Ͽ��⿡ attackable�� �ٽ� ����
                    attacker.Attackable = true;
                    break;
                }
            }

            yield return null;
        }
        GAME.IGM.Turn.Col.enabled = attacker.Col.enabled = true;
        yield break;
    }

    // �ֹ�ī��, �̴Ͼ��� �̺�Ʈ Ÿ���� � ���
    public IEnumerator TargettingCo(IBody attacker, Func<IBody, IBody, IEnumerator> RegisterCo, string[] filter )
    {
        // �ڵ� ���ǵ帮�� ��ױ�
        GAME.IGM.Hand.PlayerHand.ForEach(x=>x.Col.enabled = false);
        // �̺�Ʈ�� ���� ������ �°� , ����Ʈ���μ��̰� ���ܴ���� ������ �ð������� �����ϱ�
        GAME.IGM.Post.StartMaskingArea(filter);
        Debug.Log("������ �浹ü ����" + attacker.Col);
        attacker.Col.enabled = false;
        GAME.IGM.Turn.Col.enabled = false;

        // �ణ�� �����̸� �־ �ٷ� ���� ���� ���� ����
        yield return new WaitForSeconds(0.15f);

        // ���ݱ����� �׸��� ���η����� ���� ��ġ�� ã��
        // �̴Ͼ�ī��� �ڽ��� ���� ��ġ, �׿� �ֹ�ī�峪 ������ �ڽ��� ���� ������ġ���� ����
        Vector3 startPos = (attacker.objType == ObjType.Minion)
            ? attacker.OriginPos : 
            new Vector3(GAME.IGM.Hero.Player.OriginPos.x, -0.9f, GAME.IGM.Hero.Player.OriginPos.z);

        // ���� ��� �׸���
        StartCoroutine(DrawLine(startPos));

        // ����Ȯ��
        if (filter == null) { Debug.Log("Filter is NUll"); }

        // ī�޶� ��ġ��, �Է�Ȯ�� �ʱ�ȭ
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
                if (hit != null)
                {
                    // ���α��� �׸��� �ڷ�ƾ �������� ���� �ʱ�ȭ
                    LR.positionCount = 0;
                    // ���� �ڷ�ƾ ���� ����
                    GAME.IGM.AddAction(RegisterCo(attacker, hit.transform.GetComponent<IBody>()));
                    Debug.Log("�浹ü �̸� : " + hit.name);
                    break;
                }
                else
                {
                    Debug.Log("Ÿ�� ����");
                    // �տ��� ����ϴ�, �ֹ�ī���� ��� �߸��� ���� Ŭ���� ����ϵ��� ����
                    if (attacker.objType == ObjType.HandCard)
                    {
                        // ���α��� �׸��� �ڷ�ƾ �������� ���� �ʱ�ȭ
                        LR.positionCount = 0; break; 
                    }
                }
            }

            yield return null;
        }

        GAME.IGM.Post.ExitMaskingArea();
        // ���� ��� �ٽ� Ȱ��ȭ
        attacker.Ray = true;
        GAME.IGM.Turn.Col.enabled = attacker.Col.enabled = true;
        GAME.IGM.Hand.PlayerHand.ForEach(x => x.Col.enabled = true);
        yield break;
    }
    public IEnumerator TargettingHeroSkillCo(IBody attacker)
    {
        Hero player = GAME.IGM.Hero.Player;
        // �ڵ� ���ǵ帮�� ��ױ�
        GAME.IGM.Hand.PlayerHand.ForEach(x => x.Col.enabled = false);
        attacker.Col.enabled = false;
        GAME.IGM.Turn.Col.enabled = false;
        // �ణ�� �����̸� �־ �ٷ� ���� ���� ���� ����
        yield return new WaitForSeconds(0.15f);

        // ���� ��� �׸���
        StartCoroutine(DrawLine(attacker.OriginPos));

        
        // ī�޶� ��ġ��, �Է�Ȯ�� �ʱ�ȭ
        Vector3 camPos = Camera.main.transform.position;
        Func<bool> waitInput = (attacker.objType != Define.ObjType.HandCard) ?
            () => { return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1); }
        :
            () => { return Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1); };
        string[] filter = new string[] { "ally", "allyHero", "foe", "foeHero", };
        while (true)
        {
            // ���� ������ Ŀ������Ʈ ����������
            Vector3 CursorPos = Camera.main.ScreenToWorldPoint
               (new Vector3(Input.mousePosition.x, Input.mousePosition.y, -camPos.z));

            // ���콺 Ŭ���� �浹ü Ȯ�ν� ����
            if (waitInput.Invoke()) //Input.GetMouseButtonDown(0)
            {
                Collider2D hit = Physics2D.OverlapPoint(CursorPos, LayerMask.GetMask(filter)); ;
                if (hit != null && hit.TryGetComponent<IBody>(out IBody targetFounded))
                {
                    // ���α��� �׸��� �ڷ�ƾ �������� ���� �ʱ�ȭ
                    LR.positionCount = 0;
                    attacker.Attackable = false;
                    // ���� �ڷ�ƾ ���� ����
                    GAME.IGM.AddAction(player.heroData.SkillCo(player.heroSkill, targetFounded));
                    // ���� ����Ŀ�� � �༮���� : �̴Ͼ� �Ϲݰ���, ������������, ������ų��� ���

                    Debug.Log("�浹ü �̸� : " + targetFounded.TR.name);
                    break;
                }
                else
                {
                    // ���α��� �׸��� �ڷ�ƾ �������� ���� �ʱ�ȭ
                    LR.positionCount = 0;
                    Debug.Log("�浹ü ã�� ����");
                    // Ÿ���� �߸� �Ͽ��⿡, �ٽ� ���ݻ��·� �ٲ��ְ� Ÿ���� ���� ����ϱ�
                    attacker.Attackable = true;
                    break;
                }
            }
            yield return null;
        }
        GAME.IGM.Turn.Col.enabled = attacker.Col.enabled = true;
        GAME.IGM.Hand.PlayerHand.ForEach(x => x.Col.enabled = true);
        yield break;
    }
    // �� ���� �ΰ��Ӿ� ���� + ���� �⺻���� ���� Ȯ�ν�
    // ������ Ŭ���̾�Ʈ�� ���ӳ� Ʈ������ ĵ������ ���İ��� ���̸鼭 ���� ���� ���� �ڷ�ƾ
    public IEnumerator StartIntro()
    {
        Debug.Log("t����");
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * 0.5f;
            transitionPanel.color = new Color(0,0,0,1-t);
            yield return null;
        }
        Debug.Log("��");
    }
}
