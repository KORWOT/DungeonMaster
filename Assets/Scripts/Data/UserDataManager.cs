using System;
using UnityEngine;
using DungeonMaster.Character;
using DungeonMaster.Dungeon;
using DungeonMaster.Localization;
using DungeonMaster.Utility;
using System.Collections.Generic;
using System.Collections;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 유저 데이터 관리자 - 분리된 데이터 시스템들을 조율하는 역할
    /// </summary>
    public static class UserDataManager
    {
        private static UserData _currentUserData;

        /// <summary>
        /// 현재 유저 데이터
        /// </summary>
        public static UserData CurrentUserData
        {
            get
            {
                if (_currentUserData == null)
                {
                    LoadUserData();
                }
                return _currentUserData;
            }
            private set => _currentUserData = value;
        }

        /// <summary>
        /// 유저 데이터 초기화 (게임 시작 시 호출)
        /// </summary>
        public static void Initialize()
        {
            // 데이터베이스 초기화 (최초 접근 시 자동 초기화되지만 명시적으로 호출 가능)
            if (BlueprintDatabase.Instance == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetText("error_blueprint_database_not_found"));
                return;
            }
            
            // 데이터 로드
            LoadUserData();
            
            // 시작 카드들 확인 및 지급
            if (StarterCardProvider.EnsureStarterCards(CurrentUserData))
            {
                SaveUserData(); // 시작 카드 지급 시 저장
            }
            
            GameLogger.LogInfo(LocalizationManager.Instance.GetText("info_user_data_manager_initialized"));
        }

        /// <summary>
        /// 유저 데이터 저장
        /// </summary>
        public static void SaveUserData()
        {
            if (SaveDataManager.SaveUserData(CurrentUserData))
            {
                GameLogger.LogInfo(LocalizationManager.Instance.GetText("info_user_data_saved"));
            }
        }

        /// <summary>
        /// 유저 데이터 로드
        /// </summary>
        public static void LoadUserData()
        {
            CurrentUserData = SaveDataManager.LoadUserData();
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("info_user_data_loaded", 
                GetResourceStatus(CurrentUserData)));
        }

        /// <summary>
        /// 카드 획득
        /// </summary>
        public static void AcquireCard(string cardName)
        {
            var blueprint = BlueprintDatabase.Instance.GetBlueprint(cardName);
            if (blueprint == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("error_card_blueprint_not_found", cardName));
                return;
            }
            CurrentUserData.CardCollection.AcquireCard(blueprint.BlueprintId);
            SaveUserData(); // 자동 저장
        }

        /// <summary>
        /// 카드 강화
        /// </summary>
        public static bool EnhanceCard(long cardId, StatType statToEnhance, long enhancementValue = 1)
        {
            // TODO: UserCardCollection에 EnhanceCard 로직 구현 필요
            // var success = CurrentUserData.CardCollection.EnhanceCard(cardId, statToEnhance, enhancementValue);
            // if (success)
            // {
            //     SaveUserData(); // 성공 시 자동 저장
            // }
            // return success;
            return false;
        }

        /// <summary>
        /// 골드 추가
        /// </summary>
        public static void AddGold(int amount)
        {
            AddGold(CurrentUserData, amount);
            SaveUserData();
        }

        /// <summary>
        /// 젬 추가
        /// </summary>
        public static void AddGems(int amount)
        {
            AddGems(CurrentUserData, amount);
            SaveUserData();
        }

        /// <summary>
        /// 골드 소모
        /// </summary>
        public static bool SpendGold(int amount)
        {
            if (SpendGold(CurrentUserData, amount))
            {
                SaveUserData();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 젬 소모
        /// </summary>
        public static bool SpendGems(int amount)
        {
            if (SpendGems(CurrentUserData, amount))
            {
                SaveUserData();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 경험치 추가
        /// </summary>
        public static bool AddExperience(long userCardId, long amount)
        {
            UserCardData card = CurrentUserData.CardCollection.GetCard(userCardId);
            if (card == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("error_card_not_found", userCardId));
                return false;
            }

            bool canLevelUp = CharacterGrowthService.Instance.AddExperience(card, amount);
            
            if (canLevelUp)
            {
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("info_card_can_level_up", card.BlueprintId));
                // Optionally, fire an event here for UI to listen to.
                // OnCardCanLevelUp?.Invoke(userCardId);
            }
            
            SaveUserData();
            return canLevelUp;
        }

        public static bool LevelUpCard(long userCardId)
        {
            UserCardData card = CurrentUserData.CardCollection.GetCard(userCardId);
            if (card == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("error_card_not_found", userCardId));
                return false;
            }

            var (goldCost, gemCost) = CharacterGrowthService.Instance.GetCostForLevelUp(card.Level);

            if (!HasEnoughGold(CurrentUserData, goldCost))
            {
                GameLogger.LogWarning(LocalizationManager.Instance.GetText("warn_not_enough_gold_levelup"));
                return false;
            }

            if (!HasEnoughGems(CurrentUserData, gemCost))
            {
                GameLogger.LogWarning(LocalizationManager.Instance.GetText("warn_not_enough_gems_levelup"));
                return false;
            }

            // Spend resources first
            SpendGold(CurrentUserData, goldCost);
            SpendGems(CurrentUserData, gemCost);

            // Then perform the level up
            bool success = CharacterGrowthService.Instance.AttemptLevelUp(card);

            if (success)
            {
                SaveUserData();
            }
            
            return success;
        }

        /// <summary>
        /// 자원 상태 확인
        /// </summary>
        public static string GetResourceStatus()
        {
            return GetResourceStatus(CurrentUserData);
        }

        /// <summary>
        /// 시작 카드 상태 확인
        /// </summary>
        public static (int given, int total) GetStarterCardStatus()
        {
            return StarterCardProvider.GetStarterCardStatus(CurrentUserData);
        }

        /// <summary>
        /// 게임 종료 시 호출
        /// </summary>
        public static void OnApplicationQuit()
        {
            SaveUserData();
            GameLogger.LogInfo(LocalizationManager.Instance.GetText("info_game_quit_data_saved"));
        }

        #region Dungeon Data Management
        
        /// <summary>
        /// DungeonManager로부터 받은 현재 던전 데이터를 유저 데이터에 저장합니다.
        /// 이 메서드 호출 후에는 반드시 UserDataManager.SaveUserData()를 호출해야 영구 저장됩니다.
        /// </summary>
        public static void SaveCurrentDungeon(DungeonData dungeonData)
        {
            if (CurrentUserData == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetText("dungeon_log_error_no_user_data"));
                return;
            }
            CurrentUserData.PlayerDungeon = dungeonData;
            SaveUserData(); // 던전 데이터 변경 후 즉시 전체 유저 데이터 저장
            GameLogger.LogInfo(LocalizationManager.Instance.GetText("dungeon_log_info_dungeon_data_updated"));
        }

        /// <summary>
        /// 현재 유저 데이터에 저장된 던전 데이터를 반환합니다.
        /// </summary>
        /// <returns>저장된 DungeonData. 없을 경우 null.</returns>
        public static DungeonData GetDungeon()
        {
            return CurrentUserData?.PlayerDungeon;
        }

        #endregion

        // --- Resource Management Logic (from ResourceManager) ---

        private static void AddGold(UserData userData, int amount)
        {
            if (userData == null || amount <= 0) return;
            userData.Gold += amount;
        }

        private static void AddGems(UserData userData, int amount)
        {
            if (userData == null || amount <= 0) return;
            userData.Gems += amount;
        }

        private static bool SpendGold(UserData userData, int amount)
        {
            if (HasEnoughGold(userData, amount))
            {
                userData.Gold -= amount;
                return true;
            }
            return false;
        }

        private static bool SpendGems(UserData userData, int amount)
        {
            if (HasEnoughGems(userData, amount))
            {
                userData.Gems -= amount;
                return true;
            }
            return false;
        }

        private static bool HasEnoughGold(UserData userData, int amount)
        {
            return userData != null && userData.Gold >= amount;
        }

        private static bool HasEnoughGems(UserData userData, int amount)
        {
            return userData != null && userData.Gems >= amount;
        }

        private static string GetResourceStatus(UserData userData)
        {
            if (userData == null) return LocalizationManager.Instance.GetText("info_user_data_null");
            return $"Gold: {userData.Gold}, Gems: {userData.Gems}";
        }

        /// <summary>
        /// 유저 데이터 초기화 (에디터용)
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void ResetUserData()
        {
            if (SaveDataManager.DeleteSaveFile())
            {
                _currentUserData = null;
                GameLogger.LogInfo(LocalizationManager.Instance.GetText("info_user_data_reset_completed"));
                
                // 새로운 데이터로 초기화
                Initialize();
            }
        }

        /// <summary>
        /// 백업 생성
        /// </summary>
        public static bool CreateBackup()
        {
            return SaveDataManager.CreateBackup(CurrentUserData);
        }

        /// <summary>
        /// 백업에서 복원
        /// </summary>
        public static bool RestoreFromBackup()
        {
            var backupData = SaveDataManager.RestoreFromBackup();
            if (backupData != null)
            {
                CurrentUserData = backupData;
                SaveUserData();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        public static void PrintDebugInfo()
        {
            if (CurrentUserData == null) return;
            
            Debug.Log($"<color=yellow>--- User Data ---</color>");
            Debug.Log(CurrentUserData.ToString());
            Debug.Log($"<color=yellow>--- Starter Cards ---</color>");
            var (given, total) = GetStarterCardStatus();
            Debug.Log($"Given: {given} / Total: {total}");
        }
    }
    
    /// <summary>
    /// 마왕 장비 슬롯 타입
    /// </summary>
    [Serializable]
    public enum DemonLordEquipmentSlot
    {
        Weapon,
        Armor,
        Necklace,
        Ring1,
        Ring2,
        Bracelet,
        Unique // 전용장비
    }

    /// <summary>
    /// 마왕 스킬 슬롯 타입
    /// </summary>
    [Serializable]
    public enum DemonLordSkillSlot
    {
        Active1,
        Active2,
        Active3,
        Ultimate,
        Passive1,
        Passive2,
        Passive3
    }

    /// <summary>
    /// 플레이어의 영구적인 마왕 성장 데이터를 담는 클래스입니다.
    /// </summary>
    [Serializable]
    public class DemonLordPersistentData
    {
        public string LinkedBlueprintId;
        public int Level = 1;
        public long CurrentXP = 0;
        
        // StatType을 키로, 성장률 배율(1.0f = 100%)을 값으로 가집니다.
        public Dictionary<StatType, float> GrowthRateMultipliers = new Dictionary<StatType, float>();
        
        // 패시브 Blueprint ID를 키로, 현재 레벨을 값으로 가집니다.
        public Dictionary<string, int> UniquePassiveLevels = new Dictionary<string, int>();

        public List<string> OwnedSkillCardGuids = new List<string>();
        public List<string> OwnedPermanentEquipmentGuids = new List<string>();

        public Dictionary<DemonLordSkillSlot, string> EquippedSkills = new Dictionary<DemonLordSkillSlot, string>();
        public Dictionary<DemonLordEquipmentSlot, string> EquippedPermanentEquipment = new Dictionary<DemonLordEquipmentSlot, string>();
    }


    /// <summary>
    /// 유저 데이터 클래스
    /// </summary>
    [Serializable]
    public class UserData
    {
        /// <summary>
        /// 사용자 고유 ID
        /// </summary>
        public string UserId { get; set; } = System.Guid.NewGuid().ToString();

        /// <summary>
        /// 사용자 이름
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 사용자 레벨
        /// </summary>
        public int UserLevel { get; set; } = 1;

        /// <summary>
        /// 사용자 경험치
        /// </summary>
        public int UserExperience { get; set; } = 0;

        /// <summary>
        /// 골드
        /// </summary>
        public int Gold { get; set; } = 1000;

        /// <summary>
        /// 젬
        /// </summary>
        public int Gems { get; set; } = 100;

        /// <summary>
        /// 카드 컬렉션
        /// </summary>
        public UserCardCollection CardCollection { get; set; } = new UserCardCollection();
        
        /// <summary>
        /// 플레이어가 소유하고 성장시키는 던전의 데이터입니다.
        /// </summary>
        public DungeonData PlayerDungeon { get; set; }

        /// <summary>
        /// 플레이어가 소유하고 성장시키는 마왕의 데이터입니다.
        /// </summary>
        public DemonLordPersistentData DemonLordData { get; set; } = new DemonLordPersistentData();

        /// <summary>
        /// 마지막 접속 시간
        /// </summary>
        public DateTime LastAccessTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 생성 시간
        /// </summary>
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        public UserData()
        {
            UserId = System.Guid.NewGuid().ToString();
            UserName = LocalizationManager.Instance.GetText("default_new_player_name");
            UserLevel = 1;
            UserExperience = 0;
            Gold = 1000;
            Gems = 100;
            CardCollection = new UserCardCollection();
            LastAccessTime = DateTime.Now;
            CreatedTime = DateTime.Now;
            // 초기 생성 시에는 던전 데이터가 없습니다.
            // GameManager 또는 DungeonManager에서 필요 시 생성합니다.
            PlayerDungeon = null;
        }

        public override string ToString()
        {
            return $"User: {UserName} (Lv.{UserLevel}) - Gold: {Gold}, Gems: {Gems}, Cards: {CardCollection?.GetOwnedCardCount() ?? 0}";
        }
    }
} 