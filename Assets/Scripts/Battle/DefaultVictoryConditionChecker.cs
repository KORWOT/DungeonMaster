using System.Linq;
using DungeonMaster.Data;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// 기본적인 승리/패배 조건(한쪽 진영의 전멸)을 확인합니다.
    /// </summary>
    public class DefaultVictoryConditionChecker : IVictoryConditionChecker
    {
        public BattleOutcome Check(BattleState currentState)
        {
            bool isPlayerPartyAlive = currentState.Characters.Any(c => c.IsPlayerCharacter && c.CurrentHP > 0);
            bool isEnemyPartyAlive = currentState.Characters.Any(c => !c.IsPlayerCharacter && c.CurrentHP > 0);

            if (!isPlayerPartyAlive)
            {
                return BattleOutcome.Defeat;
            }

            if (!isEnemyPartyAlive)
            {
                return BattleOutcome.Victory;
            }

            return BattleOutcome.Ongoing;
        }
    }
} 