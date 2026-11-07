using System;
using System.Collections;
using System.Collections.Generic;
using Fangbound.CameraSystem;
using Fangbound.Combat;
using Fangbound.Enemies;
using Fangbound.Player;
using Fangbound.UI;
using Fangbound.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

namespace Fangbound.EditorTools
{
    // Drives the real dynamic body with keyboard events; no traversal teleports,
    // extra jump forces, time scaling or modifications to shared design assets.
    [InitializeOnLoad]
    public static class ArboledaPrototypeValidation
    {
        private const string RunningKey = "Fangbound.Task12.Validation";
        private static readonly Stack<IEnumerator> routines = new Stack<IEnumerator>();
        private static double startedAt;
        private static bool hadError;
        private static PlayerHealth player;
        private static PlayerController controller;
        private static Collider2D playerCollider;
        private static Rigidbody2D body;
        private static Collider2D roomBounds;
        private static PlayerMovementData movement;
        private static Camera camera;
        private static Keyboard keyboard;
        private static InputSettings.BackgroundBehavior oldBackground;
        private static InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
        private static bool oldDash, oldDouble;

        static ArboledaPrototypeValidation()
        {
            EditorApplication.update += Tick;
            Application.logMessageReceived += (message, trace, type) =>
            {
                if (SessionState.GetBool(RunningKey, false) &&
                    (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)) hadError = true;
            };
        }

        [MenuItem("Fangbound/Validate Arboleda Prototype in Play Mode")]
        public static void Run()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Ejecutar desde Edit Mode.");
            EditorSceneManager.OpenScene(ArboledaPrototypeSceneSetup.ScenePath, OpenSceneMode.Single);
            ValidateScene();
            hadError = false;
            SessionState.SetBool(RunningKey, true);
            EditorApplication.EnterPlaymode();
        }

