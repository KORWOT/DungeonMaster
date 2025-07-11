using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Character;
using DungeonMaster.Battle.Damage;
using DungeonMaster.Data;
using DungeonMaster.Localization;
using DungeonMaster.UI;
using DungeonMaster.Utility;
using UnityEngine;
using Random = System.Random;

namespace DungeonMaster.Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private DeterministicBattleSettingsData settings;
        [SerializeField] private DamageSettings damageSettings;
        [SerializeField] private BattleUI battleUI;

        [Tooltip("A fixed seed for deterministic battles. If 0, a random seed is used.")]
        [SerializeField] private int battleSeed = 0;

        [Header("Spawning Positions")]
        [SerializeField] private List<Transform> playerSpawnPoints;
        [SerializeField] private List<Transform> enemySpawnPoints;
        
        [Header("Dependencies")]
        [SerializeField] private MonoBehaviour characterFactoryProvider;
        [SerializeField] private AIActionInputProvider aiInputProvider;
        
        private DeterministicBattleRules _battleRules;
        private BattleState _currentBattleState;
        private IActionInputProvider _actionInputProvider;
        private IVictoryConditionChecker _victoryConditionChecker;
        private ICharacterFactory _characterFactory;
        
        private readonly Dictionary<long, ICharacter> _characterViewMap = new();
        private readonly List<ICharacter> _activeViews = new();

        public event Action<ICharacter> OnCharacterDeath;
        public event Action<bool> OnBattleEnd;

        public bool IsBattleActive { get; private set; }
        public long BattleTimeMs => _currentBattleState?.CurrentTimeMs ?? 0;
        
        private long _nextInstanceId = 1;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        public void StartBattle(BattleLaunchData battleData)
        {
            _characterFactory = characterFactoryProvider as ICharacterFactory;
            if (_characterFactory == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetText("log_error_no_character_factory"), this);
                return;
            }
            
            ClearPreviousBattle();

            int seed = (battleSeed == 0) ? new Random().Next() : battleSeed;
            
            var elementalTable = Resources.Load<ElementalAffinityTable>("Data/ElementalAffinityTable");
            var gradeGrowthConfig = Resources.Load<GradeGrowthConfig>("Character/GradeGrowthConfig");

            if (elementalTable == null || gradeGrowthConfig == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetText("log_error_core_data_not_found"));
                return;
            }

            _battleRules = new DeterministicBattleRules(settings, seed, elementalTable, damageSettings);
            _actionInputProvider = new AIActionInputProvider();
            _victoryConditionChecker = new DefaultVictoryConditionChecker();
            _nextInstanceId = 1; 
            _currentBattleState = new BattleState(new List<DeterministicCharacterData>(), BattleStatus.Ongoing);

            var initialCharacters = new List<DeterministicCharacterData>();
            
            for (int i = 0; i < battleData.PlayerMonsters.Count; i++)
            {
                if (i >= playerSpawnPoints.Count) break;
                var characterData = CharacterDataFactory.Create(
                    battleData.PlayerMonsters[i].Blueprint, 
                    battleData.PlayerMonsters[i].UserCard, 
                    gradeGrowthConfig, true, GetNextInstanceId());
                if (characterData != null) initialCharacters.Add(characterData);
            }
    
            for (int i = 0; i < battleData.EnemyMonsters.Count; i++)
            {
                if (i >= enemySpawnPoints.Count) break;
                var characterData = CharacterDataFactory.Create(
                    battleData.EnemyMonsters[i].Blueprint, 
                    battleData.EnemyMonsters[i].UserCard, 
                    gradeGrowthConfig, false, GetNextInstanceId());
                if (characterData != null) initialCharacters.Add(characterData);
            }
            
            _currentBattleState = _currentBattleState.With(newCharacters: initialCharacters);
            
            var allSpawnPoints = playerSpawnPoints.Concat(enemySpawnPoints).ToList();
            for(int i = 0; i < _currentBattleState.Characters.Count; i++)
            {
                var data = _currentBattleState.Characters[i];
                var spawnPoint = (i < allSpawnPoints.Count) ? allSpawnPoints[i] : transform;
                SetupCharacterView(data, spawnPoint);
            }

            IsBattleActive = true;
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("log_info_battle_started", seed));
            if (battleUI != null) battleUI.ShowBattleStart();

            ApplyStateToViews(_currentBattleState);
        }

        private void ClearPreviousBattle()
        {
            foreach (var view in _activeViews)
            {
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
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("log_error_view_creation_failed", data.BlueprintId));
                return;
            }
    
            view.Initialize(data);
            
            _characterViewMap.Add(data.InstanceId, view);
            _activeViews.Add(view);
        }

        private long GetNextInstanceId()
        {
            return _nextInstanceId++;
        }

        private void Update()
        {
            if (!IsBattleActive) return;

            var actionInputs = _actionInputProvider.CollectActionInputs(_currentBattleState, _battleRules);

            var actions = new List<IAction>();
            foreach (var input in actionInputs)
            {
                if (input.Type == ActionType.Attack)
                {
                    foreach (var targetId in input.TargetInstanceIds)
                        actions.Add(new AttackAction(input.ActorInstanceId, targetId));
                }
                else if (input.Type == ActionType.Skill)
                {
                    foreach (var targetId in input.TargetInstanceIds)
                        actions.Add(new SkillAction(input.ActorInstanceId, targetId, input.SkillToUse));
                }
            }
            
            long deltaTimeMs = (long)(Time.deltaTime * 1000);
            
            var stateForTick = _currentBattleState.With(newEvents: new List<BattleEvent>());
            var (newState, events) = _battleRules.ProcessTick(stateForTick, actions, deltaTimeMs);
            _currentBattleState = newState;

            foreach (var battleEvent in events)
            {
                if (_characterViewMap.TryGetValue(battleEvent.TargetId, out var targetView))
                {
                    switch (battleEvent.Type)
                    {
                        case BattleEventType.Damage: targetView.OnDamageReceived(battleEvent.Value); break;
                        case BattleEventType.Heal: targetView.OnHealed(battleEvent.Value); break;
                        case BattleEventType.BuffApply: targetView.OnBuffApplied(battleEvent.Value); break;
                        case BattleEventType.BuffRemove: targetView.OnBuffRemoved(battleEvent.Value); break;
                        case BattleEventType.Death:
                            targetView.OnDeath();
                            OnCharacterDeath?.Invoke(targetView);
                            break;
                    }
                }
            }

            ApplyStateToViews(_currentBattleState);
            
            if (battleUI != null) battleUI.UpdateTurnUI(_currentBattleState.TurnCount);

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
            GameLogger.LogInfo(isVictory 
                ? LocalizationManager.Instance.GetText("log_info_battle_victory") 
                : LocalizationManager.Instance.GetText("log_info_battle_defeat"));
            if (battleUI != null) battleUI.ShowBattleEnd(_currentBattleState.Status);
            OnBattleEnd?.Invoke(isVictory);
        }
    }
} 