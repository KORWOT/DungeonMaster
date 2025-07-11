using System.Collections.Generic;
using DungeonMaster.Shared.Scaling;
using UnityEngine;

namespace DungeonMaster.Dungeon
{
    public enum RoomType
    {
        Start,      // 시작
        Combat,     // 전투
        Trap,       // 함정
        Buff,       // 버프
        Resource,   // 자원
        Treasure,   // 보물
        Rest,       // 휴식
        Boss        // 보스
    }

    [CreateAssetMenu(fileName = "NewRoomBlueprint", menuName = "DungeonMaster/Blueprint/Room", order = 1)]
    public class RoomBlueprint : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("방을 식별하는 고유 ID입니다. 이 값은 변경하면 안 됩니다.")]
        public string BlueprintId;
        
        [Tooltip("방의 이름에 대한 지역화 키입니다.")]
        public string NameKey;
        
        [TextArea] 
        [Tooltip("방의 설명에 대한 지역화 키입니다.")]
        public string DescriptionKey;
        
        public Sprite Icon;
        public int Grade; // 등급

        [Header("배치 정보")]
        public RoomType Type;
        public int PlacementCost; // 배치 비용

        [Header("성장 정보")]
        [Range(1, 100)]
        public int MaxLevel = 10;
        
        [Tooltip("레벨업에 필요한 경험치 곡선. X축: 레벨, Y축: 필요 경험치 총량")]
        public AnimationCurve XpCurve = AnimationCurve.Linear(1, 100, 10, 1000);

        [Header("방 기능")]
        [Tooltip("전투 방일 경우, 최대 배치 가능한 몬스터 수")]
        public int MaxMonsterCapacity;

        [Tooltip("이 방이 가지고 있는 모든 효과 목록")]
        public List<RoomEffectBlueprint> Effects;

        // TODO: 레벨업에 따른 성장 정보, 개조 정보 등 추가
    }
} 