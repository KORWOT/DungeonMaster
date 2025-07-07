using DungeonMaster.Character;
using System.Collections.Generic;

namespace DungeonMaster.Equipment
{
    /// <summary>
    /// 몬스터 장비 인터페이스
    /// </summary>
    public interface IMonsterEquipment
    {
        string Name { get; }
        string Description { get; }
        MonsterEquipmentType EquipmentType { get; }
        EquipmentGrade Grade { get; }
        int Level { get; }
        
        // 강화 시스템
        bool CanUpgrade();
        void UpgradeLevel();
        void UpgradeGrade();
    }

    /// <summary>
    /// 몬스터 장비 타입
    /// </summary>
    public enum MonsterEquipmentType
    {
        Weapon,     // 무기 - 공격력 중심
        Armor,      // 방어구 - 방어력 중심  
        Accessory   // 악세사리 - 보조 효과 중심
    }

    /// <summary>
    /// 장비 등급
    /// </summary>
    public enum EquipmentGrade
    {
        Normal,     // 일반 - 기본 효과만
        Magic,      // 마법 - 추가 효과 1개
        Rare,       // 희귀 - 추가 효과 2-3개
        Epic,       // 에픽 - 추가 효과 4-6개
        Legendary   // 전설 - 추가 효과 7-10개
    }
} 