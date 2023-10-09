using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FatigueIcon : MonoBehaviour
{
    WaitForSeconds fatigueHoldTime = new WaitForSeconds(1f);
    public TextMeshPro ownerTmp, infoTmp;

    // 탈진 이벤트 코루틴
    public IEnumerator DeckExhausted(bool PlayerFatigue, int dmg)
    {
        // 탈진 이벤트 시작
        // 먼저 현재 탈진피해량을 tmp에 적용 
        ownerTmp.text = $"{((PlayerFatigue == true) ? "당신":"상대")}";
        infoTmp.text = $"<size=7.5><color=black>덱에 카드가 없어\n<color=black>피해를 <color=red><size=11>{dmg}<size=7.5> <color=black>받습니다.\n<color=red><size=12>점.점.증.가";

        // 나의것이 아니라면, 이벤트 전달할 필요 X
        if (PlayerFatigue  == true)
        {
            // 상대에게 현재 내 탈진피해 이벤트 전파하여 동기화 [현재 내 탈진 스택을 인자로 전해 동기화]
            GAME.IGM.Packet.SendFatigue(dmg);
        }

        // 탈진 카드의 시작, 중간경유지, 최종 위치를 실행영웅 소유여부로 결정 및 실행
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

        // 유저에게 보여지도록 잠시 대기
        yield return fatigueHoldTime;

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
