using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Character;
using DungeonMaster.Data;
using DungeonMaster.DemonLord;
using DungeonMaster.GameSystem;
using DungeonMaster.Localization;
using DungeonMaster.Shared.Scaling;
using DungeonMaster.Utility;
using UnityEngine;

namespace DungeonMaster.Character
{
    public class CharacterGrowthService : ScriptableObject
    {
        [SerializeField] private GrowthConfig _growthConfig;
        [SerializeField] private DemonLordGradeConfig _demonLordGradeConfig;
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
                GameLogger.LogWarning(LocalizationManager.Instance.GetText("log_growth_invalid_input"));
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
                GameLogger.LogWarning(LocalizationManager.Instance.GetText("log_growth_invalid_input"));
                return false;
            }

            long requiredExp = GetRequiredExperienceForNextLevel(card.Level);
            if (card.Experience < requiredExp)
            {
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_growth_levelup_fail_exp", card.Experience, requiredExp));
                return false;
            }

            var blueprint = BlueprintDatabase.Instance.GetBlueprint(card.BlueprintId);
            if (blueprint == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("log_growth_levelup_fail_blueprint", card.BlueprintId));
                return false;
            }

            // 경험치 차감 및 레벨 증가
            card.Experience -= requiredExp;
            card.Level++;

            ApplyGrowth(card, blueprint);
            
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_growth_levelup_success", blueprint.Name, card.Id, card.Level));

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
                GameLogger.LogError(LocalizationManager.Instance.GetText("log_growth_apply_fail_null"));
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
        
        #region Demon Lord Growth
        
        /// <summary>
        /// 마왕의 현재 레벨과 청사진을 기반으로 최종 스탯을 계산합니다.
        /// </summary>
        public Dictionary<StatType, long> CalculateDemonLordFinalStats(DemonLordBlueprint blueprint, DemonLordPersistentData demonLordData)
        {
            var finalStats = new Dictionary<StatType, long>();
            if (blueprint == null || demonLordData == null) return finalStats;

            // 1. 기본 스탯 복사
            foreach (var stat in blueprint.BaseStats)
            {
                finalStats[stat.StatType] = stat.Value;
            }

            // 2. 레벨업에 따른 성장치 계산
            if (demonLordData.Level > 1)
            {
                int levelsToGrow = demonLordData.Level - 1;
                
                // 2-1. 등급별 성장 배율 가져오기
                float gradeMultiplier = _demonLordGradeConfig.GetMultiplier(blueprint.Grade);

                foreach (var growthStat in blueprint.GrowthStats_x100)
                {
                    // 2-2. 기본 성장률 (x100된 값)
                    long baseGrowth = growthStat.Value;
                    
                    // 2-3. 커스텀 성장률 배율 (기본값 1.0f)
                    float customMultiplier = demonLordData.GrowthRateMultipliers.GetValueOrDefault(growthStat.StatType, 1.0f);

                    // 2-4. 최종 성장치 계산
                    // (기본성장/100) * 커스텀배율 * 등급배율 * 성장레벨수
                    long finalGrowthAmount = (long)(baseGrowth / 100f * customMultiplier * gradeMultiplier * levelsToGrow);

                    finalStats.AddValue(growthStat.StatType, finalGrowthAmount);
                }
            }
            
            return finalStats;
        }

        public bool AddExperienceToDemonLord(DemonLordPersistentData demonLordData, long amount)
        {
            if (demonLordData == null || amount <= 0) return false;
            demonLordData.CurrentXP += amount;
            // TODO: 마왕용 경험치 테이블 필요
            // long requiredExp = GetRequiredExperienceForDemonLord(demonLordData.Level);
            // return demonLordData.CurrentXP >= requiredExp;
            return false; // 임시
        }

        public bool AttemptLevelUpDemonLord(DemonLordPersistentData demonLordData)
        {
            if (demonLordData == null) return false;
            
            // TODO: 마왕용 경험치 및 재화 소모 로직 필요

            var blueprint = _blueprintDatabase.GetDemonLordBlueprint(demonLordData.LinkedBlueprintId);
            if (blueprint == null) return false;

            demonLordData.Level++;
            ApplyDemonLordGrowth(demonLordData, blueprint);
            return true;
        }

        private void ApplyDemonLordGrowth(DemonLordPersistentData demonLordData, DemonLordBlueprint blueprint)
        {
            // 이 메서드는 레벨업 시 스탯이 변하는 "영구 데이터"에는 영향을 주지 않음.
            // 스탯은 항상 CalculateDemonLordFinalStats를 통해 동적으로 계산됨.
            // 여기서는 레벨업 시 스킬 해금 등 다른 효과를 처리할 수 있음.
            GameLogger.LogInfo("Demon Lord leveled up! Future growth effects can be applied here.");
        }

        #endregion
    }
} 