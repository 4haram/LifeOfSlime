using UnityEngine;

namespace GamePlay.Utility.Sensor
{
    public struct BodySensor
    {
        public Vector2 LocalCenter { get; private set; }
        public Vector2 LocalSize { get; private set; }

        public void Initialize(Collider2D col)
        {
            if (!SensorHelper.TryGetColliderShape(col, out var size, out var offset))
            {
                return;
            }

            LocalSize = size * 0.9f;
            LocalCenter = offset;
        }
    }
}