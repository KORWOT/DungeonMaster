using UnityEngine;
using System.Collections.Generic;

namespace DungeonMaster.Dungeon
{
    /// <summary>
    /// 플레이어가 생성하고 커스터마이징한 던전의 모든 상태 정보를 저장하는 클래스입니다.
    /// 이 데이터가 저장/로드의 대상이 됩니다.
    /// </summary>
    [System.Serializable]
    public class DungeonData
    {
        public string Guid { get; private set; }
        public string CustomName { get; set; }
        public Vector2Int GridSize { get; set; }
        public Vector2Int StartPosition { get; set; }
        public Vector2Int BossRoomPosition { get; set; }

        public Dictionary<Vector2Int, RoomData> Rooms { get; private set; }

        public DungeonData(string name, Vector2Int gridSize, Vector2Int startPos, Vector2Int bossPos)
        {
            Guid = System.Guid.NewGuid().ToString();
            CustomName = name;
            GridSize = gridSize;
            StartPosition = startPos;
            BossRoomPosition = bossPos;
            Rooms = new Dictionary<Vector2Int, RoomData>();
        }
    }
} 