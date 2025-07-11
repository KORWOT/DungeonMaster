using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Battle;
using DungeonMaster.DemonLord;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 특정 시점의 전투 상태 전체를 나타내는 클래스입니다. 이 객체는 불변성을 가집니다.
    /// </summary>
    public class BattleState
    {
        public List<DeterministicCharacterData> Characters { get; }
        public List<DemonLordData> DemonLords { get; }
        public BattleStatus Status { get; }
        public long CurrentTimeMs { get; set; }
        public int TurnCount { get; private set; }
        public List<BattleEvent> Events { get; private set; }

        public BattleState(IEnumerable<DeterministicCharacterData> characters, IEnumerable<DemonLordData> demonLords, BattleStatus status)
        {
            Characters = characters?.Select(c => new DeterministicCharacterData(c)).ToList() ?? new List<DeterministicCharacterData>();
            DemonLords = demonLords?.Select(dl => dl.With()).ToList() ?? new List<DemonLordData>();
            Status = status;
            CurrentTimeMs = 0;
            TurnCount = 1; // 전투 시작 시 1턴
            Events = new List<BattleEvent>();
        }

        private BattleState(List<DeterministicCharacterData> characters, List<DemonLordData> demonLords, BattleStatus status, List<BattleEvent> events, int turnCount)
        {
            Characters = characters;
            DemonLords = demonLords;
            Status = status;
            Events = events;
            TurnCount = turnCount;
        }

        /// <summary>
        /// 상태를 변경하는 새 BattleState를 생성합니다.
        /// </summary>
        public BattleState With(IEnumerable<DeterministicCharacterData> newCharacters = null,
                                IEnumerable<DemonLordData> newDemonLords = null,
                                BattleStatus? newStatus = null,
                                List<BattleEvent> newEvents = null,
                                int? newTurnCount = null)
        {
            var characterClones = newCharacters?.Select(c => new DeterministicCharacterData(c)).ToList() ?? Characters;
            var demonLordClones = newDemonLords?.Select(dl => dl.With()).ToList() ?? DemonLords;
            var status = newStatus ?? Status;
            var events = newEvents ?? Events;
            var turnCount = newTurnCount ?? TurnCount;

            return new BattleState(characterClones, demonLordClones, status, events, turnCount);
        }
        
        public object GetCombatant(long instanceId)
        {
            object character = Characters.FirstOrDefault(c => c.InstanceId == instanceId);
            if (character != null)
            {
                return character;
            }
            return DemonLords.FirstOrDefault(dl => dl.InstanceId == instanceId);
        }

        public BattleState Clone()
        {
            var clonedCharacters = Characters.Select(c => new DeterministicCharacterData(c)).ToList();
            var clonedDemonLords = DemonLords.Select(dl => dl.With()).ToList();
            var clonedEvents = new List<BattleEvent>(Events);
            var clonedState = new BattleState(clonedCharacters, clonedDemonLords, Status, clonedEvents, TurnCount);
            clonedState.CurrentTimeMs = this.CurrentTimeMs;
            return clonedState;
        }
    }
}