using UnityEngine;
using System.Collections.Generic;
using DungeonMaster.Localization;
using DungeonMaster.Utility;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 레벨업에 필요한 경험치 테이블을 관리하는 ScriptableObject입니다.
    /// 게임 디자이너가 Unity 에디터에서 직접 레벨별 필요 경험치를 수정할 수 있습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelingConfig", menuName = "Game/Configuration/Leveling Configuration")]
    public class LevelingConfig : ScriptableObject
    {
        [Header("레벨별 필요 경험치")]
        [Tooltip("인덱스 0 = 레벨 1->2에 필요한 경험치, 인덱스 1 = 레벨 2->3에 필요한 경험치, ...")]
        [SerializeField]
        private List<long> experienceRequiredPerLevel = new List<long>();

        private static LevelingConfig _instance;
        public static LevelingConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Resources 폴더에서 에셋을 로드합니다.
                    // 여러 설정 파일을 관리하기 위해 Settings 폴더 안에 저장하는 것을 권장합니다.
                    _instance = Resources.Load<LevelingConfig>("Settings/LevelingConfig");
                    if (_instance == null)
                    {
                        GameLogger.LogError(LocalizationManager.Instance.GetText("error_leveling_config_not_found"));
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 지정된 레벨에서 다음 레벨로 레벨업하는 데 필요한 경험치를 반환합니다.
        /// </summary>
        /// <param name="currentLevel">현재 레벨 (1부터 시작)</param>
        /// <returns>필요 경험치. 설정된 최대 레벨을 초과하면 마지막 레벨의 필요 경험치를 반환합니다.</returns>
        public long GetExperienceRequiredForLevel(int currentLevel)
        {
            // 레벨은 1부터 시작하지만, 리스트 인덱스는 0부터 시작하므로 currentLevel - 1을 사용합니다.
            int index = currentLevel - 1;

            if (index < 0)
            {
                GameLogger.LogWarning(LocalizationManager.Instance.GetTextFormatted("warn_invalid_level_request", currentLevel));
                // 잘못된 입력에 대해 기본값 또는 가장 낮은 레벨의 요구 사항을 반환할 수 있습니다.
                return experienceRequiredPerLevel.Count > 0 ? experienceRequiredPerLevel[0] : 0;
            }

            if (experienceRequiredPerLevel == null || experienceRequiredPerLevel.Count == 0)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetText("error_exp_table_empty"));
                return long.MaxValue; // 레벨업을 방지하기 위해 큰 값을 반환
            }

            // 정의된 마지막 레벨보다 높은 레벨의 경우, 마지막으로 설정된 경험치 요구량을 계속 사용합니다. (만렙 처리)
            if (index >= experienceRequiredPerLevel.Count)
            {
                index = experienceRequiredPerLevel.Count - 1;
            }

            return experienceRequiredPerLevel[index];
        }
    }
} 