using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using static Define;
using WebSocketSharp;

public class CardHand : CardEle, IBody
{
    AudioSource audioPlayer;
    public Vector3  originRot, originScale;
    public int originOrder;
    public TextMeshPro cardName, Description, Stat, Type, Cost;
    public SpriteRenderer cardBackGround, cardImage;
    public GameObject TMPgo;
    [SerializeField] int currAtt, currHp, currCost;

    #region IBODY
    public bool Attackable { get; set; }
    [field: SerializeField] public int PunId { get; set; }
    [field: SerializeField] public bool IsMine { get; set; }
    public Transform TR { get { return this.transform; } }
    public Define.ObjType objType { get; set; }
    [field: SerializeField] public Collider2D Col { get; set; }
    public bool Ray 
    {
        set 
        { 
            if (Col == null) 
            {
                Col = TR.GetComponent<Collider2D>(); 
            }
            Col.enabled = value; 
        } 
    }

    public Vector3 OriginPos { get; set; }
    public IEnumerator onDead { get; set; }

    [field : SerializeField] public int OriginAtt { get; set; }
    [field: SerializeField] public int OriginHp { get; set; }
    public int Att
    {
        get
        { return currAtt; }
        set
        {
            if (CardEleType == cardType.spell) { return; }
            currAtt = value;
            DrawVar(); // tmp ����
        }
    }
    public int HP
    {
        get
        { return currHp; }
        set
        {
            if (CardEleType == cardType.spell) { return; }
            currHp = Mathf.Clamp(value,0, value) ;
            DrawVar(); // tmp ����
        }
    }
    #endregion

    [field: SerializeField] public int OriginCost { get; set; }
    public int CurrCost
    {
        get
        { return currCost; }
        set
        {
            currCost = Mathf.Clamp(value, 0, value);
            Cost.text = currCost.ToString();
        }
    }

    // ���� �ڵ� ���ڸ� ��ġ + ��ױ� (��ο� & ������ϋ� ���Ұ�)
    public void rewindHand() { StopAllCoroutines(); DragCo = null; Exit(null); }
    // OnHand�̺�Ʈ�� �����, Ư���������� �̺�Ʈ�� ���� (�̺�Ʈ ������ BattleManager.cs���� ���)
    public Action<int,bool> HandCardChanged;
    public void DrawVar() // ���� �����, tmp�ڵ� ����
    {
        switch (CardEleType)
        {
            case Define.cardType.minion:
                Stat.text = $"<color=green>ATT {Att} <color=red>HP {HP} <color=black>����";return;
            case Define.cardType.spell:
                Stat.text = "<color=black>�ֹ�";return;
            case Define.cardType.weapon:
                Stat.text = $"<color=green>ATT {Att} <color=red>dur {HP} <color=black>����"; return;
        }
    }
    public void Awake()
    {
        originScale = 0.3f * Vector3.one;
        Col = GetComponent<BoxCollider2D>();
    }
    public void Init(CardData data, bool isMine = true)
    {
        audioPlayer = GetComponent<AudioSource>();
        base.Init(data);
        IsMine = isMine;
        cardBackGround.sprite = GAME.Manager.RM.GetCardSprite(IsMine);
        originScale = Vector3.one * 0.3f;
        transform.localScale = originScale;
        objType = ObjType.HandCard;
        
        // ���� ī���� ��� ������ �ʱ�ȭ
        if (IsMine)
        {
            cardName.text = data.cardName;
            Description.text = data.cardDescription;
            Description.fontSize = ( data.cardDescription == null || data.cardDescription.Length > 25) ? 18 : 15;
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
                    Stat.text = $"<color=green>ATT {MC.att} <color=red>HP {MC.hp} <color=black>����";
                    Att = OriginAtt = MC.att; HP = OriginHp = MC.hp;
                    break;
                case Define.cardType.spell:
                    Stat.text = "<color=black>�ֹ�";
                    break;
                case Define.cardType.weapon:
                    Stat.text = $"<color=green>ATT {WC.att} <color=red>dur {WC.durability} <color=black>����";
                    Att = OriginAtt = WC.att; HP = OriginHp = WC.durability; 
                    break;
            }

            CurrCost = OriginCost = data.cost;
            cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);

