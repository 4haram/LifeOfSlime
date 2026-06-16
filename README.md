# LifeOfSlime

**LifeOfSlime**은 Unity로 제작 중인 2D 플랫폼 액션 프로토타입입니다.
슬라임 캐릭터의 이동, 점프, 착지, 낙하, 플랫폼 충돌 처리를 직접 구현하며, 플레이어 제어 로직을 FSM과 분리된 이동 시스템으로 구성하는 것을 목표로 했습니다.

현재 프로젝트는 완성된 게임이라기보다는, 2D 플랫폼 게임의 핵심 조작감을 만들기 위한 플레이어 컨트롤러 중심의 프로토타입입니다.

---

## 프로젝트 목표

이 프로젝트의 주요 목표는 Unity 기본 물리 동작에만 의존하지 않고, 2D 플랫폼 게임에서 자주 필요한 이동 판정을 직접 제어하는 것입니다.

특히 다음 요소를 직접 구현하는 데 초점을 두었습니다.

* 좌우 이동
* 점프
* 코요테 타임
* 점프 버퍼
* 지면 착지 판정
* 벽 / 천장 충돌 판정
* 단방향 플랫폼과 일반 플랫폼 구분
* 플레이어 상태 머신
* ScriptableObject 기반 스탯 설정

---

## 주요 기능

### 1. 플레이어 이동 시스템

플레이어 이동은 `PlayerLocomotion`에서 처리합니다.

이동 흐름은 다음 순서로 구성되어 있습니다.

1. 입력값을 기반으로 수평 속도 계산
2. 중력과 점프 상태를 기반으로 수직 속도 계산
3. 실제 이동량 계산
4. 이동 전 충돌 가능 여부 확인
5. 지면, 벽, 천장 충돌 보정
6. 최종 위치 반영
7. 이동 후 착지 상태 갱신

이 구조를 통해 단순히 Rigidbody2D에 힘을 주는 방식이 아니라, 캐릭터가 실제로 이동 가능한 거리만큼만 이동하도록 제어합니다.

---

### 2. 코요테 타임과 점프 버퍼

점프 입력은 `JumpContext`에서 별도로 관리합니다.

구현된 점프 보조 기능은 다음과 같습니다.

* **코요테 타임**
  발이 플랫폼에서 떨어진 직후에도 짧은 시간 동안 점프를 허용합니다.

* **점프 버퍼**
  착지 직전에 점프 키를 입력해도, 일정 시간 안에 착지하면 점프가 실행될 수 있도록 합니다.

* **점프 상태 플래그 관리**
  점프 입력, 점프 승인, 점프 실행 중 상태를 플래그로 분리해 관리합니다.

이 구조는 플레이어 조작감을 부드럽게 만들기 위한 기반입니다.

---

### 3. 플랫폼 충돌 판정

플랫폼 충돌은 `PlatformChecker`에서 처리합니다.

플레이어의 Collider 정보를 기반으로 다음 센서를 구성합니다.

* `FeetSensor`
  발밑 지면 판정에 사용합니다.

* `BodySensor`
  좌우 벽 충돌 판정에 사용합니다.

* `HeadSensor`
  머리 위 천장 충돌 판정에 사용합니다.

각 센서는 플레이어의 Collider 크기와 위치를 기준으로 계산되며, BoxCast와 Raycast를 사용해 충돌 여부를 검사합니다.

현재 지원하는 Collider 형태는 다음과 같습니다.

* `CapsuleCollider2D`
* `BoxCollider2D`

---

### 4. 일반 플랫폼과 단방향 플랫폼 분리

플랫폼 레이어 정보는 `PlatformLayers` ScriptableObject로 분리했습니다.

구분된 레이어는 다음과 같습니다.

* Solid Platform
* One Way Platform
* All Platform

이를 통해 일반 플랫폼과 단방향 플랫폼을 다른 방식으로 판정할 수 있도록 구성했습니다.

