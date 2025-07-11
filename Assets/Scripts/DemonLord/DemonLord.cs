using DungeonMaster.Character;

namespace DungeonMaster.DemonLord
{
    /// <summary>
    /// 마왕을 나타내는 전투 단위입니다.
    /// ICombatant 인터페이스를 구현하며, 시각적 표현(View)이 없는 순수 로직 객체입니다.
    /// </summary>
    public class DemonLord : ICombatant
    {
        private DemonLordData _data;
        
        public DemonLord(DemonLordData initialData)
        {
            _data = initialData;
        }

        // --- ICombatant 구현 ---
        public long InstanceId => _data.InstanceId;
        public bool IsPlayer => _data.IsPlayer;
        public float CurrentHp => _data.CurrentHP;
        public object StateData => _data;

        public void ApplyState(object data)
        {
            if (data is DemonLordData newData)
            {
                _data = newData;
            }
        }
    }
} 