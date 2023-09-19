using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackHandler : CardBaseEvtData
{
    public Define.attTargeting attTargeting;
    public Define.attType attType;
    public Define.attFX attFX;
    public int attAmount;
}