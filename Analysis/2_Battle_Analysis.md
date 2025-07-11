# 2. 전투 (Battle) 기능 상세 분석

이 문서는 DungeonMaster 프로젝트의 전투 관련 모든 스크립트에 대한 기능, 역할, 그리고 핵심 설계 원칙을 상세히 기술합니다.

---

## 2.1. `BattleManager.cs`

### **개요 및 역할**

`BattleManager`는 결정론적 전투 시스템의 **총괄 지휘자(Conductor)** 이자 **중재자(Mediator)** 입니다.
이 클래스는 Unity 씬에 존재하며, 전투의 시작, 진행, 종료와 같은 모든 생명주기를 관리합니다.

전투에 필요한 모든 핵심 구성요소들(규칙, 상태, 입력, 뷰 등)을 조립하고, 이들 간의 복잡한 상호작용을 직접 조율하여 각 모듈의 독립성을 보장하는 중추적인 역할을 담당합니다.

<br>

### **핵심 설계 원칙**

- **중재자 패턴 (Mediator Pattern)**: `BattleManager`는 전투의 모든 구성요소와 직접 통신하는 유일한 객체입니다. 예를 들어, `AIActionInputProvider`는 `DeterministicBattleRules`를 직접 알지 못하며, 오직 `BattleManager`를 통해서만 상호작용합니다. 이 설계는 각 모듈 간의 결합도를 극단적으로 낮춰, 시스템을 유연하고 확장 가능하게 만듭니다.

- **의존성 주입 (Dependency Injection)**: `ICharacterFactory`와 같은 핵심 의존성을 `[SerializeField]`를 통해 외부(Unity 인스펙터)에서 주입받습니다. 이를 통해 뷰를 생성하는 방식(예: 일반 생성 vs 오브젝트 풀링)이 변경되어도 `BattleManager` 자신은 전혀 수정할 필요가 없어집니다.

- **로직과 뷰의 분리 (Separation of Concerns)**: `BattleManager`는 '데이터(`DeterministicCharacterData`)'와 '뷰(`ICharacter`)'를 `_characterViewMap`을 통해 연결하는 다리 역할을 합니다. 전투 로직은 순수 데이터 객체만을 대상으로 연산을 수행하며, `BattleManager`는 그 결과를 받아서 뷰에 반영하는 책임만 가집니다.

<br>

### **주요 기능 및 로직 흐름**

#### **1. 전투 초기화 (`StartBattle`)**

1.  **의존성 확인 및 초기화**: 전투에 필요한 모든 구성요소(`_battleRules`, `_actionInputProvider` 등)를 생성하고, 전투 시드(Seed)를 설정하여 결정론적 환경을 구축합니다.

2.  **데이터 생성**: `CharacterDataFactory`를 호출하여 전투에 참여할 아군 및 적군의 **데이터(`DeterministicCharacterData`)** 를 생성합니다.

3.  **뷰 생성 및 매핑**: 주입받은 `ICharacterFactory`를 통해 데이터에 맞는 **뷰(`ClientMonster`)** GameObject를 생성하고, 데이터의 고유 ID와 뷰 인스턴스를 `_characterViewMap` 딕셔너리에 저장하여 1:1로 매핑합니다.

4.  **전투 시작**: `IsBattleActive` 플래그를 `true`로 설정하고, 생성된 초기 상태를 모든 뷰에 즉시 반영하여 전투를 개시합니다.

<br>

#### **2. 전투 진행 (Unity `Update` 루프)**

매 프레임마다 다음과 같은 정해진 순서로 전투 틱(Tick)을 처리합니다.

1.  **입력 수집**: `IActionInputProvider`(_현재는 `AIActionInputProvider`_)에게 현재 전투 상태를 전달하고, 이번 틱에 AI가 수행할 행동 목록(`ActionInput`)을 요청합니다.

2.  **액션 변환**: 반환된 `ActionInput` 데이터 객체를 실제 실행 가능한 **커맨드 객체**(`IAction` 구현체, 예: `AttackAction`)로 변환합니다.

