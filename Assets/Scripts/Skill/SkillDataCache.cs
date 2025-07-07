using System.Collections.Generic;
using UnityEngine;

namespace DungeonMaster.Skill
{
    /// <summary>
    /// SkillData 계산 결과를 캐싱하는 성능 최적화 시스템
    /// LRU (Least Recently Used) 캐시 전략 사용
    /// </summary>
    public static class SkillDataCache
    {
        // 캐시 설정
        private const int MAX_CACHE_SIZE = 50;  // 최대 캐시 항목 수
        private const int CLEANUP_TRIGGER = 40; // 정리 시작 임계값

        // 캐시 키 구조체
        private struct CacheKey
        {
            public int skillDataInstanceId;
            public int skillLevel;
            public string operation;

            public CacheKey(int instanceId, int level, string op)
            {
                skillDataInstanceId = instanceId;
                skillLevel = level;
                operation = op;
            }

            public override int GetHashCode()
            {
                return skillDataInstanceId ^ (skillLevel << 8) ^ operation.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is CacheKey other)
                {
                    return skillDataInstanceId == other.skillDataInstanceId &&
                           skillLevel == other.skillLevel &&
                           operation == other.operation;
                }
                return false;
            }
        }

        // 캐시 엔트리
        private class CacheEntry<T>
        {
            public T value;
            public float lastAccessTime;

            public CacheEntry(T val)
            {
                value = val;
                lastAccessTime = Time.time;
            }

            public void UpdateAccessTime()
            {
                lastAccessTime = Time.time;
            }
        }

        // 각 타입별 캐시
        private static readonly Dictionary<CacheKey, CacheEntry<SkillEffectData[]>> _scaledEffectsCache = 
            new Dictionary<CacheKey, CacheEntry<SkillEffectData[]>>();
        private static readonly Dictionary<CacheKey, CacheEntry<string>> _descriptionCache = 
            new Dictionary<CacheKey, CacheEntry<string>>();
        private static readonly Dictionary<CacheKey, CacheEntry<float>> _scaledValueCache = 
            new Dictionary<CacheKey, CacheEntry<float>>();

        /// <summary>
        /// GetScaledEffects 결과 캐싱
        /// </summary>
        public static SkillEffectData[] GetOrComputeScaledEffects(SkillData skillData, int skillLevel, 
            System.Func<SkillEffectData[]> computeFunc)
        {
            var key = new CacheKey(skillData.GetInstanceID(), skillLevel, "ScaledEffects");
            
            if (_scaledEffectsCache.TryGetValue(key, out var entry))
            {
                entry.UpdateAccessTime();
                return entry.value;
            }

            // 캐시 정리 (필요시)
            CleanupCache(_scaledEffectsCache);

            // 새로운 값 계산 및 캐싱
            var result = computeFunc();
            _scaledEffectsCache[key] = new CacheEntry<SkillEffectData[]>(result);
            
            return result;
        }

        /// <summary>
        /// 설명 문자열 캐싱
        /// </summary>
        public static string GetOrComputeDescription(SkillData skillData, int skillLevel, string operation,
            System.Func<string> computeFunc)
        {
            var key = new CacheKey(skillData.GetInstanceID(), skillLevel, operation);
            
            if (_descriptionCache.TryGetValue(key, out var entry))
            {
                entry.UpdateAccessTime();
                return entry.value;
            }

            // 캐시 정리 (필요시)
            CleanupCache(_descriptionCache);

            // 새로운 값 계산 및 캐싱
            var result = computeFunc();
            _descriptionCache[key] = new CacheEntry<string>(result);
            
            return result;
        }

        /// <summary>
        /// 스케일링된 값 캐싱 (쿨다운, 마나 등)
        /// </summary>
        public static float GetOrComputeScaledValue(SkillData skillData, int skillLevel, string operation,
            System.Func<float> computeFunc)
        {
            var key = new CacheKey(skillData.GetInstanceID(), skillLevel, operation);
            
            if (_scaledValueCache.TryGetValue(key, out var entry))
            {
                entry.UpdateAccessTime();
                return entry.value;
            }

            // 캐시 정리 (필요시)
            CleanupCache(_scaledValueCache);

            // 새로운 값 계산 및 캐싱
            var result = computeFunc();
            _scaledValueCache[key] = new CacheEntry<float>(result);
            
            return result;
        }

