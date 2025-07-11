using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DungeonMaster.Data;
using DungeonMaster.Battle;
using DungeonMaster.Utility;
using System.Collections.Generic;
using DungeonMaster.Localization;

namespace DungeonMaster.Character
{
    /// <summary>
    /// 씬에 실제로 배치되는 몬스터의 '뷰(View)' 역할을 하는 클래스입니다.
    /// 이 클래스는 ICharacter 인터페이스를 구현하며, 데이터의 시각적 표현만을 담당합니다.
    /// 모든 로직과 상태는 외부에서 주입되는 DeterministicCharacterData에 의해 관리됩니다.
    /// </summary>
    public class ClientMonster : MonoBehaviour, ICharacter
    {
        [Header("컴포넌트")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Slider hpSlider;
        
        /// <summary>
        /// 이 뷰가 어떤 프리팹으로부터 생성되었는지를 식별하는 ID.
        /// CharacterViewFactory의 오브젝트 풀링에 사용됩니다.
        /// </summary>
        public long PrefabID { get; set; }

        // --- ICombatant 구현 ---
        public long InstanceId => _data?.InstanceId ?? 0;
        public bool IsPlayer => _data?.IsPlayer ?? false;
        public float CurrentHp => _data?.CurrentHP ?? 0;
        public object StateData => _data;
        
        public void ApplyState(object data)
        {
            if (data is DeterministicCharacterData characterData)
            {
                ApplyState(characterData);
            }
        }
        
        // --- ICharacter 구현 ---
        public string Name => _data?.Name ?? LocalizationManager.Instance.GetText("monster_name_unknown");
        public Dictionary<StatType, long> Stats => _data?.Stats;

        /// <summary>
        /// 이 뷰가 렌더링하는 대상 데이터입니다. BattleManager에 의해 외부에서 주입되고 갱신됩니다.
        /// </summary>
        private DeterministicCharacterData _data;

        public void Initialize(object initialData)
        {
            if (initialData is DeterministicCharacterData characterData)
            {
                Initialize(characterData);
            }
        }
        
        // --- 기존 로직 (내부 구현) ---
        private void Initialize(DeterministicCharacterData initialData)
        {
            // 오브젝트 풀링으로 재사용될 때를 대비해 항상 활성화 상태로 시작합니다.
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            // 이름, 레벨 등 전투 중에 변하지 않는 초기 정보를 설정합니다.
            if (nameText != null)
            {
                nameText.text = LocalizationManager.Instance.GetTextFormatted("ui_name_level", initialData.Name, initialData.Level);
            }

            // ApplyState를 호출하여 HP바 등 변하는 정보의 초기 상태를 설정합니다.
            ApplyState(initialData);
        }
        
        private void ApplyState(DeterministicCharacterData data)
        {
            _data = data;
            // HP 바 업데이트
            if (hpSlider != null)
            {
                hpSlider.maxValue = data.Stats.GetValueOrDefault(StatType.MaxHP, 1);
                hpSlider.value = data.CurrentHP;
            }
            
            if (data.CurrentHP <= 0)
            {
                PlayDeathEffect();
            }
        }

        /// <summary>
        /// ICharacter 인터페이스 구현: 데미지 시각 효과를 재생합니다.
        /// </summary>
        public void PlayTakeDamageEffect()
        {
            // 향후 피격 시 색상 변경, 파티클 효과 등 구현 예정
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_monster_damaged", nameText.text));
        }

        /// <summary>
        /// ICharacter 인터페이스 구현: 죽음 시각 효과를 재생합니다.
        /// </summary>
        public void PlayDeathEffect()
        {
            if (gameObject.activeInHierarchy)
            {
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_monster_died", nameText.text));
                gameObject.SetActive(false); // 간단하게 비활성화 처리
            }
        }

        public void PlayAttackEffect()
        {
            // 향후 공격 애니메이션, 사운드 등 구현 예정
        }
        
        // --- 이벤트 기반 뷰 업데이트 구현 ---

        public void OnDamageReceived(long damage)
        {
            // 데미지 텍스트 팝업, 피격 애니메이션 재생, 사운드 재생 등
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_damage_received", Name, damage));
            // PlayHitAnimation();
            // ShowDamageText(damage);
        }

        public void OnHealed(long healAmount)
        {
            // 힐 이펙트 재생, 힐 텍스트 팝업 등
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_healed", Name, healAmount));
            // PlayHealEffect();
        }

        public void OnBuffApplied(long buffId)
        {
            // 버프 아이콘 생성/업데이트, 버프 이펙트 재생 등
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_buff_applied", Name, buffId));
            // _buffUIManager.AddBuffIcon(buffId);
        }

        public void OnBuffRemoved(long buffId)
        {
            // 버프 아이콘 제거 등
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_buff_removed", Name, buffId));
            // _buffUIManager.RemoveBuffIcon(buffId);
        }

        public void OnDeath()
        {
            // 사망 애니메이션 재생, 모델 비활성화 등
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_death", Name));
            // PlayDeathAnimation();
            // gameObject.SetActive(false);
        }
    }
} 