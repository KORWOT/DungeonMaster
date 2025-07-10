using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonMaster.Localization
{
    /// <summary>
    /// 지역화된 텍스트 하나의 항목을 나타내는 클래스
    /// SOLID 원칙: 단일 책임 - 언어별 텍스트 저장과 검색만 담당
    /// 확장성: Dictionary 기반으로 새 언어 추가 시 코드 변경 불필요
    /// </summary>
    [Serializable]
    public class LocalizedEntry
    {
        [Header("지역화 키")]
        [SerializeField] private string key;
        
        [Header("각 언어별 텍스트")]
        [SerializeField] private LocalizedText[] texts = new LocalizedText[]
        {
            new LocalizedText { language = SupportedLanguage.Korean, text = "" },
            new LocalizedText { language = SupportedLanguage.English, text = "" },
            new LocalizedText { language = SupportedLanguage.Japanese, text = "" },
            new LocalizedText { language = SupportedLanguage.Chinese, text = "" }
        };
        
        [Header("설명 (에디터용)")]
        [SerializeField, TextArea(1, 2)] private string description;
        
        // 성능을 위한 캐시 (지연 초기화)
        private Dictionary<SupportedLanguage, string> _textCache;
        
        /// <summary>
        /// 언어별 텍스트 저장 구조체
        /// </summary>
        [Serializable]
        public struct LocalizedText
        {
            public SupportedLanguage language;
            [TextArea(1, 3)]
            public string text;
        }
        
        // 프로퍼티
        public string Key => key;
        public string Description => description;
        public LocalizedText[] Texts => texts;
        
        /// <summary>
        /// 지정된 언어의 텍스트를 반환합니다.
        /// </summary>
        public string GetText(SupportedLanguage language)
        {
            // 지연 초기화
            if (_textCache == null)
                InitializeCache();
            
            // 요청된 언어 텍스트 반환
            if (_textCache.TryGetValue(language, out string text) && !string.IsNullOrEmpty(text))
                return text;
                
            // 기본 언어(한국어) 폴백
            if (_textCache.TryGetValue(SupportedLanguage.Korean, out string koreanText) && !string.IsNullOrEmpty(koreanText))
                return koreanText;
                
            // 최후 수단: 키 반환
            return key;
        }
        
        /// <summary>
        /// 텍스트 캐시 초기화
        /// </summary>
        private void InitializeCache()
        {
            _textCache = new Dictionary<SupportedLanguage, string>();
            
            if (texts != null)
            {
                foreach (var localizedText in texts)
                {
                    _textCache[localizedText.language] = localizedText.text;
                }
            }
        }
        
        /// <summary>
        /// 생성자 (에디터에서 새 항목 생성 시 사용)
        /// </summary>
        public LocalizedEntry()
        {
            key = "";
            description = "";
        }
        
        /// <summary>
        /// 생성자 (코드에서 직접 생성 시 사용)
        /// </summary>
        public LocalizedEntry(string key, string korean = "", string description = "")
        {
            this.key = key;
            this.description = description;
            
            this.texts = new LocalizedText[]
            {
                new LocalizedText { language = SupportedLanguage.Korean, text = korean },
                new LocalizedText { language = SupportedLanguage.English, text = "" },
                new LocalizedText { language = SupportedLanguage.Japanese, text = "" },
                new LocalizedText { language = SupportedLanguage.Chinese, text = "" }
            };
        }
        
        /// <summary>
        /// 키 유효성 검사
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(key))
                return false;
                
            // 최소한 한국어 텍스트는 있어야 함
            if (_textCache == null)
                InitializeCache();
                
            return _textCache.TryGetValue(SupportedLanguage.Korean, out string koreanText) && 
                   !string.IsNullOrEmpty(koreanText);
        }
        
        /// <summary>
        /// 특정 언어의 텍스트 설정 (에디터용)
        /// </summary>
        public void SetText(SupportedLanguage language, string text)
        {
            if (_textCache == null)
                InitializeCache();
                
            _textCache[language] = text;
            
            // 원본 배열도 업데이트
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].language == language)
                {
                    texts[i].text = text;
                    return;
                }
            }
            
            // 해당 언어가 없으면 추가
            var newTexts = new LocalizedText[texts.Length + 1];
            Array.Copy(texts, newTexts, texts.Length);
            newTexts[texts.Length] = new LocalizedText { language = language, text = text };
            texts = newTexts;
        }
    }
} 