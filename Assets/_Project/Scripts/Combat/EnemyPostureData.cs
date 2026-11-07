using UnityEngine;

namespace Fangbound.Combat
{
    [CreateAssetMenu(fileName = "EnemyPostureData", menuName = "Fangbound/Enemy Posture Data")]
    public sealed class EnemyPostureData : ScriptableObject
    {
        [SerializeField, Min(1)] private int maximumPosture = 3;
        [SerializeField, Min(1)] private int parryDamage = 1;
        [SerializeField, Min(0.01f)] private float vulnerableDuration = 1.5f;
        [SerializeField, Min(0.01f)] private float recoveryPerSecond = 1f;
        [SerializeField] private Color strainedColor = new Color(1f, 0.62f, 0.2f, 1f);
        [SerializeField] private Color vulnerableColor = new Color(0.3f, 0.95f, 0.8f, 1f);

        public int MaximumPosture => maximumPosture;
        public int ParryDamage => parryDamage;
        public float VulnerableDuration => vulnerableDuration;
        public float RecoveryPerSecond => recoveryPerSecond;
        public Color StrainedColor => strainedColor;
        public Color VulnerableColor => vulnerableColor;
    }
}