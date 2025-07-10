using UnityEngine;
using DungeonMaster.Localization;
using DungeonMaster.Utility;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 자원 관리자 - 골드, 젬 등 자원 관리 전담
    /// </summary>
    public static class ResourceManager
    {
        /// <summary>
        /// 골드 추가
        /// </summary>
        public static void AddGold(UserData userData, int amount)
        {
            if (userData == null) return;
            
            userData.Gold += amount;
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_gold_added", amount, userData.Gold));
        }
        
        /// <summary>
        /// 젬 추가
        /// </summary>
        public static void AddGems(UserData userData, int amount)
        {
            if (userData == null) return;
            
            userData.Gems += amount;
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_gems_added", amount, userData.Gems));
        }
        
        /// <summary>
        /// 골드 소모
        /// </summary>
        public static bool SpendGold(UserData userData, int amount)
        {
            if (userData == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetText("error_user_data_null"));
                return false;
            }
            
            if (userData.Gold >= amount)
            {
                userData.Gold -= amount;
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_gold_spent", amount, userData.Gold));
                return true;
            }
            else
            {
                GameLogger.LogWarning(LocalizationManager.Instance.GetTextFormatted("log_gold_insufficient", amount, userData.Gold));
                return false;
            }
        }
        
        /// <summary>
        /// 젬 소모
        /// </summary>
        public static bool SpendGems(UserData userData, int amount)
        {
            if (userData == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetText("error_user_data_null"));
                return false;
            }
            
            if (userData.Gems >= amount)
            {
                userData.Gems -= amount;
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_gems_spent", amount, userData.Gems));
                return true;
            }
            else
            {
                GameLogger.LogWarning(LocalizationManager.Instance.GetTextFormatted("log_gems_insufficient", amount, userData.Gems));
                return false;
            }
        }
        
        /// <summary>
        /// 골드 보유량 확인
        /// </summary>
        public static int GetGold(UserData userData)
        {
            return userData?.Gold ?? 0;
        }
        
        /// <summary>
        /// 젬 보유량 확인
        /// </summary>
        public static int GetGems(UserData userData)
        {
            return userData?.Gems ?? 0;
        }
        
        /// <summary>
        /// 골드 구매 가능 여부 확인
        /// </summary>
        public static bool CanAffordGold(UserData userData, int amount)
        {
            return userData != null && userData.Gold >= amount;
        }
        
        /// <summary>
        /// 젬 구매 가능 여부 확인
        /// </summary>
        public static bool CanAffordGems(UserData userData, int amount)
        {
            return userData != null && userData.Gems >= amount;
        }
        
        /// <summary>
        /// 자원 상태 정보
        /// </summary>
        public static string GetResourceStatus(UserData userData)
        {
            if (userData == null) return LocalizationManager.Instance.GetText("status_no_data");
            
            // "골드: {0:N0}, 젬: {1:N0}" 형식의 키를 가져와서 포맷팅
            return LocalizationManager.Instance.GetTextFormatted("status_resource", userData.Gold, userData.Gems);
        }
        
        /// <summary>
        /// 경험치 추가
        /// </summary>
        public static bool AddExperience(UserData userData, int amount)
        {
            if (userData == null) return false;
            
            userData.UserExperience += amount;
            
            // 레벨업 체크 (간단한 공식: 레벨 * 100)
            int requiredExp = userData.UserLevel * 100;
            bool leveledUp = false;
            
            while (userData.UserExperience >= requiredExp)
            {
                userData.UserExperience -= requiredExp;
                userData.UserLevel++;
                leveledUp = true;
                
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_level_up", userData.UserLevel));
                
                // 레벨업 보상 (골드)
                AddGold(userData, userData.UserLevel * 10);
                
                // 다음 레벨 경험치 요구량 계산
                requiredExp = userData.UserLevel * 100;
            }
            
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_exp_gained", amount, userData.UserExperience, requiredExp));
            return leveledUp;
        }
        
        /// <summary>
        /// 다음 레벨까지 필요한 경험치
        /// </summary>
        public static int GetExpToNextLevel(UserData userData)
        {
            if (userData == null) return 0;
            
            int requiredExp = userData.UserLevel * 100;
            return requiredExp - userData.UserExperience;
        }
    }
} 