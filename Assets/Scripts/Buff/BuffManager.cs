using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Data;
using UnityEngine;

namespace DungeonMaster.Buffs
{
    /// <summary>
    /// 모든 BuffBlueprint을 관리하고 BuffData 인스턴스 생성을 책임지는 싱글턴 클래스입니다.
    /// </summary>
    public class BuffManager : MonoBehaviour
    {
        public static BuffManager Instance { get; private set; }

        private Dictionary<long, BuffBlueprint> _buffBlueprints = new Dictionary<long, BuffBlueprint>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadBuffs();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadBuffs()
        {
            var buffs = Resources.LoadAll<BuffBlueprint>("Buffs");
            _buffBlueprints = buffs.ToDictionary(b => b.BuffId, b => b);
            
            // 레지스트리 초기화 후 블루프린트를 기반으로 자동 등록
            BuffEffectRegistry.UnregisterAll();
            foreach (var blueprint in _buffBlueprints.Values)
            {
                BuffEffectRegistry.RegisterFromBlueprint(blueprint);
            }
            
            Debug.Log($"{_buffBlueprints.Count}개의 버프 블루프린트를 로드하고, 레지스트리에 등록했습니다.");
        }

        public BuffBlueprint GetBlueprint(long buffId)
        {
            _buffBlueprints.TryGetValue(buffId, out var blueprint);
            return blueprint;
        }

        /// <summary>
        /// 버프 ID와 시전자 정보를 바탕으로 새로운 BuffData 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="buffId">생성할 버프의 ID</param>
        /// <param name="caster">버프 시전자</param>
        /// <param name="target">버프 대상</param>
        /// <returns>생성된 BuffData 인스턴스. 유효하지 않으면 null.</returns>
        public BuffData CreateBuffInstance(long buffId, DeterministicCharacterData caster, DeterministicCharacterData target)
        {
            if (!_buffBlueprints.TryGetValue(buffId, out var blueprint))
            {
                Debug.LogWarning($"ID가 {buffId}인 버프 블루프린트를 찾을 수 없습니다.");
                return null;
            }

            // 스케일링 로직을 적용하여 최종 지속시간 계산 (초기 레벨 = 1)
            int durationMs = (int)(blueprint.GetScaledDuration(1) * 1000);

            var buffData = new BuffData(
                blueprint.BuffId,
                caster.InstanceId,
                target.InstanceId,
                durationMs,
                1 // 초기 스택은 1로 설정
            );

            return buffData;
        }
    }
} 