using System;
namespace Gameplay
{
    [Flags]
    public enum JumpFlag : byte
    {
        None        = 0,           // 점프 없음
        Inputted    = 1 << 0,      // 입력 받음
        Accepted    = 1 << 1,      // 점프 승인
        Executing   = 1 << 2       // 점프 실행 중
    }

    public class JumpContext
    {
        public JumpFlag Flags { get; private set; }
        // ----- 점프 상수 -----
        private const float CoyoteTime = 0.2f;
        private const float JumpBufferTime = 0.2f;

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
        public void TryAccept(float time)
        {
            if (Flags.HasFlag(JumpFlag.Executing)) return;
            if (!Flags.HasFlag(JumpFlag.Inputted)) return;

            bool coyoteAllowed = (time - lastGroundedTime) <= CoyoteTime;
            bool buffered = (time - lastJumpInputTime) <= JumpBufferTime;

            if (coyoteAllowed || buffered)
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