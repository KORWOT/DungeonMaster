using DungeonMaster.Data;
using System.Collections.Generic;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// 전투에서 수행될 수 있는 모든 개별 행동(액션)의 인터페이스입니다.
    /// 각 액션은 자신을 실행하는 데 필요한 모든 데이터를 생성 시점에 전달받습니다.
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// 액션을 실행하고, 그 결과로 변경된 새로운 전투 상태와 발생한 이벤트 목록을 반환합니다.
        /// 이 메서드는 순수 함수처럼 동작하여, 외부 상태를 직접 변경하지 않습니다.
        /// </summary>
        /// <param name="currentState">액션이 실행되기 전의 현재 전투 상태</param>
        /// <param name="battleRules">데미지 공식 등 전투 규칙에 접근하기 위한 참조</param>
        /// <returns>액션 실행 후의 (새로운 전투 상태, 발생한 이벤트 목록) 튜플.</returns>
        (BattleState newState, List<BattleEvent> newEvents) Execute(BattleState currentState, DeterministicBattleRules battleRules);
    }
} 