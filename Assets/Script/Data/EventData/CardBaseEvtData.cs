using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardBaseEvtData 
{
    public Define.evtWhen when; // 이벤트가 발동할 순간
    public Define.evtArea area; // 자동 실행건인지 여부 ( NONE ), 아니라면 유저가 고를 영역 범위 선택
    public Define.evtType type; // 어떤 이벤트인지 : 치료 공격 드로우..등등
}