3.  **틱 처리 위임**: 생성된 커맨드 객체 목록을 `DeterministicBattleRules.ProcessTick` 메서드에 전달하여, 한 틱 동안의 전투 시뮬레이션을 **'전투의 두뇌'**에 완전히 위임합니다.

4.  **결과 수신 및 반영**: `ProcessTick`으로부터 **새로운 상태(`newState`)** 와 해당 틱에서 **발생한 모든 이벤트(`events`)** 를 반환받습니다. `_currentBattleState`를 새로운 상태로 교체하고, `ApplyStateToViews`를 호출하여 모든 뷰에 변경된 내용을 시각적으로 반영합니다.

5.  **이벤트 처리**: `events` 목록을 순회하며, 각 이벤트(데미지, 힐, 버프 적용 등)에 맞는 뷰의 메서드(예: `OnDamageReceived`)를 호출하여 데미지 숫자 팝업, 파티클 효과 재생 등의 시각적 피드백을 트리거합니다.

<br>

#### **3. 전투 종료 (`EndBattle`)**

`Update` 루프의 마지막 단계에서 `IVictoryConditionChecker`를 통해 현재 상태가 승리 또는 패배 조건을 만족하는지 확인합니다.

-   조건이 충족되면 `IsBattleActive`를 `false`로 설정하여 전투 루프를 중단시킵니다.
-   `OnBattleEnd` 이벤트를 발생시켜, 전투 결과에 따른 보상 처리 등 후속 로직을 외부에 알립니다.
-   `ClearPreviousBattle`을 호출하여 생성했던 모든 뷰 오브젝트를 파괴하지 않고 `ICharacterFactory`의 `Release`를 통해 오브젝트 풀에 반납하고, 모든 상태 데이터를 정리합니다.

---

## 2.2. `DeterministicBattleRules.cs`

### **개요 및 역할**

`DeterministicBattleRules`는 결정론적 전투 시스템의 **두뇌(Brain)** 입니다.
이 클래스는 Unity 엔진의 어떤 기능에도 의존하지 않는 **순수 C# 로직**으로만 구성되어, 시스템의 예측 가능성과 테스트 용이성을 극대화합니다.

주요 역할은 `BattleManager`로부터 현재 상태(`BattleState`)와 이번 틱에 수행될 행동들(`IAction` 리스트)을 전달받아, 한 틱(tick) 동안의 전투를 시뮬레이션하고 그 결과로 발생한 **새로운 상태**와 **이벤트 목록**을 반환하는 것입니다.

<br>

### **핵심 설계 원칙**

- **불변성 (Immutability)**: 이 클래스의 가장 중요한 설계 원칙입니다. `ProcessTick` 메서드는 전달받은 `currentState`를 직접 수정하는 대신, `currentState.Clone()`을 통해 복사본을 만듭니다. 모든 계산과 액션 실행은 이 복사본을 대상으로 이루어지며, 최종적으로 완전히 새로운 상태 객체가 반환됩니다. 이는 부수 효과(Side Effect)를 원천적으로 차단하여 코드의 안정성과 예측 가능성을 보장하는 매우 훌륭한 설계입니다.

- **결정론적 환경 (Deterministic Environment)**:
    - **난수**: 생성자에서 `seed` 값을 받아 `System.Random`을 초기화합니다. 전투 중 치명타 계산이나 AI의 타겟팅과 같은 모든 확률적 요소는 `UnityEngine.Random`이 아닌, 시드 기반으로 생성된 이 `_random` 인스턴스를 통해 처리됩니다. 이를 통해 동일한 시드와 동일한 입력에 대해서는 언제나 100% 동일한 전투 결과가 보장됩니다.
    - **순수 로직**: Unity의 생명주기나 `Time.deltaTime` 같은 비결정적 요소에 직접 의존하지 않고, `BattleManager`로부터 `long` 타입의 `deltaTimeMs`를 명시적으로 전달받아 시간을 처리합니다.

- **책임 연쇄 패턴 (Chain of Responsibility)**: 복잡한 데미지 계산 과정을 `IDamageCalculationStep` 인터페이스를 구현하는 여러 개의 작고 독립적인 단계(Step)로 분리합니다. 생성자에서 이 단계들을 원하는 순서대로 조립하여 **데미지 계산 파이프라인**을 구성하고 `DefaultDamageCalculator`에 주입합니다. 이 설계 덕분에 새로운 데미지 공식을 파이프라인에 추가하거나 기존 공식의 순서를 변경하는 작업이 매우 유연해집니다.

