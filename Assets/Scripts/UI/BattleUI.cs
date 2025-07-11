using DungeonMaster.Localization;
using DungeonMaster.Utility;
using UnityEngine;

namespace DungeonMaster.UI
{
    /// <summary>
    /// 전투 관련 UI 요소들을 관리하는 클래스입니다.
    /// 실제 로직은 추후에 구현될 예정입니다.
    /// </summary>
    public class BattleUI : MonoBehaviour
    {
        public void ShowBattleStart()
        {
            GameLogger.LogInfo(LocalizationManager.Instance.GetText("log_info_battle_ui_start"));
        }

        public void UpdateTurnUI(int turnCount)
        {
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_info_battle_ui_turn", turnCount));
        }

        public void ShowBattleEnd(object battleState) // BattleState 타입을 직접 참조하지 않기 위해 object 사용
        {
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_info_battle_ui_end", battleState));
        }
    }
} 