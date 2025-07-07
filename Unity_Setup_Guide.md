# Unity 던전 마스터 프로젝트 설정 가이드

## 📋 목차
1. [폴더 구조 생성](#1-폴더-구조-생성)
2. [ScriptableObject 에셋 생성](#2-scriptableobject-에셋-생성)
3. [매니저 설정](#3-매니저-설정)
4. [테스트 데이터 생성](#4-테스트-데이터-생성)
5. [시스템 테스트 실행](#5-시스템-테스트-실행)
6. [문제 해결](#6-문제-해결)

---

## 1. 폴더 구조 생성

### 📁 Assets/Resources 폴더 구조
```
Assets/Resources/
├── Managers/
│   ├── SkillManager.asset
│   ├── GlobalMonsterEquipmentManager.asset
│   ├── EnemyManager.asset
│   └── SkillGradeConfig.asset
├── Skills/
│   ├── Fireball_Skill.asset
│   ├── PowerStrike_Skill.asset
│   └── DarkOrb_Skill.asset
├── Equipment/
│   ├── Weapons/
│   │   ├── BasicSword.asset
│   │   └── FireSword.asset
│   ├── Armor/
│   │   ├── BasicArmor.asset
│   │   └── IronArmor.asset
│   └── Accessories/
│       ├── BasicRing.asset
│       └── PowerRing.asset
├── Cards/
│   ├── Monsters/
│   │   ├── Goblin_Card.asset
│   │   └── Orc_Card.asset
│   └── Enemies/
│       ├── Hero_Card.asset
│       └── Knight_Card.asset
└── Settings/
    ├── DamageSettings.asset
    └── AttributeAffinityTable.asset
```

### 🛠️ 폴더 생성 방법
1. Unity 프로젝트 열기
2. Project 창에서 `Assets` 폴더 우클릭
3. `Create > Folder`로 다음 폴더들 생성:
   - `Resources` (없으면 생성)
   - `Resources/Managers`
   - `Resources/Skills`
   - `Resources/Equipment`
   - `Resources/Equipment/Weapons`
   - `Resources/Equipment/Armor`
   - `Resources/Equipment/Accessories`
   - `Resources/Cards`
   - `Resources/Cards/Monsters`
   - `Resources/Cards/Enemies`
   - `Resources/Settings`

---

## 2. ScriptableObject 에셋 생성

### 🎯 2.1 SkillGradeConfig 생성
1. `Assets/Resources/Managers` 폴더에서 우클릭
2. `Create > Game > Skill Grade Config` 선택
3. 이름을 `SkillGradeConfig`로 변경
4. Inspector에서 `Apply Default Settings` 버튼 클릭

### ⚔️ 2.2 SkillManager 생성
1. `Assets/Resources/Managers` 폴더에서 우클릭
2. `Create > Game > Skill Manager` 선택
3. 이름을 `SkillManager`로 변경

### 🛡️ 2.3 GlobalMonsterEquipmentManager 생성
1. `Assets/Resources/Managers` 폴더에서 우클릭
2. `Create > Game > Equipment > Global Equipment Manager` 선택
3. 이름을 `GlobalMonsterEquipmentManager`로 변경

### 👹 2.4 EnemyManager 생성
1. `Assets/Resources/Managers` 폴더에서 우클릭
2. `Create > Game > Enemy Manager` 선택
3. 이름을 `EnemyManager`로 변경

### ⚙️ 2.5 DamageSettings 생성
1. `Assets/Resources/Settings` 폴더에서 우클릭
2. `Create > Game > Battle > Damage Settings` 선택
3. 이름을 `DamageSettings`로 변경

### 🔥 2.6 AttributeAffinityTable 생성
1. `Assets/Resources/Settings` 폴더에서 우클릭
2. `Create > Game > Battle > Attribute Affinity Table` 선택
3. 이름을 `AttributeAffinityTable`로 변경

---

## 3. 매니저 설정

### 🎯 3.1 SkillManager 설정
1. `SkillManager.asset` 선택
2. Inspector에서 `All Skills` 배열 크기를 3으로 설정
3. 각 Element에 스킬 데이터 할당 (아래 4단계에서 생성)

### 🛡️ 3.2 GlobalMonsterEquipmentManager 설정
1. `GlobalMonsterEquipmentManager.asset` 선택
2. Inspector에서 다음 배열들 설정:
   - `Weapon Pool`: 크기 2
   - `Armor Pool`: 크기 2
   - `Accessory Pool`: 크기 2
3. 각 풀에 장비 데이터 할당 (아래 4단계에서 생성)

### 👹 3.3 EnemyManager 설정
1. `EnemyManager.asset` 선택
2. Inspector에서 `Enemy Data List` 배열 크기를 2로 설정
3. 각 Element에 적 데이터 할당 (아래 4단계에서 생성)

### ⚙️ 3.4 DamageSettings 설정
1. `DamageSettings.asset` 선택
2. Inspector에서 기본값 설정:
   - `Min Damage Ratio`: 0.1
   - `Max Damage Variance`: 0.1
   - `Crit Damage Base`: 1.5

### 🔥 3.5 AttributeAffinityTable 설정
1. `AttributeAffinityTable.asset` 선택
2. Inspector에서 `Initialize Default Values` 버튼 클릭
3. 원하는 속성 상성 값 조정

---

## 4. 테스트 데이터 생성

### 🎯 4.1 스킬 데이터 생성
1. `Assets/Resources/Skills` 폴더에서 우클릭
2. `Create > Game > Skill Data` 선택
3. 다음 스킬들 생성:

#### Fireball_Skill.asset
```
Skill Name: Fireball
Description: 화염구를 발사합니다
Type: Active
Target Type: Enemy
Cooldown: 3.0
Grade: C
Max Level: 5
Effects:
- Type: Damage
- Fixed Value: 50
- Percent Value: 0
- Attribute Type: Fire
```

#### PowerStrike_Skill.asset
```
Skill Name: Power Strike
Description: 강력한 일격을 가합니다
Type: Active
Target Type: Enemy
Cooldown: 4.0
Grade: B
Max Level: 5
Effects:
- Type: Damage
- Fixed Value: 80
- Percent Value: 20
- Attribute Type: None
```

#### DarkOrb_Skill.asset
```
Skill Name: Dark Orb
Description: 어둠의 구슬을 발사합니다
Type: Active
Target Type: Enemy
Cooldown: 2.5
Grade: D
Max Level: 5
Effects:
- Type: Damage
- Fixed Value: 40
- Percent Value: 0
- Attribute Type: Dark
```

### 🛡️ 4.2 장비 데이터 생성

#### 무기 (Weapons)
1. `Assets/Resources/Equipment/Weapons` 폴더에서 우클릭
2. `Create > Game > Equipment > Monster Equipment` 선택

**BasicSword.asset**
```
Name: Basic Sword
Description: 기본 검
Type: Weapon
Grade: Normal
Level: 1
Max Level: 10
Base Effects:
- Stat Type: Attack
- Fixed Value: 15
- Percent Value: 0
```

**FireSword.asset**
```
Name: Fire Sword
Description: 화염 검
Type: Weapon
Grade: Magic
Level: 1
Max Level: 10
Base Effects:
- Stat Type: Attack
- Fixed Value: 20
- Percent Value: 0
- Stat Type: AttributeDamageBonus
- Fixed Value: 10
- Percent Value: 0
```

#### 방어구 (Armor)
**BasicArmor.asset**
```
Name: Basic Armor
Description: 기본 갑옷
Type: Armor
Grade: Normal
Level: 1
Max Level: 10
Base Effects:
- Stat Type: Defense
- Fixed Value: 10
- Percent Value: 0
```

**IronArmor.asset**
```
Name: Iron Armor
Description: 철 갑옷
Type: Armor
Grade: Magic
Level: 1
Max Level: 10
Base Effects:
- Stat Type: Defense
- Fixed Value: 15
- Percent Value: 0
- Stat Type: MaxHP
- Fixed Value: 50
- Percent Value: 0
```

#### 악세사리 (Accessories)
**BasicRing.asset**
```
Name: Basic Ring
Description: 기본 반지
Type: Accessory
Grade: Normal
Level: 1
Max Level: 10
Base Effects:
- Stat Type: CritRate
- Fixed Value: 5
- Percent Value: 0
```

**PowerRing.asset**
```
Name: Power Ring
Description: 힘의 반지
Type: Accessory
Grade: Magic
Level: 1
Max Level: 10
Base Effects:
- Stat Type: Attack
- Fixed Value: 8
- Percent Value: 0
- Stat Type: CritMultiplier
- Fixed Value: 0.2
- Percent Value: 0
```

### 🐲 4.3 몬스터 카드 생성
1. `Assets/Resources/Cards/Monsters` 폴더에서 우클릭
2. `Create > Game > Monster Card` 선택

**Goblin_Card.asset**
```
Card Name: Goblin
Description: 작은 고블린
Grade: C
Monster Type: Beast
Base Attack Growth: C
Base Defense Growth: C
Base HP Growth: C
Base Attack: 25
Base Defense: 15
Base Max HP: 80
Skill Names: ["Fireball"]
```

**Orc_Card.asset**
```
Card Name: Orc
Description: 강한 오크
Grade: B
Monster Type: Humanoid
Base Attack Growth: B
Base Defense Growth: B
Base HP Growth: B
Base Attack: 35
Base Defense: 20
Base Max HP: 120
Skill Names: ["Power Strike"]
```

### 👨‍⚔️ 4.4 적 데이터 생성
1. `Assets/Resources/Cards/Enemies` 폴더에서 우클릭
2. `Create > Game > Enemy Data` 선택

**Hero_Data.asset**
```
Name: Hero
Description: 용감한 영웅
Enemy Type: Normal
Attribute: Light
Defense Type: Physical
Attack Type: Physical
Armor Type: Heavy
Base Attack: 30
Base Defense: 20
Base Max HP: 100
Attack Per Level: 5
Defense Per Level: 3
HP Per Level: 15
Base Score: 100
```

**Knight_Data.asset**
```
Name: Knight
Description: 성기사
Enemy Type: Elite
Attribute: Light
Defense Type: Physical
Attack Type: Physical
Armor Type: Heavy
Base Attack: 40
Base Defense: 30
Base Max HP: 150
Attack Per Level: 7
Defense Per Level: 5
HP Per Level: 20
Base Score: 200
```

---

## 5. 시스템 테스트 실행

### 🧪 5.1 SystemTestManager 설정
1. 새로운 GameObject 생성 (Hierarchy에서 우클릭 > Create Empty)
2. 이름을 `SystemTestManager`로 변경
3. `SystemTestManager` 스크립트 컴포넌트 추가
4. Inspector에서 설정:
   - `Run Test On Start`: ✅ 체크
   - `Enable Detailed Logging`: ✅ 체크
   - `Equipment Manager`: GlobalMonsterEquipmentManager 에셋 할당
   - `Enemy Manager`: EnemyManager 에셋 할당
   - `Skill Grade Config`: SkillGradeConfig 에셋 할당
   - `Test Monster Cards`: 크기 2로 설정하고 Goblin_Card, Orc_Card 할당

### 🎮 5.2 테스트 실행
1. Unity에서 Play 버튼 클릭
2. Console 창에서 테스트 결과 확인
3. 모든 테스트가 성공하면 ✅ 표시
4. 실패한 테스트가 있으면 ❌ 표시와 함께 오류 메시지 확인

### 📊 5.3 예상 테스트 결과
```
=== 시스템 전체 테스트 시작 ===
--- 리소스 로딩 테스트 ---
[SystemTest] ✓ SkillManager 로딩 - 성공
[SystemTest] ✓ GlobalMonsterEquipmentManager 로딩 - 성공
[SystemTest] ✓ EnemyManager 로딩 - 성공
[SystemTest] ✓ SkillGradeConfig 로딩 - 성공
--- 스킬 시스템 테스트 ---
[SystemTest] ✓ 스킬 데이터 존재 확인 - 성공
--- 장비 풀 관리 테스트 ---
[SystemTest] ✓ 장비 풀 유효성 검증 - 성공
[SystemTest] ✓ 무기 풀 확인 - 성공
[SystemTest] ✓ 방어구 풀 확인 - 성공
[SystemTest] ✓ 악세사리 풀 확인 - 성공
[SystemTest] ✓ 전체 장비 목록 확인 - 성공
--- 몬스터 카드 데이터 테스트 ---
[SystemTest] ✓ 몬스터 카드 데이터 유효성 - 성공
--- 적 데이터 테스트 ---
[SystemTest] ✓ 적 카드 데이터 로딩 - 성공
[SystemTest] ✓ 적 카드 검색 - 성공
--- 스킬 등급 시스템 테스트 ---
[SystemTest] ✓ 스킬 등급 설정 로딩 - 성공
--- 통합 테스트 ---
[SystemTest] ✓ 전체 시스템 데이터 로딩 테스트 - 성공
=== 시스템 테스트 결과 ===
총 테스트: 10
성공: 10
실패: 0
성공률: 100.0%
🎉 모든 테스트가 성공했습니다!
```

---

## 6. 문제 해결

### ❌ 6.1 일반적인 오류들

#### "Resources.Load failed" 오류
**원인**: ScriptableObject 에셋이 Resources 폴더에 없음
**해결**: 
1. 해당 에셋이 정확한 경로에 있는지 확인
2. 에셋 이름이 정확한지 확인
3. Unity에서 에셋을 다시 Import

#### "NullReferenceException" 오류
**원인**: 매니저의 배열이나 리스트가 비어있음
**해결**:
1. 각 매니저의 Inspector에서 배열 크기 확인
2. 모든 Element에 적절한 데이터 할당
3. null 체크 로직 확인

#### "CreateAssetMenu not found" 오류
**원인**: 스크립트 컴파일 오류
**해결**:
1. Console에서 컴파일 오류 확인 및 수정
2. Unity 재시작
3. 스크립트 재컴파일 (Ctrl+R)

### 🔧 6.2 성능 최적화

#### 로깅 최적화
- 릴리즈 빌드 시 `Enable Detailed Logging` 체크 해제
- GameLogger 사용으로 조건부 로깅 활용

#### 메모리 최적화
- 불필요한 ScriptableObject 인스턴스 생성 방지
- Object Pool 패턴 활용 (향후 확장 시)

### 📋 6.3 체크리스트

#### 설정 완료 체크리스트
- [ ] 모든 폴더 구조 생성 완료
- [ ] 모든 ScriptableObject 에셋 생성 완료
- [ ] 모든 매니저 설정 완료
- [ ] 테스트 데이터 생성 완료
- [ ] SystemTestManager 설정 완료
- [ ] 모든 테스트 성공 확인

#### 확장 준비 체크리스트
- [ ] 추가 스킬 데이터 생성 준비
- [ ] 추가 장비 데이터 생성 준비
- [ ] 추가 몬스터 카드 생성 준비
- [ ] UI 시스템 구현 준비
- [ ] 인스턴트 게임 메타 시스템 구현 준비

---

## 🎯 다음 단계

### 즉시 가능한 확장
1. **더 많은 스킬 추가**: SkillData 에셋 생성
2. **더 많은 장비 추가**: BaseMonsterEquipment 에셋 생성
3. **더 많은 몬스터 추가**: BaseMonsterCard 에셋 생성
4. **전투 시스템 테스트**: SimpleBattleTest 클래스 활용

### 중장기 확장 계획
1. **UI 시스템 구현**: Unity UI 또는 UI Toolkit 활용
2. **인스턴트 게임 메타 시스템**: 침략, 소환, 자원채취 등
3. **마왕 시스템 구현**: DemonLord 클래스 및 관련 시스템
4. **네트워크 연동**: 서버 통신 및 랭킹 시스템

---

## 📞 지원

문제가 발생하면 다음을 확인하세요:
1. Unity Console의 오류 메시지
2. SystemTestManager의 테스트 결과
3. 각 ScriptableObject의 Inspector 설정
4. Resources 폴더의 파일 구조

모든 시스템이 SOLID 원칙을 준수하여 설계되었으므로, 확장과 수정이 용이합니다. 