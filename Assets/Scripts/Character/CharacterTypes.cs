namespace DungeonMaster.Character
{
    /// <summary>
    /// 방어 타입
    /// </summary>
    public enum DefenseType
    {
        None,      // 없음
        Physical,  // 물리
        Magical,   // 마법
        Mixed      // 혼합
    }

    /// <summary>
    /// 공격 타입
    /// </summary>
    public enum AttackType
    {
        Physical, // 물리 공격
        Magical   // 마법 공격
    }

    /// <summary>
    /// 아머 타입
    /// </summary>
    public enum ArmorType
    {
        None,    // 없음
        Light,   // 경갑
        Medium,  // 평갑
        Heavy    // 중갑
    }

    /// <summary>
    /// 등급
    /// </summary>
    public enum Grade
    {
        C,   // 커먼
        UC,  // 언커먼
        R,   // 레어
        SR,  // 스페셜레어
        UR   // 울트라레어
    }

    /// <summary>
    /// 성장률 등급 (GrowthGradeSystem과 통합)
    /// </summary>
    public enum GrowthGrade
    {
        F = 0,  // 50% ~ 80%
        E = 1,  // 81% ~ 110%
        D = 2,  // 111% ~ 140%
        C = 3,  // 141% ~ 170%
        B = 4,  // 171% ~ 200%
        A = 5,  // 201% ~ 230%
        S = 6   // 231% ~ 260%
    }

    /// <summary>
    /// 몬스터 타입
    /// </summary>
    public enum MonsterType
    {
        Balanced,    // 균형형
        Attacker,    // 공격형
        Defender,    // 방어형
        Support,     // 지원형
        Specialist   // 특수형
    }

    /// <summary>
    /// 적군 타입
    /// </summary>
    public enum EnemyType
    {
        Normal,  // 일반
        Elite,   // 정예
        Boss     // 보스
    }

    /// <summary>
    /// 마왕 등급
    /// </summary>
    public enum DemonLordGrade
    {
        Unique,
        Epic,
        Legendary
    }
}