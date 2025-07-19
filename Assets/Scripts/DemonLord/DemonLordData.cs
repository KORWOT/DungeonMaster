using System;
using System.Collections.Generic;
using DungeonMaster.Character;
using DungeonMaster.Data;
using System.Linq; // Added for Select

namespace DungeonMaster.DemonLord
{
    /// <summary>
    /// 결정론적 전투 상태에 포함되는 마왕의 데이터입니다.
    /// </summary>
    [Serializable]
    public class DemonLordData : ICombatantData
    {
        public long InstanceId { get; }
        public bool IsPlayer { get; }
        public string Name { get; }
        public int Level { get; }
        public float CurrentHP { get; set; } // HP는 변경 가능해야 하므로 set 추가
        
        public IReadOnlyDictionary<StatType, long> Stats { get; }
        public IReadOnlyList<long> BaseSkillIds { get; }

        // ICombatantData 인터페이스 구현을 위한 속성들
        public List<BuffData> ActiveBuffs { get; set; }
        public long AttackCooldownRemainingMs { get; set; }
        public Dictionary<long, long> SkillCooldowns { get; set; }

        public DemonLordData(long instanceId, bool isPlayer, string name, int level, float currentHp, 
            IReadOnlyDictionary<StatType, long> stats, IReadOnlyList<long> baseSkillIds)
        {
            InstanceId = instanceId;
            IsPlayer = isPlayer;
            Name = name;
            Level = level;
            CurrentHP = currentHp;
            Stats = stats;
            BaseSkillIds = baseSkillIds;
            ActiveBuffs = new List<BuffData>();
            SkillCooldowns = new Dictionary<long, long>();
        }
        
        // 전투 상태 업데이트를 위한 'With' 메서드들
        public DemonLordData With(float? newCurrentHp = null)
        {
            var clone = new DemonLordData(
                this.InstanceId,
                this.IsPlayer,
                this.Name,
                this.Level,
                newCurrentHp ?? this.CurrentHP,
                this.Stats,
                this.BaseSkillIds
            );
            
            // 컬렉션들은 깊은 복사를 수행해야 함
            clone.ActiveBuffs = new List<BuffData>(this.ActiveBuffs.Select(b => b.Clone()));
            clone.AttackCooldownRemainingMs = this.AttackCooldownRemainingMs;
            clone.SkillCooldowns = new Dictionary<long, long>(this.SkillCooldowns);

            return clone;
        }
    }
} 