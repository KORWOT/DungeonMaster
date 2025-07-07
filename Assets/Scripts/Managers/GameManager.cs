using DungeonMaster.Battle;
using DungeonMaster.Data;
using System.Collections.Generic;
using UnityEngine;
using System;
using DungeonMaster.Character;

namespace DungeonMaster.Managers
{
    /// <summary>
    /// 게임의 전체적인 흐름과 상태를 관리하는 최상위 관리자 클래스입니다.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("전투 관리자 참조")]
        [SerializeField] private BattleManager battleManager;
        
        [Header("임시 전투 시작 데이터")]
        [SerializeField] private List<TestParticipant> playerTeam;
        [SerializeField] private List<TestParticipant> enemyTeam;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [ContextMenu("Start Test Battle")]
        public void StartTestBattle()
        {
            if (battleManager == null)
            {
                Debug.LogError("BattleManager가 할당되지 않았습니다!");
                return;
            }

            var playerMonsters = new List<ParticipantData>();
            foreach (var p in playerTeam)
            {
                if (p.Blueprint != null)
                {
                    var userCard = p.ToUserCardData();
                    playerMonsters.Add(new ParticipantData(p.Blueprint, p.Level, userCard));
                }
            }

            var enemyMonsters = new List<ParticipantData>();
            foreach (var p in enemyTeam)
            {
                if (p.Blueprint != null)
                {
                    var userCard = p.ToUserCardData();
                    enemyMonsters.Add(new ParticipantData(p.Blueprint, p.Level, userCard));
                }
            }

            var dungeonData = new DungeonData(playerMonsters, enemyMonsters);
            battleManager.StartBattle(dungeonData);
        }
    }

    /// <summary>
    /// 인스펙터에서 테스트 데이터를 쉽게 설정하기 위한 보조 클래스
    /// </summary>
    [System.Serializable]
    public class TestParticipant
    {
        public CardBlueprintData Blueprint;
        [Range(1, 100)] public int Level = 1;

        [Header("개체별 성장률 (x100)")]
        [Tooltip("100 = 100%")]
        public long AttackGrowthRate_x100 = 100;
        public long DefenseGrowthRate_x100 = 100;
        public long HpGrowthRate_x100 = 100;
        
        /// <summary>
        /// 테스트용 참가자 데이터를 기반으로 UserCardData 객체를 생성합니다.
        /// </summary>
        public UserCardData ToUserCardData()
        {
            if (Blueprint == null) return null;

            var cardData = new UserCardData
            {
                Id = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0),
                BlueprintId = Blueprint.BlueprintId,
                Level = this.Level,
                Experience = 0,
                SkillLevels = new Dictionary<long, int>() // 스킬은 일단 비워둠
            };
            
            // Blueprint의 기본 스탯을 CurrentStats에 복사
            foreach (var stat in Blueprint.BaseStats)
            {
                cardData.CurrentStats[stat.StatType] = stat.Value;
            }
            
            // 고유 성장률 설정 (StatType.MaxHP 사용)
            cardData.InnateGrowthRates_x100[StatType.Attack] = (int)this.AttackGrowthRate_x100;
            cardData.InnateGrowthRates_x100[StatType.Defense] = (int)this.DefenseGrowthRate_x100;
            cardData.InnateGrowthRates_x100[StatType.MaxHP] = (int)this.HpGrowthRate_x100;

            return cardData;
        }
    }
}