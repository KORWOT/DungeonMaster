using DungeonMaster.Data;
using DungeonMaster.Character;
using UnityEngine;
using System.Collections.Generic;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// 캐릭터 뷰(GameObject)의 생성을 담당하는 MonoBehaviour 팩토리 클래스입니다.
    /// ICharacterFactory 인터페이스를 구현하며, 오브젝트 풀링을 지원합니다.
    /// </summary>
    public class CharacterViewFactory : MonoBehaviour, ICharacterFactory
    {
        [Header("프리팹 목록")]
        [SerializeField]
        [Tooltip("여기에 모든 캐릭터 프리팹을 ID와 함께 등록합니다.")]
        private List<CharacterPrefab> _characterPrefabs;

        // 빠른 조회를 위한 프리팹 딕셔너리
        private Dictionary<long, ClientMonster> _prefabMap;
        
        // 오브젝트 풀
        private Dictionary<long, Queue<ClientMonster>> _pool;

        private void Awake()
        {
            // 싱글톤 대신, 인스턴스가 직접 초기화되도록 변경
            InitializeFactory();
        }

        private void InitializeFactory()
        {
            _prefabMap = new Dictionary<long, ClientMonster>();
            _pool = new Dictionary<long, Queue<ClientMonster>>();

            foreach (var entry in _characterPrefabs)
            {
                if (entry.ID == 0 || entry.Prefab == null)
                {
                    Debug.LogWarning($"ID가 0이거나 프리팹이 할당되지 않은 항목이 있습니다. 이 항목은 건너뜁니다.");
                    continue;
                }
                if (_prefabMap.ContainsKey(entry.ID))
                {
                    Debug.LogWarning($"프리팹 ID '{entry.ID}'가 중복됩니다. 하나의 항목만 사용됩니다.");
                    continue;
                }
                
                _prefabMap.Add(entry.ID, entry.Prefab);
                _pool.Add(entry.ID, new Queue<ClientMonster>());
            }
        }

        /// <summary>
        /// 지정된 ID의 캐릭터 뷰를 풀에서 가져오거나 새로 생성합니다.
        /// </summary>
        /// <param name="id">가져올 캐릭터의 프리팹 ID (CardBlueprintData.BlueprintId)</param>
        /// <param name="parent">생성될 위치와 부모가 될 Transform</param>
        /// <returns>활성화된 ClientMonster 인스턴스</returns>
        public ICharacter Get(long id, Transform parent)
        {
            if (!_prefabMap.ContainsKey(id))
            {
                Debug.LogError($"ID '{id}'에 해당하는 프리팹이 CharacterViewFactory에 등록되지 않았습니다.");
                return null;
            }

            ClientMonster instance;
            var targetPool = _pool[id];

            if (targetPool.Count > 0)
            {
                instance = targetPool.Dequeue();
            }
            else
            {
                instance = Instantiate(_prefabMap[id]);
                instance.PrefabID = id; // 최초 생성 시에만 ID를 할당합니다.
            }
            
            // 위치, 회전, 부모 설정 및 활성화
            instance.transform.SetParent(parent, false);
            instance.transform.SetPositionAndRotation(parent.position, parent.rotation);
            instance.gameObject.SetActive(true);

            return instance;
        }

        /// <summary>
        /// 사용이 끝난 캐릭터 뷰를 풀에 반납합니다.
        /// </summary>
        /// <param name="view">반납할 ICharacter 인스턴스</param>
        public void Release(ICharacter view)
        {
            if (!(view is ClientMonster instance))
            {
                Debug.LogError($"반납하려는 뷰가 ClientMonster가 아닙니다. Type: {view?.GetType().Name}");
                return;
            }
            
            if (instance == null) return;
            
            var id = instance.PrefabID;
            if (id == 0 || !_pool.ContainsKey(id))
            {
                Debug.LogError($"이 인스턴스({instance.name})는 풀링할 수 없습니다. PrefabID가 0이거나 잘못되었습니다. 그냥 파괴합니다.");
                Destroy(instance.gameObject);
                return;
            }
            
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(transform, false); // 팩토리를 부모로 하여 씬을 깔끔하게 유지
            _pool[id].Enqueue(instance);
        }
    }
} 