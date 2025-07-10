using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DungeonMaster.Skill
{
    /// <summary>
    /// 몬스터별 스킬 매핑
    /// </summary>
    [System.Serializable]
    public class MonsterSkillMapping
    {
        [SerializeField] private string monsterName;
        [SerializeField] private SkillData[] skills;

        public string MonsterName => monsterName;
        public SkillData[] Skills => skills;
    }

    /// <summary>
    /// 스킬 매니저 ScriptableObject - 스킬 데이터 관리 전담
    /// </summary>
    [CreateAssetMenu(fileName = "SkillManager", menuName = "Game/Skill Manager")]
    public class SkillManager : ScriptableObject
    {
        private static SkillManager _instance;
        public static SkillManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<SkillManager>("Managers/SkillManager");
                    if (_instance == null)
                    {
                        Debug.LogError("SkillManager not found in Resources/Managers folder. Please create and configure it.");
                    }
                    else
                    {
                        _instance.InitializeDictionaries();
                    }
                }
                return _instance;
            }
        }

        [Header("모든 스킬 데이터")]
        [SerializeField] private SkillData[] allSkills;
        
        [Header("몬스터별 스킬 매핑")]
        [SerializeField] private MonsterSkillMapping[] monsterSkillMappings;
        
        private Dictionary<long, SkillData> skillDictionaryById;
        private Dictionary<string, SkillData[]> monsterSkillDict;
        private SkillData basicAttackSkill; // 기본 공격용 스킬 데이터
        
        private void InitializeDictionaries()
        {
            // 스킬 딕셔너리 초기화 (ID 기준)
            skillDictionaryById = new Dictionary<long, SkillData>();
            
            // 기본 공격 스킬 생성 (ID = 0)
            CreateBasicAttackSkill();
            
            if (allSkills != null)
            {
                foreach (var skill in allSkills)
                {
                    if (skill != null && !skillDictionaryById.ContainsKey(skill.SkillId))
                    {
                        skillDictionaryById[skill.SkillId] = skill;
                    }
                }
            }
            
            // 몬스터별 스킬 딕셔너리 초기화
            monsterSkillDict = new Dictionary<string, SkillData[]>();
            if (monsterSkillMappings != null)
            {
                foreach (var mapping in monsterSkillMappings)
                {
                    if (mapping != null && !string.IsNullOrEmpty(mapping.MonsterName))
                    {
                        monsterSkillDict[mapping.MonsterName] = mapping.Skills;
                    }
                }
            }
        }
        
        /// <summary>
        /// 기본 공격용 SkillData를 동적으로 생성합니다.
        /// </summary>
        private void CreateBasicAttackSkill()
        {
            // ScriptableObject.CreateInstance를 사용하여 런타임에 생성
            basicAttackSkill = ScriptableObject.CreateInstance<SkillData>();
            
            // 리플렉션을 사용하여 private 필드들을 설정 (ScriptableObject의 한계)
            var fields = typeof(SkillData).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                switch (field.Name)
                {
                    case "skillId":
                        field.SetValue(basicAttackSkill, 0L);
                        break;
                    case "skillName":
                        // 지역화 키를 사용합니다.
                        field.SetValue(basicAttackSkill, "skill_basic_attack_name");
                        break;
                    case "descriptionTemplate":
                        // 지역화 키를 사용합니다.
                        field.SetValue(basicAttackSkill, "skill_basic_attack_desc");
                        break;
                    case "skillType":
                        field.SetValue(basicAttackSkill, SkillType.Active);
                        break;
                    case "skillGrade":
                        field.SetValue(basicAttackSkill, SkillGrade.F);
                        break;
                    case "elementType":
                        field.SetValue(basicAttackSkill, DungeonMaster.Character.ElementType.Normal);
                        break;
                    case "skillCoefficient":
                        field.SetValue(basicAttackSkill, 100L); // 1.0배
                        break;
                    case "hitCount":
                        field.SetValue(basicAttackSkill, 1);
                        break;
                    case "requiredLevel":
                        field.SetValue(basicAttackSkill, 0);
                        break;
                    case "cooldown":
                        field.SetValue(basicAttackSkill, 0f);
                        break;
                    case "manaCost":
                        field.SetValue(basicAttackSkill, 0);
                        break;
                    case "targetType":
                        field.SetValue(basicAttackSkill, SkillTargetType.Enemy);
                        break;
                    case "effects":
                        field.SetValue(basicAttackSkill, new SkillEffectData[0]); // 빈 배열
                        break;
                    case "maxLevel":
                        field.SetValue(basicAttackSkill, 1);
                        break;
                }
            }
            
            // 딕셔너리에 추가
            skillDictionaryById[0] = basicAttackSkill;
        }
        
        public SkillData GetSkill(long skillId)
        {
            return skillDictionaryById.TryGetValue(skillId, out var skill) ? skill : null;
        }
        
        /// <summary>
        /// 기본 공격 스킬 데이터를 반환합니다.
        /// </summary>
        public SkillData GetBasicAttackSkill()
        {
            return basicAttackSkill;
        }
        
        public List<SkillData> GetMonsterSkills(string monsterName)
        {
            if (monsterSkillDict.TryGetValue(monsterName, out var skills))
            {
                return new List<SkillData>(skills.Where(s => s != null));
            }
            
            return new List<SkillData>();
        }
        
        public List<SkillData> GetAllSkills()
        {
            return allSkills != null ? new List<SkillData>(allSkills.Where(s => s != null)) : new List<SkillData>();
        }
        
        public List<SkillData> GetAvailableSkills(int characterLevel)
        {
            return GetAllSkills().Where(skill => skill.RequiredLevel <= characterLevel).ToList();
        }
    }
} 