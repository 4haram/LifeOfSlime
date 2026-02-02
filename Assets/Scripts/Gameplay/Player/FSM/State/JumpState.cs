namespace Gameplay.Player.FSM
{
    public class JumpState : PlayerStateBase
    {
        public JumpState(PlayerDirector player) : base(player) {}
        public override void OnEnter()
        {
            player.jumpContext.SetExecuting();
            player.Visual.Animator?.CrossFade(JumpHash, crossFadeDuration);
            player.Locomotion.JumpImpulse();
        }

        public override void Update()
        {
            var moveX = player.Input.Move.x;
            player.Locomotion.SetMoveX(moveX);

            player.Visual.SetFacing(moveX);
        }
    }
}