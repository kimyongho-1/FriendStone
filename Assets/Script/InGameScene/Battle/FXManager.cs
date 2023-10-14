using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{

    List<IFx> fxList = new List<IFx>();
    public GameObject AttPrefab;
    public GameObject HealPrefab;
    public GameObject BuffPrefab;
    public Transform Projectile;

    public IFx GetFX(Define.fxType t)
    {
        // 사용간으한 프리팹 존재하는지 찾기 (동일 타입 존재여부 && 현재 사용안하는것 )
        IFx prefab = fxList.Find(x => x.FXtype == t && x.currUsing == false) ;

        // 현재 사용가능한 fx프리팹 없으면 새로생성
        if (prefab == null)
        {
            switch (t)
            {
                case Define.fxType.Projectile:
                    prefab = GameObject.Instantiate(AttPrefab, Projectile).GetComponent<IFx>();
                    break;
                case Define.fxType.Heal:
                    prefab = GameObject.Instantiate(HealPrefab, Projectile).GetComponent<IFx>();
                    break;
                case Define.fxType.Buff:
                    prefab = GameObject.Instantiate(BuffPrefab, Projectile).GetComponent<IFx>();
                    break;
                default:
                    break;
            }
            prefab.Tr.SetParent(Projectile);
            fxList.Add(prefab);
        }

        return prefab;
    }

}
