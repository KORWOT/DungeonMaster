using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Data;
using DungeonMaster.Character;
using DungeonMaster.Skill;
using DungeonMaster.Battle;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 실제 전투 인스턴스의 모든 상태를 담는 결정론적 데이터 컨테이너입니다.
    /// 이 데이터 객체는 순수 C# 코드로만 이루어져 Unity 종속성이 없으며, 서버에서도 동일하게 사용될 수 있습니다.
    /// 모든 계산 로직은 이 데이터를 입력받아 새로운 데이터를 반환하는 순수 함수 형태로 작성되어야 합니다.
    /// </summary>
    public class DeterministicCharacterData
    {
        // --- 고유 식별자 ---
        public readonly long InstanceId; // 전투 중 사용되는 고유 ID
        public readonly long BlueprintId; // 원본 데이터(Blueprint)의 ID

        // --- 기본 정보 (Blueprint로부터 복사) ---
        public readonly string Name;
        public readonly bool IsPlayerCharacter;
        public readonly ElementType Element;
        public readonly DefenseType DefenseType;
        public readonly Grade Grade;
        public readonly List<SkillData> Skills;
        
        /// <summary>
        /// 이 캐릭터에 적용되는 모든 데미지 수정자(장비, 버프 등) 목록
        /// </summary>
        private List<IDamageModifier> _damageModifiers = new List<IDamageModifier>();
        public IReadOnlyList<IDamageModifier> DamageModifiers => _damageModifiers;

        // --- 변경 가능한 상태 ---
        public int Level;
        public long CurrentHP;

        /// <summary>
        /// 캐릭터의 모든 스탯 정보. (Key: StatType, Value: 스탯 값)
        /// </summary>
        public Dictionary<StatType, long> Stats;

        /// <summary>
        /// 현재 적용중인 스킬들의 남은 쿨타임 (밀리초).
        /// Key: 스킬 ID, Value: 남은 쿨타임
        /// </summary>
        public Dictionary<long, long> SkillCooldowns { get; set; }

        /// <summary>
        /// 캐릭터가 보유한 스킬과 그 레벨 정보.
        /// Key: 스킬 ID, Value: 스킬 레벨
        /// </summary>
        public Dictionary<long, int> SkillLevels { get; }

        /// <summary>
        /// 다음 기본 공격까지 남은 쿨타임 (밀리초).
        /// </summary>
        public long AttackCooldownRemainingMs;
        
        /// <summary>
        /// 현재 적용중인 버프 목록
        /// </summary>
        public List<BuffData> ActiveBuffs { get; set; } = new List<BuffData>();

        // 원소 스탯
        public Dictionary<ElementType, long> Elementals { get; private set; } = new Dictionary<ElementType, long>();

        public DeterministicCharacterData(
            long instanceId, long blueprintId, string name, int level, bool isPlayer, 
            ElementType element, DefenseType defenseType, Grade grade, Dictionary<long, int> skillLevels)
        {
            InstanceId = instanceId;
            BlueprintId = blueprintId;
            Name = name;
            Level = level;
            IsPlayerCharacter = isPlayer;
            Element = element;
            DefenseType = defenseType;
            Grade = grade;
            
            Stats = new Dictionary<StatType, long>();
            SkillCooldowns = new Dictionary<long, long>();
            SkillLevels = new Dictionary<long, int>(skillLevels ?? new Dictionary<long, int>());
            ActiveBuffs = new List<BuffData>();
            Skills = new List<SkillData>();
            _damageModifiers = new List<IDamageModifier>();
        }

        /// <summary>
        /// 다른 DeterministicCharacterData 객체를 복사하는 복사 생성자입니다.
        /// </summary>
        public DeterministicCharacterData(DeterministicCharacterData source)
        {
            // Readonly fields
            InstanceId = source.InstanceId;
            BlueprintId = source.BlueprintId;
            Name = source.Name;
            IsPlayerCharacter = source.IsPlayerCharacter;
            Element = source.Element;
            DefenseType = source.DefenseType;
            Grade = source.Grade;
            Skills = new List<SkillData>(source.Skills); // 리스트 자체도 복사

            // DamageModifiers는 깊은 복사가 필요할 수 있으나, UniqueEffect는 ScriptableObject일 가능성이 높으므로 우선 얕은 복사
            _damageModifiers = new List<IDamageModifier>(source._damageModifiers);

            // Mutable fields
            Level = source.Level;
            CurrentHP = source.CurrentHP;
            Stats = new Dictionary<StatType, long>(source.Stats); // 딕셔너리 복사
            SkillCooldowns = new Dictionary<long, long>(source.SkillCooldowns); // 딕셔너리 복사
            SkillLevels = new Dictionary<long, int>(source.SkillLevels); // 스킬 레벨 복사
            AttackCooldownRemainingMs = source.AttackCooldownRemainingMs;
            
            // Buffs need deep copy
            ActiveBuffs = new List<BuffData>();
            foreach (var buff in source.ActiveBuffs)
            {
                ActiveBuffs.Add(buff.Clone());
            }
        }

        public void FinalizeStatCalculation()
        {
            // 현재 HP를 MaxHP로 초기화 (전투 시작 시 풀피)
            CurrentHP = Stats.ContainsKey(StatType.MaxHP) ? Stats[StatType.MaxHP] : 1;
        }

        public void AddDamageModifier(IDamageModifier modifier)
        {
            if (modifier != null)
            {
                _damageModifiers.Add(modifier);
            }
        }

        /// <summary>
        /// 스킬 레벨을 반환합니다. 스킬을 배우지 않았으면 0을 반환합니다.
        /// </summary>
        public int GetSkillLevel(long skillId)
        {
            if (SkillLevels.TryGetValue(skillId, out var level))
            {
                return level;
            }
            return 0;
        }

        // Clone() 메서드는 생성자 변경으로 인해 유효하지 않으므로, 더 나은 상태 관리 방식을 위해 임시로 제거합니다.
        // public DeterministicCharacterData Clone()
        // {
        //     ...
        // }
    }
} 