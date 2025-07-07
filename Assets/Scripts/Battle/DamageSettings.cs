using UnityEngine;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// 피해 계산 관련 설정값들 (ScriptableObject로 관리)
    /// </summary>
    [CreateAssetMenu(fileName = "DamageSettings", menuName = "Game/Battle/DamageSettings")]
    public class DamageSettings : ScriptableObject
    {
        [Header("기본 배율 설정")]
        [SerializeField] private float defaultCritMultiplier = 1.5f; // 기본 치명타 배율
        [SerializeField] private float minDamageRatio = 0.1f; // 최소 피해량 비율 (주는 피해량의 %)
        
        [Header("확장 가능한 설정")]
        [SerializeField] private float maxCritMultiplier = 3.0f; // 최대 치명타 배율
        [SerializeField] private float maxProtectionRate = 0.9f; // 최대 보호율 (90%)
        [SerializeField] private float maxDamageReductionRate = 0.8f; // 최대 피해감소율 (80%)
        
        // 프로퍼티로 외부 접근 제공
        public float DefaultCritMultiplier => defaultCritMultiplier;
        public float MinDamageRatio => minDamageRatio;
        public float MaxCritMultiplier => maxCritMultiplier;
        public float MaxProtectionRate => maxProtectionRate;
        public float MaxDamageReductionRate => maxDamageReductionRate;
        
        // 나중에 추가될 설정들을 위한 공간
        // 예: 속성 상성 배율, 스킬별 기본 배율 등
    }
}