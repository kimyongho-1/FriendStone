using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public partial class PacketManager
{
    // 41 ~ 60
    const byte UseWeapon = 41;
    const byte EnemyHeroAttack = 42;
    const byte EnemyHeroUseSkill = 43;
    const byte EnemyEmotion = 44;
    public void InitHeroDictionary()
    {
        dic.Add(UseWeapon, ReceivedWeapon);
        dic.Add(EnemyHeroAttack, ReceivedHeroAttack);
        dic.Add(EnemyHeroUseSkill , ReceivedHeroSkill );
        dic.Add(EnemyEmotion , ReceivedEnemyEmotion);
    }
    #region 영웅 무기 착용
    // 영웅 무기 착용 이벤트 전파
    public void SendWeapon(int punID, int cardID, float x, float y, float z)
    {
        object[] data = new object[] { punID, cardID , x,y,z};
        PhotonNetwork.RaiseEvent(UseWeapon,data, Other, SendOptions.SendReliable);
    }

    // 적 영웅이 무기 카드 사용시, 해당 이벤트 전달받아 동기화 실행
    public void ReceivedWeapon(object[] data)
    { 
        // 카드객체넘버 + 카드데이터넘버 + 마지막 드래그 끝낸 위치
        int punID = (int)data[0];
        int cardID = (int)data[1];
        Debug.Log($"pun:{punID}, cardID:{cardID}");
        Vector3 dest = new Vector3((float)data[2], (float)data[3], (float)data[4]);

        GAME.IGM.AddAction(ReceivedWeaponCo(punID, cardID, dest));
        IEnumerator ReceivedWeaponCo(int punID, int cardID, Vector3 dest)
        {
            // 핸드카드 객체 찾기
            CardHand ch = GAME.IGM.Hand.EnemyHand.Find(punID);
            Debug.Log(punID+""+ch);
            // 적 핸드에서 사용했기에 리스트에서 제거
            GAME.IGM.Hand.EnemyHand.Remove(ch);

            // 리소스 매니저의 경로를 반환 받는 딕셔너리 통해 카드타입과 카드데이터 찾기
            Define.cardType type = GAME.Manager.RM.PathFinder.Dic[cardID].type;
            string jsonFile = GAME.Manager.RM.PathFinder.Dic[cardID].GetJson();

            // 카드 데이터 생성
            CardData card = JsonConvert.DeserializeObject<WeaponCardData>
                (jsonFile, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

            ch.Init(card, false);

            // 무기 카드를 내는 애니메이션과 착용 애니메이션 예약실행
            //GAME.IGM.AddAction(weaponCardMove(ch, card));
            yield return StartCoroutine(weaponCardMove(ch,card));
            IEnumerator weaponCardMove(CardHand ch, CardData card)
            {
                // 상대의 무기카드 사용 + 마나감소
                GAME.IGM.Hero.Enemy.MP -= card.cost;

                float t = 0;
                Vector3 start = ch.transform.position;
                while (t < 1f)
                {
                    t += Time.deltaTime * 2f;
                    ch.transform.position =
                        Vector3.Lerp(start, dest, t);
                    yield return null;
                }
                // 그후 무기 착용 이벤트 실행
                yield return GAME.IGM.StartCoroutine(GAME.IGM.Hero.Enemy.EquipWeapon(ch, card));
            }
        }
      

        
    }
    #endregion

    #region 적 영웅 공격 이벤트

    // 내 영웅이 공격하는 이벤트를 전파
    public void SendHeroAttack(int attackerID, int targetID)
    {
        object[] data = new object[] { attackerID, targetID };
        PhotonNetwork.RaiseEvent(EnemyHeroAttack, data, Other, SendOptions.SendReliable);
    }
    // 적영웅 공격 이벤트 받아서 실행
    public void ReceivedHeroAttack(object[] data)
    {
        // 공격자와 타겟의 식별 변수 확인
        int attackerID = (int)data[0];
        int targetID = (int)data[1];

        // 적 영웅 공격 예약
        GAME.IGM.AddAction(readyAndAttack(attackerID, targetID));//GAME.IGM.AddAction(enemyHero.AttackCo(enemyHero, target));

        IEnumerator readyAndAttack(int attackerID, int targetID)
        {
            // 공격자와 타겟 찾기
            // 공격자가 영웅인것을 알고 있는 상태
            Hero enemyHero = GAME.IGM.allIBody.Find(x => x.PunId == attackerID).TR.GetComponentInParent<Hero>();
            IBody target = GAME.IGM.allIBody.Find(x => x.PunId == targetID);
            yield return GAME.IGM.StartCoroutine(enemyHero.AttackCo(enemyHero, target));
        }
    }
    #endregion

    #region 적 영웅의 스킬이벤트
    public void SendHeroSkillEvt(int targetPunId)
    {
        PhotonNetwork.RaiseEvent(EnemyHeroUseSkill,new object[] { targetPunId }, Other, SendOptions.SendReliable);
    }
    public void ReceivedHeroSkill(object[] data)
    {
        int targetPun = (int)data[0];
        GAME.IGM.AddAction(ReceivedSkillCo(targetPun));
        IEnumerator ReceivedSkillCo(int targetPun)
        {
            IBody target = GAME.IGM.allIBody.Find(x => x.PunId == targetPun);
            yield return GAME.IGM.StartCoroutine(GAME.IGM.Hero.Enemy.CallHeroSkillAttack(target));
        }
        
    }
    #endregion

    #region 영웅 감정표현 전달 및 받기
    public void SendHeroEmotion(int idx)
    {
        PhotonNetwork.RaiseEvent(EnemyEmotion, new object[] { idx }, Other, SendOptions.SendReliable);
    }
    public void ReceivedEnemyEmotion(object[] data)
    {
        int emoIdx = (int)data[0];

        GAME.IGM.Hero.Enemy.PlayEnemyEmotion(emoIdx);
    }
    #endregion
}