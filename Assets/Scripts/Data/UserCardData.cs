using System;
using System.Collections.Generic;
using UnityEngine;
using DungeonMaster.Character;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 유저가 소유하고 성장시키는 개별 몬스터 카드에 대한 데이터입니다.
    /// 이 데이터는 유저의 인벤토리에 저장되며, JSON 등으로 직렬화되어 관리됩니다.
    /// </summary>
    [Serializable]
    public class UserCardData
    {
        public long Id; // 유저가 보유한 카드의 고유 인스턴스 ID 
        public long BlueprintId; // 어떤 종류의 카드인지 식별하는 ID (CardBlueprintData.BlueprintId와 연결)
        
        public int Level;
        public long Experience;

        /// <summary>
        /// 레벨업, 장비 장착 등의 모든 효과가 적용된 최종 스탯.
        /// 이 스탯은 CharacterGrowthService에 의해 계산되고 갱신됩니다.
        /// </summary>
        public Dictionary<StatType, long> CurrentStats = new Dictionary<StatType, long>();

        /// <summary>
        /// 카드가 생성될 때 결정되는 스탯별 '고유 성장률' (단위: %).
        /// 예: 121 -> 121%
        /// 이 값은 레벨업 시 성장량을 계산하는 데 사용되며, 한 번 정해지면 변하지 않습니다.
        /// </summary>
        public Dictionary<StatType, int> InnateGrowthRates_x100 = new Dictionary<StatType, int>();

        // 보유 스킬 및 레벨 (Key: 스킬 ID)
        public Dictionary<long, int> SkillLevels = new Dictionary<long, int>();

        // 장착한 장비의 인스턴스 ID 목록
        public List<long> EquippedItemIds = new List<long>();

        public UserCardData()
        {
            // 기본 생성자는 JSON 역직렬화를 위해 필요합니다.
            Id = System.Guid.NewGuid().GetHashCode();
            InnateGrowthRates_x100 = new Dictionary<StatType, int>();
            SkillLevels = new Dictionary<long, int>();
            EquippedItemIds = new List<long>();
        }

        /// <summary>
        /// 특정 설계도로부터 새로운 유저 카드를 생성합니다.
        /// </summary>
        public UserCardData(long blueprintId)
        {
            Id = System.Guid.NewGuid().GetHashCode();
            BlueprintId = blueprintId;
            Level = 1;
            Experience = 0;
            CurrentStats = new Dictionary<StatType, long>();
            InnateGrowthRates_x100 = new Dictionary<StatType, int>();
            SkillLevels = new Dictionary<long, int>();
            EquippedItemIds = new List<long>();
        }

        public int AttackGrowthRate_x100
        {
            get => InnateGrowthRates_x100.GetValueOrDefault(StatType.Attack, 100);
            set => InnateGrowthRates_x100[StatType.Attack] = value;
        }

        public int DefenseGrowthRate_x100
        {
            get => InnateGrowthRates_x100.GetValueOrDefault(StatType.Defense, 100);
            set => InnateGrowthRates_x100[StatType.Defense] = value;
        }

        public int HpGrowthRate_x100
        {
            get => InnateGrowthRates_x100.GetValueOrDefault(StatType.MaxHP, 100);
            set => InnateGrowthRates_x100[StatType.MaxHP] = value;
        }
    }
} 