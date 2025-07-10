using UnityEngine;
using System.Collections.Generic;
using System;

namespace DungeonMaster.Localization
{
    /// <summary>
    /// Unity용 지역화 매니저 (얇은 래퍼)
    /// SOLID 원칙: LocalizationCore를 컴포지션으로 사용하여 Unity 바인딩만 담당
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        #region Singleton
        private static LocalizationManager _instance;
        
        /// <summary>
        /// LocalizationManager 인스턴스 (지연 생성)
        /// </summary>
        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 자동 생성 (씬에서 찾기 → 없으면 새로 생성)
                    _instance = FindAnyObjectByType<LocalizationManager>();
                    
                    if (_instance == null)
                    {
                        var go = new GameObject("LocalizationManager");
                        _instance = go.AddComponent<LocalizationManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 매니저가 이미 존재하는지 확인 (성능 최적화용)
        /// </summary>
        public static bool Exists => _instance != null;
        #endregion

        #region Inspector Fields
        [Header("필수 설정")]
        [SerializeField] private LanguageSettings languageSettings;
        
        [Header("StringTable 목록")]
        [SerializeField] private StringTable mainStringTable;
        [SerializeField] private StringTable[] additionalTables = new StringTable[0];
        
        [Header("런타임 설정")]
        [SerializeField] private bool useSystemLanguageOnStart = true;
        [SerializeField] private bool enableCaching = true;
        [SerializeField] private bool enableMissingKeyLogging = true;
        #endregion

        #region Private Fields
        private LocalizationCore _core;
        private ILocalizationLogger _logger;
        private bool _isInitialized;
        #endregion

        #region Events
        /// <summary>
        /// 언어 변경 이벤트 (Unity 이벤트 시스템용)
        /// </summary>
        public static event Action<SupportedLanguage> OnLanguageChanged;
        #endregion

        #region Properties
        /// <summary>
        /// 현재 언어
        /// </summary>
        public SupportedLanguage CurrentLanguage => _core?.CurrentLanguage ?? SupportedLanguage.Korean;

        /// <summary>
        /// 초기화 완료 여부
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 통계 정보
        /// </summary>
        public LocalizationStats Stats => _core?.GetStats() ?? new LocalizationStats();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // 싱글톤 처리
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManager();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // 시스템 언어 적용 (설정된 경우)
            if (useSystemLanguageOnStart && _isInitialized)
            {
                ChangeToSystemLanguage();
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                // 이벤트 정리
                if (_core != null)
                {
                    _core.OnLanguageChanged -= HandleLanguageChanged;
                }
                
                _instance = null;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// 매니저 초기화
        /// </summary>
        private void InitializeManager()
        {
            try
            {
                // 설정 검증
                if (languageSettings == null)
                {
                    Debug.LogError("[LocalizationManager] LanguageSettings is required! Please assign it in the inspector.");
                    return;
                }

                // 로거 생성
                _logger = new UnityLocalizationLogger();

                // 코어 엔진 생성
                _core = new LocalizationCore(languageSettings, _logger);
                
                // 이벤트 구독
                _core.OnLanguageChanged += HandleLanguageChanged;

                // 설정 적용
                _core.SetCachingEnabled(enableCaching);
                _core.SetMissingKeyLoggingEnabled(enableMissingKeyLogging);

                // StringTable들 로드
                LoadStringTables();

                _isInitialized = true;
                _logger.LogInfo("LocalizationManager initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalizationManager] Initialization failed: {ex.Message}");
                _isInitialized = false;
            }
        }

        /// <summary>
        /// StringTable들 로드
        /// </summary>
        private void LoadStringTables()
        {
            var tablesToLoad = new List<StringTable>();

            // 메인 테이블 추가
            if (mainStringTable != null)
            {
                tablesToLoad.Add(mainStringTable);
            }

            // 추가 테이블들 추가
            if (additionalTables != null)
            {
                foreach (var table in additionalTables)
                {
                    if (table != null)
                    {
                        tablesToLoad.Add(table);
                    }
                }
            }

            // 코어에 추가
            _core.AddStringTables(tablesToLoad);
        }
        #endregion

        #region Public API
        /// <summary>
        /// 지역화된 텍스트 반환
        /// </summary>
        /// <param name="key">지역화 키</param>
        /// <returns>지역화된 텍스트</returns>
        public string GetText(string key)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[LocalizationManager] Not initialized yet!");
                return $"[NOT_INITIALIZED: {key}]";
            }

            return _core.GetText(key);
        }

