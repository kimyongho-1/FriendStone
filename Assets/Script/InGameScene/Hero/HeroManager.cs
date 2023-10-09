using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour
{
    public Hero Player, Enemy;
    private void Awake()
    {
        GAME.IGM.Hero = this;
        // �� ������ �� ������ �ѹ����� �ݴ�� + ������ Ŭ�� �������� ����
        Player.PunId = ((Photon.Pun.PhotonNetwork.IsMasterClient) ? 1000 : 2000);
        Player.heroSkill.PunId = Player.PunId + 1;
        Enemy.PunId = ((Player.PunId == 2000) ? 1000 : 2000);
        Enemy.heroSkill.PunId = Enemy.PunId + 1;
    }

}
