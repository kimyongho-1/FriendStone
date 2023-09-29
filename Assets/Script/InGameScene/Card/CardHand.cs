using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.ComponentModel;
using System;

public class CardHand : CardEle
{
    public Vector3  originRot, originScale;
    public int originOrder, originCost;
    public TextMeshPro cardName, Description, Stat, Type, Cost;
    public SpriteRenderer cardBackGround, cardImage;
    public GameObject TMPgo;
    public SpellCardData spellCardData { get; set; }

    // OnHand�̺�Ʈ�� �����, Ư���������� �̺�Ʈ�� ���� (�̺�Ʈ ������ BattleManager.cs���� ���)
    public Action<int,bool> HandCardChanged;

    public void Awake()
    {
        originScale = 0.3f * Vector3.one;
        Col = GetComponent<BoxCollider2D>();
    }
    public void Init(CardData dataParam , bool isMine = true)
    {
        IsMine = isMine;
        cardBackGround.sprite = GAME.Manager.RM.GetCardSprite(IsMine);
        originScale = Vector3.one * 0.3f;
        transform.localScale = originScale;

        // ���� ī���� ��� ������ �ʱ�ȭ
        if (IsMine)
        {
            data = dataParam;
            cardName.text = data.cardName;
            Description.text = data.cardDescription;
            Description.fontSize = (data.cardDescription.Length > 25) ? 18 : 15;
            // ī�� ��͵� ǥ��
            switch (data.cardRarity)
            {
                case Define.cardRarity.rare:
                    Type.text = "<color=blue>���"; break;
                case Define.cardRarity.legend:
                    Type.text = "<color=red>����"; break;
                default: Type.text = "<color=black>�Ϲ�"; break;
            }
            // ī�� Ÿ�� ǥ��
            switch (data.cardType)
            {
                case Define.cardType.minion:
                    MinionCardData cardData = data as MinionCardData;
                    Stat.text = $"<color=green>ATT {cardData.att} <color=red>HP {cardData.hp} <color=black>����";
                    OriginAtt = cardData.att; OriginHp = cardData.hp;
                    break;
                case Define.cardType.spell:
                    Stat.text = "<color=black>�ֹ�";
                    break;
                case Define.cardType.weapon:
                    WeaponCardData wData = (WeaponCardData)data;
                    Stat.text = $"<color=green>ATT {wData.att} <color=red>dur {wData.durability} <color=black>����";
                    OriginAtt = wData.att; OriginHp = wData.durability;
                    break;
            }
            originCost = data.cost;
            Cost.text = originCost.ToString();
            cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);

            // �巡�� �̺�Ʈ ����
            GAME.Manager.UM.BindEvent(this.gameObject, StartDrag, Define.Mouse.StartDrag, Define.Sound.Pick);
            GAME.Manager.UM.BindEvent(this.gameObject, Dragging, Define.Mouse.Dragging);
            GAME.Manager.UM.BindEvent(this.gameObject, EndDrag, Define.Mouse.EndDrag);
            // Ŀ�� ������ ��� �̺�Ʈ
            GAME.Manager.UM.BindEvent(this.gameObject, Enter, Define.Mouse.Enter, Define.Sound.None);
            GAME.Manager.UM.BindEvent(this.gameObject, Exit, Define.Mouse.Exit, Define.Sound.None);

            // �ֹ�ī���� �׿� �°� ���ε� ( �̴Ͼ�ī��� ����ī��� ������ �ʵ忡��ȯ �ϰų� ����� ����Ÿ ĳ����)
            if (dataParam is SpellCardData)
            {
                spellCardData = (SpellCardData)dataParam;
            }
        }

