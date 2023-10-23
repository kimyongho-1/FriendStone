using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class SoundManager
{
    public AudioClip Left, Right; // 좌우클릭 기본 효과음
    public AudioSource BGM, FX, Speech; // 오디오소스 참조
    public List<AudioClip> BGMS = new List<AudioClip>(); // 배경음 리스트
    public Dictionary<Define.Sound , AudioClip> soundDic = new Dictionary<Define.Sound, AudioClip>();
    // 생성자 : GAME.CS Awake에서 실행
    public SoundManager(AudioSource b, AudioSource f, AudioSource s)
    {
        BGMS.AddRange(Resources.LoadAll<AudioClip>("Sound/BGM"));
        BGM = b;
        FX = f;
        Speech = s;
    }



    // 씬별 배경음 재생
    public void PlayBGM()
    {
        switch (GAME.Manager.CurrScene)
        {
            case Define.Scene.Login:
                BGM.clip = BGMS.Find(x=>x.name == "Lobby") ; break;
            case Define.Scene.Lobby:
                BGM.clip = BGMS.Find(x => x.name == "Lobby"); break;
            case Define.Scene.InGame:
                List<AudioClip> clips = BGMS.FindAll(x => x.name.Contains("Battle"));
                int rand = UnityEngine.Random.Range(0, clips.Count - 1);
                BGM.clip = clips[rand];
                break;
        }

        BGM.Play();
    }

    #region 로비씬의 옵션팝업창내 볼륨조절
    public float masterVolumeRate = 1.0f;  // 전체 볼륨 조절률
    float bgmVol = 0.1f;
    float fxVol = 1f;
    public void ChangedVol(float rate)
    {
        masterVolumeRate = rate;
        BGM.volume = Mathf.Clamp(bgmVol * masterVolumeRate, 0, 1f);
        FX.volume = Mathf.Clamp(fxVol * masterVolumeRate, 0, 1f);
    }
    public void BGMVol(float val)
    {
        BGM.volume = Mathf.Clamp(val * bgmVol * masterVolumeRate, 0, 1f);
    }
    public void FXVol(float val)
    {
        FX.volume = Mathf.Clamp(val * fxVol * masterVolumeRate, 0, 1f);
    }
    #endregion

}
