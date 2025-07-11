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

        /// <summary>
        /// 특정 등급의 기본 스탯 성장치 맵을 반환합니다.
        /// </summary>
        public Dictionary<StatType, int> GetBaseGrowthForGrade(Grade grade, StatType statType)
        {
            var data = GetGradeGrowthData(grade);
            if (data != null)
            {
                return data.BaseGrowth;
            }
            return new Dictionary<StatType, int>();
        }
    }

    /// <summary>
    /// 등급별 성장 데이터를 담는 클래스.
    /// ISerializationCallbackReceiver를 구현하여 딕셔너리를 직렬화합니다.
    /// </summary>
    [Serializable]
    public class GradeGrowthData : ISerializationCallbackReceiver
    {
        public Grade grade;
        
        [Tooltip("이 등급의 기본 스탯별 성장치. 레벨업 시 이 값이 기본으로 적용됩니다.")]
        [HideInInspector]
        public Dictionary<StatType, int> BaseGrowth = new Dictionary<StatType, int>();

        // Unity 직렬화를 위한 리스트
        [SerializeField]
        private List<StatGrowthPair> _growthList = new List<StatGrowthPair>();

        [Serializable]
        private struct StatGrowthPair
        {
            public StatType StatType;
            public int GrowthValue;
        }

        public void OnBeforeSerialize()
        {
            _growthList.Clear();
            foreach (var pair in BaseGrowth)
            {
                _growthList.Add(new StatGrowthPair { StatType = pair.Key, GrowthValue = pair.Value });
            }
        }

        public void OnAfterDeserialize()
        {
            BaseGrowth = new Dictionary<StatType, int>();
            foreach (var pair in _growthList)
            {
                BaseGrowth[pair.StatType] = pair.GrowthValue;
            }
        }
    }
} 