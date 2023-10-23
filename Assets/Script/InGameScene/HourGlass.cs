using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class HourGlass : MonoBehaviour
{
    public float TimeLimit;
    public TextMeshPro TimeTmp;
    public SpriteRenderer Up, Down;
    public ParticleSystem Flow;
    Gradient gradient = new Gradient();
    private void OnDisable()
    {
        TimeTmp.text = TimeLimit.ToString();
    }
    public void StartTimeLimit()
    {
        TimeTmp.text = TimeLimit.ToString();
        this.gameObject.SetActive(true);
        StartCoroutine(TimeStart());
    }

    // ���� �ð����� ����
    IEnumerator TimeStart()
    {
        // �ð��� �� ���ٴ� ���� ��� ����
        GAME.IGM.Hero.Player.HeroSaying(Define.Emotion.TimeLess);
        
        Vector3 downStart = new Vector3(0, -6f, 0);
        Vector3 downDest = new Vector3(0, -3f, 0);
        // ������ ����, ��ƼŬ ������ 1�� ����
        ColorOverLifetimeModule over = Flow.colorOverLifetime;
        gradient.alphaKeys = new GradientAlphaKey[]
            { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1f) };
        over.color = gradient;

        float t = 0;
        while (t < TimeLimit)
        {
            // ���� ��� �ð� t��  �װ��� 0~1 ������ ��Ÿ���� ratio
            t += Time.deltaTime;
            float ratio = t / TimeLimit;

            // �ð��� ������ ����, ���Ʒ� �𷡾˵��� ���� �ٲ�� ����� �����ֱ�
            TimeTmp.text = ((int)(TimeLimit - t)).ToString();
            Up.transform.localScale = new Vector3(1, Mathf.Lerp(1, 0, ratio), 1);
            Down.transform.localPosition = Vector3.Lerp(downStart, downDest, ratio);

            // �÷�����������Ÿ�ӿ����� �ִ� �����ѵ��� ���� ���缭 ��ü���� ��ƼŬ�� �������� ����߸���
            gradient.alphaKeys = new GradientAlphaKey[]
            { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, (1 - ratio)) };
            over.color = gradient;

            yield return null;
        }

        // ���������, ���İ��� 0���� �־� ��ƼŬ �Ⱥ��̰� ����
        gradient.alphaKeys = new GradientAlphaKey[]
            { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(0.0f, 0f) };
        over.color = gradient;

        // ���� �ð����ѱ��� ����ÿ�
        // ������ ��ư�� ���� ������ �ʾҴٸ�
        if (GAME.IGM.Turn.Col.enabled == true)
        {
            // �� �����ε���, �߰��̺�Ʈ �������̾��ٸ�
            if (GAME.IGM.FindEvt.enabled == true)
            {
                Debug.Log("�ð���������, �߰��̺�Ʈ ���� ����");
                // ������ ���� ����
                GAME.IGM.FindEvt.ClickedCard(GAME.IGM.FindEvt.left.gameObject);
            }

            // ������ �� ���� ���� �� ����
            GAME.IGM.Turn.ClickedOnTurnEnd(null);
        }
        
        this.gameObject.SetActive(false);
    }
}
