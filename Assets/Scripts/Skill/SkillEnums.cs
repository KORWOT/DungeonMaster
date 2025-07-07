namespace DungeonMaster.Skill
{
    /// <summary>
    /// 스킬 등급 (F~S)
    /// </summary>
    public enum SkillGrade
    {
        F = 0,  // 최하급 (기본 스킬, 높은 등장 확률)
        E = 1,  // 하급 (약한 스킬, 높은 등장 확률)
        D = 2,  // 일반급 (일반적인 스킬, 보통 등장 확률)
        C = 3,  // 중급 (괜찮은 스킬, 보통 등장 확률)
        B = 4,  // 상급 (좋은 스킬, 낮은 등장 확률)
        A = 5,  // 고급 (강력한 스킬, 매우 낮은 등장 확률)
        S = 6   // 최고급 (최강 스킬, 극히 낮은 등장 확률)
    }

    /// <summary>
    /// 스킬 효과 타입 (확장됨)
    /// </summary>
    public enum SkillEffectType
    {
        Damage = 100,           // 피해
        Heal = 101,            // 회복
        Buff = 102,        // ✅ 추가
        AttackBuff = 103,      // 공격력 증가
        DefenseBuff = 104,     // 방어력 증가
        SpeedBuff = 105,       // 공격속도 증가
        AttackDebuff = 106,    // 공격력 감소
        DefenseDebuff = 107,   // 방어력 감소
        SpeedDebuff = 108,     // 공격속도 감소
        Stun = 109,            // 기절
        Poison = 110,          // 독
        Shield = 111,          // 보호막
        Summon = 112,          // 소환
        Teleport = 113,        // 이동
        StatusEffect = 114     // 상태이상
    }

    /// <summary>
    /// 스킬 대상 타입 (확장됨)
    /// </summary>
    public enum SkillTargetType
    {
        Self = 200,         // 자신
        Enemy = 201,        // 적 단일
        AllEnemies = 202,   // 적 전체
        Ally = 203,         // 아군 단일
        AllAllies = 204,    // 아군 전체
        Random = 205,       // 무작위
        Area = 206,         // 지역
        All = 207           // 전체
    }
} 