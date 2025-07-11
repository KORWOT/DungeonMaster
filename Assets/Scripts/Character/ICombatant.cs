namespace DungeonMaster.Character
{
    /// <summary>
    /// 전투에 참여하는 모든 개체(몬스터, 마왕 등)를 위한 최상위 인터페이스입니다.
    /// 전투의 '데이터' 측면을 대표합니다.
    /// </summary>
    public interface ICombatant
    {
        /// <summary>
        /// 전투 중 이 인스턴스를 식별하는 고유 ID입니다.
        /// </summary>
        long InstanceId { get; }

        /// <summary>
        /// 이 전투원이 플레이어 소속인지 여부입니다.
        /// </summary>
        bool IsPlayer { get; }
        
        /// <summary>
        /// 현재 체력입니다.
        /// </summary>
        float CurrentHp { get; }

        /// <summary>
        /// 이 전투원의 현재 상태 데이터입니다.
        /// </summary>
        object StateData { get; }
        
        /// <summary>
        /// 결정론적 전투 상태(State)의 변경 사항을 이 전투원에게 적용합니다.
        /// </summary>
        /// <param name="data">적용할 최신 상태 데이터입니다.</param>
        void ApplyState(object data);
    }
} 