using UnityEngine;
using Gameplay.Player.Stat;
namespace Gameplay.Player
{
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance;
        // 에셋에서 할당
        [SerializeField] private UnitBaseStatsSO baseStatsConfig;

        public UnitStats CurrentStats { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                CurrentStats = baseStatsConfig.CreateRuntimeStats();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}