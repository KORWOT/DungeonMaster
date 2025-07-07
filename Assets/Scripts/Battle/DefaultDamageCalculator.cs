using System;
using DungeonMaster.Character;
using DungeonMaster.Skill;
using System.Collections.Generic;
using DungeonMaster.Battle.Damage;
using DungeonMaster.Data;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// 데미지 계산 파이프라인을 사용하여 데미지를 계산하는 클래스입니다.
    /// </summary>
    public class DefaultDamageCalculator : IDamageCalculator
    {
        private readonly List<IDamageCalculationStep> _pipeline;

        /// <summary>
        /// 데미지 계산 단계들의 리스트를 주입받아 파이프라인을 구성합니다.
        /// </summary>
        /// <param name="pipeline">실행할 데미지 계산 단계의 목록</param>
        public DefaultDamageCalculator(List<IDamageCalculationStep> pipeline)
        {
            _pipeline = pipeline ?? new List<IDamageCalculationStep>();
        }

        public long Calculate(DeterministicCharacterData attacker, DeterministicCharacterData defender, SkillData skill)
        {
            if (skill == null)
            {
                // 스킬이 없는 경우(기본 공격 등) 최소 데미지 또는 기본값 반환
                return 1;
            }

            var context = new DamageCalculationContext(attacker, defender, skill);

            foreach (var step in _pipeline)
            {
                step.Calculate(context);
            }

            return Math.Max(1, context.CurrentDamage);
        }
    }
} 