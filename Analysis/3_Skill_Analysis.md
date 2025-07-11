# 3. `Skill` 시스템 분석

이 문서에서는 `Skill` 폴더 내의 스크립트들을 분석하고, 스킬 데이터 구조, 효과 처리 방식, 그리고 시스템 전반의 설계 원칙을 설명합니다.

---

## 3.1. `SkillData.cs` - 스킬 데이터의 청사진

### **개요 및 역할**

`SkillData.cs`는 `ScriptableObject`를 상속받는 클래스로, 개별 스킬의 모든 정보를 담고 있는 **데이터 컨테이너이자 청사진(Blueprint)** 입니다.
이 설계를 통해 프로그래머가 아닌 게임 기획자나 디자이너도 Unity 에디터 내에서 직접 스킬을 생성하고, 이름, 아이콘부터 데미지 계수, 쿨타임, 각종 효과에 이르기까지 모든 속성을 손쉽게 편집하고 관리할 수 있습니다.

<br>

### **핵심 구성 요소**

#### **1. 데이터 필드**
- **기본 정보**: `skillId` (고유 숫자 ID), `skillName`, `icon`, `skillType` (Active/Passive), `skillGrade` (D~S 등급) 등 스킬의 정체성을 정의하는 기본 데이터입니다.
- **데미지 공식 관련**: `elementType` (속성), `skillCoefficient` (데미지 배율), `hitCount` 등 `Battle` 시스템의 데미지 계산 파이프라인과 직접적으로 연동되는 핵심 수치입니다.
- **사용 조건**: `cooldown` (쿨타임), `manaCost` (마나 소모량), `targetType` (타겟 종류) 등 스킬 사용의 제약 조건을 정의합니다.
- **스킬 효과 (`SkillEffectData[]`)**: 이 스킬이 가진 모든 구체적인 효과(데미지, 힐, 버프 부여 등)를 배열 형태로 저장합니다. 이 데이터는 `SkillAction`에서 **전략 패턴(Strategy Pattern)**을 통해 실제 행동으로 변환되는 가장 핵심적인 부분입니다.

<br>

#### **2. 레벨 기반 동적 스케일링**
- `GetScaledEffects`, `GetScaledCooldown`, `GetScaledManaCost`와 같은 `GetScaled...` 메서드들은 스킬의 현재 레벨을 인자로 받아, 레벨에 맞게 **동적으로 계산된 최종 수치**를 반환합니다.
- 실제 스케일링 로직은 `IndividualScaling`이라는 별도의 클래스에 위임됩니다. 이를 통해 각 속성(쿨타임, 효과 수치 등)이 서로 다른 성장 공식(선형 증가, 지수 증가 등)을 가질 수 있어 매우 유연한 레벨 디자인이 가능합니다.

<br>

#### **3. 성능 최적화를 위한 캐싱**
- 레벨별 스케일링 값이나 동적으로 생성되는 설명 텍스트는 매번 계산하기에 비용이 많이 드는 작업입니다.
- `SkillData`의 모든 `GetScaled...` 메서드와 설명 생성 메서드는 `SkillDataCache`라는 정적 클래스를 통해 **계산 결과를 캐싱(메모이제이션)**합니다.
- 즉, `(특정 스킬, 특정 레벨)` 조합에 대한 계산은 최초 한 번만 수행되고, 이후 동일한 요청에 대해서는 캐시에서 값을 즉시 반환하여 불필요한 연산을 막고 성능을 최적화합니다.

<br>

#### **4. 동적 설명 생성 및 현지화**
- `GetCompletePreview`나 `GetDetailedDescription` 같은 메서드들은 `descriptionTemplate` 필드에 `{value1}`, `{duration}`과 같은 특수한 형식의 placeholder를 포함한 템플릿 문자열을 넣어두면, 현재 스킬 레벨에 맞는 값으로 동적으로 치환하여 최종 UI에 표시될 설명 텍스트를 생성합니다.
- 이 과정에서 `LocalizationManager`를 사용하여 모든 텍스트(예: "초", "공격력")를 가져오므로, 다국어 처리까지 완벽하게 지원하는 유연한 시스템입니다.

---

## 3.2. 스킬 효과 처리 - 전략 & 팩토리 패턴

`SkillData`에 정의된 추상적인 효과 데이터(`SkillEffectData`)는 **전략 패턴(Strategy Pattern)**을 통해 구체적인 행동으로 변환됩니다. `SkillAction`은 이 패턴을 사용하여 스킬 효과를 실행합니다.

