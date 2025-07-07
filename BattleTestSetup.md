# 🎮 Unity 씬 기반 전투 테스트 가이드

## 📋 준비 사항
- 기존에 만든 스킬 에셋 파일들 (Fireball_Skill.asset 등)
- MonsterCardDatabase의 몬스터 카드들
- Unity 에디터

---

## 🏗️ 1단계: 테스트 씬 생성

### 1.1 새 씬 생성
```
File > New Scene > Basic (Template)
씬 이름: "BattleTestScene"으로 저장
```

### 1.2 기본 오브젝트 배치
```
Hierarchy에서 우클릭 > 3D Object > Cube
이름을 "Monster1"로 변경
Position: (-2, 0, 0)

Hierarchy에서 우클릭 > 3D Object > Cube  
이름을 "Monster2"로 변경
Position: (2, 0, 0)
```

### 1.3 테스트 매니저 오브젝트 생성
```
Hierarchy에서 우클릭 > Create Empty
이름을 "BattleTestManager"로 변경
SimpleBattleTest.cs 스크립트를 드래그해서 추가
```

---

## ⚙️ 2단계: SimpleBattleTest 설정

### 2.1 필수 설정 (Inspector에서)

#### 🎯 테스트 몬스터 설정
- **Monster1 Object**: Monster1 큐브를 드래그
- **Monster2 Object**: Monster2 큐브를 드래그  
- **Monster1 Card**: 원하는 몬스터 카드 에셋을 드래그
- **Monster2 Card**: 다른 몬스터 카드 에셋을 드래그

#### ⚔️ 테스트 스킬 설정
- **Monster1 Skills**: 사용할 스킬 에셋들을 배열에 추가
- **Monster2 Skills**: 상대 몬스터용 스킬 에셋들을 배열에 추가

### 2.2 선택사항: UI 설정

#### UI Canvas 생성 (원한다면)
```
Hierarchy 우클릭 > UI > Canvas
Canvas 하위에 Text 요소들 추가:
- Monster1InfoText
- Monster2InfoText  
- BattleLogText

Canvas 하위에 Button 요소들 추가:
- StartBattleButton
- ResetButton
- Monster1AttackButton
- Monster2AttackButton
```

---

## 🎯 3단계: 테스트 실행

### 3.1 Inspector에서 즉시 테스트
```
BattleTestManager 선택
Inspector에서 우클릭 메뉴 사용:

- "Quick Test - Monster1 Attack" → 몬스터1이 한 번 공격
- "Quick Test - Monster2 Attack" → 몬스터2가 한 번 공격  
- "Quick Test - Start Auto Battle" → 자동 전투 시작
- "Quick Test - Reset Battle" → 전투 리셋
```

### 3.2 Play 모드에서 테스트
```
Play 버튼 클릭
Game 뷰에서 확인:
- 몬스터 큐브들이 색상으로 HP 상태 표시
- Console에서 전투 로그 확인
- UI 버튼들로 조작 (설정했다면)
```

### 3.3 시각적 피드백 확인
```
몬스터 큐브 색상 의미:
🟢 초록색: HP 70% 이상 (건강)
🟡 노란색: HP 30-70% (부상)  
🔴 빨간색: HP 30% 미만 (위험)
⚫ 회색: HP 0% (사망)
```

---

## 🛠️ 4단계: 고급 설정

### 4.1 전투 설정 조정
```
Inspector의 "전투 설정" 섹션:
- Auto Mode: 체크하면 자동 진행
- Battle Speed: 전투 속도 (초 단위)
- Max Battle Turns: 최대 턴 수 (무한 루프 방지)
```

### 4.2 다양한 조합 테스트
```
몬스터 카드 조합:
- 화염 고블린 vs 철갑 골렘
- 같은 몬스터끼리 대결
- 다른 등급 몬스터들

스킬 조합:
- 공격 스킬만
- 힐링 + 공격 스킬
- 버프 + 공격 스킬  
- 다중 히트 스킬
```

---

## 📊 5단계: 결과 분석

### 5.1 Console 로그 확인
```
Window > General > Console
다음과 같은 로그들 확인:
[BattleTest] ✅ 테스트 몬스터들이 생성되었습니다!
[BattleTest] 🔥 화염 고블린 VS 철갑 골렘
[BattleTest] ⚔️ 전투 시작!
[BattleTest] 몬스터1: [기본 공격] 사용!
[BattleTest] 💥 85.0 피해! (1/1 히트)
```

### 5.2 밸런스 체크 포인트
```
✅ 체크할 것들:
- 스킬 데미지가 적절한가?
- 힐링량이 밸런스에 맞는가?
- 전투가 너무 길거나 짧지 않은가?
- 스킬 레벨업 효과가 체감되는가?
- 다양한 전략이 가능한가?
```

---

## 🐛 문제 해결

### 자주 발생하는 문제들
```
❌ "몬스터 카드가 설정되지 않았습니다!"
→ Monster1Card, Monster2Card 필드에 카드 에셋 드래그

❌ 스킬이 작동하지 않음
→ Monster1Skills, Monster2Skills 배열에 스킬 에셋 추가

❌ 몬스터가 색상 변경 안됨  
→ Monster1Object, Monster2Object에 큐브 오브젝트 연결

❌ UI가 업데이트 안됨
→ UI 요소들을 해당 필드에 연결 (선택사항)
```

---

## 🎉 테스트 완료!

이제 Unity 씬에서 직접적이고 시각적으로 전투 시스템을 테스트할 수 있습니다!

### 추천 테스트 시나리오
1. **기본 테스트**: 두 몬스터가 기본 공격만 사용
2. **스킬 테스트**: 다양한 스킬 조합 테스트
3. **밸런스 테스트**: 같은 몬스터끼리 여러 번 대결
4. **극한 테스트**: 강력한 스킬 vs 약한 몬스터
5. **지구력 테스트**: 힐링 스킬이 있는 장기전

즐거운 테스트 되세요! 🚀 