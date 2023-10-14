using System;
using System.Collections;
using UnityEngine;

public interface IFx
{
    public Define.fxType FXtype { get; }
    public IEnumerator Invoke(IBody attacker, IBody target);
    public bool currUsing { get; set; }
    public Transform Tr { get;  }
}