### `ISkillEffectStrategy.cs` (전략 인터페이스)
- **역할**: `Damage`, `Heal`, `Buff` 등 스킬의 개별 효과를 실제로 실행하는 모든 '전략' 클래스가 구현해야 하는 공통 인터페이스입니다.
- **핵심 메서드**: `Execute(battleState, rules, caster, target, skill, effect)`
    - 이 메서드는 효과를 적용하는 데 필요한 모든 정보(현재 상태, 규칙, 시전자, 대상자 등)를 인자로 받습니다.
    - `Battle` 시스템의 다른 부분들과 마찬가지로 **불변성(Immutability)** 원칙을 철저히 지킵니다. 현재 상태(`battleState`)를 직접 수정하지 않고, 효과가 적용된 후의 **새로운 상태(`BattleState`)** 객체를 생성하여 반환합니다. 이는 시스템 전체의 안정성과 예측 가능성을 보장하는 일관된 설계 철학입니다.

<br>

### `SkillEffectStrategyFactory.cs` (전략 팩토리)
- **역할**: `SkillEffectType` 열거형 값을 기반으로, 그에 맞는 구체적인 전략 객체(`ISkillEffectStrategy` 구현체)를 찾아 반환해주는 **공장(Factory)** 역할을 수행합니다.
- **디자인 패턴 활용**:
    - **싱글턴 패턴 (Singleton Pattern)**: `Instance` 프로퍼티를 통해 시스템 전체에서 단 하나의 팩토리 인스턴스만 존재하도록 보장합니다. 팩토리는 생성 시점에 모든 전략(`DamageStrategy`, `HealStrategy` 등)을 `Dictionary`에 미리 등록해 둡니다.
    - **전략 패턴의 핵심**: `SkillAction`은 `factory.GetStrategy(effect.EffectType)`을 호출하는 것만으로, 효과의 실제 로직이 어떤 클래스에 있는지 전혀 알 필요 없이 올바른 전략 객체를 얻을 수 있습니다.
    - **개방-폐쇄 원칙 (Open-Closed Principle)**: 이 설계는 시스템의 확장성을 극대화합니다. "적의 마나를 태우는" `ManaBurn`이라는 새로운 효과를 추가하고 싶을 때, 개발자는 `ManaBurnStrategy` 클래스를 새로 만들어 팩토리에 한 줄만 추가하면 됩니다. `SkillAction`을 포함한 기존의 어떤 코드도 수정할 필요가 없습니다.
    - **널 객체 패턴 (Null Object Pattern)**: 만약 `Dictionary`에 등록되지 않은 `SkillEffectType`을 요청받으면, `null`을 반환하여 `NullReferenceException`을 발생시키는 대신, 아무 동작도 하지 않는 `EmptySkillEffectStrategy` 객체를 반환합니다. 이는 예외 상황에서도 시스템이 멈추지 않고 안정적으로 동작하도록 보장하는 매우 견고한 설계입니다.

---

## 3.3. 구체적인 전략 클래스 예시

`ISkillEffectStrategy` 인터페이스를 구현하는 구체적인 전략 클래스들은 각자 명확하게 분리된 책임을 가집니다.

### `DamageStrategy.cs` (데미지 전략)
- **역할**: 스킬 효과 타입이 `Damage`일 때의 행동을 정의합니다.
- **책임 위임 (Delegation)**:
    - 이 클래스는 데미지를 **'어떻게'** 계산해야 하는지 전혀 알지 못합니다.
    - `rules.DamageCalculator.Calculate(...)`를 호출하여, 데미지 계산의 모든 복잡한 과정을 `Battle` 시스템의 책임 연쇄 파이프라인에 완전히 위임합니다.
- **단일 책임 원칙 (Single Responsibility Principle)**:
    - `DamageStrategy`의 책임은 오직 "계산된 데미지 결과를 받아와 대상의 `CurrentHP`를 변경하고, `Damage`와 `Death` 이벤트를 생성하는 것"에 한정됩니다.
    - 데미지 공식(예: 치명타 공식, 방어력 공식)이 아무리 복잡하게 변경되어도 이 클래스는 수정할 필요가 전혀 없습니다.

<br>

