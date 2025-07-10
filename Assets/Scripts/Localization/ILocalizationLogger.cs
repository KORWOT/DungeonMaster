namespace DungeonMaster.Localization
{
    /// <summary>
    /// 지역화 시스템의 로깅을 담당하는 인터페이스
    /// SOLID 원칙: 의존성 역전 - 구체적인 로거에 의존하지 않음
    /// Unity 의존성 없음 - 서버에서도 사용 가능
    /// </summary>
    public interface ILocalizationLogger
    {
        /// <summary>
        /// 일반 정보 로그
        /// </summary>
        void LogInfo(string message);
        
        /// <summary>
        /// 경고 로그
        /// </summary>
        void LogWarning(string message);
        
        /// <summary>
        /// 에러 로그
        /// </summary>
        void LogError(string message);
    }
    
    /// <summary>
    /// Unity용 로거 구현체
    /// </summary>
    public class UnityLocalizationLogger : ILocalizationLogger
    {
        public void LogInfo(string message)
        {
            UnityEngine.Debug.Log($"[Localization] {message}");
        }
        
        public void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning($"[Localization] {message}");
        }
        
        public void LogError(string message)
        {
            UnityEngine.Debug.LogError($"[Localization] {message}");
        }
    }
    
    /// <summary>
    /// 콘솔용 로거 구현체 (서버용)
    /// </summary>
    public class ConsoleLocalizationLogger : ILocalizationLogger
    {
        public void LogInfo(string message)
        {
            System.Console.WriteLine($"[INFO][Localization] {message}");
        }
        
        public void LogWarning(string message)
        {
            System.Console.WriteLine($"[WARN][Localization] {message}");
        }
        
        public void LogError(string message)
        {
            System.Console.WriteLine($"[ERROR][Localization] {message}");
        }
    }
    
    /// <summary>
    /// 조용한 로거 (테스트용)
    /// </summary>
    public class SilentLocalizationLogger : ILocalizationLogger
    {
        public void LogInfo(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message) { }
    }
} 