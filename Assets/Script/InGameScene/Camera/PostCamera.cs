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
    LayerEnums layerBits;
    private void Awake()
    {
        GAME.IGM.Post = this;
        cam = GetComponent<Camera>();
        mainCam = Camera.main;
        Vol.profile.TryGet<Vignette>(out Vig);
        Vol.profile.TryGet<ColorAdjustments>(out ColorAdj);
        layerBits = LayerEnums.ally | LayerEnums.foe;
        //cam.cullingMask = (int)layerBits;

        mainCam.cullingMask = ~0;
        mainCam.TryGetComponent<UniversalAdditionalCameraData>(out camData);
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

        // ȭ��ǥ�� ��� �ٲ��ֱ�
        int arrowLayer = LayerMask.NameToLayer("ArrowPointer");
        cam.cullingMask |= (1 << arrowLayer);
        mainCam.cullingMask &= ~(1 << arrowLayer);

        this.gameObject.SetActive(true);
    }
    public void ExitMaskingArea()
    {
        if (this.gameObject.activeSelf == false) { return; }
        mainCam.cullingMask = 0;
        mainCam.cullingMask = ~0;
        camData.renderPostProcessing = false;
        this.gameObject.SetActive(false);
    }
}
// & and
// | or
// ~ not
// ^ xor