<br>

### **주요 기능 및 로직 흐름**

#### **1. 생성자 (`DeterministicBattleRules`)**

- `BattleManager`로부터 전투 설정, 시드 값, 그리고 `ElementalAffinityTable` 같은 핵심 데이터를 주입받습니다.
- 결정론적 `Random` 인스턴스를 생성합니다.
- 데미지 계산 파이프라인(`BaseDamageStep`, `CriticalDamageStep` 등)을 조립하여 `DefaultDamageCalculator`를 생성합니다.

<br>

#### **2. 틱 처리 (`ProcessTick`)**

1.  **상태 복사**: `currentState.Clone()`을 호출하여 불변성을 보장하며 계산을 시작합니다.

2.  **시간 경과 처리**: `deltaTimeMs`만큼 모든 캐릭터의 버프/디버프 지속시간을 갱신하고(`ApplyAllBuffEffects`), 스킬 및 공격 쿨타임을 감소시킵니다(`UpdateAllCooldowns`).

3.  **액션 실행**: `BattleManager`로부터 전달받은 `IAction` 목록을 순차적으로 실행합니다. 각 액션의 `Execute` 메서드는 수정된 **'다음 상태'**와 **'발생한 이벤트'**를 반환하며, 이 결과가 다음 액션의 입력으로 연결됩니다.

4.  **결과 반환**: 모든 계산이 완료된 최종 `nextState`와, 해당 틱 동안 발생한 모든 `BattleEvent` 목록을 튜플 `(BattleState, List<BattleEvent>)` 형태로 `BattleManager`에 반환합니다.

---

## 2.3. 행동 (Actions) - 커맨드 패턴

전투 중 발생하는 모든 행동은 **커맨드 패턴(Command Pattern)**을 통해 하나의 '액션 객체'로 캡슐화됩니다.
이 패턴은 행동의 **요청**(`BattleManager`가 객체 생성)과 **실행**(`DeterministicBattleRules`가 `Execute` 호출)을 분리하여 시스템의 유연성을 높입니다.

### `IAction.cs`
- **역할**: 모든 액션 객체가 구현해야 하는 공통 인터페이스입니다.
- **핵심 메서드**: `Execute(currentState, battleRules)`
    - 이 메서드는 `DeterministicBattleRules`의 `ProcessTick`처럼 **불변성**을 따릅니다.
    - 현재 상태를 직접 수정하지 않고, 액션이 적용된 후의 **새로운 상태**와 그 과정에서 발생한 **이벤트 목록**을 튜플로 반환합니다.

<br>

### `AttackAction.cs`
- **역할**: '기본 공격'이라는 구체적인 행동을 캡슐화하는 커맨드 객체입니다.
- **핵심 설계**:
    - **로직 통합**: 기본 공격을 별도의 로직으로 만들지 않고, `SkillManager`에서 '기본 공격용 스킬 데이터'를 가져와 **스킬 시스템을 재사용**합니다. 이는 "3타마다 추가 데미지"와 같은 복잡한 효과를 기본 공격에 부여할 때, 스킬 시스템을 그대로 활용할 수 있게 하는 매우 확장성 높은 설계입니다.
    - **책임 위임**: 실제 데미지 계산은 `rules.DamageCalculator.Calculate(...)`를 호출하여 데미지 계산 파이프라인에 완전히 위임합니다. `AttackAction`은 데미지 공식을 전혀 알 필요가 없습니다.

<br>

