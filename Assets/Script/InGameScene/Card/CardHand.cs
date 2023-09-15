using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardHand : CardEle
{
    public Vector3  originRot, originScale;
    public int originOrder;
    public TextMeshPro cardName, Description, Stat, Type, Cost;
    public SpriteRenderer cardBackGround, cardImage;
    
    public void Awake()
    {
        originScale = 0.3f * Vector3.one;
        Col = GetComponent<BoxCollider2D>();
        
        // 엔터엑시트 이벤트 연결 => 카드팝업으로 보이게
        //GAME.Manager.UM.BindCardPopupEvent(this.gameObject, CardPopupEnter, 0.3f);
        GAME.Manager.UM.BindEvent(this.gameObject, Enter ,Define.Mouse.Enter, Define.Sound.None);
        GAME.Manager.UM.BindEvent(this.gameObject, Exit , Define.Mouse.Exit, Define.Sound.None);

        // 드래그
        GAME.Manager.UM.BindEvent(this.gameObject, StartDrag, Define.Mouse.StartDrag , Define.Sound.Pick);
        GAME.Manager.UM.BindEvent(this.gameObject, Dragging, Define.Mouse.Dragging);
        GAME.Manager.UM.BindEvent(this.gameObject, EndDrag, Define.Mouse.EndDrag);
    }
    public void Init(ref CardData dataParam)
    {
        data = dataParam;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = "";
        Type.text = data.cardType.ToString();
        Cost.text = data.cost.ToString();
        cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);
    }

    // TMP 소팅오더 정렬
    public void SetOrder(int i)
    {
        cardImage.sortingOrder = i * 10 - 1;
        cardBackGround.sortingOrder = i * 10;
        Description.sortingOrder =
        Stat.sortingOrder =
        Type.sortingOrder =
        Cost.sortingOrder=
        cardName.sortingOrder  = i * 10 + 1;
    }

    // 마우스 엔터시, 카드팝업 호출
    public void CardPopupEnter()
    {
        Vector3 pos = default;

        // 나의 소유, 적의 소유인지에 따라 또 위치 변경
        switch (data.cardType)
        {
            case Define.cardType.minion:
                pos = new Vector3(-3f, -1.3f, 0);
                break;
            case Define.cardType.spell:
                pos = new Vector3(-4f, -1.3f, 0);
                break;
            case Define.cardType.weapon:
                pos = new Vector3(-3f, -1.3f, 0);
                break;
        }

        // 카드팝업 호출 + 보여질 데이터와 위치값 함께
        GAME.Manager.IGM.ShowCardPopup(ref data, new Vector3(-5f, 0.8f, 0));
    }

    #region 마우스 이벤트
    public void Enter(GameObject go)
    {
        // 드래그 중일떄 취소
        if (DragCo != null) { return; }
        SetOrder(originOrder * 10);
        transform.localScale = originScale * 1.5f;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(transform.localPosition.x, -1.75f,-0.5f);
    }
    public void Exit(GameObject go)
    { 
        // 드래그 중일떄 취소
        if (DragCo != null) { return; }
        SetOrder(originOrder );
        transform.localScale = originScale;
        transform.localRotation = Quaternion.Euler(originRot);
        transform.localPosition = OriginPos;
    }

    IEnumerator DragCo = null;
    public void StartDrag(Vector3 v)
    {
        // 드래그 시작 : 현재 드래깅 객체 크기,회전등 초기화
        DragCo = BackToOrigin();
        StartCoroutine(DragCo);

        // 드래그 시작시, 확대와 회전등 초기화
        IEnumerator BackToOrigin()
        {
            // SR의 오더 강조시키기 (제일 앞에 그려지도록)
            SetOrder(1000);
            // 현재 드래그중인 카드 제외, 모든 카드 레이끄기
            GAME.Manager.IGM.Hand.PlayerHand.FindAll(x => x.PunId != this.PunId).ForEach(x => x.Ray = false);
            // 크기등 모두 초기화
            float t = 0;
            Vector3 currScale = transform.localScale;
            Vector3 currRot = transform.localRotation.eulerAngles;
            while (t < 1f)
            {
                t += Time.deltaTime  *2.5F;    
                transform.localScale = Vector3.Lerp(currScale , originScale, t);
                transform.localRotation = Quaternion.Euler(Vector3.Lerp(currRot, Vector3.zero , t));
                yield return null;
            }
        }

        // 드래그동안에 상하좌우등의 이동에 맞게 회전 코루틴 실행
        StartCoroutine(Rotate());
        IEnumerator Rotate()
        {
            while (true)
            {
               // Vector3 euler = transform.rotation.eulerAngles;
               //
               // if (euler.y > 180)
               // { euler.y -= 360f; }
               // if (euler.x > 180)
               // { euler.x -= 360f; }
                // 현재 회전값이 0.1 이하일시, 강제로 1로 벨류를 고정
                float angle = Quaternion.Angle(transform.localRotation, Quaternion.identity);
                float val = (angle < 0.1f) ? 1f  : 0.05f;
                transform.localRotation
                = Quaternion.Lerp(transform.localRotation, Quaternion.identity, val);
                yield return null;
            }
        }
    }
    public void Dragging(Vector3 worldPos)
    {
        Debug.Log(data.cardType); 
        GAME.Manager.IGM.Spawn.MinionAlignment(this, worldPos);
        if (data.cardType == Define.cardType.minion)
        {
           // GAME.Manager.IGM.Spawn.MinionAlignment(this, worldPos);
        }

        Vector3 euler = transform.rotation.eulerAngles;
        if (euler.y > 180)
        { euler.y -= 360f; }
        if (euler.x > 180)
        { euler.x -= 360f; }

        if ((transform.position.x - worldPos.x) > 0.01f)
        { euler.y = Mathf.Clamp(euler.y + 7f, -45f, 45f); }
        else if ((transform.position.x - worldPos.x) < -0.01f)
        { euler.y = Mathf.Clamp(euler.y - 7f, -45f, 45f); }
        
        if ((transform.position.y - worldPos.y) > 0.01f)
        { euler.x = Mathf.Clamp(euler.x - 4f, -25f, 25f); }
        else if ((transform.position.y - worldPos.y) < -0.01f)
        { euler.x = Mathf.Clamp(euler.x + 4f, -25f, 25f); }

        this.transform.localRotation = Quaternion.Euler(euler.x, euler.y, euler.z);
        this.transform.localPosition = worldPos;
    }

    // 카드 투명화로 소멸 코루틴 : 주로 카드 삭제 또는 미니언카드를 필드로 소환할떄 사용
    public IEnumerator FadeOutCo()
    {
        // 삭제전 핸드매니저에서 핸드카드들 재정렬 시작
        GAME.Manager.StartCoroutine(GAME.Manager.IGM.Hand.CardAllignment());

        // 투명화 위해 모든 TMP와 SR을 묶기
        List<TextMeshPro> tmpList = new List<TextMeshPro>() { cardName, Description, Cost, Stat ,Type};
        List<SpriteRenderer> imageList =    new List<SpriteRenderer>() { cardImage, cardBackGround};
        float t = 1;
        Color tempColor = Color.white;
        while (t > 0f)
        {
            // 알파값 점차 0으로 변환
            t -= Time.deltaTime * 2.5f;
            tempColor.a = t;
            tmpList.ForEach(x => x.alpha = t);
            imageList.ForEach(x => x.color = tempColor ) ;
            yield return null;
        }

        GameObject.Destroy(this.gameObject);
    }
    public void EndDrag(Vector3 v)
    {
        Ray = false; // Ray를 비활성화시 Exit가 호출되지만, DragCo를 뒤에서 null로 초기화하여서 Exit 먼저 실행을 막기
        StopAllCoroutines();
        if (GAME.Manager.IGM.Spawn.CheckInBox(
            new Vector2(this.transform.localPosition.x, this.transform.localPosition.y)))
        {
            // 현재 핸드목록에서 소환할 이 미니언카드 제거
            GAME.Manager.IGM.Hand.PlayerHand.Remove(this);
            // 미니언 소환 완료시, 모든 핸드카드 레이활성 초기화
            GAME.Manager.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);
            // 미니언 스폰 시작 (카드 소멸화 애니메이션 및 삭제는 StartSpawn내부에서 실행)
            GAME.Manager.IGM.Spawn.StartSpawn(this);
            return;
        }
        switch (data.cardType)
        {
            case Define.cardType.minion:
               
                break;
            case Define.cardType.spell:
                break;
            case Define.cardType.weapon:
                break;
        }
        GAME.Manager.IGM.Spawn.idx = -1000;
        // 원위치 코루틴 실행
        DragCo = BackToOrigin();
        StartCoroutine(DragCo);
        IEnumerator BackToOrigin()
        {
            // SR의 오더 초기화 원래대로
            SetOrder(originOrder);
            // 크기등 모두 초기화
            float t = 0;
            Vector3 currScale = transform.localScale;
            Vector3 currRot = transform.localRotation.eulerAngles;
            currRot.z = 0;
            Vector3 currPos = transform.localPosition;
            while (t < 1f)
            {
                t += Time.deltaTime * 5f;
                transform.localScale = Vector3.Lerp(currScale, originScale, t);
                transform.localRotation = Quaternion.Euler(Vector3.Lerp(currRot, originRot, t));
                transform.localPosition = Vector3.Lerp(currPos, OriginPos, t);
                yield return null;
            }
            GAME.Manager.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);

            DragCo = null;
        }
    }
    #endregion
}
