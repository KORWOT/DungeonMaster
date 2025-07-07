using DungeonMaster.Character;
using UnityEngine;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 몬스터의 종류 자체에 대한 '설계도' 데이터.
    /// ScriptableObject로 만들어 에디터에서 몬스터의 원본 데이터를 정의합니다.
    /// 이 데이터는 게임 중에 변경되지 않는 불변(Immutable) 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCardBlueprint", menuName = "DungeonMaster/Data/Card Blueprint")]
    public class CardBlueprintData : ScriptableObject
    {
        [Header("=== 기본 정보 ===")]
        public long BlueprintId; // 이 설계도의 고유 숫자 ID
        public string Name; // 이름
        public Grade Grade; // 초기 등급
        public ElementType Element; // 속성
        public DefenseType DefenseType; // 방어 타입
        
        [TextArea]
        public string Description; // 설명
        public Sprite Artwork; // 카드 아트워크
        
        [Header("=== 스탯 정보 (Lv.1 기준) ===")]
        public StatValue[] BaseStats; // 기본 스탯 (HP, 공격력, 방어력 등)
        [Tooltip("초당 공격 횟수 x 100. 예: 1.5회/초 -> 150")]
        public long BaseAttackSpeed_x100;

        [Header("=== 성장 정보 ===")]
        [Tooltip("이 카드가 레벨업 할 때마다 성장하는 스탯의 종류 목록입니다.")]
        public StatType[] GrowableStatTypes; // 레벨업 시 스탯별 증가량 정보

        [Header("스킬 정보")]
        public long[] SkillIds; // 습득 가능한 스킬의 고유 숫자 ID 목록

        [Header("기타 설정")]
        public bool IsStarterCard = false; // 이 카드가 시작 시 지급되는 카드인지 여부
    }
} 