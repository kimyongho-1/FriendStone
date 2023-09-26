using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour
{
    public Hero Player, Enemy;
    private void Awake()
    {
        GAME.IGM.Hero = this;
    }

}
