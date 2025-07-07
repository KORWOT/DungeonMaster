# 몬스터 성장 시스템 역할 분리

## 시스템 구조

### 1. MonsterGrowthConfig (설정 데이터)
**역할**: 성장 관련 모든 설정 데이터 관리
- 등급별 기본 성장 수치 (2~5, 3~6, 5~7, 6~9, 7~11)
- 성장률 수치 → 등급 자동 매핑 (0~29%=F, 30~49%=E, ...)
- 특수 성장 이벤트 설정 (크리티컬, 완벽, 균형 성장)
- 성장률 범위 설정 (0~200%)

**주요 메서드**:
- `GetGradeGrowthData()`: 등급별 성장 범위 조회
- `GetGrowthGradeFromRate()`: 수치 → 등급 변환
- `CalculateStatGrowth()`: 실제 성장량 계산
- `CalculateGrowthPrediction()`: 성장 예측

### 2. MonsterGrowthSystem (레벨업 처리)
**역할**: 몬스터의 레벨업과 경험치 처리 전담
- 레벨업 시 스텟 증가 처리
- 경험치 획득 및 자동 레벨업
- 성장 예측 및 전망 계산
- 성장률 정보 조회

**주요 메서드**:
- `ProcessLevelUp()`: 레벨업 처리
- `GainExperience()`: 경험치 획득 처리
- `GetGrowthPrediction()`: 다음 레벨업 예측
- `CalculateGrowthProjection()`: 목표 레벨까지 전망
- `GetGrowthRateInfo()`: 성장률 정보 조회

### 3. GrowthRateEnhancementSystem (성장률 강화)
**역할**: 성장률 수치 강화 기능 전담
- 개별 성장률 강화 처리
- 배치 강화 (여러 스텟 동시)
- 강화 시뮬레이션 (실제 적용 없이 결과 확인)
- 강화 비용 계산
- 성장률 재분배 (스텟간 이동)
- 성장률 초기화

**주요 메서드**:
- `EnhanceGrowthRate()`: 성장률 강화
- `EnhanceMultipleStats()`: 배치 강화
- `SimulateEnhancement()`: 강화 시뮬레이션
- `CalculateEnhancementCost()`: 비용 계산
- `RedistributeGrowthRates()`: 성장률 재분배
- `ResetGrowthRates()`: 성장률 초기화

### 4. Monster (몬스터 데이터)
**역할**: 몬스터의 상태 데이터 관리
- 기본 정보 (이름, 등급, 레벨, 경험치)
- 현재 스텟 (공격력, 방어력, 체력)
- 성장률 수치 (공격, 방어, 체력 각각 0~200%)
- 스킬 목록

**주요 메서드**:
- `LevelUp()`: MonsterGrowthSystem 호출
- `AddExperience()`: MonsterGrowthSystem 호출
- `EnhanceGrowthRate()`: GrowthRateEnhancementSystem 호출
- 기본적인 상태 관리 (체력 회복, 데미지, 스킬 관리)

## 시스템 간 의존성

```
Monster
├── MonsterGrowthSystem (레벨업/경험치)
│   └── MonsterGrowthConfig (설정 데이터)
└── GrowthRateEnhancementSystem (성장률 강화)
    └── MonsterGrowthConfig (설정 데이터)
```

## 핵심 변경사항

### 수치 기반 성장률 시스템
- **기존**: 등급 설정 → 배율 적용 (A등급 = 1.2배)
- **신규**: 성장률 수치 설정 → 등급 자동 계산 (82% → E등급, 0.82배)

### 계산 공식
```
최종 스텟 증가량 = 등급별 기본 증가량(랜덤) × (성장률 수치 / 100)
```

### 예시
```
UC등급 몬스터, 공격력 성장률 82%:
- 기본 증가량: 3~6 (UC등급 범위에서 랜덤)
- 성장률 적용: (3~6) × 0.82 = 2.46~4.92
- 등급 표시: E등급 (82%는 70~84% 범위)
```

## 확장성

### 1. 새로운 강화 방법 추가
`GrowthRateEnhancementSystem`에 새 메서드 추가만으로 가능

### 2. 새로운 성장 이벤트
`MonsterGrowthConfig`의 `SpecialGrowthSettings` 확장

### 3. 새로운 스텟 타입
`StatType` enum에 추가하고 관련 시스템에 반영

### 4. UI 연동
각 시스템의 결과 클래스들을 UI에서 직접 사용 가능

## 테스트 가능성

각 시스템이 독립적이므로:
- `MonsterGrowthConfig`: Inspector에서 설정 조정 및 검증
- `MonsterGrowthSystem`: 레벨업 로직 단위 테스트
- `GrowthRateEnhancementSystem`: 강화 로직 단위 테스트
- `Monster`: 상태 관리 테스트

## 유지보수성

- **단일 책임 원칙**: 각 시스템이 명확한 하나의 역할
- **의존성 역전**: 구체적 구현이 아닌 인터페이스에 의존
- **확장성**: 새 기능 추가 시 기존 코드 수정 최소화
- **테스트 용이성**: 각 시스템 독립적 테스트 가능 