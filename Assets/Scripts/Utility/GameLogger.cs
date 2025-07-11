using UnityEngine;

namespace DungeonMaster.Utility
{
    /// <summary>
    /// 게임 전용 로깅 유틸리티
    /// 빌드 타입에 따라 로그 출력을 제어합니다.
    /// </summary>
    public static class GameLogger
    {
        private static bool isDebugBuild = Application.isEditor || Debug.isDebugBuild;
        
        /// <summary>
        /// 개발용 정보 로그 (릴리즈에서는 출력되지 않음)
        /// </summary>
        public static void LogDev(object message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[DEV] {message}");
#endif
        }
        
        /// <summary>
        /// 개발용 경고 로그 (릴리즈에서는 출력되지 않음)
        /// </summary>
        public static void LogDevWarning(object message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"[DEV] {message}");
#endif
        }
        
        /// <summary>
        /// 중요한 정보 로그 (모든 빌드에서 출력)
        /// </summary>
        public static void LogInfo(object message)
        {
            Debug.Log($"[INFO] {message}");
        }
        
        /// <summary>
        /// 경고 로그 (모든 빌드에서 출력)
        /// </summary>
        public static void LogWarning(object message)
        {
            Debug.LogWarning($"[WARNING] {message}");
        }
        
        /// <summary>
        /// 에러 로그 (모든 빌드에서 출력)
        /// </summary>
        public static void LogError(object message)
        {
            Debug.LogError($"[ERROR] {message}");
        }

        /// <summary>
        /// 컨텍스트 오브젝트를 포함하는 에러 로그
        /// </summary>
        public static void LogError(object message, Object context)
        {
            Debug.LogError($"[ERROR] {message}", context);
        }

        /// <summary>
        /// 스킬 시스템 전용 로그
        /// </summary>
        public static void LogSkill(object message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[SKILL] {message}");
#endif
        }
        
        /// <summary>
        /// 장비 시스템 전용 로그
        /// </summary>
        public static void LogEquipment(object message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[EQUIPMENT] {message}");
#endif
        }
        
        /// <summary>
        /// 전투 시스템 전용 로그
        /// </summary>
        public static void LogBattle(object message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[BATTLE] {message}");
#endif
        }
    }
} 