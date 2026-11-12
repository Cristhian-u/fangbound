using Fangbound.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fangbound.World
{
    // Small, repeatable environmental observation. It grants no item or progression.
    [DisallowMultipleComponent]
    public sealed class LookoutDiscovery : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private SpriteRenderer marker;
        [SerializeField, Min(.1f)] private float interactionRange = 1.35f;
        [SerializeField, Min(.1f)] private float messageDuration = 4f;
        [SerializeField] private string observationTitle = "Brote del mirador";
        [SerializeField, TextArea] private string observationText = "Un pequeño brote crece entre la corteza, por encima del sendero.";
        [SerializeField] private Color unseenColor = new Color(1f, .78f, .36f);
        [SerializeField] private Color examinedColor = new Color(.65f, .55f, .36f);
        private InputAction interact;
        private float messageRemaining;

        public bool IsExamined { get; private set; }
        public bool IsMessageVisible => isActiveAndEnabled && messageRemaining > 0 && Player != null && !Player.IsDead;
        public float InteractionRange => interactionRange;

        private PlayerHealth Player
        {
            get
            {
                RoomSession session = RoomSession.Instance;
                return session != null && !session.IsLoading && session.CurrentRoom != null &&
                    session.CurrentRoom.gameObject.scene == gameObject.scene ? session.CurrentRoom.Player : null;
            }
        }

        private void Awake()
        {
            if (inputActions == null || marker == null)
            {
                Debug.LogError("LookoutDiscovery necesita Input Actions y marcador visual.", this);
                enabled = false;
                return;
            }
            // PlayerCheckpoint owns the shared Interact action, as for FangPickup.
            interact = inputActions.FindActionMap("Player", true).FindAction("Interact", true);
            marker.color = unseenColor;
        }

        private void Update()
        {
            messageRemaining = Mathf.Max(0, messageRemaining - Time.unscaledDeltaTime);
            if (interact != null && interact.WasPressedThisFrame()) TryExamine(Player);
        }

        private void OnDisable() => messageRemaining = 0;

        private bool CanExamine(PlayerHealth player)
        {
            if (!isActiveAndEnabled || player == null || player != Player || player.IsDead) return false;
            PlayerController controller = player.GetComponent<PlayerController>();
            return !controller.IsControlLocked && !controller.IsDashing &&
                Vector2.Distance(player.transform.position, transform.position) <= interactionRange;
        }

        public bool TryExamine(PlayerHealth player)
        {
            if (!CanExamine(player)) return false;
            IsExamined = true;
            messageRemaining = messageDuration;
            marker.color = examinedColor;
            return true;
        }

        private void OnGUI()
        {
            if (IsMessageVisible)
            {
                float width = Mathf.Min(480, Screen.width - 24);
                Rect panel = new Rect((Screen.width - width) * .5f, Screen.height - 175, width, 95);
                GUI.Box(panel, GUIContent.none);
                var style = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 17 };
                GUI.Label(new Rect(panel.x + 12, panel.y + 8, panel.width - 24, panel.height - 16),
                    observationTitle + "\n" + observationText, style);
            }
            if (interact == null || !CanExamine(Player)) return;
            string binding = interact.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
            GUI.Box(new Rect(Screen.width * .5f - 180, Screen.height - 70, 360, 38),
                $"{binding} — Examinar brote");
        }
    }
}
