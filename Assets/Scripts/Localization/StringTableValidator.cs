using System.Collections.Generic;
using System.Linq;

namespace DungeonMaster.Localization
{
    /// <summary>
    /// StringTable 검증 결과
    /// </summary>
    public struct ValidationResult
    {
        public bool IsValid;
        public string[] InvalidKeys;
        public string[] DuplicateKeys;
        public string[] MissingKoreanKeys;
        public int TotalEntries;
        public int ValidEntries;
        
        /// <summary>
        /// 검증 요약 메시지
        /// </summary>
        public string GetSummary()
        {
            if (IsValid)
                return $"StringTable 검증 성공! 총 {TotalEntries}개 항목이 모두 유효합니다.";
                
            var issues = new List<string>();
            
            if (InvalidKeys.Length > 0)
                issues.Add($"유효하지 않은 키 {InvalidKeys.Length}개");
                
            if (DuplicateKeys.Length > 0)
                issues.Add($"중복 키 {DuplicateKeys.Length}개");
                
            if (MissingKoreanKeys.Length > 0)
                issues.Add($"한국어 누락 키 {MissingKoreanKeys.Length}개");
                
            return $"StringTable 검증 실패: {string.Join(", ", issues)}";
        }
    }

    /// <summary>
    /// StringTable 검증을 담당하는 순수 C# 클래스
    /// SOLID 원칙: 단일 책임 - 검증 로직만 담당
    /// Unity 의존성 없음 - 서버에서도 사용 가능
    /// </summary>
    public static class StringTableValidator
    {
        /// <summary>
        /// StringTable 전체 검증
        /// </summary>
        public static ValidationResult Validate(StringTable stringTable)
        {
            if (stringTable == null || stringTable.Entries == null)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    InvalidKeys = new string[0],
                    DuplicateKeys = new string[0],
                    MissingKoreanKeys = new string[0],
                    TotalEntries = 0,
                    ValidEntries = 0
                };
            }

            var entries = stringTable.Entries;
            var validEntries = new List<LocalizedEntry>();
            var invalidKeys = new List<string>();
            var missingKoreanKeys = new List<string>();

            // 유효하지 않은 항목들 찾기
            foreach (var entry in entries)
            {
                if (entry == null)
                {
                    invalidKeys.Add("[NULL_ENTRY]");
                    continue;
                }

                if (string.IsNullOrEmpty(entry.Key))
                {
                    invalidKeys.Add("[EMPTY_KEY]");
                    continue;
                }

                if (!entry.IsValid())
                {
                    invalidKeys.Add(entry.Key);
                    continue;
                }

                // 한국어 텍스트 확인
                string koreanText = entry.GetText(SupportedLanguage.Korean);
                if (string.IsNullOrEmpty(koreanText))
                {
                    missingKoreanKeys.Add(entry.Key);
                }

                validEntries.Add(entry);
            }

            // 중복 키 찾기
            var duplicateKeys = validEntries
                .GroupBy(e => e.Key)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToArray();

            // 결과 생성
            bool isValid = invalidKeys.Count == 0 && 
                          duplicateKeys.Length == 0 && 
                          missingKoreanKeys.Count == 0;

            return new ValidationResult
            {
                IsValid = isValid,
                InvalidKeys = invalidKeys.ToArray(),
                DuplicateKeys = duplicateKeys,
                MissingKoreanKeys = missingKoreanKeys.ToArray(),
                TotalEntries = entries.Length,
                ValidEntries = validEntries.Count
            };
        }

        /// <summary>
        /// 특정 키의 유효성 검사
        /// </summary>
        public static bool IsValidKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            // 키 네이밍 규칙: 영문자, 숫자, 언더스코어만 허용
            foreach (char c in key)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 키 네이밍 규칙 검증
        /// </summary>
        public static string[] ValidateKeyNaming(StringTable stringTable)
        {
            if (stringTable?.Entries == null)
                return new string[0];

            var invalidKeys = new List<string>();

            foreach (var entry in stringTable.Entries)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.Key))
                {
                    if (!IsValidKey(entry.Key))
                    {
                        invalidKeys.Add(entry.Key);
                    }
                }
            }

            return invalidKeys.ToArray();
        }

        /// <summary>
        /// 번역 완성도 검사
        /// </summary>
        public static Dictionary<SupportedLanguage, float> GetTranslationCompleteness(StringTable stringTable)
        {
            var completeness = new Dictionary<SupportedLanguage, float>();

            if (stringTable?.Entries == null || stringTable.Entries.Length == 0)
            {
                foreach (SupportedLanguage language in System.Enum.GetValues(typeof(SupportedLanguage)))
                {
                    completeness[language] = 0f;
                }
                return completeness;
            }

            var validEntries = stringTable.Entries.Where(e => e != null && e.IsValid()).ToArray();
            if (validEntries.Length == 0)
            {
                foreach (SupportedLanguage language in System.Enum.GetValues(typeof(SupportedLanguage)))
                {
                    completeness[language] = 0f;
                }
                return completeness;
            }

            foreach (SupportedLanguage language in System.Enum.GetValues(typeof(SupportedLanguage)))
            {
                int translatedCount = 0;

                foreach (var entry in validEntries)
                {
                    string text = entry.GetText(language);
                    if (!string.IsNullOrEmpty(text))
                    {
                        translatedCount++;
                    }
                }

                completeness[language] = (float)translatedCount / validEntries.Length;
            }

            return completeness;
        }
    }
} 