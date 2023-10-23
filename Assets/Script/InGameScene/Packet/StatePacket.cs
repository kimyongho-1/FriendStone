using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class PacketManager
{
    // 0 ~ 20
    const byte UserInfo = 0;
    const byte StartIntro = 1; // �ΰ��Ӿ� ���۽� ȭ���� ������� ��Ʈ�� ���ý��� ����
    const byte InitDraw = 2; //  �ʱ� 4�� ��ο� ���·� ���ӽ��� ���ý��� ����
    const byte IsOffensive = 3; // ���İ� ���� ��� ���ý��� ����
    const byte UserDraw = 4; // ��밡 ī�带 ���������� �ڵ����� ���Ե� �̺�Ʈ�� ����
    const byte OtherTurnEnd = 5;
    const byte OtherTurnStartMSG = 6;
    const byte DoDraw = 7; // ��ο츦 ���� ����

    const byte FindEvt = 10; // �߰� �̺�Ʈ ���� ����
    const byte FindEvtResult = 11; // �߰� �̺�Ʈ �����ڰ� ī�带 ���� �� ��� ����
    const byte AcqusitionEvt = 12; // �ܼ� ȹ�� �̺�Ʈ ����� ����

    const byte BuffEvt = 14; // ���� �̺�Ʈ ���޹����� ����ȭ�ϱ�
    const byte AttEvt = 15; // ���� �̺�Ʈ ���޹����� ����ȭ�ϱ�
    const byte RestoreEvt = 16; // ġ���̺�Ʈ
    const byte FatigueEvt = 17; // ��밡 ���� ī�尡 ���� �����Դ� Ż�� �̺�Ʈ
    const byte OverDrawEvt = 18; // ��밡 ī�尡 10���ε� �߰� ��ο��, �̴� ī�� ���ִ� �̺�Ʈ
    const byte GameEnd = 19; // ���Ӱ���� ������ ���� �����, ������ �����ϱ⿡ �޾Ƽ� ����ȭ
    public void InitStateDictionary()
    {
        dic.Add(UserInfo, ReceivedUserInfo);
        dic.Add(StartIntro, (object[] o)=> { GAME.IGM.AddAction(GAME.IGM.TC.StartIntro()); }); 
        dic.Add(InitDraw, (object[] o) => { GAME.IGM.AddAction(GAME.IGM.Hand.CardDrawing(4)); });
        dic.Add(IsOffensive, ReceivedOffensiveResult);
        dic.Add( UserDraw , ReceivedOtherDraw );
        dic.Add(OtherTurnEnd, ReceivedTurnEnd);
        dic.Add(OtherTurnStartMSG, ShowOtherTurn);
        dic.Add(DoDraw, ReceivedDoDraw);
        dic.Add(FindEvt, ReceivedFindEvt);
        dic.Add(FindEvtResult, ReceivedResultFindEvt);
        dic.Add(AcqusitionEvt, ReceivedAcquisition );
        dic.Add(BuffEvt, ReceivedBuffEvt);
        dic.Add(AttEvt, ReceivedAttEvt );
        dic.Add(RestoreEvt, ReceivedRestoreEvt );
        dic.Add(FatigueEvt, ReceivedFatigueEvt);
        dic.Add(OverDrawEvt, ReceivedOverDrawEvt);
        dic.Add(GameEnd, GameEndStart);
    }

    // �� �� ���� ����
    public void SendMyTurnMSG()
    {
        // ��뿡�� �� �� �����Ѵٰ� �˸���
        PhotonNetwork.RaiseEvent(OtherTurnStartMSG, null, Other, SendOptions.SendReliable);
    }
    // ����� �� ���۽�, ����� �� ���� �޽��� ��� 
    public void ShowOtherTurn(object[] data)
    {
        // ��� �� ����, ��� ���� �ʱ�ȭ
        GAME.IGM.Hero.Enemy.MP = Mathf.Min(10, GAME.IGM.GameTurn+1);
        // ��� ������ �˸���
        GAME.IGM.AddAction(GAME.IGM.Turn.ShowTurnMSG(false));
    }

    #region ���ӽ��۽�, �� �⺻���� ���� (�г��Ӱ� ��������) + ���İ� ���� 
    public void SendUserInfo(string nick, int ownerClass)
    {
        // ����
        #region �г��Ӱ� �������� ����

        // ������ ������
        // ���� �г���
        // ���� �������� Ÿ��
        object[] packets = new object[]
        {
            nick,
            ownerClass,
        };

        // �� ���� ����
        PhotonNetwork.RaiseEvent(UserInfo, packets, Other, SendOptions.SendReliable);
        #endregion

    }

    // ���κ��� �⺻���� ���� �޾� ó��
    public void ReceivedUserInfo(object[] data)
    {
        Debug.Log("���� ���� �ޱ� ����");
        string nickName = (string)data[0];
        Define.classType classType = (Define.classType)(data[1]);

        // �� �г��� �ʱ�ȭ
        GAME.IGM.Hero.Enemy.nickTmp.text = nickName;
        // �� ���� ĵ���� �ʱ�ȭ
        GAME.IGM.Hero.Enemy.heroData = GAME.Manager.RM.GetHeroData(classType);
        GAME.IGM.Hero.Enemy.heroData.Init(GAME.IGM.Hero.Enemy.playerImg, GAME.IGM.Hero.Enemy.skillImg,false);
        GAME.IGM.Hero.Enemy.heroSkill.InitSkill(false);
    }

    // ���İ� ��� �����ͷκ��� �ޱ� (�����ʹ� �ڽ� ���ο��� ����)
    public void ReceivedOffensiveResult(object[] data)
    {
        bool result = (bool)data[0];
        Debug.Log($"���� ���� ��� : {result}");

        // �����Ͷ�� �״�� ����, Ÿ Ŭ���̾�Ʈ�� ������Ű��
        result = (PhotonNetwork.IsMasterClient) ? result : !result;
        // ���� �İ��̶��
        if (result == false)
        {
            // ���� ������� �ƴ�����, �Ų����� ������ ���� ������ ����
            GAME.IGM.Turn.ClickedOnTurnEnd(null);
            //GAME.IGM.AddAction(GAME.IGM.Turn.EndMyTurn());
        }

    }

    #endregion

    #region ��ο� ���� �� ����

    // ���� ��ο��, ����� ȭ��� �� �ڵ忡�� ��ο츦 �ؾ��ϱ⿡ ���� ī�� �ݳѹ��� ����
    public void SendDrawInfo(int id)
    {
        // ���� ���� ���� ī�� �ݳѹ��� ����
        PhotonNetwork.RaiseEvent(UserDraw,
        new object[] { id } ,
        Other, SendOptions.SendReliable);
    }

    // ���κ��� �ݳѹ��� ���޹޾�, ��ο� ����ȭ ����
    public void ReceivedOtherDraw(object[] data)
    {
        Debug.Log($"���κ��� ��ο� �̺�Ʈ ���޹��� {(int)data[0]}");
        if (GAME.IGM.GameTurn < 2f)
        {
            // ����� ��ο� ����
            GAME.IGM.StartCoroutine(GAME.IGM.Hand.EnemyCardDrawing((int)data[0]));
        }
        else
        { // ����� ��ο� ����
            GAME.IGM.AddAction(GAME.IGM.Hand.EnemyCardDrawing((int)data[0]));
        }
        
    }

    // ��뿡�� ��ο츦 �϶�� �̺�Ʈ ���� [��븸 ��ų����, ���� ��ο��ϴ°���]
    public void SendDoDraw(int count, bool AllClient = false)
    {
        PhotonNetwork.RaiseEvent( DoDraw ,
        new object[] { count },
        (AllClient) ? Both : Other, SendOptions.SendReliable);
    }

    // ���κ��� ��ο츦 �϶�� �̺�Ʈ�޾� ��ο� ����
    public void ReceivedDoDraw(object[] data)
    {
        GAME.IGM.AddAction(GAME.IGM.Hand.CardDrawing((int)data[0]));
    }
    #endregion

    #region  �� ���� ���Ŀ� �ޱ�
    public void SendTurnEnd()
    {
        Debug.Log("�� ���� ����");
        
        // ����Ÿ�� ���� �����Ḧ ������ Ŭ���̾�Ʈ�� �Ͽ� +1�� ���� ���� ���� ���� (integer)
        PhotonNetwork.RaiseEvent(OtherTurnEnd,
            new object[] { GAME.IGM.GameTurn+1 },
            Other, SendOptions.SendReliable);
    }

    // ����� ������ ���� �ޱ�
    public void ReceivedTurnEnd(object[] data)
    {
        Debug.Log("����� �� ���Ḧ ����");
        // ��밡 ���� �����, �ڽ��� �Ͽ� +1�� �� ���ο� �ϳѹ��� ���� �ޱ�
        GAME.IGM.GameTurn = (int)data[0];

        // ������ ��ư �ʱ�ȭ �� �ؽ�Ʈ �ִϸ��̼� ���� + ��ο�
        GAME.IGM.AddAction(GAME.IGM.Turn.StartMyTurn());

    }
    #endregion

    #region �߰��̺�Ʈ�� ���� �� �ޱ�

    // ���κ��� �߰��̺�Ʈ �����Ѵٴ°��� ����
    public void SendFindEvt() 
    { PhotonNetwork.RaiseEvent(FindEvt, null, Other, SendOptions.SendReliable); }

    // ���κ��� �߰��̺�Ʈ ������ ���� �޾�����, ����ȭ ���� ����
    public void ReceivedFindEvt(object[] data)
    { GAME.IGM.AddAction(GAME.IGM.FindEvt.ShowEnemyFindEvt()); }

    // �߰��̺�Ʈ�� �������� ���� ī�带 ���� �� ����� ��뿡�� ���� ����
    public void SendResultFindEvt(int idx, int punID)
    { PhotonNetwork.RaiseEvent(FindEvtResult, new object[] { idx , punID }, Other, SendOptions.SendReliable); }

    // ��밡 �߰��̺�Ʈ ����� �޾Ƽ� ����ȭ �ϱ�
    public void ReceivedResultFindEvt(object[] data)
    { 
        // ���κ��� ���° ī������ + �� ī���� ���� �ѹ��� �������� ���޹޾� �Ȱ��� ȭ�� �׷��ֱ�
        int idx = (int)data[0];
        int punID = (int)data[1];

        // �̺�Ʈ ����
        GAME.IGM.AddAction(GAME.IGM.FindEvt.ShowEnemyFindEvtResult(idx, punID));
    }
    #endregion

    #region ȹ�� �̺�Ʈ ���� �� �ޱ�

    // ���� ȹ���� ī����� ī��ĺ��ѹ��� �����Ͽ� ��뿡�Ե� ����ȭ��Ű��
    public void SendAcquisition(Define.ObjType objType,int punID ,int[] nums, int[] puns)
    {
        object[] data = new object[] { (objType == Define.ObjType.Minion) ? true : false, punID, nums, puns};
        // ���� ����
        PhotonNetwork.RaiseEvent(AcqusitionEvt, data, Other, SendOptions.SendReliable);
    }

    // ����� ȹ�� �̺�Ʈ ���޹޾�����, �Ȱ��� ������ֱ�
    public void ReceivedAcquisition(object[] data)
    {
        // �����ڰ� �̴Ͼ����� ����
        bool isMinionAct = (bool)data[0];
        // ������ ã�� + ī��ѹ� + �ĺ��ѹ� 
        int punID = (int)data[1];
        Debug.Log($"ȹ���̺�Ʈ ������ punID : {punID}");
        IBody caster = (isMinionAct) ? GAME.IGM.allIBody.Find(x=>x.PunId == punID) : GAME.IGM.Hand.EnemyHand.Find(punID);
        
        int[] cardIDs = (int[])data[2];
        int[] puns = (int[])data[3];

        GAME.IGM.AddAction(SyncAcquisitionEvt(cardIDs, puns));

        // ����� ȹ�� �̺�Ʈ�� �� ȭ�鿡�� �Ȱ��� ����ȭ ���ֱ�
        IEnumerator SyncAcquisitionEvt(int[] cardIDs, int[] puns)
        {
            // ȹ���ϴ� ��ġ��, �ִ� 10���� �Ѿ���� Ȯ�� (������ ������ ����ŭ�� ȹ��)
            int count = cardIDs.Length;
            // ����ī��� ����
            for (int i = 0; i < count; i++)
            {
                // ���ҽ� �Ŵ����� ��θ� ��ȯ �޴� ��ųʸ� ���� ī��Ÿ�԰� ī�嵥���� ã��
                Define.cardType type = GAME.Manager.RM.PathFinder.Dic[cardIDs[i]].type;
                string jsonFile = GAME.Manager.RM.PathFinder.Dic[cardIDs[i]].GetJson();
                CardData card = null;
                // Ȯ�ε� ī��Ÿ������, ���� ī��Ÿ������ Ŭ����ȭ
                switch (type)
                {
                    case Define.cardType.minion:
                        card = JsonConvert.DeserializeObject<MinionCardData>
                    (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                        break;
                    case Define.cardType.spell:
                        card = JsonConvert.DeserializeObject<SpellCardData>
                    (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                        break;
                    case Define.cardType.weapon:
                        card = JsonConvert.DeserializeObject<WeaponCardData>
                    (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                        break;
                    default: break;
                }

                // �ش� ī���ȣ�� ������ ����
                CardHand ch =
                GameObject.Instantiate(Resources.Load<CardHand>("Prefab/InGamePrefab/CardHand"), GAME.IGM.Hand.EnemyHandGO.transform);
                ch.Init(card, false);
                ch.PunId = puns[i];

                ch.transform.localScale = Vector3.zero;
                ch.transform.localPosition = new Vector3(0.45f,3.8f,0);
                GAME.IGM.Hand.EnemyHand.Add(ch);
                yield return null;
            }

            yield return GAME.IGM.StartCoroutine(GAME.IGM.Hand.CardAllignment(false));
        }
    }

    #endregion

    #region ���� �̺�Ʈ ���� �� �ޱ�
    public void SendBuffEvt(int targetPunID, Define.buffType type,int att, int hp)
    {
        object[] data = new object[] {targetPunID, (int)type , att,hp };
        // ���� ����
        PhotonNetwork.RaiseEvent(BuffEvt, data, Other, SendOptions.SendReliable);
    }
    public void ReceivedBuffEvt(object[] data)
    {
        int targetPunID = (int)data[0];
        Define.buffType type = (Define.buffType)data[1];
        int att = (int)data[2];
        int hp = (int)data[3];
        
        GAME.IGM.AddAction( DelayedBuff(targetPunID, att, hp, type));
        IEnumerator DelayedBuff(int targetPunID, int att, int hp , Define.buffType type)
        {
            CardField cf1 = GAME.IGM.Spawn.enemyMinions.Find(x => x.PunId == targetPunID);
            CardHand cf2 = GAME.IGM.Hand.EnemyHand.Find(targetPunID);
            Debug.Log($"cardField : {cf1}, cardHand : {cf2}");
            yield return GAME.IGM.StartCoroutine(
                GAME.IGM.Battle.ReceivedBuff(type, GAME.IGM.Spawn.enemyMinions.Find(x => x.PunId == targetPunID)
                , att, hp));
        }
    }
    #endregion

    #region ���� �̺�Ʈ ���� �� �ޱ�
    public void SendAttEvt(int attackerPunID ,int targetPunID, Define.attType type, int attAmount , Define.ObjType objType)
    {
        object[] data = new object[] { attackerPunID, targetPunID, (int)type, attAmount ,(int)objType };
        PhotonNetwork.RaiseEvent(AttEvt, data, Other, SendOptions.SendReliable);
    }
    public void ReceivedAttEvt(object[] data)
    {
        int attackerPunID = (int)data[0];
        int targetPunID = (int)data[1];
        Define.attType attType = (Define.attType)data[2];
        int attAmount = (int)data[3];
        Define.ObjType objType = (Define.ObjType)data[4];

        GAME.IGM.AddAction(MakeAttEvt(attackerPunID, targetPunID, attType, attAmount, objType));

        IEnumerator MakeAttEvt(int attackerPunID, int targetPunID, Define.attType attType, int attAmount, Define.ObjType objType)
        {
            IBody target = GAME.IGM.allIBody.Find(x => x.PunId == targetPunID);
            // ���� �̺�Ʈ�� ������, �������� objŸ���� �̴Ͼ��̶�� �̴Ͼ��� ã��
            // �׿ܿ��� �� ������ �����ڷ� ���� (���� ����Ʈ�� ������ġ�� �� �������� �����ϱ� ���ϱ⿡)
            IBody attacker = (objType == Define.ObjType.Minion) ?
                GAME.IGM.allIBody.Find(x => x.PunId == attackerPunID)
                : GAME.IGM.Hero.Enemy;
            Debug.Log($"���� �̺�Ʈ ������:{attacker.PunId}, Ÿ�� : {target.PunId}");
            // ���κ��� ���� �����̺�Ʈ ������ ����
            yield return GAME.IGM.StartCoroutine(GAME.IGM.Battle.AttackEvt(attacker, target, attAmount, attType));
        }
       
    }

    #endregion

    #region ġ�� �̺�Ʈ ���� �� ���޹ޱ�
    public void SendRestoreEvt(int casterID, int targetID, int amount, bool casterIsHandCard)
    {
        object[] data = new object[] { casterID, targetID, amount, (object)casterIsHandCard };
        PhotonNetwork.RaiseEvent(RestoreEvt, data, Other, SendOptions.SendReliable);
    }
    public void ReceivedRestoreEvt(object[] data)
    {
        int casterPunID = (int)data[0];
        int targetPunID = (int)data[1];
        int amount = (int)data[2];
        bool casterIsHand = (bool)data[3];

        // �����ڿ� ��� ã�� (�ֹ�ī��� �� ���� ��ġ�� ����)
        IBody caster = (casterIsHand == true) ?
            GAME.IGM.Hero.Enemy
            : GAME.IGM.allIBody.Find(x => x.PunId == casterPunID);
        IBody target = GAME.IGM.allIBody.Find(x => x.PunId == targetPunID);

        // ���κ��� ���� ġ���̺�Ʈ ������ ����
        GAME.IGM.AddAction(GAME.IGM.Battle.Restore(caster, target, amount));
    }
    #endregion

    #region Ż�� �̺�Ʈ ���� �� �ޱ�
    public void SendFatigue(int dmg)
    {
        PhotonNetwork.RaiseEvent(FatigueEvt, new object[] { dmg }, Other, SendOptions.SendReliable);
    }
    public void ReceivedFatigueEvt(object[] data)
    {
        int dmg = (int)data[0];
        // ���� ��������, Ż�����ط��� �Բ� �̺�Ʈ ���� ����
        GAME.IGM.AddAction(GAME.IGM.Hand.Fatigue.DeckExhausted(false, dmg));
    }
    #endregion

    #region �ڵ尡 10���ϋ� �߰���ο��� ī��� �Ҹ�Ǵ� �̺�Ʈ ����&�ޱ�
    public void SendOverDrawInfo(int cardID)
    { PhotonNetwork.RaiseEvent(OverDrawEvt , new object[] { cardID} , Other, SendOptions.SendReliable); }
    public void ReceivedOverDrawEvt(object[] data)
    {
        // ���� �Ҹ�Ǵ� ��� ī�尡 � ī������, ī��ĺ��ڷ� ã��
        int cardID = (int)data[0];

        // ī��ĺ��� �Ѱ��ָ�, ����� ��ο�ī�� �Ҹ� �̺�Ʈ ����
        GAME.IGM.AddAction(GAME.IGM.Hand.EnemyHandOverFlow(cardID));
    }
    #endregion

    #region ���ӿ��� ����
    public void SendGameEnd(bool isPlayerWin)
    { PhotonNetwork.RaiseEvent(GameEnd, new object[] { isPlayerWin }, Other, SendOptions.SendReliable); }
    public void GameEndStart(object[] data)
    {
        GAME.IGM.AddAction(GAME.IGM.EndingGame((bool)data[0], false));
    }
    #endregion
}
