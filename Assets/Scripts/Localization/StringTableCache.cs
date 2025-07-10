using System.Collections.Generic;

namespace DungeonMaster.Localization
{
    /// <summary>
    /// StringTable의 성능 향상을 위한 캐시 시스템
    /// SOLID 원칙: 단일 책임 - 캐시 관리만 담당
    /// 개방/폐쇄 원칙: StringTable을 수정하지 않고 성능 확장
    /// </summary>
    public class StringTableCache
    {
        private readonly StringTable _stringTable;
        private readonly Dictionary<string, Dictionary<SupportedLanguage, string>> _cache;
        private bool _isInitialized;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="stringTable">캐시할 StringTable</param>
        public StringTableCache(StringTable stringTable)
        {
            _stringTable = stringTable;
            _cache = new Dictionary<string, Dictionary<SupportedLanguage, string>>();
            _isInitialized = false;
        }

        /// <summary>
        /// 캐시 초기화 (지연 초기화)
        /// </summary>
        private void EnsureInitialized()
        {
            if (_isInitialized || _stringTable == null)
                return;

            _cache.Clear();

            if (_stringTable.Entries != null)
            {
                foreach (var entry in _stringTable.Entries)
                {
                    if (entry != null && entry.IsValid())
                    {
                        var languageCache = new Dictionary<SupportedLanguage, string>();
                        
                        // 모든 지원 언어에 대해 캐시 생성
                        foreach (SupportedLanguage language in System.Enum.GetValues(typeof(SupportedLanguage)))
                        {
                            string text = entry.GetText(language);
                            if (!string.IsNullOrEmpty(text))
                            {
                                languageCache[language] = text;
                            }
                        }
                        
                        if (languageCache.Count > 0)
                        {
                            _cache[entry.Key] = languageCache;
                        }
                    }
                }
            }

            _isInitialized = true;
        }

        /// <summary>
        /// 캐시된 텍스트 반환 (고성능)
        /// </summary>
        /// <param name="key">지역화 키</param>
        /// <param name="language">요청 언어</param>
        /// <returns>지역화된 텍스트 (키가 없으면 null 반환)</returns>
        public string GetText(string key, SupportedLanguage language)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            EnsureInitialized();

            if (_cache.TryGetValue(key, out var languageCache))
            {
                // 요청된 언어 반환
                if (languageCache.TryGetValue(language, out string text))
                    return text;
                    
                // 기본 언어(한국어) 폴백
                if (languageCache.TryGetValue(SupportedLanguage.Korean, out string koreanText))
                    return koreanText;
            }

            return null;
        }

        /// <summary>
        /// 키 존재 여부 확인 (고성능)
        /// </summary>
        public bool ContainsKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            EnsureInitialized();
            return _cache.ContainsKey(key);
        }

        /// <summary>
        /// 캐시 강제 갱신
        /// </summary>
        public void RefreshCache()
        {
            _isInitialized = false;
            EnsureInitialized();
        }

        /// <summary>
        /// 캐시 통계 정보
        /// </summary>
        public CacheStats GetStats()
        {
            EnsureInitialized();
            return new CacheStats
            {
                CachedKeyCount = _cache.Count,
                TotalCachedTexts = GetTotalCachedTexts(),
                IsInitialized = _isInitialized
            };
        }

        /// <summary>
        /// 전체 캐시된 텍스트 개수 계산
        /// </summary>
        private int GetTotalCachedTexts()
        {
            int total = 0;
            foreach (var languageCache in _cache.Values)
            {
                total += languageCache.Count;
            }
            return total;
        }

        /// <summary>
        /// 캐시 통계 정보 구조체
        /// </summary>
        public struct CacheStats
        {
            public int CachedKeyCount;
            public int TotalCachedTexts;
            public bool IsInitialized;
        }
    }
} 