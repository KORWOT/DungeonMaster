using System;

namespace DungeonMaster.Character
{
    /// <summary>
    /// 스킬과 캐릭터가 가질 수 있는 속성을 정의합니다.
    /// </summary>
    [Serializable]
    public enum ElementType
    {
        Normal = 0, // 무속성
        Fire = 1,   // 화속성
        Water = 2,  // 수속성
        Wind = 3,   // 풍속성
        Earth = 4,  // 지(地)속성
        Light = 5,  // 명속성
        Dark = 6,   // 암속성
    }
} 