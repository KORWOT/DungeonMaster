# 5. `Equipment` 시스템 분석

이 문서에서는 `Equipment` 폴더 내의 스크립트들을 분석하고, 장비의 데이터 구조, 효과 적용 방식, 그리고 시스템 전반의 설계 원칙을 설명합니다.

---

## 5.1. 장비 데이터 구조의 핵심

장비 시스템은 `Card`나 `Skill` 시스템과 유사하게 `ScriptableObject`를 중심으로 구축되었지만, 데이터 관리 방식에 약간의 차이가 있습니다.

### `IMonsterEquipment.cs` (장비 인터페이스)
- **역할**: 모든 장비(무기, 방어구, 장신구)가 가져야 할 최소한의 공통 속성(이름, 등급, 레벨 등)과 기능(강화 가능 여부 확인 등)을 정의하는 기본 계약입니다.
- **타입 정의**: `MonsterEquipmentType` (Weapon, Armor, Accessory)과 `EquipmentGrade` (Normal, Magic, Epic 등) 열거형을 통해 장비의 종류와 희귀도를 명확하게 구분합니다.

<br>

### `BaseMonsterEquipment.cs` (장비 데이터의 청사진이자 인스턴스)
- **역할**: `ScriptableObject`를 상속받는 **추상 클래스**로, 모든 장비 데이터의 실질적인 기반이 됩니다. `WeaponData`, `ArmorData` 등 구체적인 장비 타입은 모두 이 클래스를 상속받습니다.
- **혼합된 데이터 모델**:
    - `Card` 시스템이 '청사진'(`CardBlueprintData`)과 '인스턴스'(`UserCardData`)를 명확히 분리한 것과 달리, `BaseMonsterEquipment`는 이 두 가지 역할을 모두 수행하는 혼합된 구조를 가집니다.
    - `equipmentId`, `equipmentName`과 같은 불변의 '청사진' 데이터와, `level`, `grade`와 같이 플레이어의 성장에 따라 변하는 '인스턴스' 데이터를 모두 가지고 있습니다.
    - 이는 개별 장비마다 별도의 `UserEquipmentData` 클래스를 두지 않고, `ScriptableObject` 에셋을 `Instantiate`하여 복제하고, 이 복제본의 상태를 직접 변경하여 개별 인스턴스로 사용하는 방식을 암시합니다.
- **효과 시스템**:
    - **`baseEffects`**: 장비 종류에 따라 고정적으로 붙는 기본 효과입니다. (예: 무기는 항상 '공격력' 증가)
    - **`additionalEffects`**: 등급(`Grade`)이 올라갈수록 무작위로 추가되는 부가 효과 목록입니다.
    - **`uniqueEffectSO`**: 에픽(`Epic`) 등급 이상에서만 부여될 수 있는, `ScriptableObject`로 별도 정의된 매우 특별한 '고유 효과'입니다.
- **스탯 보너스 계산**: `GetAllStatBonuses(level)` 메서드는 장비의 현재 레벨을 기반으로 `baseEffects`와 `additionalEffects`에 정의된 모든 스탯 보너스를 합산하여, `CharacterDataFactory`가 사용할 수 있는 최종 스탯 보너스 딕셔너리(`Dictionary<StatType, long>`)를 반환합니다.
- **`IDamageModifier` 인터페이스 구현**:
    - 이 클래스는 `CharacterDataFactory` 분석에서 보았던 `IDamageModifier` 인터페이스를 직접 구현합니다.
    - 만약 이 장비에 `uniqueEffectSO`가 할당되어 있다면, 전투 중 데미지 계산 파이프라인의 `ModifierStep`에서 이 장비의 `ModifyDamage` 메서드가 호출됩니다.
    - 이를 통해 장비는 단순히 스탯을 올려주는 것을 넘어, "특정 조건에서 최종 데미지 10% 증가"와 같은 복잡하고 특별한 효과를 발휘할 수 있습니다. 이는 장비 시스템의 깊이를 더하는 핵심적인 메커니즘입니다.

---

## 5.2. 장비 관리 시스템

장비 데이터는 '원본 데이터베이스'와 '개별 캐릭터의 장비 슬롯'이라는 두 가지 종류의 매니저를 통해 체계적으로 관리됩니다.

### `GlobalMonsterEquipmentManager.cs` (장비 청사진 데이터베이스)
- **역할**: 이 클래스는 `BlueprintDatabase`와 정확히 동일한 역할을 합니다. `ScriptableObject`로서, 게임에 존재하는 모든 장비의 원본(청사진) 에셋을 `weaponPool`, `armorPool`, `accessoryPool`이라는 리스트에 담아 관리하는 **중앙 데이터베이스**입니다.
- **데이터 소스**: 기획자는 Unity 에디터에서 이 `ScriptableObject` 에셋에 모든 장비 에셋들을 미리 드래그 앤 드롭하여 등록합니다. 이 클래스는 게임에 어떤 종류의 장비가 존재하는지에 대한 정보 소스 역할을 합니다.

<br>

### `MonsterEquipmentManager.cs` (캐릭터의 장비 슬롯)
- **역할**: 이 클래스는 싱글턴 매니저가 아닌, **개별 캐릭터(`ICharacter`)마다 하나씩 생성되는 '인벤토리' 또는 '장비 슬롯'** 역할을 합니다. 생성될 때 장비의 효과를 적용할 소유자(`owner`)를 명시적으로 주입받습니다.
- **장비 관리**: `weapon`, `armor`, `accessory`라는 3개의 필드를 가지고, `EquipItem`과 `UnequipItem` 메서드를 통해 각 슬롯에 장비를 장착하거나 해제하는 기능을 제공합니다.
- **효과 적용 및 제거**:
    - `EquipItem`이 호출되면, 해당 장비의 `ApplyTo(owner)` 메서드를 호출하여 장비가 가진 모든 스탯 보너스 효과를 소유자 캐릭터의 스탯에 직접 더합니다.
    - `UnequipItem`이 호출되면, 반대로 `RemoveFrom(owner)`를 호출하여 이전에 더했던 스탯 보너스를 다시 빼서 원래 상태로 돌려놓습니다.
- **데이터 흐름**: 이 클래스는 플레이어의 전체 장비 '보관함'이 아닌, 특정 캐릭터가 현재 **'몸에 착용하고 있는'** 장비만을 관리하는 명확한 책임을 가집니다.

<br>

### 전체적인 장비 데이터 흐름
1.  `GlobalMonsterEquipmentManager`는 게임에 존재할 수 있는 모든 장비의 '원본 목록'을 제공합니다.
2.  플레이어가 새로운 장비를 획득하면, 이 원본 에셋 중 하나를 `Instantiate`하여 복제한 '인스턴스'가 생성되고, 이 인스턴스는 `UserData` 어딘가에 있는 플레이어의 전체 장비 보관함에 저장됩니다.
3.  플레이어가 특정 캐릭터에게 이 장비를 장착시키면, 해당 캐릭터에게 귀속된 `MonsterEquipmentManager`가 장비 인스턴스를 참조하여 자신의 슬롯(`weapon`, `armor` 등)에 등록합니다.
4.  동시에 `ApplyTo` 메서드를 통해 장비의 효과가 캐릭터의 최종 스탯(`UserCardData.CurrentStats`)에 반영됩니다.
5.  이 최종 스탯은 전투 시작 시 `CharacterDataFactory`를 통해 전투용 데이터(`DeterministicCharacterData`)로 전달되어 실제 전투에 사용됩니다. 