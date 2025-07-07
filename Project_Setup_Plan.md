# 프로젝트 설정 및 리팩토링 계획

이 문서는 본격적인 코딩에 앞서 프로젝트의 기반을 다지기 위한 계획을 정리합니다.

## 1. 버전 관리 (Git) 설정

- **목표**: Git 사용을 위한 프로젝트 환경을 구성합니다. Unity가 생성하는 임시 파일이나 로컬 설정 파일이 원격 저장소에 올라가지 않도록 방지합니다.
- **실행 방안**:
    1.  Unity 프로젝트에 표준적인 내용이 담긴 `.gitignore` 파일을 생성합니다. 이 파일은 `Library/`, `Temp/`, `Logs/` 등 버전 관리가 필요 없는 폴더들을 무시하도록 설정합니다.
    2.  `Project Settings > Editor`에서 다음을 확인하고 설정합니다.
        -   `Version Control Mode`: `Visible Meta Files`
        -   `Asset Serialization Mode`: `Force Text`
        -   (이 설정은 이미 되어 있을 가능성이 높지만, 확인차 포함합니다.)

## 2. 에셋 폴더 구조 정리

- **목표**: 에셋을 종류별로 명확하게 분류하여 프로젝트의 유지보수성을 높입니다.
- **실행 방안**:
    1.  아래 구조에 맞춰 새로운 폴더들을 생성합니다. 기존에 있던 폴더(Scripts, Resources, Scenes)는 그대로 활용합니다.
    2.  **주의**: 파일 이동은 반드시 **Unity 에디터 내에서** 진행해야 합니다. Windows 탐색기에서 직접 옮기면 `.meta` 파일과의 연결이 끊어져 모든 참조(Reference)가 깨집니다.

```
Assets/
├── _Project/       # (신규) 씬에 항상 필요한 핵심 시스템 프리팹 (매니저 등)
├── Art/            # (신규) 2D/3D 아트 리소스
│   ├── Fonts/
│   └── Sprites/
├── Audio/          # (신규) 사운드 리소스 (BGM, SFX)
├── Prefabs/        # (신규) 재사용 가능한 게임 오브젝트 프리팹
│   ├── Characters/
│   ├── Effects/
│   └── UI/
├── Resources/      # (기존) 동적 로딩 에셋
├── Scenes/         # (기존) 씬 파일
├── ScriptableObjects/ # (신규) 모든 ScriptableObject 데이터 에셋
│   ├── Cards/
│   ├── Equipment/
│   ├── Skills/
│   └── System/       # 시스템 설정용 SO (GradeConfig 등)
└── Scripts/        # (기존) C# 스크립트
```

## 3. 글로벌 언어 설정을 위한 리팩토링 (지역화 시스템)

- **목표**: 게임 내 모든 텍스트(UI, 스킬 설명 등)를 코드에서 분리하여, 향후 다국어 지원이 용이한 구조를 만듭니다.
- **실행 방안 (설계 제안)**:
    1.  **`LocalizationManager` 구현**: 현재 언어 설정을 관리하고, 텍스트 요청에 맞는 문자열을 반환하는 싱글톤 매니저를 만듭니다.
    2.  **`StringTable` ScriptableObject 구현**:
        -   `Key` (string)와 `Value` (언어별 string 딕셔너리)를 가지는 데이터 테이블을 `ScriptableObject`로 설계합니다.
        -   예: `skill_name_fireball`이라는 키에 `{"Korean": "화염구", "English": "Fireball"}` 형태의 값을 저장합니다.
    3.  **`LocalizedText` 컴포넌트 구현**:
        -   UI의 `TextMeshPro - Text` 컴포넌트와 함께 사용하여, 지정된 `Key`에 해당하는 텍스트를 `LocalizationManager`로부터 받아와 자동으로 표시하는 컴포넌트를 만듭니다.
    4.  **코드 리팩토링**:
        -   `SkillData`, `BaseMonsterEquipment` 등 `ScriptableObject`에 있던 `skillName`, `description` 같은 필드를 텍스트 키 (`skill_name_key`, `description_key`)로 대체합니다.
        -   `SkillDescriptionProcessor` 등 동적으로 문자열을 생성하던 클래스들이 `LocalizationManager`를 통해 지역화된 포맷 문자열을 가져와 사용하도록 수정합니다.
        -   모든 `Debug.Log`를 포함한 하드코딩된 문자열을 검토하고, 필요한 경우 지역화 시스템을 태우거나 별도의 디버그 메시지 시스템으로 분리합니다.

---

위 계획에 동의하시면, 먼저 **1번 (Git 설정)**과 **2번 (폴더 생성)**을 실행하겠습니다. **3번 (지역화 리팩토링)**은 규모가 크므로, 1, 2번 완료 후 단계적으로 진행하는 것을 추천합니다. 