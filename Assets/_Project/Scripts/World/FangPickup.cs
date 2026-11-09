using Fangbound.Player;
using Fangbound.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fangbound.World
{
    [DisallowMultipleComponent]
    public sealed class FangPickup : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private GameObject presentation;
        [SerializeField, Min(.1f)] private float interactionRange = 1.5f;
        private InputAction interactAction;

        private PlayerHealth Player
        {
            get
            {
                RoomSession session = RoomSession.Instance;
                return session != null && !session.IsLoading && session.CurrentRoom != null &&
                    session.CurrentRoom.gameObject.scene == gameObject.scene ? session.CurrentRoom.Player : null;
            }
        }
        public bool IsCollected => Player != null && Player.GetComponent<PlayerAbilities>().FangOwned;
        public bool IsVisible => presentation != null && presentation.activeSelf;
        public float InteractionRange => interactionRange;

        private void Awake()
        {
            if (inputActions == null || presentation == null || presentation == gameObject)
            {
                Debug.LogError("FangPickup necesita Input Actions y una presentación hija separada.", this);
                enabled = false;
                return;
            }
            interactAction = inputActions.FindActionMap("Player", true).FindAction("Interact", true);
            // RoomPlayerRig's PlayerCheckpoint owns the shared Interact action lifetime.
            // Collecting/hiding this object must never disable the player's interaction input.
        }

        private void Update()
        {
            RefreshVisual();
            if (interactAction.WasPressedThisFrame()) TryCollect(Player);
        }

        private bool CanCollect(PlayerHealth player)
        {
            if (!isActiveAndEnabled || player == null || player != Player || player.IsDead ||
                player.GetComponent<PlayerAbilities>().FangOwned) return false;
            PlayerController controller = player.GetComponent<PlayerController>();
            return !controller.IsControlLocked && !controller.IsDashing &&
                Vector2.Distance(player.transform.position, transform.position) <= interactionRange;
        }

        public bool TryCollect(PlayerHealth player)
        {
            if (!CanCollect(player)) return false;
            player.GetComponent<PlayerAbilities>().SetFangOwned(true);
            RefreshVisual();
            player.GetComponent<CombatVisualFeedback>()?.ShowFangAcquisition();
            return true;
        }

        private void RefreshVisual()
        {
            PlayerHealth player = Player;
            // Preserve the last presentation while the source room is being unloaded.
            if (player == null) return;
            bool visible = !player.GetComponent<PlayerAbilities>().FangOwned;
            if (presentation.activeSelf != visible) presentation.SetActive(visible);
        }

        private void OnGUI()
        {
            if (interactAction == null || !CanCollect(Player)) return;
            string binding = interactAction.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
            GUI.Box(new Rect(Screen.width * .5f - 150, Screen.height - 70, 300, 38), $"{binding} — Recoger Colmillo");
        }
    }
}
