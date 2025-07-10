using UnityEngine;
using System;

namespace DungeonMaster.Localization
{
    /// <summary>
    /// 지역화 텍스트들을 저장하는 ScriptableObject
    /// SOLID 원칙: 단일 책임 - 데이터 저장만 담당
    /// </summary>
    [CreateAssetMenu(fileName = "StringTable", menuName = "Localization/String Table", order = 1)]
    public class StringTable : ScriptableObject
    {
        [Header("테이블 정보")]
        [SerializeField] private string tableName = "Main";
        [SerializeField, TextArea(2, 4)] private string description = "메인 지역화 테이블";
        
        [Header("지역화 항목들")]
        [SerializeField] private LocalizedEntry[] entries = new LocalizedEntry[0];
        
        /// <summary>
        /// 테이블 이름
        /// </summary>
        public string TableName => tableName;
        
        /// <summary>
        /// 테이블 설명
        /// </summary>
        public string Description => description;
        
        /// <summary>
        /// 전체 항목 수
        /// </summary>
        public int EntryCount => entries?.Length ?? 0;
        
        /// <summary>
        /// 모든 항목들 (읽기 전용)
        /// </summary>
        public LocalizedEntry[] Entries => entries;
        
        /// <summary>
        /// 지정된 키와 언어에 해당하는 텍스트를 반환합니다.
        /// 캐시 없이 직접 검색 (성능이 중요한 경우 StringTableCache 사용)
        /// </summary>
        /// <param name="key">지역화 키</param>
        /// <param name="language">요청 언어</param>
        /// <returns>지역화된 텍스트 (키가 없으면 null 반환)</returns>
        public string GetText(string key, SupportedLanguage language)
        {
            if (string.IsNullOrEmpty(key) || entries == null)
                return null;
                
            foreach (var entry in entries)
            {
                if (entry != null && entry.Key == key)
                {
                    return entry.GetText(language);
                }
            }
            
            return null; // 키를 찾을 수 없음
        }
        
        /// <summary>
        /// 키가 존재하는지 확인
        /// </summary>
        public bool ContainsKey(string key)
        {
            if (string.IsNullOrEmpty(key) || entries == null)
                return false;
                
            foreach (var entry in entries)
            {
                if (entry != null && entry.Key == key)
                    return true;
            }
            
            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터 전용: 새 항목 추가
        /// </summary>
        public void AddEntry(LocalizedEntry newEntry)
        {
            if (newEntry == null || !newEntry.IsValid())
                return;
                
            if (ContainsKey(newEntry.Key))
            {
                UnityEngine.Debug.LogWarning($"[StringTable] Key '{newEntry.Key}' already exists in table '{tableName}'");
                return;
            }
            
            var newEntries = new LocalizedEntry[entries.Length + 1];
            Array.Copy(entries, newEntries, entries.Length);
            newEntries[entries.Length] = newEntry;
            entries = newEntries;
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        /// <summary>
        /// 에디터 전용: 항목 제거
        /// </summary>
        public bool RemoveEntry(string key)
        {
            if (string.IsNullOrEmpty(key) || entries == null)
                return false;
                
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i] != null && entries[i].Key == key)
                {
                    var newEntries = new LocalizedEntry[entries.Length - 1];
                    Array.Copy(entries, 0, newEntries, 0, i);
                    Array.Copy(entries, i + 1, newEntries, i, entries.Length - i - 1);
                    entries = newEntries;
                    
                    UnityEditor.EditorUtility.SetDirty(this);
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 에디터 전용: 항목 정렬
        /// </summary>
        [ContextMenu("Sort Entries by Key")]
        private void SortEntriesByKey()
        {
            if (entries != null && entries.Length > 1)
            {
                Array.Sort(entries, (a, b) => 
                {
                    if (a == null && b == null) return 0;
                    if (a == null) return 1;
                    if (b == null) return -1;
                    return string.Compare(a.Key, b.Key, StringComparison.Ordinal);
                });
                
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
} 