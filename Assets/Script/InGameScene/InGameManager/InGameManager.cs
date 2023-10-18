using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Define;
public class CustomList : List<CardField>
{
    public new void Insert(int idx ,CardField ele)
    {
        int newIdx = Mathf.Clamp(idx, 0, this.Count);
        Debug.Log($"����Ʈ����, ����idx:{idx},  ����idx:{newIdx}");
        // �⺻ Insert �Լ� ������
        base.Insert(newIdx, ele);
        // �� ���� ������ LIST���� �߰�
        GAME.IGM.allIBody.Add(ele);
    }

    public new void Remove(CardField ele) 
    {
        base.Remove(ele);
        GAME.IGM.allIBody.Remove(ele);
    }
}

public class InGameManager : MonoBehaviour
{
    // ���ӳ� ü�� ��ȣ�ۿ��� �̷�� ������, �̴Ͼ���� ���⿡�� ���� ã������ ����
    public List<IBody> allIBody = new List<IBody>();

    public HourGlass TimeLimiter;
    public int GameTurn = 0;
    public CardPopupEvtHolder cardPopup;
    public TextMeshPro cardName, Description, Stat, Type, Cost;
    public SpriteRenderer cardImage,cardBackground;
    AudioSource audioPlayer;
    public Dictionary<Define.IGMsound, AudioClip> sceneAudio = new Dictionary<IGMsound, AudioClip>();
    public void Awake()
    {
        GAME.IGM = this;
        #region audioŬ�� ��Ƶα� 
        // �ΰ��Ӿ����� ���� ����� Ŭ������, �Ŵ������ο��� �����Ͽ� ����ϱ�� ����
        audioPlayer = GetComponent<AudioSource>();
        sceneAudio.Add(Define.IGMsound.Draw, Resources.Load<AudioClip>("Sound/InGame/Managers/Draw"));
        sceneAudio.Add(Define.IGMsound.Pick, Resources.Load<AudioClip>("Sound/InGame/Managers/Pick"));
        sceneAudio.Add(Define.IGMsound.Popup, Resources.Load<AudioClip>("Sound/InGame/Managers/Popup"));
        sceneAudio.Add(Define.IGMsound.Punch, Resources.Load<AudioClip>("Sound/InGame/Managers/Punch"));
        sceneAudio.Add(Define.IGMsound.Summon, Resources.Load<AudioClip>("Sound/InGame/Managers/Summon"));
        sceneAudio.Add(Define.IGMsound.Click, Resources.Load<AudioClip>("Sound/InGame/Managers/Click"));
        sceneAudio.Add(Define.IGMsound.Cancel, Resources.Load<AudioClip>("Sound/InGame/Managers/Cancel"));
        sceneAudio.Add(Define.IGMsound.ClickTurnBtn, Resources.Load<AudioClip>("Sound/InGame/Managers/ClickTurnBtn"));
        sceneAudio.Add(Define.IGMsound.TurnStart, Resources.Load<AudioClip>("Sound/InGame/Managers/TurnStart"));
        #endregion

    }

    // Ÿ ������Ʈ����, �ַ� ����� Ŭ���ҽ��� ����ҋ�, �Ŵ������Լ� ������ ����ϱ�
    public AudioClip GetClip(Define.IGMsound s) { return sceneAudio[s]; }