        /// <summary>
        /// 포맷된 지역화 텍스트 반환
        /// </summary>
        /// <param name="key">지역화 키</param>
        /// <param name="args">포맷 매개변수들</param>
        /// <returns>포맷된 지역화 텍스트</returns>
        public string GetTextFormatted(string key, params object[] args)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[LocalizationManager] Not initialized yet!");
                return $"[NOT_INITIALIZED: {key}]";
            }

            return _core.GetTextFormatted(key, args);
        }

        /// <summary>
        /// 언어 변경
        /// </summary>
        /// <param name="newLanguage">새로운 언어</param>
        public void ChangeLanguage(SupportedLanguage newLanguage)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[LocalizationManager] Not initialized yet!");
                return;
            }

            _core.ChangeLanguage(newLanguage);
        }

        /// <summary>
        /// 시스템 언어로 변경
        /// </summary>
        public void ChangeToSystemLanguage()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[LocalizationManager] Not initialized yet!");
                return;
            }

            _core.ChangeToSystemLanguage();
        }

        /// <summary>
        /// 키 존재 여부 확인
        /// </summary>
        /// <param name="key">확인할 키</param>
        /// <returns>키 존재 여부</returns>
        public bool ContainsKey(string key)
        {
            if (!_isInitialized)
                return false;

            return _core.ContainsKey(key);
        }

        /// <summary>
        /// StringTable 런타임 추가
        /// </summary>
        /// <param name="stringTable">추가할 StringTable</param>
        public void AddStringTable(StringTable stringTable)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[LocalizationManager] Not initialized yet!");
                return;
            }

            _core.AddStringTable(stringTable);
        }

        /// <summary>
        /// 캐시 설정 변경
        /// </summary>
        /// <param name="enabled">캐시 활성화 여부</param>
        public void SetCachingEnabled(bool enabled)
        {
            enableCaching = enabled;
            
            if (_isInitialized)
            {
                _core.SetCachingEnabled(enabled);
            }
        }

        /// <summary>
        /// 누락 키 로깅 설정 변경
        /// </summary>
        /// <param name="enabled">로깅 활성화 여부</param>
        public void SetMissingKeyLoggingEnabled(bool enabled)
        {
            enableMissingKeyLogging = enabled;
            
            if (_isInitialized)
            {
                _core.SetMissingKeyLoggingEnabled(enabled);
            }
        }

        /// <summary>
        /// 모든 캐시 갱신
        /// </summary>
        public void RefreshAllCaches()
        {
            if (_isInitialized)
            {
                _core.RefreshAllCaches();
            }
        }

        /// <summary>
        /// 누락된 키 목록 클리어
        /// </summary>
        public void ClearMissingKeys()
        {
            if (_isInitialized)
            {
                _core.ClearMissingKeys();
            }
        }
        #endregion

        #region Event Handling
        /// <summary>
        /// 코어 엔진의 언어 변경 이벤트 처리
        /// </summary>
        private void HandleLanguageChanged(SupportedLanguage newLanguage)
        {
            // Unity 이벤트 발생
            OnLanguageChanged?.Invoke(newLanguage);
        }
        #endregion

        #region Debug Methods
        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        [ContextMenu("Print Debug Info")]
        public void PrintDebugInfo()
        {
            if (!_isInitialized)
            {
                Debug.Log("[LocalizationManager] Not initialized");
                return;
            }

            var stats = _core.GetStats();
            Debug.Log("=== LocalizationManager Debug Info ===");
            Debug.Log(stats.GetSummary());
        }

        /// <summary>
        /// 모든 테이블 유효성 검사
        /// </summary>
        [ContextMenu("Validate All Tables")]
        public void ValidateAllTables()
        {
            if (!_isInitialized)
            {
                Debug.Log("[LocalizationManager] Not initialized");
                return;
            }

            var tables = new List<StringTable>();
            if (mainStringTable != null) tables.Add(mainStringTable);
            if (additionalTables != null) tables.AddRange(additionalTables);

            foreach (var table in tables)
            {
                if (table != null)
                {
                    var result = StringTableValidator.Validate(table);
                    Debug.Log($"[{table.TableName}] {result.GetSummary()}");
                }
            }
        }
        #endregion
    }
} 