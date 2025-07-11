using UnityEngine;

namespace DungeonMaster.Dungeon
{
    [CreateAssetMenu(fileName = "DungeonConfig", menuName = "DungeonMaster/Dungeon Config", order = 0)]
    public class DungeonConfig : ScriptableObject
    {
        [Header("New Dungeon Settings")]
        [Tooltip("The localization key for the default dungeon name.")]
        public string DefaultDungeonNameKey = "default_dungeon_name";
        
        [Tooltip("Default grid size for a new dungeon.")]
        public Vector2Int DefaultGridSize = new Vector2Int(3, 3);
        
        [Tooltip("The Blueprint ID for the starting room.")]
        public string StartRoomBlueprintId;

        [Tooltip("The Blueprint ID for the boss room.")]
        public string BossRoomBlueprintId;

        [Tooltip("Default position for the start room.")]
        public Vector2Int DefaultStartPosition = new Vector2Int(2, 1);
        
        [Tooltip("Default position for the boss room.")]
        public Vector2Int DefaultBossPosition = new Vector2Int(0, 1);
    }
} 