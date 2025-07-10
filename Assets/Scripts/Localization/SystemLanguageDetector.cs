using UnityEngine;

namespace DungeonMaster.Localization
{
    /// <summary>
    /// Unity 의존적인 시스템 언어 감지 기능
    /// SOLID 원칙: 단일 책임 - 시스템 언어 감지만 담당
    /// </summary>
    public static class SystemLanguageDetector
    {
        /// <summary>
        /// 현재 시스템 언어를 감지하여 지원 언어로 변환
        /// </summary>
        public static SupportedLanguage DetectSystemLanguage(LanguageSettings settings)
        {
            if (settings == null)
                return SupportedLanguage.Korean; // 기본값
            
            var systemLanguage = Application.systemLanguage;
            
            // 설정에서 매핑된 언어 찾기
            foreach (var langInfo in settings.SupportedLanguages)
            {
                if (langInfo.unitySystemLanguage == systemLanguage)
                    return langInfo.language;
            }
            
            // 매핑되지 않은 언어면 기본 언어 반환
            return settings.DefaultLanguage;
        }
        
        /// <summary>
        /// 현재 시스템 언어를 Unity SystemLanguage로 반환
        /// </summary>
        public static SystemLanguage GetUnitySystemLanguage()
        {
            return Application.systemLanguage;
        }
    }
} 