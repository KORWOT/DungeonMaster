# 결정론적 전투 시스템 가이드

## 개요

이 문서는 Unity 프로젝트의 결정론적 전투 시스템에 대한 종합적인 가이드입니다. 본 시스템은 서버-클라이언트 간 완벽한 동기화를 보장하며, 동일한 입력에 대해 항상 동일한 결과를 생성합니다.

## 주요 개선사항

### 1. Unity 의존성 완전 제거
- `UnityEngine.Random` → `System.Random` (시드 기반 결정론적 난수)
- 모든 전투 로직에서 Unity 네임스페이스 제거
- 서버에서도 동일한 코드 실행 가능

### 2. 부동소수점 연산 제거
- 모든 계산을 정수 기반으로 변경 (100 = 1.0배)
- `float` → `long` 변환으로 완벽한 결정론성 확보
- 플랫폼 간 부동소수점 오차 문제 해결

### 3. 열거형 값 명시적 할당
- 모든 `enum`에 고유한 숫자 값 할당
- 서버-클라이언트 간 열거형 불일치 방지
- 코드 변경 시에도 안정적인 직렬화 보장

### 4. 통합된 공격 시스템
- 기본 공격을 특별한 `SkillData`(ID=0)로 처리
- `DamageCalculator` 로직 일원화
- 코드 중복 제거 및 일관성 향상

### 5. 미사용 코드 정리
- TODO 주석 정리 및 업데이트
- 주석 처리된 코드 제거
- 코드베이스 최적화

## 아키텍처 개요

### 핵심 컴포넌트

#### 1. DeterministicBattleRules
```csharp
// 결정론적 전투 규칙 엔진
var battleRules = new DeterministicBattleRules(damageCalculator, victoryChecker, randomSeed);
var (newState, events) = battleRules.ProcessTick(currentState, actions, deltaTimeMs);
```

#### 2. DeterministicCharacterData
```csharp
// 순수 데이터 컨테이너 (Unity 의존성 없음)
var character = new DeterministicCharacterData(instanceId, blueprintId, name, level, isPlayer, element, defenseType, grade);
character.Stats[StatType.Attack] = 100;
character.FinalizeStatCalculation();
```

#### 3. SkillManager (싱글턴)
```csharp
// 스킬 데이터 관리
var skill = SkillManager.Instance.GetSkill(skillId);
var basicAttack = SkillManager.Instance.GetBasicAttackSkill(); // ID=0
```

#### 4. EquipmentManager (싱글턴)
```csharp
// 장비 데이터 관리
var equipment = EquipmentManager.Instance.GetEquipment(equipmentId);
if (equipment.UniqueEffect is IDamageModifier modifier)
{
    character.DamageModifiers.Add(modifier);
}
```

### 데이터 흐름

1. **초기화**: `CharacterDataFactory`가 `UserCardData`를 `DeterministicCharacterData`로 변환
2. **전투 루프**: `DeterministicBattleRules.ProcessTick`이 상태 변화 처리
3. **액션 처리**: `IAction` 구현체들이 상태 변경 및 이벤트 생성
4. **뷰 업데이트**: `BattleEvent`를 통해 UI 업데이트

## 사용법

### 1. 전투 시스템 초기화

```csharp
// 필수 컴포넌트 설정
var damageSettings = Resources.Load<DamageSettings>("Settings/DamageSettings");
var affinityTable = Resources.Load<ElementalAffinityTable>("Data/ElementalAffinityTable");
var damageCalculator = new DefaultDamageCalculator(affinityTable, damageSettings);
var victoryChecker = new DefaultVictoryConditionChecker();

// 전투 규칙 생성 (시드는 서버에서 제공)
var battleRules = new DeterministicBattleRules(damageCalculator, victoryChecker, randomSeed);
```

### 2. 캐릭터 데이터 생성

```csharp
// 전투용 캐릭터 데이터 생성
var characterData = CharacterDataFactory.Create(blueprint, userCard, isPlayer, instanceId);

// 전투 상태 초기화
var characters = new Dictionary<long, DeterministicCharacterData>
{
    { characterData.InstanceId, characterData }
};
var battleState = new BattleState(characters, BattleStatus.Ongoing);
```

