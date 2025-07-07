# 🎮 Dungeon Master - 고급 스킬 시스템

Unity 기반 RPG 게임의 확장 가능한 스킬 시스템입니다. SOLID 원칙을 준수하여 설계되었으며, 데이터 기반의 유연한 스킬 생성과 관리를 지원합니다.

---

## 📋 목차

1. [시스템 개요](#-시스템-개요)
2. [SOLID 원칙 검토](#-solid-원칙-검토)
3. [핵심 컴포넌트](#-핵심-컴포넌트)
4. [설정 및 사용법](#-설정-및-사용법)
5. [테스트 시스템](#-테스트-시스템)
6. [확장 가이드](#-확장-가이드)
7. [문제 해결](#-문제-해결)

---

## 🎯 시스템 개요

### 주요 특징
- **데이터 기반**: ScriptableObject를 활용한 완전 데이터 기반 스킬 시스템
- **유연한 스케일링**: 6가지 스케일링 방식 (Linear, Exponential, Logarithmic, Step, Custom, None)
- **개별 스케일링**: 각 효과 요소별 독립적 스케일링 지원
- **다중 히트**: 3연격, 5연격 등 멀티히트 스킬 지원
- **동적 설명**: 템플릿 변수를 통한 실시간 스킬 설명 생성
- **확장성**: 새로운 효과 타입과 스케일링 방식 쉽게 추가 가능

### 아키텍처 다이어그램
```
SkillData (ScriptableObject)
    ├── SkillEffectData[] (효과 배열)
    │   ├── IndividualScaling (개별 스케일링)
    │   └── SkillEffectApplier (효과 적용)
    ├── DataSkill (ISkill 구현)
    └── SkillManager (팩토리 & 매핑)
```

---

## 🏗️ SOLID 원칙 검토

### ✅ Single Responsibility Principle (단일 책임 원칙)
- **SkillData**: 스킬 데이터 관리만 담당
- **SkillEffectData**: 효과 데이터와 스케일링 계산만 담당
- **SkillEffectApplier**: 효과 적용 로직만 담당
- **DataSkill**: 스킬 인스턴스와 레벨 관리만 담당
- **StatContainer**: 스탯 관리와 HP 처리만 담당

### ✅ Open/Closed Principle (개방/폐쇄 원칙)
- **새로운 효과 타입**: `SkillEffectType` enum 확장으로 새 효과 추가 가능
- **새로운 스케일링**: `SkillScalingType` enum과 `IndividualScaling` 확장
- **새로운 스킬**: ScriptableObject로 코드 수정 없이 무한 확장

### ✅ Liskov Substitution Principle (리스코프 치환 원칙)
- **ISkill 인터페이스**: DataSkill이 ISkill을 완전히 대체 가능
- **ICharacter 인터페이스**: Monster가 ICharacter를 완전히 대체 가능

### ✅ Interface Segregation Principle (인터페이스 분리 원칙)
- **ISkill**: 스킬에 필요한 최소한의 인터페이스만 제공
- **ICharacter**: 캐릭터에 필요한 기본 동작만 정의

### ✅ Dependency Inversion Principle (의존성 역전 원칙)
- **인터페이스 의존**: 구체 클래스가 아닌 ISkill, ICharacter 인터페이스에 의존
- **ScriptableObject**: 하드코딩 대신 데이터 에셋에 의존

### 🔄 개선 권장사항
1. **SkillEffectApplier**: Strategy 패턴으로 각 효과별 클래스 분리
2. **SkillData**: 긴 텍스트 처리 메서드를 별도 클래스로 분리
3. **Monster**: 생성자 오버로딩 정리

---

## 🧩 핵심 컴포넌트

### 1. SkillData.cs
**기능**: 스킬의 모든 데이터를 정의하는 ScriptableObject
```csharp
// 주요 프로퍼티
public string SkillName          // 스킬 이름
public SkillEffectData[] Effects // 효과 배열
public int MaxLevel              // 최대 레벨
public float Cooldown           // 쿨다운
```

**변경 방법**:
- Unity Inspector에서 모든 설정 변경 가능
- 새 스킬: `Assets > Create > Game > Skill Data`
- 레벨링: `maxLevel` 값 조정
- 효과 추가: `effects` 배열에 새 요소 추가

**설정 예시**:
```
Skill Name: "화염구"
Max Level: 10
Effects[0]:
  - Type: Damage
  - Fixed Value: 25
  - Percent Value: 80
  - Fixed Value Scaling: Linear (5 per level)
```

### 2. SkillEffect.cs
**기능**: 스킬 효과 데이터와 적용 로직
```csharp
// 스킬 효과 데이터
public struct SkillEffectData
{
    public SkillEffectType Type;
    public float FixedValue;      // 고정 데미지/힐량
    public float PercentValue;    // 퍼센트 데미지/힐량
    public int HitCount;          // 히트 수
    public IndividualScaling fixedValueScaling;
}
```

**변경 방법**:
- 새 효과: `SkillEffectType` enum에 추가 후 `ApplySingleHit()` 수정
- 히트수 조정: `HitCount` 값 변경
- 스케일링: 각 `IndividualScaling` 필드 설정

### 3. SkillManager.cs
**기능**: 스킬 생성과 몬스터별 스킬 매핑 관리
```csharp
// 주요 메서드
public ISkill CreateSkill(string skillName, int skillLevel)
public void AssignSkillsToMonster(Monster monster)
public SkillData[] GetMonsterSkills(string monsterName)
```

**설정 방법**:
1. `Resources/SkillManager.asset` 생성
2. `All Skills` 배열에 모든 스킬 데이터 추가
3. `Monster Skill Mappings`에서 몬스터별 스킬 매핑

### 4. SimpleBattleTest.cs
**기능**: Unity 씬 기반 전투 테스트 시스템
```csharp
// 주요 기능
- 씬에서 큐브 2개로 몬스터 시각화
- 실시간 HP 색상 변화
- Inspector 우클릭 테스트 메뉴
- 자동/수동 전투 모드
```

**설정 방법**:
1. 씬에 큐브 2개 배치
2. BattleTestManager 오브젝트 생성
3. SimpleBattleTest 컴포넌트 추가
4. Monster Card와 Skills 설정

### 5. IndividualScaling 구조체
**기능**: 각 요소별 독립적 스케일링 시스템
```csharp
public enum SkillScalingType
{
    None,         // 스케일링 없음
    Linear,       // 선형 증가
    Exponential,  // 지수 증가
    Logarithmic,  // 로그 증가
    Step,         // 단계별 증가
    Custom        // 커스텀 배수
}
```

**변경 방법**:
- **Linear**: `perLevelValue`로 레벨당 증가량 설정
- **Step**: `stepLevels`와 `stepValues` 배열 설정
- **Custom**: `customMultipliers` 배열에 레벨별 배수 설정

---

## ⚙️ 설정 및 사용법

### 1단계: 기본 설정
```bash
1. SkillManager.asset 생성
   - Assets/Resources/ 폴더에 생성
   - Create > Game > Skill Manager

2. 스킬 데이터 생성
   - Create > Game > Skill Data
   - 이름, 효과, 스케일링 설정

3. MonsterCard 연결
   - BaseMonsterCard에 사용할 스킬들 연결
```

### 2단계: 스킬 생성 예시
```yaml
# 화염구 스킬
Skill Name: "화염구"
Description: "적을 {hittext} 공격하여 {damage} 피해를 입힙니다{hitsuffix}!"
Cooldown: 2초
Max Level: 10

Effects[0]:
  Type: Damage
  Fixed Value: 25
  Percent Value: 80
  Hit Count: 1
  Fixed Value Scaling:
    Scaling Type: Linear
    Per Level Value: 5
  Percent Value Scaling:
    Scaling Type: Linear
    Per Level Value: 10
```

### 3단계: 테스트 실행
```bash
1. 테스트 씬 생성
2. 큐브 2개 배치 (-2,0,0), (2,0,0)
3. BattleTestManager 오브젝트 생성
4. SimpleBattleTest 컴포넌트 설정
5. Play 모드에서 테스트
```

---

## 🧪 테스트 시스템

### SimpleBattleTest 기능
- **시각적 피드백**: 몬스터 HP에 따른 큐브 색상 변화
- **Inspector 테스트**: 우클릭 메뉴로 즉시 테스트
- **자동 전투**: 코루틴 기반 턴제 전투
- **전투 로그**: Console과 UI에 실시간 로그

### 테스트 메뉴
```csharp
// Inspector 우클릭 메뉴
"Quick Test - Monster1 Attack"    // 몬스터1 공격
"Quick Test - Monster2 Attack"    // 몬스터2 공격  
"Quick Test - Start Auto Battle"  // 자동 전투 시작
"Quick Test - Reset Battle"       // 전투 리셋
```

### 색상 코드
- 🟢 **초록**: HP 70% 이상 (건강)
- 🟡 **노랑**: HP 30-70% (부상)
- 🔴 **빨강**: HP 30% 미만 (위험)
- ⚫ **회색**: HP 0% (사망)

---

## 🔧 확장 가이드

### 새로운 효과 타입 추가
```csharp
// 1. SkillEffectType enum에 추가
public enum SkillEffectType
{
    // ... 기존 효과들
    NewEffect,  // 새 효과
}

// 2. SkillEffectApplier.ApplySingleHit()에 케이스 추가
case SkillEffectType.NewEffect:
    ApplyNewEffect(caster, target, finalValue, effectData.Duration);
    break;

// 3. 새 효과 적용 메서드 구현
private static void ApplyNewEffect(ICharacter caster, ICharacter target, float value, float duration)
{
    // 새 효과 로직 구현
}
```

### 새로운 스케일링 방식 추가
```csharp
// 1. SkillScalingType enum에 추가
public enum SkillScalingType
{
    // ... 기존 방식들
    NewScaling,  // 새 스케일링
}

// 2. IndividualScaling.CalculateScaling()에 케이스 추가
case SkillScalingType.NewScaling:
    return CalculateNewScaling(level);

// 3. 새 계산 메서드 구현
private float CalculateNewScaling(int level)
{
    // 새 스케일링 로직 구현
    return result;
}
```

### 새로운 템플릿 변수 추가
```csharp
// SkillData.ProcessDescriptionTemplate()에 추가
template = template.Replace("{newvariable}", GetNewVariable(skillLevel).ToString());

private string GetNewVariable(int skillLevel)
{
    // 새 변수 계산 로직
    return result;
}
```

---

## 🐛 문제 해결

### 자주 발생하는 문제들

#### 1. 스킬이 적용되지 않음
```bash
원인: SkillManager.asset이 없거나 스킬이 등록되지 않음
해결: Resources/SkillManager.asset 생성 후 스킬 등록
```

#### 2. 몬스터가 생성되지 않음
```bash
원인: BaseMonsterCard의 ToMonsterCardData() 호환성 문제
해결: Monster 생성자 확인 및 기본 스탯 설정
```

#### 3. 스케일링이 작동하지 않음
```bash
원인: IndividualScaling 설정 누락
해결: 각 효과의 스케일링 설정 확인
```

#### 4. UI가 업데이트되지 않음
```bash
원인: UI 요소가 연결되지 않음
해결: SimpleBattleTest Inspector에서 UI 요소 연결
```

### 성능 최적화 팁
1. **스킬 캐싱**: 자주 사용하는 스킬은 미리 생성하여 캐싱
2. **효과 풀링**: 이펙트 오브젝트 풀링으로 GC 압박 감소
3. **배치 처리**: 다중 타겟 효과는 배치로 처리

---

## 📝 코드 컨벤션

### 네이밍 규칙
- **클래스**: PascalCase (예: `SkillData`)
- **메서드**: PascalCase (예: `GetScaledEffect`)
- **프로퍼티**: PascalCase (예: `SkillName`)
- **필드**: camelCase (예: `skillName`)
- **상수**: UPPER_SNAKE_CASE (예: `MAX_SKILL_LEVEL`)

### 주석 규칙
```csharp
/// <summary>
/// 메서드나 클래스의 기능 설명
/// </summary>
/// <param name="parameter">매개변수 설명</param>
/// <returns>반환값 설명</returns>
```

### 폴더 구조
```
Assets/Scripts/
├── Character/          # 캐릭터 관련 (Monster, StatContainer)
├── Skill/             # 스킬 시스템 (SkillData, SkillEffect, SkillManager)
├── Data/              # 데이터 클래스들 (BaseMonsterCard, MonsterCardData)
├── Test/              # 테스트 시스템 (SimpleBattleTest)
└── Editor/            # 에디터 스크립트들
```

---

## 🚀 향후 계획

### 단기 계획
- [ ] Strategy 패턴으로 효과 적용 시스템 리팩토링
- [ ] 버프/디버프 시스템 완성
- [ ] 스킬 애니메이션 시스템 연동

### 중기 계획  
- [ ] AI 기반 스킬 사용 패턴
- [ ] 스킬 조합 시스템
- [ ] 상태 이상 효과 확장

### 장기 계획
- [ ] 멀티플레이어 동기화
- [ ] 스킬 밸런싱 AI
- [ ] 사용자 정의 스킬 에디터

---

## 📄 라이선스

이 프로젝트는 교육 목적으로 제작되었습니다.

---

## 👥 기여자

- **개발**: Unity RPG Team
- **설계**: SOLID Principles 준수
- **테스트**: 씬 기반 테스트 시스템

**Happy Coding!** 🎮✨ 