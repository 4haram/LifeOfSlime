using System;
namespace Gameplay
{
    [Flags]
    public enum JumpFlag : byte
    {
        None = 0,                   // 점프 없음
        Inputted = 1 << 0,          // 입력 받음
        Accepted = 1 << 1,          // 점프 승인
        Executing = 1 << 2,         // 점프 실행 중
        DownJumping = 1 << 3,       // 하단 점프 중
    }

    public class JumpContext
    {
        public JumpFlag Flags { get; private set; }
        // ----- 점프 상수 -----
        private const float COYOTE_TIME = 0.2f;
        private const float JUMP_BUFFER_TIME = 0.2f;

        private float lastGroundedTime;
        private float lastJumpInputTime;

        public void OnJumpInput(float time)
        {
            lastJumpInputTime = time;
            if (!Flags.HasFlag(JumpFlag.Executing))
                Flags |= JumpFlag.Inputted;
        }
        public void OnGrounded(float time)
        {
            lastGroundedTime = time;
            Flags &= ~JumpFlag.Executing;
        }
        public void TryAccept(float time, bool isCurrentlyGrounded)
        {
            if (Flags.HasFlag(JumpFlag.Executing)) return;
            if (!Flags.HasFlag(JumpFlag.Inputted)) return;

            // 코요테 타임을 만족하거나 지상에 위치함
            bool AbleToJump = isCurrentlyGrounded || (time - lastGroundedTime) <= COYOTE_TIME;
            // 점프 버퍼 타임을 만족함
            bool isJumpBuffered = (time - lastJumpInputTime) <= JUMP_BUFFER_TIME;

            if (AbleToJump && isJumpBuffered)
            {
                Flags |= JumpFlag.Accepted;
                Flags &= ~JumpFlag.Inputted;
            }
        }
        public void SetExecuting()
        {
            Flags |= JumpFlag.Executing;
            Flags &= ~JumpFlag.Accepted;
        }
        public bool HasAccepted => Flags.HasFlag(JumpFlag.Accepted);
    }
}