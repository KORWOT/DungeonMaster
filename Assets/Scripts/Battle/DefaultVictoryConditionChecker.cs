using System.Linq;
using DungeonMaster.Data;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// 기본적인 승리/패배 조건(한쪽 진영의 전멸)을 확인합니다.
    /// 마왕이 존재할 경우, 마왕의 생존이 승패에 결정적인 영향을 미칩니다.
    /// </summary>
    public class DefaultVictoryConditionChecker : IVictoryConditionChecker
    {
        public BattleOutcome Check(BattleState currentState)
        {
            // --- 패배 조건 확인 ---
            // 1. 플레이어 소속 마왕이 죽었는지 확인
            var playerDemonLord = currentState.DemonLords.FirstOrDefault(dl => dl.IsPlayer);
            if (playerDemonLord != null && playerDemonLord.CurrentHP <= 0)
            {
                return BattleOutcome.Defeat;
            }
            
            // 2. 플레이어 마왕이 없거나 살아있다면, 플레이어 몬스터들이 전멸했는지 확인
            bool isPlayerMonstersAlive = currentState.Characters.Any(c => c.IsPlayerCharacter && c.CurrentHP > 0);
            if (playerDemonLord == null && !isPlayerMonstersAlive)
            {
                return BattleOutcome.Defeat;
            }

            // --- 승리 조건 확인 ---
            // 1. 적 몬스터가 모두 죽었는지 확인
            bool isEnemyMonstersAlive = currentState.Characters.Any(c => !c.IsPlayerCharacter && c.CurrentHP > 0);
            // 2. 적 마왕이 모두 죽었는지 확인
            bool isEnemyDemonLordsAlive = currentState.DemonLords.Any(dl => !dl.IsPlayer && dl.CurrentHP > 0);

            if (!isEnemyMonstersAlive && !isEnemyDemonLordsAlive)
            {
                return BattleOutcome.Victory;
            }

            return BattleOutcome.Ongoing;
        }
    }
} 