### 3. 액션 처리

```csharp
// 기본 공격
var attackAction = new AttackAction(attackerId, targetId);
var (newState, events) = battleRules.ProcessTick(currentState, new[] { attackAction }, deltaTimeMs);

// 스킬 사용
var skillAction = new SkillAction(casterId, targetId, skillData);
var (newState2, events2) = battleRules.ProcessTick(newState, new[] { skillAction }, deltaTimeMs);
```

### 4. 결정론성 테스트

```csharp
// 테스트 컴포넌트 사용
var testComponent = gameObject.AddComponent<DeterministicBattleTest>();
testComponent.RunAllTests(); // 모든 결정론성 테스트 실행
```

## 중요한 설계 원칙

### 1. 불변성 (Immutability)
- `BattleState`는 직접 수정되지 않음
- 모든 변경은 새로운 상태 객체 생성을 통해 이루어짐
- 이벤트는 상태 변경과 분리되어 수집됨

### 2. 순수 함수 (Pure Functions)
- 모든 전투 로직은 부작용 없는 순수 함수
- 동일한 입력에 대해 항상 동일한 출력 보장
- 외부 상태에 의존하지 않음

### 3. 결정론적 난수
- `System.Random`을 시드 기반으로 사용
- 서버에서 제공하는 시드로 클라이언트 동기화
- 모든 확률 계산이 예측 가능

### 4. 정수 기반 계산
- 모든 수치 계산을 정수로 처리 (100 = 1.0배)
- 부동소수점 오차 완전 제거
- 플랫폼 간 일관성 보장

## 확장 가이드

### 1. 새로운 스킬 효과 추가

```csharp
// ISkillEffectStrategy 구현
public class NewEffectStrategy : ISkillEffectStrategy
{
    public (BattleState newState, List<BattleEvent> events) ApplyEffect(
        BattleState currentState, 
        DeterministicCharacterData caster, 
        DeterministicCharacterData target, 
        SkillEffectData effectData, 
        DeterministicBattleRules rules)
    {
        // 새로운 효과 로직 구현
        // 반드시 새로운 상태 객체 반환
    }
}
```

### 2. 새로운 액션 타입 추가

```csharp
// IAction 구현
public class NewAction : IAction
{
    public (BattleState newState, List<BattleEvent> newEvents) Execute(
        BattleState currentState, 
        DeterministicBattleRules rules)
    {
        // 새로운 액션 로직 구현
        // 반드시 불변성 원칙 준수
    }
}
```

### 3. 새로운 데미지 수정자 추가

```csharp
// IDamageModifier 구현
public class NewDamageModifier : IDamageModifier
{
    public long ModifyDamage(long damage, DeterministicCharacterData attacker, 
                           DeterministicCharacterData defender, DamageContext context)
    {
        // 데미지 수정 로직 구현 (정수 기반)
        return modifiedDamage;
    }
    
    public int Priority => 100; // 적용 순서
}
```

## 성능 최적화

### 1. 객체 풀링
- `DeterministicCharacterData` 복사 생성자 활용
- 상태 객체 재사용으로 GC 압박 감소

### 2. 딕셔너리 최적화
- 스킬/장비 매니저의 사전 초기화
- 런타임 검색 성능 향상

### 3. 이벤트 배치 처리
- 틱 단위로 이벤트 일괄 수집
- UI 업데이트 최적화

## 문제 해결

### 1. 결정론성 문제
- `DeterministicBattleTest` 실행으로 검증
- 시드 값 일치 확인
- 부동소수점 연산 사용 여부 점검

### 2. 성능 문제
- 프로파일러로 병목 지점 확인
- 객체 생성 빈도 모니터링
- 딕셔너리 조회 최적화

### 3. 동기화 문제
- 서버-클라이언트 시드 일치 확인
- 액션 순서 동일성 검증
- 열거형 값 일치 확인

## 결론

본 결정론적 전투 시스템은 완전한 서버-클라이언트 동기화를 보장하며, 확장 가능하고 유지보수가 용이한 아키텍처를 제공합니다. 모든 설계 원칙을 준수하여 개발하면 안정적이고 예측 가능한 전투 시스템을 구축할 수 있습니다. 