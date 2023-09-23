using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TurnEndBtn : MonoBehaviour
{
    public Collider2D Col;
    public TextMeshPro btnTmp;
    private void Awake()
    {
        btnTmp.text = "나의 턴";
        GAME.Manager.IGM.Turn = this;
        GAME.Manager.UM.BindEvent(
            this.gameObject,
            (GameObject go) => 
            {
                btnTmp.text = "상대 턴";
                Debug.Log("TurnEnd!!"); 
            },
            Define.Mouse.ClickL,
            Define.Sound.Click
            );
    }

}
