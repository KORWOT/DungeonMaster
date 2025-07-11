# 4. `Data` 시스템 분석

이 문서에서는 `Data` 폴더 내의 스크립트들을 분석하고, 게임의 핵심 데이터 구조(캐릭터, 전투 상태, 카드 등)와 데이터의 흐름, 그리고 관리 방식을 설명합니다.

---

## 4.1. 결정론적 전투 데이터 코어

`Battle` 시스템의 심장부에는 특정 시점의 전투 상황 전체를 완벽하게 저장하는 순수 데이터 클래스들이 있습니다. 이 클래스들은 `UnityEngine`에 대한 종속성이 전혀 없어, Unity 엔진 외부에서도 전투 로직의 테스트와 시뮬레이션이 가능하게 만드는 핵심 요소입니다.

### `BattleState.cs` (전투 스냅샷)
- **역할**: 특정 한순간의 전투 상황 전체를 나타내는 **순수 데이터 컨테이너**입니다. 이 객체 안에는 전투에 참여 중인 모든 캐릭터의 목록(`Characters`), 현재 전투의 상태(`Status`), 전투 경과 시간(`CurrentTimeMs`), 그리고 해당 틱에 발생한 이벤트 목록(`Events`)이 포함됩니다.
- **불변성 (Immutability)**: 이 클래스의 가장 중요한 설계 원칙입니다. 전투 상태를 변경해야 할 때, 기존 객체의 값을 직접 수정하는 대신 `Clone()`이나 `With()` 메서드를 사용하여 필요한 부분만 변경된 **완전히 새로운 `BattleState` 객체를 생성**하여 반환합니다.
- **예측 가능성**: 이 불변성 원칙 덕분에, `DeterministicBattleRules`의 `ProcessTick` 함수는 입력된 `BattleState`에 대해서 항상 동일한 `BattleState` 결과를 출력하는 '순수 함수'처럼 동작할 수 있습니다. 이는 복잡한 전투 로직에서 발생할 수 있는 예기치 않은 부수 효과(Side Effect)를 원천적으로 차단하고 시스템의 안정성과 예측 가능성을 극대화합니다.

<br>

### `DeterministicCharacterData.cs` (전투 캐릭터 데이터)
- **역할**: `BattleState`를 구성하는 핵심 요소로, 전투에 참여하는 개별 캐릭터의 모든 데이터를 담는 순수 C# 클래스입니다.
- **데이터의 명확한 분리**:
    - **고유 식별자**: `InstanceId`(이번 전투에서만 유효한 고유 ID), `BlueprintId`(캐릭터의 원본 데이터 ID)
    - **불변 데이터 (Readonly)**: `Name`, `IsPlayerCharacter`, `Skills` 등 전투가 시작된 후에는 변하지 않아야 하는 원본(Blueprint) 데이터로부터 복사된 값들입니다.
    - **가변 데이터 (Mutable)**: `CurrentHP`, `Stats`(모든 스탯 딕셔너리), `SkillCooldowns`, `ActiveBuffs` 등 전투 중에 실시간으로 변하는 모든 상태 값입니다.
- **깊은 복사 (Deep Copy)**:
    - `BattleState`가 복제될 때, 그 안의 `DeterministicCharacterData` 리스트도 함께 복제됩니다.
    - 이때 `Stats`, `SkillCooldowns` 같은 컬렉션과 `ActiveBuffs` 리스트는 단순히 참조 주소만 복사하는 '얕은 복사'가 아닙니다. 내부 요소까지 모두 새로 생성하는 **'깊은 복사'**가 이루어집니다.
    - 이는 복제된 상태 B에서 캐릭터의 HP를 변경해도, 원본 상태 A에 있는 동일 캐릭터의 HP는 전혀 영향을 받지 않도록 보장하는, 불변성 유지를 위한 필수적인 처리입니다.

---

## 4.2. 청사진(Blueprint)과 인스턴스(Instance)

게임의 핵심 콘텐츠인 캐릭터(카드) 데이터는 **청사진(Blueprint)** 과 **인스턴스(Instance)** 라는 두 가지 클래스로 명확하게 분리되어 관리됩니다. 이 설계는 변하지 않는 원본 데이터와, 플레이어의 활동에 따라 계속해서 변하는 상태 데이터를 분리하여 시스템 전체의 데이터 관리를 용이하게 하고 확장성을 높입니다.

### `CardBlueprintData.cs` (카드의 청사진)
- **역할**: `ScriptableObject`를 사용하여 카드의 **불변(Immutable) 원본 데이터**를 정의합니다.
- **데이터 내용**:
    - `BlueprintId` (고유 ID), 이름, 등급, 속성
    - 1레벨 기준의 기본 스탯 (`BaseStats`)
    - 획득 가능한 스킬 ID 목록 (`SkillIds`)
    - 레벨업 시 어떤 스탯이 성장하는지에 대한 정보 (`GrowableStatTypes`)
