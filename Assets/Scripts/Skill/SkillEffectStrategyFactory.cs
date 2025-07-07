using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DungeonMaster.Skill
{
    public class SkillEffectStrategyFactory
    {
        private readonly Dictionary<SkillEffectType, ISkillEffectStrategy> _strategies = new Dictionary<SkillEffectType, ISkillEffectStrategy>();
        private readonly EmptySkillEffectStrategy _emptyStrategy = new EmptySkillEffectStrategy();
        private static SkillEffectStrategyFactory _instance;

        public static SkillEffectStrategyFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SkillEffectStrategyFactory();
                }
                return _instance;
            }
        }

        private SkillEffectStrategyFactory()
        {
            RegisterStrategies();
        }

        private void RegisterStrategies()
        {
            _strategies.Add(SkillEffectType.Damage, new DamageStrategy());
            _strategies.Add(SkillEffectType.Heal, new HealStrategy());
            _strategies.Add(SkillEffectType.Buff, new BuffStrategy()); // 버프 ID 하드코딩 제거
        }

        public ISkillEffectStrategy GetStrategy(SkillEffectType type)
        {
            if (_strategies.TryGetValue(type, out var strategy))
            {
                return strategy;
            }
            
            // 요청된 타입의 전략이 없을 경우, 경고를 로그하고 비어있는 전략(Null Object)을 반환합니다.
            // 이렇게 하면 NullReferenceException을 방지하여 시스템 안정성을 높일 수 있습니다.
            Debug.WriteLine($"[Warning] No strategy registered for SkillEffectType '{type}'. Returning EmptySkillEffectStrategy.");
            return _emptyStrategy;
        }
    }
}