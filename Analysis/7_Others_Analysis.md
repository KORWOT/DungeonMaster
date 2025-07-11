# 7. 기타 시스템 분석 (Shared, Utility, Managers 등)

이 문서에서는 특정 도메인(전투, 스킬, 버프 등)에 속하지 않고 프로젝트 전반에 걸쳐 사용되는 공유 모듈, 유틸리티, 관리자 클래스, 에디터 확장 스크립트 등을 분석하고 기록합니다.

---

## 7.1. Shared/Scaling: 범용 스케일링 시스템

`Shared/Scaling` 폴더에는 프로젝트의 거의 모든 동적 수치(스킬 데미지, 버프 지속시간, 캐릭터 성장률 등)를 일관되고 확장 가능한 방식으로 처리하기 위한 강력한 범용 스케일링 시스템이 구현되어 있습니다. 이 시스템의 핵심은 **전략(Strategy) 패턴**과 **팩토리(Factory) 패턴**입니다.

### `IScalingStrategy.cs` (스케일링 전략 인터페이스)
- **역할**: 모든 스케일링 계산 방식(예: 선형, 지수, 로그, 구간별)이 따라야 하는 **공통 계약(Interface)** 입니다.
- **핵심 메서드**:
    - `Calculate(level, maxLevel, config)`: 특정 레벨에서의 스케일링 값을 계산하는 핵심 로직입니다. 현재 레벨, 최대 레벨, 그리고 추가 설정(`ScalingConfig`)까지 인자로 받아 매우 유연한 계산이 가능합니다.
    - `ValidateConfig(config)`: Unity 에디터에서 기획자가 입력한 설정 값이 해당 전략에 유효한지 검증하는 기능입니다. (예: 로그 스케일링에서 밑(base)이 1 이하인 경우를 방지)
    - `GetDefaultConfig()`: 특정 스케일링 전략에 필요한 기본 설정 값을 제공하여, 사용자가 에디터에서 전략을 선택했을 때 올바른 기본값으로 시작할 수 있도록 돕습니다.

<br>

### `IndividualScaling.cs` (개별 스케일링 설정 구조체)
- **역할**: `[System.Serializable]` 속성을 가진 `struct`(구조체)로, `SkillData`, `BuffBlueprint` 등 다른 `ScriptableObject`에 **포함(embed)**되어 사용됩니다. 기획자가 Unity 에디터에서 특정 값(예: 스킬 데미지, 버프 지속시간)의 성장 방식을 직접 설정할 수 있는 **UI와 로직을 제공**하는 래퍼(Wrapper) 클래스입니다.
- **동작 방식**:
    1.  기획자가 에디터에서 `scalingType` 드롭다운 메뉴를 통해 '선형 증가', '지수 증가' 등의 스케일링 방식을 선택합니다.
    2.  선택된 `scalingType`에 따라 `IndividualScaling`의 커스텀 에디터(존재할 것으로 예상)가 `config` 필드에 필요한 입력 UI(예: 레벨당 증가 값, 지수 등)를 표시합니다.
    3.  게임 로직에서는 이 구조체의 `CalculateScaling(level, maxLevel)` 메서드를 호출하기만 하면 됩니다.
    4.  이 메서드는 내부적으로 `ScalingStrategyFactory`에 자신의 `scalingType`과 `config`를 전달하여, 실제 계산을 선택된 전략 클래스에게 위임합니다.
- **설계의 우수성**: `IndividualScaling`은 복잡한 스케일링 로직을 **캡슐화**하고, `ScalingStrategyFactory`를 통해 구체적인 전략 구현으로부터 **자신을 분리**합니다. 덕분에 `SkillData`나 `BuffBlueprint` 같은 상위 모듈은 스케일링이 내부적으로 어떻게 동작하는지 전혀 알 필요 없이, 그저 `IndividualScaling` 필드를 하나 가지고 `CalculateScaling`을 호출하면 됩니다. 이는 코드의 재사용성과 유지보수성을 극대화하는 매우 뛰어난 설계입니다.

<br>

