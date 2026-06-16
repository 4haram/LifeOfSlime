using UnityEngine;

namespace GamePlay.Utility.Sensor
{
    public static class SensorHelper
    {
        public static bool TryGetColliderShape(Collider2D col, out Vector2 size, out Vector2 offset)
        {
            // 현재 지원하는 콜라이더 형태: CapsuleCollider2D, BoxCollider2D
            if (col == null)
            {
                size = Vector2.zero;
                offset = Vector2.zero;
                Debug.LogError("[Error] Collider2D is null.");
                return false;
            }

            if (col is CapsuleCollider2D capsule)
            {
                size = capsule.size;
                offset = capsule.offset;
                return true;
            }

            if (col is BoxCollider2D box)
            {
                size = box.size;
                offset = box.offset;
                return true;
            }
            // 다른 콜라이더 형태가 추가될 시 여기에 초기화 로직 추가

            size = Vector2.zero;
            offset = Vector2.zero;
            Debug.Log("[Warning] Unsupported collider type: " + col.GetType().Name);
            return false;
        }
    }
}