        // ��� ī���� ���α�
        else
        {
            cardImage.gameObject.SetActive(false);
            TMPgo.gameObject.SetActive(false);  
        }


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
            // ���� �巡������ ī�� ����, ��� ī�� ���̲��� (Ÿ �ڵ�ī���� ���� ����Ʈ ���� �̺�Ʈ �ߺ� ����)
            GAME.IGM.Hand.PlayerHand.FindAll(x => x.PunId != this.PunId).ForEach(x => x.Ray = false);
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
        GAME.IGM.Spawn.MinionAlignment(this, worldPos);
        if (data.cardType == Define.cardType.minion)
        {
           // GAME.IGM.Spawn.MinionAlignment(this, worldPos);
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
    public IEnumerator FadeOutCo(bool isMine = true)
    {
        if (IsMine)
        {
            // ����ȭ ���� ��� TMP�� SR�� ����
            List<TextMeshPro> tmpList = new List<TextMeshPro>() { cardName, Description, Cost, Stat, Type };
            List<SpriteRenderer> imageList = new List<SpriteRenderer>() { cardImage, cardBackGround };
            float t = 1;
            Color tempColor = Color.white;
            while (t > 0f)
            {
                // ���İ� ���� 0���� ��ȯ
                t -= Time.deltaTime * 2.5f;
                tempColor.a = t;
                tmpList.ForEach(x => x.alpha = t);
                imageList.ForEach(x => x.color = tempColor);
                yield return null;
            }

        }

        // ���� �ڵ�ī�� ��� ��׶��� �̹����� ����
        else
        {
            // ����ȭ ����  ��׶��� �̹����� ã��
            float t = 1;
            Color tempColor = Color.white;
            while (t > 0f)
            {
                // ���İ� ���� 0���� ��ȯ
                t -= Time.deltaTime * 2.5f;
                tempColor.a = t;
                cardBackGround.color = tempColor;
                yield return null;
            }
        }
        GameObject.Destroy(this.gameObject);
    }
    public void EndDrag(Vector3 v)
    {
        Ray = false; // Ray�� ��Ȱ��ȭ�� Exit�� ȣ�������, DragCo�� �ڿ��� null�� �ʱ�ȭ�Ͽ��� Exit ���� ������ ����
        StopAllCoroutines();
     
        switch (data.cardType)
        {
            case Define.cardType.minion:
                if (GAME.IGM.Spawn.CheckInBox(
                 new Vector2(this.transform.localPosition.x, this.transform.localPosition.y)))
                {
                    // ���� �ڵ��Ͽ��� ��ȯ�� �� �̴Ͼ�ī�� ����
                    GAME.IGM.Hand.PlayerHand.Remove(this);
                    // �̴Ͼ� ��ȯ �Ϸ��, ��� �ڵ�ī�� ����Ȱ�� �ʱ�ȭ
                    GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);
                    // �̴Ͼ� ���� ���� (ī�� �Ҹ�ȭ �ִϸ��̼� �� ������ StartSpawn���ο��� ����)
                    GAME.IGM.Spawn.StartSpawn(this);
                    //�ڵ�Ŵ������� �ڵ�ī��� ������ ����
                    GAME.IGM.AddAction(GAME.IGM.Hand.CardAllignment(this.IsMine));
                    return;
                }
                break;
            case Define.cardType.spell:
                if (GAME.IGM.Spawn.CheckInBox(
                 new Vector2(this.transform.localPosition.x, this.transform.localPosition.y)))
                {
                    // ���� �ڵ��Ͽ��� ��ȯ�� �� �̴Ͼ�ī�� ����
                    GAME.IGM.Hand.PlayerHand.Remove(this);

                    // ���� �̺�Ʈ ������ Ȯ��
                    List<CardBaseEvtData> list = data.evtDatas.FindAll(x => x.when == Define.evtWhen.onPlayed);

                    // �̺�Ʈ ������ �ڵ� ���
                    if (list == null) { return; }

                    // �̺�Ʈ�߿� ���� �̺�Ʈ �ִ��� Ȯ��
                    CardBaseEvtData selectEvt = data.evtDatas.Find(x => x.targeting == Define.evtTargeting.Select);

                    // ���� Ÿ�����ؾ��ϴ� �̺�Ʈ�� �ִٸ�
                    if (selectEvt != null)
                    {
                        // ��ü �̺�Ʈ���� ���� �������� Ÿ�����ϴ� �̺�Ʈ�� ����
                        list.Remove(selectEvt);

                        // Ÿ���� ī�޶� ���� + ���� Ÿ���� ������ �����Լ� ���� ����
                        GAME.Manager.StartCoroutine(GAME.IGM.TC.TargettingCo
                            (this,

                            (IBody a, IBody t) =>
                            {
                                return evt(a, t);
                            },

                            GAME.IGM.Battle.FindLayer(selectEvt) // ���� ���� Ÿ�� �̺�Ʈ�� ���̾� ������ �°� ����
                            ));

                        // ������ Ÿ���� ���� �ش� �̺�Ʈ ���� ������, ������ ��Ÿ �̺�Ʈ ������� ����
                        IEnumerator evt(IBody a, IBody t)
                        {
                            // ��뿡�� ���� �ֹ� ī�带 ����Ѱ� �˸���
                            GAME.IGM.Packet.SendUseSpellCard(this.PunId, spellCardData.cardIdNum);

                            // ���� ������ ���� �����ϴ� Ÿ���̺�Ʈ ���� ����
                            GAME.IGM.Battle.Evt(selectEvt, a, t);

                            yield return null;
                            for (int i = 0; i < list.Count; i++)
                            {
                                GAME.IGM.Battle.Evt(list[i], this);
                                yield return null;
                            }
                            GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);  
                            

                            // ��뿡�� �� �ֹ�ī�� ��� ���� �˸���
                            GAME.IGM.AddAction(EndSpell());
                            //�ڵ�Ŵ������� �ڵ�ī��� ������ ����
                            GAME.IGM.AddAction(GAME.IGM.Hand.CardAllignment(this.IsMine));
                            IEnumerator EndSpell()
                            {
                                // �ֹ�ī�� ��� �Ϸ��, ��� �ڵ�ī�� ����Ȱ�� �ʱ�ȭ
                                GAME.IGM.StartCoroutine(FadeOutCo(IsMine));
                                GAME.IGM.Packet.SendEndingSpellCard(this.PunId);
                                yield return null;

                            }
                        }
                    }

                    // ���� �ڵ����� �̺�Ʈ�� �����ϸ�, ������� ���
                    else
                    {
                        // ��뿡�� ���� �ֹ� ī�带 ����Ѱ� �˸���
                        GAME.IGM.Packet.SendUseSpellCard(this.PunId, spellCardData.cardIdNum);
                        // �̺�Ʈ ����
                        for (int i = 0; i < list.Count; i++)
                        {
                            GAME.IGM.Battle.Evt(list[i], this);
                        }

                        GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);

                        // �ֹ�ī�� ��� �Ϸ��, ��� �ڵ�ī�� ����Ȱ�� �ʱ�ȭ
                        GAME.IGM.StartCoroutine(FadeOutCo(IsMine));

                        // ��뿡�� �� �ֹ�ī�� ��� ���� �˸���
                        GAME.IGM.AddAction(EndSpell());
                        IEnumerator EndSpell()
                        {
                            yield return null;
                            GAME.IGM.Packet.SendEndingSpellCard(this.PunId);
                            //�ڵ�Ŵ������� �ڵ�ī��� ������ ����
                            GAME.IGM.AddAction(GAME.IGM.Hand.CardAllignment(this.IsMine));
                        }
                    }
                    
                    return;
                }
                else { break; }
            case Define.cardType.weapon:
                if (GAME.IGM.Spawn.CheckInBox(
                 new Vector2(this.transform.localPosition.x, this.transform.localPosition.y)))
                {
                    // ���� �ڵ��Ͽ��� ī�� ����
                    GAME.IGM.Hand.PlayerHand.Remove(this);
                    // a���� ����, ��� �ڵ�ī�� ����Ȱ�� �ʱ�ȭ
                    GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);
                    // ���� ���� ���� ���� ( ī�� �Ҹ� �ִϸ��̼� �ڷ�ƾ�� �Լ� ���ο��� ����)
                    GAME.IGM.AddAction(GAME.IGM.Hero.Player.EquipWeapon(this));

                    // ��뿡�� �� ���� ���� �̺�Ʈ ����
                    GAME.IGM.Packet.SendWeapon(PunId, data.cardIdNum, this.transform.position.x
                        , this.transform.position.y, this.transform.position.z);

                    //�ڵ�Ŵ������� �ڵ�ī��� ������ ����
                    GAME.IGM.AddAction(GAME.IGM.Hand.CardAllignment(this.IsMine));

                    return;
                }
                break;
        }
        GAME.IGM.Spawn.idx = -1000;
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
            GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);

            DragCo = null;
        }
    }
    #endregion
}