    public IEnumerator EndingGame(bool playerWin, bool senderIsMine) // ���� �̰����, ���� ���濡�� ����� �����ؾ��ϴ���
    {
        // ���� ���� ������ ����� �� ����̶��, ���濡�� ������ ����� ����
        if (senderIsMine)
        {
            // ���濡�� ����� �����ϱ�
            Packet.SendGameEnd(playerWin);
        }
        // ���濡�� �̺�Ʈ�� ���� �޾� �����ϴ°Ŷ�� ����(���� �޾� ���������״�)
        else
        { yield break; }

        Turn.StopAllCoroutines();
        // ����� ���� Ŭ�� �غ�
        GRB.ReloadClip(playerWin);
        GRB.resultTmp.text = (playerWin) ? "����� �¸�" : "����� �й�";
        GRB.resultSr.sprite = Hero.Player.playerImg.sprite;

        // ��� ����� �ҽ��� ã�� ��� ��� ����
        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        for (int i = 0; i < sources.Length; i++)
        { sources[i].Stop(); }

        yield return new WaitForSeconds(1f);

        // �ƿ�Ʈ�� ��ó�� �ִϸ��̼� �غ� �� ���
        Post.ReadyOutro();
        // ���� �������̱⿡, ����� ť�� �ڷ�ƾ�� ��� ����
        Battle.ActionQueue.Clear();
        Battle.DeathRattleQueue.Clear();
        Post.gameObject.SetActive(true);
        StartCoroutine(Post.Outro());
        yield return new WaitForSeconds(0.5f);
        // ���Ӱ�� ������ ���ֱ�
        GRB.gameObject.SetActive(true);
        GRB.Play();


        PhotonNetwork.Disconnect();
        Destroy(GAME.Manager.GetComponent<PhotonView>());
        GAME.Manager.AddComponent<PhotonView>();
    }

    #region ����
    public GameResultBoard GRB { get; set; }
    public FindEvtHolder FindEvt { get; set; }
    public HeroManager Hero { get; set; }
    public HandManager Hand { get; set; }
    public SpawnManager Spawn { get; set; }
    public TargetingCamera TC { get; set; }
    public BattleManager Battle { get; set; }
    public TurnEndBtn Turn { get; set; }
    public PacketManager Packet { get; set; }
    public PostCamera Post { get; set; }
 
    #endregion

    // BattleManager�� �׼�ť ������ ���ٿ뵵
    public void AddAction(IEnumerator co) { if (co != null) { Battle.ActionQueue.Enqueue(co); } }
    public void AddDeathAction(IEnumerator co) { if (co != null) { Battle.PlayDeathRattle(co); } }


    #region �˾�â �˸� �̺�Ʈ

    // ���� �ɷ� �˾�â ȣ��
    public void ShowHeroSkill(Vector3 pos, HeroData skill)
    {
        cardPopup.transform.position = pos;
        cardName.text = skill.skillName;
        Description.text = skill.skillDesc;
        Stat.gameObject.SetActive(false);
        Type.text = "skill";
        Cost.text = skill.skillCost.ToString();
        cardImage.sprite = (skill.IsMine) ? GAME.IGM.Hero.Player.skillImg.sprite : GAME.IGM.Hero.Enemy.skillImg.sprite;
        cardPopup.gameObject.SetActive(true);
    }

    // ī���˾� ȣ��
    public void ShowMinionPopup(MinionCardData data, Vector3 pos, Sprite sprite)
    {
        Stat.gameObject.SetActive(true);
        cardPopup.transform.position= pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        // ���� ���ڰ� 25���� �̻��̸� ��Ʈũ�⸦ �ణ ���̱�
        Debug.Log("���� ���� : "+data.cardDescription.Length);
        Description.fontSize = (data.cardDescription.Length > 39) ? 15f : 18f;
        Stat.text = $"<color=green>ATT {data.att} <color=red>HP {data.hp} <color=black>����";
        Type.text = data.cardType.ToString();
        Cost.text = data.cost.ToString();
        cardImage.sprite = sprite;
        cardPopup.gameObject.SetActive(true);
    }
   
