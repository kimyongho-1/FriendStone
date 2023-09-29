using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardBaseEvtData 
{
    public Define.evtWhen when; // 이벤트가 발동할 순간 : 카드를 손에서 낼떄, 미니언카드라면 죽을떄.. 등등
    public Define.evtTargeting targeting; // 손에서 내면 자동 실행인지, 선택해야하는지
    public Define.evtArea area; // 어느 진영이 대상인지 : 유저 | 적 | 모두
    public Define.evtFaction faction; // 진영의 어떤 타입 : 영웅만 | 미니언 | 모두

    public Define.evtType type; // 어떤 이벤트인지 : 치료 공격 드로우..등등
}
