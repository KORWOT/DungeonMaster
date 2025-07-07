using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DungeonMaster.Character;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 유저가 소유한 모든 UserCardData를 관리하는 컬렉션입니다.
    /// 이 클래스는 데이터의 저장과 조회만을 책임집니다.
    /// </summary>
    [Serializable]
    public class UserCardCollection : ISerializationCallbackReceiver
    {
        // 유니티 인스펙터에 노출되고, 실제로 파일에 저장될 데이터 리스트
        [SerializeField]
        private List<UserCardData> _cards = new List<UserCardData>();

        // 런타임 중 빠른 조회를 위한 딕셔너리 (Key: UserCardData.Id)
        private Dictionary<long, UserCardData> _cardsById;

        /// <summary>
        /// 특정 설계도(Blueprint)에 해당하는 카드를 이미 소유하고 있는지 확인합니다.
        /// </summary>
        public bool HasCard(long blueprintId)
        {
            // 이 방식은 컬렉션이 커질 경우 성능 저하가 있을 수 있으나,
            // 카드 획득은 빈번한 작업이 아니므로 일반적으로는 괜찮습니다.
            return _cards.Any(card => card.BlueprintId == blueprintId);
        }

        /// <summary>
        /// ID로 소유한 카드를 조회합니다.
        /// </summary>
        public UserCardData GetCard(long cardId)
        {
            if (_cardsById == null) RebuildDictionary();
            _cardsById.TryGetValue(cardId, out var card);
            return card;
        }

        /// <summary>
        /// 소유한 모든 카드의 리스트를 반환합니다.
        /// </summary>
        public IReadOnlyList<UserCardData> GetAllCards()
        {
            return _cards;
        }

        public int GetOwnedCardCount()
        {
            return _cards?.Count ?? 0;
        }

        /// <summary>
        /// 새로운 카드를 컬렉션에 추가합니다.
        /// </summary>
        public void AddCard(UserCardData newCard)
        {
            if (newCard == null) return;
            if (_cardsById == null) RebuildDictionary();

            if (_cardsById.ContainsKey(newCard.Id))
            {
                Debug.LogWarning($"이미 소유한 카드 ID({newCard.Id})를 추가하려고 합니다. 중복 카드는 허용되지 않습니다.");
                return;
            }

            _cards.Add(newCard);
            _cardsById[newCard.Id] = newCard;
        }

        /// <summary>
        /// 컬렉션에서 카드를 제거합니다.
        /// </summary>
        public bool RemoveCard(long cardId)
        {
            if (_cardsById == null) RebuildDictionary();

            if (_cardsById.TryGetValue(cardId, out var cardToRemove))
            {
                _cardsById.Remove(cardId);
                _cards.Remove(cardToRemove);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 새로운 카드를 설계도 ID로부터 생성하여 컬렉션에 추가하고,
        /// 이 카드의 '고유 성장치'를 결정론적 시스템의 규칙에 따라 단 한 번 설정합니다.
        /// </summary>
        public UserCardData AcquireCard(long blueprintId)
        {
            if (HasCard(blueprintId))
            {
                Debug.LogWarning($"이미 소유한 설계도(ID: {blueprintId})의 카드를 획득하려고 합니다. 중복 획득은 현재 비활성화되어 있습니다.");
                return _cards.First(c => c.BlueprintId == blueprintId);
            }

            var blueprint = BlueprintDatabase.Instance.GetBlueprint(blueprintId);
            if (blueprint == null)
            {
                Debug.LogError($"[UserCardCollection] BlueprintId({blueprintId})에 해당하는 설계도를 찾을 수 없습니다!");
                return null;
            }
            
            var newCard = new UserCardData(blueprintId);
            
            // 새 카드의 초기 스탯과 고유 성장률을 설정합니다.
            InitializeNewCard(newCard, blueprint);
            
            AddCard(newCard);
            
            var cardName = blueprint.Name;
            Debug.Log($"새로운 카드 획득: {cardName} (Blueprint ID: {blueprintId}, Instance ID: {newCard.Id})");

            return newCard;
        }

        /// <summary>
        /// 새로 생성된 카드의 초기 상태(CurrentStats, InnateGrowthRates)를 설정합니다.
        /// </summary>
        private void InitializeNewCard(UserCardData card, CardBlueprintData blueprint)
        {
            // 1. 기본 스탯을 CurrentStats에 복사하여 초기화합니다.
            foreach (var stat in blueprint.BaseStats)
            {
                card.CurrentStats[stat.StatType] = stat.Value;
            }
            // 공격 속도도 초기 스탯에 포함합니다.
            card.CurrentStats[StatType.AttackSpeed] = blueprint.BaseAttackSpeed_x100;
            
            // 2. 이 카드의 '고유 성장률'을 최소값(100%)으로 초기화합니다.
            // 이 값은 향후 '강화' 시스템을 통해 변경될 수 있습니다.
            foreach (var statType in blueprint.GrowableStatTypes)
            {
                card.InnateGrowthRates_x100[statType] = 100;
                Debug.Log($"카드({card.Id})의 {statType} 고유 성장률 초기화: 100%");
            }
        }

        /// <summary>
        /// (내부용) 직렬화된 리스트로부터 런타임 딕셔너리를 다시 만듭니다.
        /// </summary>
        private void RebuildDictionary()
        {
            _cardsById = _cards.ToDictionary(card => card.Id, card => card);
        }

        // --- ISerializationCallbackReceiver 구현 ---

        /// <summary>
        /// Unity가 이 객체를 직렬화하기 전에 호출됩니다.
        /// (런타임 딕셔너리의 변경사항이 없으므로, 여기서는 아무것도 할 필요가 없습니다.)
        /// </summary>
        public void OnBeforeSerialize()
        {
            // _cards 리스트가 직접 수정되므로 딕셔너리를 리스트에 동기화할 필요가 없어졌습니다.
        }

        /// <summary>
        /// Unity가 파일로부터 데이터를 역직렬화한 후에 호출됩니다.
        /// </summary>
        public void OnAfterDeserialize()
        {
            // 데이터 로드 후 런타임 딕셔너리를 다시 만듭니다.
            RebuildDictionary();
        }
    }
} 