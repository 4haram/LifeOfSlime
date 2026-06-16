using Assets.Scripts.Gameplay.Platform;
using GamePlay.Utility.Sensor;
using System.Collections.Generic;
using UnityEngine;
// TODO
// 플랫폼의 윗면을 판단하는 방법 구상
// 측면과 밑면 충돌 개선

namespace GamePlay.Utility
{
    [System.Serializable]
    public class PlatformChecker
    {
        [Header("Cast Properties")]
        [SerializeField] private float _castOffset;

        // [References]
        private Transform _transform;
        private Collider2D _collider;
        private FeetSensor _feetSensor;
        private BodySensor _bodySensor;
        private HeadSensor _headSensor;

        private Vector2 AbsLossyScale => new Vector2(Mathf.Abs(_transform.lossyScale.x), Mathf.Abs(_transform.lossyScale.y));
        // [FeetSensor properties]
        private Vector2 FeetSensorCenter => _transform.TransformPoint(_feetSensor.LocalCenter);
        private Vector2 FeetSensorSize => Vector2.Scale(_feetSensor.LocalSize, AbsLossyScale);
        // [BodySensor properties]
        private Vector2 BodySensorCenter => _transform.TransformPoint(_bodySensor.LocalCenter);
        private Vector2 BodySensorSize => Vector2.Scale(_bodySensor.LocalSize, AbsLossyScale);
        // [HeadSensor properties]
        private Vector2 HeadSensorCenter => _transform.TransformPoint(_headSensor.LocalCenter);
        private Vector2 HeadSensorSize => Vector2.Scale(_headSensor.LocalSize, AbsLossyScale);

        // [BoxCast's members]
        private ContactFilter2D _castFilter;
        private readonly List<RaycastHit2D> _castResult = new List<RaycastHit2D>(8);
        private readonly List<RaycastHit2D> _probeResult = new List<RaycastHit2D>(8);


        // [Layer]
        private PlatformLayers _layers;

        // [Const Members]
        private const float DEFAULT_CAST_DISTANCE = 0.05f;
        private const float MIN_GROUND_NORMAL_Y = 0.65f;
        private const float MIN_HIT_DISTANCE = 0.001f;
        private const float SURFACE_TOLERANCE = 0.01f;
        private const int FOOT_PROBE_COUNT = 3;

        //=====[Property]=====
        public float CastOffset { get; private set; }
        // [Hit Caching]
        public RaycastHit2D LandHit { get; private set; }
        public RaycastHit2D CeilingHit { get; private set; }
        public RaycastHit2D WallHit { get; private set; }
        public void Initialize(Transform transform, Collider2D col, PlatformLayers layers)
        {
            _transform = transform;
            _collider = col;
            _layers = layers;

            _feetSensor.Initialize(col);
            _bodySensor.Initialize(col);
            _headSensor.Initialize(col);

            _castFilter = new ContactFilter2D {useLayerMask = true, useTriggers = false};
        }
        // [Cast, get RaycastHit2D]
        private RaycastHit2D GetCastHit(Vector2 size, Vector2 offset, Vector2 direction, LayerMask layers, float castDist = DEFAULT_CAST_DISTANCE)
        {
            float skinWidth = _castOffset;

            float castDistance = Mathf.Max(Mathf.Abs(castDist), DEFAULT_CAST_DISTANCE) + skinWidth;

            _castFilter.SetLayerMask(layers);
            _castResult.Clear();
            int hitCount = Physics2D.BoxCast(offset, size, 0f, direction, _castFilter, _castResult, castDistance);
            return hitCount > 0f && HasHit(_castResult[0]) ? _castResult[0] : default;
        }
        public RaycastHit2D GetLandCastHit(float deltaY = DEFAULT_CAST_DISTANCE)
        {
            return GetCastHit(FeetSensorSize, FeetSensorCenter, Vector2.down, _layers.All, deltaY);
        }
        private Vector2 GetFootProbeOrigin(int index)
        {
            float feetBottomY = FeetSensorCenter.y - FeetSensorSize.y * 0.5f;
            float leftX = FeetSensorCenter.x - FeetSensorSize.x * 0.5f;
            float stepX = FeetSensorSize.x / (FOOT_PROBE_COUNT - 1);

            return new Vector2(leftX + stepX * index, feetBottomY + SURFACE_TOLERANCE);
        }
        private bool TryGetFootProbeHit(Vector2 origin, float distance, Collider2D target, out RaycastHit2D hit)
        {
            _castFilter.SetLayerMask(_layers.All);
            _probeResult.Clear();

            int hitCount = Physics2D.Raycast(origin, Vector2.down, _castFilter, _probeResult, distance);
            if (hitCount > 1)
            {
                _probeResult.Sort((a, b) => a.distance.CompareTo(b.distance));
            }

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D candidate = _probeResult[i];
                bool hitTarget = target == null || candidate.collider == target;

                if (hitTarget && IsValidHit(candidate) && IsLandableHit(candidate))
                {
                    hit = candidate;
                    return true;
                }
            }

