using System;
using UnityEngine;
using DungeonMaster.Character;

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
                Debug.LogError("BlueprintDatabase를 찾을 수 없습니다! Resources/Data 폴더에 BlueprintDatabase.asset이 있는지 확인하세요.");
                return;
            }
            
            // 데이터 로드
            LoadUserData();
            
            // 시작 카드들 확인 및 지급
            if (StarterCardProvider.EnsureStarterCards(CurrentUserData))
            {
                SaveUserData(); // 시작 카드 지급 시 저장
            }
            
            Debug.Log("UserDataManager 초기화 완료");
        }

        /// <summary>
        /// 유저 데이터 저장
        /// </summary>
        public static void SaveUserData()
        {
            if (SaveDataManager.SaveUserData(CurrentUserData))
            {
                Debug.Log("유저 데이터 저장 완료");
            }
        }

        /// <summary>
        /// 유저 데이터 로드
        /// </summary>
        public static void LoadUserData()
        {
            CurrentUserData = SaveDataManager.LoadUserData();
            Debug.Log($"유저 데이터 로드 완료 - {ResourceManager.GetResourceStatus(CurrentUserData)}");
        }

        /// <summary>
        /// 카드 획득
        /// </summary>
        public static void AcquireCard(string cardName)
        {
            var blueprint = BlueprintDatabase.Instance.GetBlueprint(cardName);
            if (blueprint == null)
            {
                Debug.LogError($"[UserDataManager] '{cardName}'에 해당하는 카드 설계도를 찾을 수 없습니다.");
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
            ResourceManager.AddGold(CurrentUserData, amount);
            SaveUserData();
        }

        /// <summary>
        /// 젬 추가
        /// </summary>
        public static void AddGems(int amount)
        {
            ResourceManager.AddGems(CurrentUserData, amount);
            SaveUserData();
        }

        /// <summary>
        /// 골드 소모
        /// </summary>
        public static bool SpendGold(int amount)
        {
            if (ResourceManager.SpendGold(CurrentUserData, amount))
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
            if (ResourceManager.SpendGems(CurrentUserData, amount))
            {
                SaveUserData();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 경험치 추가
        /// </summary>
        public static bool AddExperience(int amount)
        {
            bool leveledUp = ResourceManager.AddExperience(CurrentUserData, amount);
            if (leveledUp || amount > 0)
            {
                SaveUserData();
            }
            return leveledUp;
        }

        /// <summary>
        /// 자원 상태 확인
        /// </summary>
        public static string GetResourceStatus()
        {
            return ResourceManager.GetResourceStatus(CurrentUserData);
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
            Debug.Log("게임 종료 시 데이터 저장 완료");
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
                Debug.Log("유저 데이터 초기화 완료");
                
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
            if (CurrentUserData == null)
            {
                Debug.Log("UserData가 없습니다!");
                return;
            }

            Debug.Log("=== 유저 데이터 디버그 정보 ===");
            Debug.Log($"사용자 ID: {CurrentUserData.UserId}");
            Debug.Log($"사용자 이름: {CurrentUserData.UserName}");
            Debug.Log($"레벨: {CurrentUserData.UserLevel}");
            Debug.Log($"경험치: {CurrentUserData.UserExperience}");
            Debug.Log($"자원: {ResourceManager.GetResourceStatus(CurrentUserData)}");
            Debug.Log($"카드 컬렉션: {CurrentUserData.CardCollection}");
            Debug.Log($"시작 카드 상태: {GetStarterCardStatus()}");
            Debug.Log($"마지막 접속: {CurrentUserData.LastAccessTime}");
            Debug.Log($"생성 시간: {CurrentUserData.CreatedTime}");
        }
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
        public string UserName { get; set; } = "새로운 플레이어";

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
            UserName = "새로운 플레이어";
            UserLevel = 1;
            UserExperience = 0;
            Gold = 1000;
            Gems = 100;
            CardCollection = new UserCardCollection();
            LastAccessTime = DateTime.Now;
            CreatedTime = DateTime.Now;
        }

        public override string ToString()
        {
            return $"User: {UserName} (Lv.{UserLevel}) - Gold: {Gold}, Gems: {Gems}, Cards: {CardCollection?.GetOwnedCardCount() ?? 0}";
        }
    }
} 