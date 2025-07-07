using DungeonMaster.Character;
using UnityEngine;

namespace DungeonMaster.Data
{
    [System.Serializable]
    public class CharacterPrefab
    {
        [Tooltip("프리팹을 식별하는 고유 ID. CardBlueprintData의 ID와 일치시켜야 합니다.")]
        public long ID;
        
        [Tooltip("해당 ID에 매핑될 캐릭터 프리팹")]
        public ClientMonster Prefab;
    }
} 