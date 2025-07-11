using DungeonMaster.Data;
using DungeonMaster.Localization;
using DungeonMaster.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonMaster.Dungeon
{
    /// <summary>
    /// 던전의 생성, 편집, 저장, 로드, 플레이를 총괄하는 중앙 관리자입니다.
    /// </summary>
    public class DungeonManager : MonoBehaviour
    {
        public static DungeonManager Instance { get; private set; }

        public DungeonData CurrentDungeon { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 새로운 커스텀 던전 생성을 시작합니다.
        /// </summary>
        public void CreateNewDungeon(string name, Vector2Int gridSize, Vector2Int startPos, Vector2Int bossPos)
        {
            CurrentDungeon = new DungeonData(name, gridSize, startPos, bossPos);
            // TODO: 시작방과 보스방을 기본적으로 추가하는 로직
        }

        /// <summary>
        /// 저장된 던전 데이터를 불러옵니다.
        /// </summary>
        public void LoadDungeon(DungeonData data)
        {
            CurrentDungeon = data;
        }

        /// <summary>
        /// 현재 던전의 유효성을 검사하고 저장합니다.
        /// </summary>
        public bool TrySaveCurrentDungeon()
        {
            if (!ValidateDungeon())
            {
                GameLogger.LogError(LocalizationManager.Instance.GetText("dungeon_log_error_invalid_save"));
                return false;
            }
            
            UserDataManager.SaveCurrentDungeon(CurrentDungeon);
            return true;
        }

        /// <summary>
        /// 던전이 규칙에 맞게 구성되었는지 확인합니다 (예: 경로 연결).
        /// </summary>
        private bool ValidateDungeon()
        {
            if (CurrentDungeon == null) return false;

            // 1. 입구와 보스방이 존재하는지 확인
            if (!CurrentDungeon.Rooms.ContainsKey(CurrentDungeon.StartPosition) ||
                !CurrentDungeon.Rooms.ContainsKey(CurrentDungeon.BossRoomPosition))
            {
                GameLogger.LogError(LocalizationManager.Instance.GetText("dungeon_log_error_no_entry_or_boss"));
                return false;
            }
            
            // 2. 입구에서 보스방까지 길이 연결되어 있는지 확인 (경로 탐색)
            return IsPathValid();
        }

        /// <summary>
        /// 너비 우선 탐색(BFS)을 사용하여 입구에서 보스방까지 경로가 있는지 확인합니다.
        /// </summary>
        private bool IsPathValid()
        {
            var startNode = CurrentDungeon.StartPosition;
            var endNode = CurrentDungeon.BossRoomPosition;

            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();

            queue.Enqueue(startNode);
            visited.Add(startNode);

            while (queue.Count > 0)
            {
                var currentNode = queue.Dequeue();

                if (currentNode == endNode)
                {
                    return true; // 목적지 도착
                }

                if (CurrentDungeon.Rooms.TryGetValue(currentNode, out var roomData))
                {
                    foreach (var connection in roomData.Connections)
                    {
                        if (!visited.Contains(connection))
                        {
                            visited.Add(connection);
                            queue.Enqueue(connection);
                        }
                    }
                }
            }
            
            GameLogger.LogError(LocalizationManager.Instance.GetText("dungeon_log_error_no_path"));
            return false; // 목적지까지 경로 없음
        }

        #region Dungeon Customization API

        /// <summary>
        /// 지정된 위치에 새로운 방을 배치합니다.
        /// </summary>
        /// <param name="roomBlueprintId">배치할 방의 청사진 ID</param>
        /// <param name="position">배치할 위치</param>
        public bool PlaceRoom(string roomBlueprintId, Vector2Int position)
        {
            if (CurrentDungeon == null) return false;
            if (CurrentDungeon.Rooms.ContainsKey(position))
            {
                GameLogger.LogWarning(LocalizationManager.Instance.GetTextFormatted("dungeon_log_warn_room_exists", position));
                return false;
            }

            var newRoom = new RoomData(roomBlueprintId, position);
            CurrentDungeon.Rooms[position] = newRoom;
            
            var roomName = newRoom.Blueprint != null ? LocalizationManager.Instance.GetText(newRoom.Blueprint.NameKey) : "Unknown";
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("dungeon_log_info_room_placed", position, roomName));
            return true;
        }

        /// <summary>
        /// 지정된 위치의 방을 제거합니다.
        /// </summary>
        /// <param name="position">제거할 방의 위치</param>
        public bool RemoveRoom(Vector2Int position)
        {
            if (CurrentDungeon == null) return false;
            if (position == CurrentDungeon.StartPosition || position == CurrentDungeon.BossRoomPosition)
            {
                GameLogger.LogWarning(LocalizationManager.Instance.GetText("dungeon_log_warn_cant_remove_special"));
                return false;
            }

            if (CurrentDungeon.Rooms.Remove(position))
            {
                // 이 방을 향하는 모든 연결도 제거해야 합니다.
                foreach (var room in CurrentDungeon.Rooms.Values)
                {
                    room.Connections.RemoveAll(conn => conn == position);
                }
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("dungeon_log_info_room_removed", position));
                return true;
            }

            return false;
        }

        /// <summary>
        /// 두 방 사이의 길(연결)을 추가하거나 제거합니다. (양방향)
        /// </summary>
        public bool ToggleConnection(Vector2Int pos1, Vector2Int pos2)
        {
            if (CurrentDungeon == null) return false;

            if (CurrentDungeon.Rooms.TryGetValue(pos1, out var room1) &&
                CurrentDungeon.Rooms.TryGetValue(pos2, out var room2))
            {
                bool wasConnected = room1.Connections.Contains(pos2);

                if (wasConnected)
                {
                    // 연결 제거
                    room1.Connections.Remove(pos2);
                    room2.Connections.Remove(pos1);
                    GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("dungeon_log_info_connection_removed", pos1, pos2));
                }
                else
                {
                    // 연결 추가
                    room1.Connections.Add(pos2);
                    room2.Connections.Add(pos1);
                    GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("dungeon_log_info_connection_added", pos1, pos2));
                }
                return true;
            }

            GameLogger.LogWarning(LocalizationManager.Instance.GetTextFormatted("dungeon_log_warn_room_not_exist", pos1, pos2));
            return false;
        }

        #endregion

        #region Room Growth API

        /// <summary>
        /// 특정 방에 경험치를 추가하고 레벨업을 처리합니다.
        /// </summary>
        public void AddXpToRoom(Vector2Int position, long xpAmount)
        {
            if (CurrentDungeon == null || !CurrentDungeon.Rooms.TryGetValue(position, out var room)) return;

            var blueprint = room.Blueprint;
            if (blueprint == null || room.Level >= blueprint.MaxLevel) return;

            room.CurrentXP += xpAmount;

            var blueprintName = LocalizationManager.Instance.GetText(blueprint.NameKey);
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("dungeon_log_info_xp_gained", blueprintName, position, xpAmount, room.CurrentXP));

            // 레벨업 체크
            long requiredXp = (long)blueprint.XpCurve.Evaluate(room.Level + 1);
            while (room.CurrentXP >= requiredXp && room.Level < blueprint.MaxLevel)
            {
                LevelUpRoom(room);
                requiredXp = (long)blueprint.XpCurve.Evaluate(room.Level + 1);
            }
        }

        /// <summary>
        /// 방을 레벨업 시킵니다.
        /// </summary>
        private void LevelUpRoom(RoomData room)
        {
            room.Level++;
            var roomName = room.Blueprint != null ? LocalizationManager.Instance.GetText(room.Blueprint.NameKey) : "Unknown";
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("dungeon_log_info_level_up", roomName, room.Level));
            // TODO: 레벨업 시 발생하는 이벤트 (VFX, 사운드 등) 호출
        }

        /// <summary>
        /// 방의 레벨에 따라 효과의 최종 수치를 계산하여 반환합니다.
        /// </summary>
        public float GetScaledEffectValue(RoomEffectBlueprint effectBlueprint, int roomLevel)
        {
            if (effectBlueprint == null) return 0f;

            var scaling = effectBlueprint.ValueScaling;
            var roomBlueprint = GetBlueprintForEffect(effectBlueprint); // 이 효과를 소유한 방 청사진 찾기
            int maxLevel = roomBlueprint != null ? roomBlueprint.MaxLevel : 10;
            
            return scaling.CalculateScaling(roomLevel, maxLevel);
        }
        
        // TODO: 이 메서드는 BlueprintDatabase 같은 곳으로 옮겨져야 할 수 있습니다.
        private RoomBlueprint GetBlueprintForEffect(RoomEffectBlueprint effectBlueprint)
        {
            // 임시 구현: 모든 RoomBlueprint을 뒤져서 찾아냅니다. 성능에 좋지 않습니다.
            // 실제로는 Blueprint 데이터베이스를 통해 더 효율적으로 찾아야 합니다.
            var allRoomBlueprints = Resources.FindObjectsOfTypeAll<RoomBlueprint>();
            foreach(var roomBP in allRoomBlueprints)
            {
                if (roomBP.Effects.Contains(effectBlueprint))
                {
                    return roomBP;
                }
            }
            return null;
        }

        #endregion

        #region Battle Integration

        /// <summary>
        /// 특정 방의 전투를 위한 BattleLaunchData를 생성합니다.
        /// </summary>
        /// <param name="roomPosition">전투가 벌어질 방의 위치</param>
        /// <param name="attackerGuids">공격해오는 적 몬스터들의 Guid 목록</param>
        /// <returns>BattleManager가 사용할 수 있는 전투 시작 데이터</returns>
        public Battle.BattleLaunchData CreateBattleLaunchData(Vector2Int roomPosition, List<string> attackerGuids)
        {
            if (CurrentDungeon == null || !CurrentDungeon.Rooms.TryGetValue(roomPosition, out var roomData))
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("dungeon_log_error_room_not_found", roomPosition));
                return null;
            }

            // 수비팀 (방에 배치된 몬스터)
            var defenders = new List<Battle.ParticipantData>();
            foreach (var monsterGuid in roomData.PlacedMonsterGuids)
            {
                var userCard = UserDataManager.CurrentUserData.CardCollection.GetCardByGuid(monsterGuid);
                if (userCard != null)
                {
                    var blueprint = BlueprintDatabase.Instance.GetBlueprint(userCard.BlueprintId);
                    if (blueprint != null)
                    {
                        defenders.Add(new Battle.ParticipantData(blueprint, userCard));
                    }
                }
            }

            // 공격팀 (외부에서 온 몬스터)
            var attackers = new List<Battle.ParticipantData>();
            foreach (var monsterGuid in attackerGuids)
            {
                var userCard = UserDataManager.CurrentUserData.CardCollection.GetCardByGuid(monsterGuid); // 실제로는 적 팩션의 카드 목록에서 찾아야 함
                if (userCard != null)
                {
                    var blueprint = BlueprintDatabase.Instance.GetBlueprint(userCard.BlueprintId);
                    if (blueprint != null)
                    {
                        attackers.Add(new Battle.ParticipantData(blueprint, userCard));
                    }
                }
            }
            
            // BattleManager의 컨텍스트에서 'Player'는 수비팀, 'Enemy'는 공격팀이 됩니다.
            return new Battle.BattleLaunchData(playerMonsters: defenders, enemyMonsters: attackers);
        }

        #endregion
    }
} 