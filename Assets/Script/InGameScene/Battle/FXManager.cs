using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    List<ParticleSystem> pjList = new List<ParticleSystem>();
    public GameObject PJprefab;
    public Transform Projectile;
    private void Awake()
    {
        ParticleSystem prefab = GameObject.Instantiate(PJprefab, Projectile).GetComponent<ParticleSystem>();
        prefab.transform.SetParent(Projectile);
        pjList.Add(prefab);
    }
    public ParticleSystem GetPJ
    {
        get
        {
            ParticleSystem pj = pjList.Find(x => x.gameObject.activeSelf == false);
            if (pj == null)
            {
                ParticleSystem prefab = GameObject.Instantiate(PJprefab, Projectile).GetComponent<ParticleSystem>();
                prefab.transform.SetParent(Projectile);
                pjList.Add(prefab);
                return prefab;
            }
            else
            {
                return pj;
            }
            
        }
    }

}
