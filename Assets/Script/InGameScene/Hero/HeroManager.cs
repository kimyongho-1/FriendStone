using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour
{
    public Hero Player, Enemy;
    private void Awake()
    {
        GAME.IGM.Hero = this;
        // 적 영웅과 내 영웅의 넘버링은 반대로 + 마스터 클라를 기준으로 선정
        Player.PunId = ((Photon.Pun.PhotonNetwork.IsMasterClient) ? 1000 : 2000);
        Player.heroSkill.PunId = Player.PunId + 1;
        Enemy.PunId = ((Player.PunId == 2000) ? 1000 : 2000);
        Enemy.heroSkill.PunId = Enemy.PunId + 1;
    }

}
