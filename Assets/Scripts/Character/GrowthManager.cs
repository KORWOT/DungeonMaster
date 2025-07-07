using DungeonMaster.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonMaster.Character
{
    /// <summary>
    /// 캐릭터의 성장(레벨업)과 관련된 모든 로직을 처리하는 정적 클래스.
    /// 이 클래스는 UserCardData의 상태를 직접 변경합니다.
    /// </summary>
    public static class GrowthManager
    {
        // TODO: 경험치 테이블은 별도의 ScriptableObject(ExperienceConfig)로 분리하는 것이 좋습니다.
        private static readonly Dictionary<int, long> _expTable = new Dictionary<int, long>
        {
            { 1, 100 }, { 2, 250 }, { 3, 500 } // 예시 데이터
        };

        /// <summary>
        /// 카드에 경험치를 부여하고, 필요 시 레벨업을 처리합니다.
        /// </summary>
        public static void GrantExperienceAndLevelUp(UserCardData card, long expGained)
        {
            card.Experience += expGained;
            Debug.Log($"카드({card.BlueprintId}) 경험치 +{expGained}, 현재 경험치: {card.Experience}");

            while (_expTable.TryGetValue(card.Level, out long requiredExp) && card.Experience >= requiredExp)
            {
                LevelUp(card, 1);
            }
        }

        /// <summary>
        /// 카드를 지정된 레벨만큼 성장시킵니다.
        /// '자리수 단위' 로직을 사용하여 대량 레벨업을 처리합니다.
        /// </summary>
        public static void LevelUp(UserCardData card, int levelsToGain)
        {
            if (levelsToGain <= 0) return;

            Debug.Log($"카드({card.BlueprintId}) 레벨업 시작. 현재 레벨: {card.Level}, 레벨업: {levelsToGain}");

            var blueprint = BlueprintDatabase.Instance.GetBlueprint(card.BlueprintId);
            var growthConfig = Resources.Load<GradeGrowthConfig>("GrowthGradeConfig");
            if (blueprint == null || growthConfig == null)
            {
                Debug.LogError("레벨업에 필요한 Blueprint 또는 GrowthConfig를 찾을 수 없습니다.");
                return;
            }

            var gradeGrowth = growthConfig.GetGradeGrowthData(blueprint.Grade);
            if (gradeGrowth == null)
            {
                Debug.LogError($"등급({blueprint.Grade})에 대한 성장 데이터가 없습니다.");
                return;
            }

            // 자리수 단위로 레벨업 처리
            int remainingLevels = levelsToGain;
            int unit = 1;
            while (remainingLevels > 0)
            {
                int amount = remainingLevels % 10;
                if (amount > 0)
                {
                    ProcessLevelUpUnit(card, blueprint, gradeGrowth, amount * unit);
                }
                remainingLevels /= 10;
                unit *= 10;
            }

            card.Level += levelsToGain;
            Debug.Log($"카드({card.BlueprintId}) 레벨업 완료. 최종 레벨: {card.Level}");
        }
        
        /// <summary>
        /// 특정 단위의 레벨업을 실제로 처리하고 스탯을 증가시키는 메서드.
        /// </summary>
        private static void ProcessLevelUpUnit(UserCardData card, CardBlueprintData blueprint, GradeGrowthData gradeGrowth, int levels)
        {
            // 성장 가능한 모든 스탯에 대해
            foreach (var statType in blueprint.GrowableStatTypes)
            {
                long totalStatGain = 0;
                
                // 해당 레벨업 횟수만큼 반복하며 스탯 증가량 계산
                for (int i = 0; i < levels; i++)
                {
                    // 1. 등급별 기본 성장치 범위(예: C등급 2.5 ~ 5.0)에서 랜덤 값 추출
                    // GradeGrowthData의 Rate는 100을 곱한 정수이므로 100.0f로 나누어 사용합니다.
                    float baseGrowth = UnityEngine.Random.Range(gradeGrowth.MinRate / 100.0f, gradeGrowth.MaxRate / 100.0f);

                    // 2. 카드의 고유 성장률(예: 121%)을 가져옴
                    if (card.InnateGrowthRates_x100.TryGetValue(statType, out int innateRate_x100))
                    {
                        // 3. 최종 성장치 계산
                        float finalGrowth = baseGrowth * (innateRate_x100 / 100.0f);
                        totalStatGain += (long)Math.Round(finalGrowth);
                    }
                }

                // 계산된 총 증가량을 카드의 현재 스탯에 더해줌
                if (!card.CurrentStats.ContainsKey(statType)) card.CurrentStats[statType] = 0;
                card.CurrentStats[statType] += totalStatGain;
                
                Debug.Log($" - {statType}: +{totalStatGain} (총 {card.CurrentStats[statType]})");
            }
        }
    }
} 