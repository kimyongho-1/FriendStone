using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class TurnEndBtn : MonoBehaviour
{
    public Collider2D Col;
    public TextMeshPro btnTmp;
    public TextMeshProUGUI infoTmp;
    public float baseTimeLimit = 10;
    public Material blinkMat;
    AudioSource audioPlayer;
    private void Awake()
    {
        audioPlayer = GetComponent<AudioSource>();
        Col.enabled = false;
        btnTmp.text = "��� ��";
        GAME.IGM.Turn = this;
        GAME.Manager.UM.BindEvent( this.gameObject , ClickedOnTurnEnd , Define.Mouse.ClickL);
    }

    // ������ ��ư ������ ȣ��
    public void ClickedOnTurnEnd(GameObject go)
    {
        // �� ��ư Ŭ���� ���
        audioPlayer.clip = GAME.IGM.GetClip(Define.IGMsound.ClickTurnBtn);
        audioPlayer.Play();

        // �ð��ӹ� Ÿ�̸����̾��ٸ� ���� ����
        if (GAME.IGM.TimeLimiter.gameObject.activeSelf == true)
        { GAME.IGM.TimeLimiter.gameObject.SetActive(false); }

        Col.enabled = false;
        btnTmp.text = "��� ��";
        blinkMat.SetColor("_Color", new Color(1,0,0,1) );
        // �� �ð� ���߱�
        if (turnTimer != null)
        {
            StopCoroutine(turnTimer);
            turnTimer = null;
        }
        Debug.Log("TurnEnd!!");

        // ������ Exit�Լ��� �ʱ�ȭ ����
        GAME.IGM.Hand.PlayerHand.ForEach(x => x.rewindHand());

        // �� ���� ���� ���� (���� �ȳ��� �ൿ�� �������� �ֱ⿡)
        GAME.IGM.AddAction(EndMyTurn());
    }

    // ������ ��ư ������, ��� ��ϵ� �Լ� �����Ŀ� ������ ����
    public IEnumerator EndMyTurn()
    {
        // �ʵ� �ϼ��� ��� ��������
        GAME.IGM.Spawn.playerMinions.ForEach(x => x.Attackable = false);
        // �� �� ����
        GAME.IGM.Packet.isMyTurn = false;
        // ��뿡�� �� �� ���� ����
        GAME.IGM.Packet.SendTurnEnd();

        for (int i = 0; i < GAME.IGM.Spawn.enemyMinions.Count; i++)
        {
            GAME.IGM.Spawn.enemyMinions[i].Attackable = true;
            GAME.IGM.Spawn.enemyMinions[i].sleep.gameObject.SetActive(false);
        }

        yield break;
    }

    // ������ ���� ���۵��� �˸��� �ؽ�Ʈ�ִ� �ڷ�ƾ
    public IEnumerator ShowTurnMSG(bool isMyTurn)
    {
        // �ؽ�Ʈ ǥ��
         btnTmp.text = (isMyTurn) ? "���� ��" : "��� ��";
        infoTmp.text = (isMyTurn) ? "���� ����" : "����� ����";
        infoTmp.color = new Color(1, 1, 1, 0);
        infoTmp.gameObject.SetActive(true);
        float t = 0;
        Color c = Color.white;
        c.a = 0;
        // UI �۾� ���� ����
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            c.a = t;
            infoTmp.color = c;
            yield return null;
        }

        // �� �� �����̸�, �� �� ������ ���
        if (isMyTurn)
        {
            // �� ���� ȿ���� ���
            audioPlayer.clip = GAME.IGM.GetClip(Define.IGMsound.TurnStart);
            audioPlayer.Play();
        }
        
        // UI �۾� ���� ����
        while (t > 0)
        {
            t -= Time.deltaTime * 3f;
            c.a = t;
            infoTmp.color = c;
            yield return null;
        }

        if (isMyTurn) // �������� ���۵ɋ����� ������ ���ݱǰ� ��ų���ݱ� �ʱ�ȭ
        {
            // ������ ���ݻ��� �ʱ�ȭ
            GAME.IGM.Hero.Player.heroSkill.Attackable = GAME.IGM.Hero.Player.Attackable = true;
        }
        else // ��뵵 ����
        {
            // ����� �ʱ�ȭ
            GAME.IGM.Hero.Enemy.heroSkill.Attackable = true;
        }
    }

    // ����� ������ �̺�Ʈ ������, ���� �� ����
    public IEnumerator StartMyTurn()
    {
        // �� �� ���� �˸� �ؽ�Ʈ ����
        StartCoroutine(ShowTurnMSG(true));
        // ������ ��ư ���׸��� ���� �� ���ϋ��� �ʱ�ȭ
        blinkMat.SetColor("_Color", new Color(0, 1, 0, 1));
        for (int i = 0; i < GAME.IGM.Spawn.playerMinions.Count; i++)
        {
            GAME.IGM.Spawn.playerMinions[i].Attackable = true;
            GAME.IGM.Spawn.playerMinions[i].sleep.gameObject.SetActive(false);
        }
        // ����� ȭ�鿡 �� �� ���� ���� �̺�Ʈ ����
        GAME.IGM.Packet.SendMyTurnMSG();

        // ���� ���� �� ����, ���� �ʱ�ȭ (�ִ�ġ�� 10, ������ ���� ���������� �ϳ��� �ִ� ���� ���� like �Ͻ�����)
        GAME.IGM.Hero.Player.MP = Mathf.Min(10, GAME.IGM.GameTurn);

        // �� ��ο� ����
        yield return StartCoroutine(GAME.IGM.Hand.CardDrawing(1));
        // �� �� ����
        GAME.IGM.Packet.isMyTurn = true;
        // ������ ��ư ������ �ֵ��� ���� Ȱ��ȭ
        Col.enabled = true;
        // �� Ÿ�̸� ����
        turnTimer = UserTurnTimer();
        StartCoroutine(turnTimer);
    }
    IEnumerator turnTimer;
    // ���� ���� ���۵ɋ����� ����Ǵ� ��Ÿ�̸�
    public IEnumerator UserTurnTimer()
    {
        // 30�ʰ� ������ �� ���� �ð� ���
        float startTime = Time.time; // ��Ȯ�� �ð������� ���� time���
        while ((Time.time - startTime) < baseTimeLimit)
        {
            yield return null;
        }

        // �ð��� ���� ���� ��� ����
        GAME.IGM.Hero.Player.HeroSaying(Define.Emotion.TimeLimitStart);

        startTime = Time.time;
        while ((Time.time - startTime) < baseTimeLimit)
        {
            yield return null;
        }

        // �� �ð������� ��������, �����ᰡ ������ �ʾҴٸ�
        if (Col.enabled == true)
        {
            GAME.IGM.TimeLimiter.StartTimeLimit();
        }

        turnTimer = null;
    }
}
