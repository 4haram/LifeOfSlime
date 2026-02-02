namespace Gameplay.Player.FSM
{
    public class IdleState : PlayerStateBase
    {
        public IdleState(PlayerDirector player) : base(player) {}

        public override void OnEnter()
        {
            player.Visual.Animator?.CrossFade(IdleHash, crossFadeDuration);
            player.Visual.Animator?.SetFloat(SpeedHash, 0f);
        }

        public override void Update()
        {
            var moveX = player.Input.Move.x;
            player.Locomotion.SetMoveX(moveX);
        }
    }
}