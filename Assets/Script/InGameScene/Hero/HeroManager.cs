using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour
{
    public Hero Player, Enemy;
    private void Awake()
    {
        GAME.Manager.IGM.Hero = this;
    }

}
