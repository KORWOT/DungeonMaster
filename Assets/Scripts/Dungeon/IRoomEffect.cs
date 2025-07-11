namespace DungeonMaster.Dungeon
{
    /// <summary>
    /// 모든 방 효과가 구현해야 하는 기본 인터페이스입니다.
    /// </summary>
    public interface IRoomEffect
    {
        /// <summary>
        /// 이 효과의 청사진 데이터를 반환합니다.
        /// </summary>
        RoomEffectBlueprint Blueprint { get; }

        /// <summary>
        /// 특정 트리거가 발생했을 때 효과를 실행합니다.
        /// </summary>
        /// <param name="trigger">발생한 트리거 종류</param>
        /// <param name="context">효과 실행에 필요한 모든 정보(대상, 방 상태 등)</param>
        void Execute(RoomEffectTrigger trigger, RoomEffectContext context);
    }
} 