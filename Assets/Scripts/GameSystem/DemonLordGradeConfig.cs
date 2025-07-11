using System.Collections.Generic;
using DungeonMaster.Character;
using UnityEngine;

namespace DungeonMaster.GameSystem
{
    [CreateAssetMenu(fileName = "DemonLordGradeConfig", menuName = "DungeonMaster/System/DemonLord Grade Config")]
    public class DemonLordGradeConfig : ScriptableObject
    {
        [SerializeField]
        private List<GradeMultiplier> gradeMultipliers = new List<GradeMultiplier>();

        private Dictionary<DemonLordGrade, float> _multiplierDict;

        public Dictionary<DemonLordGrade, float> MultiplierDict
        {
            get
            {
                if (_multiplierDict == null)
                {
                    _multiplierDict = new Dictionary<DemonLordGrade, float>();
                    foreach (var item in gradeMultipliers)
                    {
                        _multiplierDict[item.Grade] = item.Multiplier;
                    }
                }
                return _multiplierDict;
            }
        }

        public float GetMultiplier(DemonLordGrade grade)
        {
            return MultiplierDict.TryGetValue(grade, out float multiplier) ? multiplier : 1.0f;
        }
    }

    [System.Serializable]
    public class GradeMultiplier
    {
        public DemonLordGrade Grade;
        [Tooltip("기본 성장률에 곱해지는 배율입니다. (예: 1.5는 150%를 의미)")]
        public float Multiplier = 1.0f;
    }
} 