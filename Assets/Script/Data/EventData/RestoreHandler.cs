using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RestoreHandler : CardBaseEvtData
{
    public Define.restoreAutoMode restoreAutoMode;
    public Define.restoreFX restoreFX;
    public int restoreAmount;
}