### `ScalingStrategies.cs` (구체적인 스케일링 전략들)
- **역할**: `IScalingStrategy` 인터페이스를 구현하는 구체적인 클래스들의 모음입니다. 각 클래스는 고유한 계산 로직을 가집니다.
- **구현된 전략들**:
    - `NoneScalingStrategy`: 스케일링 효과가 없습니다. 항상 0을 반환합니다.
    - `LinearScalingStrategy`: 레벨당 고정된 값을 더하는 가장 기본적인 선형 증가 방식입니다.
    - `ExponentialScalingStrategy`: 레벨이 오를수록 증가량이 기하급수적으로 늘어나는 지수 증가 방식입니다.
    - `LogarithmicScalingStrategy`: 레벨이 오를수록 증가량이 점차 둔화되는 로그 증가 방식입니다.
    - `StepScalingStrategy`: 특정 레벨 구간(예: 3, 6, 9레벨)에 도달할 때마다 정해진 값을 더해주는 계단식 증가 방식입니다.
    - `CustomScalingStrategy`: 기획자가 레벨별 성장 배율을 배열로 직접 정의하는 가장 유연한 방식입니다.
- **SRP 준수**: 각 클래스는 오직 자신의 계산법만 책임지며, 다른 전략에 대해 전혀 알지 못합니다. 이는 단일 책임 원칙(SRP)을 완벽하게 준수하는 구조입니다.

<br>

### `ScalingStrategyFactory.cs` (스케일링 전략 팩토리)
- **역할**: **팩토리 패턴**을 사용하는 `static` 클래스로, `ScalingType` 열거형 값에 따라 적절한 `IScalingStrategy` 구현체(전략)를 생성하고 제공하는 중앙 관리자입니다.
- **핵심 기능**:
    1.  **전략 생성 및 캐싱**: `GetStrategy(type)` 메서드는 요청된 타입의 전략 객체가 캐시에 있는지 먼저 확인합니다. 없으면 `CreateStrategy` 내부의 `switch` 문을 통해 객체를 생성하고, 다음 요청을 위해 캐시에 저장합니다. 이는 동일한 전략 객체가 불필요하게 여러 번 생성되는 것을 방지하는 **성능 최적화(메모이제이션)** 기법입니다.
    2.  **퍼사드(Facade) 역할**: `Calculate`, `ValidateConfig`, `GetDefaultConfig`와 같은 정적 메서드를 제공합니다. 외부(예: `IndividualScaling`)에서는 이 팩토리의 정적 메서드만 호출하면, 팩토리가 알아서 적절한 전략 객체를 찾아 해당 로직을 대신 실행해줍니다. 이는 외부 클래스가 개별 전략 클래스들을 직접 알 필요가 없게 만들어 결합도를 낮춥니다.
    3.  **에디터 지원**: `GetSupportedTypes()`나 `GetDescription(type)` 같은 메서드는 커스텀 에디터 스크립트가 기획자에게 친절한 UI(드롭다운 메뉴, 설명 툴팁 등)를 제공할 수 있도록 지원하는 유틸리티 함수입니다.

---

## 7.2. Utility: 범용 유틸리티 및 로거

`Utility` 폴더에는 특정 도메인에 종속되지 않고 프로젝트 전반의 코드 안정성과 개발 편의성을 높여주는 범용 헬퍼 클래스들이 포함되어 있습니다.

### `DictionaryExtensions.cs` (딕셔너리 확장 메서드)
- **역할**: `System.Collections.Generic.Dictionary` 클래스에 새로운 편의 메서드를 추가하는 `static` 클래스입니다. C#의 확장 메서드 기능을 활용합니다.
- **주요 기능**:
    - `GetValueOrDefault(key, defaultValue)`: `TryGetValue`의 반복적인 사용 패턴을 줄여줍니다. 딕셔너리에서 키를 찾아 값을 반환하고, 키가 없으면 지정된 기본값(또는 타입의 기본값)을 반환하여 `KeyNotFoundException`을 원천적으로 방지하고 코드의 안정성을 높입니다.
    - `AddValue(key, value)`: 키의 존재 여부를 미리 확인할 필요 없이 값을 안전하게 더해주는 매우 유용한 메서드입니다. 키가 존재하면 기존 값에 새 값을 더하고, 키가 없으면 새 값을 키와 함께 새로 추가합니다. 이는 캐릭터 스탯, 재화, 아이템 개수 등을 관리하는 로직을 매우 간결하고 안전하게 만들어줍니다.

<br>