### `SkillAction.cs`
- **역할**: '스킬 사용'이라는 구체적인 행동을 캡슐화하는 커맨드 객체입니다.
- **핵심 설계**:
    - **전략 패턴 (Strategy Pattern) 결합**: `SkillAction` 자체는 커맨드 객체이지만, 스킬의 구체적인 효과 처리는 `SkillEffectStrategyFactory`를 통해 얻어온 다양한 **전략 객체**(`ISkillEffectStrategy` 구현체)에 위임합니다.
    - **개방-폐쇄 원칙 (OCP)**: 이 설계 덕분에 "마나 번"이나 "적의 버프 제거" 같은 새로운 스킬 효과를 추가할 때, `SkillAction` 코드를 전혀 수정할 필요 없이 새로운 `Strategy` 클래스를 만들어 `Factory`에 등록하기만 하면 됩니다. 시스템의 확장성이 극대화된 우수한 사례입니다.
    - **상태 전파**: 한 스킬이 여러 효과를 가질 경우(예: 자신에게 공격력 버프 후 공격), 첫 번째 효과로 변경된 `nextState`에서 캐릭터 정보를 다시 조회하여 다음 효과 계산에 반영합니다. 이는 한 스킬 내의 여러 효과가 서로에게 올바르게 영향을 미치도록 보장하는 중요한 디테일입니다.

---

## 2.4. 행동 입력 (Action Inputs) - 전략 패턴

`BattleManager`는 매 틱마다 '누가 어떤 행동을 할 것인지'를 결정해야 합니다. 이 행동의 주체(AI, 플레이어 등)를 시스템과 분리하기 위해 **전략 패턴(Strategy Pattern)**이 사용됩니다.

### `IActionInputProvider.cs`
- **역할**: 행동 입력을 제공하는 모든 주체(AI, 플레이어 컨트롤러 등)가 구현해야 하는 공통 인터페이스입니다.
- **핵심 메서드**: `CollectActionInputs(currentState, battleRules)`
    - 이 메서드를 통해 `BattleManager`는 행동 입력의 소스가 AI인지, 네트워크로 연결된 다른 플레이어인지 전혀 신경 쓸 필요 없이, 단지 "이번 틱의 행동 목록을 달라"고 요청하기만 하면 됩니다.

<br>

### `AIActionInputProvider.cs`
- **역할**: `IActionInputProvider` 인터페이스의 AI 버전 구현체로, AI 캐릭터들의 행동을 결정하는 '전략'입니다.
- **핵심 로직**:
    - **우선순위 기반 행동 결정**:
        1. **스킬 우선**: `currentState`의 모든 AI 캐릭터를 순회하며, 사용 가능한 스킬(쿨타임이 완료된 스킬)이 있는지 먼저 확인합니다.
        2. **기본 공격**: 사용할 스킬이 없다면, 기본 공격 쿨타임이 완료되었는지 확인합니다.
        3. **타겟팅**: 스킬이나 기본 공격을 하기로 결정했다면, `FindBestTargetFor`를 호출하여 공격 대상을 찾습니다.
    - **결정론적 타겟팅**:
        - `FindBestTargetFor` 메서드는 단순히 살아있는 적 목록 중 하나를 무작위로 고르지만, 이때 `UnityEngine.Random`이 아닌 `battleRules.GetRandomInt`를 사용합니다.
        - 이를 통해 AI의 타겟 선택 또한 결정론적으로 이루어져, 전투 결과를 100% 재현할 수 있게 됩니다.
        - 이 메서드는 향후 "도발에 걸린 대상 우선", "어그로 수치가 가장 높은 대상 우선" 등 복잡한 타겟팅 전략으로 쉽게 확장될 수 있는 구조를 가지고 있습니다.

---

## 2.5. 승패 판정 (Victory Condition) - 전략 패턴

전투의 승리/패배 조건을 결정하는 로직 또한 **전략 패턴**을 통해 `BattleManager`와 분리되어 있습니다.

### `IVictoryConditionChecker.cs`
- **역할**: 모든 승패 판정 로직이 구현해야 하는 공통 인터페이스입니다.
- **핵심 메서드**: `Check(currentState)`
    - `BattleManager`는 매 틱의 마지막에 이 메서드를 호출하여 전투가 끝났는지 확인합니다.
    - `BattleManager`는 구체적인 승패 조건을 알 필요가 없으며, 단지 반환된 `BattleOutcome` (Victory, Defeat, Ongoing) 값에 따라 다음 행동을 결정할 뿐입니다.

<br>

