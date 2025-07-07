using System.Collections.Generic;
using DungeonMaster.Character;

namespace DungeonMaster.Utility
{
    /// <summary>
    /// Dictionary 확장 메서드
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Dictionary에서 키에 해당하는 값을 가져오거나, 키가 없으면 기본값을 반환합니다.
        /// </summary>
        /// <typeparam name="TKey">키 타입</typeparam>
        /// <typeparam name="TValue">값 타입</typeparam>
        /// <param name="dictionary">대상 딕셔너리</param>
        /// <param name="key">찾을 키</param>
        /// <param name="defaultValue">키가 없을 때 반환할 기본값</param>
        /// <returns>키에 해당하는 값 또는 기본값</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            if (dictionary == null) return defaultValue;
            
            return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
        }

        /// <summary>
        /// Dictionary에 값을 추가하거나, 키가 이미 존재하면 기존 값에 더합니다.
        /// 이 메서드는 제네릭하며 모든 키 타입에 대해 효율적으로 작동합니다.
        /// </summary>
        /// <typeparam name="TKey">키 타입</typeparam>
        /// <param name="dictionary">대상 딕셔너리</param>
        /// <param name="key">키</param>
        /// <param name="value">추가할 값</param>
        public static void AddValue<TKey>(this Dictionary<TKey, long> dictionary, TKey key, long value)
        {
            if (dictionary == null) return;

            if (dictionary.TryGetValue(key, out long currentValue))
            {
                dictionary[key] = currentValue + value;
            }
            else
            {
                dictionary[key] = value;
            }
        }
    }
} 