        private static void ValidateScene()
        {
            Require(GameObject.Find("T13 Expansion") != null, "Falta la ampliación T13 guardada.");
            Require(Object.FindObjectsByType<PlayerHealth>().Length == 1, "Debe existir un jugador.");
            Require(Object.FindObjectsByType<GroundEnemyController>().Length == 1, "Debe existir un enemigo.");
            Require(Object.FindObjectsByType<MemoryFlame>().Length == 1, "Debe existir una llama.");
            foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) == 0, "Script ausente: " + child.name);
            CameraFollow2D follow = Object.FindAnyObjectByType<CameraFollow2D>();
            SerializedObject followData = new SerializedObject(follow);
            Require(followData.FindProperty("target").objectReferenceValue == Object.FindAnyObjectByType<PlayerHealth>().transform &&
                followData.FindProperty("roomBounds").objectReferenceValue != null, "Referencias de cámara inválidas.");
            SerializedObject checkpoint = new SerializedObject(Object.FindAnyObjectByType<PlayerCheckpoint>());
            Require(checkpoint.FindProperty("cameraFollow").objectReferenceValue == follow, "Cámara de respawn ausente.");
            Require(Object.FindAnyObjectByType<PlayerHealthUI>() != null && Object.FindAnyObjectByType<CombatHitstop>() != null, "Falta HUD/hitstop.");
            GroundEnemyController enemy = Object.FindAnyObjectByType<GroundEnemyController>();
            Require(PrefabUtility.IsPartOfPrefabInstance(enemy) &&
                new SerializedObject(enemy).FindProperty("target").objectReferenceValue == Object.FindAnyObjectByType<PlayerHealth>(), "Prefab/objetivo enemigo inválido.");
            Require(PrefabUtility.IsPartOfPrefabInstance(Object.FindAnyObjectByType<MemoryFlame>()), "Checkpoint debe reutilizar prefab.");
            foreach (Collider2D collider in GameObject.Find("Level Geometry").GetComponentsInChildren<Collider2D>())
                Require(!collider.isTrigger && collider.gameObject.layer == 0, "Geometría debe ser sólida en Default.");
            Require(GameObject.Find("Optional Upper Route - Four Platforms").GetComponentsInChildren<Collider2D>().Length == 4, "La ruta superior debe tener cuatro plataformas.");
            Require(GameObject.Find("Optional Lookout Route").GetComponentsInChildren<Collider2D>().Length == 7, "Mirador: tres apoyos de subida, captura y tres de regreso.");
            Require(Mathf.Abs(PlatformBounds("Safety Floor").size.x - 150f) < 0.01f, "Suelo continuo no cubre 150 m.");
            CombatFeedbackData feedback = AssetDatabase.LoadAssetAtPath<CombatFeedbackData>(CombatFeedbackSceneSetup.DataPath);
            Require(feedback.HitstopEnabled && Mathf.Approximately(feedback.NormalHitstop, 0.10f) &&
                Mathf.Approximately(feedback.PogoHitstop, 0.04f) && Mathf.Approximately(feedback.ParryHitstop, 0.07f), "Cambió el hitstop aprobado.");
            Debug.Log("TASK12_SCENE_PASS: referencias, prefabs, scripts, colisiones, HUD y hitstop aprobados.");
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
                Require(EditorApplication.timeSinceStartup - startedAt < 360d, "Validación excedió 360 s.");
                IEnumerator current = routines.Peek();
                if (!current.MoveNext())
                {
                    routines.Pop();
                    if (routines.Count == 0) Finish(!hadError);
                }
                else if (current.Current is IEnumerator nested) routines.Push(nested);
            }
            catch (Exception exception) { Debug.LogException(exception); Finish(false); }
        }

        private static void Finish(bool success)
        {
            SessionState.SetBool(RunningKey, false);
            routines.Clear();
            if (keyboard != null)
            {
                InputSystem.RemoveDevice(keyboard);
                keyboard = null;
                InputSystem.settings.backgroundBehavior = oldBackground;
                InputSystem.settings.editorInputBehaviorInPlayMode = oldEditorInput;
            }
            if (player != null)
            {
                player.GetComponent<PlayerAbilities>().SetDashUnlocked(oldDash);
                player.GetComponent<PlayerAbilities>().SetDoubleJumpUnlocked(oldDouble);
            }
            Debug.Log(success ? "TASK12_PLAYMODE_PASS" : "TASK12_PLAYMODE_FAIL");
            Debug.Log(success ? "TASK13_PLAYMODE_PASS" : "TASK13_PLAYMODE_FAIL");
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
            else EditorApplication.ExitPlaymode();
        }

        private static IEnumerator Validate()
        {
            player = Object.FindAnyObjectByType<PlayerHealth>();
            controller = player.GetComponent<PlayerController>();
            playerCollider = player.GetComponent<Collider2D>();
            body = player.GetComponent<Rigidbody2D>();
            movement = (PlayerMovementData)new SerializedObject(controller).FindProperty("movementData").objectReferenceValue;
            camera = Object.FindAnyObjectByType<CameraFollow2D>().GetComponent<Camera>();
            roomBounds = (Collider2D)new SerializedObject(camera.GetComponent<CameraFollow2D>()).FindProperty("roomBounds").objectReferenceValue;
            oldBackground = InputSystem.settings.backgroundBehavior;
            oldEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>("ArboledaValidationKeyboard");
            PlayerAbilities abilities = player.GetComponent<PlayerAbilities>();
            oldDash = abilities.DashUnlocked;
            oldDouble = abilities.DoubleJumpUnlocked;
            abilities.SetDashUnlocked(false);
            abilities.SetDoubleJumpUnlocked(false);
            GroundEnemyController enemy = Object.FindAnyObjectByType<GroundEnemyController>();
            enemy.enabled = false; // Isolate geometry; real AI is exercised below.
            yield return Wait(0.5f);
            Require(controller.IsGrounded && Mathf.Abs(playerCollider.bounds.min.y) < 0.1f, "Spawn sin suelo.");
            Require(Physics2D.GetIgnoreCollision(playerCollider, enemy.GetComponent<Collider2D>()), "Se perdió exclusión de empuje.");
            for (int route = 0; route < 3; route++)
            {
                if (route != 0)
                {
                    player.GetComponent<PlayerCheckpoint>().ClearCheckpoint();
                    player.RestartForTesting(); // Existing spawn, between independent route trials.
                    yield return Wait(0.5f);
                }
                yield return Traverse(route);
            }
            yield return ValidateEncounter(enemy);
        }

        private static IEnumerator Traverse(int variant)
        {
            double began = Time.realtimeSinceStartupAsDouble;
            var route = new List<string> { "Safety Floor", "A1", "A2", "B Mesa" };
            route.AddRange(variant == 1
                ? new[] { "Upper 1", "Upper 2", "Upper 3", "Upper Discovery", "Ruin Return 1", "Ruin Return 2" }
                : new[] { "B Lower 1", "B Lower 2", "B Return Step" });
            route.AddRange(new[] { "Grove Entry Terrace", "Grove Entry", "Grove East", "Grove Switchback", "Grove Upper Rest", "Grove Crossing" });
            if (variant == 2) route.AddRange(new[] { "Lookout 1", "Lookout 2", "Lookout Deck", "Lookout Return 1", "Lookout Return 2", "Lookout Return 3" });
            route.AddRange(new[] { "Passage Low Rest", "Passage 1", "Passage 2", "Passage Upper Rest", "Passage Descent 1",
                "Passage Descent 2", "Passage Descent 3", "Passage Descent 4", "C Arena", "D Step", "D Deck" });
            for (int i = 1; i < route.Count; i++)
            {
                Bounds source = PlatformBounds(route[i - 1]), target = PlatformBounds(route[i]);
                if (target.max.y < source.max.y - 0.1f && target.min.x <= source.max.x + 0.05f)
                    yield return DropTo(route[i - 1], route[i]);
                else if (Mathf.Abs(target.max.y - source.max.y) < 0.05f && target.min.x <= source.max.x + 0.05f)
                {
                    yield return WalkTo(target.min.x + 1.1f);
                    Require(controller.IsGrounded, "Unión sin apoyo: " + route[i]);
                }
                else yield return JumpTo(route[i - 1], route[i]);
                // Deliberate misses with real movement, then climb back without
                // teleporting, changing gravity, healing or using extra abilities.
                if (variant == 1 && route[i] == "Upper 2")
                    yield return MissAndRecover("Upper 2", "Ruin Catch");
                if (variant == 2 && route[i] == "Grove Switchback")
                {
                    yield return DropTo("Grove Switchback", "Grove Entry");
                    yield return JumpTo("Grove Entry", "Grove East");
                    yield return JumpTo("Grove East", "Grove Switchback");
                    Debug.Log("TASK13_RECOVERY_PASS: grove, caída de 3.3 m y retorno por dos saltos normales.");
                }
                if (variant == 2 && route[i] == "Lookout 2")
                    yield return MissAndRecover("Lookout 2", "Lookout Catch");
                if (variant == 2 && route[i] == "Passage 2")
                    yield return MissAndRecover("Passage 2", "Passage Catch");
                if (route[i] == "C Arena")
                {
                    yield return WalkTo(Object.FindAnyObjectByType<MemoryFlame>().transform.position.x - 0.4f);
                    yield return Press(Key.E);
                    PlayerCheckpoint checkpoint = player.GetComponent<PlayerCheckpoint>();
                    Require(checkpoint.CurrentCheckpoint != null && checkpoint.CurrentCheckpoint.IsCurrent &&
                        checkpoint.TryGetRespawnPose(out _, out _), "E no estableció un checkpoint seguro tras reconectar.");
                }
            }
            yield return WalkTo(GameObject.Find("End Label").transform.position.x);
            CheckCameraAndSafety();
            Require(player.CurrentHealth == player.MaximumHealth, "Hubo daño fuera del encuentro.");
            Debug.Log($"TASK12_ROUTE_PASS / TASK13_ROUTE_PASS: {new[] { "principal", "ruina y retorno", "mirador, retorno y recuperaciones" }[variant]}, solo A/D+Espacio; E en llama; final x={body.position.x:F2}; bot={Time.realtimeSinceStartupAsDouble - began:F1}s (no cronometraje humano).");
        }

        private static IEnumerator JumpTo(string from, string to)
        {
            Bounds source = PlatformBounds(from), target = PlatformBounds(to);
            int direction = target.center.x >= body.position.x ? 1 : -1;
            float takeoff = direction > 0 ? Mathf.Min(source.max.x - 0.65f, target.min.x - 0.9f)
                : Mathf.Max(source.min.x + 0.65f, target.max.x + 0.9f);
            yield return WalkTo(takeoff);
            Require(controller.IsGrounded, "Despegue no apoyado: " + from);
            float footStart = playerCollider.bounds.min.y;
            float peak = footStart;
            float landing = direction > 0 ? Mathf.Min(target.min.x + 1.1f, target.center.x)
                : Mathf.Max(target.max.x - 1.1f, target.center.x);
            Keys(direction > 0 ? Key.D : Key.A, Key.Space);
            double end = Time.realtimeSinceStartupAsDouble + 4d;
            bool airborne = false, stopped = false;
            do
            {
                yield return null;
                CheckCameraAndSafety();
                peak = Mathf.Max(peak, playerCollider.bounds.min.y);
                airborne |= !controller.IsGrounded;
                if (!stopped && direction * (body.position.x - landing) >= 0f)
                {
                    Keys(Key.Space);
                    stopped = true;
                }
                Require(Time.realtimeSinceStartupAsDouble < end, $"Salto {from}->{to} sin aterrizar; pos={player.transform.position}.");
            } while (!airborne || !controller.IsGrounded);
            Keys();
            yield return Wait(0.12f);
            Require(Mathf.Abs(playerCollider.bounds.min.y - target.max.y) < 0.12f &&
                player.transform.position.x > target.min.x && player.transform.position.x < target.max.x,
                $"Salto {from}->{to} cayó fuera de destino; pos={player.transform.position}, pie={playerCollider.bounds.min.y:F2}, destino={target}.");
            Debug.Log($"TASK12_JUMP_PASS: {from}->{to}, subida={target.max.y - source.max.y:F2}, hueco={Mathf.Max(0, target.min.x - source.max.x):F2}, altura real={peak - footStart:F2}.");
        }

        private static IEnumerator WalkTo(float x)
        {
            double end = Time.realtimeSinceStartupAsDouble + 12d;
            // Brake before the target using the unchanged deceleration value.
            // This also keeps setup walks on the narrower recovery supports.
            float deceleration = movement.Deceleration;
            while (Mathf.Abs(body.position.x - x) > 0.1f || Mathf.Abs(body.linearVelocity.x) > 0.2f)
            {
                float delta = x - body.position.x, speed = body.linearVelocity.x;
                float brake = speed * speed / (2f * deceleration) + 0.05f;
                if (Mathf.Abs(delta) < 0.1f || (delta * speed > 0 && Mathf.Abs(delta) < brake)) Keys();
                else Keys(delta > 0 ? Key.D : Key.A);
                CheckCameraAndSafety();
                Require(Time.realtimeSinceStartupAsDouble < end, $"Caminar hacia {x:F2} bloqueado en {player.transform.position}.");
                yield return null;
            }
            Keys();
            yield return Wait(0.16f);
        }

        private static IEnumerator DropTo(string from, string to)
        {
            Bounds source = PlatformBounds(from), target = PlatformBounds(to);
            float landing = target.min.x < source.max.x
                ? Mathf.Min(source.max.x + 0.7f, target.max.x - 0.4f)
                : Mathf.Min(target.min.x + 1.1f, target.center.x);
            yield return WalkTo(source.max.x - 0.6f);
            bool airborne = false, stopped = false;
            double end = Time.realtimeSinceStartupAsDouble + 4d;
            Keys(Key.D);
            do
            {
                yield return null;
                CheckCameraAndSafety();
                airborne |= !controller.IsGrounded;
                float brake = body.linearVelocity.x * body.linearVelocity.x / (2f * movement.Deceleration) + 0.03f;
                if (!stopped && body.position.x >= landing - brake) { Keys(); stopped = true; }
                Require(Time.realtimeSinceStartupAsDouble < end, $"Descenso {from}->{to} bloqueado: {body.position}.");
            } while (!airborne || !controller.IsGrounded);
            Keys();
            yield return Wait(0.16f);
            Require(Mathf.Abs(playerCollider.bounds.min.y - target.max.y) < 0.12f &&
                body.position.x > target.min.x && body.position.x < target.max.x,
                $"Descenso {from}->{to} fuera del apoyo: {body.position}, pie={playerCollider.bounds.min.y:F2}.");
            Require(player.CurrentHealth == player.MaximumHealth, "Caída produjo daño.");
            Debug.Log($"TASK13_DROP_PASS: {from}->{to}, caída natural de {source.max.y - target.max.y:F2} m, sin daño.");
        }

        private static IEnumerator MissAndRecover(string source, string catchPlatform)
        {
            yield return DropTo(source, catchPlatform);
            yield return JumpTo(catchPlatform, source);
            Debug.Log($"TASK13_RECOVERY_PASS: {source}->{catchPlatform}->{source}, controles reales, sin teletransporte ni daño.");
        }

        private static Bounds PlatformBounds(string name) => GameObject.Find(name).GetComponent<Collider2D>().bounds;

        private static IEnumerator ValidateEncounter(GroundEnemyController enemy)
        {
            PlayerCheckpoint checkpoint = player.GetComponent<PlayerCheckpoint>();
            Require(checkpoint.TryGetRespawnPose(out Vector3 respawn, out _), "Falta destino seguro del checkpoint.");
            player.RestartForTesting();
            yield return Wait(0.4f);
            Require(Vector2.Distance(player.transform.position, respawn) < 0.15f, "Respawn de llama fuera del suelo seguro.");
            enemy.enabled = true;
            float min = enemy.transform.position.x, max = min;
            double end = Time.realtimeSinceStartupAsDouble + 6d;
            while (Time.realtimeSinceStartupAsDouble < end)
            {
                min = Mathf.Min(min, enemy.transform.position.x);
                max = Mathf.Max(max, enemy.transform.position.x);
                Require(enemy.State == GroundEnemyController.EnemyState.Patrol &&
                    enemy.transform.position.x >= enemy.LeftLimit - 0.1f && enemy.transform.position.x <= enemy.RightLimit + 0.1f, "Patrulla de arena inválida.");
                yield return null;
            }
            Require(max - min > 2f, "Enemigo no patrulla en la arena.");
            Keys(Key.D);
            end = Time.realtimeSinceStartupAsDouble + 5d;
            bool chased = false;
            while (enemy.State != GroundEnemyController.EnemyState.Telegraph)
            {
                chased |= enemy.State == GroundEnemyController.EnemyState.Chase;
                Require(Time.realtimeSinceStartupAsDouble < end, "Enemigo no detectó/telegrafió.");
                yield return null;
            }
            Keys();
            yield return Wait(1f);
            Require(chased && player.CurrentHealth < player.MaximumHealth, "Persecución/ataque real no causó daño.");
            yield return WalkTo(checkpoint.CurrentCheckpoint.transform.position.x - 0.4f);
            yield return Press(Key.E);
            Require(player.CurrentHealth == player.MaximumHealth && checkpoint.CurrentCheckpoint.IsCurrent, "Llama no curó tras combate.");
            yield return Wait(0.9f);
            Require(player.TryTakeDamage(player.MaximumHealth, (Vector2)player.transform.position + Vector2.right) && player.IsDead, "Preparación de muerte rechazada.");
            yield return Press(Key.R);
            Require(!player.IsDead && player.CurrentHealth == player.MaximumHealth &&
                Vector2.Distance(player.transform.position, respawn) < 0.15f, "R no reapareció con vida completa.");
            CheckCameraAndSafety();
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            end = Time.realtimeSinceStartupAsDouble + 12d;
            while (!health.IsDead)
            {
                float delta = enemy.transform.position.x - player.transform.position.x;
                Key direction = delta >= 0 ? Key.D : Key.A;
                if (Mathf.Abs(delta) > 0.95f)
                {
                    Keys(direction);
                    yield return Wait(0.04f);
                }
                else
                {
                    Keys(direction, Key.J);
                    yield return Wait(0.04f);
                    Keys();
                    yield return Wait(0.22f);
                }
                Require(!player.IsDead && Time.realtimeSinceStartupAsDouble < end, "Combate normal no completado.");
            }
            Keys();
            yield return Wait(0.2f);
            Require(enemy.State == GroundEnemyController.EnemyState.Dead && !enemy.GetComponent<Rigidbody2D>().simulated,
                "Enemigo muerto conservó física/IA.");
            Debug.Log("TASK12_ENCOUNTER_PASS: patrulla, persecución, ataque real, curación E, muerte preparada por contrato, R seguro/cámara y muerte enemiga por J sin habilidades especiales.");
        }

        private static void CheckCameraAndSafety()
        {
            Bounds body = playerCollider.bounds;
            Require(body.min.y > PlatformBounds("Safety Floor").max.y - 0.2f &&
                body.min.x > roomBounds.bounds.min.x - 0.1f && body.max.x < roomBounds.bounds.max.x + 0.1f, "Jugador salió del volumen seguro.");
            Vector3 low = camera.WorldToViewportPoint(body.min);
            Vector3 high = camera.WorldToViewportPoint(body.max);
            Require(low.x >= -0.02f && low.y >= -0.02f && high.x <= 1.02f && high.y <= 1.02f, "Cámara dejó al jugador fuera de pantalla.");
        }

        private static void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        private static IEnumerator Press(Key key) { Keys(key); yield return Wait(0.08f); Keys(); yield return Wait(0.1f); }
        private static IEnumerator Wait(float seconds)
        {
            double end = Time.realtimeSinceStartupAsDouble + seconds;
            while (Time.realtimeSinceStartupAsDouble < end) yield return null;
        }
        private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    }
}
