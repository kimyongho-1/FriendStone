using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostCamera : MonoBehaviour
{
    [System.Flags]
    enum LayerEnums
    {
        ally = 1 << 6,
        foe = 1 << 7,
        allyHero = 1 << 8,
        foeHero = 1 << 9,
    }
    
    public Volume Vol;
    Camera cam, mainCam;
    UniversalAdditionalCameraData camData;
    Vignette Vig;
    ColorAdjustments ColorAdj;
    Bloom Blooms;
    LayerEnums layerBits;
    private void Awake()
    {
        Camera.main.aspect = 16f / 9f;
        GAME.IGM.Post = this;
        cam = GetComponent<Camera>();
        mainCam = Camera.main;
        Vol.profile.TryGet<Vignette>(out Vig);
        Vol.profile.TryGet<ColorAdjustments>(out ColorAdj);
        Vol.profile.TryGet<Bloom>(out Blooms);
        layerBits = LayerEnums.ally | LayerEnums.foe;
        //cam.cullingMask = (int)layerBits;

        mainCam.cullingMask = ~0;
        mainCam.TryGetComponent<UniversalAdditionalCameraData>(out camData);
        camData.renderPostProcessing = true; 

        camData.renderPostProcessing = false;
        this.gameObject.SetActive(false);
    }

    // 미니언이나 주문카드의 사용시, 만약 범위가 지정되있다면 해당 범위들만 포스트프로세싱의 범위 밖으로 뺴어 강조해주기
    public void StartMaskingArea(string[] layers)
    {
        if (layers.Length == 0) { return; }

        // 메인카메라의 포스트프로세싱 On
        camData.renderPostProcessing = true;
        cam.cullingMask = 0;
        // 현재 설정해야할 레이어만 제거
        for (int i = 0; i < layers.Length; i++)
        {
            int layer = LayerMask.NameToLayer(layers[i]);
            // 메인카메라에서 해당 레이어 제거
            mainCam.cullingMask &= ~(1 << layer); 
            // 현재 포커싱카메라에서는 해당 레이어들이 잘보여야하기에 렌더링을 위해 레이어 넣기
            cam.cullingMask |= (1 << layer); 
        }

        // 강조에 항상 포함되는것 추가 : 타겟티의 화살표 + 영웅의 말풍선 등등
        int arrowLayer = LayerMask.NameToLayer("Always");
        cam.cullingMask |= (1 << arrowLayer);
        mainCam.cullingMask &= ~(1 << arrowLayer);

        this.gameObject.SetActive(true);
    }

    // 포스트프로세싱 범위 초기화
    public void ExitMaskingArea()
    {
        if (this.gameObject.activeSelf == false) { return; }
        // 메인카메라의 컬링마스크 범위 전체로 다시 초기화
        mainCam.cullingMask = 0;
        mainCam.cullingMask = ~0;
        // 메인카메라의 후처리 효과 옵션끄기
        camData.renderPostProcessing = false;
        this.gameObject.SetActive(false);
    }

    public void ReadyOutro()
    {
        camData.renderPostProcessing = true;
        Blooms.active = ColorAdj.active = false;
        Vig.active = true;
        // 미니언 꺼버리기
        GAME.IGM.Hand.PlayerHandGO.SetActive(false);    
    }
    public IEnumerator Outro()
    {
       
        float t = 0;
        while (t < 1f)
        { 
            t += Time.deltaTime * 0.3f;
            Vig.intensity.value = Mathf.Lerp(0,0.65f,t);
            yield return null;
        }

    }
  
}
// & and
// | or
// ~ not
// ^ xor
