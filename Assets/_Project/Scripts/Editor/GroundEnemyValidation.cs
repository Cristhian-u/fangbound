using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Fangbound.Combat;
using Fangbound.Enemies;
using Fangbound.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

namespace Fangbound.EditorTools
{
    // Uses the real MovementTest components and physics. Reflection only arranges
    // input windows/positions and invokes the existing player attack query.
    [InitializeOnLoad]
    public static class GroundEnemyValidation
    {
        private const string RunningKey = "Fangbound.Task09.Validation";
        private static readonly Stack<IEnumerator> routines = new Stack<IEnumerator>();
        private static bool hadRuntimeError;
        private static double startedAt;

        static GroundEnemyValidation()
        {
            EditorApplication.update += Tick;
            Application.logMessageReceived += (message, trace, type) =>
            {
                if (SessionState.GetBool(RunningKey, false) &&
                    (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)) hadRuntimeError = true;
            };
        }

        [MenuItem("Fangbound/Validate Ground Enemy in Play Mode")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Ejecutar la validación desde Edit Mode.");
            EditorSceneManager.OpenScene(GroundEnemySceneSetup.ScenePath, OpenSceneMode.Single);
            GroundEnemySceneSetup.ValidateScene();
            hadRuntimeError = false;
            SessionState.SetBool(RunningKey, true);
            EditorApplication.EnterPlaymode();
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(RunningKey, false) || !EditorApplication.isPlaying || EditorApplication.isPaused) return;
            try
            {
                if (routines.Count == 0)
                {
                    startedAt = EditorApplication.timeSinceStartup;
                    routines.Push(Validate());
                }
                if (EditorApplication.timeSinceStartup - startedAt > 120d) throw new TimeoutException("Validación excedió 120 s.");
                IEnumerator current = routines.Peek();
                if (!current.MoveNext())
                {
                    routines.Pop();
                    if (routines.Count == 0) Finish(!hadRuntimeError);
                }
                else if (current.Current is IEnumerator nested) routines.Push(nested);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Finish(false);
            }
        }

        private static void Finish(bool success)
        {
            SessionState.SetBool(RunningKey, false);
            routines.Clear();
            Debug.Log(success ? "TASK09_PLAYMODE_PASS" : "TASK09_PLAYMODE_FAIL");
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
            else EditorApplication.ExitPlaymode();
        }

