using UnityEngine;

namespace Fangbound.Combat
{
    public enum CombatHitstopKind { Normal, Pogo, Parry }

    [DisallowMultipleComponent]
    public sealed class CombatHitstop : MonoBehaviour
    {
        [SerializeField] private CombatFeedbackData feedbackData;
        private static CombatHitstop instance;
        private float previousTimeScale;
        private double startedAt;
        private double endsAt;

        public bool IsActive { get; private set; }
        public int PauseCount { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            if (instance != null) instance.Cancel();
            instance = null;
        }

        private void OnEnable()
        {
            if (instance != null && instance != this)
            {
                Debug.LogError("Solo debe existir un CombatHitstop activo por escena.", this);
                enabled = false;
                return;
            }
            instance = this;
        }

        public static void Request(CombatHitstopKind kind)
        {
            if (instance == null || !instance.isActiveAndEnabled || instance.feedbackData == null || !instance.feedbackData.HitstopEnabled) return;
            float duration = kind == CombatHitstopKind.Pogo ? instance.feedbackData.PogoHitstop :
                kind == CombatHitstopKind.Parry ? instance.feedbackData.ParryHitstop : instance.feedbackData.NormalHitstop;
            if (duration <= 0f) return;
            if (instance.IsActive)
            {
                // A stronger simultaneous impact can win, but never adds time to
                // the current deadline or starts a queue of pauses.
                instance.endsAt = System.Math.Max(instance.endsAt, instance.startedAt + duration);
                return;
            }
            if (Time.timeScale <= 0f) return; // Respect a pause owned by another system.
            instance.previousTimeScale = Time.timeScale;
            instance.startedAt = Time.realtimeSinceStartupAsDouble;
            instance.endsAt = instance.startedAt + duration;
            instance.IsActive = true;
            instance.PauseCount++;
            Time.timeScale = 0f;
        }

        private void Update()
        {
            if (IsActive && (feedbackData == null || !feedbackData.HitstopEnabled ||
                Time.timeScale != 0f || Time.realtimeSinceStartupAsDouble >= endsAt)) Cancel();
        }

        public void Cancel()
        {
            // If another owner has already changed the scale, keep its new value.
            if (IsActive && Time.timeScale == 0f) Time.timeScale = previousTimeScale;
            IsActive = false;
        }

        private void OnDisable()
        {
            Cancel();
            if (instance == this) instance = null;
        }

        private void OnDestroy() => OnDisable();
        private void OnApplicationFocus(bool focused) { if (!focused) Cancel(); }
        private void OnApplicationPause(bool paused) { if (paused) Cancel(); }
    }
}
