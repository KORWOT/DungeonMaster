using DungeonMaster.Battle;
using DungeonMaster.Character;
using DungeonMaster.Skill;
using DungeonMaster.Data;

namespace DungeonMaster.Battle.Damage
{
    /// <summary>
    /// 데미지 계산 과정의 개별 단계를 나타내는 인터페이스입니다.
    /// </summary>
    public interface IDamageCalculationStep
    {
        /// <summary>
        /// 데미지 계산 단계를 실행합니다.
        /// </summary>
        /// <param name="context">현재 데미지 계산의 모든 정보를 담고 있는 컨텍스트 객체</param>
        void Calculate(DamageCalculationContext context);
    }

    /// <summary>
    /// 데미지 계산 파이프라인을 통해 전달되는 데이터 컨텍스트입니다.
    /// </summary>
    public class DamageCalculationContext
    {
        public DeterministicCharacterData Attacker { get; }
        public DeterministicCharacterData Defender { get; }
        public SkillData Skill { get; }
        
        /// <summary>
        /// 파이프라인을 거치며 계산되는 현재 데미지 값입니다.
        /// </summary>
        public long CurrentDamage { get; set; }
        
        /// <summary>
        /// 방어 관통이 적용된 후의 최종 방어력입니다.
        /// </summary>
        public long FinalDefense { get; set; }

        /// <summary>
        /// 이번 공격이 치명타로 판정되었는지 여부입니다.
        /// </summary>
        public bool IsCritical { get; set; }

        public float CriticalRate { get; set; }
        public long CriticalDamage { get; set; }

        public DamageCalculationContext(DeterministicCharacterData attacker, DeterministicCharacterData defender, SkillData skill)
        {
            Attacker = attacker;
            Defender = defender;
            Skill = skill;
            CurrentDamage = 0;
            FinalDefense = 0;
            IsCritical = false;
            CriticalRate = 0.0f;
            CriticalDamage = 0;
        }
    }
} 