using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FatigueIcon : MonoBehaviour
{
    WaitForSeconds fatigueHoldTime = new WaitForSeconds(1f);
    public TextMeshPro ownerTmp, infoTmp;

    // Ż�� �̺�Ʈ �ڷ�ƾ
    public IEnumerator DeckExhausted(bool PlayerFatigue, int dmg)
    {
        // Ż�� �̺�Ʈ ����
        // ���� ���� Ż�����ط��� tmp�� ���� 
        ownerTmp.text = $"{((PlayerFatigue == true) ? "���":"���")}";
        infoTmp.text = $"<size=7.5><color=black>���� ī�尡 ����\n<color=black>���ظ� <color=red><size=11>{dmg}<size=7.5> <color=black>�޽��ϴ�.\n<color=red><size=12>��.��.��.��";

        // ���ǰ��� �ƴ϶��, �̺�Ʈ ������ �ʿ� X
        if (PlayerFatigue  == true)
        {
            // ��뿡�� ���� �� Ż������ �̺�Ʈ �����Ͽ� ����ȭ [���� �� Ż�� ������ ���ڷ� ���� ����ȭ]
            GAME.IGM.Packet.SendFatigue(dmg);
        }

        // Ż�� ī���� ����, �߰�������, ���� ��ġ�� ���࿵�� �������η� ���� �� ����
        Vector3 Start = new Vector3(9f,(PlayerFatigue == true) ? -0.4f : 2.4f, -0.5f);
        Vector3 Center = new Vector3(0.5f, (PlayerFatigue == true) ? -0.4f : 2.4f, -0.5f);
        Vector3 Dest = new Vector3(-8.5f, (PlayerFatigue == true) ? -0.4f : 2.4f, -0.5f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            this.transform.position =
                Vector3.Lerp(Start, Center , t);
            yield return null;
        }

        // �������� ���������� ��� ���
        yield return fatigueHoldTime;

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
