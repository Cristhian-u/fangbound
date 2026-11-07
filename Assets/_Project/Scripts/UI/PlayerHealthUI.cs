using Fangbound.Player;
using UnityEngine;

namespace Fangbound.UI
{
    public sealed class PlayerHealthUI : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField, Min(12f)] private float blockSize = 26f;
        [SerializeField] private Color filledColor = new Color(89f / 255f, 217f / 255f, 208f / 255f, 1f);
        [SerializeField] private Color emptyColor = new Color(52f / 255f, 52f / 255f, 61f / 255f, 1f);
        [SerializeField] private Color borderColor = new Color(145f / 255f, 169f / 255f, 172f / 255f, 1f);
        [SerializeField, Range(0f, 4f)] private float borderWidth = 2f;

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

            Color previousColor = GUI.color;
            GUI.color = Color.white;
            GUI.Label(new Rect(20f, 16f, 200f, 24f), $"LIFE {playerHealth.CurrentHealth}/{playerHealth.MaximumHealth}");
            for (int index = 0; index < playerHealth.MaximumHealth; index++)
            {
                Rect block = new Rect(20f + index * (blockSize + 5f), 42f, blockSize, blockSize);
                GUI.color = borderColor;
                GUI.DrawTexture(block, Texture2D.whiteTexture);
                float inset = Mathf.Clamp(borderWidth, 0f, blockSize * 0.5f);
                Rect fill = new Rect(block.x + inset, block.y + inset, block.width - inset * 2f, block.height - inset * 2f);
                GUI.color = index < playerHealth.CurrentHealth ? filledColor : emptyColor;
                GUI.DrawTexture(fill, Texture2D.whiteTexture);
            }

            GUI.color = Color.white;
            if (playerHealth.IsDead)
            {
                Rect messageRect = new Rect(Screen.width * 0.5f - 150f, Screen.height * 0.5f - 36f, 300f, 72f);
                GUI.Box(messageRect, "DEFEATED\nPress R to restart");
            }
            GUI.color = previousColor;
        }
    }
}
