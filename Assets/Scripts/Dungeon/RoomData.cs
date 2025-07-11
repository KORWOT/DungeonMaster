using UnityEngine;
using System.Collections.Generic;

namespace DungeonMaster.Dungeon
{
    /// <summary>
    /// 플레이어의 던전에 실제로 배치된 방의 상태 정보를 저장하는 데이터 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class RoomData
    {
        public string Guid { get; private set; }
        public string BlueprintId { get; private set; }
        public Vector2Int Position { get; set; }
        public int Level { get; set; }
        public long CurrentXP { get; set; }

        [Range(0f, 1f)]
        public float Integrity { get; set; } // 내구도 (0.0 ~ 1.0)

        public List<string> PlacedMonsterGuids { get; private set; }
        public List<Vector2Int> Connections { get; private set; }

        private RoomBlueprint _blueprint;
        public RoomBlueprint Blueprint
        {
            get
            {
                // TODO: BlueprintDatabase를 통해 BlueprintId에 해당하는 RoomBlueprint 로드
                if (_blueprint == null)
                {
                    // _blueprint = BlueprintDatabase.Get<RoomBlueprint>(BlueprintId);
                }
                return _blueprint;
            }
        }

        public RoomData(string blueprintId, Vector2Int position)
        {
            Guid = System.Guid.NewGuid().ToString();
            BlueprintId = blueprintId;
            Position = position;
            Level = 1;
            CurrentXP = 0;
            Integrity = 1.0f;
            PlacedMonsterGuids = new List<string>();
            Connections = new List<Vector2Int>();
        }
    }
} 