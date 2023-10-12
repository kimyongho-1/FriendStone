using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static Define;

public partial class PacketManager
{
    // 61 ~ 80
    const byte SpellSpawn = 61;
    const byte SpellEnd = 62;

    public void InitSpellDictionary()
    {
        dic.Add(SpellSpawn, ReceivedSpellCard);
        dic.Add(SpellEnd, ReceivedEndingSpellCard);
    }

    #region �ֹ� ī�� ���
    public void SendUseSpellCard(int punID, int cardID)
    {
        object[] data = new object[] { punID, cardID };
        PhotonNetwork.RaiseEvent(SpellSpawn, data, Other, SendOptions.SendReliable);
    }

    public void ReceivedSpellCard(object[] data)
    {
        int punID = (int)data[0]; // ���� ȭ�鳻 � ī�� ��ü���� �ĺ���
        int cardID = (int)data[1]; // �ش� ī�尡 ���� � ī������ , ī�嵥����

        // ����� �ڵ�ī�尡 ��� �巡�׸� ���´��� ��ġ ã��
        Vector3 dest = new Vector3(0.5f, 2.25f, -0.5f);

        // ���ҽ� �Ŵ����� ��θ� ��ȯ �޴� ��ųʸ� ���� ī��Ÿ�԰� ī�嵥���� ã��
        Define.cardType type = GAME.Manager.RM.PathFinder.Dic[cardID].type;
        string jsonFile = GAME.Manager.RM.PathFinder.Dic[cardID].GetJson();

        // ī�� ������ ����
        SpellCardData card = JsonConvert.DeserializeObject<SpellCardData>
            (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

        // �ڵ�ī��, ���� ī�嵥���͸� ã�� �������ֱ�
        CardHand ch = GAME.IGM.Hand.EnemyHand.Find(punID);
        GAME.IGM.Hand.EnemyHand.Remove(ch);

        // � �ֹ�ī������ ���� + ����� ���� ����
        GAME.IGM.Hero.Enemy.MP -= card.cost;
        GAME.IGM.ShowSpellPopup(card,new Vector3(3.5f, 2.8f, -0.5f) );

        Debug.Log($"{punID}�� ���� {ch}�� {ch.cardName}���� Ȯ��");
        ch.SC = card;
        // ��밡 �巡�׸� ���� ��ġ�� �̵��ϴ� �ִϸ��̼��ڷ�ƾ ���� ����
        GAME.IGM.AddAction(spellHandCardMove(ch, dest));
        // ����� �ڵ�ī�尡 �ش� ��ġ�� �̵��ϴ� ��� �����ϱ�
        IEnumerator spellHandCardMove(CardHand ch, Vector3 dest)
        {
            float t = 0;
            Vector3 start = ch.transform.position;
            while (t < 1f)
            {
                t += Time.deltaTime;
                ch.transform.position = Vector3.Lerp(start, dest, t);
                yield return null;
            }
            // ���� ������ �ƴ� ����ȭ�� ����
            yield return GAME.IGM.StartCoroutine(ch.FadeOutCo(false, false));
        }
    }
    #endregion

    #region �ֹ�ī�� ��������
    public void SendEndingSpellCard(int punID)
    {
        object[] data = new object[] { punID, };
        PhotonNetwork.RaiseEvent(SpellEnd, data, Other, SendOptions.SendReliable);
    }
    public void ReceivedEndingSpellCard(object[] data)
    {
        int punID = (int)data[0]; // ���� ȭ�鳻 � ī�� ��ü���� �ĺ���

        // �ٽ� ī�� ã��
        CardHand ch = GAME.IGM.Hand.AllCardHand.Find(x => x.PunId == punID);
        GAME.IGM.AddAction(RemoveSpellCard(ch));
        IEnumerator RemoveSpellCard(CardHand ch)
        {
            Debug.Log($"{punID}�� ���� {ch.SC.cardName}�� ���� Ȯ��");
            // �ڵ�Ŵ������� �� �ڵ�ī��� ������ ����
            yield return StartCoroutine(GAME.IGM.Hand.CardAllignment(false));
            // ��밡 � ī�� ����ߴ��� �����ִ� ī���˾� ����
            GAME.IGM.cardPopup.isEnmeySpawning = false;
            GAME.IGM.cardPopup.gameObject.SetActive(false);
            
            // �ֹ�ī�� ���� ����Ͽ��⿡ ���� ����
            GAME.IGM.Hand.AllCardHand.Remove(ch);
            GameObject.Destroy(ch.gameObject);
        }

    }
    #endregion
}