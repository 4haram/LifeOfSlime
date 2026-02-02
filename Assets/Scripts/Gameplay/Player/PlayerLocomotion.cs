using UnityEngine;
namespace Gameplay.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerLocomotion : MonoBehaviour
    {
        [Header("OneWayPlatform Check")]
        [SerializeField] private LayerMask ? oneWayPlatformLayer;

        [Header("SolidPlatform Check")]
        [SerializeField] private LayerMask ? solidPlatformLayer;

        private Collider2D playerCollider;
        private Rigidbody2D rigidBody;
        private const float Skin = 0.01f;

        private Vector2 velocity;
        private float gravity;
        private float moveSpeed;
        private float jumpForce;
        // 플레이어가 입력한 방향값
        private float wantToMoveX;
        // 점프시 충돌한 대상
        private RaycastHit2D[] verticalHits = new RaycastHit2D[1];
        // 좌우 이동시 충돌한 대상
        private RaycastHit2D[] horizentalHits = new RaycastHit2D[1];
        private void Awake() {
            if (!rigidBody)
                rigidBody = GetComponent<Rigidbody2D>();
            if (!playerCollider)
                playerCollider = GetComponent<Collider2D>();
            if (oneWayPlatformLayer == null)
                oneWayPlatformLayer = LayerMask.GetMask("OneWayPlatform");
            if (solidPlatformLayer == null)
                solidPlatformLayer = LayerMask.GetMask("SolidPlatform");
        }
        public float CurrentSpeedAbs => Mathf.Abs(velocity.x);
        public bool IsAtJumpPeak => velocity.y <= 0f;
        public bool IsGrounded { get; private set; }
        private float SnapVerticalMovement(float deltaY)
        {
            float dir = Mathf.Sign(deltaY);
            float dist = Mathf.Abs(deltaY);

            LayerMask layerMask = solidPlatformLayer.Value;
            if (dir <= 0) layerMask |= oneWayPlatformLayer.Value;

            ContactFilter2D filter = new ContactFilter2D();
            filter.useLayerMask = true;
            filter.SetLayerMask(layerMask);
            filter.useTriggers = false;

            int hitCount = playerCollider.Cast(Vector2.up * dir, filter, verticalHits, dist + Skin);

            if (hitCount > 0)
            {
                var hit = verticalHits[0];
                bool isSolid = (solidPlatformLayer.Value & (1 << hit.collider.gameObject.layer)) != 0;
                bool canLand = isSolid || velocity.y <= 0f;
                if (canLand)
                {
                    velocity.y = 0f;
                    IsGrounded = true;
                    return Mathf.Max(hit.distance - Skin, 0f) * dir;
                }
            }
            IsGrounded = false;
            return deltaY;
        }
        private float SnapHorizontalMovement(float deltaX)
        { 
            float dir = Mathf.Sign(deltaX);
            float dist = Mathf.Abs(deltaX);

            ContactFilter2D filter = new ContactFilter2D();
            filter.useLayerMask = true;
            filter.SetLayerMask(solidPlatformLayer.Value | oneWayPlatformLayer.Value);
            filter.useTriggers = false;

            int hitCount = playerCollider.Cast(Vector2.right * dir, filter, horizentalHits, dist + Skin);

            if (hitCount > 0 && IsGrounded)
            {
                velocity.x = 0f;
                return Mathf.Max(horizentalHits[0].distance - Skin, 0f) * dir;

            }
            return deltaX;
        }
        public void Initialize(Stat.UnitStats stats)
        {
            moveSpeed = stats.MoveSpeed.FinalValue;
            jumpForce = stats.JumpForce.FinalValue;
            gravity = stats.Gravity.FinalValue;
        }
        public void SetMoveX(float x) => wantToMoveX = x;

        public void JumpImpulse()
        {
            velocity.y = jumpForce;
        }
        public void TickWorkFlow()
        {
            float dt = Time.fixedDeltaTime;

            velocity.x = wantToMoveX * moveSpeed;
            velocity.y += gravity * dt;

            float deltaX = velocity.x * dt;
            float deltaY = velocity.y * dt;

            deltaX = SnapHorizontalMovement(deltaX);
            deltaY = SnapVerticalMovement(deltaY);

            Vector2 delta = new Vector2(deltaX, deltaY);
            rigidBody.MovePosition(rigidBody.position + delta);
        }
    }
}