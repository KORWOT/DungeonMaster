using System.Collections.Generic;
using DungeonMaster.Character;
using DungeonMaster.Localization;

namespace DungeonMaster.DemonLord
{
    /// <summary>
    /// DemonLordData를 생성하는 팩토리 클래스입니다.
    /// </summary>
    public static class DemonLordDataFactory
    {
        /// <summary>
        /// 전투에 참여할 마왕의 결정론적 데이터를 생성합니다.
        /// </summary>
        public static DemonLordData Create(DemonLordBlueprint blueprint, int level, bool isPlayer, long instanceId)
        {
            if (blueprint == null) return null;

            // 1. 최종 스탯 계산
            var finalStats = new Dictionary<StatType, long>();
            // 기본 스탯 복사
            foreach (var stat in blueprint.BaseStats)
            {
                finalStats[stat.StatType] = stat.Value;
            }

            // 성장 스탯 계산 (레벨 1부터 시작이므로 level-1 만큼 성장)
            if (level > 1)
            {
                foreach (var growthStat in blueprint.GrowthStats_x100)
                {
                    // 성장률은 x100된 정수이므로 100으로 나누어 실제 값을 곱함
                    long growthAmount = (growthStat.Value * (level - 1)) / 100;
                    if (finalStats.ContainsKey(growthStat.StatType))
                    {
                        finalStats[growthStat.StatType] += growthAmount;
                    }
                    else
                    {
                        finalStats[growthStat.StatType] = growthAmount;
                    }
                }
            }
            
            // 2. 최대 HP 설정
            long maxHp = finalStats.ContainsKey(StatType.MaxHP) ? finalStats[StatType.MaxHP] : 1;

            // 3. 마왕 이름 지역화 처리
            string localizedName = !string.IsNullOrEmpty(blueprint.NameKey) 
                ? LocalizationManager.Instance.GetText(blueprint.NameKey)
                : "Unknown Demon Lord";

            // 4. 스킬 ID 목록 처리 (StartingActiveSkills에서 추출)
            var skillIds = new List<long>();
            if (blueprint.StartingActiveSkills != null)
            {
                foreach (var skill in blueprint.StartingActiveSkills)
                {
                    if (skill != null && !string.IsNullOrEmpty(skill.BlueprintId))
                    {
                        // 문자열 ID를 long으로 해시 변환 (임시 방식)
                        skillIds.Add(skill.BlueprintId.GetHashCode());
                    }
                }
            }

            // 5. DemonLordData 인스턴스 생성
            var demonLordData = new DemonLordData(
                instanceId,
                isPlayer,
                localizedName,
                level,
                maxHp, // 전투 시작 시 HP는 최대치
                finalStats,
                skillIds
            );

            return demonLordData;
        }
    }
} 