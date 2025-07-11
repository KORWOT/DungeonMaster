using System.Collections.Generic;
using DungeonMaster.Character;
using UnityEngine;

namespace DungeonMaster.DemonLord
{
    [CreateAssetMenu(fileName = "NewDemonLord", menuName = "DungeonMaster/DemonLord/DemonLordBlueprint")]
    public class DemonLordBlueprint : ScriptableObject
    {
        [Header("ID 및 이름")]
        [Tooltip("마왕을 식별하는 고유 ID입니다.")]
        public string BlueprintId;
        [Tooltip("마왕 이름의 지역화 키입니다.")]
        public string NameKey;
        [TextArea]
        [Tooltip("마왕 설명의 지역화 키입니다.")]
        public string DescriptionKey;
        
        [Header("기본 분류")]
        public DemonLordGrade Grade;
        public ElementType Element;
        public DefenseType ArmorType;

        [Header("시각 정보")]
        [Tooltip("마왕의 초상화 또는 모델 프리팹 등")]
        public GameObject ArtPrefab;
        
        [Header("능력치 정보")]
        [Tooltip("레벨 1일 때의 기본 능력치 목록입니다.")]
        public List<StatValue> BaseStats;
        [Tooltip("레벨 업 당 스탯 성장률입니다. (예: 150 입력 시 매 레벨 1.5씩 증가)")]
        public List<StatValue> GrowthStats_x100;

        [Header("스킬 및 특성")]
        [Tooltip("이 마왕 타입이 가질 수 있는 고유 특성(패시브) 목록입니다.")]
        public List<ScriptableObject> UniquePassiveBlueprints;
        [Tooltip("이 마왕이 처음부터 보유하고 있는 액티브 스킬들입니다. (최대 3개)")]
        public List<DemonLordSkillBlueprint> StartingActiveSkills;
        [Tooltip("이 마왕이 처음부터 보유하고 있는 궁극 스킬입니다.")]
        public DemonLordSkillBlueprint StartingUltimateSkill;
    }
    
    /// <summary>
    /// 스탯 타입과 초기값을 묶어서 에디터에서 쉽게 수정하기 위한 보조 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class StatValue
    {
        public StatType StatType;
        public long Value; // 정수형 스탯을 사용
    }
} 