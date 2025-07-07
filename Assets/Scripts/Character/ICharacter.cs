using UnityEngine;
using DungeonMaster.Data;
using DungeonMaster.Battle;
using System.Collections.Generic;
using DungeonMaster.Character;

namespace DungeonMaster.Character
{
    /// <summary>
    /// 게임 내 모든 캐릭터(몬스터, 플레이어 등)의 뷰(View)가 구현해야 하는 인터페이스입니다.
    /// 이 인터페이스는 '데이터의 시각적 표현'에 대한 책임만을 가집니다.
    /// </summary>
    public interface ICharacter
    {
        /// <summary>
        /// 뷰를 초기화합니다. 전투 시작 시 한 번만 호출됩니다.
        /// </summary>
        /// <param name="initialData">캐릭터의 초기 전투 상태 데이터</param>
        void Initialize(DeterministicCharacterData initialData);

        /// <summary>
        /// 매 틱마다 최신 상태를 뷰에 적용합니다.
        /// </summary>
        /// <param name="data">적용할 최신 데이터</param>
        void ApplyState(DeterministicCharacterData data);
        
        /// <summary>
        /// 데미지를 받았을 때의 시각/청각적 효과를 재생합니다.
        /// </summary>
        void PlayTakeDamageEffect();

        /// <summary>
        /// 죽었을 때의 시각/청각적 효과를 재생합니다.
        /// </summary>
        void PlayDeathEffect();

        /// <summary>
        /// 공격 시의 시각/청각적 효과를 재생합니다.
        /// </summary>
        void PlayAttackEffect();

        // ✅ 누락된 속성들 추가
        long InstanceId { get; }
        string Name { get; }
        Dictionary<StatType, long> Stats { get; }

        // --- 이벤트 기반 뷰 업데이트 ---
        void OnDamageReceived(long damage);
        void OnHealed(long healAmount);
        void OnBuffApplied(long buffId);
        void OnBuffRemoved(long buffId);
        void OnDeath();
    }
}

