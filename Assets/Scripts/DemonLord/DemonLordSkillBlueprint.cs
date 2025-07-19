using UnityEngine;
using System.Collections.Generic;
using DungeonMaster.Shared.Scaling;

namespace DungeonMaster.DemonLord
{
    public enum DemonLordSkillType
    {
        Active,
        Ultimate
    }

    [CreateAssetMenu(fileName = "NewDemonLordSkill", menuName = "DungeonMaster/DemonLord/Skill Blueprint")]
    public class DemonLordSkillBlueprint : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("스킬을 식별하는 고유 ID입니다.")]
        public string BlueprintId;
        [Tooltip("스킬 이름의 지역화 키입니다.")]
        public string NameKey;
        [TextArea]
        [Tooltip("스킬 설명의 지역화 키입니다.")]
        public string DescriptionKey;
        public Sprite Icon;

        [Header("게임플레이 정보")]
        public DemonLordSkillType SkillType;
        [Tooltip("레벨 1일 때의 스킬 재사용 대기시간 (초)")]
        public float BaseCooldown;
        [Tooltip("레벨 1일 때의 스킬 사용에 필요한 자원량")]
        public int BaseResourceCost;

        [Header("레벨 스케일링 설정")]
        public ScalingType CooldownScalingType;
        public ScalingConfig CooldownScalingConfig;
        public ScalingType ResourceCostScalingType;
        public ScalingConfig ResourceCostScalingConfig;
        
        [Header("효과 목록")]
        [Tooltip("이 스킬이 가진 효과 목록입니다.")]
        public List<ScriptableObject> Effects = new List<ScriptableObject>();

        private IScalingStrategy _cooldownStrategy;
        private IScalingStrategy _resourceCostStrategy;

        /// <summary>
        /// 지정된 레벨에 맞는 최종 쿨다운을 계산합니다.
        /// </summary>
        public float GetScaledCooldown(int level, int maxLevel = 10)
        {
            if (_cooldownStrategy == null)
                _cooldownStrategy = ScalingStrategyFactory.GetStrategy(CooldownScalingType);

            float addedValue = _cooldownStrategy.Calculate(level, maxLevel, CooldownScalingConfig);
            return BaseCooldown + addedValue;
        }

        /// <summary>
        /// 지정된 레벨에 맞는 최종 자원 소모량을 계산합니다.
        /// </summary>
        public int GetScaledResourceCost(int level, int maxLevel = 10)
        {
            if (_resourceCostStrategy == null)
                _resourceCostStrategy = ScalingStrategyFactory.GetStrategy(ResourceCostScalingType);

            float addedValue = _resourceCostStrategy.Calculate(level, maxLevel, ResourceCostScalingConfig);
            return BaseResourceCost + (int)addedValue;
        }
    }
} 