using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Character;
using DungeonMaster.Data;
using DungeonMaster.Shared.Scaling;
using DungeonMaster.Utility;
using UnityEngine;

namespace DungeonMaster.Character
{
    public class CharacterGrowthService : ScriptableObject
    {
        [SerializeField] private GrowthConfig _growthConfig;
        [SerializeField] private BlueprintDatabase _blueprintDatabase;

        // 이 서비스는 ScriptableObject이므로, 싱글턴 인스턴스가 필요합니다.
        private static CharacterGrowthService _instance;
        public static CharacterGrowthService Instance
        {
            get
            {
                if (_instance == null)
                {
                    // "Resources" 폴더에서 로드합니다.
                    // 이 방식은 Resources 폴더에 이 SO의 인스턴스가 하나만 존재한다고 가정합니다.
                    _instance = Resources.Load<CharacterGrowthService>(nameof(CharacterGrowthService));
                    if (_instance == null)
                    {
                        GameLogger.LogError($"Failed to load {nameof(CharacterGrowthService)} from Resources. Make sure an instance exists.");
                    }
                }
                return _instance;
            }
        }

        public (int gold, int gems) GetCostForLevelUp(int currentLevel)
        {
            return _growthConfig.GetCostForLevel(currentLevel + 1);
        }

        /// <summary>
        /// 경험치를 추가하고 레벨업 가능 여부를 반환합니다.
        /// </summary>
        public bool AddExperience(UserCardData card, long amount)
        {
            if (card == null || amount <= 0)
            {
                GameLogger.LogWarning("AddExperience: 유효하지 않은 카드 또는 경험치 양입니다.");
                return false;
            }

            card.Experience += amount;
            
            long requiredExp = GetRequiredExperienceForNextLevel(card.Level);
            return card.Experience >= requiredExp;
        }

        /// <summary>
        /// 카드의 레벨업을 시도합니다.
        /// </summary>
        public bool AttemptLevelUp(UserCardData card)
        {
            if (card == null)
            {
                GameLogger.LogWarning("AttemptLevelUp: 카드가 null입니다.");
                return false;
            }

            long requiredExp = GetRequiredExperienceForNextLevel(card.Level);
            if (card.Experience < requiredExp)
            {
                GameLogger.LogInfo($"레벨업 경험치 부족. 현재: {card.Experience}, 필요: {requiredExp}");
                return false;
            }

            var blueprint = BlueprintDatabase.Instance.GetBlueprint(card.BlueprintId);
            if (blueprint == null)
            {
                GameLogger.LogError($"AttemptLevelUp: Blueprint ID {card.BlueprintId}를 찾을 수 없습니다.");
                return false;
            }

            // 경험치 차감 및 레벨 증가
            card.Experience -= requiredExp;
            card.Level++;

            ApplyGrowth(card, blueprint);
            
            GameLogger.LogInfo($"{blueprint.Name} (카드 ID: {card.Id}) 레벨업! -> Lv. {card.Level}");

            return true;
        }

        /// <summary>
        /// 다음 레벨에 필요한 경험치를 반환합니다.
        /// </summary>
        private long GetRequiredExperienceForNextLevel(int currentLevel)
        {
            return _growthConfig.GetExperienceForLevel(currentLevel + 1);
        }

        /// <summary>
        /// 카드의 성장을 적용합니다.
        /// </summary>
        private void ApplyGrowth(UserCardData card, CardBlueprintData blueprint)
        {
            if (card == null || blueprint == null)
            {
                GameLogger.LogError("ApplyGrowth: 카드 또는 블루프린트가 null입니다.");
                return;
            }

            // 성장 가능한 스탯 타입 목록 가져오기
            var growableStats = blueprint.GrowableStatTypes ?? Enumerable.Empty<StatType>();

            foreach (var statType in growableStats)
            {
                // 1. 등급별 기본 성장치 가져오기
                var gradeGrowthMap = _growthConfig.GetBaseGrowthForGrade(blueprint.Grade, statType);
                int gradeGrowth = DictionaryExtensions.GetValueOrDefault(gradeGrowthMap, statType, 0);

                // 2. 카드별 고유 성장률 가져오기 (기본값 100)
                int innateGrowthRate = DictionaryExtensions.GetValueOrDefault(card.InnateGrowthRates_x100, statType, 100);

                // 3. 최종 성장치 계산 (소수점 계산을 위해 100을 곱한 상태로 계산 후 나눔)
                long finalGrowthAmount = (long)gradeGrowth * innateGrowthRate / 100;

                // 4. 스탯 적용
                DictionaryExtensions.AddValue(card.CurrentStats, statType, finalGrowthAmount);
            }
        }
    }
} 