            // �巡�� �̺�Ʈ ����
            GAME.Manager.UM.BindEvent(this.gameObject, StartDrag, Define.Mouse.StartDrag);
            GAME.Manager.UM.BindEvent(this.gameObject, Dragging, Define.Mouse.Dragging);
            GAME.Manager.UM.BindEvent(this.gameObject, EndDrag, Define.Mouse.EndDrag);
            // Ŀ�� ������ ��� �̺�Ʈ
            GAME.Manager.UM.BindEvent(this.gameObject, Enter, Define.Mouse.Enter);
            GAME.Manager.UM.BindEvent(this.gameObject, Exit, Define.Mouse.Exit);

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
        // �巡�� ���� �̺�Ʈ�� �ߵ��ɋ�, DragCo�� �����ڷ�ƾ�� ���� �� ����
        // DragCo�� Null�� �ƴϸ�, ���� �巡�װ��� �̺�Ʈ���̱⿡ Ȯ��/��� �̺�Ʈ�� ���� ����
        if (DragCo != null) { return; }
        
        SetOrder(originOrder * 10);
        transform.localScale = originScale * 1.5f;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(transform.localPosition.x, -1.65f,-0.5f);
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
    public IEnumerator DragCo = null;
    public float xDegree, yDegree;
    public float rotVelX, rotVelY;
    float X, Y;
    public void StartDrag(Vector3 v)
    {
        // ��빮���� ��� ���ϴ� ī���� �ٷ� ���
        if (CurrCost > GAME.IGM.Hero.Player.MP) { return; }

        // �ڵ�ī�� �巡�׽�, �ʵ��� �ϼ��ΰ� ��ġ�� �ȵǱ⿡ ��ò���
        GAME.IGM.Spawn.playerMinions.ForEach(x=>x.Ray = false);

        // �巡�� ���� ȿ���� ���
        audioPlayer.clip =  GAME.IGM.GetClip(Define.IGMsound.Pick);
        audioPlayer.Play();

        // �ֹ�ī���̸� ���� Ÿ�����ϴ� ���, ī�带 ���������ʰ� ���� �ڵ�ī�忡�� Ÿ���� ȭ��ǥ ����
        if (CardEleType == cardType.spell && SC.evtDatas[0].targeting == evtTargeting.Select)
        {
            // �̺�Ʈ�߿� ���� �̺�Ʈ ã��
            CardBaseEvtData selectEvt = SC.evtDatas[0];

            // ���� ���� Ÿ������ �ϴ� �ֹ�������
            // ����� �� �̴Ͼ� ���̰�, �� �̴Ͼ��� ������
            // ������ ��ο� �� �Լ��� ȣ���Ͽ� ��� �������
            string[] layers = GAME.IGM.Battle.FindLayer(selectEvt);
            if (layers.Length == 1 && layers[0] == "foe"
                && GAME.IGM.Spawn.enemyMinions.Count == 0)
            { EndDrag(default); return; }

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
                //  evt �ڷ�ƾ�� ����� �ǹ̴� , ������ ������ Ÿ������ �����Ͽ��⿡ �̺�Ʈ ���� �� �ֹ�ī�� ������� ����
                // ���� �ڵ��Ͽ��� ����� �ֹ� ī�� ����
                GAME.IGM.Hand.PlayerHand.Remove(this);

                // ���� ������ �ƴ� ����ȭ�� ����
                GAME.IGM.AddAction(FadeOutCo(true, false));

                // ��뿡�� ���� �ֹ� ī�带 ����Ѱ� �˸���
                GAME.IGM.Packet.SendUseSpellCard(this.PunId, SC.cardIdNum);

                for (int i = 0; i < SC.evtDatas.Count; i++)
                {
                   GAME.IGM.AddAction(GAME.IGM.Battle.Evt(SC.evtDatas[i], this, t));
                    yield return null;
                }

                // ������ �ڵ� ���� Ǯ���ֱ�
                GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);

                // ��뿡�� �� �ֹ�ī�� ��� ���� �˸���
                GAME.IGM.AddAction(EndSpell());
                IEnumerator EndSpell()
                {
                    //�ڵ�Ŵ������� �ڵ�ī��� ������ ����
                    yield return  GAME.IGM.Hand.CardAllignment(this.IsMine);
                    // �ֹ�ī�� ��� ���� ����
                    GAME.IGM.Packet.SendEndingSpellCard(this.PunId);
                    // �ֹ�ī�� ���� ����Ͽ��⿡ ���� ����
                    GAME.IGM.Hand.AllCardHand.Remove(this);
                    GameObject.Destroy(this.gameObject);
                }
            }
        }

        // �׿� ��� ī����� �ʵ忡 �巡�׸� �Ͽ� ���ƾ� ����ϴ°����� ����
        else
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
                Quaternion currRot = transform.localRotation;
                while (t < 1f)
                {
                    t += Time.deltaTime * 2.5F;
                    transform.localScale = Vector3.Lerp(currScale, originScale, t);
                    transform.localRotation = Quaternion.Lerp(currRot , Quaternion.identity,t);
                    yield return null;
                } 
                
                // �巡�׵��� ȸ�� 0������ ���ư��� ��� �õ� (�����ӿ� ���� ���� ȸ���� ������ Dragging�Լ����� )
                while (true)
                {
                    X = Mathf.SmoothDamp(X, 0, ref rotVelX, 0.3f);
                    Y = Mathf.SmoothDamp(Y, 0, ref rotVelY, 0.3f);

                    transform.rotation = Quaternion.Euler(new Vector3(X, Y, 0));
                    yield return null;
                }
            }

        }

    }
    public void Dragging(Vector3 worldPos)
    {
        if (DragCo == null) { return; }

        if (CardEleType == cardType.spell && SC.evtDatas[0].targeting == evtTargeting.Select) { return; }

        if (MC != null)
        {
            GAME.IGM.Spawn.MinionAlignment(this, worldPos);
        }

        Vector3 euler = transform.rotation.eulerAngles;
        // worldPos�� ���� ���콺�� WP
        // dir :  �̵� ����
        Vector3 dir = worldPos - this.transform.position;

        // ���� �̵��� �����̶� �ߴ��� ���� Ȯ��
        if (dir.sqrMagnitude > Mathf.Epsilon)
        {
            // ���� �̵� Ȯ�ν� �̵��������� ȸ������ ���� (���ڸ�����(0��)�� StartDrag�Լ������� �ڷ�ƾ���� ������ )
            X += dir.y * 15f;
            X = Mathf.Clamp(X, -40f, 40f);
            Y += -dir.x * 15f;
            Y = Mathf.Clamp(Y, -40f, 40f);
        }

        this.transform.localPosition = worldPos;
    }

    // ī�� ����ȭ�� �Ҹ� �ڷ�ƾ : �ַ� ī�� ���� �Ǵ� �̴Ͼ�ī�带 �ʵ�� ��ȯ�ҋ� ���
    public IEnumerator FadeOutCo(bool isMine = true, bool bothDelete = true)
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

        // ������ �����Ұ���
        if (bothDelete)
        {
            GAME.IGM.Hand.AllCardHand.Remove(this);
            GameObject.Destroy(this.gameObject);
        }
    }
    public void EndDrag(Vector3 v)
    {

        Ray = false; // Ray�� ��Ȱ��ȭ�� Exit�� ȣ�������, DragCo�� �ڿ��� null�� �ʱ�ȭ�Ͽ��� Exit ���� ������ ����
        StopAllCoroutines();
        // �ڵ�ī�� �巡�׽�, �ʵ��� �ϼ��ΰ� ��ġ�� �ȵǱ⿡ ��ò���
        GAME.IGM.Spawn.playerMinions.ForEach(x => x.Ray = true);
        switch (CardEleType)
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
                    //�ڵ�Ŵ������� �ڵ�ī�� �������� �� ��ŸƮ�����Լ� �������� ����
                    return;
                }
                break;
            case Define.cardType.spell:
                if (SC.evtDatas[0].targeting != evtTargeting.Select
                     && GAME.IGM.Spawn.CheckInBox( new Vector2(this.transform.localPosition.x, this.transform.localPosition.y)))
                {
                    // ��� ���� �� �ڵ帮��Ʈ���� ����
                    GAME.IGM.Hero.Player.MP -= this.currCost;
                    GAME.IGM.Hand.PlayerHand.Remove(this);

                    // ��뿡�� ���� �ֹ� ī�带 ����Ѱ� �˸���
                    GAME.IGM.Packet.SendUseSpellCard(this.PunId, SC.cardIdNum);
                    // �̺�Ʈ ����
                    for (int i = 0; i < SC.evtDatas.Count; i++)
                    {
                        GAME.IGM.AddAction(GAME.IGM.Battle.Evt(SC.evtDatas[i], this)) ;
                    }

                    GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);

                    // ��뿡�� �� �ֹ�ī�� ��� ���� �˸���
                    GAME.IGM.AddAction(EndSpell());
                    IEnumerator EndSpell()
                    {
                        yield return null;
                        GAME.IGM.Packet.SendEndingSpellCard(this.PunId);
                        //�ڵ�Ŵ������� �ڵ�ī��� ������ ����
                        GAME.IGM.AddAction(GAME.IGM.Hand.CardAllignment(this.IsMine));
                        // �ֹ�ī�� ��� �Ϸ��, ��� �ڵ�ī�� ����Ȱ�� �ʱ�ȭ
                        GAME.IGM.AddAction(FadeOutCo(IsMine));
                    }


                    return;
                }
                else
                { GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true); break; }
            case Define.cardType.weapon:
                if (GAME.IGM.Spawn.CheckInBox(
                 new Vector2(this.transform.localPosition.x, this.transform.localPosition.y)))
                {
                    // ��븸ŭ ���� ����
                    GAME.IGM.Hero.Player.MP -= CurrCost;

                    // ���� �ڵ��Ͽ��� ī�� ����
                    GAME.IGM.Hand.PlayerHand.Remove(this);
                    // a���� ����, ��� �ڵ�ī�� ����Ȱ�� �ʱ�ȭ
                    GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);

                    //�ڵ�Ŵ������� �ڵ�ī��� ������ ����
                    GAME.IGM.AddAction(GAME.IGM.Hand.CardAllignment(this.IsMine));

                    // ��뿡�� �� ���� ���� �̺�Ʈ ����
                    GAME.IGM.Packet.SendWeapon(PunId, WC.cardIdNum, this.transform.position.x
                        , this.transform.position.y, this.transform.position.z);

                    // ���� ���� ���� ���� ( ī�� �Ҹ� �ִϸ��̼� �ڷ�ƾ�� �Լ� ���ο��� ����)
                    GAME.IGM.AddAction(GAME.IGM.Hero.Player.EquipWeapon(this));
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
            // �巡�� �� ȿ���� ���
            audioPlayer.clip = GAME.IGM.GetClip(Define.IGMsound.Cancel); 
            yield return null;
            audioPlayer.Play();
            // SR�� ���� �ʱ�ȭ �������
            SetOrder(originOrder);
            // ũ��� ��� �ʱ�ȭ
            float t = 0;
            Vector3 currScale = transform.localScale;
            Quaternion currRot = transform.localRotation;
            Quaternion originEuler = Quaternion.Euler(originRot);
            currRot.z = 0;
            Vector3 currPos = transform.localPosition;
            while (t < 1f)
            {
                t += Time.deltaTime * 5f;
                transform.localScale = Vector3.Lerp(currScale, originScale, t);
                transform.localRotation = Quaternion.Lerp(currRot, originEuler, t);
                transform.localPosition = Vector3.Lerp(currPos, OriginPos, t);
                yield return null;
            }
            GAME.IGM.Hand.PlayerHand.ForEach(x => x.Ray = true);

            DragCo = null;
        }
    }
    #endregion
}