        /// <summary>
        /// LRU 방식으로 캐시 정리
        /// </summary>
        private static void CleanupCache<T>(Dictionary<CacheKey, CacheEntry<T>> cache)
        {
            if (cache.Count < CLEANUP_TRIGGER)
                return;

            // 가장 오래된 항목들을 찾아서 제거
            var itemsToRemove = new List<CacheKey>();
            float currentTime = Time.time;
            float oldestTimeThreshold = currentTime - 60f; // 60초 이상 된 항목 제거

            foreach (var kvp in cache)
            {
                if (kvp.Value.lastAccessTime < oldestTimeThreshold)
                {
                    itemsToRemove.Add(kvp.Key);
                }
            }

            // 시간 기준으로 충분히 제거되지 않으면, 가장 오래된 항목들 추가 제거
            if (itemsToRemove.Count < 10 && cache.Count > MAX_CACHE_SIZE)
            {
                var sortedEntries = new List<KeyValuePair<CacheKey, CacheEntry<T>>>(cache);
                sortedEntries.Sort((a, b) => a.Value.lastAccessTime.CompareTo(b.Value.lastAccessTime));
                
                int additionalRemoveCount = Mathf.Min(10 - itemsToRemove.Count, sortedEntries.Count / 2);
                for (int i = 0; i < additionalRemoveCount; i++)
                {
                    itemsToRemove.Add(sortedEntries[i].Key);
                }
            }

            // 실제 제거
            foreach (var key in itemsToRemove)
            {
                cache.Remove(key);
            }

            if (itemsToRemove.Count > 0)
            {
                Debug.Log($"SkillDataCache: {itemsToRemove.Count}개 항목 정리. 남은 항목: {cache.Count}");
            }
        }

        /// <summary>
        /// 전체 캐시 초기화
        /// </summary>
        public static void ClearAllCaches()
        {
            _scaledEffectsCache.Clear();
            _descriptionCache.Clear();
            _scaledValueCache.Clear();
            Debug.Log("SkillDataCache: 모든 캐시가 초기화되었습니다.");
        }

        /// <summary>
        /// 특정 스킬의 캐시만 제거 (스킬 데이터 변경 시 사용)
        /// </summary>
        public static void InvalidateSkillCache(SkillData skillData)
        {
            int instanceId = skillData.GetInstanceID();
            var keysToRemove = new List<CacheKey>();

            // 모든 캐시에서 해당 스킬 관련 항목 찾기
            foreach (var key in _scaledEffectsCache.Keys)
            {
                if (key.skillDataInstanceId == instanceId)
                    keysToRemove.Add(key);
            }

            foreach (var key in _descriptionCache.Keys)
            {
                if (key.skillDataInstanceId == instanceId)
                    keysToRemove.Add(key);
            }

            foreach (var key in _scaledValueCache.Keys)
            {
                if (key.skillDataInstanceId == instanceId)
                    keysToRemove.Add(key);
            }

            // 제거 실행
            foreach (var key in keysToRemove)
            {
                _scaledEffectsCache.Remove(key);
                _descriptionCache.Remove(key);
                _scaledValueCache.Remove(key);
            }

            Debug.Log($"SkillDataCache: {skillData.SkillName} 스킬의 캐시 {keysToRemove.Count}개 항목이 무효화되었습니다.");
        }

        /// <summary>
        /// 캐시 통계 정보
        /// </summary>
        public static string GetCacheStats()
        {
            return $"SkillDataCache 통계:\n" +
                   $"- ScaledEffects: {_scaledEffectsCache.Count}개\n" +
                   $"- Descriptions: {_descriptionCache.Count}개\n" +
                   $"- ScaledValues: {_scaledValueCache.Count}개\n" +
                   $"- 총 캐시 항목: {_scaledEffectsCache.Count + _descriptionCache.Count + _scaledValueCache.Count}개";
        }
    }
} 