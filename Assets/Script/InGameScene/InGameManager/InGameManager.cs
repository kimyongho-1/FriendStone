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
        Debug.Log($"리스트삽입, 원본idx:{idx},  변경idx:{newIdx}");
        // 기본 Insert 함수 실행후
        base.Insert(newIdx, ele);
        // 총 관리 목적의 LIST에도 추가
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
    // 게임내 체력 상호작용을 이루는 영웅들, 미니언들을 여기에서 쉽게 찾기위해 생성
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
        #region audio클립 모아두기 
        // 인게임씬에서 자주 사용할 클립들은, 매니저내부에서 관리하여 사용하기로 결정
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

    // 타 컴포넌트에서, 주로 사용할 클립소스를 사용할떄, 매니저에게서 가져와 사용하기
    public AudioClip GetClip(Define.IGMsound s) { return sceneAudio[s]; }

    // 영웅의 체력이 0 이하로 떨어질시 게임종료
    public IEnumerator EndingGame(bool playerWin, bool senderIsMine) // 내가 이겼는지, 내가 상대방에게 결과를 전파해야하는지
    {
        if (!GAME.IGM.Packet.isMyTurn && senderIsMine == true) { yield break; }

        // 내가 먼저 게임의 결과를 본 사람이라면, 상대방에게 동일한 결과를 전파
        if (senderIsMine)
        {
            // 상대방에게 결과를 전송하기
            Packet.SendGameEnd(!playerWin);
        }
        Debug.Log($"승패결과 : {playerWin}");
        Turn.StopAllCoroutines();
        // 재생할 승패 클립 준비
        GRB.ReloadClip(playerWin);
        GRB.resultTmp.text = (playerWin) ? "당신의 승리" : "당신의 패배";
        GRB.resultSr.sprite = Hero.Player.playerImg.sprite;

        // 모든 오디오 소스를 찾아 모두 재생 중지
        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        for (int i = 0; i < sources.Length; i++)
        { sources[i].Stop(); }

        yield return new WaitForSeconds(1f);

        // 아웃트로 후처리 애니메이션 준비 및 재생
        Post.ReadyOutro();
        // 게임 끝낼것이기에, 예약된 큐의 코루틴들 모두 비우기
        Battle.ActionQueue.Clear();
        Battle.DeathRattleQueue.Clear();
        Post.gameObject.SetActive(true);
        StartCoroutine(Post.Outro());
        yield return new WaitForSeconds(0.5f);
        // 게임결과 보드판 켜주기
        GRB.gameObject.SetActive(true);
        GRB.Play();


        PhotonNetwork.LeaveRoom();
    }

    // 현재 나의턴이며 턴종료 누르지 않았다면, 플레이가 가능한 상태임을 반환
    public bool IsPlayable { get { return (Packet.isMyTurn && Turn.Col.enabled) ; } }

    #region 참조
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

    // BattleManager의 액션큐 빠르게 접근용도
    public void AddAction(IEnumerator co) 
    {
        if (co != null) { Battle.ActionQueue.Enqueue(co); }
    }
    public void AddDeathAction(IEnumerator co) { if (co != null) { Battle.PlayDeathRattle(co); } }


    #region 팝업창 알림 이벤트

    // 영웅 능력 팝업창 호출
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
    // 필드의 미니언 커서 포인터엔터 이벤트 (상대와 나의것 구분하여 위치변경)
    public void ShowMinionPopup(MinionCardData data, Vector3 pos, Sprite sprite)
    {
        Stat.gameObject.SetActive(true);
        cardPopup.transform.position= pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        // 만약 글자가 25글자 이상이면 폰트크기를 약간 줄이기
        Debug.Log("글자 길이 : "+data.cardDescription.Length);
        Description.fontSize = (data.cardDescription.Length > 39) ? 15f : 18f;
        Stat.text = $"<color=green>ATT {data.att} <color=red>HP {data.hp} <color=black>몬스터";
        Type.text = data.cardType.ToString();
        Cost.text = data.cost.ToString();
        cardImage.sprite = sprite;
        cardPopup.gameObject.SetActive(true);
    }
    // 무기카드 팝업이벤트
    public void ShowCardPopup(ref WeaponCardData data, Vector3 pos)
    {
        Stat.gameObject.SetActive(true);
        cardPopup.transform.position = pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        // 만약 글자가 25글자 이상이면 폰트크기를 약간 줄이기
        Debug.Log("글자 길이 : " + data.cardDescription.Length);
        Description.fontSize = (data.cardDescription.Length > 39) ? 15f : 18f;
        Type.text = data.cardType.ToString();
        Cost.text = data.cost.ToString();
        cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);
        cardPopup.gameObject.SetActive(true);
    }

    #region 카드사용시, 강조 팝업
    public void ShowEnemySpellPopup(SpellCardData data, Vector3 pos)
    {
        StopAllCoroutines();
        // 팝업창 효과음 재생
        audioPlayer.clip = sceneAudio[Define.IGMsound.Popup];
        audioPlayer.Play();
        // 현재 스폰중인카드가 있다고 설정해, 다른 카드 엔터 이벤트 방지
        cardPopup.isEnmeySpawning = true;

        cardPopup.transform.position = pos;
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = $"<color=black>주문";
        Type.text = data.cardType.ToString();
        Cost.text = data.cost.ToString();
        cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum );
        StartCoroutine(FadeIn());
        IEnumerator FadeIn()
        {
            float t = 0;
            // 투명화 위해 모든 TMP와 SR을 묶기
            List<TextMeshPro> tmpList = new List<TextMeshPro>() { cardName, Description, Cost, Stat, Type };
            List<SpriteRenderer> imageList = new List<SpriteRenderer>() { cardImage, cardBackground };

            cardPopup.gameObject.SetActive(true);

            tmpList.ForEach(x => x.alpha = 0);
            imageList.ForEach(x => x.color = new Color(1, 1, 1, 0));
            Color tempColor = Color.white;
            while (t < 1f)
            {
                // 알파값 점차 1으로 변환
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
    public void ShowEnemyMinionPopup(MinionCardData data, int att, int hp, int cost)
    {
        StopAllCoroutines();
        // 팝업창 효과음 재생
        audioPlayer.clip = sceneAudio[Define.IGMsound.Popup];
        audioPlayer.Play();

        // 현재 스폰중인카드가 있다고 설정해, 다른 카드 엔터 이벤트 방지
        cardPopup.isEnmeySpawning = true;

        cardPopup.transform.position = new Vector3(3.5f, 2.8f, -0.5f);
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        Stat.text = $"<color=green>ATT {att} <color=red>HP {hp} <color=black>몬스터";
        Type.text = data.cardType.ToString();
        Cost.text = cost.ToString();
        cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);
        StartCoroutine(FadeIn());
        IEnumerator FadeIn()
        {
            float t = 0;
            // 투명화 위해 모든 TMP와 SR을 묶기
            List<TextMeshPro> tmpList = new List<TextMeshPro>() { cardName, Description, Cost, Stat, Type };
            List<SpriteRenderer> imageList = new List<SpriteRenderer>() { cardImage, cardBackground };

            cardPopup.gameObject.SetActive(true);

            tmpList.ForEach(x => x.alpha = 0);
            imageList.ForEach(x => x.color = new Color(1, 1, 1, 0));
            Color tempColor = Color.white;
            while (t < 1f)
            {
                // 알파값 점차 1으로 변환
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
        StopAllCoroutines();
        // 팝업창 효과음 재생
        audioPlayer.clip = sceneAudio[Define.IGMsound.Popup];
        audioPlayer.Play();
        // 현재 스폰중인카드가 있다고 설정해, 다른 카드 엔터 이벤트 방지
        cardPopup.isEnmeySpawning = true;
        cardPopup.transform.position = new Vector3(3.5f, 2.8f, -0.5f);
        cardName.text = data.cardName;
        Description.text = data.cardDescription;
        // 만약 글자가 25글자 이상이면 폰트크기를 약간 줄이기
        Debug.Log("글자 길이 : " + data.cardDescription.Length);
        Description.fontSize = (data.cardDescription.Length > 39) ? 15f : 18f;
        Stat.text = $"<color=green>ATT {data.att} <color=red>HP {data.durability} <color=black>무기";
        Type.text = data.cardType.ToString();
        Cost.text = data.cost.ToString();
        cardImage.sprite = GAME.Manager.RM.GetImage(data.cardClass, data.cardIdNum);
        StartCoroutine(FadeIn());
        IEnumerator FadeIn()
        {
            float t = 0;
            // 투명화 위해 모든 TMP와 SR을 묶기
            List<TextMeshPro> tmpList = new List<TextMeshPro>() { cardName, Description, Cost, Stat, Type };
            List<SpriteRenderer> imageList = new List<SpriteRenderer>() { cardImage, cardBackground };

            tmpList.ForEach(x => x.alpha = 0);
            imageList.ForEach(x => x.color = new Color(1, 1, 1, 0));
            cardPopup.gameObject.SetActive(true);

            Color tempColor = Color.white;
            while (t < 1f)
            {
                // 알파값 점차 1으로 변환
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
    #endregion


    #endregion
}
