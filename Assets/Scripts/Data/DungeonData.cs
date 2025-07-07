using System.Collections.Generic;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 단일 전투(Dungeon)에 필요한 모든 초기 데이터를 담는 컨테이너입니다.
    /// 이 데이터는 BattleManager에게 전달되어 전투를 시작하는 데 사용됩니다.
    /// </summary>
    public class DungeonData
    {
        public List<ParticipantData> PlayerMonsters { get; private set; }
        public List<ParticipantData> EnemyMonsters { get; private set; }

        public DungeonData(List<ParticipantData> playerMonsters, List<ParticipantData> enemyMonsters)
        {
            PlayerMonsters = playerMonsters;
            EnemyMonsters = enemyMonsters;
        }
    }

    /// <summary>
    /// 개별 전투 참가자의 초기 데이터를 정의합니다.
    /// </summary>
    public class ParticipantData
    {
        public CardBlueprintData Blueprint { get; private set; }
        public int Level { get; private set; }
        public UserCardData UserCard { get; private set; }
        // TODO: 장착한 장비 ID 목록, 강화된 스킬 정보 등을 여기에 추가할 수 있습니다.

        public ParticipantData(CardBlueprintData blueprint, int level, UserCardData userCard)
        {
            Blueprint = blueprint;
            Level = level;
            UserCard = userCard;
        }
    }
} 