---

### 5. 플레이어 FSM

플레이어 상태는 FSM으로 관리합니다.

현재 구성된 상태는 다음과 같습니다.

* `IdleState`
* `MovementState`
* `JumpState`
* `FallState`
* `DownJumpState`

상태 전환 조건은 `PlayerDirector`에서 등록하며, 각 상태는 자신의 진입 처리와 업데이트 로직을 가집니다.

예를 들어, `JumpState`에 진입하면 점프 애니메이션을 재생하고 실제 점프 속도를 적용합니다.
`MovementState`와 `FallState`에서는 이동 입력을 계속 받아 수평 이동과 방향 전환을 처리합니다.

---

### 6. 애니메이션 및 방향 전환

플레이어의 시각적 처리는 `PlayerVisual`에서 담당합니다.

주요 역할은 다음과 같습니다.

* Animator 참조 관리
* SpriteRenderer 참조 관리
* 이동 방향에 따른 좌우 반전
* 상태에 따른 애니메이션 전환 보조

FSM 상태에서는 `PlayerVisual`을 통해 Idle, Movement, Jump, Fall, DownJump 애니메이션을 전환합니다.

---

### 7. 입력 처리

입력 처리는 Unity Input System을 사용합니다.

현재 기본 입력은 다음과 같습니다.

| 입력          | 동작                 |
| ----------- | ------------------ |
| Left Arrow  | 왼쪽 이동              |
| Right Arrow | 오른쪽 이동             |
| Up Arrow    | 점프                 |
| Down Arrow  | 하단 이동 / 드롭스루 확장 예정 |

이동 입력과 점프 입력은 `PlayerInputReader`에서 읽고, 플레이어 제어 로직은 입력값을 직접 참조해 동작합니다.

---

### 8. 스탯 시스템

플레이어의 기본 능력치는 ScriptableObject로 관리합니다.

현재 포함된 스탯은 다음과 같습니다.

* Max HP
* Attack
* Defense
* Move Speed
* Jump Force
* Gravity

`UnitBaseStatsSO`는 에디터에서 설정하는 기본 스탯 데이터이고, 런타임에서는 `UnitStats`로 변환되어 사용됩니다.

각 스탯은 `StatValue`로 감싸져 있으며, 다음 값을 가질 수 있습니다.

* Base Value
* Flat Bonus Value
* Percent Bonus Value
* Final Value

`Final Value`는 값이 변경되었을 때만 다시 계산되도록 캐싱 구조를 사용합니다.

---

## 코드 구조

```text
Gameplay
├── Player
│   ├── PlayerDirector.cs
│   ├── PlayerInputReader.cs
│   ├── PlayerLocomotion.cs
│   ├── PlayerVisual.cs
│   └── JumpContext.cs
│
├── Player/FSM
│   ├── IState.cs
│   ├── PlayerStateBase.cs
│   ├── IdleState.cs
│   ├── MovementState.cs
│   ├── JumpState.cs
│   ├── FallState.cs
│   ├── DownJumpState.cs
│   ├── StateMachine.cs
│   ├── Transition.cs
│   ├── ITransition.cs
│   ├── IPredicate.cs
│   └── FuncPredicate.cs
│
├── Player/Stat
│   ├── StatValue.cs
│   ├── UnitBaseStats.cs
│   ├── UnitBaseStatsSO.cs
│   └── UnitStats.cs
│
└── Utility
    ├── PlatformChecker.cs
    └── Sensor
        ├── FeetSensor.cs
        ├── BodySensor.cs
        ├── HeadSensor.cs
        └── SensorHelper.cs

Assets/Scripts/Gameplay/Platform
└── PlatformLayers.cs
```

---

## 구현 포인트

### FSM 기반 상태 분리

