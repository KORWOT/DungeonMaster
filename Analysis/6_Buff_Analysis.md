# 6. `Buff` 시스템 분석

이 문서에서는 `Buff` 폴더 내의 스크립트들을 분석하고, 버프의 데이터 구조, 생명주기(적용, 갱신, 제거), 효과 처리 방식, 그리고 시스템 전반의 설계 원칙을 설명합니다.

---

## 6.1. 버프 데이터 구조: 청사진과 인스턴스

버프 시스템은 `Card` 시스템과 마찬가지로, 변하지 않는 원본 데이터와 캐릭터에게 적용된 실제 상태 데이터를 명확하게 분리하는 **청사진(Blueprint)**과 **인스턴스(Instance)** 패턴을 따릅니다.

### `BuffBlueprint.cs` (버프의 청사진)
- **역할**: `ScriptableObject`를 사용하여 버프의 **불변(Immutable) 원본 데이터**를 정의합니다. Unity 에디터에서 기획자가 버프의 모든 기본 속성을 직접 설정할 수 있습니다.
- **주요 데이터**:
    - `buffId`: 버프의 고유 숫자 ID입니다.
    - `buffName`, `icon`, `description`: UI에 표시될 정보입니다.
    - `baseDurationSeconds`, `maxStacks`: 버프의 기본 지속시간과 최대 중첩 수를 정의합니다.
    - `effectType`: 이 버프가 어떤 종류의 효과(예: `StatChange`, `DamageOverTime`)를 내는지 식별하는 열거형 값입니다.
    - `targetStat`, `effectValue`: `StatChange` 타입의 버프가 어떤 스탯을 얼마나 변경할 것인지에 대한 구체적인 데이터입니다.
- **동적 스케일링**: `GetScaledDuration(level)`과 `GetScaledValue(level)` 메서드를 제공합니다. 이를 통해 버프의 중첩(level/stacks)이 쌓임에 따라 지속시간이나 효과 수치가 동적으로 변화하는 복잡한 로직을 구현할 수 있습니다.

<br>

### `BuffData.cs` (개별 버프 인스턴스)
- **역할**: `DeterministicCharacterData`의 `ActiveBuffs` 리스트에 저장되는, 캐릭터에게 실제로 적용된 **개별 버프 인스턴스의 가변(Mutable) 상태**를 담는 순수 C# 클래스입니다.
- **주요 상태 값**:
    - `BuffId`: 이 버프가 어떤 종류의 버프인지 식별하는 **'원본 청사진'의 ID**입니다.
    - `CasterId`, `TargetId`: 누가 누구에게 이 버프를 걸었는지 식별하는 ID입니다.
    - `RemainingDurationMs`: 버프의 남은 지속시간 (밀리초 단위)입니다. 이 값은 `DeterministicBattleRules`의 `ApplyAllBuffEffects` 메서드에서 매 틱마다 감소하며, 0이 되면 버프가 제거됩니다.
    - `Stacks`: 현재 버프의 중첩 수입니다. `maxStacks`를 초과할 수 없습니다.

<br>

### 데이터 흐름: Blueprint → Instance
1.  `Skill`의 `BuffStrategy` 등 버프를 적용하는 로gic이 호출됩니다.
2.  `BuffManager`는 인자로 받은 `buffId`를 이용해 `BuffBlueprint` (청사진)를 조회합니다.
3.  조회한 청사진의 정보를 바탕으로, 캐릭터에게 적용될 `BuffData` (인스턴스)를 생성합니다.
4.  이때 청사진의 `GetScaledDuration`을 호출하여 계산된 지속시간을 `BuffData.RemainingDurationMs`에 설정합니다.
5.  생성된 `BuffData` 객체는 대상 캐릭터의 `ActiveBuffs` 리스트에 추가되어, 전투 중에 실제 효과를 발휘하게 됩니다.

---

## 6.2. 버프 효과 처리: 전략 패턴과 생명주기

버프의 실제 효과는 `Skill` 시스템과 동일하게 **전략 패턴(Strategy Pattern)**을 통해 처리됩니다. 이를 통해 새로운 종류의 버프 효과를 추가하더라도 기존 코드를 수정할 필요가 없어 OCP(개방-폐쇄 원칙)를 준수합니다.

### `IBuffEffect.cs` (버프 효과 인터페이스)
- **역할**: 버프의 **생명주기(Life Cycle)**를 정의하는 핵심 인터페이스입니다. 모든 버프 효과 클래스(전략)는 반드시 이 인터페이스를 구현해야 합니다.
- **메서드 (생명주기)**:
    - `OnApply(state, ref buffData)`: 버프가 **처음 적용될 때** 단 한 번 호출됩니다. (예: 스탯 증가)
    - `OnTick(state, ref buffData)`: 버프가 활성화된 상태에서 **매 틱(tick)마다** 호출됩니다. (예: 초당 데미지, 초당 힐)
    - `OnRemove(state, buffData)`: 버프의 지속 시간이 만료되거나 해제될 때 호출됩니다. 적용했던 효과를 **되돌리는(revert)** 뒷정리 로직을 수행합니다. (예: 증가시켰던 스탯 원상복구)
    - `OnReapply(state, ref buffData)`: 동일한 버프가 다시 적용될 때(중첩) 호출됩니다. 지속 시간을 초기화하거나, 중첩 수(`Stacks`)를 증가시키고 차이 값을 적용하는 등의 로직을 처리합니다.

