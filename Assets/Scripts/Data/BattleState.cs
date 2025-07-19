using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Battle;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 특정 시점의 전투 상태 전체를 나타내는 클래스입니다. 이 객체는 불변성을 가집니다.
    /// </summary>
    public class BattleState
    {
        public List<DeterministicCharacterData> Characters { get; }
        public BattleStatus Status { get; }
        public long CurrentTimeMs { get; set; }
        public List<BattleEvent> Events { get; private set; }

        public BattleState(IEnumerable<DeterministicCharacterData> characters, BattleStatus status)
        {
            Characters = characters.Select(c => new DeterministicCharacterData(c)).ToList();
            Status = status;
            CurrentTimeMs = 0;
            Events = new List<BattleEvent>();
        }

        private BattleState(List<DeterministicCharacterData> characters, BattleStatus status, List<BattleEvent> events)
        {
            Characters = characters;
            Status = status;
            Events = events;
        }

        /// <summary>
        /// 상태를 변경하는 새 BattleState를 생성합니다.
        /// </summary>
        public BattleState With(IEnumerable<DeterministicCharacterData> newCharacters = null,
                                BattleStatus? newStatus = null,
                                List<BattleEvent> newEvents = null)
        {
            var characterClones = newCharacters?.Select(c => new DeterministicCharacterData(c)).ToList() ?? Characters;
            var status = newStatus ?? Status;
            var events = newEvents ?? Events;

            return new BattleState(characterClones, status, events);
        }

        public DeterministicCharacterData GetCharacter(long instanceId)
        {
            return Characters.FirstOrDefault(c => c.InstanceId == instanceId);
        }

        public BattleState Clone()
        {
            var clonedCharacters = Characters.Select(c => new DeterministicCharacterData(c)).ToList();
            var clonedEvents = new List<BattleEvent>(Events); // 이벤트는 복사합니다.
            var clonedState = new BattleState(clonedCharacters, Status, clonedEvents);
            clonedState.CurrentTimeMs = this.CurrentTimeMs;
            return clonedState;
        }

        // AddEvent와 ClearEvents는 불변성을 위반하므로 제거합니다.
        // public void AddEvent(BattleEvent newEvent)
        // {
        //     Events.Add(newEvent);
        // }
        // 
        // public void ClearEvents()
        // {
        //     Events.Clear();
        // }
    }
}