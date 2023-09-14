using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSkill : MonoBehaviour, IBody
{
    Hero owenrUser;
    Action<GameObject> func;
    # region
    public IEnumerator Do { get; set; }
    public bool IsMine { get; set; }
    public int PunId { get; set; }

    public Define.BodyType bodyType { get; set; }

    public Transform TR { get; set; }

    public Vector3 OriginPos { get; set; }
    public Collider2D Col { get; set; }
    public bool Ray { set { Col.enabled = value; } }

#endregion

    public void InitSkill(Hero user)
    {
        bodyType = Define.BodyType.HeroSkill;
        TR = transform;
        Col = Col.GetComponent<CircleCollider2D>();   

        owenrUser = user;   
        switch (GAME.Manager.RM.GameDeck.ownerClass)
        { 
            
        }

        // 스킬 아이콘 클릭시, 타겟팅 및 스킬 이벤트 실행 연결
        GAME.Manager.UM.BindEvent(this.gameObject, ClickedOnSkill, Define.Mouse.ClickL, Define.Sound.None);
    }

    public void ClickedOnSkill(GameObject go)
    {
        // 레이 비활성화 : 이미 사용 확인
        // 코루틴 사용중 : 현재 다른 드래깅클릭이 스킬컴포넌트를 클릭한 경우
        if (Col.enabled == false || GAME.Manager.IGM.TC.TargetCo != null)
        { return; }

        // 공격자 자신과, 스폰영역 레이 비활성화
        GAME.Manager.IGM.Spawn.SpawnRay = Ray = false;

        // 타겟팅 카메라 실행
        GAME.Manager.IGM.TC.StartTargetting(this);
    }


    public IEnumerator HJClick()
    {
        yield return null;
    }     
    public IEnumerator HZClick()
    { yield return null; }   
    public IEnumerator KHClick()
    { yield return null; }
}
