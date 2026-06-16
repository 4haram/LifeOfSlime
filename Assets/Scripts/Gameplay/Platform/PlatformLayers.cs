using UnityEngine;
namespace Assets.Scripts.Gameplay.Platform
{
    [CreateAssetMenu(fileName = "PlatformLayers", menuName = "Config/PlatformLayers")]
    public class PlatformLayers : ScriptableObject
    {
        [SerializeField] private LayerMask solidPlatformLayer;
        [SerializeField] private LayerMask oneWayPlatformLayer;

        public LayerMask Solid => solidPlatformLayer;
        public LayerMask OneWay => oneWayPlatformLayer;
        public LayerMask All => solidPlatformLayer | oneWayPlatformLayer;

        public bool IsSameLayer(GameObject target, LayerMask _mask)
        {
            return (_mask.value & (1 << target.layer)) != 0;
        }
    }
}