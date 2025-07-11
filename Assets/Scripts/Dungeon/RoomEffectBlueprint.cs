using DungeonMaster.Shared.Scaling;
using UnityEngine;

namespace DungeonMaster.Dungeon
{
    /// <summary>
    /// 모든 방 효과 데이터의 기반이 되는 추상 ScriptableObject입니다.
    /// 실제 효과는 이 클래스를 상속하여 만듭니다.
    /// </summary>
    public abstract class RoomEffectBlueprint : ScriptableObject
    {
        [Header("공통 설정")]
        [Tooltip("이 효과가 발동될 조건 목록")]
        public RoomEffectTrigger[] Triggers;

        [Header("효과 스케일링")]
        [Tooltip("효과의 주된 값(피해량, 회복량 등)이 방 레벨에 따라 어떻게 강해질지 설정합니다.")]
        public IndividualScaling ValueScaling = new IndividualScaling(ScalingType.Linear, 1.0f);

        [Tooltip("효과의 지속시간이 방 레벨에 따라 어떻게 변할지 설정합니다.")]
        public IndividualScaling DurationScaling = new IndividualScaling(ScalingType.None, 0f);

        /// <summary>
        /// 이 청사진에 해당하는 실제 효과 로직 인스턴스를 생성하여 반환합니다.
        /// </summary>
        public abstract IRoomEffect CreateEffect();
    }
} 