using UnityEngine;
using UnityEngine.InputSystem;

namespace Fangbound.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerParry : MonoBehaviour
    {
        [SerializeField] private PlayerParryData parryData;
        [SerializeField] private InputActionAsset inputActions;

        private SpriteRenderer playerRenderer;
        private InputAction parryAction;
        private Color normalColor;
        private float windowRemaining;
        private float recoveryRemaining;
        private float feedbackRemaining;
        private bool feedbackVisible;

        public bool IsWindowOpen => windowRemaining > 0f;
        public bool IsRecovering => recoveryRemaining > 0f;

        private void Awake()
        {
            playerRenderer = GetComponent<SpriteRenderer>();
            normalColor = playerRenderer.color;

            if (parryData == null || inputActions == null)
            {
                Debug.LogError("PlayerParry necesita Parry Data e Input Actions.", this);
                enabled = false;
                return;
            }

            parryAction = inputActions.FindActionMap("Player", true).FindAction("Parry", true);
        }

        private void OnEnable()
        {
            if (parryAction != null && !parryAction.enabled)
            {
                parryAction.Enable();
            }
        }

        private void OnDisable()
        {
            if (parryAction != null && parryAction.enabled)
            {
                parryAction.Disable();
            }

            windowRemaining = 0f;
            recoveryRemaining = 0f;
            feedbackRemaining = 0f;
            if (playerRenderer != null)
            {
                playerRenderer.color = normalColor;
            }
        }

        private void Update()
        {
            recoveryRemaining = Mathf.Max(0f, recoveryRemaining - Time.deltaTime);

            if (feedbackVisible)
            {
                feedbackRemaining -= Time.deltaTime;
                if (feedbackRemaining <= 0f)
                {
                    feedbackVisible = false;
                    playerRenderer.color = normalColor;
                }
            }

            if (windowRemaining > 0f)
            {
                windowRemaining -= Time.deltaTime;
                if (windowRemaining <= 0f)
                {
                    windowRemaining = 0f;
                    recoveryRemaining = parryData.RecoveryDuration;
                    ShowFeedback(parryData.FailureColor);
                }
            }

            if (parryAction != null && parryAction.WasPressedThisFrame() &&
                recoveryRemaining <= 0f && windowRemaining <= 0f)
            {
                windowRemaining = parryData.WindowDuration;
            }
        }

        public bool TryConsumeWindow()
        {
            if (windowRemaining <= 0f)
            {
                return false;
            }

            windowRemaining = 0f;
            recoveryRemaining = parryData.RecoveryDuration;
            ShowFeedback(parryData.SuccessColor);
            return true;
        }

        public void ShowUnblockedImpact()
        {
            ShowFeedback(parryData.ImpactColor);
        }

        private void ShowFeedback(Color color)
        {
            playerRenderer.color = color;
            feedbackRemaining = parryData.FeedbackDuration;
            feedbackVisible = true;
        }
    }
}