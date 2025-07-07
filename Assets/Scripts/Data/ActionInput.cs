using System.Collections.Generic;
using DungeonMaster.Skill;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 전투 시스템에서 한 틱(tick)에 처리될 단일 액션(입력)을 정의합니다.
    /// 예: "A가 B에게 스킬 C를 사용했다"
    /// </summary>
    public class ActionInput
    {
        /// <summary>
        /// 액션의 종류 (기본 공격, 스킬 사용 등)
        /// </summary>
        public ActionType Type { get; set; }

        /// <summary>
        /// 액션을 수행하는 주체의 인스턴스 ID
        /// </summary>
        public long ActorInstanceId { get; set; }

        /// <summary>
        /// 액션의 대상이 되는 하나 또는 여러 객체의 인스턴스 ID 목록
        /// </summary>
        public List<long> TargetInstanceIds { get; set; }

        /// <summary>
        /// 사용된 스킬의 정보 (스킬 액션일 경우)
        /// </summary>
        public SkillData SkillToUse { get; set; }

        // 향후 확장성을 위해 위치 정보 등을 추가할 수 있습니다.
        // public DeterministicVector3 TargetPosition;

        public ActionInput()
        {
            TargetInstanceIds = new List<long>();
        }
    }

    /// <summary>
    /// 액션의 종류를 정의하는 열거형.
    /// 데이터 저장/전송 시 값이 밀리는 것을 방지하기 위해, 그룹으로 묶인 고유한 숫자 값을 할당합니다.
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// 기본 공격
        /// </summary>
        Attack = 100,
        
        /// <summary>
        /// 스킬 사용
        /// </summary>
        Skill = 200,
        
        /// <summary>
        /// 이동
        /// </summary>
        Move = 300,
        
        /// <summary>
        /// 방어
        /// </summary>
        Guard = 400,
        
        // 필요에 따라 이동 = 300, 아이템 사용 = 400 등의 액션을 추가할 수 있습니다.
        // Move,
        // UseItem
    }
} 