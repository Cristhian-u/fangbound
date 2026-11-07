using UnityEngine;

namespace Fangbound.Player
{
    [CreateAssetMenu(fileName = "PlayerParryData", menuName = "Fangbound/Player Parry Data")]
    public sealed class PlayerParryData : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float windowDuration = 0.15f;
        [SerializeField, Min(0f)] private float recoveryDuration = 0.4f;
        [SerializeField, Min(0.01f)] private float feedbackDuration = 0.12f;
        [SerializeField] private Color successColor = new Color(1f, 0.85f, 0.25f, 1f);
        [SerializeField] private Color failureColor = new Color(0.9f, 0.2f, 0.18f, 1f);
        [SerializeField] private Color impactColor = new Color(0.95f, 0.34f, 0.25f, 1f);

        public float WindowDuration => windowDuration;
        public float RecoveryDuration => recoveryDuration;
        public float FeedbackDuration => feedbackDuration;
        public Color SuccessColor => successColor;
        public Color FailureColor => failureColor;
        public Color ImpactColor => impactColor;
    }
}