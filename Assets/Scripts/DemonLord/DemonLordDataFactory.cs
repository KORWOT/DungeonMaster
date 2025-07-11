using System.Collections.Generic;
using DungeonMaster.Character;

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
                finalStats[stat.Key] = stat.Value;
            }

            // 성장 스탯 계산 (레벨 1부터 시작이므로 level-1 만큼 성장)
            if (level > 1)
            {
                foreach (var growthStat in blueprint.GrowthStats)
                {
                    // 성장률은 x100된 정수이므로 100으로 나누어 실제 값을 곱함
                    long growthAmount = (growthStat.Value * (level - 1)) / 100;
                    if (finalStats.ContainsKey(growthStat.Key))
                    {
                        finalStats[growthStat.Key] += growthAmount;
                    }
                    else
                    {
                        finalStats[growthStat.Key] = growthAmount;
                    }
                }
            }
            
            // 2. 최대 HP 설정
            long maxHp = finalStats.ContainsKey(StatType.MaxHP) ? finalStats[StatType.MaxHP] : 1;

            // 3. DemonLordData 인스턴스 생성
            var demonLordData = new DemonLordData(
                instanceId,
                isPlayer,
                blueprint.Name,
                level,
                maxHp, // 전투 시작 시 HP는 최대치
                finalStats,
                blueprint.BaseSkillIds
            );

            return demonLordData;
        }
    }
} 