### `BuffStrategy.cs` (버프 전략)
- **역할**: 스킬 효과 타입이 `Buff`일 때의 행동을 정의합니다.
- **상태 관리의 책임**:
    - `DamageStrategy`와 달리, 이 전략은 대상(`target`)의 상태, 구체적으로는 `ActiveBuffs` 리스트를 직접 조작하는 더 복잡한 책임을 가집니다.
- **핵심 로직 분기**:
    1.  먼저 `BuffManager.Instance.GetBlueprint(...)`를 호출하여 버프의 원본 데이터(지속시간, 최대 중첩 등)를 가져옵니다.
    2.  대상이 이미 동일한 버프를 가지고 있는지 확인합니다.
    3.  **버프가 이미 있다면 (Re-apply)**: 버프의 중첩(Stack)을 1 올리고, 지속시간을 갱신하며, `IBuffEffect`의 `OnReapply` 로직을 호출합니다.
    4.  **새로운 버프라면 (Apply)**: `BuffManager`를 통해 새로운 `BuffData` 인스턴스를 생성하여 대상의 `ActiveBuffs` 리스트에 추가하고, `IBuffEffect`의 `OnApply` 로직을 호출합니다.
- **외부 시스템 연동**:
    - 데미지 전략이 `DamageCalculator`에 의존하는 것처럼, 버프 전략은 버프의 청사진(`Blueprint`)과 데이터 인스턴스 생성을 위해 `BuffManager`와, 버프의 실제 틱당/적용시 효과 로직을 위해 `BuffEffectRegistry`와 긴밀하게 연동됩니다.

이 두 클래스는 전략 패턴을 통해 어떻게 복잡한 시스템이 명확하고 독립적인 책임 단위로 분리될 수 있는지를 잘 보여주는 훌륭한 예시입니다.

---

## 3.4. `SkillManager.cs` - 스킬 데이터 중앙 관리

### **개요 및 역할**

`SkillManager`는 `ScriptableObject`이면서 동시에 **싱글턴(Singleton)**으로 동작하는 스킬 시스템의 중앙 허브입니다.
게임이 시작될 때 `Resources` 폴더에서 자신을 로드하여 단일 인스턴스를 유지하며, 게임 내 어디서든 `SkillManager.Instance`를 통해 모든 스킬 데이터에 접근할 수 있는 통로를 제공합니다.

<br>

### **핵심 기능**

#### **1. 데이터 로딩 및 딕셔너리 캐싱**
- **데이터 소스**: 기획자는 Unity 에디터에서 `SkillManager` 에셋의 `allSkills` 배열에 게임에 사용될 모든 `SkillData` 에셋을 미리 등록합니다.
- **초기화**: 최초 접근 시 `InitializeDictionaries` 메서드가 호출됩니다.
- **빠른 조회**: 등록된 모든 스킬을 순회하며 `skillId`를 키로 하는 `Dictionary<long, SkillData>`에 저장합니다. 이 덕분에 `GetSkill(skillId)` 메서드는 배열을 순회하는 `O(n)`이 아닌, 해시 테이블 검색 속도인 `O(1)`에 가까운 매우 빠른 속도로 원하는 스킬 데이터를 찾아낼 수 있습니다.

<br>

#### **2. 기본 공격 스킬의 동적 생성**
- **로직 일원화**: 이 시스템은 기본 공격 또한 하나의 '스킬'로 취급하여, `AttackAction`이 `SkillAction`과 유사한 로직을 공유할 수 있도록 설계되었습니다.
- **동적 생성**: 하지만 기본 공격은 모든 캐릭터가 공통으로 사용하므로 별도의 에셋 파일로 만들지 않습니다. 대신 `CreateBasicAttackSkill` 메서드가 `ScriptableObject.CreateInstance`를 사용하여 **런타임에 동적으로 기본 공격용 `SkillData` 객체를 생성**합니다.
- **ID 할당**: 이렇게 생성된 기본 공격 스킬은 고유 ID로 `0`을 할당받아 다른 스킬들과 동일한 방식으로 딕셔너리에서 관리됩니다.

<br>

#### **3. 몬스터 스킬 매핑**
- `monsterSkillMappings` 배열을 통해 어떤 이름의 몬스터가 어떤 스킬들을 사용하는지를 미리 정의해둘 수 있습니다.
- 이 데이터 역시 `Dictionary<string, SkillData[]>` 형태로 변환되어, `GetMonsterSkills("몬스터이름")`과 같은 직관적인 호출로 특정 몬스터의 스킬 목록을 빠르게 조회할 수 있습니다. 