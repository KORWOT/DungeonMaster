using System.Collections.Generic;
using DungeonMaster.Skill;
using UnityEngine;
using System.Linq;

namespace DungeonMaster.Skill
{
    /// <summary>
    /// 스킬 등급 데이터 구조체. 등급에 대한 모든 정보를 포함.
    /// </summary>
    [System.Serializable]
    public class SkillGradeData
    {
        public string Name = "일반급";
        [TextArea] public string Description = "평균적인 성능을 가집니다.";
        public Color GradeColor = Color.white;
        public float AppearanceRate = 50f;
    }

    /// <summary>
    /// 인스펙터에서 등급과 데이터를 매핑하기 위한 클래스
    /// </summary>
    [System.Serializable]
    public class SkillGradeMapping
    {
        public SkillGrade Grade;
        public SkillGradeData Data = new();
    }

    /// <summary>
    /// 스킬 등급 설정 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "SkillGradeConfig", menuName = "Game/Skill Grade Config")]
    public class SkillGradeConfig : ScriptableObject
    {
        [SerializeField]
        private List<SkillGradeMapping> gradeMappings = new();

        private readonly Dictionary<SkillGrade, SkillGradeData> gradeDatabase = new();

        private void OnEnable()
        {
            Initialize();
        }

        private void OnValidate()
        {
            Initialize();
        }

        private void Initialize()
        {
            gradeDatabase.Clear();
            foreach (var mapping in gradeMappings)
            {
                if (!gradeDatabase.ContainsKey(mapping.Grade))
                {
                    gradeDatabase.Add(mapping.Grade, mapping.Data);
                }
                else
                {
                    Debug.LogWarning($"중복된 스킬 등급 키: {mapping.Grade}");
                }
            }
        }

        // 기본 설정 적용 메서드
        [ContextMenu("Apply Default Settings")]
        public void ApplyDefaultSettings()
        {
            gradeMappings = new List<SkillGradeMapping>
            {
                new() { Grade = SkillGrade.F, Data = new SkillGradeData { Name = "최하급", Description = "기본적인 효과.", GradeColor = new Color(0.3f, 0.3f, 0.3f), AppearanceRate = 100f }},
                new() { Grade = SkillGrade.E, Data = new SkillGradeData { Name = "하급", Description = "약하지만 안정적.", GradeColor = new Color(0.5f, 0.5f, 0.5f), AppearanceRate = 80f }},
                new() { Grade = SkillGrade.D, Data = new SkillGradeData { Name = "일반급", Description = "평균적인 성능.", GradeColor = new Color(0.7f, 0.7f, 0.7f), AppearanceRate = 60f }},
                new() { Grade = SkillGrade.C, Data = new SkillGradeData { Name = "중급", Description = "괜찮은 성능.", GradeColor = new Color(0.2f, 0.6f, 1f), AppearanceRate = 40f }},
                new() { Grade = SkillGrade.B, Data = new SkillGradeData { Name = "상급", Description = "좋은 성능.", GradeColor = new Color(0.8f, 0.2f, 0.8f), AppearanceRate = 20f }},
                new() { Grade = SkillGrade.A, Data = new SkillGradeData { Name = "고급", Description = "강력한 성능.", GradeColor = new Color(0.2f, 0.8f, 0.2f), AppearanceRate = 8f }},
                new() { Grade = SkillGrade.S, Data = new SkillGradeData { Name = "최고급", Description = "최강의 성능.", GradeColor = new Color(1f, 0.8f, 0f), AppearanceRate = 2f }}
            };
            Initialize();
            Debug.Log("스킬 등급 기본 설정이 적용되었습니다.");
        }
        
        /// <summary>
        /// 등급에 해당하는 모든 데이터를 가져옵니다.
        /// </summary>
        public SkillGradeData GetGradeData(SkillGrade grade)
        {
            if (gradeDatabase.TryGetValue(grade, out var data))
            {
                return data;
            }
            
            Debug.LogWarning($"요청한 스킬 등급({grade})에 대한 설정이 없습니다. 기본값을 반환합니다.");
            // D 등급 또는 첫 번째 데이터를 기본값으로 반환
            return gradeDatabase.TryGetValue(SkillGrade.D, out var defaultData) ? defaultData : (gradeDatabase.Count > 0 ? gradeDatabase.Values.First() : new SkillGradeData());
        }

        public Color GetGradeColor(SkillGrade grade)
        {
            var data = GetGradeData(grade);
            return data?.GradeColor ?? Color.white;
        }

        public string GetGradeName(SkillGrade grade)
        {
            var data = GetGradeData(grade);
            return data?.Name ?? "알 수 없음";
        }

        public string GetGradeDescription(SkillGrade grade)
        {
            var data = GetGradeData(grade);
            return data?.Description ?? "알 수 없는 등급입니다.";
        }
    }
} 