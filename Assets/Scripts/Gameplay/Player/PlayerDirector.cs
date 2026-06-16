using Gameplay.Player.FSM;
using Gameplay.Player.Stat;
using UnityEngine;
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
        public bool IsFacingLeft { get; set; }
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

            // [점프]
            SetTransitionCondition(idleState, jumpState, new FuncPredicate(() => jumpContext.HasAccepted));
            SetTransitionCondition(movementState, jumpState, new FuncPredicate(() => jumpContext.HasAccepted));

            // [추락]
            SetTransitionCondition(idleState, fallState, new FuncPredicate(() => !locomotion.IsGrounded));
            SetTransitionCondition(movementState, fallState, new FuncPredicate(() => !locomotion.IsGrounded));
            SetTransitionCondition(jumpState, fallState, new FuncPredicate(() => locomotion.IsAtJumpPeak));

            // [이동]
            SetTransitionCondition(idleState, movementState, new FuncPredicate(() => locomotion.IsGrounded && Mathf.Abs(input.Move.x) > 0.01f));
            SetTransitionCondition(fallState, movementState, new FuncPredicate(() => locomotion.IsGrounded && Mathf.Abs(input.Move.x) > 0.01f));

            // [대기]
            SetTransitionCondition(movementState, idleState, new FuncPredicate(() => locomotion.IsGrounded && Mathf.Abs(input.Move.x) < 0.01f));
            SetTransitionCondition(fallState, idleState, new FuncPredicate(() => locomotion.IsGrounded && Mathf.Abs(input.Move.x) <= 0.01f));

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
            // 입력 수용 여부를 매 프레임마다 체크
            jumpContext.TryAccept(Time.time, locomotion.IsGrounded);

            // 착지 시점 감지
            if (locomotion.IsGrounded && !wasGrounded)
            {
                jumpContext.OnGrounded(Time.time);
            }
            wasGrounded = locomotion.IsGrounded;
        }
        private void FixedUpdate()
        {
            locomotion.TickWorkFlow();

            // FSM 업데이트, 각 상태의 Update()에 locomotion과 input을 활용한 행동 로직이 들어감
            stateMachine.Update();
        }
    }
}