            hit = default;
            return false;
        }
        private bool HasFootGroundSensorHit(float deltaY = DEFAULT_CAST_DISTANCE, Collider2D target = null)
        {
            float distance = Mathf.Max(Mathf.Abs(deltaY), DEFAULT_CAST_DISTANCE) + _castOffset + SURFACE_TOLERANCE;

            for (int i = 0; i < FOOT_PROBE_COUNT; i++)
            {
                if (TryGetFootProbeHit(GetFootProbeOrigin(i), distance, target, out _))
                {
                    return true;
                }
            }

            return false;
        }
        public RaycastHit2D GetCeilingCastHit(float deltaY = DEFAULT_CAST_DISTANCE)
        {
            return GetCastHit(HeadSensorSize, HeadSensorCenter, Vector2.up, _layers.Solid, deltaY);
        }
        public RaycastHit2D GetWallCastHit(float dirX, bool includeOneWayPlatform, float deltaX = 0f)
        {
            if (dirX == 0f) return default;

            LayerMask wallLayers = includeOneWayPlatform ? _layers.All : _layers.Solid;
            Vector2 direction = dirX > 0f ? Vector2.right : Vector2.left;
            return GetCastHit(BodySensorSize, BodySensorCenter, direction, wallLayers, deltaX);
        }
        public void ClearLandHitCache() => LandHit = default;
        // [TryGet Method]
        public bool TryGetLandHit(out RaycastHit2D hit, float deltaY = DEFAULT_CAST_DISTANCE)
        {
            float castDistance = Mathf.Max(Mathf.Abs(deltaY), DEFAULT_CAST_DISTANCE) + _castOffset;

            _castFilter.SetLayerMask(_layers.All);
            _castResult.Clear();

            int hitCount = Physics2D.BoxCast(FeetSensorCenter, FeetSensorSize, 0f, Vector2.down, _castFilter, _castResult, castDistance);
            if (hitCount > 1)
            {
                _castResult.Sort((a, b) => a.distance.CompareTo(b.distance));
            }

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D candidate = _castResult[i];

                if (IsLandableHit(candidate) && HasFootGroundSensorHit(deltaY, candidate.collider))
                {
                    hit = candidate;
                    LandHit = hit;
                    return true;
                }
            }

            hit = default;
            LandHit = default;
            return false;
        }
        public bool TryGetCeilingHit(out RaycastHit2D hit, float deltaY = 0f)
        {
            hit = GetCeilingCastHit(deltaY);
            bool result = IsCeilingHit(hit);
            CeilingHit = result ? hit : default;
            return result;
        }
        // [Validation hit]
        private bool HasHit(RaycastHit2D hit) => hit.collider != null;
        private bool IsValidHit(RaycastHit2D hit) => HasHit(hit) && hit.distance > MIN_HIT_DISTANCE;
        public bool IsLandableHit(RaycastHit2D hit) => HasHit(hit) && hit.normal.y >= MIN_GROUND_NORMAL_Y;
        public bool IsCeilingHit(RaycastHit2D hit) => HasHit(hit) && hit.normal.y <= -MIN_GROUND_NORMAL_Y;
        public bool IsValidCeilingHit()
        {
            if (!IsValidHit(CeilingHit)) return false;
            return CeilingHit.normal.y <= -MIN_GROUND_NORMAL_Y;
        }
        private bool HasGroundContact()
        {
            return HasFootGroundSensorHit();
        }
        // 발 밑 센서로 윗면 접촉 유지 여부 체크
        public bool IsGrounded(bool wasGrounded)
        {
            bool landedThisFrame = IsLandableHit(LandHit);
            bool stayedGrounded = wasGrounded && HasGroundContact();

            return landedThisFrame || stayedGrounded;
        }
        // [for Gizmos]
        public void DrawGizmos()
        {
            if (_transform == null) return;

            float skinWidth = _castOffset;
            float defaultRange = skinWidth * 2;

            Vector2 origin = FeetSensorCenter + Vector2.up * _castOffset;
            Vector2 castSize = FeetSensorSize;

            // 1. 박스캐스트 시작점 (녹색)
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(origin, castSize);

            // 2. 이 선 안에 지면이 들어와야 IsGrounded가 True임 (청색)
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + Vector2.down * defaultRange);
            if (IsLandableHit(LandHit))
            {
                // [충돌 지점 - 빨간색 구체]
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(LandHit.point, skinWidth);
                // [충돌한 박스 면 - 노란색]
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(LandHit.centroid, castSize);
            }
        }
    }
}
