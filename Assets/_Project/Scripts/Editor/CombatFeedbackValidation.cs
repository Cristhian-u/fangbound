using System;
using System.Collections;
using System.Reflection;
using Fangbound.Combat;
using Fangbound.Enemies;
using Fangbound.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

namespace Fangbound.EditorTools
{
    public static class CombatFeedbackValidation
    {
        [MenuItem("Fangbound/Validate Combat Feedback in Play Mode")]
        public static void Run() => GroundEnemyValidation.Run();

        internal static IEnumerator Validate(PlayerHealth player)
        {
            CombatHitstop hitstop = Object.FindAnyObjectByType<CombatHitstop>();
            Require(hitstop != null, "Falta el servicio de hitstop.");
            hitstop.Cancel();
            CombatFeedbackData data = (CombatFeedbackData)Read(hitstop, "feedbackData");
            float originalScale = Time.timeScale;
            float originalFixedStep = Time.fixedDeltaTime;
            PlayerController controller = player.GetComponent<PlayerController>();
            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            PlayerParry parry = player.GetComponent<PlayerParry>();
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            CombatVisualFeedback playerVisual = player.GetComponent<CombatVisualFeedback>();
            var background = InputSystem.settings.backgroundBehavior;
            var editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>("CombatFeedbackValidationKeyboard");
            GameObject enemy = null, trainer = null;
            GameObject floor = new GameObject("Feedback Validation Ground", typeof(BoxCollider2D));
            floor.transform.position = new Vector3(100f, -2.5f);
            floor.GetComponent<BoxCollider2D>().size = new Vector2(12f, 0.8f);
            try
            {
                enemy = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(GroundEnemySceneSetup.PrefabPath),
                    new Vector3(100f, 0.5f), Quaternion.identity);
                yield return WaitReal(0.06f);
                enemy.GetComponent<GroundEnemyController>().enabled = false;
                Rigidbody2D enemyBody = enemy.GetComponent<Rigidbody2D>();
                enemyBody.gravityScale = 0f;
                enemyBody.constraints = RigidbodyConstraints2D.FreezeAll;
                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                EnemyPosture posture = enemy.GetComponent<EnemyPosture>();
                CombatVisualFeedback visual = enemy.GetComponent<CombatVisualFeedback>();
                Require(visual != null && playerVisual != null, "Faltan referencias visuales.");
                GameObject extraCollider = new GameObject("Feedback Duplicate Collider", typeof(BoxCollider2D));
                extraCollider.transform.SetParent(enemy.transform, false);
                extraCollider.layer = enemy.layer;
                extraCollider.GetComponent<BoxCollider2D>().isTrigger = true;
                yield return Until(() => (float)Read(combat, "attackTimeRemaining") <= 0f, 0.5f, "fin del ataque previo");

                PlacePlayer(player, new Vector2(103f, 2f));
                yield return WaitReal(0.04f);
                int pauses = hitstop.PauseCount;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J));
                yield return WaitReal(0.2f);
                Require(hitstop.PauseCount == pauses && health.CurrentHealth == 4, "Ataque fallido activó feedback de impacto.");
                yield return Release(keyboard);

