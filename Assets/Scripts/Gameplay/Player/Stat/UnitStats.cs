namespace Gameplay.Player.Stat
{
    public class UnitStats
    {
        public StatValue MaxHp;
        public StatValue Attack;
        public StatValue Defense;
        public StatValue MoveSpeed;
        public StatValue JumpForce;
        public StatValue Gravity;

        public UnitStats(UnitBaseStats baseStats)
        {
            MaxHp = new StatValue(baseStats.hp);
            Attack = new StatValue(baseStats.attack);
            Defense = new StatValue (baseStats.defense);
            MoveSpeed = new StatValue (baseStats.moveSpeed);
            JumpForce = new StatValue (baseStats.jumpForce);
            Gravity = new StatValue (baseStats.gravity);
        }

        public UnitStats(UnitBaseStatsSO so)
        {
            MaxHp = new StatValue(so.hp);
            Attack = new StatValue(so.attack);
            Defense = new StatValue (so.defense);
            MoveSpeed = new StatValue (so.moveSpeed);
            JumpForce = new StatValue (so.jumpForce);
            Gravity = new StatValue (so.gravity);
        }
    }
}