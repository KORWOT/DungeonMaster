using DungeonMaster.Data;
using System.Collections.Generic;

namespace DungeonMaster.Dungeon
{
    /// <summary>
    /// 방 효과 실행에 필요한 모든 컨텍스트 정보를 담는 클래스입니다.
    /// </summary>
    public class RoomEffectContext
    {
        /// <summary>
        /// 효과를 발동시킨 주체 (방, 몬스터 등).
        /// </summary>
        public object Source { get; }

        /// <summary>
        /// 효과의 주 대상.
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// 효과의 영향을 받는 모든 대상 목록.
        /// </summary>
        public List<object> Targets { get; set; }

        /// <summary>
        /// 효과가 발생한 방의 데이터.
        /// </summary>
        public RoomData CurrentRoom { get; }

        /// <summary>
        /// 현재 던전의 전체 데이터.
        /// </summary>
        public DungeonData CurrentDungeon { get; }

        public RoomEffectContext(object source, RoomData currentRoom, DungeonData currentDungeon)
        {
            Source = source;
            CurrentRoom = currentRoom;
            CurrentDungeon = currentDungeon;
            Targets = new List<object>();
        }
    }
} 