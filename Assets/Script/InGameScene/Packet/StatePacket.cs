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
    const byte StartIntro = 1;
    const byte InitDraw = 2;
    const byte IsOffensive = 3;
    const byte UserDraw = 4;
    const byte OtherTurnEnd = 5;
    const byte OtherTurnStartMSG = 6;
    const byte DoDraw = 7;

    const byte FindEvt = 10;
    const byte FindEvtResult = 11;
    const byte AcqusitionEvt = 12;
    public void InitStateDictionary()
    {
        dic.Add(UserInfo, ReceivedUserInfo);
        dic.Add(StartIntro, (object[] o)=> { GAME.IGM.AddAction(GAME.IGM.TC.StartIntro()); } );  // �� �÷��̾� �غ�ɽ�, ��Ʈ�� �ִϸ��̼� ����
        dic.Add(InitDraw, (object[] o) => { GAME.IGM.AddAction(GAME.IGM.Hand.CardDrawing(4)); });
        dic.Add(IsOffensive, ReceivedOffensiveResult);
        dic.Add( UserDraw , ReceivedOtherDraw );
        dic.Add(OtherTurnEnd, ReceivedTurnEnd);
        dic.Add(OtherTurnStartMSG, ShowOtherTurn);
        dic.Add(DoDraw, ReceivedDoDraw);
        dic.Add(FindEvt, ReceivedFindEvt);
        dic.Add(FindEvtResult, ReceivedResultFindEvt);
        dic.Add(AcqusitionEvt, ReceivedAcquisition );
        
    }

    // ���� ���� ����
    public void SendMyTurnMSG()
    {
        // ��� ������ �˸���
        PhotonNetwork.RaiseEvent(OtherTurnStartMSG, null, Other, SendOptions.SendReliable);
    }
    // ����� �� ���۽�, ����� �� ���� �޽��� ��� 
    public void ShowOtherTurn(object[] data)
    {
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
        string nickName = (string)data[0];
        Define.classType classType = (Define.classType)(data[1]);

        // �� �г��� �ʱ�ȭ
        GAME.IGM.Hero.Enemy.nickTmp.text = nickName;
        // �� ���� ĵ���� �ʱ�ȭ
        GAME.IGM.Hero.Enemy.heroSkill.InitEnemySkill(GAME.IGM.Hero.Enemy, classType);
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
            GAME.IGM.AddAction(GAME.IGM.Turn.EndMyTurn());
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
        // ����� ��ο� ����
        StartCoroutine(GAME.IGM.Hand.EnemyCardDrawing((int)data[0]));
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
            new object[] { GAME.IGM.GameTurn + 1 },
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
        IBody caster = (isMinionAct) ? GAME.IGM.allIBody.Find(x=>x.PunId == punID) : GAME.IGM.Hand.EnemyHand.Find(x => x.PunId == punID);
        
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

        }
    }
    
    #endregion
}
