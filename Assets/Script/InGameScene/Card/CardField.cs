using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static Define;
using System.Linq;
using System;

public class CardField : CardEle
{
    public TextMeshPro AttTmp, HpTmp;
    public SpriteRenderer cardImage, attIcon, hpIcon;
    public bool attackable = true;
    public ParticleSystem sleep;
    public SpriteMask mask;
    MinionCardData minionCardData;
    public override int Att
    {
        get { return minionCardData.att; }
        set { minionCardData.att = value; AttTmp.text = minionCardData.att.ToString(); }
    }

    public override int HP
    {
        get { return minionCardData.hp; } 
        set { minionCardData.hp = value; HpTmp.text = minionCardData.hp.ToString(); } 
    }

    // �̴Ͼ� ī�尡 ������ �Ѵ� ������, �ε����� ��ġ�� ��ġ�� ����
    // ���� ���̾ �����ϸ� �̹����� ��ġ�ų� ���� ������ �־�, �����ڰ� �ֻ�ܿ� ��ġ�ϵ��� ���̾� ����
    public void ChangeSortingLayer(bool isOn)
    {
        SortingLayer[] layers = SortingLayer.layers;
        SortingLayer layer = Array.Find(layers, x => x.name == ((isOn) ? "Attacker" : "None"));
        
        mask.frontSortingLayerID = layer.id;
        cardImage.sortingLayerID = layer.id;
        AttTmp.sortingLayerID = HpTmp.sortingLayerID = layer.id;
    }
    public void Init(CardData dataParam, bool isMine)
    {
        data = dataParam;
        IsMine = isMine;
        gameObject.layer = LayerMask.NameToLayer((IsMine == true) ? "ally" : "foe") ;
        
        Col = GetComponent<CircleCollider2D>();
        minionCardData = (MinionCardData)data;
        Att = OriginAtt = minionCardData.att;
        HP = OriginHp = minionCardData.hp;
        cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);
        
        GAME.Manager.UM.BindCardPopupEvent(this.gameObject, CallPopup, 0.75f);
        
        attackable = (minionCardData.isCharge) ? true : false;
        sleep.gameObject.SetActive((attackable)? true : false);
        attackable = true; sleep.gameObject.SetActive(false);
        if (IsMine)
        { 
            // ��Ŭ���� ī�޶�� ����Ȱ��ȭ�Ͽ� Ÿ���� �̺�Ʈ ����
            GAME.Manager.UM.BindEvent(this.gameObject, StartAttack, Define.Mouse.ClickL, Define.Sound.Ready);

            // ��ȯ�� , �տ��� ���� ������ �̺�Ʈ �ִ��� Ȯ��
            List<CardBaseEvtData> list = data.evtDatas.FindAll(x => x.when == Define.evtWhen.onPlayed);

            for (int i = 0; i < list.Count; i++)
            {
                GAME.IGM.Battle.Evt(list[i], this);
            }
        }

