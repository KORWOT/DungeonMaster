namespace DungeonMaster.Battle
{
    /// <summary>
    /// 전투 중에 발생하여 뷰(View)에 전달될 단일 이벤트를 나타냅니다.
    /// (예: '캐릭터A가 10의 피해를 입었다')
    /// </summary>
    public struct BattleEvent
    {
        /// <summary>
        /// 이벤트 타입 (데미지, 힐, 버프 등)
        /// </summary>
        public readonly BattleEventType Type;
        
        /// <summary>
        /// 이벤트의 대상이 된 캐릭터의 인스턴스 ID
        /// </summary>
        public readonly long TargetId;
        
        /// <summary>
        /// 이벤트와 관련된 값 (예: 데미지량, 힐량, 버프 ID)
        /// </summary>
        public readonly long Value;

        /// <summary>
        /// 버프/디버프의 현재 스택 정보
        /// </summary>
        public readonly int Stacks;

        public BattleEvent(BattleEventType type, long targetId, long value = 0, int stacks = 0)
        {
            Type = type;
            TargetId = targetId;
            Value = value;
            Stacks = stacks;
        }
    }
} 