- **데이터 소스**: 이 `ScriptableObject` 에셋은 게임 디자이너가 Unity 에디터에서 직접 값을 설정하는 핵심 데이터 소스 역할을 합니다. `BlueprintDatabase`는 게임 시작 시 `Resources` 폴더에 있는 이 모든 청사진 에셋을 로드하여 중앙에서 관리합니다.

<br>

### `UserCardData.cs` (개별 카드 인스턴스)
- **역할**: 플레이어가 실제로 소유하고 성장시키는 **개별 카드(인스턴스)의 가변(Mutable) 상태**를 저장하는 순수 C# 클래스입니다. `[Serializable]` 어트리뷰트를 통해 이 객체는 JSON 등으로 쉽게 변환되어 서버나 로컬 파일에 저장될 수 있습니다.
- **주요 상태 값**:
    - `Id`: 플레이어가 소유한 수많은 카드들 사이에서 이 카드를 유일하게 식별하는 **인스턴스 ID**입니다.
    - `BlueprintId`: 이 카드가 어떤 종류의 카드인지 알려주는 **'원본 청사진'의 ID**입니다.
    - `Level`, `Experience`: 카드의 현재 레벨과 경험치입니다.
    - `CurrentStats`: 레벨업, 장비 장착 등 모든 성장 요소가 **최종적으로 반영된 스탯** 딕셔너리입니다. 전투에 참여하는 `DeterministicCharacterData`는 바로 이 값을 그대로 복사해갑니다.
    - `InnateGrowthRates_x100`: 이 카드가 생성될 때 단 한 번 랜덤하게 결정되는 **'고유 성장률'**입니다. 이 값을 통해 같은 종류의 '고블린' 카드라도 성장 잠재력이 다른 개체를 만들 수 있습니다. (예: 공격력 성장률 120% 고블린, 95% 고블린)

<br>

### 데이터 흐름: Blueprint → Instance
1.  플레이어가 새로운 카드를 획득하면, `CardBlueprintData` (청사진)를 기반으로 새로운 `UserCardData` (인스턴스)가 생성됩니다.
2.  `CharacterGrowthSystem`은 이 `UserCardData`의 레벨과 `InnateGrowthRates_x100` 값, 그리고 원본인 `CardBlueprintData`의 기본 스탯을 조합하여 레벨업 시 성장할 스탯을 계산합니다.
3.  계산된 최종 스탯은 `UserCardData.CurrentStats`에 덮어씌워집니다.
4.  전투가 시작되면 `CharacterDataFactory`가 이 `UserCardData`를 참조하여 전투에 사용될 `DeterministicCharacterData`를 생성합니다.

---

## 4.3. 데이터베이스와 팩토리

데이터를 효율적으로 관리하고 변환하기 위해 데이터베이스와 팩토리 패턴이 사용됩니다.

### `BlueprintDatabase.cs` (청사진 데이터베이스)
- **역할**: `SkillManager`와 거의 동일한 역할을 하는, `CardBlueprintData` 에셋에 대한 **중앙 관리자**입니다. `ScriptableObject`이면서 **싱글턴(Singleton)**으로 구현되어, 게임 내 모든 카드 청사진에 대한 일관된 접근점을 제공합니다.
- **데이터 캐싱**: 게임 시작 시 `Resources` 폴더에서 모든 `CardBlueprintData` 에셋을 로드합니다. 이후 `BlueprintId`를 키로 하는 딕셔너리와, `Grade`(등급)를 키로 하는 딕셔너리를 각각 만들어 메모리에 저장합니다.
- **빠른 조회**: 미리 만들어 둔 딕셔너리 덕분에 "ID로 찾기"(`GetBlueprint(id)`), "등급으로 찾기"(`GetBlueprintsByGrade(grade)`) 등 다양한 조건의 검색을 배열 순회 없이 매우 빠른 속도로 수행할 수 있습니다.

<br>

