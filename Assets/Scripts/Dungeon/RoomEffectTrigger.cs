namespace DungeonMaster.Dungeon
{
    /// <summary>
    /// 방의 효과가 언제 발동되는지를 정의하는 열거형입니다.
    /// </summary>
    public enum RoomEffectTrigger
    {
        // 전투 중 트리거
        OnBattleStart,      // 전투 시작 시
        OnTurnStart,        // 턴 시작 시
        OnEnemyEnter,       // 적이 방에 진입했을 때
        OnAllyMonsterPlaced, // 아군 몬스터가 배치될 때
        OnAllyMonsterDeath, // 아군 몬스터 사망 시
        OnEnemyMonsterDeath,// 적 몬스터 사망 시

        // 시간 기반 트리거 (비전투)
        OnPeriodically,     // 주기적으로 (예: 10초마다)

        // 기타
        OnDamaged           // 방이 피해를 입었을 때
    }
} 