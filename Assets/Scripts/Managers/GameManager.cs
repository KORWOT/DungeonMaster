using DungeonMaster.Data;
using DungeonMaster.Dungeon;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Battle;
using DungeonMaster.Localization;
using DungeonMaster.Utility;

namespace DungeonMaster.Managers
{
    /// <summary>
    /// 게임의 전체적인 흐름과 상태를 관리하는 최상위 관리자 클래스입니다.
    /// 이 클래스는 게임의 핵심 상태(예: 메인 메뉴, 인게임, 일시정지)를 전환하고,
    /// 다른 관리자들을 총괄하는 역할을 수행할 수 있습니다.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private DungeonConfig _dungeonConfig;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadConfigs();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadConfigs()
        {
            _dungeonConfig = Resources.Load<DungeonConfig>("DungeonConfig");
            if (_dungeonConfig == null)
            {
                GameLogger.LogError("DungeonConfig not found in Resources folder. Please create one.");
            }
        }

        private void Start()
        {
            InitializeDungeon();
        }

        private void InitializeDungeon()
        {
            var dungeonData = UserDataManager.GetDungeon();

            if (dungeonData == null)
            {
                if (_dungeonConfig == null)
                {
                    GameLogger.LogError("Cannot create new dungeon because DungeonConfig is not loaded.");
                    return;
                }

                GameLogger.LogInfo(LocalizationManager.Instance.GetText("dungeon_log_info_new_dungeon_created"));
                
                var dungeonManager = DungeonManager.Instance;
                
                string dungeonName = LocalizationManager.Instance.GetText(_dungeonConfig.DefaultDungeonNameKey);
                Vector2Int gridSize = _dungeonConfig.DefaultGridSize;
                Vector2Int startPos = _dungeonConfig.DefaultStartPosition;
                Vector2Int bossPos = _dungeonConfig.DefaultBossPosition;

                dungeonManager.CreateNewDungeon(dungeonName, gridSize, startPos, bossPos);
                
                dungeonManager.PlaceRoom(_dungeonConfig.StartRoomBlueprintId, startPos);
                dungeonManager.PlaceRoom(_dungeonConfig.BossRoomBlueprintId, bossPos);

                // 새 던전 생성 후 즉시 저장
                dungeonManager.TrySaveCurrentDungeon();
            }
            else
            {
                GameLogger.LogInfo(LocalizationManager.Instance.GetText("dungeon_log_info_dungeon_loaded"));
                DungeonManager.Instance.LoadDungeon(dungeonData);
            }
        }

        public void StartBattleForEntrance()
        {
            var dungeonManager = DungeonManager.Instance;
            if (dungeonManager.CurrentDungeon == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetText("dungeon_log_error_no_current_dungeon"));
                return;
            }

            // 1. 임시 공격자 목록 생성 (플레이어의 첫 번째 카드를 적으로 사용)
            var allPlayerCards = UserDataManager.CurrentUserData.CardCollection.GetAllCards();
            if (allPlayerCards.Count == 0)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetText("dungeon_log_error_no_player_cards"));
                return;
            }
            var attackerGuids = new List<string> { allPlayerCards.First().Guid };

            // 2. 전투 데이터 생성
            var entrancePosition = dungeonManager.CurrentDungeon.StartPosition;
            var battleData = dungeonManager.CreateBattleLaunchData(entrancePosition, attackerGuids);

            if (battleData == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetText("dungeon_log_error_battle_data_failed"));
                return;
            }

            // 3. 전투 시작
            BattleManager.Instance.StartBattle(battleData);
        }
    }
}