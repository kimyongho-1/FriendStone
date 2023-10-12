using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FatigueIcon : MonoBehaviour
{
    public Animator bonkAnim;
    public TextMeshPro ownerTmp, infoTmp, dmgTmp;
    public GameObject bonkPivot;
    public SpriteRenderer bonk;
    public bool resume = true;
    AudioSource sound;

    private void Awake()
    {
        sound = GetComponent<AudioSource>();    
    }

    // Ż�� �ڷ�ƾ�� ���� �������� ����
    public void ChangeResume()
    { resume = false; }

    // ���ڰ� �Ӹ��� ������ ī�޶� ����
    public void HeadAche()
    {
        // �ʾ� �Ҹ� ���
        sound.Play();
        StartCoroutine(GAME.IGM.TC.ShakeCo());
        StartCoroutine(ShowDmg());
        IEnumerator ShowDmg()
        {
            float t = 0;
            Vector3 dest = Vector3.one * 0.15f;
            while (t < 1f)
            { 
                t += Time.deltaTime;
                // ���α׷����� 0 ~ 2/3�� ����������
                float theta = t * (2f / 3f) * Mathf.PI;
                float scaleValue = 0.3f * Mathf.Sin(theta * 1.5f);

                dmgTmp.transform.localScale = Vector3.one * scaleValue;
                yield return null;
            }
        }
    }

    // Ż�� �̺�Ʈ �ڷ�ƾ
    public IEnumerator DeckExhausted(bool PlayerFatigue, int dmg)
    {
        dmgTmp.transform.localScale = Vector3.zero;
        resume = true;
        // Ż�� �̺�Ʈ ����
        // Ż�����ط��� tmp�� ���� 
        ownerTmp.text = $"{((PlayerFatigue == true) ? "���":"���")}";
        infoTmp.text = $"<size=7.5><color=black>���� ī�尡 ����\n<color=black>���ظ� <color=red><size=11>{dmg}<size=7.5> <color=black>�޽��ϴ�.\n<color=red><size=12>��.��.��.��";

        // ���ǰ��� �ƴ϶��, �̺�Ʈ ������ �ʿ� X
        if (PlayerFatigue  == true)
        {
            // ��뿡�� ���� �� Ż������ �̺�Ʈ �����Ͽ� ����ȭ [���� �� Ż�� ������ ���ڷ� ���� ����ȭ]
            GAME.IGM.Packet.SendFatigue(dmg);
        }

        // Ż�� ī���� ����, �߰�������, ���� ��ġ�� ���࿵�� �������η� ���� �� ����
        Vector3 Start = new Vector3(9f,   1f , -0.5f);
        Vector3 Center = new Vector3(-2f, 1f , -0.5f);
        Vector3 Dest = new Vector3(-9f,  1f , -0.5f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            this.transform.position =
                Vector3.Lerp(Start, Center , t);
            yield return null;
        }



        #region ���ذ��� �����ؽ�Ʈ ǥ��
        // �ִϸ����Ϳ��� ��� �� �ؽ�Ʈ ǥ��
        dmgTmp.text = "-"+dmg.ToString();
        if (PlayerFatigue)
        {
            dmgTmp.transform.localPosition = new Vector3(1.8f, -0.7f, 0);
            bonkAnim.Play("PlayerBonk", 0);
        }
        else
        {
            dmgTmp.transform.localPosition = new Vector3(1.8f, 0.7f, 0);
            bonkAnim.Play("EnemyBonk", 0);
        }
        // �������� ���������� ��� ��� (resume�� bonkPivot �ִϸ����Ϳ��� true�� ���� )
        yield return new WaitUntil(() => (resume == false));
        // ���� ���
        if (PlayerFatigue == true)
        {
            GAME.IGM.Hero.Player.HP -= dmg;
            if (GAME.IGM.Hero.Player.HP <= 0)
            { yield return StartCoroutine(GAME.IGM.Hero.Player.onDead); }
        }
        else
        {
            GAME.IGM.Hero.Enemy.HP -= dmg;
            if (GAME.IGM.Hero.Enemy.HP <= 0)
            { yield return StartCoroutine(GAME.IGM.Hero.Enemy.onDead); }
        }
        #endregion



        // �ٽ� ������ġ��
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            this.transform.position =
                Vector3.Lerp(Center, Dest, t);
            yield return null;
        }

    }
   
}
