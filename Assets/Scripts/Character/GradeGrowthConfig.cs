using System;
using System.Collections.Generic;
using DungeonMaster.Localization;
using DungeonMaster.Utility;
using UnityEngine;

namespace DungeonMaster.Character
{
    /// <summary>
    /// 등급(Grade)에 따른 스탯 성장치의 최소/최대 범위를 정의하는 ScriptableObject.
    /// CharacterDataFactory에서 이 데이터를 참조하여 레벨업 시 스탯 증가량을 계산합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "GradeGrowthConfig", menuName = "DungeonMaster/Character/Grade Growth Config")]
    public class GradeGrowthConfig : ScriptableObject
    {
        [Tooltip("몬스터의 등급(C, UC, R...)에 따른 기본 성장치 범위를 정의합니다.")]
        [SerializeField]
        private GradeGrowthData[] gradeGrowthTable;

        private Dictionary<Grade, GradeGrowthData> _cache;

        private void OnEnable()
        {
            _cache = new Dictionary<Grade, GradeGrowthData>();
            if (gradeGrowthTable == null) return;
            
            foreach (var data in gradeGrowthTable)
            {
                _cache[data.grade] = data;
            }
        }

        /// <summary>
        /// 특정 등급에 해당하는 성장치 데이터를 반환합니다.
        /// </summary>
        /// <param name="grade">찾고자 하는 등급</param>
        /// <returns>해당 등급의 성장 데이터. 없으면 null을 반환합니다.</returns>
        public GradeGrowthData GetGradeGrowthData(Grade grade)
        {
            if (_cache == null) OnEnable();
            
            if (_cache.TryGetValue(grade, out var data))
            {
                return data;
            }
            
            GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("error_growth_data_not_found", grade));
            return null;
        }
    }

    /// <summary>
    /// 등급별 성장 데이터를 담는 클래스.
    /// </summary>
    [Serializable]
    public class GradeGrowthData
    {
        public Grade grade;
        
        [Tooltip("이 등급의 카드가 가질 수 있는 '고유 성장률'의 최소값 (단위: %). 예: 81 -> 81%")]
        public int MinRate;
        
        [Tooltip("이 등급의 카드가 가질 수 있는 '고유 성장률'의 최대값 (단위: %). 예: 110 -> 110%")]
        public int MaxRate;
        
        [Tooltip("이 등급의 성장률을 강화할 때의 성공 확률 (사용처 미정, 확장성을 위해 유지)")]
        public float Probability;
    }
} 