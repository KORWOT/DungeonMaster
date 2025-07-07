using System.Collections.Generic;
using DungeonMaster.Data;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// 전투 중 행동(공격, 스킬 사용 등) 입력을 수집하는 모든 주체의 인터페이스입니다.
    /// (예: AI, 플레이어 컨트롤러)
    /// </summary>
    public interface IActionInputProvider
    {
        /// <summary>
        /// 현재 전투 상태를 기반으로 이번 틱에 발생할 행동 목록을 수집합니다.
        /// </summary>
        /// <param name="currentState">현재의 전투 상태</param>
        /// <param name="battleRules">결정론적 랜덤값 등을 얻기 위한 규칙 참조</param>
        /// <returns>수집된 행동 입력 리스트</returns>
        List<ActionInput> CollectActionInputs(BattleState currentState, DeterministicBattleRules battleRules);
    }
} 