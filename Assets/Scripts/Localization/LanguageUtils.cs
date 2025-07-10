namespace DungeonMaster.Localization
{
    /// <summary>
    /// 순수 C# 언어 유틸리티 (Unity 의존성 없음)
    /// SOLID 원칙: 단일 책임 - 언어 변환 로직만 담당
    /// </summary>
    public static class LanguageUtils
    {
        /// <summary>
        /// 언어 설정을 기반으로 표시 문자열 반환
        /// </summary>
        public static string GetDisplayName(SupportedLanguage language, LanguageSettings settings)
        {
            if (settings == null)
                return language.ToString();
                
            return settings.GetLanguageInfo(language).displayName;
        }
        
        /// <summary>
        /// 언어 설정을 기반으로 ISO 코드 반환
        /// </summary>
        public static string GetISOCode(SupportedLanguage language, LanguageSettings settings)
        {
            if (settings == null)
                return "ko"; // 기본값
                
            return settings.GetLanguageInfo(language).isoCode;
        }
        
        /// <summary>
        /// 언어가 유효한지 검증
        /// </summary>
        public static bool IsValidLanguage(SupportedLanguage language)
        {
            return System.Enum.IsDefined(typeof(SupportedLanguage), language);
        }
    }
} 