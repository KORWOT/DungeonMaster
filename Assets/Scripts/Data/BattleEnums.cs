namespace DungeonMaster.Data
{
    /// <summary>
    /// 전투의 현재 진행 상태를 나타냅니다.
    /// </summary>
    public enum BattleStatus
    {
        /// <summary>
        /// 전투가 준비 중인 상태입니다.
        /// </summary>
        Preparing = 0,

        /// <summary>
        /// 전투가 활발하게 진행 중인 상태입니다.
        /// </summary>
        Ongoing = 1,
        InProgress = 1,

        /// <summary>
        /// 전투가 종료된 상태입니다.
        /// </summary>
        Finished = 2
    }

    /// <summary>
    /// 전투 종료 시의 결과를 나타냅니다.
    /// </summary>
    public enum BattleOutcome
    {
        /// <summary>
        /// 아직 전투가 끝나지 않았습니다.
        /// </summary>
        Ongoing = 100,

        /// <summary>
        /// 플레이어 진영이 승리했습니다.
        /// </summary>
        Victory = 101,

        /// <summary>
        /// 플레이어 진영이 패배했습니다.
        /// </summary>
        Defeat = 102
    }
} 