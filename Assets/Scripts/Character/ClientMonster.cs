using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DungeonMaster.Data;
using DungeonMaster.Battle;
using DungeonMaster.Utility;
using System.Collections.Generic;

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

        /// <summary>
        /// ICharacter 인터페이스 구현: 이 뷰가 표현하는 데이터의 고유 ID를 반환합니다.
        /// </summary>
        public long InstanceId => _data?.InstanceId ?? 0;

        public string Name => _data?.Name ?? "Unknown";

        public Dictionary<StatType, long> Stats => _data?.Stats;

        /// <summary>
        /// 이 뷰가 렌더링하는 대상 데이터입니다. BattleManager에 의해 외부에서 주입되고 갱신됩니다.
        /// </summary>
        private DeterministicCharacterData _data;

        /// <summary>
        /// ICharacter 인터페이스 구현: BattleManager가 전투 시작 시 이 뷰와 데이터를 연결하기 위해 호출합니다.
        /// </summary>
        public void Initialize(DeterministicCharacterData initialData)
        {
            // 오브젝트 풀링으로 재사용될 때를 대비해 항상 활성화 상태로 시작합니다.
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            // 이름, 레벨 등 전투 중에 변하지 않는 초기 정보를 설정합니다.
            if (nameText != null)
            {
                nameText.text = $"{initialData.Name} (Lv.{initialData.Level})";
            }

            // ApplyState를 호출하여 HP바 등 변하는 정보의 초기 상태를 설정합니다.
            ApplyState(initialData);
        }

        /// <summary>
        /// ICharacter 인터페이스 구현: BattleManager가 매 틱마다 최신 상태를 뷰에 반영하기 위해 호출합니다.
        /// </summary>
        public void ApplyState(DeterministicCharacterData data)
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
            Debug.Log($"{nameText.text} received damage!");
        }

        /// <summary>
        /// ICharacter 인터페이스 구현: 죽음 시각 효과를 재생합니다.
        /// </summary>
        public void PlayDeathEffect()
        {
            if (gameObject.activeInHierarchy)
            {
                Debug.Log($"{nameText.text} has died!");
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
            Debug.Log($"{_data.Name}이(가) {damage}의 데미지를 받았습니다! (View)");
            // PlayHitAnimation();
            // ShowDamageText(damage);
        }

        public void OnHealed(long healAmount)
        {
            // 힐 이펙트 재생, 힐 텍스트 팝업 등
            Debug.Log($"{_data.Name}이(가) {healAmount}만큼 회복했습니다! (View)");
            // PlayHealEffect();
        }

        public void OnBuffApplied(long buffId)
        {
            // 버프 아이콘 생성/업데이트, 버프 이펙트 재생 등
            Debug.Log($"{_data.Name}에게 버프 {buffId}가 적용되었습니다! (View)");
            // _buffUIManager.AddBuffIcon(buffId);
        }

        public void OnBuffRemoved(long buffId)
        {
            // 버프 아이콘 제거 등
            Debug.Log($"{_data.Name}의 버프 {buffId}가 제거되었습니다! (View)");
            // _buffUIManager.RemoveBuffIcon(buffId);
        }

        public void OnDeath()
        {
            // 사망 애니메이션 재생, 모델 비활성화 등
            Debug.Log($"{_data.Name}이(가) 사망했습니다! (View)");
            // PlayDeathAnimation();
            // gameObject.SetActive(false);
        }
    }
} 