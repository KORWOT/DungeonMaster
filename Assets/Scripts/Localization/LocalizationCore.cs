using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonMaster.Localization
{
    /// <summary>
    /// 순수 C# 지역화 코어 엔진
    /// SOLID 원칙: 단일 책임 - 텍스트 검색과 언어 관리만 담당
    /// Unity 의존성 없음 - 서버에서도 사용 가능
    /// </summary>
    public class LocalizationCore
    {
        private readonly List<StringTable> _stringTables;
        private readonly Dictionary<StringTable, StringTableCache> _caches;
        private readonly ILocalizationLogger _logger;
        private readonly LanguageSettings _languageSettings;
        private readonly HashSet<string> _missingKeys;

        private SupportedLanguage _currentLanguage;
        private bool _enableCaching;
        private bool _enableMissingKeyLogging;

        /// <summary>
        /// 언어 변경 이벤트
        /// </summary>
        public event Action<SupportedLanguage> OnLanguageChanged;

        /// <summary>
        /// 현재 언어
        /// </summary>
        public SupportedLanguage CurrentLanguage => _currentLanguage;

        /// <summary>
        /// 로드된 테이블 수
        /// </summary>
        public int LoadedTableCount => _stringTables.Count;

        /// <summary>
        /// 누락된 키 수
        /// </summary>
        public int MissingKeyCount => _missingKeys.Count;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="languageSettings">언어 설정</param>
        /// <param name="logger">로거 (nullable)</param>
        public LocalizationCore(LanguageSettings languageSettings, ILocalizationLogger logger = null)
        {
            _languageSettings = languageSettings ?? throw new ArgumentNullException(nameof(languageSettings));
            _logger = logger ?? new SilentLocalizationLogger();
            
            _stringTables = new List<StringTable>();
            _caches = new Dictionary<StringTable, StringTableCache>();
            _missingKeys = new HashSet<string>();
            
            _currentLanguage = languageSettings.DefaultLanguage;
            _enableCaching = true;
            _enableMissingKeyLogging = true;

            _logger.LogInfo($"LocalizationCore initialized with language: {GetCurrentLanguageDisplayName()}");
        }

        /// <summary>
        /// StringTable 추가
        /// </summary>
        public void AddStringTable(StringTable stringTable)
        {
            if (stringTable == null)
            {
                _logger.LogWarning("Attempted to add null StringTable");
                return;
            }

            if (_stringTables.Contains(stringTable))
            {
                _logger.LogWarning($"StringTable '{stringTable.TableName}' is already added");
                return;
            }

            _stringTables.Add(stringTable);

            // 캐시 생성 (필요한 경우)
            if (_enableCaching)
            {
                _caches[stringTable] = new StringTableCache(stringTable);
            }

            _logger.LogInfo($"Added StringTable: '{stringTable.TableName}' ({stringTable.EntryCount} entries)");
        }

        /// <summary>
        /// 여러 StringTable 한번에 추가
        /// </summary>
        public void AddStringTables(IEnumerable<StringTable> stringTables)
        {
            if (stringTables == null) return;

            foreach (var table in stringTables)
            {
                AddStringTable(table);
            }
        }

        /// <summary>
        /// 지역화된 텍스트 반환
        /// </summary>
        public string GetText(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "[EMPTY_KEY]";

            // 캐시된 테이블들에서 검색 (성능 최적화)
            if (_enableCaching)
            {
                foreach (var kvp in _caches)
                {
                    string cachedResult = kvp.Value.GetText(key, _currentLanguage);
                    if (cachedResult != null)
                        return cachedResult;
                }
            }

            // 일반 테이블들에서 검색
            foreach (var table in _stringTables)
            {
                if (table != null && table.ContainsKey(key))
                {
                    return table.GetText(key, _currentLanguage);
                }
            }

            // 키를 찾지 못한 경우
            HandleMissingKey(key);
            return $"[MISSING: {key}]";
        }

        /// <summary>
        /// 포맷된 지역화 텍스트 반환
        /// </summary>
        public string GetTextFormatted(string key, params object[] args)
        {
            string text = GetText(key);

            if (args == null || args.Length == 0)
                return text;

            try
            {
                return string.Format(text, args);
            }
            catch (FormatException ex)
            {
                _logger.LogError($"Format error for key '{key}': {ex.Message}");
                return text; // 포맷 실패 시 원본 반환
            }
        }

        /// <summary>
        /// 언어 변경
        /// </summary>
        public void ChangeLanguage(SupportedLanguage newLanguage)
        {
            if (!LanguageUtils.IsValidLanguage(newLanguage))
            {
                _logger.LogWarning($"Invalid language: {newLanguage}");
                return;
            }

            if (_currentLanguage == newLanguage)
                return;

            var oldLanguage = _currentLanguage;
            _currentLanguage = newLanguage;

            // 캐시 갱신 (언어 변경시)
            if (_enableCaching)
            {
                RefreshAllCaches();
            }

            _logger.LogInfo($"Language changed: {GetLanguageDisplayName(oldLanguage)} → {GetCurrentLanguageDisplayName()}");

            // 이벤트 발생
            OnLanguageChanged?.Invoke(newLanguage);
        }

        /// <summary>
        /// 시스템 언어로 변경 (Unity에서만 사용)
        /// </summary>
        public void ChangeToSystemLanguage()
        {
            if (_languageSettings == null)
            {
                _logger.LogWarning("LanguageSettings is null, cannot detect system language");
                return;
            }

            try
            {
                var systemLanguage = SystemLanguageDetector.DetectSystemLanguage(_languageSettings);
                ChangeLanguage(systemLanguage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to detect system language: {ex.Message}");
            }
        }

        /// <summary>
        /// 키 존재 여부 확인
        /// </summary>
        public bool ContainsKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            foreach (var table in _stringTables)
            {
                if (table != null && table.ContainsKey(key))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 캐시 활성화/비활성화
        /// </summary>
        public void SetCachingEnabled(bool enabled)
        {
            if (_enableCaching == enabled)
                return;

            _enableCaching = enabled;

            if (enabled)
            {
                // 캐시 생성
                foreach (var table in _stringTables)
                {
                    if (!_caches.ContainsKey(table))
                    {
                        _caches[table] = new StringTableCache(table);
                    }
                }
                _logger.LogInfo("Caching enabled");
            }
            else
            {
                // 캐시 제거
                _caches.Clear();
                _logger.LogInfo("Caching disabled");
            }
        }

        /// <summary>
        /// 누락 키 로깅 활성화/비활성화
        /// </summary>
        public void SetMissingKeyLoggingEnabled(bool enabled)
        {
            _enableMissingKeyLogging = enabled;
            _logger.LogInfo($"Missing key logging: {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 모든 캐시 갱신
        /// </summary>
        public void RefreshAllCaches()
        {
            foreach (var cache in _caches.Values)
            {
                cache.RefreshCache();
            }
            _logger.LogInfo("All caches refreshed");
        }

        /// <summary>
        /// 누락된 키 목록 클리어
        /// </summary>
        public void ClearMissingKeys()
        {
            _missingKeys.Clear();
            _logger.LogInfo("Missing keys cleared");
        }

        /// <summary>
        /// 통계 정보 반환
        /// </summary>
        public LocalizationStats GetStats()
        {
            int totalEntries = _stringTables.Sum(t => t?.EntryCount ?? 0);
            int cachedTables = _caches.Count;

            return new LocalizationStats
            {
                CurrentLanguage = _currentLanguage,
                LoadedTableCount = _stringTables.Count,
                TotalEntries = totalEntries,
                CachedTableCount = cachedTables,
                MissingKeyCount = _missingKeys.Count,
                IsCachingEnabled = _enableCaching,
                IsMissingKeyLoggingEnabled = _enableMissingKeyLogging
            };
        }

        /// <summary>
        /// 누락된 키 처리
        /// </summary>
        private void HandleMissingKey(string key)
        {
            if (_enableMissingKeyLogging && !_missingKeys.Contains(key))
            {
                _missingKeys.Add(key);
                _logger.LogWarning($"Missing key: '{key}' for language: {GetCurrentLanguageDisplayName()}");
            }
        }

        /// <summary>
        /// 현재 언어의 표시명 반환
        /// </summary>
        private string GetCurrentLanguageDisplayName()
        {
            return GetLanguageDisplayName(_currentLanguage);
        }

        /// <summary>
        /// 언어의 표시명 반환
        /// </summary>
        private string GetLanguageDisplayName(SupportedLanguage language)
        {
            return LanguageUtils.GetDisplayName(language, _languageSettings);
        }
    }

    /// <summary>
    /// 지역화 시스템 통계 정보
    /// </summary>
    public struct LocalizationStats
    {
        public SupportedLanguage CurrentLanguage;
        public int LoadedTableCount;
        public int TotalEntries;
        public int CachedTableCount;
        public int MissingKeyCount;
        public bool IsCachingEnabled;
        public bool IsMissingKeyLoggingEnabled;

        /// <summary>
        /// 통계 요약 문자열
        /// </summary>
        public string GetSummary()
        {
            return $"Language: {CurrentLanguage}, Tables: {LoadedTableCount}, " +
                   $"Entries: {TotalEntries}, Missing: {MissingKeyCount}, " +
                   $"Cache: {(IsCachingEnabled ? "ON" : "OFF")}";
        }
    }
} 