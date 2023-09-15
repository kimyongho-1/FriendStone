using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEngine.GraphicsBuffer;
using UnityEditor.Experimental.GraphView;

public class CardField : CardEle
{
    public TextMeshPro AttTmp, HpTmp;
    public SpriteRenderer  cardImage;
    public bool attackable = true;
    public ParticleSystem sleep;
    
    public void Init(CardData dataParam)
    {
        attackable = true;
        Col = GetComponent<CircleCollider2D>();
        if (dataParam is MinionCardData)
        {
            MinionCardData minionCardData = (MinionCardData)dataParam;
            data = dataParam;
            AttTmp.text = minionCardData.att.ToString();
            HpTmp.text = minionCardData.hp.ToString();
            cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);
        }
        else
        {
            data = dataParam;
            cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);
        }

        // 좌클릭시 카메라로 레이활성화하여 타겟팅 이벤트 시작
        GAME.Manager.UM.BindEvent(this.gameObject, StartAttack , Define.Mouse.ClickL, Define.Sound.Ready );
    }

    // 미니언 공격시도 함수 (좌클릭 마우스 이벤트)
    public void StartAttack(GameObject go)
    {
        // 공격 가능한 상태가 아니거나
        // 이미 다른 객체가 타겟팅 중인데 이 객체를 클릭시 취소
        if (!attackable || GAME.Manager.IGM.TC.TargetCo != null)
        { return; }

        // 공격자 자신과, 스폰영역 레이 비활성화
        GAME.Manager.IGM.Spawn.SpawnRay = Ray = false;

        // 현재 타겟팅 코루틴 등록 및 실행
        GAME.Manager.IGM.TC.TargetCo = GAME.Manager.IGM.TC.TargettingCo
            (this, (IBody a, IBody t) => { return AttackCo(a, t); });

        // 타겟팅 카메라 실행 + 만약 타겟팅 성공시 공격함수 예약 실행
        GAME.Manager.IGM.TC.StartCoroutine(GAME.Manager.IGM.TC.TargetCo);
    }

    public IEnumerator AttackCo(IBody attacker, IBody target)
    {
        yield return null;
    }
}