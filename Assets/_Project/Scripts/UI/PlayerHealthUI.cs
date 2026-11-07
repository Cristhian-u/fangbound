using Fangbound.Player;
using UnityEngine;

namespace Fangbound.UI
{
    public sealed class PlayerHealthUI : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField, Min(12f)] private float blockSize = 26f;

        private void Awake()
        {
            if (playerHealth == null)
            {
                playerHealth = FindAnyObjectByType<PlayerHealth>();
            }
        }

        private void OnGUI()
        {
            if (playerHealth == null)
            {
                return;
            }

            GUI.Label(new Rect(20f, 16f, 200f, 24f), $"LIFE {playerHealth.CurrentHealth}/{playerHealth.MaximumHealth}");
            for (int index = 0; index < playerHealth.MaximumHealth; index++)
            {
                GUI.color = index < playerHealth.CurrentHealth
                    ? new Color(0.82f, 0.24f, 0.22f, 1f)
                    : new Color(0.2f, 0.2f, 0.2f, 0.8f);
                GUI.Box(new Rect(20f + index * (blockSize + 5f), 42f, blockSize, blockSize), GUIContent.none);
            }

            GUI.color = Color.white;
            if (playerHealth.IsDead)
            {
                Rect messageRect = new Rect(Screen.width * 0.5f - 150f, Screen.height * 0.5f - 36f, 300f, 72f);
                GUI.Box(messageRect, "DEFEATED\nPress R to restart");
            }
        }
    }
}