### `GameLogger.cs` (게임 로깅 유틸리티)
- **역할**: Unity의 기본 `Debug.Log`를 목적과 환경에 맞게 체계적으로 관리하기 위해 래핑(wrapping)한 `static` 클래스입니다.
- **주요 기능**:
    1.  **빌드 환경 분리**: `#if UNITY_EDITOR || DEVELOPMENT_BUILD` 전처리기 지시문을 사용하여, 특정 로그(예: `LogDev`, `LogSkill`)가 개발 빌드나 유니티 에디터에서만 출력되고, **최종 릴리즈 빌드에서는 컴파일조찰 되지 않도록** 합니다. 이는 불필요한 로그 호출로 인한 릴리즈 빌드의 성능 저하를 막는 핵심적인 최적화 기법입니다.
    2.  **로그 수준 및 컨텍스트 구분**: `LogInfo`, `LogWarning`, `LogError` 등 로그의 중요도에 따라 메서드를 분리하고, `[DEV]`, `[SKILL]`, `[BATTLE]` 과 같은 명확한 접두사를 붙여 로그의 출처와 맥락을 명확하게 구분합니다. 이를 통해 개발자는 Unity 콘솔에서 수많은 로그 중에 원하는 정보를 훨씬 쉽게 필터링하고 찾아낼 수 있어 디버깅 효율을 크게 향상시킵니다.

---

## 7.3. Localization: 코어와 래퍼가 분리된 지역화 시스템

[[memory:2801344]] 사용자의 "모든 하드코딩된 문자열을 `LocalizationManager`로 대체" 요구사항에 따라, 이 시스템은 프로젝트의 유지보수성과 확장성에 매우 중요한 역할을 합니다. 분석 결과, 이 시스템은 SOLID 원칙을 매우 잘 따르는 **코어(Core)와 래퍼(Wrapper)가 분리된 정교한 아키텍처**를 가지고 있습니다.

### `LocalizationManager.cs` (퍼사드 & Unity 래퍼)
- **역할**: **싱글턴** `MonoBehaviour` 클래스로서, 로컬라이제이션 시스템의 유일한 **외부 진입점(Facade)** 역할을 합니다. 게임의 다른 모든 부분은 오직 `LocalizationManager.Instance`를 통해서만 텍스트를 요청합니다.
- **책임**:
    - **Unity 생명주기 관리**: `Awake()`에서 `DontDestroyOnLoad`를 통해 씬이 바뀌어도 파괴되지 않도록 설정하고, 시스템을 초기화합니다.
    - **인스펙터 연동**: `LanguageSettings`, `StringTable` 등 필요한 애셋들을 Unity 에디터의 인스펙터 창을 통해 편리하게 할당받습니다.
    - **요청 전달**: `GetText`, `ChangeLanguage` 등의 모든 public API는 실제 로직을 수행하지 않고, 내부의 `LocalizationCore` 인스턴스로 요청을 그대로 전달하는 **'얇은 래퍼(thin wrapper)'** 역할을 합니다.
    - **Unity 이벤트 발행**: `LocalizationCore`에서 발생한 언어 변경 이벤트를 받아, Unity의 `static event`인 `OnLanguageChanged`로 다시 발행하여 UI 등 다른 Unity 컴포넌트들이 언어 변경에 반응할 수 있도록 합니다.

<br>

### `LocalizationCore.cs` (순수 C# 코어 엔진)
- **역할**: **Unity에 대한 의존성이 전혀 없는** 순수 C# 클래스로, 로컬라이제이션의 모든 핵심 로직을 담당하는 시스템의 **'두뇌'** 입니다.
- **책임**:
    - **데이터 관리**: 여러 `StringTable`을 리스트로 관리하며, `AddStringTables`를 통해 동적으로 테이블을 추가할 수 있습니다.
    - **텍스트 검색**: `GetText(key)` 요청이 오면, 등록된 모든 `StringTable`을 순회하며 키에 해당하는 텍스트를 현재 설정된 언어(`_currentLanguage`)로 찾아 반환합니다.
    - **캐싱**: `_enableCaching`이 활성화되면, 각 `StringTable`에 대한 `StringTableCache` 객체를 생성하여 검색 성능을 최적화합니다. `GetText` 호출 시 캐시를 먼저 확인하여 반복적인 검색 비용을 줄입니다.
    - **언어 상태 관리**: `_currentLanguage` 상태를 유지하고, `ChangeLanguage` 메서드를 통해 언어를 변경하며, 변경 시 `OnLanguageChanged` 이벤트를 발생시켜 `LocalizationManager`에게 알립니다.
    - **오류 처리**: `_missingKeys` 집합을 이용해 찾지 못한 키를 기록하고, `_logger`를 통해 경고를 남기는 등 안정적인 운영을 위한 로직을 포함합니다.
