using UnityEngine;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 결정론적 전투 시스템의 핵심 규칙과 상수들을 정의하는 ScriptableObject 입니다.
    /// 이 설정을 통해 게임 디자이너는 코드 변경 없이 전투의 세부 밸런스를 조정할 수 있습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "DeterministicBattleSettings", menuName = "Game/Configuration/Deterministic Battle Settings")]
    public class DeterministicBattleSettingsData : ScriptableObject
    {
        [Header("기본 전투 규칙")]
        [Tooltip("공격이 방어에 막혀도 보장되는 최소 데미지")]
        public long MinimumDamage = 1;

        [Tooltip("치명타 확률은 0부터 10000 사이의 값으로 표현됩니다 (100.00%).")]
        public int MaxCritChance_x10000 = 10000;
        
        // 여기에 더 많은 전투 규칙들을 추가할 수 있습니다.
        // 예: 방어력 공식 변수, 속성 상성 배율, 회피 공식 변수 등
    }
} 