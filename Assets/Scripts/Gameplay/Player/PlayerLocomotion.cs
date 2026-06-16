using Assets.Scripts.Gameplay.Platform;
using GamePlay.Utility;
using System;
using UnityEngine;
namespace Gameplay.Player
{
    // TODO:
    // 플랫폼의 윗면에서만 착지할 수 있도록 재설계 필요
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class PlayerLocomotion : MonoBehaviour
    {
        [Header("ScriptableObject Layer")]
        [SerializeField] private PlatformLayers platformLayers;

        [Header("Checker")]
        [SerializeField] private PlatformChecker platformChecker;

        // [Properties]
        public bool IsGrounded {  get; private set; }

        // x, y 방향 속도
        private Vector2 velocity;

        private float gravity;
        private float moveSpeed;
        private float jumpForce;
        // 플레이어가 입력한 방향값
        private float wantToMoveX;
        // 접촉 판단 크기
        public const float CONTACT_OFFSET = 0.01f;

        public float CurrentSpeedAbs => Mathf.Abs(velocity.x);
        public bool IsDescending => velocity.y < -0.01f;
        public bool IsAscending => velocity.y > 0.01f;
        public bool IsAtJumpPeak => MathF.Abs(velocity.y) < 0.5f && !IsGrounded;

        private void Awake()
        {
            CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();

            platformChecker.Initialize(transform, col, platformLayers);
        }
        private void OnDrawGizmos()
        {
            if (platformChecker != null)
            {
                platformChecker.DrawGizmos();
            }
        }
        // 플레이어가 땅에 붙도록 위치를 보정
        private float ResolveGrounding(float deltaY, RaycastHit2D hit)
        {
            if (IsGrounded)
            {
                velocity.y = 0f;
                return 0f;
            }
            float result = hit.distance;
            
            if (result < 0 && result > -0.001f) result = 0f;
            
            if (result > 0f && Mathf.Abs(deltaY) > result)
            {
                velocity.y = 0f;
                return -result;
            }

            if (result <= 0f)
            {
                velocity.y = 0f;
                return 0f;
            }

            return deltaY;
        }
        private float ResolveCeiling(float deltaY, RaycastHit2D hit)
        {
            velocity.y = 0f;
            float ceilingDistance = hit.distance - platformChecker.CastOffset - CONTACT_OFFSET;
            return ceilingDistance;
        }
        private float ResolveVerticalMovement(float deltaY)
        {

            // [상승 처리]
            if (velocity.y > 0f)
            {
                if (!platformChecker.TryGetCeilingHit(out var ceilingHit, deltaY)) return deltaY;

                return ResolveCeiling(deltaY, ceilingHit);
            }
            // [지면 확인]
            if (!platformChecker.TryGetLandHit(out var hit, deltaY))
            {
                return deltaY;
            }
            // [공중 상태 착지 필터]
            if (!IsGrounded && !platformChecker.IsLandableHit(hit))
            {
                return deltaY;
            }

            return ResolveGrounding(deltaY, hit);
        }
        private float ResolveHorizontalMovement(float deltaX)
        {
            float absDeltaX = Mathf.Abs(deltaX);
            bool isMovingALittle = absDeltaX < CONTACT_OFFSET;

            if (isMovingALittle && wantToMoveX == 0f) return 0f;

            float dir = isMovingALittle ? wantToMoveX : Mathf.Sign(deltaX);

            RaycastHit2D hit = platformChecker.GetWallCastHit(dir, IsGrounded && velocity.y <= 0f, deltaX);
            // 충돌한 벽이 있음
            if (hit.collider != null)
            {
                velocity.x = 0f;
                if (Mathf.Sign(wantToMoveX) == dir)
                {
                    float moveAmount = Mathf.Max(hit.distance - CONTACT_OFFSET, 0f) * dir;
                    //Debug.Log($"벽 보정 실행: moveAmount={moveAmount}");
                    return moveAmount;
                }
                else
                {
                    //Debug.Log($"벽 감지됐지만 보정 안 함: wantToMoveX={wantToMoveX}, dir={dir}");
                }
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
        private void UpdateVelocity(float dt)
        {
            // [수평 속도 결정]
            velocity.x = wantToMoveX * moveSpeed;

            // [수직 속도 결정]
            if (IsGrounded)
            {
                if (velocity.y <= 0f)
                {
                    velocity.y = 0f;
                }
                else
                {
                    velocity.y -= gravity * dt;
                }
            }
            else
            {
                velocity.y -= gravity * dt;
            }
        }
        private Vector2 CalculateCollisionFreeMovement(float dt)
        {
            // [실제 이동 가능한 값 계산]
            float deltaX = velocity.x * dt;
            float deltaY = velocity.y * dt;

            // [충돌 보정]
            deltaX = ResolveHorizontalMovement(deltaX);
            deltaY = ResolveVerticalMovement(deltaY);

            return new Vector2(deltaX, deltaY);
        }
        // 반드시 FixedUpdate에서 호출되어야 하는 함수
        public void TickWorkFlow()
        {
            float dt = Time.fixedDeltaTime;

            // [속도 업데이트]
            UpdateVelocity(dt);

            // [실제 이동량 계산]
            Vector2 finalDelta = CalculateCollisionFreeMovement(dt);

            // [위치 이동]
            transform.position += (Vector3)finalDelta;

            // [이동 후 착지 판정]
            IsGrounded = platformChecker.IsGrounded(IsGrounded);
        }
    }
}