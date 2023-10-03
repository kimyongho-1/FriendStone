using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PacketManager
{
    // 41 ~ 60
    const byte UseWeapon = 41;
    const byte EnemyHeroAttack = 42;

    public void InitHeroDictionary()
    {
        dic.Add(UseWeapon, ReceivedWeapon);

        dic.Add(EnemyHeroAttack, ReceivedHeroAttack);

    }
    #region ���� ���� ����
    // ���� ���� ���� �̺�Ʈ ����
    public void SendWeapon(int punID, int cardID, float x, float y, float z)
    {
        object[] data = new object[] { punID, cardID , x,y,z};
        PhotonNetwork.RaiseEvent(UseWeapon,data, Other, SendOptions.SendReliable);
    }

    // �� ������ ���� ī�� ����, �ش� �̺�Ʈ ���޹޾� ����ȭ ����
    public void ReceivedWeapon(object[] data)
    { 
        // ī�尴ü�ѹ� + ī�嵥���ͳѹ� + ������ �巡�� ���� ��ġ
        int punID = (int)data[0];
        int cardID = (int)data[1];
        Vector3 dest = new Vector3((float)data[2], (float)data[3], (float)data[4]);

        // �ڵ�ī�� ��ü ã��
        CardHand ch = GAME.IGM.Hand.EnemyHand.Find(x=>x.PunId == punID);

        // �� �ڵ忡�� ����߱⿡ ����Ʈ���� ����
        GAME.IGM.Hand.EnemyHand.Remove(ch);

        // ���ҽ� �Ŵ����� ��θ� ��ȯ �޴� ��ųʸ� ���� ī��Ÿ�԰� ī�嵥���� ã��
        Define.cardType type = GAME.Manager.RM.PathFinder.Dic[cardID].type;
        string jsonFile = GAME.Manager.RM.PathFinder.Dic[cardID].GetJson();

        // ī�� ������ ����
        CardData card = JsonConvert.DeserializeObject<WeaponCardData>
            (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

        ch.Init(card,false);

        // ���� ī�带 ���� �ִϸ��̼� ���� ����
        GAME.IGM.AddAction(weaponCardMove());
        IEnumerator weaponCardMove()
        {
            float t = 0;
            Vector3 start = ch.transform.position;
            while (t < 1f) 
            {
                t += Time.deltaTime * 2f;
                ch.transform.position =
                    Vector3.Lerp(start,dest,t);
                yield return null;
            }
        }

        // ���� ���� ���� �̺�Ʈ ����
        GAME.IGM.AddAction(GAME.IGM.Hero.Enemy.EquipWeapon(ch,card));
    }
    #endregion

    #region �� ���� ���� �̺�Ʈ

    // �� ������ �����ϴ� �̺�Ʈ�� ����
    public void SendHeroAttack(int attackerID, int targetID)
    {
        object[] data = new object[] { attackerID, targetID };
        PhotonNetwork.RaiseEvent(EnemyHeroAttack, data, Other, SendOptions.SendReliable);
    }
    // ������ ���� �̺�Ʈ �޾Ƽ� ����
    public void ReceivedHeroAttack(object[] data)
    {
        // �����ڿ� Ÿ���� �ĺ� ���� Ȯ��
        int attackerID = (int)data[0];
        int targetID = (int)data[1];

        // �����ڿ� Ÿ�� ã��
        // �����ڰ� �����ΰ��� �˰� �ִ� ����
        Hero enemyHero = GAME.IGM.allIBody.Find(x => x.PunId == attackerID).TR.GetComponentInParent<Hero>();
        IBody target = GAME.IGM.allIBody.Find(x => x.PunId == targetID);

        // �� ���� ���� ����
        GAME.IGM.AddAction(enemyHero.AttackCo(enemyHero, target));

    }
    #endregion

}