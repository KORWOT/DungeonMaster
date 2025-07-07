using UnityEngine;

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
            Debug.Log($"골드 {amount} 추가됨. 현재 골드: {userData.Gold}");
        }
        
        /// <summary>
        /// 젬 추가
        /// </summary>
        public static void AddGems(UserData userData, int amount)
        {
            if (userData == null) return;
            
            userData.Gems += amount;
            Debug.Log($"젬 {amount} 추가됨. 현재 젬: {userData.Gems}");
        }
        
        /// <summary>
        /// 골드 소모
        /// </summary>
        public static bool SpendGold(UserData userData, int amount)
        {
            if (userData == null)
            {
                Debug.LogError("UserData가 null입니다!");
                return false;
            }
            
            if (userData.Gold >= amount)
            {
                userData.Gold -= amount;
                Debug.Log($"골드 {amount} 소모됨. 현재 골드: {userData.Gold}");
                return true;
            }
            else
            {
                Debug.LogWarning($"골드가 부족합니다! 필요: {amount}, 보유: {userData.Gold}");
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
                Debug.LogError("UserData가 null입니다!");
                return false;
            }
            
            if (userData.Gems >= amount)
            {
                userData.Gems -= amount;
                Debug.Log($"젬 {amount} 소모됨. 현재 젬: {userData.Gems}");
                return true;
            }
            else
            {
                Debug.LogWarning($"젬이 부족합니다! 필요: {amount}, 보유: {userData.Gems}");
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
            if (userData == null) return "데이터 없음";
            
            return $"골드: {userData.Gold:N0}, 젬: {userData.Gems:N0}";
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
                
                Debug.Log($"레벨업! 현재 레벨: {userData.UserLevel}");
                
                // 레벨업 보상 (골드)
                AddGold(userData, userData.UserLevel * 10);
                
                // 다음 레벨 경험치 요구량 계산
                requiredExp = userData.UserLevel * 100;
            }
            
            Debug.Log($"경험치 {amount} 획득. 현재: {userData.UserExperience}/{requiredExp}");
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