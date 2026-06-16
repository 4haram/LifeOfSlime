using UnityEngine;

namespace Gameplay.Player.FSM
{
    public abstract class PlayerStateBase : IState
    {
        protected readonly PlayerDirector player;

        protected static readonly int MovementHash = Animator.StringToHash("Movement");
        protected static readonly int JumpHash = Animator.StringToHash("Jump");
        protected static readonly int IdleHash = Animator.StringToHash("Idle");
        protected static readonly int FallHash = Animator.StringToHash("Fall");

        protected static readonly int SpeedHash = Animator.StringToHash("speed");

        protected const float crossFadeDuration = 0.1f; // 전환에 걸리는 시간
        protected PlayerStateBase(PlayerDirector player)
        {
            this.player = player;
        }
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void Update() { }
    }
}