using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Character;
using UnityEngine;

namespace DungeonMaster.Data
{
    [CreateAssetMenu(fileName = "ElementalAffinityTable", menuName = "Game/Data/Elemental Affinity Table")]
    public class ElementalAffinityTable : ScriptableObject
    {
        [SerializeField] private List<ElementalAffinity> affinities;

        private Dictionary<(ElementType, ElementType), long> _affinityMap;

        private void OnEnable()
        {
            _affinityMap = new Dictionary<(ElementType, ElementType), long>();
            if (affinities == null) return;
            
            foreach (var affinity in affinities)
            {
                _affinityMap[(affinity.AttackingElement, affinity.DefendingElement)] = (long)(affinity.DamageMultiplier * 100); // float를 정수로 변환 (100 기준)
            }
        }

        public long GetMultiplier(ElementType attacker, ElementType defender)
        {
            if (_affinityMap == null) OnEnable();

            if (_affinityMap.TryGetValue((attacker, defender), out long multiplier))
            {
                return multiplier;
            }

            // 상성표에 정의되지 않은 경우 기본값 100 (1.0배) 반환
            return 100;
        }
    }
} 