플레이어 상태를 `Idle`, `Movement`, `Jump`, `Fall` 등으로 분리했습니다.
각 상태는 공통 기반 클래스인 `PlayerStateBase`를 상속하고, 상태 진입 시 애니메이션 전환이나 점프 실행 같은 고유 동작을 수행합니다.

상태 전환 조건은 `IPredicate` 인터페이스와 `FuncPredicate` 클래스를 통해 등록됩니다.
이를 통해 조건식을 상태 머신 내부에 직접 작성하지 않고 외부에서 주입할 수 있도록 구성했습니다.

---

### 센서 기반 충돌 판정

플레이어 Collider 전체를 단순히 검사하는 대신, 발 / 몸 / 머리 영역을 분리해 각각 다른 목적으로 사용합니다.

* 발 센서: 착지 및 지면 유지 판정
* 몸 센서: 벽 충돌 판정
* 머리 센서: 천장 충돌 판정

이 구조는 플랫폼 게임에서 필요한 충돌 판정을 더 세밀하게 제어하기 위한 시도입니다.

---

### ScriptableObject 기반 설정 분리

플레이어의 기본 스탯과 플랫폼 레이어 정보를 ScriptableObject로 분리했습니다.
이를 통해 코드 수정 없이 Unity Inspector에서 수치를 조정할 수 있습니다.

---

### 점프 입력과 실제 점프 실행 분리

점프 키를 눌렀다고 즉시 점프하는 것이 아니라, `JumpContext`가 입력 시점과 지면 상태를 기준으로 점프 승인 여부를 판단합니다.

이 덕분에 점프 입력 처리와 실제 점프 실행 로직을 분리할 수 있고, 코요테 타임이나 점프 버퍼 같은 기능을 비교적 명확하게 관리할 수 있습니다.

---

## 현재 개발 상태

현재 구현된 부분은 다음과 같습니다.

* 플레이어 좌우 이동
* 점프
* 낙하 상태 전환
* 코요테 타임
* 점프 버퍼
* 지면 착지 판정
* FSM 기반 플레이어 상태 관리
* ScriptableObject 기반 스탯 설정
* ScriptableObject 기반 플랫폼 레이어 설정

---

## 개선 예정 사항

현재 코드 기준으로 추후 개선이 필요한 부분은 다음과 같습니다.

### 1. 플랫폼 충돌 처리 재설계

현재 충돌 판정은 동작 실험 중심으로 구현되어 있습니다.
추후에는 벽, 천장, 지면 판정 흐름을 더 일관된 구조로 정리하고, 캐싱과 보정 로직을 개선할 예정입니다.

### 2. DownJump / DropThrough 기능 확장

`DownJumpState`와 `DropThrough` 입력은 준비되어 있지만, 현재 코드 기준으로는 실제 하단 점프 전이와 단방향 플랫폼 통과 로직이 완전히 연결된 상태는 아닙니다.
추후 단방향 플랫폼을 아래로 통과하는 기능을 구현할 예정입니다.

### 3. 스탯 시스템 확장

현재는 이동 속도, 점프 힘, 중력 등 플레이어 이동에 필요한 기본 스탯을 중심으로 구성되어 있습니다.
추후에는 체력, 공격력, 방어력 등을 실제 전투 시스템과 연결할 수 있도록 확장할 예정입니다.

---

## 사용 기술

* Unity
* C#
* Unity Input System
* Unity Physics2D
* ScriptableObject
* Finite State Machine
* 2D Platformer Controller

---

## 개발 목적

이 프로젝트는 2D 플랫폼 게임의 기본 조작과 충돌 판정을 직접 구현해보며, Unity의 컴포넌트 구조와 상태 머신 설계를 학습하기 위해 제작했습니다.

특히 단순한 이동 구현을 넘어서, 플레이어 조작감에 영향을 주는 코요테 타임, 점프 버퍼, 센서 기반 착지 판정, 상태별 애니메이션 전환을 직접 설계하는 것에 중점을 두었습니다.
