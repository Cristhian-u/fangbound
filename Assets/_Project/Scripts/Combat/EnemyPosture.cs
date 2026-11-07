using System;
using UnityEngine;

namespace Fangbound.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class EnemyPosture : MonoBehaviour
    {
        [SerializeField] private EnemyPostureData postureData;
        [SerializeField] private SpriteRenderer postureIndicator;

        private SpriteRenderer spriteRenderer;
        private SpriteRenderer feedbackRenderer;
        private CombatVisualFeedback visualFeedback;
        private Color normalColor;
        private float vulnerableRemaining;
        private float recoveryProgress;

        public int CurrentPosture { get; private set; }
        public bool IsVulnerable { get; private set; }
        public bool IsRecovering { get; private set; }
        public SpriteRenderer VisualRenderer => feedbackRenderer;
        public Color VisualColor => postureData == null ? normalColor : IsVulnerable ? postureData.VulnerableColor :
            Color.Lerp(postureData.StrainedColor, normalColor, CurrentPosture / (float)postureData.MaximumPosture);
        public event Action Broken;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            visualFeedback = GetComponent<CombatVisualFeedback>();

            if (postureData == null)
            {
                Debug.LogError("EnemyPosture necesita Posture Data.", this);
                enabled = false;
                return;
            }

            feedbackRenderer = postureIndicator != null ? postureIndicator : spriteRenderer;
            normalColor = feedbackRenderer.color;
            CurrentPosture = postureData.MaximumPosture;
        }

        private void Update()
        {
            if (IsVulnerable)
            {
                vulnerableRemaining -= Time.deltaTime;
                if (vulnerableRemaining <= 0f)
                {
                    IsVulnerable = false;
                    IsRecovering = true;
                    recoveryProgress = 0f;
                    RefreshColor();
                }

                return;
            }

            if (IsRecovering)
            {
                recoveryProgress += postureData.RecoveryPerSecond * Time.deltaTime;
                int restoredPoints = Mathf.FloorToInt(recoveryProgress);
                if (restoredPoints > 0)
                {
                    CurrentPosture = Mathf.Min(postureData.MaximumPosture, CurrentPosture + restoredPoints);
                    recoveryProgress -= restoredPoints;
                }

                if (CurrentPosture >= postureData.MaximumPosture)
                {
                    CurrentPosture = postureData.MaximumPosture;
                    IsRecovering = false;
                }

                RefreshColor();
            }
        }

        public bool TryApplyParryDamage()
        {
            if (postureData == null || IsVulnerable)
            {
                return false;
            }

            IsRecovering = false;
            recoveryProgress = 0f;
            CurrentPosture = Mathf.Max(0, CurrentPosture - postureData.ParryDamage);
            if (CurrentPosture == 0)
            {
                IsVulnerable = true;
                vulnerableRemaining = postureData.VulnerableDuration;
                Broken?.Invoke();
            }

            RefreshColor();
            return true;
        }

        private void RefreshColor()
        {
            if (feedbackRenderer == null || postureData == null || (visualFeedback != null && visualFeedback.isActiveAndEnabled))
            {
                return;
            }

            feedbackRenderer.color = VisualColor;
        }
    }
}