<br>

### `StatBuffEffect.cs` (스탯 변경 효과 구현체)
- **역할**: `IBuffEffect` 인터페이스의 구체적인 구현 클래스 중 하나로, `BuffEffectType.StatChange` 타입의 버프 효과를 실제로 구현합니다.
- **핵심 동작 방식**:
    1.  **OnApply**: 생성자를 통해 참조하고 있는 `BuffBlueprint`로부터 `targetStat`과 `GetScaledValue(1)` 값을 가져와 대상 캐릭터의 스탯에 더해줍니다.
    2.  **OnReapply**: 이미 버프가 적용된 상태에서 다시 적용되면, `(현재 스택의 값) - (이전 스택의 값)`의 **차이(delta)만큼만** 스탯을 추가로 적용하여 중복 계산을 방지하고 정확성을 보장합니다.
    3.  **OnRemove**: 버프가 제거될 때, 현재까지 적용된 총 스택에 해당하는 값(`GetScaledValue(현재_총_스택)`) 만큼을 스탯에서 **빼서** 캐릭터의 상태를 완벽하게 원상복구합니다.
- **설계의 우수성**: 이 클래스는 '어떤 스탯'을 '얼마나' 바꿀지에 대한 구체적인 값에 전혀 의존하지 않습니다. 모든 데이터는 `BuffBlueprint`로부터 오고, `StatBuffEffect`는 오직 '스탯을 변경하고 되돌린다'는 **행위(Behavior)**에만 집중합니다. 덕분에 새로운 스탯 버프(공격력, 방어력, 속도 등)가 수십 개 추가되어도 이 클래스는 단 하나도 수정할 필요가 없습니다.

---

## 6.3. 중앙 관리: 레지스트리와 매니저

이 모든 구성 요소를 하나로 묶어 시스템을 동작시키는 것은 `BuffEffectRegistry`와 `BuffManager`입니다. 두 클래스는 명확한 역할 분담을 통해 시스템을 효율적으로 관리합니다.

### `BuffEffectRegistry.cs` (버프 효과 팩토리 & 레지스트리)
- **역할**: **팩토리(Factory) 패턴**과 **레지스트리(Registry) 패턴**의 역할을 동시에 수행하는 `static` 클래스입니다.
- **동작 방식**:
    1.  **등록(Register)**: `BuffManager`가 게임 시작 시 모든 `BuffBlueprint`를 넘겨주면, `RegisterFromBlueprint` 메서드가 호출됩니다.
    2.  **생성(Create)**: 내부의 `CreateEffectFromBlueprint` 메서드가 `BuffBlueprint`의 `EffectType`을 `switch` 문으로 확인하고, 그에 맞는 `IBuffEffect` 구현체(예: `StatBuffEffect`)를 **생성(`new`)**하여 반환합니다. 이때 효과 로직에 필요한 원본 데이터인 `BuffBlueprint` 자체를 생성자의 인자로 넘겨줍니다.
    3.  **저장(Store)**: 생성된 효과 객체는 `Dictionary<long, IBuffEffect>`에 `buffId`를 Key로 하여 저장됩니다.
- **설계**: `static` 클래스로 구현되어, 전투 로직 등 어디서든 `BuffEffectRegistry.Get(buffId)`를 통해 특정 버프의 효과 로직 객체를 쉽게 가져올 수 있습니다. 새로운 버프 효과(예: `DotDamageEffect`)를 추가하려면, `IBuffEffect`를 구현하는 새 클래스를 만들고 `switch` 문에 한 줄만 추가하면 되므로 OCP(개방-폐쇄 원칙)를 완벽하게 지원합니다.

<br>

### `BuffManager.cs` (버프 시스템의 중앙 관리자)
- **역할**: **싱글턴(Singleton) 패턴**으로 구현된 `MonoBehaviour` 클래스이며, 버프 시스템의 전반적인 관리와 외부 시스템과의 상호작용을 담당하는 **퍼사드(Facade)** 역할을 합니다.
- **핵심 기능**:
    1.  **초기화(Initialization)**: `Awake()` 시점에 `Resources.LoadAll<BuffBlueprint>("Buffs")`를 통해 `Resources/Buffs` 폴더 안의 모든 `BuffBlueprint` 애셋을 로드합니다.
    2.  **레지스트리 등록**: 로드한 `BuffBlueprint`들을 `BuffEffectRegistry`에 전달하여, 버프 효과 로직들이 메모리에 등록되고 사용할 수 있도록 준비시킵니다.
    3.  **인스턴스 생성**: `CreateBuffInstance(buffId, ...)` 메서드는 `buffId`를 받아, 해당하는 `BuffBlueprint`를 찾고, `new BuffData(...)`를 통해 캐릭터에게 적용될 실제 **버프 인스턴스**를 생성하여 반환합니다. 이 과정에서 버프의 초기 지속시간과 스택이 설정됩니다.
- **역할 분담**: `BuffManager`는 **데이터 관리(청사진 로드, 인스턴스 생성)**에 집중하고, `BuffEffectRegistry`는 **로직 관리(효과 객체 생성 및 제공)**에 집중함으로써 SRP(단일 책임 원칙)를 명확하게 지킵니다. 