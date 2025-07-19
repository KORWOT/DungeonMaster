using UnityEngine;
using DungeonMaster.Data;
using DungeonMaster.Battle;
using System.Collections.Generic;
using DungeonMaster.Character;

namespace DungeonMaster.Character
{
    /// <summary>
    /// 몬스터의 뷰(View)가 구현해야 하는 인터페이스입니다.
    /// 이 인터페이스는 '데이터의 시각적 표현'과 관련된 책임을 가집니다.
    /// </summary>
    public interface ICharacter : ICombatant
    {
        /// <summary>
        /// 캐릭터의 현재 스탯 정보에 접근합니다.
        /// </summary>
        Dictionary<StatType, long> Stats { get; }
        
        /// <summary>
        /// 캐릭터의 이름에 접근합니다.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 뷰를 초기화합니다. 전투 시작 시 한 번만 호출됩니다.
        /// </summary>
        /// <param name="initialData">캐릭터의 초기 전투 상태 데이터</param>
        void Initialize(object initialData);

        // --- 이벤트 기반 뷰 업데이트 ---
        
        /// <summary>
        /// 데미지를 받았을 때의 시각/청각적 효과를 재생합니다.
        /// </summary>
        void OnDamageReceived(long damage);
        
        /// <summary>
        /// 치유를 받았을 때의 시각/청각적 효과를 재생합니다.
        /// </summary>
        void OnHealed(long healAmount);
        
        /// <summary>
        /// 버프를 받았을 때의 시각/청각적 효과를 재생합니다.
        /// </summary>
        void OnBuffApplied(long buffId);
        
        /// <summary>
        /// 버프가 제거되었을 때의 시각/청각적 효과를 재생합니다.
        /// </summary>
        void OnBuffRemoved(long buffId);
        
        /// <summary>
        /// 죽었을 때의 시각/청각적 효과를 재생합니다.
        /// </summary>
        void OnDeath();
    }
}