- **설계의 우수성**: Unity와 완벽하게 분리되어 있기 때문에, 이 `LocalizationCore` 클래스는 이론적으로 서버 코드나 다른 C# 프로젝트에서도 재사용이 가능합니다. 이는 SRP(단일 책임 원칙)와 DIP(의존성 역전 원칙, `ILocalizationLogger` 사용)를 극명하게 보여주는 훌륭한 예시입니다.

<br>

### `StringTable`과 `StringTableCache` (데이터와 성능의 분리)
이 시스템은 데이터의 **저장**과 **성능 최적화**라는 두 가지 책임을 `StringTable`과 `StringTableCache`라는 두 개의 클래스로 완벽하게 분리하여 OCP(개방-폐쇄 원칙)를 보여주는 훌륭한 사례입니다.

-   **`StringTable.cs` (데이터 저장소)**:
    -   **역할**: `ScriptableObject`로서, `LocalizedEntry`의 배열(`entries`) 형태로 로컬라이제이션 데이터를 **저장하는 단일 책임**을 가집니다. Unity 에디터에서 데이터를 쉽게 편집하고 애셋으로 관리할 수 있게 해줍니다.
    -   **동작 방식**: `GetText(key, language)` 메서드는 `entries` 배열을 처음부터 끝까지 순회(`foreach`)하여 일치하는 키를 찾는 **선형 검색(Linear Search)** 방식을 사용합니다. 이 방식은 구현이 간단하지만, 데이터의 양이 많아질수록 성능이 저하되는 단점이 있습니다.

-   **`StringTableCache.cs` (성능 최적화 래퍼)**:
    -   **역할**: `StringTable`의 성능 문제를 해결하기 위해 도입된 **캐시(Cache) 클래스**입니다. `StringTable`을 수정하지 않고, 그 위에 새로운 성능 계층을 추가함으로써 **OCP(개방-폐쇄 원칙)**를 완벽하게 준수합니다.
    -   **동작 방식**:
        1.  **지연 초기화(Lazy Initialization)**: `StringTableCache` 객체가 생성될 때 바로 캐시를 만들지 않고, `GetText`나 `ContainsKey`가 처음 호출될 때 `EnsureInitialized()`를 통해 **단 한 번**만 캐시를 생성합니다.
        2.  **효율적인 자료구조**: `EnsureInitialized()`는 `StringTable`의 `entries` 배열을 단 한 번만 순회하여, 검색에 매우 효율적인 **중첩된 딕셔너리(`Dictionary<string, Dictionary<SupportedLanguage, string>>`)** 자료구조를 만듭니다.
        3.  **고속 검색**: 캐시가 생성된 이후의 `GetText` 호출은 딕셔너리의 O(1) 시간 복잡도 덕분에 `StringTable`을 직접 검색하는 것보다 압도적으로 빠르게 동작합니다.
        4.  **폴백(Fallback) 로직**: 요청한 언어의 텍스트가 없을 경우, 자동으로 기본 언어(한국어)의 텍스트가 있는지 확인하고 반환하는 안정적인 폴백 기능을 제공합니다.

-   **결론**: `LocalizationCore`는 `_enableCaching` 플래그 값에 따라 `StringTableCache`를 사용할지, 아니면 `StringTable`을 직접 검색할지를 결정합니다. 이 구조는 **데이터의 편집 용이성(`StringTable`)**과 **런타임 성능(`StringTableCache`)**이라는 두 가지 상충될 수 있는 요구사항을 모두 만족시키는 매우 영리하고 실용적인 설계입니다.

---

## 7.4. CharacterGrowthService: 통합 성장 관리 서비스

