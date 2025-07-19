using System;
using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Data;
using DungeonMaster.Character;
using UnityEngine;
using Random = System.Random;
using DungeonMaster.UI;
using System.Collections;
using DungeonMaster.Battle.Damage;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// 결정론적 전투의 '지휘자' 역할을 합니다.
    /// 전투의 상태(State)와 규칙(Rules)을 관리하고, 매 틱(Tick)마다의 진행을 담당합니다.
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private DeterministicBattleSettingsData settings;
        [SerializeField] private DamageSettings damageSettings;
        [SerializeField] private BattleUI battleUI;
        [Tooltip("결정론성을 보장하기 위한 전투 시드. 0이면 랜덤 시드를 사용합니다.")]
        [SerializeField] private int battleSeed = 0;

        [Header("생성 위치")]
        [SerializeField] private List<Transform> playerSpawnPoints;
        [SerializeField] private List<Transform> enemySpawnPoints;
        
        [Header("의존성 주입 (Dependencies)")]
        [SerializeField] private MonoBehaviour characterFactoryProvider;
        [SerializeField] private AIActionInputProvider aiInputProvider;
        
        // --- 전투의 핵심 구성요소 ---
        private DeterministicBattleRules _battleRules;
        private BattleState _currentBattleState;
        private IActionInputProvider _actionInputProvider; // AI 또는 플레이어 입력을 제공
        private IVictoryConditionChecker _victoryConditionChecker; // 승리/패배 조건 확인
        private ICharacterFactory _characterFactory; // 뷰 생성을 담당
        
        // --- 데이터와 뷰의 연결고리 ---
        private readonly Dictionary<long, ICharacter> _characterViewMap = new Dictionary<long, ICharacter>();
        private readonly List<ICharacter> _activeViews = new List<ICharacter>(); // 생성된 뷰들을 추적하여 풀에 반납하기 위함

        // --- 이벤트 ---
        public Action<ICharacter> OnCharacterDeath;
        public event Action<bool> OnBattleEnd; // true: 승리, false: 패배

        // --- 프로퍼티 ---
        public bool IsBattleActive { get; private set; }
        public long BattleTimeMs => _currentBattleState?.CurrentTimeMs ?? 0;
        
        // --- 전투 인스턴스 정보 ---
        private long _nextInstanceId = 1; // ID 생성 책임을 BattleManager가 가집니다.
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 필요 시 활성화
        }
        
        /// <summary>
        /// 제공된 데이터로 전투를 시작합니다.
        /// BattleLaunchData를 받아 전투의 모든 과정을 관리합니다.
        /// </summary>
        public void StartBattle(BattleLaunchData battleData)
        {
            // 0. 의존성 확인
            _characterFactory = characterFactoryProvider as ICharacterFactory;
            if (_characterFactory == null)
            {
                Debug.LogError("ICharacterFactory가 할당되지 않았습니다! BattleManager 인스펙터를 확인해주세요.", this);
                return;
            }
            
            // 1. 이전 전투 정리
            ClearPreviousBattle();

            // 2. 전투 시스템 초기화 (두뇌, 상태, 입력, 판정)
            int seed = (battleSeed == 0) ? new Random().Next() : battleSeed;
            
            var elementalTable = Resources.Load<ElementalAffinityTable>("Data/ElementalAffinityTable");
            var gradeGrowthConfig = Resources.Load<GradeGrowthConfig>("Character/GradeGrowthConfig");

            if (elementalTable == null || gradeGrowthConfig == null)
            {
                Debug.LogError("핵심 설정 파일(Elemental, Growth)을 찾을 수 없습니다. Resources 폴더를 확인하세요.");
                return;
            }

            _battleRules = new DeterministicBattleRules(settings, seed, elementalTable, damageSettings);
            _actionInputProvider = new AIActionInputProvider();
            _victoryConditionChecker = new DefaultVictoryConditionChecker();
            _nextInstanceId = 1; 
            _currentBattleState = new BattleState(new List<DeterministicCharacterData>(), BattleStatus.Ongoing);

            // 3. 캐릭터 데이터 생성
            var initialCharacters = new List<DeterministicCharacterData>();
            
            // 플레이어 캐릭터 생성
            for (int i = 0; i < battleData.PlayerMonsters.Count; i++)
            {
                if (i >= playerSpawnPoints.Count)
                {
                    Debug.LogWarning($"플레이어 스폰 포인트가 부족합니다. {i+1}번째 캐릭터를 생성할 수 없습니다.");
                    break;
                }
                var characterData = CharacterDataFactory.Create(
                    battleData.PlayerMonsters[i].Blueprint, 
                    battleData.PlayerMonsters[i].UserCard, 
                    gradeGrowthConfig, 
                    true, // isPlayer
                    GetNextInstanceId()
                );
                if (characterData != null)
                {
                    initialCharacters.Add(characterData);
                }
            }
    
            // 적 캐릭터 생성
            for (int i = 0; i < battleData.EnemyMonsters.Count; i++)
            {
                if (i >= enemySpawnPoints.Count)
                {
                    Debug.LogWarning($"적 스폰 포인트가 부족합니다. {i+1}번째 캐릭터를 생성할 수 없습니다.");
                    break;
                }
                var characterData = CharacterDataFactory.Create(
                    battleData.EnemyMonsters[i].Blueprint, 
                    battleData.EnemyMonsters[i].UserCard, 
                    gradeGrowthConfig, 
                    false, // isPlayer
                    GetNextInstanceId()
                );
                if (characterData != null)
                {
                    initialCharacters.Add(characterData);
                }
            }
            
            // 4. 첫 BattleState 생성 및 뷰 초기화
            _currentBattleState = _currentBattleState.With(newCharacters: initialCharacters);
            
            var allSpawnPoints = playerSpawnPoints.Concat(enemySpawnPoints).ToList();
            for(int i = 0; i < _currentBattleState.Characters.Count; i++)
            {
                var data = _currentBattleState.Characters[i];
                var spawnPoint = (i < allSpawnPoints.Count) ? allSpawnPoints[i] : transform; // 스폰 포인트 부족 시 매니저 위치에 생성
                SetupCharacterView(data, spawnPoint);
            }

            // 5. 전투 시작
            IsBattleActive = true;
            Debug.Log($"전투 시작! Seed: {seed}");

            // 초기 상태를 뷰에 즉시 반영
            ApplyStateToViews(_currentBattleState);
        }

        private void ClearPreviousBattle()
        {
            foreach (var view in _activeViews)
            {
                // Destroy 대신 Factory의 Release를 호출하여 풀에 반납
                _characterFactory.Release(view);
            }
            _activeViews.Clear();
            _characterViewMap.Clear();
        }

        private void SetupCharacterView(DeterministicCharacterData data, Transform spawnPoint)
        {
            var view = _characterFactory.Get(data.BlueprintId, spawnPoint);
            if (view == null)
            {
                Debug.LogError($"캐릭터 뷰 생성에 실패했습니다. Prefab ID: {data.BlueprintId}");
                return;
            }
    
            view.Initialize(data);
            
            _characterViewMap.Add(data.InstanceId, view);
            _activeViews.Add(view);
        }

        /// <summary>
        /// 다음으로 사용할 고유 인스턴스 ID를 반환합니다.
        /// </summary>
        private long GetNextInstanceId()
        {
            return _nextInstanceId++;
        }

        private void Update()
        {
            if (!IsBattleActive) return;

            // 1. 행동 입력 수집 (AI 및 플레이어 입력)
            var actionInputs = _actionInputProvider.CollectActionInputs(_currentBattleState, _battleRules);

            // 2. ActionInput을 실제 IAction으로 변환
            var actions = new List<IAction>();
            foreach (var input in actionInputs)
            {
                if (input.Type == ActionType.Attack)
                {
                    foreach (var targetId in input.TargetInstanceIds)
                    {
                        actions.Add(new AttackAction(input.ActorInstanceId, targetId));
                    }
                }
                else if (input.Type == ActionType.Skill)
                {
                    foreach (var targetId in input.TargetInstanceIds)
                    {
                        actions.Add(new SkillAction(input.ActorInstanceId, targetId, input.SkillToUse));
                    }
                }
            }
            
            // 3. 결정론적 규칙에 따라 틱 처리 (long 타입 시간 사용)
            long deltaTimeMs = (long)(Time.deltaTime * 1000);
            
            // 이벤트 처리를 위해, 이번 틱을 처리하기 전에 이벤트 목록을 비운 상태를 생성합니다.
            var stateForTick = _currentBattleState.With(newEvents: new List<BattleEvent>());
            var (newState, events) = _battleRules.ProcessTick(stateForTick, actions, deltaTimeMs);
            _currentBattleState = newState;

            // --- 이벤트 처리 로직 추가 ---
            // ProcessTick의 결과로 반환된 상태에는 이번 틱에서 발생한 이벤트만 포함됩니다.
            foreach (var battleEvent in events)
            {
                if (_characterViewMap.TryGetValue(battleEvent.TargetId, out var targetView))
                {
                    switch (battleEvent.Type)
                    {
                        case BattleEventType.Damage:
                            targetView.OnDamageReceived(battleEvent.Value);
                            break;
                        case BattleEventType.Heal:
                            targetView.OnHealed(battleEvent.Value);
                            break;
                        case BattleEventType.BuffApply:
                            targetView.OnBuffApplied(battleEvent.Value); // Value는 BuffId
                            break;
                        case BattleEventType.BuffRemove:
                            targetView.OnBuffRemoved(battleEvent.Value); // Value는 BuffId
                            break;
                        case BattleEventType.Death:
                            targetView.OnDeath();
                            break;
                        // 다른 이벤트 타입들이 추가될 수 있습니다.
                    }
                }
            }

            // 4. 변경된 상태를 뷰에 적용
            ApplyStateToViews(_currentBattleState);

            // 5. 전투 종료 조건 확인
            var outcome = _victoryConditionChecker.Check(_currentBattleState);
            if (outcome != BattleOutcome.Ongoing)
            {
                EndBattle(outcome);
            }
        }

        private void ApplyStateToViews(BattleState state)
        {
            foreach (var characterData in state.Characters)
            {
                if (_characterViewMap.TryGetValue(characterData.InstanceId, out var view))
                {
                    view.ApplyState(characterData);
                }
            }
        }

        private void EndBattle(BattleOutcome outcome)
        {
            IsBattleActive = false;
            _currentBattleState = _currentBattleState.With(newStatus: BattleStatus.Finished);
            
            bool isVictory = outcome == BattleOutcome.Victory;
            Debug.Log(isVictory ? "승리!" : "패배!");
            OnBattleEnd?.Invoke(isVictory);
        }
    }
} 