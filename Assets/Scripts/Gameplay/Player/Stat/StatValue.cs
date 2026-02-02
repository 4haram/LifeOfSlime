using System;
using UnityEngine;

namespace Gameplay.Player.Stat
{
    public class StatValue
    {

        [SerializeField] private StatValueData data;
        private float cachedFinalValue;
        private bool isDirty = true;

        private const float MAX_STAT_VALUE = 1000000f;
        private const float MIN_STAT_VALUE = -1000000f;

        public event Action<float, float> OnBaseValueChanged;

        public StatValue(float baseValue)
        {
            data = new StatValueData();
            data.baseValue = baseValue;
            data.flatBonusValue = 0f;
            data.percentBonusValue = 0f;
            isDirty = true;
        }

        public float BaseValue
        {
            get => data.baseValue;
            set
            {
                var v = Mathf.Clamp(value, 0f, MAX_STAT_VALUE);
                if (!Mathf.Approximately(v, data.baseValue)) {
                    float oldValue = data.baseValue;
                    data.baseValue = v;
                    isDirty = true;
                    OnBaseValueChanged?.Invoke(oldValue, v);
                }
            }
        }
        public float FlatBonusValue
        {
            get => data.flatBonusValue;
            set
            {
                var v = Mathf.Max(value, MIN_STAT_VALUE);
                if (!Mathf.Approximately(v, data.flatBonusValue))
                {
                    data.flatBonusValue = v;
                    isDirty = true;
                }
            }
        }
        public float PercentBonusValue
        {
            get => data.percentBonusValue;
            set
            {
                var v = Mathf.Max(value, MIN_STAT_VALUE);
                if (!Mathf.Approximately(v, data.percentBonusValue))
                {
                    data.percentBonusValue = v;
                    isDirty = true;
                }
            }
        }
        public float FinalValue
        {
            get
            {
                if (isDirty)
                {
                    cachedFinalValue = (data.baseValue + data.flatBonusValue) * (1f + data.percentBonusValue * 0.01f);
                    isDirty = false;
                }
                return cachedFinalValue;
            }
        }
    }
    public struct StatValueData
    {
        public float baseValue;
        public float flatBonusValue;
        public float percentBonusValue;
    }
}