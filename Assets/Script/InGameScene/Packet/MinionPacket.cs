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

    public void InitMinionDictionary()
    {
        dic.Add(EnemySpawn, ReceivedMinionSpawn);
        dic.Add(EnemyAtt, ReceivedMinionAttack); 
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

            // ��밡 ��ȯ�� �̴Ͼ��� ���� ������ ǥ��
            GAME.IGM.ShowSpawningMinionPopup(mc, currAtt, currHp, currCost);

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
}