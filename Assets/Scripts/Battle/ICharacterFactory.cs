using DungeonMaster.Character;
using UnityEngine;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// 전투에 사용될 캐릭터의 뷰(ICharacter)를 생성하고 관리하는 모든 팩토리의 인터페이스입니다.
    /// </summary>
    public interface ICharacterFactory
    {
        /// <summary>
        /// 지정된 ID의 캐릭터 뷰를 가져옵니다. (풀링 또는 새로 생성)
        /// </summary>
        /// <param name="prefabId">생성할 프리팹의 ID</param>
        /// <param name="parent">생성될 위치의 부모 Transform</param>
        /// <returns>생성된 캐릭터 뷰</returns>
        ICharacter Get(long prefabId, Transform parent);

        /// <summary>
        /// 사용이 끝난 뷰를 반납합니다. (풀에 반환 또는 파괴)
        /// </summary>
        /// <param name="view">반납할 캐릭터 뷰</param>
        void Release(ICharacter view);
    }
} 