namespace Gameplay.Player.FSM
{
    public class DownJumpState : PlayerStateBase
    {
        public DownJumpState(PlayerDirector player) : base(player) { }

        public override void OnEnter()
        {
            player.Visual.Animator?.CrossFade(DownJumpHash, crossFadeDuration);
        }

        public override void Update()
        {
            var moveX = player.Input.Move.x;
            player.Locomotion.SetMoveX(moveX);

            player.Visual.Animator?.SetFloat(SpeedHash, player.Locomotion.CurrentSpeedAbs);

            player.Visual.SetFacing(moveX);
        }
    }
}