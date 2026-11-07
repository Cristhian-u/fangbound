using System;
using System.Collections;
using System.Reflection;
using Fangbound.CameraSystem;
using Fangbound.Player;
using Fangbound.UI;
using Fangbound.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

namespace Fangbound.EditorTools
{
    public static class MemoryFlameValidation
    {
        [MenuItem("Fangbound/Validate Memory Flames in Play Mode")]
        public static void Run() => GroundEnemyValidation.Run();

        internal static IEnumerator Validate(PlayerHealth player)
        {
            PlayerCheckpoint checkpoints = player.GetComponent<PlayerCheckpoint>();
            Require(checkpoints != null, "Falta PlayerCheckpoint.");
            MemoryFlame first = GameObject.Find("Memory Flame - Start").GetComponent<MemoryFlame>();
            MemoryFlame second = GameObject.Find("Memory Flame - Upper").GetComponent<MemoryFlame>();
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            PlayerController controller = player.GetComponent<PlayerController>();
            CameraFollow2D camera = Object.FindAnyObjectByType<CameraFollow2D>();
            PlayerHealthUI hud = Object.FindAnyObjectByType<PlayerHealthUI>();
            var background = InputSystem.settings.backgroundBehavior;
            var editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>("MemoryFlameValidationKeyboard");
            GameObject blocker = null;
            int rests = 0, changes = 0, respawns = 0;
            Action<MemoryFlame> restHandler = flame => rests++;
            Action<MemoryFlame> changeHandler = flame => changes++;
            Action respawnHandler = () => respawns++;
            checkpoints.ClearCheckpoint();
            checkpoints.Rested += restHandler;
            checkpoints.CheckpointChanged += changeHandler;
            player.Respawned += respawnHandler;
            try
            {
                Require(first.TryGetSafePose(player, out Vector3 firstPosition, out _) &&
                    second.TryGetSafePose(player, out _, out _), "Los puntos configurados no son seguros.");
                Vector3 originalSpawn = (Vector3)Read(player, "spawnPosition");
                PreparePlayer(player, new Vector2(-10f, -1.5f));
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
                Require(player.TryTakeDamage(player.MaximumHealth, new Vector2(-11f, -1.5f)), "No se pudo preparar muerte sin checkpoint.");
                Require(player.IsDead && !checkpoints.TryRest(first), "Se permitió descanso estando muerto.");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
                yield return Until(() => !player.IsDead, 0.5f, "R sin checkpoint");
                Require(Vector2.Distance(body.position, originalSpawn) < 0.05f && player.CurrentHealth == player.MaximumHealth,
                    "El reinicio sin checkpoint perdió el spawn original.");
                yield return Release(keyboard);

                PreparePlayer(player, new Vector2(-10f, -1.5f));
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
                yield return WaitReal(0.15f);
                Require(rests == 0 && changes == 0 && checkpoints.NearbyFlame == null && !checkpoints.TryRest(first),
                    "Se activó una llama desde lejos.");
                yield return Release(keyboard);

                PreparePlayer(player, (Vector2)first.transform.position + Vector2.up * 0.58f);
                Require(player.TryTakeDamage(3, new Vector2(-9f, -1.5f)), "No se pudo preparar curación.");
                yield return WaitReal(0.35f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
                yield return Until(() => rests == 1, 0.5f, "activar primera llama");
                Require(checkpoints.CurrentCheckpoint == first && changes == 1 && first.IsCurrent && !second.IsCurrent &&
                    first.IsActivationVisible && player.CurrentHealth == player.MaximumHealth, "Primera activación/curación incorrecta.");
                yield return WaitReal(0.4f);
                Require(rests == 1 && changes == 1 && player.CurrentHealth == player.MaximumHealth,
                    "Mantener E repitió descansos o superó la vida máxima.");
                yield return Release(keyboard);
                Require(player.TryTakeDamage(1, new Vector2(-9f, -1.5f)), "No se pudo preparar reactivación.");
                yield return WaitReal(0.35f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
                yield return Until(() => rests == 2, 0.5f, "reactivar primera llama");
                Require(changes == 1 && checkpoints.CurrentCheckpoint == first && player.CurrentHealth == player.MaximumHealth,
                    "Reactivar creó otro checkpoint o no curó.");
                yield return Release(keyboard);

                PreparePlayer(player, (Vector2)second.transform.position + Vector2.up * 0.58f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
                yield return Until(() => checkpoints.CurrentCheckpoint == second, 0.5f, "activar segunda llama");
                Require(changes == 2 && rests == 3 && second.IsCurrent && !first.IsCurrent,
                    "No se reemplazó correctamente el checkpoint.");
                yield return Release(keyboard);
                Require(second.TryGetSafePose(player, out Vector3 secondPosition, out _), "Segundo destino inseguro.");
                PreparePlayer(player, new Vector2(-10f, -1.5f));
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
                Require(player.TryTakeDamage(player.MaximumHealth, new Vector2(-11f, -1.5f)), "No se pudo preparar muerte con checkpoint.");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
                yield return Until(() => !player.IsDead, 0.5f, "R en segunda llama");
                Require(Vector2.Distance(body.position, secondPosition) < 0.06f && checkpoints.CurrentCheckpoint == second &&
                    player.CurrentHealth == player.MaximumHealth && respawns >= 2, "El checkpoint no persistió al reiniciar.");
                Require(!controller.IsControlLocked && !controller.IsDashing && !player.IsInvulnerable &&
                    player.GetComponent<PlayerCombat>().enabled && player.GetComponent<PlayerPogo>().enabled && player.GetComponent<PlayerParry>().enabled &&
                    Mathf.Approximately((float)Read(player, "knockbackControlRemaining"), 0f) &&
                    (float)Read(player.GetComponent<PlayerCombat>(), "attackTimeRemaining") <= 0f &&
                    (float)Read(player.GetComponent<PlayerPogo>(), "attackTimeRemaining") <= 0f &&
                    !player.GetComponent<PlayerParry>().IsWindowOpen && !player.GetComponent<PlayerParry>().IsRecovering &&
                    body.constraints == RigidbodyConstraints2D.FreezeRotation && body.gravityScale > 0f && Mathf.Abs(body.linearVelocity.y) < 1f,
                    "Respawn dejó estados temporales o físicos residuales.");
                Require(Read(hud, "playerHealth") == (object)player && Read(camera, "target") == (object)player.transform,
                    "HUD o cámara perdió la referencia al jugador.");
                Vector3 cameraPosition = camera.transform.position;
                camera.SnapToTarget();
                Require(Vector3.Distance(cameraPosition, camera.transform.position) < 0.1f &&
                    ((Vector3)Read(camera, "smoothVelocity")).sqrMagnitude < 0.001f, "La cámara no se sincronizó tras reaparecer.");
                yield return Release(keyboard);

                float respawnX = body.position.x;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
                yield return WaitReal(0.15f);
                Require(body.position.x > respawnX + 0.1f, "El jugador no pudo caminar después de R.");
                yield return Release(keyboard);

                // Alive calls of the existing restart API must clear combat too.
                Set(player.GetComponent<PlayerCombat>(), "attackTimeRemaining", 0.12f);
                Set(player.GetComponent<PlayerPogo>(), "attackTimeRemaining", 0.2f);
                Set(player.GetComponent<PlayerPogo>(), "attackRequested", true);
                Set(player.GetComponent<PlayerParry>(), "windowRemaining", 0.15f);
                player.RestartForTesting();
                Require((float)Read(player.GetComponent<PlayerCombat>(), "attackTimeRemaining") == 0f &&
                    (float)Read(player.GetComponent<PlayerPogo>(), "attackTimeRemaining") == 0f &&
                    !(bool)Read(player.GetComponent<PlayerPogo>(), "attackRequested") && !player.GetComponent<PlayerParry>().IsWindowOpen,
                    "El reinicio conservó un ataque/parry en curso.");

                PreparePlayer(player, (Vector2)first.transform.position + Vector2.up * 0.58f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
                yield return Until(() => checkpoints.CurrentCheckpoint == first, 0.5f, "volver a la primera llama");
                Require(changes == 3 && first.IsCurrent && !second.IsCurrent, "No se pudo volver a una llama anterior.");
                yield return Release(keyboard);

                blocker = new GameObject("Checkpoint Validation Blocker", typeof(BoxCollider2D));
                blocker.transform.position = firstPosition;
                blocker.GetComponent<BoxCollider2D>().size = Vector2.one;
                Physics2D.SyncTransforms();
                Require(!first.TryGetSafePose(player, out _, out _) && !checkpoints.TryRest(first), "Se aceptó un destino ocupado.");
                player.RestartForTesting();
                Require(Vector2.Distance(body.position, originalSpawn) < 0.05f, "Destino inválido no usó el spawn alternativo.");
                Object.Destroy(blocker);
                blocker = null;
                yield return WaitReal(0.04f);
                Require(first.TryGetSafePose(player, out _, out _), "El destino no volvió a estar disponible.");
                Debug.Log("TASK11_CHECKPOINT_PASS: E/R reales, rango, mantener pulsado, curación, reactivación, cambio/retorno, persistencia, estados, cámara/HUD y fallback seguro.");
            }
            finally
            {
                if (blocker != null) Object.Destroy(blocker);
                checkpoints.Rested -= restHandler;
                checkpoints.CheckpointChanged -= changeHandler;
                player.Respawned -= respawnHandler;
                checkpoints.ClearCheckpoint();
                InputSystem.RemoveDevice(keyboard);
                InputSystem.settings.backgroundBehavior = background;
                InputSystem.settings.editorInputBehaviorInPlayMode = editorInput;
                player.RestartForTesting();
                controller.enabled = false;
                body.constraints = RigidbodyConstraints2D.FreezeAll;
                body.gravityScale = 0f;
                body.position = new Vector2(-10f, -1.5f);
                player.transform.position = body.position;
                body.linearVelocity = Vector2.zero;
                Physics2D.SyncTransforms();
            }
        }

        private static void PreparePlayer(PlayerHealth player, Vector2 position)
        {
            player.RestartForTesting();
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            body.constraints = RigidbodyConstraints2D.FreezeAll;
            body.position = position;
            player.transform.position = position;
            body.linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms();
        }
        private static IEnumerator Release(Keyboard keyboard)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return WaitReal(0.06f);
        }
        private static IEnumerator WaitReal(float seconds)
        {
            double end = Time.realtimeSinceStartupAsDouble + seconds;
            while (Time.realtimeSinceStartupAsDouble < end) yield return null;
        }
        private static IEnumerator Until(Func<bool> condition, float timeout, string context)
        {
            double end = Time.realtimeSinceStartupAsDouble + timeout;
            while (!condition())
            {
                if (Time.realtimeSinceStartupAsDouble > end) throw new TimeoutException($"Checkpoints: {context}.");
                yield return null;
            }
        }
        private static object Read(object target, string field) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
