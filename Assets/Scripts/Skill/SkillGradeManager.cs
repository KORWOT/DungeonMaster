using UnityEngine;
using System.Collections.Generic;
using DungeonMaster.Localization;

namespace DungeonMaster.Skill
{
    /// <summary>
    /// 스킬 등급별 설정 데이터
    /// </summary>


    /// <summary>
    /// 스킬 등급 관리자
    /// </summary>
    public static class SkillGradeManager
    {
        private static SkillGradeConfig _config;
        
        public static SkillGradeConfig Config
        {
            get
            {
                if (_config == null)
                {
                    _config = Resources.Load<SkillGradeConfig>("SkillGradeConfig");
                    if (_config == null)
                    {
                        Debug.LogWarning("SkillGradeConfig를 찾을 수 없습니다. 기본 설정을 사용합니다.");
                        _config = CreateDefaultConfig();
                    }
                }
                return _config;
            }
        }
        
        // 기본 설정 생성
        private static SkillGradeConfig CreateDefaultConfig()
        {
            var config = ScriptableObject.CreateInstance<SkillGradeConfig>();
            config.ApplyDefaultSettings();
            return config;
        }
        
        // 스킬 등장 확률 계산
        public static float GetSkillAppearanceRate(SkillGrade grade)
        {
            var gradeData = Config?.GetGradeData(grade);
            if (gradeData == null) return 0f;
            
            return gradeData.AppearanceRate;
        }
        
        // TODO: 스킬 등급 생성 시스템은 나중에 구현 예정
        // - GenerateRandomSkillGrade() - 마왕용 랜덤 스킬 등급 생성
        // - GenerateRandomSkillGradeForMonster() - 몬스터용 랜덤 스킬 등급 생성
        
        // 등급별 색상 가져오기
        public static Color GetGradeColor(SkillGrade grade)
        {
            return Config?.GetGradeColor(grade) ?? Color.white;
        }
        
        // 등급별 이름 가져오기
        public static string GetGradeName(SkillGrade grade)
        {
            var name = Config?.GetGradeName(grade);
            return string.IsNullOrEmpty(name) ? LocalizationManager.Instance.GetText("skill_grade_unknown_name") : name;
        }
        
        // 등급별 설명 가져오기
        public static string GetGradeDescription(SkillGrade grade)
        {
            var desc = Config?.GetGradeDescription(grade);
            return string.IsNullOrEmpty(desc) ? LocalizationManager.Instance.GetText("skill_grade_unknown_desc") : desc;
        }
    }
} 