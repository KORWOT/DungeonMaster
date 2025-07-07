using DungeonMaster.Character;
using System;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 두 속성 간의 상성 관계와 데미지 배율을 정의합니다.
    /// </summary>
    [Serializable]
    public struct ElementalAffinity
    {
        public ElementType AttackingElement;
        public ElementType DefendingElement;
        public float DamageMultiplier;
    }
} 