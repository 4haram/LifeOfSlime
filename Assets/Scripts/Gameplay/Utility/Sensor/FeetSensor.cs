using UnityEngine;

namespace GamePlay.Utility.Sensor
{
    public struct FeetSensor
    {
        private const float DEFAULT_SENSOR_HEIGHT = 0.02f;
        private const float DEFAULT_WIDTH_RATIO = 0.8f;

        public Vector2 LocalCenter { get; private set; }
        public Vector2 LocalSize { get; private set; }

        public void Initialize(Collider2D col)
        {
            if (!SensorHelper.TryGetColliderShape(col, out var size, out var offset))
            {
                return;
            }

            LocalSize = new Vector2(size.x * DEFAULT_WIDTH_RATIO, DEFAULT_SENSOR_HEIGHT);
            LocalCenter = offset + Vector2.down * (size.y * 0.5f - DEFAULT_SENSOR_HEIGHT * 0.5f);
        }
    }
}