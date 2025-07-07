using DungeonMaster.Character;                   // [설명] StatType(공격력 등)와 관련된 타입 사용
using System.Collections.Generic;       // [설명] Dictionary(사전형 자료구조) 사용
using UnityEngine;

namespace DungeonMaster.Buffs           // [설명] 이 코드는 'DungeonMaster.Buffs' 그룹에 속함
{
    // [설명]
    // BuffEffectRegistry는
    // 모든 버프 효과(StatBuffEffect 등)를 'ID'별로 한 곳에 저장·관리하는 "버프 효과 등록소(레지스트리)"
    // 쉽게 말하면 "번호=효과"를 등록해두고 필요할 때 바로 꺼내쓸 수 있는 창고 같은 역할

    public static class BuffEffectRegistry // [설명] static = 인스턴스 없이 어디서나 접근 가능
    {
        // [설명]
        // long(숫자) 버프ID → IBuffEffect(버프 효과 객체)
        private static readonly Dictionary<long, IBuffEffect> Effects = new Dictionary<long, IBuffEffect>();

        /// <summary>
        /// BuffBlueprint 데이터를 기반으로 버프 효과를 동적으로 생성하고 등록합니다.
        /// </summary>
        public static void RegisterFromBlueprint(BuffBlueprint blueprint)
        {
            if (blueprint == null || Effects.ContainsKey(blueprint.BuffId))
            {
                return;
            }

            IBuffEffect effect = CreateEffectFromBlueprint(blueprint);
            if (effect != null)
            {
                Effects.Add(blueprint.BuffId, effect);
            }
        }
        
        /// <summary>
        /// Blueprint의 EffectType에 따라 적절한 IBuffEffect 인스턴스를 생성합니다.
        /// </summary>
        private static IBuffEffect CreateEffectFromBlueprint(BuffBlueprint blueprint)
        {
            switch (blueprint.EffectType)
            {
                case BuffEffectType.StatChange:
                    return new StatBuffEffect(blueprint);
                // 다른 버프 효과 타입에 대한 케이스를 여기에 추가합니다.
                // case BuffEffectType.DotDamage:
                //     return new DotDamageEffect(blueprint);
                default:
                    Debug.LogWarning($"ID {blueprint.BuffId} ({blueprint.BuffName})에 대한 '{blueprint.EffectType}' 타입의 버프 효과 생성 로직이 정의되지 않았습니다.");
                    return null;
            }
        }

        // [설명]
        // 등록된 버프 효과를 꺼내오는 함수
        // 만약 찾는 ID가 없으면 null 반환
        public static IBuffEffect Get(long buffId)
        {
            Effects.TryGetValue(buffId, out var effect);
            return effect; // 없으면 null
        }

        /// <summary>
        /// 테스트 또는 특정 상황을 위해 모든 등록 정보를 초기화합니다.
        /// </summary>
        public static void UnregisterAll()
        {
            Effects.Clear();
        }
    }
}
