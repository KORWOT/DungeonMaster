namespace DungeonMaster.Buffs
{
    /// <summary>
    /// 버프가 수행하는 핵심 로직의 종류를 정의합니다.
    /// </summary>
    public enum BuffEffectType
    {
        /// <summary>
        /// 캐릭터의 특정 스탯을 고정값/비율로 변경합니다.
        /// </summary>
        StatChange,
        
        // 향후 추가될 버프 로직 타입들
        // DotDamage, // 지속 피해
        // Stun,      // 기절
        // Shield,    // 보호막
    }
} 