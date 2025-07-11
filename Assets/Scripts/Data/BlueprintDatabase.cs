using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Character;
using DungeonMaster.Localization;
using DungeonMaster.Utility;
using UnityEngine;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 몬스터 설계도(CardBlueprintData) 에셋들의 데이터베이스 역할을 합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "BlueprintDatabase", menuName = "DungeonMaster/Data/Blueprint Database")]
    public class BlueprintDatabase : ScriptableObject
    {
        [Header("몬스터 설계도 목록")]
        [SerializeField] private List<CardBlueprintData> cardBlueprints = new List<CardBlueprintData>();
        
        [Header("마왕 설계도 목록")]
        [SerializeField] private List<DemonLordBlueprint> demonLordBlueprints = new List<DemonLordBlueprint>();

        private readonly Dictionary<long, CardBlueprintData> _blueprintById = new Dictionary<long, CardBlueprintData>();
        private readonly Dictionary<string, DemonLordBlueprint> _demonLordBlueprintById = new Dictionary<string, DemonLordBlueprint>();
        private readonly Dictionary<Grade, List<CardBlueprintData>> _blueprintsByGrade = new Dictionary<Grade, List<CardBlueprintData>>();
        private bool _isInitialized = false;
        
        private static BlueprintDatabase _instance;
        public static BlueprintDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<BlueprintDatabase>("Data/BlueprintDatabase");
                    if (_instance == null)
                    {
                        GameLogger.LogError(LocalizationManager.Instance.GetText("error_blueprint_db_not_found"));
                    }
                    else
                    {
                        _instance.Initialize();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 데이터베이스를 초기화하고 조회용 딕셔너리를 생성합니다.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            _blueprintById.Clear();
            _blueprintsByGrade.Clear();
            _demonLordBlueprintById.Clear();

            // 카드 블루프린트 초기화
            if (cardBlueprints != null)
            {
                foreach (var blueprint in cardBlueprints)
                {
                    if (blueprint == null) continue;

                    if (!_blueprintById.ContainsKey(blueprint.BlueprintId))
                    {
                        _blueprintById.Add(blueprint.BlueprintId, blueprint);
                    }
                    else
                    {
                        GameLogger.LogWarning(LocalizationManager.Instance.GetTextFormatted("warn_duplicate_blueprint_id", blueprint.BlueprintId, blueprint.Name));
                    }

                    if (!_blueprintsByGrade.ContainsKey(blueprint.Grade))
                    {
                        _blueprintsByGrade[blueprint.Grade] = new List<CardBlueprintData>();
                    }
                    _blueprintsByGrade[blueprint.Grade].Add(blueprint);
                }
            }
            
            // 마왕 블루프린트 초기화
            if (demonLordBlueprints != null)
            {
                foreach (var blueprint in demonLordBlueprints)
                {
                    if (blueprint == null) continue;
                    
                    if (!_demonLordBlueprintById.ContainsKey(blueprint.BlueprintId))
                    {
                        _demonLordBlueprintById.Add(blueprint.BlueprintId, blueprint);
                    }
                    else
                    {
                        GameLogger.LogWarning($"Duplicate Demon Lord Blueprint ID found: {blueprint.BlueprintId}");
                    }
                }
            }
            
            _isInitialized = true;
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("info_blueprint_db_initialized", _blueprintById.Count));
        }

        /// <summary>
        /// ID로 설계도를 찾습니다. (가장 빠르고 권장되는 방법)
        /// </summary>
        public CardBlueprintData GetBlueprint(long blueprintId)
        {
            _blueprintById.TryGetValue(blueprintId, out var blueprint);
            return blueprint;
        }

        /// <summary>
        /// 이름으로 설계도를 찾습니다. (주로 디버깅 및 에디터용)
        /// </summary>
        public CardBlueprintData GetBlueprint(string monsterName)
        {
            return cardBlueprints?.FirstOrDefault(bp => bp != null && bp.Name == monsterName);
        }

        /// <summary>
        /// ID로 마왕 설계도를 찾습니다.
        /// </summary>
        public DemonLordBlueprint GetDemonLordBlueprint(string blueprintId)
        {
            _demonLordBlueprintById.TryGetValue(blueprintId, out var blueprint);
            return blueprint;
        }

        /// <summary>
        /// 모든 설계도 목록을 가져옵니다.
        /// </summary>
        public IReadOnlyList<CardBlueprintData> GetAllBlueprints()
        {
            return cardBlueprints;
        }

        /// <summary>
        /// 특정 등급의 모든 설계도 목록을 가져옵니다. 불필요한 리스트 생성을 방지하기 위해 IEnumerable을 반환합니다.
        /// </summary>
        public IEnumerable<CardBlueprintData> GetBlueprintsByGrade(Grade grade)
        {
            if (_blueprintsByGrade.TryGetValue(grade, out var blueprints))
            {
                return blueprints;
            }
            return Enumerable.Empty<CardBlueprintData>();
        }

        /// <summary>
        /// 시작 카드로 지정된 모든 설계도 목록을 가져옵니다.
        /// </summary>
        public IEnumerable<CardBlueprintData> GetStarterBlueprints()
        {
            return cardBlueprints?.Where(bp => bp != null && bp.IsStarterCard) ?? Enumerable.Empty<CardBlueprintData>();
        }

        // --- 기존의 통계 및 에디터용 메서드들은 필요에 따라 유지하거나 수정할 수 있습니다. ---
        // 여기서는 핵심 기능만 남기고 단순화합니다.
    }
} 