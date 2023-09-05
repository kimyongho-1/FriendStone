using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UtillHandler : CardBaseEvtData
{
    public Define.utillType utillType; // 어떤 편의이벤트인지
    public int[] relatedCards = new int[] { }; // 획득하거나 발견 카드들의 고유번호 등록후, 후에 번호 통해서 찾아 사용
    public int utillAmount; // 기타 또는 드로우 수치
}