                PlacePlayer(player, new Vector2(99.1f, 0.5f));
                yield return WaitReal(0.04f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.J));
                yield return Until(() => health.CurrentHealth == 3, 0.5f, "impacto horizontal");
                Require(hitstop.PauseCount == pauses + 1 && visual.DamageFlashCount == 1 && visual.IsDamageFlashVisible,
                    "Impacto horizontal no activó un único hitstop/destello.");
                yield return Release(keyboard);
                yield return Until(() => (float)Read(combat, "attackTimeRemaining") <= 0f && !hitstop.IsActive, 0.6f, "recuperación horizontal");

                while (!posture.IsVulnerable) Require(posture.TryApplyParryDamage(), "No se pudo preparar postura rota.");
                Require(visual.PostureBreakCount == 1 && visual.IsSignalVisible, "Falta la señal de ruptura.");
                Require(!posture.TryApplyParryDamage() && visual.PostureBreakCount == 1, "Ruptura repetida dentro de vulnerabilidad.");
                Color vulnerableIndicator = posture.VisualColor;
                PlacePlayer(player, new Vector2(99.1f, 0.5f));
                yield return WaitReal(0.04f);
                pauses = hitstop.PauseCount;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.J));
                yield return Until(() => health.CurrentHealth == 1, 0.5f, "daño vulnerable");
                Require(visual.DamageFlashCount == 2 && posture.VisualRenderer.color == vulnerableIndicator && hitstop.PauseCount == pauses + 1,
                    "El destello vulnerable borró postura o duplicó el impacto.");
                yield return Release(keyboard);
                yield return WaitReal(data.DamageFlashDuration + 0.04f);
                Require(!visual.IsDamageFlashVisible && posture.VisualRenderer.color == vulnerableIndicator, "La postura no sobrevivió al destello.");
                yield return Until(() => (float)Read(combat, "attackTimeRemaining") <= 0f && !hitstop.IsActive, 0.5f, "fin de ataque vulnerable");

                PlacePlayer(player, new Vector2(100f, -0.4f));
                yield return WaitReal(0.04f);
                pauses = hitstop.PauseCount;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.J));
                yield return Until(() => health.IsDead, 0.5f, "impacto ascendente letal");
                Require(hitstop.PauseCount == pauses + 1 && enemy.GetComponent<SpriteRenderer>().color == health.DeathColor && !visual.IsSignalVisible,
                    "El impacto letal no priorizó muerte o faltó hitstop ascendente.");
                yield return Release(keyboard);
                yield return WaitReal(data.PostureBreakFeedbackDuration + data.DamageFlashDuration);
                Require(enemy.GetComponent<SpriteRenderer>().color == health.DeathColor && !visual.IsSignalVisible, "Un feedback tardío borró muerte.");
                Debug.Log("TASK10_VISUAL_PASS: fallo sin pausa, horizontal/ascendente, impacto único, ruptura, destello vulnerable y muerte final.");

                trainer = Object.Instantiate(GameObject.Find("Parry Training Target"), new Vector3(104f, -1.35f), Quaternion.identity);
                trainer.GetComponent<TrainingAttackEmitter>().enabled = false;
                yield return WaitReal(0.04f);
                EnemyPosture trainerPosture = trainer.GetComponent<EnemyPosture>();
                CombatVisualFeedback trainerVisual = trainer.GetComponent<CombatVisualFeedback>();
                EnemyAttackHitbox trainerHitbox = trainer.GetComponentInChildren<EnemyAttackHitbox>();
                PlacePlayer(player, trainerHitbox.WorldCenter);
                controller.enabled = false;
                body.constraints = RigidbodyConstraints2D.FreezeAll;
                yield return WaitReal(data.ParryFeedbackDuration + 0.04f);
                Set(parry, "windowRemaining", 0f);
                int parryCount = playerVisual.ParryFeedbackCount;
                pauses = hitstop.PauseCount;
                Require(!parry.TryConsumeWindow() && playerVisual.ParryFeedbackCount == parryCount && hitstop.PauseCount == pauses,
                    "Parry fallido activó feedback de éxito.");
                for (int index = 0; index < 3; index++)
                {
                    yield return Until(() => !hitstop.IsActive, 0.4f, "fin de pausa de parry");
                    Set(parry, "windowRemaining", 0.15f);
                    trainerHitbox.BeginAttack(1);
                    yield return Until(() => !trainerHitbox.IsActive, 0.4f, "parry del entrenador");
                    Require(playerVisual.ParryFeedbackCount == parryCount + index + 1 && hitstop.PauseCount == pauses + index + 1 &&
                        trainerPosture.CurrentPosture == 2 - index && player.CurrentHealth == 5, "Parry duplicado, ausente o con daño.");
                }
                Require(trainerVisual.PostureBreakCount == 1 && trainerPosture.IsVulnerable && trainerVisual.IsSignalVisible,
                    "El entrenador perdió el feedback de ruptura.");
                yield return Until(() => !trainerPosture.IsVulnerable, 2.5f, "fin de vulnerabilidad del entrenador");
                Require(trainer.GetComponent<SpriteRenderer>().color != vulnerableIndicator, "No se restauró apariencia de postura tras recuperación.");
                Debug.Log("TASK10_PARRY_PASS: éxito único, fallo sin éxito, tres parries del entrenador y recuperación visual.");

                yield return ValidatePauseLifecycle(hitstop, player, keyboard);
            }
            finally
            {
                hitstop.Cancel();
                Time.timeScale = originalScale;
                InputSystem.RemoveDevice(keyboard);
                InputSystem.settings.backgroundBehavior = background;
                InputSystem.settings.editorInputBehaviorInPlayMode = editorInput;
                if (enemy != null) Object.Destroy(enemy);
                if (trainer != null) Object.Destroy(trainer);
                Object.Destroy(floor);
                player.RestartForTesting();
                controller.enabled = false;
                body.constraints = RigidbodyConstraints2D.FreezeAll;
                body.gravityScale = 0f;
                body.position = new Vector2(-10f, -1.5f);
                player.transform.position = body.position;
                body.linearVelocity = Vector2.zero;
                Physics2D.SyncTransforms();
            }
            Require(Mathf.Approximately(Time.fixedDeltaTime, originalFixedStep), "Hitstop alteró fixedDeltaTime.");
            Debug.Log("TASK10_FEEDBACK_PASS");
        }

        private static IEnumerator ValidatePauseLifecycle(CombatHitstop hitstop, PlayerHealth player, Keyboard keyboard)
        {
            CombatFeedbackData originalData = (CombatFeedbackData)Read(hitstop, "feedbackData");
            CombatFeedbackData testData = Object.Instantiate(originalData);
            try
            {
                Set(testData, "normalHitstop", 0.2f);
                Set(testData, "parryHitstop", 0.2f);
                Set(hitstop, "feedbackData", testData);
                Time.timeScale = 0.5f;
                int pauses = hitstop.PauseCount;
                CombatHitstop.Request(CombatHitstopKind.Normal);
                for (int index = 0; index < 20; index++) CombatHitstop.Request(CombatHitstopKind.Parry);
                Require(hitstop.IsActive && Time.timeScale == 0f && hitstop.PauseCount == pauses + 1, "Se acumularon pausas.");
                yield return Until(() => !hitstop.IsActive, 0.5f, "restauración automática");
                Require(Time.timeScale == 0.5f, "No restauró la escala anterior.");
                CombatHitstop.Request(CombatHitstopKind.Normal);
                hitstop.enabled = false;
                Require(Time.timeScale == 0.5f && !hitstop.IsActive, "Desactivar dejó el juego congelado.");
                hitstop.enabled = true;
                CombatHitstop.Request(CombatHitstopKind.Normal);
                hitstop.SendMessage("OnApplicationFocus", false);
                Require(Time.timeScale == 0.5f && !hitstop.IsActive, "Perder foco dejó el juego congelado.");
                CombatHitstop.Request(CombatHitstopKind.Normal);
                Time.timeScale = 0.75f;
                yield return WaitReal(0.03f);
                Require(!hitstop.IsActive && Time.timeScale == 0.75f, "Sobrescribió un cambio externo de escala.");
                Set(testData, "hitstopEnabled", false);
                pauses = hitstop.PauseCount;
                CombatHitstop.Request(CombatHitstopKind.Normal);
                Require(!hitstop.IsActive && hitstop.PauseCount == pauses && Time.timeScale == 0.75f, "No se pudo desactivar por datos.");
                Set(testData, "hitstopEnabled", true);
                Time.timeScale = 0f;
                CombatHitstop.Request(CombatHitstopKind.Normal);
                Require(!hitstop.IsActive && hitstop.PauseCount == pauses && Time.timeScale == 0f, "Se apropió de una pausa externa.");
                Time.timeScale = 1f;

                // The existing controller queues jump/dash in Update. Make the
                // test pause longer than a tap, then release before physics resumes.
                PlayerController controller = player.GetComponent<PlayerController>();
                PlayerParry parry = player.GetComponent<PlayerParry>();
                PlacePlayer(player, new Vector2(102f, -1.55f));
                player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                yield return WaitReal(0.1f);
                CombatHitstop.Request(CombatHitstopKind.Normal);
                float frozenClock = Time.time;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
                yield return Until(() => (float)Read(controller, "jumpBufferRemaining") > 0f, 0.12f, "input salto durante hitstop");
                Require(Time.time == frozenClock && hitstop.IsActive, "El reloj de combate avanzó dentro de hitstop.");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return Until(() => !hitstop.IsActive, 0.4f, "salida de pausa para salto");
                yield return Until(() => player.GetComponent<Rigidbody2D>().linearVelocity.y > 0f, 0.2f, "salto almacenado");

                PlacePlayer(player, new Vector2(102f, -1.55f));
                yield return WaitReal(0.05f);
                CombatHitstop.Request(CombatHitstopKind.Normal);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.LeftShift));
                yield return Until(() => (bool)Read(controller, "dashRequested"), 0.12f, "input dash durante hitstop");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return Until(() => !hitstop.IsActive, 0.4f, "salida de pausa para dash");
                yield return Until(() => controller.IsDashing, 0.2f, "dash almacenado");
                yield return Until(() => !controller.IsDashing, 0.5f, "fin del dash");

                PlacePlayer(player, new Vector2(102f, -1.55f));
                yield return WaitReal(0.06f);
                Require(player.TryTakeDamage(1, new Vector2(101f, -1.5f)), "No se pudo preparar invulnerabilidad.");
                CombatHitstop.Request(CombatHitstopKind.Normal);
                float invulnerability = (float)Read(player, "invulnerabilityRemaining");
                Set(parry, "windowRemaining", 0.15f);
                yield return WaitReal(0.06f);
                Require(Mathf.Approximately((float)Read(player, "invulnerabilityRemaining"), invulnerability) &&
                    Mathf.Approximately((float)Read(parry, "windowRemaining"), 0.15f), "Ventanas de combate avanzaron con la simulación congelada.");
                hitstop.Cancel();
                Set(parry, "windowRemaining", 0f);
                Debug.Log("TASK10_HITSTOP_PASS: coalescencia, restauración, desactivación/foco, escala externa, salto/dash almacenados y relojes coherentes.");
            }
            finally
            {
                hitstop.Cancel();
                hitstop.enabled = true;
                Set(hitstop, "feedbackData", originalData);
                Time.timeScale = 1f;
                Object.Destroy(testData);
            }
        }

        private static void PlacePlayer(PlayerHealth player, Vector2 position)
        {
            player.RestartForTesting();
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.position = position;
            player.transform.position = position;
            body.linearVelocity = Vector2.down * 2f;
            Physics2D.SyncTransforms();
        }
        private static IEnumerator Release(Keyboard keyboard)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return WaitReal(0.07f);
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
                if (Time.realtimeSinceStartupAsDouble > end) throw new TimeoutException($"Feedback: {context}.");
                yield return null;
            }
        }
        private static object Read(object target, string field) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
