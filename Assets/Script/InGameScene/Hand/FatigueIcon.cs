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

    // 탈진 코루틴의 이후 진행할지 여부
    public void ChangeResume()
    { resume = false; }

    // 국자가 머리에 닿을떄 카메라 흔들기
    public void HeadAche()
    {
        // 꽈앙 소리 재생
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
                // 사인그래프의 0 ~ 2/3π 범위까지만
                float theta = t * (2f / 3f) * Mathf.PI;
                float scaleValue = 0.3f * Mathf.Sin(theta * 1.5f);

                dmgTmp.transform.localScale = Vector3.one * scaleValue;
                yield return null;
            }
        }
    }

    // 탈진 이벤트 코루틴
    public IEnumerator DeckExhausted(bool PlayerFatigue, int dmg)
    {
        dmgTmp.transform.localScale = Vector3.zero;
        resume = true;
        // 탈진 이벤트 시작
        // 탈진피해량을 tmp에 적용 
        ownerTmp.text = $"{((PlayerFatigue == true) ? "당신":"상대")}";
        infoTmp.text = $"<size=7.5><color=black>덱에 카드가 없어\n<color=black>피해를 <color=red><size=11>{dmg}<size=7.5> <color=black>받습니다.\n<color=red><size=12>점.점.증.가";

        // 나의것이 아니라면, 이벤트 전달할 필요 X
        if (PlayerFatigue  == true)
        {
            // 상대에게 현재 내 탈진피해 이벤트 전파하여 동기화 [현재 내 탈진 스택을 인자로 전해 동기화]
            GAME.IGM.Packet.SendFatigue(dmg);
        }

        // 탈진 카드의 시작, 중간경유지, 최종 위치를 실행영웅 소유여부로 결정 및 실행
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



        #region 피해계산과 피해텍스트 표시
        // 애니메이터에서 재생 및 텍스트 표시
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
        // 유저에게 보여지도록 잠시 대기 (resume은 bonkPivot 애니메이터에서 true로 설정 )
        yield return new WaitUntil(() => (resume == false));
        // 피해 계산
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



        // 다시 복귀위치로
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