### `DefaultVictoryConditionChecker.cs`
- **역할**: `IVictoryConditionChecker` 인터페이스의 가장 기본적인 구현체입니다.
- **핵심 로직**:
    1.  `currentState`의 캐릭터 목록에서 살아있는 플레이어측 캐릭터(`IsPlayerCharacter == true`)가 있는지 확인합니다.
    2.  살아있는 적측 캐릭터(`IsPlayerCharacter == false`)가 있는지 확인합니다.
    3.  플레이어측이 전멸했다면 **패배 (`Defeat`)**를, 적측이 전멸했다면 **승리 (`Victory`)**를, 양측 모두 생존자가 있다면 **진행중 (`Ongoing`)**을 반환합니다.
- **확장성**: 이 구조 덕분에 "5분 버티기", "특정 NPC 호위" 등 새로운 승리 조건을 가진 전투를 만들 때, `BattleManager`를 수정할 필요 없이 `IVictoryConditionChecker`를 구현하는 새로운 '전략' 클래스를 만들어 교체하기만 하면 됩니다.

---

## 2.6. 데미지 계산 파이프라인 - 책임 연쇄 패턴

복잡한 데미지 계산 로직은 **책임 연쇄 패턴(Chain of Responsibility Pattern)**을 통해 여러 개의 독립적인 단계로 분리되어 관리됩니다. 이 파이프라인은 `DeterministicBattleRules`의 생성자에서 조립됩니다.

### `IDamageCalculationStep.cs`와 `DamageCalculationContext`
- **`IDamageCalculationStep`**: 모든 데미지 계산 단계(Step)가 구현해야 하는 공통 인터페이스입니다. `Calculate(context)`라는 단일 메서드만 가집니다.

- **`DamageCalculationContext`**: 이 패턴의 핵심입니다. 데미지 계산에 필요한 모든 데이터(공격자, 방어자, 스킬, 현재까지 계산된 데미지, 치명타 여부, 최종 방어력 등)를 담는 **하나의 컨텍스트 객체**입니다.

<br>

### **파이프라인 동작 방식**

`DefaultDamageCalculator`가 `Calculate`를 호출하면, 조립된 `IDamageCalculationStep` 목록을 순서대로 실행합니다.

1.  `DamageCalculationContext` 객체가 생성되어 파이프라인에 주입됩니다.
2.  각 `Step`은 이 `context` 객체를 전달받아, 필요한 값을 읽고 자신이 책임지는 계산을 수행한 뒤, 그 결과를 다시 `context` 객체에 덮어씁니다.
3.  다음 `Step`은 이전 단계까지의 계산이 모두 반영된 `context` 객체를 받아 자신의 계산을 이어갑니다.
4.  모든 파이프라인이 끝나면, `context.CurrentDamage`에 최종 데미지 값이 담기게 됩니다.

<br>

### **주요 Step 클래스 예시**

- **`BaseDamageStep`**: 파이프라인의 첫 단계. 공격자의 공격력과 스킬 계수를 기반으로 **기본 데미지**를 계산하여 `context.CurrentDamage`에 기록합니다.
- **`CriticalDamageStep`**: `context.CurrentDamage`를 읽고, **결정론적 난수**를 사용하여 치명타 여부를 판정합니다. 치명타가 발생하면 정해진 배율만큼 데미지를 증폭시켜 다시 `context.CurrentDamage`에 기록하고, `context.IsCritical`을 `true`로 설정합니다.
- **`PenetrationStep`**: 방어자의 방어력과 공격자의 방어 관통력을 계산하여 **최종 방어력**을 `context.FinalDefense`에 기록합니다.
- **`DefenseStep`**: 파이프라인 후반부에 위치하며, `context.FinalDefense`에 기록된 값을 읽어 `context.CurrentDamage`에서 빼는 단순한 역할을 수행합니다.

이 설계는 데미지 공식의 순서를 바꾸거나(예: 방어력 계산을 치명타보다 먼저 적용), 새로운 계산 로직(예: "대상이 출혈 상태일 때 추가 데미지" Step)을 추가하는 작업을 기존 코드에 거의 영향을 주지 않고 매우 쉽게 만들어 줍니다. 