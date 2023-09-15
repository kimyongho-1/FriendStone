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
        
        // ���Ϳ���Ʈ �̺�Ʈ ���� => ī���˾����� ���̰�
        //GAME.Manager.UM.BindCardPopupEvent(this.gameObject, CardPopupEnter, 0.3f);
        GAME.Manager.UM.BindEvent(this.gameObject, Enter ,Define.Mouse.Enter, Define.Sound.None);
        GAME.Manager.UM.BindEvent(this.gameObject, Exit , Define.Mouse.Exit, Define.Sound.None);

        // �巡��
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

    // TMP ���ÿ��� ����
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

    // ���콺 ���ͽ�, ī���˾� ȣ��
    public void CardPopupEnter()
    {
        Vector3 pos = default;

        // ���� ����, ���� ���������� ���� �� ��ġ ����
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

        // ī���˾� ȣ�� + ������ �����Ϳ� ��ġ�� �Բ�
        GAME.Manager.IGM.ShowCardPopup(ref data, new Vector3(-5f, 0.8f, 0));
    }

    #region ���콺 �̺�Ʈ
    public void Enter(GameObject go)
    {
        // �巡�� ���ϋ� ���
        if (DragCo != null) { return; }
        SetOrder(originOrder * 10);
        transform.localScale = originScale * 1.5f;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(transform.localPosition.x, -1.75f,-0.5f);
    }
    public void Exit(GameObject go)
    { 
        // �巡�� ���ϋ� ���
        if (DragCo != null) { return; }
        SetOrder(originOrder );
        transform.localScale = originScale;
        transform.localRotation = Quaternion.Euler(originRot);
        transform.localPosition = OriginPos;
    }

    IEnumerator DragCo = null;
    public void StartDrag(Vector3 v)
    {
        // �巡�� ���� : ���� �巡�� ��ü ũ��,ȸ���� �ʱ�ȭ
        DragCo = BackToOrigin();
        StartCoroutine(DragCo);

        // �巡�� ���۽�, Ȯ��� ȸ���� �ʱ�ȭ
        IEnumerator BackToOrigin()
        {
            // SR�� ���� ������Ű�� (���� �տ� �׷�������)
            SetOrder(1000);
            // ���� �巡������ ī�� ����, ��� ī�� ���̲���
            GAME.Manager.IGM.Hand.PlayerHand.FindAll(x => x.PunId != this.PunId).ForEach(x => x.Ray = false);
            // ũ��� ��� �ʱ�ȭ
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

        // �巡�׵��ȿ� �����¿���� �̵��� �°� ȸ�� �ڷ�ƾ ����
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
                // ���� ȸ������ 0.1 �����Ͻ�, ������ 1�� ������ ����
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

    // ī�� ����ȭ�� �Ҹ� �ڷ�ƾ : �ַ� ī�� ���� �Ǵ� �̴Ͼ�ī�带 �ʵ�� ��ȯ�ҋ� ���
    public IEnumerator FadeOutCo()
    {
        // ������ �ڵ�Ŵ������� �ڵ�ī��� ������ ����
        GAME.Manager.StartCoroutine(GAME.Manager.IGM.Hand.CardAllignment());

        // ����ȭ ���� ��� TMP�� SR�� ����
        List<TextMeshPro> tmpList = new List<TextMeshPro>() { cardName, Description, Cost, Stat ,Type};
        List<SpriteRenderer> imageList =    new List<SpriteRenderer>() { cardImage, cardBackGround};
        float t = 1;
        Color tempColor = Color.white;
        while (t > 0f)
        {
            // ���İ� ���� 0���� ��ȯ
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
        Ray = false; // Ray�� ��Ȱ��ȭ�� Exit�� ȣ�������, DragCo�� �ڿ��� null�� �ʱ�ȭ�Ͽ��� Exit ���� ������ ����
        StopAllCoroutines();
        if (GAME.Manager.IGM.Spawn.CheckInBox(
            new Vector2(this.transform.localPosition.x, this.transform.localPosition.y)))
        {
            // ���� �ڵ��Ͽ��� ��ȯ�� �� �̴Ͼ�ī�� ����
            GAME.Manager.IGM.Hand.PlayerHand.Remove(this);
            // �̴Ͼ� ��ȯ �Ϸ��, ��� �ڵ�ī�� ����Ȱ�� �ʱ�ȭ
            GAME.Manager.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);
            // �̴Ͼ� ���� ���� (ī�� �Ҹ�ȭ �ִϸ��̼� �� ������ StartSpawn���ο��� ����)
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
        // ����ġ �ڷ�ƾ ����
        DragCo = BackToOrigin();
        StartCoroutine(DragCo);
        IEnumerator BackToOrigin()
        {
            // SR�� ���� �ʱ�ȭ �������
            SetOrder(originOrder);
            // ũ��� ��� �ʱ�ȭ
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