### `CharacterDataFactory.cs` (전투 데이터 팩토리)
- **역할**: 영구적인 성장 데이터(`UserCardData`)를 일시적인 전투용 데이터(`DeterministicCharacterData`)로 변환하는 **팩토리(Factory)** 클래스입니다. 전투 시작 직전에 호출되어, 전투에 필요한 모든 정보를 종합하여 하나의 객체로 조립하는 중요한 다리 역할을 합니다.
- **데이터 변환 및 조립 과정**: `Create` 메서드는 다음 순서로 데이터를 조립합니다.
    1.  **스탯 복사**: `UserCardData`에 이미 모든 성장이 반영된 `CurrentStats`를 `DeterministicCharacterData`의 `Stats`로 그대로 복사합니다.
    2.  **스킬 정보 추가**: `Blueprint`에 정의된 스킬 ID 목록을 이용해 `SkillManager`에서 실제 `SkillData` 객체들을 가져와 `Skills` 리스트에 채워 넣습니다.
    3.  **장비 효과 적용**: `UserCardData`의 `EquippedItemIds`를 통해 `EquipmentManager`에서 장비 정보를 조회합니다.
    4.  장비가 가진 스탯 보너스를 `Stats` 딕셔너리에 **추가로 합산**합니다.
    5.  만약 장비가 `IDamageModifier` 인터페이스를 구현했다면(예: "화염 속성 적에게 10% 추가 데미지" 효과), 해당 장비 객체 자체를 `DamageModifiers` 리스트에 추가하여 전투 중 특별한 효과가 발동될 수 있도록 합니다.
    6.  **최종 마무리**: 모든 스탯 계산이 끝나면 `FinalizeStatCalculation`을 호출하여 `MaxHP` 값을 `CurrentHP`에 설정하는 등 최종 마무리를 합니다.

---

## 4.4. 데이터 관리와 저장 - 퍼사드 패턴

플레이어의 모든 영구적인 데이터(재화, 보유 카드, 레벨 등)는 **퍼사드 패턴(Facade Pattern)** 과 **단일 책임 원칙(Single Responsibility Principle)** 에 따라 체계적으로 관리되고 저장됩니다.

### `UserDataManager.cs` (유저 데이터 퍼사드)
- **역할**: 게임의 다른 모든 시스템(UI, 상점, 전투 준비 등)이 플레이어 데이터에 접근하기 위한 **단일 창구(Facade)** 역할을 하는 최상위 정적 클래스입니다.
- **데이터 총괄**: `UserData`라는 내부 클래스를 통해 플레이어의 모든 영구 데이터(재화, 보유 카드 목록, 레벨, 경험치 등)를 하나의 객체로 묶어 관리합니다.
- **책임 위임**: 이 클래스는 데이터를 '어떻게' 디스크에 저장하고 로드하는지에 대해서는 전혀 알지 못합니다. `SaveUserData()`나 `LoadUserData()`가 호출되면, 그저 `SaveDataManager`에 실제 작업을 그대로 전달할 뿐입니다.
- **편의 API 제공**: `AddGold`, `AddExperience`, `AcquireCard` 등 외부 시스템이 자주 사용하는 기능들을 간단한 메서드 호출로 사용할 수 있도록 제공합니다. 이 메서드들은 내부적으로 `CurrentUserData`의 값을 변경한 뒤, 거의 항상 자동으로 `SaveUserData()`를 호출하여 데이터의 정합성을 유지합니다.

<br>

### `SaveDataManager.cs` (저장/로드 전담)
- **역할**: 파일 입출력(I/O)과 관련된 모든 로우 레벨(low-level) 작업을 전담하는 정적 클래스입니다.
- **단일 책임 원칙 (SRP)**: 이 클래스의 책임은 오직 "주어진 `UserData` 객체를 디스크에 쓰거나, 디스크에서 읽어와 `UserData` 객체로 만드는 것"에 한정됩니다. 데이터의 내용이나 구조가 어떻게 생겼는지는 전혀 신경 쓰지 않습니다.
- **데이터 직렬화 (Serialization)**:
    - Unity의 내장 기능인 `JsonUtility.ToJson`을 사용하여 `UserData` 객체를 **JSON** 형식의 문자열로 변환(직렬화)합니다.
    - `JsonUtility.FromJson`을 사용하여 JSON 문자열을 다시 `UserData` 객체로 변환(역직렬화)합니다.
- **안전한 파일 경로**: `Application.persistentDataPath`를 사용하여 각 운영체제(Windows, Android, iOS 등)에 맞는 안전한 영구 데이터 저장 경로에 `userdata.json` 파일을 생성하고 관리합니다.

<br>

### 설계의 장점
이러한 설계는 향후 저장 방식을 JSON에서 암호화된 바이너리 파일이나, SQLite 데이터베이스, 혹은 Firebase 같은 클라우드 서버로 변경해야 할 때, 오직 `SaveDataManager`의 내부 로직만 수정하면 될 뿐 `UserDataManager`를 포함한 다른 어떤 코드에도 영향을 주지 않게 만드는 매우 유연하고 유지보수하기 좋은 구조입니다. 