using DungeonMaster.Data;          // [설명] BuffData, BattleState 등 데이터 구조 불러옴
using DungeonMaster.Character;       // [설명] StatType(공격력/방어력 등 스탯 종류) 정의
using System.Linq;                 // [설명] FirstOrDefault(조건에 맞는 첫 요소 찾기) 등 LINQ 기능 사용
using DungeonMaster.Utility;

namespace DungeonMaster.Buffs      // [설명] 이 코드는 'DungeonMaster.Buffs'라는 그룹(네임스페이스)에 속함
{
    // [설명]
    // StatBuffEffect는 '스탯 버프 효과(능력치 일시 증가/감소)'를 담당하는 클래스
    // 예) "방어력 +20 버프", "공격력 -10 디버프" 등

    public class StatBuffEffect : IBuffEffect   // [설명] IBuffEffect 인터페이스(=버프 효과의 규약)를 반드시 따라야 함
    {
        private readonly BuffBlueprint _blueprint;

        public StatBuffEffect(BuffBlueprint blueprint)
        {
            _blueprint = blueprint;
        }

        // [설명]
        // 버프가 "처음 적용"될 때 호출됨
        // BattleState: 현재 전투 상태(모든 캐릭터, 버프 등 포함)
        public void OnApply(BattleState state, ref BuffData buffData)
        {
            var target = state.GetCharacter(buffData.TargetId);
            if (target == null) return;
            
            // 첫 적용 시, 레벨 1에 해당하는 스케일링된 값을 적용합니다.
            long value = _blueprint.GetScaledValue((int)1);
            target.Stats.AddValue(_blueprint.TargetStat, value);
        }

        // [설명] 버프가 유지되는 매 턴마다(틱마다) 호출
        // 스탯 버프는 매 턴 변화 없이 한 번만 적용 → 아무것도 안함
        public void OnTick(BattleState state, ref BuffData buffData)
        {
            // 지속적인 틱 효과가 필요하다면 여기에 구현
        }

        // [설명]
        // 버프가 "사라질 때"(해제, 만료 등) 호출
        // 적용했던 _amount 만큼 다시 빼서 원상복구
        public void OnRemove(BattleState state, BuffData buffData)
        {
            var target = state.GetCharacter(buffData.TargetId);
            if (target == null) return;

            // 버프 제거 시, 현재 스택 레벨에 해당하는 총 스케일링된 값을 제거합니다.
            long totalValue = _blueprint.GetScaledValue((int)buffData.Stacks);
            target.Stats.AddValue(_blueprint.TargetStat, -totalValue);
        }
        
        public void OnReapply(BattleState state, ref BuffData buffData)
        {
            var target = state.GetCharacter(buffData.TargetId);
            if (target == null) return;

            if (buffData.Stacks <= 1) // 재적용이지만 스택이 1이면 OnApply와 동일
            {
                OnApply(state, ref buffData);
                return;
            }

            // 이전 스택과 현재 스택의 값 차이만 계산하여 적용합니다.
            long previousValue = _blueprint.GetScaledValue((int)buffData.Stacks - 1);
            long currentValue = _blueprint.GetScaledValue((int)buffData.Stacks);
            long diff = currentValue - previousValue;
            
            target.Stats.AddValue(_blueprint.TargetStat, diff);
        }
    }
}
