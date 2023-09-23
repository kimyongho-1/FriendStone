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
        btnTmp.text = "���� ��";
        GAME.Manager.IGM.Turn = this;
        GAME.Manager.UM.BindEvent(
            this.gameObject,
            (GameObject go) => 
            {
                btnTmp.text = "��� ��";
                Debug.Log("TurnEnd!!"); 
            },
            Define.Mouse.ClickL,
            Define.Sound.Click
            );
    }

}
