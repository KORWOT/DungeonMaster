using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 게임 시작 시 유저에게 기본 카드를 지급하는 역할을 담당합니다.
    /// </summary>
    public static class StarterCardProvider
    {
        /// <summary>
        /// 유저가 시작 카드를 모두 보유하고 있는지 확인하고, 없는 경우 지급합니다.
        /// </summary>
        /// <param name="userData">확인할 유저의 데이터</param>
        /// <returns>카드가 지급되었으면 true, 아니면 false</returns>
        public static bool EnsureStarterCards(UserData userData)
        {
            if (userData == null)
            {
                Debug.LogError("UserData가 null입니다! 시작 카드를 지급할 수 없습니다.");
                return false;
            }
            
            var starterBlueprints = BlueprintDatabase.Instance.GetStarterBlueprints().ToList();
            if (starterBlueprints.Count == 0)
            {
                Debug.LogWarning("데이터베이스에 시작 카드로 설정된 설계도가 없습니다.");
                return false;
            }

            var collection = userData.CardCollection;
            bool needsStarterCards = starterBlueprints.Any(bp => !collection.HasCard(bp.BlueprintId));

            if (needsStarterCards)
            {
                Debug.Log("시작 카드를 지급합니다...");
                int cardsGiven = 0;
                foreach (var blueprint in starterBlueprints)
                {
                    if (!collection.HasCard(blueprint.BlueprintId))
                    {
                        collection.AcquireCard(blueprint.BlueprintId);
                        cardsGiven++;
                        Debug.Log($"시작 카드 지급: {blueprint.Name} (ID: {blueprint.BlueprintId})");
                    }
                }
                
                if (cardsGiven > 0)
                {
                    Debug.Log($"총 {cardsGiven}개의 시작 카드가 지급되었습니다.");
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// 유저의 시작 카드 보유 현황을 확인합니다.
        /// </summary>
        /// <param name="userData">확인할 유저의 데이터</param>
        /// <returns>(보유 수, 전체 수)</returns>
        public static (int given, int total) GetStarterCardStatus(UserData userData)
        {
            if (userData == null) return (0, 0);

            var starterBlueprints = BlueprintDatabase.Instance.GetStarterBlueprints().ToList();
            int total = starterBlueprints.Count;
            if (total == 0) return (0, 0);

            var collection = userData.CardCollection;
            int given = starterBlueprints.Count(bp => collection.HasCard(bp.BlueprintId));
            
            return (given, total);
        }
    }
} 