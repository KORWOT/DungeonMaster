using UnityEngine;

namespace DungeonMaster.Localization
{
    /// <summary>
    /// 언어별 설정 정보 (하드코딩 방지)
    /// </summary>
    [System.Serializable]
    public struct LanguageInfo
    {
        [Tooltip("언어 코드")]
        public SupportedLanguage language;
        
        [Tooltip("사용자에게 표시되는 언어명")]
        public string displayName;
        
        [Tooltip("ISO 639-1 언어 코드")]
        public string isoCode;
        
        [Tooltip("Unity SystemLanguage 매핑")]
        public SystemLanguage unitySystemLanguage;
    }

    /// <summary>
    /// 모든 언어 설정을 관리하는 ScriptableObject
    /// SOLID 원칙: 단일 책임 - 언어 정보 저장만 담당
    /// </summary>
    [CreateAssetMenu(fileName = "LanguageSettings", menuName = "Localization/Language Settings")]
    public class LanguageSettings : ScriptableObject
    {
        [Header("지원 언어 목록")]
        [SerializeField] private LanguageInfo[] supportedLanguages = new LanguageInfo[]
        {
            new LanguageInfo 
            { 
                language = SupportedLanguage.Korean, 
                displayName = "한국어", 
                isoCode = "ko", 
                unitySystemLanguage = SystemLanguage.Korean 
            },
            new LanguageInfo 
            { 
                language = SupportedLanguage.English, 
                displayName = "English", 
                isoCode = "en", 
                unitySystemLanguage = SystemLanguage.English 
            },
            new LanguageInfo 
            { 
                language = SupportedLanguage.Japanese, 
                displayName = "日本語", 
                isoCode = "ja", 
                unitySystemLanguage = SystemLanguage.Japanese 
            },
            new LanguageInfo 
            { 
                language = SupportedLanguage.Chinese, 
                displayName = "中文", 
                isoCode = "zh", 
                unitySystemLanguage = SystemLanguage.ChineseSimplified 
            }
        };

        [Header("기본 설정")]
        [SerializeField] private SupportedLanguage defaultLanguage = SupportedLanguage.Korean;

        public LanguageInfo[] SupportedLanguages => supportedLanguages;
        public SupportedLanguage DefaultLanguage => defaultLanguage;

        /// <summary>
        /// 언어 코드로 언어 정보 찾기
        /// </summary>
        public LanguageInfo GetLanguageInfo(SupportedLanguage language)
        {
            foreach (var langInfo in supportedLanguages)
            {
                if (langInfo.language == language)
                    return langInfo;
            }
            
            // 기본값 반환
            return GetLanguageInfo(defaultLanguage);
        }
    }
} 