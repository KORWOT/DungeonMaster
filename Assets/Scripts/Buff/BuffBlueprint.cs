using UnityEngine;
using DungeonMaster.Character; // StatType을 위해 추가
using DungeonMaster.Shared.Scaling;
using DungeonMaster.Localization;

namespace DungeonMaster.Buffs
{
    /// <summary>
    /// 버프의 정적인 기본 데이터를 정의하는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "New Buff", menuName = "Game/Buff Blueprint")]
    public class BuffBlueprint : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private long buffId;
        [SerializeField] private string buffName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        
        [Header("효과 정보")]
        [SerializeField] private BuffEffectType effectType;
        [SerializeField] private float baseDurationSeconds; // 기본 지속 시간 (초)
        [SerializeField] private bool isDebuff; // 디버프 여부 (UI 등에서 구분용)
        [SerializeField] private int maxStacks = 1; // 최대 중첩 수

        [Header("StatChange 효과 전용")]
        [Tooltip("StatChange 타입일 때만 사용됩니다.")]
        [SerializeField] private StatType targetStat;
        [Tooltip("StatChange 타입일 때만 사용됩니다.")]
        [SerializeField] private long effectValue;
        
        [Header("스케일링 설정")]
        [SerializeField] private IndividualScaling durationScaling = new IndividualScaling(ScalingType.None);
        [SerializeField] private IndividualScaling valueScaling = new IndividualScaling(ScalingType.None);
        
        // TODO: 스킬 레벨이나 시전자 스탯에 따른 스케일링 로직 추가
        // 예: public ScalingInfo durationScaling;
        // 예: public ScalingInfo effectValueScaling;

        public long BuffId => buffId;
        public string BuffName => LocalizationManager.Instance.GetText(buffName);
        public string Description => LocalizationManager.Instance.GetText(description);
        public Sprite Icon => icon;
        public BuffEffectType EffectType => effectType;
        public float BaseDurationSeconds => baseDurationSeconds;
        public bool IsDebuff => isDebuff;
        public int MaxStacks => maxStacks;

        // StatChange 속성
        public StatType TargetStat => targetStat;
        public long EffectValue => effectValue;

        // 스케일링 속성
        public IndividualScaling DurationScaling => durationScaling;
        public IndividualScaling ValueScaling => valueScaling;

        /// <summary>
        /// 레벨(중첩)에 따라 스케일링된 지속시간을 계산합니다.
        /// </summary>
        public float GetScaledDuration(int level)
        {
            return baseDurationSeconds + durationScaling.CalculateScaling(level, maxStacks);
        }

        /// <summary>
        /// 레벨(중첩)에 따라 스케일링된 효과값을 계산합니다.
        /// </summary>
        public long GetScaledValue(int level)
        {
            return effectValue + (long)valueScaling.CalculateScaling(level, maxStacks);
        }
    }
} 