이번 리팩토링을 통해, 기존에 `CharacterGrowthSystem`, `GrowthManager`, 그리고 `ResourceManager`의 일부 기능에 분산되어 있던 캐릭터 성장 관련 로직이 `CharacterGrowthService`라는 단일 서비스로 통합되었습니다. 이는 **단일 책임 원칙(SRP)**을 강화하고 시스템의 응집도를 높이는 중요한 개선입니다.

### `CharacterGrowthService.cs` (성장 로직 중앙 허브)
- **역할**: `ScriptableObject` 기반의 **싱글턴(Singleton)** 서비스로, 캐릭터(카드)의 경험치 획득, 레벨업, 스탯 성장에 대한 모든 계산을 전담합니다.
- **핵심 기능**:
    - **경험치 및 레벨 관리**: `AddExperience`를 통해 경험치를 추가하고 레벨업 가능 여부를 반환하며, `AttemptLevelUp`을 통해 실제 레벨업을 처리합니다.
    - **성장 계산**: 레벨업 시 `ApplyGrowth` 메서드를 호출하여, `GrowthConfig`에 정의된 데이터를 기반으로 등급별 성장치와 개별 성장률을 조합하여 최종 스탯을 계산하고 `UserCardData`에 반영합니다.
    - **비용 조회**: `GetCostForLevelUp` API를 통해 레벨업에 필요한 재화(골드, 젬) 정보를 제공합니다.

### `GrowthConfig.cs` (성장 데이터 중앙 저장소)
- **역할**: `ScriptableObject`로서, 캐릭터 성장에 필요한 모든 **설정 데이터**를 한 곳에서 관리합니다.
- **데이터 내용**:
    - 레벨별 요구 경험치 (`ExperiencePerLevel`)
    - 레벨업 시 필요한 재화 비용 (`GoldCostPerLevel`, `GemCostPerLevel`)
    - `GradeGrowthConfig` 애셋 참조를 통한 등급별 기본 성장률 데이터

### 설계의 장점
- **중앙 집중화**: 성장과 관련된 모든 로직과 데이터가 각각 `CharacterGrowthService`와 `GrowthConfig`에 집중되어 있어, 향후 성장 공식 변경이나 밸런스 조정 시 수정해야 할 파일이 명확해졌습니다.
- **의존성 감소**: `UserDataManager`는 이제 복잡한 성장 계산 로직을 알 필요 없이, `CharacterGrowthService`에 요청을 위임하기만 하면 됩니다. 이는 시스템 간의 결합도를 낮추고 각 클래스가 자신의 핵심 책임에만 집중할 수 있게 합니다.

---

## 7.5. Managers: 게임 흐름 및 테스트 관리

`Managers` 폴더에는 게임의 전반적인 흐름을 제어하거나, 특정 시스템들을 총괄하는 관리자 클래스들이 위치합니다.

### `GameManager.cs` (게임 관리자 및 테스트 런처)
- **역할**: 현재 버전에서는 게임의 전체적인 상태 관리보다는 **테스트 전투의 진입점** 역할을 주로 수행하는 `Singleton` `MonoBehaviour` 클래스입니다.
- **핵심 기능**:
    - **테스트 데이터 설정**: `[SerializeField]`로 노출된 `playerTeam`과 `enemyTeam` 리스트를 통해, Unity 에디터 인스펙터 창에서 테스트 전투에 참여할 아군과 적군의 `CardBlueprintData`와 레벨, 심지어 개체별 고유 성장률까지 직접 설정할 수 있습니다.
    - **테스트 전투 시작**: `StartTestBattle()` 메서드는 인스펙터에 설정된 이 테스트 데이터를 기반으로 `UserCardData`와 `ParticipantData` 객체들을 동적으로 생성하고, 이를 `DungeonData`로 묶어 `BattleManager.StartBattle(dungeonData)`를 호출함으로써 전투를 시작시킵니다.
- **`TestParticipant` 보조 클래스**:
    - `GameManager` 내부에 정의된 `[System.Serializable]` 클래스로, 인스펙터에서 테스트 전투 참가자의 세부 정보를 편리하게 설정할 수 있는 UI를 제공합니다.
    - `ToUserCardData()` 메서드는 이 설정 값을 바탕으로 실제 전투에 필요한 `UserCardData` 객체를 동적으로 생성하는 **소규모 팩토리** 역할을 수행합니다.
