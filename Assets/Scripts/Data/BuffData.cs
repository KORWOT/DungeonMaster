using System;

namespace DungeonMaster.Data
{
    [Serializable]
    public class BuffData
    {
        public long BuffId;
        public long CasterId;
        public long TargetId;
        public int RemainingDurationMs;
        public int Stacks;
        
        public BuffData(long buffId, long casterId, long targetId, int durationMs, int stacks = 1)
        {
            BuffId = buffId;
            CasterId = casterId;
            TargetId = targetId;
            RemainingDurationMs = durationMs;
            Stacks = stacks;
        }

        public BuffData Clone()
        {
            return new BuffData(BuffId, CasterId, TargetId, RemainingDurationMs, Stacks);
        }
    }
} 