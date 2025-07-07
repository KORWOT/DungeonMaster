using DungeonMaster.Character;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Linq;
using DungeonMaster.Skill;
using DungeonMaster.Equipment;
using DungeonMaster.Managers;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 결정론적 캐릭터 데이터(DeterministicCharacterData)를 생성하는 팩토리 클래스.
    /// UserCardData에 이미 계산된 최종 스탯을 전투용 데이터 구조로 변환하는 역할을 합니다.
    /// </summary>
    public static class CharacterDataFactory
    {
        /// <summary>
        /// 전투에 참여할 캐릭터의 결정론적 데이터를 생성합니다.
        /// </summary>
        /// <param name="blueprint">캐릭터의 원본 설계도</param>
        /// <param name="userCard">모든 성장이 완료된 최종 스탯을 담고 있는 유저 소유 데이터</param>
        /// <param name="gradeGrowthConfig">등급별 성장 설정 (현재는 사용하지 않지만 확장성을 위해 유지)</param>
        /// <param name="isPlayer">플레이어 소속 여부</param>
        /// <param name="instanceId">이번 전투에서 사용할 고유 ID</param>
        /// <returns>생성된 캐릭터 데이터</returns>
        public static DeterministicCharacterData Create(
            CardBlueprintData blueprint, 
            UserCardData userCard,
            GradeGrowthConfig gradeGrowthConfig,
            bool isPlayer, 
            long instanceId)
        {
            if (blueprint == null || userCard == null) return null;
            
            var characterData = new DeterministicCharacterData(
                instanceId, 
                blueprint.BlueprintId, 
                blueprint.Name, 
                userCard.Level, 
                isPlayer, 
                blueprint.Element,
                blueprint.DefenseType,
                blueprint.Grade,
                userCard.SkillLevels);
            
            // 1. UserCardData에 저장된 최종 스탯을 그대로 복사합니다.
            foreach (var stat in userCard.CurrentStats)
            {
                characterData.Stats[stat.Key] = stat.Value;
            }
            
            // 2. 스킬 정보를 복사합니다.
            if(blueprint.SkillIds != null)
            {
                characterData.Skills.AddRange(blueprint.SkillIds.Select(id => SkillManager.Instance.GetSkill(id)).Where(s => s != null));
            }

            // 3. 장비의 스탯 보너스와 특수 효과(IDamageModifier)를 적용합니다.
            if (userCard.EquippedItemIds != null && userCard.EquippedItemIds.Count > 0)
            {
                foreach (var itemId in userCard.EquippedItemIds)
                {
                    var equipment = EquipmentManager.Instance.GetEquipment(itemId);
                    if (equipment == null) continue;

                    // 3-1. 장비의 스탯 보너스를 합산합니다.
                    var statBonuses = equipment.GetAllStatBonuses(userCard.Level); // 캐릭터 레벨 기준으로 스탯 계산
                    foreach (var bonus in statBonuses)
                    {
                        if (characterData.Stats.ContainsKey(bonus.Key))
                        {
                            characterData.Stats[bonus.Key] += bonus.Value;
                        }
                        else
                        {
                            characterData.Stats[bonus.Key] = bonus.Value;
                        }
                    }

                    // 3-2. 장비의 고유 효과 (IDamageModifier)를 추가합니다.
                    if (equipment.UniqueEffect is DungeonMaster.Battle.IDamageModifier modifier)
                    {
                        characterData.AddDamageModifier(modifier);
                    }
                }
            }

            // 4. 모든 스탯 계산 완료 후 최종 상태 설정 (예: HP 초기화)
            characterData.FinalizeStatCalculation();

            // 5. 공격 쿨타임 초기화
            characterData.AttackCooldownRemainingMs = 0;

            return characterData;
        }
    }
}