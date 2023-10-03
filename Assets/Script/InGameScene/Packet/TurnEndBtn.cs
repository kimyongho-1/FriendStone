using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class TurnEndBtn : MonoBehaviour
{
    public Collider2D Col;
    public TextMeshPro btnTmp;
    public TextMeshProUGUI infoTmp;
    private void Awake()
    {
        Col.enabled = false;
        btnTmp.text = "��� ��";
        GAME.IGM.Turn = this;
        GAME.Manager.UM.BindEvent( this.gameObject , ClickedOnTurnEnd , Define.Mouse.ClickL, Define.Sound.Click );
    }

    // ������ ��ư ������ ȣ��
    void ClickedOnTurnEnd(GameObject go)
    {
        Col.enabled = false;
        btnTmp.text = "��� ��";
        Debug.Log("TurnEnd!!");

        // �� ���� ���� ���� (���� �ȳ��� �ൿ�� �������� �ֱ⿡)
        GAME.IGM.AddAction(EndMyTurn());
    }

    // ������ ��ư ������, ��� ��ϵ� �Լ� �����Ŀ� ������ ����
    public IEnumerator EndMyTurn()
    {
        // ��� �ڵ� ��ġ ũ�� ���� �ʱ�ȭ
        GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = false);

        // ������ Exit�Լ��� �ʱ�ȭ ����
        GAME.IGM.Hand.PlayerHand.ForEach(x => x.Exit(null));

        // �� �� ����
        GAME.IGM.Packet.isMyTurn = false;
        // ��뿡�� �� �� ���� ����
        GAME.IGM.Packet.SendTurnEnd();
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
        // UI �۾� ���� ����
        while (t > 0)
        {
            t -= Time.deltaTime * 3f;
            c.a = t;
            infoTmp.color = c;
            yield return null;
        }
    }

    // ����� ������ �̺�Ʈ ������, ���� �� ����
    public IEnumerator StartMyTurn()
    {
        // �� �� ���� �˸� �ؽ�Ʈ ����
        StartCoroutine(ShowTurnMSG(true));
        // �� �� ����
        GAME.IGM.Packet.isMyTurn = true;
        // ����� ȭ�鿡 �� �� ���� ���� �̺�Ʈ ����
        GAME.IGM.Packet.SendMyTurnMSG();

        // �� ��ο� ����
        yield return StartCoroutine(GAME.IGM.Hand.CardDrawing(1));

        // ���� ���� �� ����, ���� �ʱ�ȭ (�ִ�ġ�� 10, ������ ���� ���������� �ϳ��� �ִ� ���� ���� like �Ͻ�����)
        GAME.IGM.Hero.Player.mp = Mathf.Clamp(GAME.IGM.GameTurn, 0, 10);


        // ������ ��ư ������ �ֵ��� ���� Ȱ��ȭ
        Col.enabled = true;


    }
}
