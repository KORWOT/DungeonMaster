using DungeonMaster.Data;
using System.Collections.Generic;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// 단일 전투를 시작하는 데 필요한 모든 초기 데이터를 담는 컨테이너입니다.
    /// </summary>
    public class BattleLaunchData
    {
        public List<ParticipantData> PlayerMonsters { get; private set; }
        public List<ParticipantData> EnemyMonsters { get; private set; }

        public BattleLaunchData(List<ParticipantData> playerMonsters, List<ParticipantData> enemyMonsters)
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
        public UserCardData UserCard { get; private set; }

        public ParticipantData(CardBlueprintData blueprint, UserCardData userCard)
        {
            Blueprint = blueprint;
            UserCard = userCard;
        }
    }
} 