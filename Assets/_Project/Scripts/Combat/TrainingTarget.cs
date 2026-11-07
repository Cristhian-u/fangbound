using UnityEngine;

namespace Fangbound.Combat
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public sealed class TrainingTarget : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int maximumHealth = 3;
        [SerializeField, Min(0.01f)] private float hitFlashDuration = 0.1f;
        [SerializeField] private Color hitFlashColor = Color.white;
        [SerializeField] private Color depletedColor = new Color(0.25f, 0.2f, 0.16f, 1f);

        private SpriteRenderer spriteRenderer;
        private Collider2D targetCollider;
        private Color healthyColor;
        private float hitFlashRemaining;

        public int CurrentHealth { get; private set; }
        public bool CanReceiveDamage => CurrentHealth > 0;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            targetCollider = GetComponent<Collider2D>();
            maximumHealth = Mathf.Max(1, maximumHealth);
            CurrentHealth = maximumHealth;
            healthyColor = spriteRenderer.color;
        }

        private void Update()
        {
            if (hitFlashRemaining <= 0f)
            {
                return;
            }

            hitFlashRemaining = Mathf.Max(0f, hitFlashRemaining - Time.deltaTime);
            if (hitFlashRemaining <= 0f)
            {
                float healthRatio = CurrentHealth / (float)maximumHealth;
                spriteRenderer.color = Color.Lerp(depletedColor, healthyColor, healthRatio);
            }
        }

        public void TakeDamage(int damage)
        {
            if (!CanReceiveDamage || damage <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            hitFlashRemaining = hitFlashDuration;
            spriteRenderer.color = hitFlashColor;

            if (CurrentHealth == 0)
            {
                targetCollider.enabled = false;
            }
        }
    }
}