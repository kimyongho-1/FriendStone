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

    // �̴Ͼ��̳� �ֹ�ī���� ����, ���� ������ �������ִٸ� �ش� �����鸸 ����Ʈ���μ����� ���� ������ ���� �������ֱ�
    public void StartMaskingArea(string[] layers)
    {
        if (layers.Length == 0) { return; }

        // ����ī�޶��� ����Ʈ���μ��� On
        camData.renderPostProcessing = true;
        cam.cullingMask = 0;
        // ���� �����ؾ��� ���̾ ����
        for (int i = 0; i < layers.Length; i++)
        {
            int layer = LayerMask.NameToLayer(layers[i]);
            // ����ī�޶󿡼� �ش� ���̾� ����
            mainCam.cullingMask &= ~(1 << layer); 
            // ���� ��Ŀ��ī�޶󿡼��� �ش� ���̾���� �ߺ������ϱ⿡ �������� ���� ���̾� �ֱ�
            cam.cullingMask |= (1 << layer); 
        }

        // ������ �׻� ���ԵǴ°� �߰� : Ÿ��Ƽ�� ȭ��ǥ + ������ ��ǳ�� ���
        int arrowLayer = LayerMask.NameToLayer("Always");
        cam.cullingMask |= (1 << arrowLayer);
        mainCam.cullingMask &= ~(1 << arrowLayer);

        this.gameObject.SetActive(true);
    }

    // ����Ʈ���μ��� ���� �ʱ�ȭ
    public void ExitMaskingArea()
    {
        if (this.gameObject.activeSelf == false) { return; }
        // ����ī�޶��� �ø�����ũ ���� ��ü�� �ٽ� �ʱ�ȭ
        mainCam.cullingMask = 0;
        mainCam.cullingMask = ~0;
        // ����ī�޶��� ��ó�� ȿ�� �ɼǲ���
        camData.renderPostProcessing = false;
        this.gameObject.SetActive(false);
    }

    public void ReadyOutro()
    {
        camData.renderPostProcessing = true;
        Blooms.active = ColorAdj.active = false;
        Vig.active = true;
        // �̴Ͼ� ��������
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
