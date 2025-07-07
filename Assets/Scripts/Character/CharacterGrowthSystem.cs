using DungeonMaster.Data;
using System;
using System.Collections.Generic;

namespace DungeonMaster.Character
{
    /// <summary>
    /// 캐릭터 성장 시스템.
    /// UserCardData의 레벨, 경험치, 성장률 같은 영구적인 데이터를 관리하고 계산합니다.
    /// 이 클래스는 Unity에 종속되지 않는 순수 C# 로직으로만 구성됩니다.
    /// </summary>
    public static class CharacterGrowthSystem
    {
        /// <summary>
        /// 캐릭터의 레벨업을 처리합니다. UserCardData의 레벨을 1 올립니다.
        /// 스탯의 최종 계산은 CharacterDataFactory에서 수행하므로, 여기서는 레벨만 올립니다.
        /// </summary>
        public static CharacterGrowthResult ProcessLevelUp(UserCardData userCard)
        {
            if (userCard == null) throw new ArgumentNullException(nameof(userCard));

            var result = new CharacterGrowthResult
            {
                PreviousLevel = userCard.Level,
                NewLevel = userCard.Level + 1
            };

            userCard.Level++;
            
            // 실제 스탯 계산은 Factory가 수행하므로 여기서는 로그용으로만 계산하여 보여줄 수 있습니다.
            // 또는 UI에서 표시할 때 Factory를 통해 이전 레벨과 새 레벨의 스탯을 각각 계산하여 차이를 보여줄 수도 있습니다.

            return result;
        }
        
        /// <summary>
        /// 경험치 획득을 처리하고, 필요 시 자동 레벨업을 수행합니다.
        /// </summary>
        public static CharacterExperienceResult GainExperience(UserCardData userCard, int experienceAmount)
        {
            if (userCard == null) throw new ArgumentNullException(nameof(userCard));
            if (experienceAmount <= 0) throw new ArgumentOutOfRangeException(nameof(experienceAmount));

            var result = new CharacterExperienceResult
            {
                InitialLevel = userCard.Level,
                InitialExperience = userCard.Experience,
                ExperienceGained = experienceAmount,
                LevelUpsOccurred = 0,
                GrowthResults = new System.Collections.Generic.List<CharacterGrowthResult>()
            };

            userCard.Experience += experienceAmount;

            // LevelingConfig 에셋에서 필요 경험치를 가져옵니다.
            long requiredExperience = LevelingConfig.Instance.GetExperienceRequiredForLevel(userCard.Level);

            // 경험치가 0이거나 최대값(설정 오류)인 경우 레벨업을 중단합니다.
            while (requiredExperience > 0 && requiredExperience != long.MaxValue && userCard.Experience >= requiredExperience)
            {
                userCard.Experience -= requiredExperience;
                var growthResult = ProcessLevelUp(userCard);
                
                if (growthResult != null)
                {
                    result.GrowthResults.Add(growthResult);
                    result.LevelUpsOccurred++;
                }
                
                // 다음 레벨에 필요한 경험치 업데이트
                requiredExperience = LevelingConfig.Instance.GetExperienceRequiredForLevel(userCard.Level);
            }

            result.FinalLevel = userCard.Level;
            result.FinalExperience = userCard.Experience;
            
            return result;
        }
    }
    
    // --- Data Transfer Objects (DTO) ---
    // 아래 클래스들은 시스템 간 데이터 전달을 위해 사용됩니다.
    
    /// <summary>
    /// 캐릭터 성장 결과 정보
    /// </summary>
    [System.Serializable]
    public class CharacterGrowthResult
    {
        public int PreviousLevel;
        public int NewLevel;
        // 필요하다면 레벨업 전/후 스탯 정보(Factory를 통해 계산된)를 여기에 추가할 수 있습니다.
    }
    
    /// <summary>
    /// 캐릭터 경험치 획득 결과 정보
    /// </summary>
    [System.Serializable]
    public class CharacterExperienceResult
    {
        public int InitialLevel;
        public int FinalLevel;
        public long InitialExperience;
        public long FinalExperience;
        public int ExperienceGained;
        public int LevelUpsOccurred;
        public System.Collections.Generic.List<CharacterGrowthResult> GrowthResults;
    }
} 