        // �ʵ� �ϼ����� �׾����� ������ �ִ� �ڷ�ƾ ����
        onDead = Dead(IsMine);
        IEnumerator Dead(bool isMine)
        {
            // ���� ����������
            StartCoroutine(FadeOut());
            IEnumerator FadeOut()
            {
                float t = 1;
                while (t < 1f)
                {
                    // ����ȭ ����
                    t -= Time.deltaTime ;
                    Color tempColor = new Color(1,1,1,t);
                    cardImage.color = attIcon.color = hpIcon.color = tempColor;
                    AttTmp.alpha = HpTmp.alpha =t;
                    yield return null;
                }
                mask.enabled = false;
            }

            // �¿�� �Դٰ��ٷ� �״� �ڷ�ƾ �ִ�
            yield return StartCoroutine(Wiggle());
            IEnumerator Wiggle()
            {
                float t = 0;
                float min = this.transform.position.x -0.25f;
                float max = this.transform.position.x + 0.25f;
                while (t < 1f)
                {
                    t += Time.deltaTime;
                    float x = Mathf.Lerp(min, max, MathF.Sin(t * MathF.PI));
                    transform.position = new Vector3(x, transform.position.y, transform.position.z);
                    yield return null;
                }
            }

            // ���� ���� �ϼ����� ������ �������� ���� �������� �����ϱ�
            if (isMine)
            { 
                // �� �ϼ��� �׾�����, ����Ʈ���� ���� �� ������ġ�� ������
                GAME.IGM.Spawn.playerMinions.Remove(GAME.IGM.Spawn.playerMinions.Find(x=>x.PunId == this.PunId));
                // �ʵ� ������
                yield return StartCoroutine(GAME.IGM.Spawn.AllPlayersAlignment());
            }
            // �� �ϼ��ε� �� ����
            else
            {
                GAME.IGM.Spawn.enemyMinions.Remove(GAME.IGM.Spawn.enemyMinions.Find(x => x.PunId == this.PunId));
                yield return StartCoroutine(GAME.IGM.Spawn.AllEnemiesAlignment());
            }

            Destroy(this.gameObject);
        }
    }

    // �̴Ͼ� ī���� ���, ������ ���� Ŀ���� ������ ��� ī���� ������ �����ִ� ī���˾� ȣ�� �̺�Ʈ
    public void CallPopup()
    {
        Debug.Log(data.cardType.ToString());

        if (this.data is MinionCardData == false)
        {
            Debug.Log("ERROR : �� �����Ͱ� �̴Ͼ�Ÿ���� �ƴ���");
        }

        else
        {
            // ����� ���� �ε��� ã��
            int idx = (this.IsMine) ? GAME.IGM.Spawn.playerMinions.IndexOf(this)
                : GAME.IGM.Spawn.enemyMinions.IndexOf(this) ;

            // �������� �ʹ� �и� �̴Ͼ��� ��� �������� ī���˾��� ����ֱ�
            Vector3 pos = transform.position + Vector3.right * 2f;
            if (this.transform.position.x > 3f)
            { pos = transform.position - Vector3.right * 2f; }
            
            // ������ �����Ϳ� ��ġ ��������, ī���˾� ����
            GAME.IGM.ShowMinionPopup((MinionCardData)data, pos, cardImage.sprite); 
        }
        
    }

    // �̴Ͼ� ���ݽõ� �Լ� (��Ŭ�� ���콺 �̺�Ʈ)
    public void StartAttack(GameObject go)
    {
        // ���� ������ ���°� �ƴϰų�
        // �̹� �ٸ� ��ü�� Ÿ���� ���ε� �� ��ü�� Ŭ���� ���
        if (!attackable || GAME.IGM.TC.LR.gameObject.activeSelf == true
            || sleep.gameObject.activeSelf == true)
        { return; }

        // ������ �ڽŰ�, �������� ���� ��Ȱ��ȭ
        GAME.IGM.Spawn.SpawnRay = Ray = false;

        // Ÿ���� ī�޶� ���� + ���� Ÿ���� ������ �����Լ� ���� ����
        GAME.Manager.StartCoroutine(GAME.IGM.TC.TargettingCo
            (this,
            (IBody a, IBody t) => { return AttackCo(a, t); },
            new string[] { "foe", "foeHero" }
            ));
    }

    public IEnumerator AttackCo(IBody attacker, IBody target)
    {
        #region ���� �ڷ�ƾ : ��뿡�� ��ġ��
        ChangeSortingLayer(true); // ������ ���÷��̾�� �Ű� �ֻ�ܿ� ��ġ�ϱ�
        float t = 0;
        Vector3 start = attacker.Pos;
        Vector3 dest = target.Pos;
        while (t < 1f)
        {
            t += Time.deltaTime * 1f;
            this.transform.position = Vector3.Lerp(start, dest, t);
            yield return null;
        }
        #endregion

        #region ī�޶� ���� ����Ʈ
        // 0~PI ������ ���̸� ���ѵ� ������ ����ϸ�
        // 0 ~ 1 ��, 1 ~ 0 ���� �ǵ��� ���⿡ Z�� ȸ�� �ڷ�ƾ���� �̿��ϱ�� ����
        StartCoroutine(GAME.IGM.TC.ShakeCo());
        yield return null;

        #endregion

        #region ���ڸ��� ����
        t = 0 ;
        while (t < 1f)
        {
            t += Time.deltaTime * 1f;
            this.transform.localPosition = Vector3.Lerp( dest , OriginPos, t);
            yield return null;
        }
        ChangeSortingLayer(false); // ���÷��̾� �ʱ�ȭ
        #endregion

        // ������ ��ȯ
        attacker.HP -= target.Att;
        target.HP -= attacker.Att;

        // ���� ���̰�, ���� ���� �̴Ͼ��� �����ߴٸ�
        // ���� ���� ������ �ൿ���� Ȯ�� �� ���� �̺�Ʈ ��뿡�� ����
        if (GAME.IGM.Packet.isMyTurn && attacker.IsMine)
        {
            GAME.IGM.Packet.SendMinionAttack(attacker.PunId, target.PunId);
        }

        // ���� �ϼ����� �ִ��� Ȯ�� �� �׾��ٸ� �״� �ִϸ��̼� ����
        if (target.HP <= 0) { yield return StartCoroutine(target.onDead); }
        if (attacker.HP <= 0) { yield return StartCoroutine(attacker.onDead); }
    }
    
}