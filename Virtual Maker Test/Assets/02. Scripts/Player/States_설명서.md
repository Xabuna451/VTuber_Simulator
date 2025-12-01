# VTuberState 클래스 설명서

이 문서는 `States.cs` 스크립트에 정의된 VTuber 상태 관련 데이터 구조에 대해 설명합니다. 이 구조는 Unity 기반 VTuber 시뮬레이션 게임에서 한 명의 VTuber의 전체 상태를 관리하는 데 사용됩니다.

## 1. VTuberState 클래스

- **역할:** VTuber 한 명의 전체 상태를 표현하는 루트 데이터 모델입니다.
- **포함 항목:**
  - `VTuberProfile profile` : VTuber의 기본 프로필 정보
  - `VTuberFollowers followers` : 외부 지표(구독자, 팔로워, 시청자 등)
  - `VTuberCondition condition` : 내부 컨디션(스트레스, 피로도 등)
  - `VTuberStats stats` : 능력치(도덕성, 매력, 지능 등)
  - `방송민심 민심` : 방송에 대한 대중의 평판

## 2. 세부 데이터 구조

### VTuberProfile

- VTuber의 이름, 성별, 나이 등 기본 정보
- **필드:**
  - `string vtuberName`
  - `Gender gender` (열거형)
  - `int age`

### VTuberFollowers

- 외부 지표(팔로워, 구독자, 시청자 등)
- **필드:**
  - `int youtubeSubscribers` : 유튜브 구독자 수
  - `int instagramFollowers` : 인스타 팔로워 수
  - `int channelSubscribers` : 생방송 플랫폼 구독자 수
  - `int averageViewers` : 평균 동시 시청자 수

### VTuberCondition

- 내부 컨디션(심리적/신체적 상태)
- **필드:**
  - `float stress` : 스트레스 (0~100)
  - `float fatigue` : 피로도 (0~100)
  - `float happiness` : 행복도 (0~100)
  - `float health` : 건강 (0~100)
  - `float health` : 방송 분위기 (0~100) (높을수록 긍정적)

### VTuberStats

- VTuber의 능력치
- **필드:**
  - `int morality` : 도덕성 (0~999)
  - `int charm` : 매력 (0~999)
  - `int intelligence` : 지능 (0~999)
  - (TODO: singing, talkSkill, gameSense 등 추가 가능)

### ManagerStats

- VTuber의 현실 능력

  - `int mouney` // 게임 머니. 장비 구매/훈련/이벤트 비용에 사용
  - `int fame` // 업계에서의 명성. 광고/콜라보/스폰서 발생에 영향을 줌
  - `int reputation` // 업계 평판. NPC 관계, 회사 이벤트, 협력 노선에 영향
  - `int agencyPower` // 소속사/팀의 영향력. 확장된 경제 시스템에서 사용

### 방송민심 (열거형)

- 방송에 대한 대중의 평판을 5단계로 구분
  - `나락`, `비호감`, `중립`, `호감`, `찬양`

### Gender (열거형)

- 성별 구분
  - `None`, `Male`, `Female`, `Other`

## 3. 활용 예시

- 게임 내 VTuber의 상태 저장/불러오기
- UI에 VTuber 상태 표시
- 각종 게임 로직(컨디션 변화, 팔로워 증감 등)에 활용

---

> 본 구조는 확장 가능하며, 필요에 따라 새로운 지표나 능력치를 추가할 수 있습니다.
