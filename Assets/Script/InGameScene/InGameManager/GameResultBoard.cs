using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameResultBoard : MonoBehaviour
{
    // ½ÂÆÐ È¿°úÀ½ 
    public AudioClip[] clips;
    public TextMeshPro resultTmp, leftBtn;
    public SpriteRenderer resultSr;
    
    AudioSource audioPlayer;
    private void Awake()
    {
        GAME.Manager.CurrScene = Define.Scene.InGame;
        GAME.IGM.GRB = this;
        audioPlayer = GetComponent<AudioSource>();
        GAME.Manager.UM.BindEvent(leftBtn.gameObject,
            (GameObject go) => { SceneManager.LoadScene("Lobby"); },
            Define.Mouse.ClickL );
        this.gameObject.SetActive(false);
        
    }
    public void ReloadClip(bool isPlayerWin)
    {
        audioPlayer.clip = (isPlayerWin) ? clips[0] : clips[1];
    }
    public void Play() { audioPlayer.Play(); }
}
