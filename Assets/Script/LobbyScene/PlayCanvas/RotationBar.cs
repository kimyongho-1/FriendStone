using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RotationBar : MonoBehaviour
{
    public Material mat;
    public SelectedDeckIcon sdi;
    // ���� ������ ���ؼ� �������� ����
    public bool stop = false;

    void OnEnable()
    {
        mat.SetFloat("_Wheel", 0);
        StartCoroutine(rotate());
    }
    void OnDisable()
    {
        StopAllCoroutines();
        stop = false;
        
    }
    public float speedLimit = 5;
    IEnumerator rotate()
    {
        float moreSpeed = 0f;
        IEnumerator Speed()
        {
            while (stop == false)
            {
                moreSpeed = Mathf.Min(moreSpeed + Time.deltaTime, speedLimit);
                yield return null;
            } 
            while (moreSpeed > 0)
            {
                moreSpeed = Mathf.Max(moreSpeed - Time.deltaTime *2.5f , 0);
                yield return null;
            }
        }
        StartCoroutine(Speed());
        float t = 0;
        while (moreSpeed > 0)
        {
            t += Time.deltaTime * moreSpeed;
            mat.SetFloat("_Wheel",t % 1);
            yield return null;
        }
        sdi.matchingState.gameObject.SetActive(true);   
        sdi.matchingState.text = "<color=red>������ ��븦 ã�Ҵ�!";

        float currVal = mat.GetFloat("_Wheel");
        currVal = currVal % 1; // �Ҽ����� �����
        t = 0;
        // ������ ���, ��� �ؽ�ó�� ���ƿ��� ����
        while (t < 1)
        {
            t += Time.deltaTime * 0.5f;
            mat.SetFloat("_Wheel", Mathf.Lerp(currVal, 1, t));
            yield return null;
        }


        // ������ Ŭ���̾�Ʈ��, ���ӽ���
        if (PhotonNetwork.IsMasterClient)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log("�����Ͱ� ��ȯ ����");
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.LoadLevel("InGame");
        }
    }
}
