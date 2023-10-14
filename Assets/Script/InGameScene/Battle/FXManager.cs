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
        // ��밣���� ������ �����ϴ��� ã�� (���� Ÿ�� ���翩�� && ���� �����ϴ°� )
        IFx prefab = fxList.Find(x => x.FXtype == t && x.currUsing == false) ;

        // ���� ��밡���� fx������ ������ ���λ���
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
