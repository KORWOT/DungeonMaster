using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Localization;
using DungeonMaster.Utility;
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
                GameLogger.LogError(LocalizationManager.Instance.GetText("error_null_user_data_for_starter_cards"));
                return false;
            }
            
            var starterBlueprints = BlueprintDatabase.Instance.GetStarterBlueprints().ToList();
            if (starterBlueprints.Count == 0)
            {
                GameLogger.LogWarning(LocalizationManager.Instance.GetText("warn_no_starter_blueprints"));
                return false;
            }

            var collection = userData.CardCollection;
            bool needsStarterCards = starterBlueprints.Any(bp => !collection.HasCard(bp.BlueprintId));

            if (needsStarterCards)
            {
                GameLogger.LogInfo(LocalizationManager.Instance.GetText("info_providing_starter_cards"));
                int cardsGiven = 0;
                foreach (var blueprint in starterBlueprints)
                {
                    if (!collection.HasCard(blueprint.BlueprintId))
                    {
                        collection.AcquireCard(blueprint.BlueprintId);
                        cardsGiven++;
                        GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("info_starter_card_provided", blueprint.Name, blueprint.BlueprintId));
                    }
                }
                
                if (cardsGiven > 0)
                {
                    GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("info_total_starter_cards_provided", cardsGiven));
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