- **향후 확장성**:
    - 현재 `GameManager`의 기능은 매우 제한적이지만, 싱글턴 구조와 `DontDestroyOnLoad` 설정은 이 클래스가 향후 게임의 **상태 머신(State Machine)**(예: `MainMenu`, `InGame`, `Paused`, `GameOver` 상태 관리)으로 확장되거나, 여러 다른 매니저들을 총괄하는 **중앙 허브**로 발전할 수 있는 기반을 마련해 둔 것으로 볼 수 있습니다. 

---

## 7.6. Editor: 개발 생산성 향상 도구

`Editor` 폴더에는 Unity 에디터의 기능을 확장하여, 데이터 관리의 편의성과 정확성을 높이고 복잡한 데이터를 직관적으로 편집할 수 있도록 돕는 스크립트들이 위치합니다.

### `StringTableCsvImporter.cs` (CSV 데이터 임포터)
- **역할**: **`[MenuItem]`** 속성을 사용하여 Unity 에디터 상단 메뉴에 "Tools/Localization/Import Keys from CSV"라는 새로운 메뉴를 추가합니다. 이 메뉴를 클릭하면 지정된 경로의 CSV 파일을 읽어 `StringTable`에 자동으로 데이터를 추가/갱신하는 **데이터 자동화 파이프라인**을 제공합니다.
- **핵심 기능**:
    - **자동화**: 기획자가 구글 시트나 Excel 같은 외부 툴에서 로컬라이제이션 텍스트를 작성하고 CSV로 내보내기만 하면, 개발자는 메뉴 클릭 한 번으로 모든 내용을 게임 애셋에 안전하게 반영할 수 있습니다. 수작업으로 인한 실수를 원천적으로 방지하고 작업 시간을 극적으로 단축시킵니다.
    - **안정성**: 임포트 과정에서 기존에 존재하는 키는 건너뛰고, 새로운 키만 추가하여 데이터 손실을 방지합니다. 또한 `Undo.RecordObject`를 사용하여 임포트 작업을 취소할 수 있도록 하여 안정성을 높입니다.
    - **성능 최적화**: `StringTable`에 이미 존재하는 수만 개의 키를 매번 순회하며 비교하는 것을 피하기 위해, 처음에 한 번만 모든 키를 **`HashSet`**에 넣어두고, 이후에는 O(1) 시간 복잡도로 키의 존재 여부를 빠르게 확인하여 대용량 데이터 처리 시에도 빠른 속도를 보장합니다.

<br>

### `UniqueEffectSOEditor.cs` (동적 커스텀 인스펙터)
- **역할**: **`[CustomEditor]`**와 **`[CustomPropertyDrawer]`** 속성을 사용하여 `UniqueEffectSO`와 그 내부의 `TriggeredRule` 클래스의 Unity 인스펙터 UI를 기본 UI보다 훨씬 더 강력하고 직관적인 인터페이스로 완전히 대체합니다.
- **핵심 기능**:
    - **`ReorderableList`**: Unity 에디터의 기본 배열/리스트 UI는 항목을 추가, 삭제, 재정렬하기 매우 불편합니다. 이 스크립트는 `UnityEditorInternal.ReorderableList`를 사용하여 이 단점을 완벽하게 보완합니다. 드래그 앤 드롭으로 순서를 바꾸고, '+'/'–' 버튼으로 쉽게 항목을 추가/삭제할 수 있는 훨씬 직관적인 UI를 제공합니다.
    - **인스펙터에서의 다형성(Polymorphism) 구현**: `onAddDropdownCallback` 부분은 이 커스텀 에디터의 가장 강력한 기능입니다. '+' 버튼을 누르면 단순한 빈 슬롯이 추가되는 것이 아니라, `EffectCondition`이나 `EffectAction`을 **상속하는 모든 자식 클래스들의 목록이 드롭다운 메뉴로** 나타납니다. 사용자가 메뉴에서 특정 조건(예: `IsHealthBelowCondition`)을 선택하면, 해당 타입의 객체가 `[SerializeReference]`를 통해 생성되고 리스트에 추가됩니다. 이는 복잡한 규칙 기반 시스템을 코딩 없이 에디터에서 시각적으로 조립할 수 있게 해주는 매우 강력하고 유연한 기능입니다. 