        private static IEnumerator Validate()
        {
            yield return Wait(0.2f);
            GroundEnemyController enemy = Object.FindAnyObjectByType<GroundEnemyController>();
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            EnemyPosture posture = enemy.GetComponent<EnemyPosture>();
            EnemyAttackHitbox hitbox = enemy.GetComponentInChildren<EnemyAttackHitbox>();
            PlayerHealth player = Object.FindAnyObjectByType<PlayerHealth>();
            PlayerController playerController = player.GetComponent<PlayerController>();
            PlayerParry parry = player.GetComponent<PlayerParry>();
            TrainingAttackEmitter trainer = Object.FindAnyObjectByType<TrainingAttackEmitter>();
            EnemyAttackHitbox trainerHitbox = trainer.GetComponentInChildren<EnemyAttackHitbox>();
            EnemyPosture trainerPosture = trainer.GetComponent<EnemyPosture>();
            trainer.enabled = false;
            yield return ValidateCollisionAndRecovery(player, enemy);
            yield return GroundEnemyPogoValidation.Validate(player);
            FreezePlayer(player, new Vector2(-10f, -1.5f));

            Rigidbody2D enemyBody = enemy.GetComponent<Rigidbody2D>();
            float previousX = enemyBody.position.x;
            float patrolMin = previousX, patrolMax = previousX;
            bool movedLeft = false, movedRight = false;
            float until = Time.time + 9f;
            while (Time.time < until)
            {
                float x = enemyBody.position.x;
                patrolMin = Mathf.Min(patrolMin, x);
                patrolMax = Mathf.Max(patrolMax, x);
                movedLeft |= x < previousX - 0.001f;
                movedRight |= x > previousX + 0.001f;
                Require(x >= enemy.LeftLimit - 0.05f && x <= enemy.RightLimit + 0.05f, "Patrulla excedió límites.");
                Require(enemy.State == GroundEnemyController.EnemyState.Patrol, "Detectó jugador fuera de rango.");
                previousX = x;
                yield return null;
            }
            Require(movedLeft && movedRight, $"La patrulla no cambió de sentido: min={patrolMin}, max={patrolMax}, pos={enemyBody.position}, velocity={enemyBody.linearVelocity}, left={movedLeft}, right={movedRight}.");
            Debug.Log("TASK09_PASS: patrulla en ambos sentidos dentro de límites.");

            PlaceEnemy(enemy, new Vector2(4.5f, -1.58f));
            FreezePlayer(player, new Vector2(6.7f, -1.55f));
            yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Chase, 1f);
            yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Telegraph, 2f);
            Require(enemy.transform.position.x > 4.8f, "No persiguió al jugador.");
            float lockedSide = Mathf.Sign(hitbox.transform.position.x - enemy.transform.position.x);
            FreezePlayer(player, (Vector2)enemy.transform.position + new Vector2(-lockedSide * 0.9f, 0f));
            yield return Wait(0.35f);
            Require(Mathf.Sign(hitbox.transform.position.x - enemy.transform.position.x) == lockedSide, "Cambió de lado durante telegraph.");
            yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Recovery, 1f);
            Require(player.CurrentHealth == 5, "El ataque alcanzó el lado contrario al telegraph.");

            yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Telegraph, 2f);
            FreezePlayer(player, hitbox.WorldCenter);
            yield return Until(() => player.CurrentHealth == 4, 1f);
            yield return Wait(0.2f);
            Require(player.CurrentHealth == 4, "Un ataque aplicó daño repetido.");
            FreezePlayer(player, new Vector2(-10f, -1.5f));
            yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Patrol, 2f);
            Debug.Log("TASK09_PASS: detección, persecución, dirección fijada, daño único y pérdida del objetivo.");

            enemy.enabled = false;
            player.RestartForTesting();
            player.transform.SetParent(trainer.transform.parent, true);
            GameObject extraCollider = new GameObject("Validation Extra Player Collider", typeof(BoxCollider2D));
            extraCollider.transform.SetParent(player.transform, false);
            extraCollider.GetComponent<BoxCollider2D>().isTrigger = true;
            FreezePlayer(player, new Vector2(-7.15f, -1.4f));
            Require(Vector2.Distance(trainerHitbox.WorldCenter, new Vector2(-5.6f, -1.4f)) < 0.01f &&
                Vector2.Distance(trainerHitbox.WorldSize, new Vector2(3.6f, 0.75f)) < 0.01f, "Cambió la zona original del entrenador.");
            IncomingAttackOutcome result = IncomingAttackOutcome.Ignored;
            trainerHitbox.AttackResolved += outcome => result = outcome;
            trainerHitbox.BeginAttack(1);
            yield return Until(() => !trainerHitbox.IsActive, 0.5f);
            Require(result == IncomingAttackOutcome.Hit && player.CurrentHealth == 4, "Entrenador no golpeó en el extremo original o filtró hermanos.");
            yield return Wait(0.1f);
            Require(player.CurrentHealth == 4, "Colliders múltiples duplicaron el daño.");
            SetField(parry, "windowRemaining", 0.15f);
            trainerHitbox.BeginAttack(1);
            yield return Until(() => !trainerHitbox.IsActive, 0.5f);
            Require(result == IncomingAttackOutcome.IgnoredByInvulnerability && trainerPosture.CurrentPosture == 3 && parry.IsWindowOpen,
                "Cambió el contrato de impacto durante invulnerabilidad.");

            player.RestartForTesting();
            FreezePlayer(player, new Vector2(-7.15f, -1.4f));
            SetField(playerController, "isDashing", true);
            SetField(parry, "windowRemaining", 0.15f);
            trainerHitbox.BeginAttack(1);
            yield return Until(() => !trainerHitbox.IsActive, 0.5f);
            Require(result == IncomingAttackOutcome.AvoidedByDash && player.CurrentHealth == 5 &&
                trainerPosture.CurrentPosture == 3 && parry.IsWindowOpen, "Dash no conservó prioridad sobre parry.");
            SetField(playerController, "isDashing", false);
            SetField(parry, "windowRemaining", 0f);
            Object.Destroy(extraCollider);

            SetEnumField(trainer, "state", "Waiting");
            SetField(trainer, "stateRemaining", 0.1f);
            trainer.enabled = true;
            yield return Until(() => GetField(trainer, "state").ToString() == "Telegraphing", 0.5f);
            yield return Until(() => GetField(trainer, "state").ToString() == "Active", 1f);
            yield return Until(() => !trainerHitbox.IsActive, 0.1f);
            Require(GetField(trainer, "state").ToString() == "Active", "Resolver el golpe acortó la fase activa del entrenador.");
            yield return Until(() => GetField(trainer, "state").ToString() == "Cooldown", 0.3f);
            trainer.enabled = false;
            player.RestartForTesting();
            FreezePlayer(player, new Vector2(-7.15f, -1.4f));
            for (int index = 0; index < 3; index++)
            {
                SetField(parry, "windowRemaining", 0.15f);
                trainerHitbox.BeginAttack(1);
                yield return Until(() => !trainerHitbox.IsActive, 0.2f);
                Require(result == IncomingAttackOutcome.Parried && trainerPosture.CurrentPosture == 2 - index && player.CurrentHealth == 5,
                    "El entrenador perdió el parry o aplicó postura más de una vez.");
            }
            Require(trainerPosture.IsVulnerable && trainer.GetComponent<SpriteRenderer>().color ==
                AssetDatabase.LoadAssetAtPath<EnemyPostureData>("Assets/_Project/Data/Enemies/EnemyPostureData.asset").VulnerableColor,
                "Se perdió el feedback de postura del entrenador sin indicador opcional.");
            Debug.Log("TASK09_PASS: entrenador conserva área, fases, daño, i-frames, dash, tres parries y feedback de postura.");

            player.RestartForTesting();
            FreezePlayer(player, new Vector2(-10f, -1.5f));
            PlaceEnemy(enemy, new Vector2(4.5f, -1.58f));
            HitWithPlayerQuery(player, enemy);
            Require(health.CurrentHealth == 3, "IDamageable no recibió el ataque normal del jugador.");
            FreezePlayer(player, new Vector2(5.6f, -1.58f));
            enemy.enabled = true;
            for (int index = 0; index < 3; index++)
            {
                yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Telegraph, 3f);
                FreezePlayer(player, hitbox.WorldCenter);
                yield return Until(() => (float)GetField(enemy, "stateRemaining") < 0.09f, 1f);
                SetField(parry, "windowRemaining", 0.15f);
                int expected = 2 - index;
                yield return Until(() => posture.CurrentPosture == expected, 0.4f);
                Require(player.CurrentHealth == 5, "Parry causó daño al jugador.");
            }
            Require(posture.IsVulnerable && enemy.State == GroundEnemyController.EnemyState.Vulnerable && !hitbox.IsActive,
                "Romper postura no canceló el ataque/IA.");
            HitWithPlayerQuery(player, enemy);
            Require(health.CurrentHealth == 1, "Vulnerabilidad no multiplicó el daño.");
            Vector2 vulnerablePosition = enemy.transform.position;
            FreezePlayer(player, new Vector2(-10f, -1.5f));
            yield return Wait(0.5f);
            Require(Vector2.Distance(vulnerablePosition, enemy.transform.position) < 0.04f && !hitbox.IsActive,
                "El enemigo se movió o atacó vulnerable.");
            yield return Until(() => !posture.IsVulnerable, 2f);
            yield return Until(() => posture.CurrentPosture > 0, 2f);
            Require(!health.IsDead, "Recuperación de postura alteró la vida.");
            yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Patrol, 2f);
            FreezePlayer(player, (Vector2)enemy.transform.position + Vector2.right);
            yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Telegraph, 2f);
            int deaths = 0;
            health.Died += () => deaths++;
            HitWithPlayerQuery(player, enemy);
            Require(health.IsDead && !health.CanReceiveDamage && deaths == 1 &&
                enemy.State == GroundEnemyController.EnemyState.Dead && !hitbox.IsActive, "Muerte no detuvo combate.");
            foreach (Collider2D collider in enemy.GetComponentsInChildren<Collider2D>()) Require(!collider.enabled, "Collider activo tras muerte.");
            health.TakeDamage(99);
            yield return Wait(2f);
            Require(deaths == 1 && !hitbox.IsActive && !enemy.GetComponent<Rigidbody2D>().simulated, "El enemigo muerto se reactivó.");
            Debug.Log("TASK09_PASS: ataque del jugador, tres parries, vulnerabilidad x2, recuperación y muerte durante telegraph.");

            FreezePlayer(player, new Vector2(-10f, -1.5f));
            GameObject platform = new GameObject("Validation Ledge", typeof(BoxCollider2D));
            platform.transform.position = new Vector3(100f, 0f);
            platform.GetComponent<BoxCollider2D>().size = new Vector2(3f, 0.5f);
            GameObject fixture = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(GroundEnemySceneSetup.PrefabPath),
                new Vector3(100f, 0.77f), Quaternion.identity);
            GroundEnemyController fixtureController = fixture.GetComponent<GroundEnemyController>();
            SetField(fixtureController, "patrolOffsets", new Vector2(-10f, 10f));
            float minX = 100f, maxX = 100f;
            until = Time.time + 5f;
            while (Time.time < until)
            {
                minX = Mathf.Min(minX, fixture.transform.position.x);
                maxX = Mathf.Max(maxX, fixture.transform.position.x);
                Require(fixture.transform.position.y > 0.65f, "El enemigo cayó del borde.");
                yield return null;
            }
            Require(minX < 99.7f && maxX > 100.3f, "No recorrió ambos lados de la plataforma.");
            GameObject wall = new GameObject("Validation Wall", typeof(BoxCollider2D));
            wall.transform.position = new Vector3(100.8f, 1f);
            wall.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 2f);
            PlaceEnemy(fixtureController, new Vector2(100f, 0.77f));
            yield return Wait(2f);
            Require(fixture.transform.position.x < 100.31f && fixture.transform.position.y > 0.65f, "No reaccionó ante pared.");
            Object.Destroy(fixture);
            Object.Destroy(platform);
            Object.Destroy(wall);
            Debug.Log("TASK09_PASS: bordes y obstáculos sin navegación ni cambios al jugador.");
        }

        private static IEnumerator ValidateCollisionAndRecovery(PlayerHealth player, GroundEnemyController enemy)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
            Rigidbody2D enemyBody = enemy.GetComponent<Rigidbody2D>();
            BoxCollider2D enemyCollider = enemy.GetComponent<BoxCollider2D>();
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            PlayerMovementData movement = (PlayerMovementData)GetField(controller, "movementData");
            RigidbodyConstraints2D constraints = playerBody.constraints;
            Vector2 patrolOffsets = (Vector2)GetField(enemy, "patrolOffsets");
            InputSettings.BackgroundBehavior backgroundBehavior = InputSystem.settings.backgroundBehavior;
            InputSettings.EditorInputBehaviorInPlayMode editorInputBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>("Task09ValidationKeyboard");
            try
            {
                Require(!enemyCollider.isTrigger && enemyBody.bodyType == RigidbodyType2D.Dynamic,
                    "Se modificó el contrato sólido/dinámico del enemigo.");
                Require(Physics2D.GetIgnoreCollision(enemyCollider, playerCollider), "Los cuerpos todavía pueden empujarse.");
                Collider2D ground = GameObject.Find("Ground - Far Side").GetComponent<Collider2D>();
                Require(!Physics2D.GetIgnoreCollision(enemyCollider, ground) && !Physics2D.GetIgnoreCollision(playerCollider, ground),
                    "Se perdieron contactos con el terreno.");
                enemy.enabled = false;
                PlaceEnemy(enemy, new Vector2(4.5f, -1.6f));
                yield return Wait(0.1f);
                float stationaryX = enemyBody.position.x;

                // Actual Move/Dash actions and the unchanged player controller drive
                // the body through the enemy in both directions, with physics enabled.
                foreach (int direction in new[] { 1, -1 })
                {
                    foreach (bool dash in new[] { false, true })
                    {
                        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                        player.RestartForTesting();
                        playerBody.constraints = constraints;
                        playerBody.position = new Vector2(4.5f - direction * 1.2f, -1.55f);
                        player.transform.position = playerBody.position;
                        Physics2D.SyncTransforms();
                        Key moveKey = direction > 0 ? Key.D : Key.A;
                        InputSystem.QueueStateEvent(keyboard, dash ? new KeyboardState(moveKey, Key.LeftShift) : new KeyboardState(moveKey));
                        if (dash) yield return Until(() => controller.IsDashing, 0.5f);
                        float maxSpeed = 0f;
                        float end = Time.time + (dash ? 0.3f : 0.45f);
                        while (Time.time < end)
                        {
                            maxSpeed = Mathf.Max(maxSpeed, Mathf.Abs(playerBody.linearVelocity.x));
                            Require(Mathf.Abs(enemyBody.position.x - stationaryX) < 0.03f,
                                $"{(dash ? "Dash" : "Caminar")} empujó al enemigo: x={enemyBody.position.x}.");
                            yield return null;
                        }
                        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                        yield return Wait(0.06f);
                        Require(direction * (playerBody.position.x - stationaryX) > 0.25f,
                            $"El jugador no pudo cruzar el cuerpo enemigo: dash={dash}, direction={direction}, position={playerBody.position}, maxSpeed={maxSpeed}, input={((InputAction)GetField(controller, "moveAction")).ReadValue<Vector2>()}, enabled={controller.enabled}, locked={controller.IsControlLocked}, keyboard={keyboard[moveKey].isPressed}, focus={Application.isFocused}.");
                        Require(playerBody.position.y > -1.65f, "El jugador atravesó el suelo.");
                        if (dash) Require(Mathf.Abs(maxSpeed - movement.DashSpeed) < 0.05f, "Cambió la velocidad del dash.");
                        Require(player.CurrentHealth == 5, "El mero contacto infligió daño.");
                    }
                }

                FreezePlayer(player, new Vector2(enemyBody.position.x,
                    enemyCollider.bounds.max.y + playerCollider.bounds.extents.y + 0.04f));
                bool groundedOnEnemy = (bool)typeof(PlayerController).GetMethod("CheckGrounded", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(controller, null);
                Require(!groundedOnEnemy, "El cuerpo que se atraviesa contó como suelo del jugador.");
                FreezePlayer(player, new Vector2(-10f, -1.5f));
                enemy.gameObject.SetActive(false);
                enemy.gameObject.SetActive(true);
                enemy.enabled = true;
                yield return Wait(0.04f);
                Require(Physics2D.GetIgnoreCollision(enemyCollider, playerCollider), "Reactivar el enemigo restauró el empuje.");
                Debug.Log("TASK09_COLLISION_PASS: caminar/dash reales en ambos sentidos sin empuje; terreno, velocidad, suelo y reactivación conservados.");

                // Reproduce an unexpected displacement independently of the fix
                // preventing player impulses. The old leash rejected both directions.
                PlaceEnemy(enemy, new Vector2(7.35f, -1.6f));
                yield return Until(() => enemyBody.position.x < 7.15f, 0.6f);
                yield return Until(() => enemyBody.position.x <= enemy.RightLimit, 1f);
                PlaceEnemy(enemy, new Vector2(0.95f, -1.6f));
                yield return Until(() => enemyBody.position.x > 1.15f, 0.6f);
                yield return Until(() => enemyBody.position.x >= enemy.LeftLimit, 1f);

                // Put the patrol boundary behind the real wall, so it is the wall
                // sensor (not the leash) that must interrupt this pursuit.
                SetField(enemy, "patrolOffsets", new Vector2(patrolOffsets.x, 3.2f));
                PlaceEnemy(enemy, new Vector2(7.25f, -1.6f));
                FreezePlayer(player, new Vector2(8.95f, -1.55f));
                yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Chase, 1f);
                yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Patrol && enemyBody.position.x < 7f, 1.5f);
                Require(enemyBody.position.y > -1.65f, "La retirada de la pared dejó caer al enemigo.");
                FreezePlayer(player, new Vector2(-10f, -1.5f));
                SetField(enemy, "patrolOffsets", patrolOffsets);
                yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Patrol, 2f);

                PlaceEnemy(enemy, new Vector2(4.5f, -1.6f));
                FreezePlayer(player, new Vector2(5.6f, -1.55f));
                yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Telegraph, 3f);
                PlaceEnemy(enemy, new Vector2(7.35f, -1.6f));
                FreezePlayer(player, new Vector2(-10f, -1.5f));
                yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Attack, 1f);
                yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Recovery, 0.5f);
                yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Patrol && enemyBody.position.x <= enemy.RightLimit, 3f);
                Debug.Log("TASK09_RECOVERY_PASS: regresa desde ambos límites, abandona persecución bloqueada y se recupera tras Telegraph/Attack/Recovery.");
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard);
                InputSystem.settings.backgroundBehavior = backgroundBehavior;
                InputSystem.settings.editorInputBehaviorInPlayMode = editorInputBehavior;
                SetField(enemy, "patrolOffsets", patrolOffsets);
                playerBody.constraints = constraints;
                player.RestartForTesting();
                FreezePlayer(player, new Vector2(-10f, -1.5f));
            }
        }

        private static void HitWithPlayerQuery(PlayerHealth player, GroundEnemyController enemy)
        {
            FreezePlayer(player, (Vector2)enemy.transform.position + Vector2.left * 0.9f);
            player.GetComponent<SpriteRenderer>().flipX = false;
            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            Invoke(combat, "EndAttack");
            SetField(combat, "facingDirection", 1);
            SetEnumField(combat, "currentAttack", "Horizontal");
            Invoke(combat, "ApplyHitbox");
            Invoke(combat, "EndAttack");
        }

        private static void FreezePlayer(PlayerHealth player, Vector2 position)
        {
            player.GetComponent<PlayerController>().enabled = false;
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeAll;
            body.position = position;
            player.transform.position = position;
            body.linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms();
        }

        private static void PlaceEnemy(GroundEnemyController enemy, Vector2 position)
        {
            enemy.GetComponent<Rigidbody2D>().position = position;
            enemy.transform.position = position;
            enemy.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms();
        }

        private static IEnumerator Wait(float seconds)
        {
            float end = Time.time + seconds;
            while (Time.time < end) yield return null;
        }

        private static IEnumerator Until(Func<bool> condition, float timeout)
        {
            float end = Time.time + timeout;
            while (!condition())
            {
                if (Time.time > end) throw new TimeoutException("No se alcanzó la condición esperada de la prueba.");
                yield return null;
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static FieldInfo Field(object target, string name) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(target.GetType().Name, name);
        private static object GetField(object target, string name) => Field(target, name).GetValue(target);
        private static void SetField(object target, string name, object value) => Field(target, name).SetValue(target, value);
        private static void SetEnumField(object target, string name, string value) => SetField(target, name, Enum.Parse(Field(target, name).FieldType, value));
        private static void Invoke(object target, string name) => target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
    }
}
