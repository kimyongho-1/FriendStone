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

    // 최종 시간제한 실행
    IEnumerator TimeStart()
    {
        // 시간이 얼마 없다는 영웅 대사 실행
        GAME.IGM.Hero.Player.HeroSaying(Define.Emotion.TimeLess);
        
        Vector3 downStart = new Vector3(0, -6f, 0);
        Vector3 downDest = new Vector3(0, -3f, 0);
        // 시작을 위해, 파티클 불투명도 1로 시작
        ColorOverLifetimeModule over = Flow.colorOverLifetime;
        gradient.alphaKeys = new GradientAlphaKey[]
            { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1f) };
        over.color = gradient;

        float t = 0;
        while (t < TimeLimit)
        {
            // 실제 경과 시간 t와  그것을 0~1 비율로 나타내는 ratio
            t += Time.deltaTime;
            float ratio = t / TimeLimit;

            // 시간이 지남에 따라, 위아래 모래알들의 수가 바뀌는 모습을 보여주기
            TimeTmp.text = ((int)(TimeLimit - t)).ToString();
            Up.transform.localScale = new Vector3(1, Mathf.Lerp(1, 0, ratio), 1);
            Down.transform.localPosition = Vector3.Lerp(downStart, downDest, ratio);

            // 컬러오버라이프타임에서의 최대 알파한도를 점차 낮춰서 전체적인 파티클의 투명도값을 떨어뜨리기
            gradient.alphaKeys = new GradientAlphaKey[]
            { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, (1 - ratio)) };
            over.color = gradient;

            yield return null;
        }

        // 끝났을경우, 알파값을 0으로 주어 파티클 안보이게 설정
        gradient.alphaKeys = new GradientAlphaKey[]
            { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(0.0f, 0f) };
        over.color = gradient;

        // 최종 시간제한까지 경과시에
        // 턴종료 버튼이 아직 눌리지 않았다면
        if (GAME.IGM.Turn.Col.enabled == true)
        {
            // 턴 제한인데도, 발견이벤트 진행중이었다면
            if (GAME.IGM.FindEvt.enabled == true)
            {
                Debug.Log("시간제한으로, 발견이벤트 강제 선택");
                // 강제로 왼쪽 설정
                GAME.IGM.FindEvt.ClickedCard(GAME.IGM.FindEvt.left.gameObject);
            }

            // 강제로 턴 종료 실행 및 전파
            GAME.IGM.Turn.ClickedOnTurnEnd(null);
        }
        
        this.gameObject.SetActive(false);
    }
}
