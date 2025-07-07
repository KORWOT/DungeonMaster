using DungeonMaster.Data;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// 전투의 승리/패배 조건을 확인하는 모든 규칙의 인터페이스입니다.
    /// </summary>
    public interface IVictoryConditionChecker
    {
        /// <summary>
        /// 현재 전투 상태를 기반으로 전투 결과를 확인합니다.
        /// </summary>
        /// <param name="currentState">현재의 전투 상태</param>
        /// <returns>전투 결과 (진행중, 승리, 또는 패배)</returns>
        BattleOutcome Check(BattleState currentState);
    }
} 