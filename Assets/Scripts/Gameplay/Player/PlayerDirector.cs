using Gameplay.Player.FSM;
using Gameplay.Player.Stat;
using UnityEngine;
// TODO:
// PlayerDirector에 너무 많은 기능이 집중되어 있어서 역할 분리 필요
namespace Gameplay.Player
{
    [RequireComponent(typeof(PlayerLocomotion))]
    public class PlayerDirector : MonoBehaviour
    {
        // References 관련
        [Header("References")]
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private PlayerLocomotion locomotion;
        [SerializeField] private PlayerVisual visual;
        public UnitStats CurrentStats { get; private set; }
        // ----- 지면 -----
        private bool wasGrounded;
        // 점프
        public JumpContext jumpContext;
        // FSM
        private StateMachine stateMachine;
        private IState currentState;
        // 방향
        public bool IsFacingLeft { get; set;}
        // References 공개 프로퍼티
        public PlayerLocomotion Locomotion => locomotion;
        public PlayerInputReader Input => input;
        public PlayerVisual Visual => visual;

        private void Start()
        {
            CurrentStats = PlayerDataManager.Instance.CurrentStats;
            locomotion.Initialize(CurrentStats);
            Input?.Enable();
            stateMachine.SetState(currentState);
        }
        private void Reset()
        {
            if (!locomotion) locomotion = GetComponent<PlayerLocomotion>();
            if (!input) input = GetComponent<PlayerInputReader>();
            if (!visual) visual = GetComponent<PlayerVisual>();
        }
        private void OnDestroy()
        {
            if (!input)
            {
                input.OnJumpPressed -= OnJumpPressed;
            }
        }
        private void Awake()
        {

            if (!locomotion) locomotion = GetComponent<PlayerLocomotion>();
            if (!input) input = GetComponent<PlayerInputReader>();
            if (!visual) visual = GetComponent<PlayerVisual>();
            // ----- 점프 -----
            jumpContext = new JumpContext();

            input.OnJumpPressed += OnJumpPressed;
            // ----------------
            stateMachine = new StateMachine();
            var idleState = new IdleState(this);
            var movementState = new MovementState(this);
            var jumpState = new JumpState(this);
            var fallState = new FallState(this);

            // 지상 움직임 전이
            // Move.x의 값이 0.01f보다 크면 이동, 작으면 대기 상태로 전이
            SetTransitionCondition(idleState, movementState, new FuncPredicate(() => locomotion.IsGrounded && Mathf.Abs(input.Move.x) > 0.01f));
            SetTransitionCondition(movementState, idleState, new FuncPredicate(() => locomotion.IsGrounded && Mathf.Abs(input.Move.x) < 0.01f));

            // 점프 전이
            // 점프 입력이 들어오고 지상에 있을 때 점프 상태로 전이
            SetTransitionCondition(idleState, jumpState, new FuncPredicate(() => jumpContext.HasAccepted));
            SetTransitionCondition(movementState, jumpState, new FuncPredicate(() => jumpContext.HasAccepted));

            // 추락 전이
            // 지상에 있지 않거나 점프 최고점에 위치할 때 추락 상태로 전이
            SetTransitionCondition(idleState, fallState, new FuncPredicate(() => !locomotion.IsGrounded));
            SetTransitionCondition(movementState, fallState, new FuncPredicate(() => !locomotion.IsGrounded));
            SetTransitionCondition(jumpState, fallState, new FuncPredicate(() => locomotion.IsAtJumpPeak));

            // 착지 전이
            // 추락 상태가 끝나고 나서 이동 입력에 따라 대기 또는 이동 상태로 전이
            SetTransitionCondition(fallState, idleState, new FuncPredicate(() => locomotion.IsGrounded && Mathf.Abs(input.Move.x) < 0.01f));
            SetTransitionCondition(fallState, movementState, new FuncPredicate(() => locomotion.IsGrounded && Mathf.Abs(input.Move.x) > 0.01f));

            currentState = idleState;
            IsFacingLeft = false;
        }
        private void OnJumpPressed(float time)
        {
            jumpContext.OnJumpInput(time);
        }
        private void SetTransitionCondition(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        private void Update()
        {
            if (locomotion.IsGrounded && !wasGrounded)
            {
                jumpContext.OnGrounded(Time.time);
            }

            wasGrounded = locomotion.IsGrounded;

            jumpContext.TryAccept(Time.time);
            stateMachine.Update();
        }
        private void FixedUpdate()
        {
            locomotion.TickWorkFlow();
        }
    }
}