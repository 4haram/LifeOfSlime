using UnityEngine;
namespace Gameplay.Player.Stat
{
    [CreateAssetMenu(fileName = "BaseStatsSO", menuName = "Gameplay/Player/Stats/Base Stats SO")]
    public class UnitBaseStatsSO : ScriptableObject
    {
        public float hp;
        public float attack;
        public float defense;
        public float moveSpeed;
        public float jumpForce;
        public float gravity;

        public UnitBaseStats ToBaseStats()
        {
            return new UnitBaseStats
            {
                hp = this.hp,
                attack = this.attack,
                defense = this.defense,
                moveSpeed = this.moveSpeed,
                jumpForce = this.jumpForce,
                gravity = this.gravity
            };
        }

        public UnitStats CreateRuntimeStats()
        {
            return new UnitStats(this);
        }
    }
}
