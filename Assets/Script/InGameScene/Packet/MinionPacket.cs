using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public partial class PacketManager
{
    // 21 ~ 40
    const byte EnemySpawn = 21;
    const byte EnemyAtt = 22;
    const byte WaitDeathRattleEnd = 23;

    const byte DeathRattleRestoreEvt = 24;
    const byte DeathRattleAttEvt = 25;
    const byte DeathRattleBuffEvt = 26;
    public void InitMinionDictionary()
    {
        dic.Add(EnemySpawn, ReceivedMinionSpawn);
        dic.Add(EnemyAtt, ReceivedMinionAttack);
        dic.Add(WaitDeathRattleEnd , DeathRattleEnd );
        dic.Add(DeathRattleRestoreEvt , ReceivedDeathRestoreEvt);
        dic.Add(DeathRattleAttEvt , ReceivedDeathAttEvt);
        dic.Add(DeathRattleBuffEvt, ReceivedDeathBuffEvt);
    }

    #region �̴Ͼ� ��ȯ �̺�Ʈ ���� �ޱ�

    public void SendMinionSpawn(int punID, int fieldIdx, int cardID , int currAtt, int currHp, int currCost ) // ���� att hp cost
    {
        object[] data = new object[] { punID , fieldIdx , cardID, currAtt, currHp, currCost};
        PhotonNetwork.RaiseEvent(EnemySpawn, data, Other, SendOptions.SendReliable);
    }

    // ����� �̴Ͼ� ��ȯ �̺�Ʈ�� ���� �޾� ����ȭ ����
    public void ReceivedMinionSpawn(object[] data)
    {
        int punID = (int)data[0]; // ���� ȭ�鳻 � ī�� ��ü���� �ĺ���
        int fieldIdx = (int)data[1]; // �ʵ峻 ���° ��ġ�� ��ȯ�ɰ���
        int cardID = (int)data[2]; // �ش� ī�尡 ���� � ī������ , ī�嵥����

        // ����Ǿ����� �� ������ ��ü�� �ޱ�
        int currAtt = (int)data[3];
        int currHp = (int)data[4];
        int currCost = (int)data[5];
        Debug.Log($"�̴Ͼ� ��ȯ ���� �ޱ� ����, punID {punID}");
        // �����Ͱ� �����ϱ⿡, ��ȯ �̺�Ʈ ����
        GAME.IGM.AddAction(MakeEnemyData(punID, fieldIdx, cardID, currAtt, currHp, currCost));

        IEnumerator MakeEnemyData(int punID, int fieldIdx, int cardID, int currAtt, int currHp, int currCost)
        {
            // ���ҽ� �Ŵ����� ��θ� ��ȯ �޴� ��ųʸ� ���� ī��Ÿ�԰� ī�嵥���� ã��
            Define.cardType type = GAME.Manager.RM.PathFinder.Dic[cardID].type;
            string jsonFile = GAME.Manager.RM.PathFinder.Dic[cardID].GetJson();

            // ī�� ������ ����
            MinionCardData mc = JsonConvert.DeserializeObject<MinionCardData>
                (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

            // ��밨��
            GAME.IGM.Hero.Enemy.MP -= currCost;

            // ��밡 ��ȯ�� �̴Ͼ��� ���� ������ ǥ��
            GAME.IGM.ShowEnemyMinionPopup(mc, currAtt, currHp, currCost);

            // �̴Ͼ� ��ȯ��, ���� ī�嵥���͸� ã�� �������ֱ�
            GAME.IGM.Hand.EnemyHand.Find(punID).MC = mc;
            yield return GAME.IGM.StartCoroutine(GAME.IGM.Spawn.EnemySpawn(punID, fieldIdx, currAtt, currHp, currCost));
        }

    }
    #endregion

    #region �̴Ͼ� ���� �̺�Ʈ

    // �� �̴Ͼ� ���� ����� �����ϱ� ( ��������id, Ÿ���� id )
    public void SendMinionAttack(int attackerID, int targetID)
    {
        object[] data = new object[] { attackerID, targetID };
        PhotonNetwork.RaiseEvent(EnemyAtt, data , Other, SendOptions.SendReliable);
    }

    // ���� ���� ��� �̴Ͼ� ���� �̺�Ʈ ���� �ޱ�
    public void ReceivedMinionAttack(object[] data)
    {
        // �����ڿ� Ÿ���� �ĺ� ���� Ȯ��
        int attackerID = (int)data[0];
        int targetID = (int)data[1];

        // �����ڿ� Ÿ�� ã��
        IBody attacker = GAME.IGM.allIBody.Find(x => x.PunId == attackerID);
        IBody target = GAME.IGM.allIBody.Find(x => x.PunId == targetID);

        // ���� �̺�Ʈ�� �̴Ͼ� ������ �������� �ϱ⿡ , CardField������Ʈ ã��
        CardField minion = attacker.TR.GetComponent<CardField>();

        // �׸��� ���� �̺�Ʈ ����
        GAME.IGM.AddAction(minion.AttackCo(attacker, target));
    }
    #endregion

    #region �̴Ͼ��� ������ ������ ���� �̺�Ʈ ���� �� �ޱ�
    public void SendDeathBuffEvt(int targetPunID, Define.buffType type, int att, int hp)
    {
        object[] data = new object[] { targetPunID, (int)type, att, hp };
        // ���� ����
        PhotonNetwork.RaiseEvent(DeathRattleBuffEvt, data, Other, SendOptions.SendReliable);
    }
    public void ReceivedDeathBuffEvt(object[] data)
    {
        int targetPunID = (int)data[0];
        Define.buffType type = (Define.buffType)data[1];
        int att = (int)data[2];
        int hp = (int)data[3];

        GAME.IGM.AddDeathAction(DelayedBuff(targetPunID, att, hp, type));
        IEnumerator DelayedBuff(int targetPunID, int att, int hp, Define.buffType type)
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

    #region �̴Ͼ��� ������ ������ ġ�� �̺�Ʈ ���� �� ���޹ޱ�
    public void SendDeathRestoreEvt(int casterID, int targetID, int amount, bool casterIsHandCard)
    {
        object[] data = new object[] { casterID, targetID, amount, (object)casterIsHandCard };
        PhotonNetwork.RaiseEvent(DeathRattleRestoreEvt, data, Other, SendOptions.SendReliable);
    }
    public void ReceivedDeathRestoreEvt(object[] data)
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
        GAME.IGM.AddDeathAction(GAME.IGM.Battle.Restore(caster, target, amount, true));
    }
    #endregion


    #region �̴Ͼ��� ������ ������ ���� �̺�Ʈ ���� �� �ޱ�
    public void SendDeathAttEvt(int attackerPunID, int targetPunID, Define.attType type, int attAmount, Define.ObjType objType)
    {
        object[] data = new object[] { attackerPunID, targetPunID, (int)type, attAmount, (int)objType };
        PhotonNetwork.RaiseEvent(DeathRattleAttEvt, data, Other, SendOptions.SendReliable);
    }
    public void ReceivedDeathAttEvt(object[] data)
    {
        int attackerPunID = (int)data[0];
        int targetPunID = (int)data[1];
        Define.attType attType = (Define.attType)data[2];
        int attAmount = (int)data[3];
        Define.ObjType objType = (Define.ObjType)data[4];

        IBody target = GAME.IGM.allIBody.Find(x => x.PunId == targetPunID);
        // ���� �̺�Ʈ�� ������, �������� objŸ���� �̴Ͼ��̶�� �̴Ͼ��� ã��
        // �׿ܿ��� �� ������ �����ڷ� ���� (���� ����Ʈ�� ������ġ�� �� �������� �����ϱ� ���ϱ⿡)
        IBody attacker = (objType == Define.ObjType.Minion) ?
            GAME.IGM.allIBody.Find(x => x.PunId == attackerPunID)
            : GAME.IGM.Hero.Enemy;

        // ���κ��� ���� �����̺�Ʈ ������ ����
        GAME.IGM.AddDeathAction(
            GAME.IGM.Battle.AttackEvt(attacker, target, attAmount, attType, true)
            );
    }

    #endregion

    #region ������ �޾Ƹ� ���Ḧ ��ٸ���
    public void SendDeathRattleEnd(int punID)
    {
        Debug.Log("Send EndOfDeathRattle");
        PhotonNetwork.RaiseEvent(WaitDeathRattleEnd, new object[] { punID}, Other, SendOptions.SendReliable);
    }
    public void DeathRattleEnd(object[] data)
    {
        Debug.Log("Received EndOfDeathRattle");
        int punID = (int)data[0];
        // �� �̺�Ʈ�� ��������, ���������� ���� ������ ������ �̺�Ʈ�� �޾� �������ϰ��̹Ƿ�
        // ���������� ��� ������ �̺�Ʈ ���� �Ϸ� ��ȣ�� ���޹޾� ����ȭ���ֱ�
        GAME.IGM.AddDeathAction(FinishingDeathRattle(punID));
        IEnumerator FinishingDeathRattle(int punID)
        {
            yield return null;
            IBody deadMan = GAME.IGM.allIBody.Find(x=>x.PunId == punID);

            // ���� �̴Ͼ� ������ �̺�Ʈ ������ �����ϱ⿡
            deadMan.TR.GetComponent<CardField>().waitDeathRattleEnd = true;
        }
    }
    #endregion
}