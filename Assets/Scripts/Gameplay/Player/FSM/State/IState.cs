namespace Gameplay.Player.FSM
{
    public interface IState
    {
        void OnEnter();
        void OnExit();
        void Update();
    }
}