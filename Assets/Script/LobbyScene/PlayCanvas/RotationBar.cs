using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RotationBar : MonoBehaviour
{
    public List<RotateOBJ> list = new List<RotateOBJ>();
    public float angle = 0;
    public float radius = 5.0f;
    public float speed = 30.0f;
    public float zThick = 0.5f;
    public Vector3 scale;
    public SelectedDeckIcon sdi;
    // ���� ������ ���ؼ� �������� ����
    public static bool stop = false;

    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            list.Add(transform.GetChild(i).GetComponent<RotateOBJ>());
            // �ʱ� �� ����
            list[i].startRotate = list[i].rotate = 270f - (angle * i);
        }
        
    }
    
    void OnEnable()
    {
        StartCoroutine(rotate());
    }
    void OnDisable()
    {
        StopAllCoroutines();
        stop = false;
        speed = 125f;
    }
    IEnumerator rotate()
    {
        // �ݸŴ������� ��Ī ������ �����̼��� �ӵ��� ���ߴ� �ڷ�ƾ
        StartCoroutine(MatchingTimer());
        IEnumerator MatchingTimer()
        {
            float t = 0;
            float m = speed;
            // �ݸŴ������� ��Ī ������ stop ������ true�� ����
            yield return new WaitUntil(() => (stop == true));
            while (t < 1f)
            {
                t += Time.deltaTime;
                speed = Mathf.Lerp(m, 0, t);
                yield return null;
            }
        }

        // ���ǵ尡 �����ϴ°�, �ݸŴ������� ��Ī ������ 
        // ���� ��ĪŸ�̸� �ڷ�ƾ���� �ӵ����� ����
        while (speed > 0)
        {
            // ��Ī�� ���������� ��� Bar�� ��ü�������� ȸ����Ű��
            for (int i = 0; i < list.Count; i++)
            {
                // ���� 270ȸ������ �ֻ��� ������ �ǹ��ؼ� �ٽ� �ʱ�ȭ
                if (list[i].rotate > 270f)
                { list[i].rotate = list[i].rotate % 270f + 90f; } // 90F���� �ؿ��� ����

                // ȸ�� �� ����
                list[i].rotate += Time.deltaTime * speed;
                float z = Mathf.Cos( list[i].rotate * Mathf.Deg2Rad) * radius * zThick;
                float y = -Mathf.Sin(list[i].rotate * Mathf.Deg2Rad) * radius ;
                
                // �ڷ� ������ �������� 0���� �ٿ� �Ⱥ��̰� ����
                list[i].tr.localScale =scale * Mathf.Abs(Mathf.Cos(list[i].rotate * Mathf.Deg2Rad));
                list[i].tr.localPosition = new Vector3(0, y, z);// + 400f
            }
            yield return null;
        }

        Debug.Log("RotationBar ���� ����");
        // ��Ī�� �������� ��� Bar���� �ʱ� ��ġ�� �ǵ����� (list[i]�� startRotate������ �����ϱ���� ȸ��)
        for (int i = 0; i < list.Count; i++)
        {
            StartCoroutine(indieRotate(list[i]));
            IEnumerator indieRotate(RotateOBJ r)
            {
                // �Ҽ����� ����ġ�� rotate�� startRotate�� ��ġ�����ʱ� ������ ������ ���߱�
                r.rotate = (float)Mathf.CeilToInt(r.rotate);

                // ȸ���� �ϴٰ� ������ ȸ�������� ���ƿý� ������ (�ʱ� ��ġ�� �°��̱� ������)
                while (r.rotate == r.startRotate)
                {
                    // ���� 270ȸ������ �ֻ��� ������ �ǹ��ؼ� �ٽ� �ʱ�ȭ
                    if (r.rotate > 270f)
                    { r.rotate = r.rotate % 270f + 90f; } // 90F���� �ؿ��� ����
                                                          
                    r.rotate += 1f;// ȸ�� �� ����
                    float z = Mathf.Cos(r.rotate * Mathf.Deg2Rad) * radius * zThick;
                    float y = -Mathf.Sin(r.rotate * Mathf.Deg2Rad) * radius;
                    // �ڷ� ������ �������� 0���� �ٿ� �Ⱥ��̰� ����
                    r.tr.localScale = scale * Mathf.Abs(Mathf.Cos(r.rotate * Mathf.Deg2Rad));
                    r.tr.localPosition = new Vector3(0, y, z);// + 400f
                    yield return null;
                }
                
            }
            
        }

        yield return new WaitForSeconds(1f);
        // ȭ�� ��ȯ �ڷ�ƾ ���� (���� ȭ���� �˾����� �ڷ�ƾ)
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            sdi.transitionPanel.color
                = new Color(0,0,0,t);
            yield return null;
        }

        // ���� ��� ����Ͽ� ������� �°��
        // ��Ī�� ���� ��Ȳ
        // ���� ����

        // �� ��ȯ
        //SceneManager.LoadScene("InGame", LoadSceneMode.Single);
        
        if (PhotonNetwork.IsMasterClient)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log("�����Ͱ� ��ȯ ����");
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.LoadLevel("InGame");
        }
    }
}
