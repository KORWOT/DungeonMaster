namespace DungeonMaster.Character
{
    /// <summary>
    /// 캐릭터가 가질 수 있는 모든 스탯의 종류를 정의합니다.
    /// </summary>
    public enum StatType
    {
        // --- 기본 정보 (0-99) ---
        Level = 0,
        Experience = 1,

        // --- 생존 스탯 (100-199) ---
        Hp = 100,          // ✅ MaxHP 별칭 추가
        MaxHP = 100,              // 최대 생명력
        Defense = 101,            // 방어력
        ProtectionRate = 102,     // 보호율 (보호막 흡수량 등에 영향)
        DamageReductionRate = 103,// 피해 감소율 (%)
        DamageReduction = 104,    // 피해 감소 (고정 수치)
        EvasionRate = 105,        // 회피율 (%)
        HealBonus = 106,          // 받는 회복량 증가 (%)
        LifeOnKill = 107,         // 처치 시 생명력 회복 (고정 수치)

        // --- 공격 스탯 (200-299) ---
        Attack = 200,             // 공격력
        CritRate = 201,           // 치명타율 (%)
        CritMultiplier = 202,     // 치명타 피해 배율 (%)
        DamageBonus = 203,        // 최종 피해 증가 (%)
        Penetration = 204,        // 방어 관통력 (고정 수치)
        PenetrationRate = 205,    // 방어 관통률 (%)
        LifeSteal = 206,          // 흡혈률 (%)
        AttackSpeed = 207,        // 공격 속도
        CooldownReduction = 208,  // 재사용 대기시간 감소 (%)

        // --- 속성 관련 스탯 (300-399) ---
        FireDamageBonus = 300,    // 화속성 피해 증가 (%)
        WaterDamageBonus = 301,   // 수속성 피해 증가 (%)
        WindDamageBonus = 302,    // 풍속성 피해 증가 (%)
        EarthDamageBonus = 303,   // 지(地)속성 피해 증가 (%)
        LightDamageBonus = 304,   // 명속성 피해 증가 (%)
        DarkDamageBonus = 305,    // 암속성 피해 증가 (%)
    }
    
    /// <summary>
    /// 특정 스탯의 타입과 값을 함께 묶는 구조체입니다.
    /// Unity 인스펙터에 노출시키기 위해 Serializable 특성을 가집니다.
    /// </summary>
    [System.Serializable]
    public struct StatValue
    {
        public StatType StatType;
        public long Value;
    }
    
    /// <summary>
    /// 레벨업 시 스탯 보너스의 고정 성장치를 정의하는 구조체입니다.
    /// Min-Max 범위를 제거하여 결정론적 계산을 보장합니다.
    /// Unity 인스펙터에 노출시키기 위해 Serializable 특성을 가집니다.
    /// </summary>
    [System.Serializable]
    public struct LevelUpBonusStat
    {
        public StatType StatType;
        public float GrowthValue;
    }
} 