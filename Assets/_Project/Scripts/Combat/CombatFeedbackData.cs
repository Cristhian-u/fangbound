using UnityEngine;

namespace Fangbound.Combat
{
    [CreateAssetMenu(fileName = "CombatFeedbackData", menuName = "Fangbound/Combat Feedback Data")]
    public sealed class CombatFeedbackData : ScriptableObject
    {
        [SerializeField] private bool hitstopEnabled = true;
        [SerializeField, Range(0f, 0.2f)] private float normalHitstop = 0.05f;
        [SerializeField, Range(0f, 0.2f)] private float pogoHitstop = 0.04f;
        [SerializeField, Range(0f, 0.2f)] private float parryHitstop = 0.07f;
        [SerializeField, Min(0.01f)] private float damageFlashDuration = 0.1f;
        [SerializeField] private Color damageFlashColor = Color.white;
        [SerializeField, Min(0.01f)] private float parryFeedbackDuration = 0.16f;
        [SerializeField] private Color parryColor = new Color(0.35f, 1f, 1f, 0.85f);
        [SerializeField, Min(1f)] private float parryScale = 1.35f;
        [SerializeField, Min(0.01f)] private float postureBreakFeedbackDuration = 0.25f;
        [SerializeField] private Color postureBreakColor = new Color(1f, 0.85f, 0.2f, 0.95f);
        [SerializeField, Min(1f)] private float postureBreakScale = 1.7f;
        [SerializeField, Range(0f, 1f)] private float vulnerableBodyTint = 0.6f;
        [SerializeField, Range(0f, 0.5f)] private float vulnerableDamageFlash = 0.4f;

        public bool HitstopEnabled => hitstopEnabled;
        public float NormalHitstop => Mathf.Clamp(normalHitstop, 0f, 0.2f);
        public float PogoHitstop => Mathf.Clamp(pogoHitstop, 0f, 0.2f);
        public float ParryHitstop => Mathf.Clamp(parryHitstop, 0f, 0.2f);
        public float DamageFlashDuration => damageFlashDuration;
        public Color DamageFlashColor => damageFlashColor;
        public float ParryFeedbackDuration => parryFeedbackDuration;
        public Color ParryColor => parryColor;
        public float ParryScale => parryScale;
        public float PostureBreakFeedbackDuration => postureBreakFeedbackDuration;
        public Color PostureBreakColor => postureBreakColor;
        public float PostureBreakScale => postureBreakScale;
        public float VulnerableBodyTint => vulnerableBodyTint;
        public float VulnerableDamageFlash => vulnerableDamageFlash;
    }
}