    public void ShowEnemySpellPopup(SpellCardData data, Vector3 pos)
    {
        // �˾�â ȿ���� ���
        audioPlayer.clip = sceneAudio[Define.IGMsound.Popup];
        audioPlayer.Play();
        // ���� ��������ī�尡 �ִٰ� ������, �ٸ� ī�� ���� �̺�Ʈ ����
        cardPopup.isEnmeySpawning = true;

        cardPopup.transform.position = pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = $"<color=black>�ֹ�";
        Type.text = data.cardType.ToString();
        Cost.text = data.cost.ToString();
        cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum );
        StartCoroutine(FadeIn());
        IEnumerator FadeIn()
        {
            float t = 0;
            // ����ȭ ���� ��� TMP�� SR�� ����
            List<TextMeshPro> tmpList = new List<TextMeshPro>() { cardName, Description, Cost, Stat, Type };
            List<SpriteRenderer> imageList = new List<SpriteRenderer>() { cardImage, cardBackground };

            cardPopup.gameObject.SetActive(true);

            tmpList.ForEach(x => x.alpha = 0);
            imageList.ForEach(x => x.color = new Color(1, 1, 1, 0));
            Color tempColor = Color.white;
            while (t < 1f)
            {
                // ���İ� ���� 1���� ��ȯ
                t += Time.deltaTime;
                tempColor.a = t;
                tmpList.ForEach(x => x.alpha = t);
                imageList.ForEach(x => x.color = tempColor);
                yield return null;
            }
        }
    }
    public void ShowEnemyMinionPopup(MinionCardData data, int att, int hp, int cost)
    {
        // �˾�â ȿ���� ���
        audioPlayer.clip = sceneAudio[Define.IGMsound.Popup];
        audioPlayer.Play();

        // ���� ��������ī�尡 �ִٰ� ������, �ٸ� ī�� ���� �̺�Ʈ ����
        cardPopup.isEnmeySpawning = true;

        cardPopup.transform.position = new Vector3(3.5f, 2.8f, -0.5f);
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = $"<color=green>ATT {att} <color=red>HP {hp} <color=black>����";
        Type.text = data.cardType.ToString();
        Cost.text = cost.ToString();
        cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);
        StartCoroutine(FadeIn());
        IEnumerator FadeIn()
        {
            float t = 0;
            // ����ȭ ���� ��� TMP�� SR�� ����
            List<TextMeshPro> tmpList = new List<TextMeshPro>() { cardName, Description, Cost, Stat, Type };
            List<SpriteRenderer> imageList = new List<SpriteRenderer>() { cardImage, cardBackground };

            cardPopup.gameObject.SetActive(true);

            tmpList.ForEach(x => x.alpha = 0);
            imageList.ForEach(x => x.color = new Color(1, 1, 1, 0));
            Color tempColor = Color.white;
            while (t < 1f)
            {
                // ���İ� ���� 1���� ��ȯ
                t += Time.deltaTime;
                tempColor.a = t;
                tmpList.ForEach(x => x.alpha = t);
                imageList.ForEach(x => x.color = tempColor);
                yield return null;
            }

            yield return new WaitForSeconds(1.5f);

            cardPopup.isEnmeySpawning = false;
            cardPopup.gameObject.SetActive(false);
        }
    }
    public void ShowEnemyWeaponPopup(WeaponCardData data)
    {
        // �˾�â ȿ���� ���
        audioPlayer.clip = sceneAudio[Define.IGMsound.Popup];
        audioPlayer.Play();
        // ���� ��������ī�尡 �ִٰ� ������, �ٸ� ī�� ���� �̺�Ʈ ����
        cardPopup.isEnmeySpawning = true;
        cardPopup.transform.position = new Vector3(3.5f, 2.8f, -0.5f);
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        // ���� ���ڰ� 25���� �̻��̸� ��Ʈũ�⸦ �ణ ���̱�
        Debug.Log("���� ���� : " + data.cardDescription.Length);
        Description.fontSize = (data.cardDescription.Length > 39) ? 15f : 18f;
        Stat.text = $"<color=green>ATT {data.att} <color=red>HP {data.durability} <color=black>����";
        Type.text = data.cardType.ToString();
        Cost.text = data.cost.ToString();
        cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);
        cardPopup.gameObject.SetActive(true);
    }

    public void ShowCardPopup(ref WeaponCardData data, Vector3 pos)
    {
        Stat.gameObject.SetActive(true);
        cardPopup.transform.position = pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = "";
        Type.text = data.cardType.ToString();
        Cost.text = data.cost.ToString();
        cardImage.sprite = null;
        cardPopup.gameObject.SetActive(true);
    }

    #endregion
}
