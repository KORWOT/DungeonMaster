using DungeonMaster.Data;

namespace DungeonMaster.Buffs
{
    public interface IBuffEffect
    {
        void OnApply(BattleState state, ref BuffData buffData);
        void OnTick(BattleState state, ref BuffData buffData);
        void OnRemove(BattleState state, BuffData buffData);
        void OnReapply(BattleState state, ref BuffData buffData);
    }
} 