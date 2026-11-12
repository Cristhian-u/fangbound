using System;
using System.Collections;
using System.Linq;
using System.Reflection;
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
    public static partial class B2EncounterValidation
    {
        private static Keyboard keyboard;
        private static Gamepad gamepad;
        private static InputSettings.BackgroundBehavior oldBackground;
        private static InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
        private static RoomScene Room => RoomSession.Instance?.CurrentRoom;
        private static PlayerHealth Player => Room.Player;
        private static PlayerController Controller => Player.GetComponent<PlayerController>();
        private static Rigidbody2D Body => Player.GetComponent<Rigidbody2D>();

        [MenuItem("Fangbound/Validate B2 Encounter and Discovery T26 in Play Mode")]
        public static void Run()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode && !Enumerable.Range(0,
                UnityEngine.SceneManagement.SceneManager.sceneCount).Any(i =>
                UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty), "Guardar escenas y salir de Play.");
            EditorSceneManager.OpenScene(InitialRegionGeographySetup.B2ScenePath);
            ValidateAssets(Object.FindAnyObjectByType<RoomScene>());
            SessionState.SetBool(CombatMode, true); SessionState.SetBool(Running, true);
            Steps.Clear(); hadError = false;
            EditorApplication.EnterPlaymode();
        }

        internal static void ValidateAssets(RoomScene room)
        {
            GroundEnemyController[] enemies = Object.FindObjectsByType<GroundEnemyController>();
            LookoutDiscovery[] discoveries = Object.FindObjectsByType<LookoutDiscovery>();
            Require(room.Definition.RoomId == "B2" && enemies.Length == 1 && discoveries.Length == 1,
                "B2 debe tener un enemigo y un descubrimiento.");
            GroundEnemyController enemy = enemies[0];
            GroundEnemyData data = AssetDatabase.LoadAssetAtPath<GroundEnemyData>(B2EncounterSceneSetup.DataPath);
            var controller = new SerializedObject(enemy);
            var health = new SerializedObject(enemy.GetComponent<EnemyHealth>());
            Require(PrefabUtility.IsPartOfPrefabInstance(enemy) &&
                controller.FindProperty("enemyData").objectReferenceValue == data &&
                health.FindProperty("enemyData").objectReferenceValue == data &&
                controller.FindProperty("target").objectReferenceValue == room.Player &&
                controller.FindProperty("patrolOffsets").vector2Value == new Vector2(-1.2f, 1.2f) &&
                Vector2.Distance(enemy.transform.position, new Vector2(7.4f, 1.72f)) < .01f,
                "El encuentro no conserva prefab, perfil local, límites o posición.");
            GroundEnemyData baseline = AssetDatabase.LoadAssetAtPath<GroundEnemyData>(GroundEnemySceneSetup.DataPath);
            // Compare every serialized parameter, excluding the one intended tuning change.
            var values = new SerializedObject(data).GetIterator();
            while (values.NextVisible(true))
            {
                if (values.propertyPath.StartsWith("m_") || values.propertyPath == "recoveryDuration") continue;
                var previous = new SerializedObject(baseline).FindProperty(values.propertyPath);
                Require(previous != null && SerializedProperty.DataEquals(values, previous),
                    "El perfil B2 altera un parámetro no autorizado: " + values.propertyPath);
            }
            Require(Mathf.Abs(data.RecoveryDuration - .75f) < .001f &&
                Mathf.Abs(baseline.RecoveryDuration - 1.3f) < .001f &&
                enemy.GetComponent<PogoTarget>() != null && enemy.GetComponent<Collider2D>().isTrigger == false &&
                enemy.gameObject.layer == LayerMask.NameToLayer("EnemyBody"), "Perfil o contratos del enemigo inválidos.");
            Require(Vector2.Distance(discoveries[0].transform.position, new Vector2(13.2f, 5.1f)) < .01f &&
                discoveries[0].GetComponentsInChildren<Collider2D>(true).Length == 0 &&
                Object.FindObjectsByType<MemoryFlame>().Length == 0,
                "Descubrimiento desplazado, con colisión o confundido con un checkpoint.");
            Debug.Log("TASK26_ASSETS_PASS: enemigo prefab/perfil local, todos los otros parámetros iguales, descubrimiento sin colisiones y sin Llama nueva.");
        }

        private static IEnumerator ValidateCombat()
        {
            yield return Wait(.4f);
            oldBackground = InputSystem.settings.backgroundBehavior;
            oldEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>("T26Keyboard");
            gamepad = InputSystem.AddDevice<Gamepad>("T26Gamepad");
            AssertState("B2", 5);
            GroundEnemyController enemy = Object.FindAnyObjectByType<GroundEnemyController>();
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            EnemyPosture posture = enemy.GetComponent<EnemyPosture>();
            LookoutDiscovery discovery = Object.FindAnyObjectByType<LookoutDiscovery>();
            Require(Physics2D.GetIgnoreCollision(Player.GetComponent<Collider2D>(), enemy.GetComponent<Collider2D>()),
                "Se perdió la exclusión de empuje entre cuerpos.");
            yield return Press(Key.E);
            Require(!discovery.IsExamined && !discovery.IsMessageVisible, "Se examinó desde la entrada, fuera de alcance.");
            bool left = false, right = false;
            Rigidbody2D enemyBody = enemy.GetComponent<Rigidbody2D>();
            float previous = enemyBody.position.x;
            float until = Time.time + 5;
            double end = EditorApplication.timeSinceStartup + 12;
            while (Time.time < until)
            {
                Require(EditorApplication.timeSinceStartup < end, "La física no avanzó durante la patrulla.");
                float x = enemyBody.position.x;
                left |= x < previous - .001f; right |= x > previous + .001f;
                Require(enemy.State == GroundEnemyController.EnemyState.Patrol &&
                    x >= enemy.LeftLimit - .03f && x <= enemy.RightLimit + .03f, "Patrulla B2 o separación de entrada incorrecta.");
                previous = x; yield return null;
            }
            Require(left && right, $"No patrulló en ambos sentidos: left={left}, right={right}, pose={enemyBody.position}, renderPose={enemy.transform.position}, velocity={enemyBody.linearVelocity}, bounds={enemy.GetComponent<Collider2D>().bounds}, enabled={enemy.enabled}; {ProbeDescription(enemy)}.");
            Debug.Log("TASK26_PATROL_PASS: patrulla acotada y entrada sin agresión inmediata.");
            yield return Walk(3.2f);
            yield return Jump(5.25f, 1.2f);
            EnemyAttackHitbox attack = enemy.GetComponentInChildren<EnemyAttackHitbox>();
            // Separate development-unlock fixture: the same committed telegraph
            // must remain dodgeable. No player parameters or progression are saved.
            yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Telegraph, 4);
            Player.GetComponent<PlayerAbilities>().SetDashUnlocked(true);
            Controller.SetFacingDirection(-1);
            float dashStart = Body.position.x;
            Keys(Key.LeftShift);
            yield return Until(() => Controller.IsDashing, .5f);
            Keys();
            yield return Until(() => !Controller.IsDashing, 1);
            Require(Player.CurrentHealth == 5 && Body.position.x < dashStart - 2 &&
                Physics2D.GetIgnoreCollision(Player.GetComponent<Collider2D>(), enemy.GetComponent<Collider2D>()),
                $"El dash no evitó el ataque o cambió la exclusión de empuje: room={Room.Definition.RoomId}, HP={Player.CurrentHealth}, start={dashStart}, end={Body.position.x}, velocity={Body.linearVelocity}.");
            // Existing post-dash deceleration may carry this western retreat through
            // the open P02. Preserve it and physically return, instead of changing dash.
            yield return Wait(.7f);
            if (Room.Definition.RoomId == "B")
            {
                Keys(Key.D);
                yield return Until(() => Room.Definition.RoomId == "B2" && !RoomSession.Instance.IsLoading, 12);
                Keys(); yield return Wait(.4f);
            }
            Require(Room.Definition.RoomId == "B2" && Player.CurrentHealth == 5, "La retirada perdió vida o bloqueó P02.");
            Player.GetComponent<PlayerAbilities>().SetDashUnlocked(false);
            enemy = Object.FindAnyObjectByType<GroundEnemyController>();
            health = enemy.GetComponent<EnemyHealth>(); posture = enemy.GetComponent<EnemyPosture>();
            attack = enemy.GetComponentInChildren<EnemyAttackHitbox>();
            discovery = Object.FindAnyObjectByType<LookoutDiscovery>();
            Debug.Log("TASK26_DASH_EVADE_PASS: desbloqueo temporal de prueba, dash real evita el ataque comprometido; sin alterar datos del jugador.");
            yield return Walk(3.2f);
            yield return Jump(5.25f, 1.2f);
            for (int i = 0; i < 3; i++)
            {
                yield return Until(() => enemy.State == GroundEnemyController.EnemyState.Telegraph && Remaining(enemy) < .09f, 4);
                Require(attack.GetComponent<SpriteRenderer>().enabled, "Falta anticipación visible.");
                int before = posture.CurrentPosture;
                Keys(Key.K);
                yield return Until(() => posture.CurrentPosture < before, 1);
                Keys();
                Require(Player.CurrentHealth == 5, "El parry correcto recibió daño.");
                yield return Wait(.15f);
            }
            Require(posture.IsVulnerable && enemy.State == GroundEnemyController.EnemyState.Vulnerable &&
                Mathf.Abs(enemy.GetComponent<Rigidbody2D>().linearVelocity.x) < .01f,
                "La ruptura no aturde al enemigo de B2.");
            Debug.Log("TASK26_PARRY_PASS: tres parries por K, postura rota y aturdimiento real; ventana/recovery sin cambios.");

            Keys(Key.J);
            yield return Until(() => health.CurrentHealth == 2, .7f);
            Keys();
            yield return Wait(.3f);
            Require(!health.IsDead, "El golpe vulnerable produjo instakill.");
            // Jump on the open west side of the canopy: no fixture teleport or extra impulse.
            Keys(Key.Space, Key.D);
            yield return Until(() => Body.position.x > 5.83f, 1);
            Keys(Key.Space);
            yield return Until(() => Body.linearVelocity.y < -.1f && Body.position.y < 3.25f, 2);
            Keys(Key.S, Key.J);
            int beforePogo = health.CurrentHealth;
            yield return Until(() => health.CurrentHealth < beforePogo, .8f);
            Require(Body.linearVelocity.y > 0, "El pogo B2 no rebotó.");
            Keys();
            yield return Until(() => Controller.IsGrounded, 3);
            if (!health.IsDead)
            {
                Controller.SetFacingDirection(1);
                Keys(Key.J);
                yield return Until(() => health.IsDead, .8f);
                Keys();
            }
            Require(!enemy.GetComponent<Rigidbody2D>().simulated && !attack.IsActive, "Muerte B2 incompleta.");
            Debug.Log("TASK26_COMBAT_PASS: daño horizontal vulnerable, pogo con salto/gravedad reales y muerte; sin impulso nuevo.");

            yield return Walk(5.2f);
            yield return Jump(7.2f, 3);
            yield return Walk(9);
            yield return Jump(12.3f, 4.8f);
            yield return Walk(13.2f);
            int hp = Player.CurrentHealth;
            yield return Press(Key.E);
            Require(discovery.IsExamined && discovery.IsMessageVisible && Player.CurrentHealth == hp &&
                !Player.GetComponent<PlayerAbilities>().DashUnlocked &&
                !Player.GetComponent<PlayerCheckpoint>().Location.IsValid,
                "Examinar no mostró texto o alteró estadísticas/checkpoint.");
            yield return Wait(4.1f);
            Require(!discovery.IsMessageVisible, "El mensaje no finalizó.");
            InputSystem.QueueStateEvent(gamepad, new GamepadState().WithButton(GamepadButton.North));
            yield return Until(() => discovery.IsMessageVisible, 1);
            yield return Wait(4.1f);
            Require(!discovery.IsMessageVisible, "Mantener norte repite la interacción sin nueva pulsación.");
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            Debug.Log("TASK26_DISCOVERY_PASS: ruta superior física, brote visible, E/norte, texto temporal/repetible sin efectos de progresión.");

            // Return via the existing western drop and hollow, not into its east wall.
            yield return Walk(10.3f);
            yield return Until(() => Controller.IsGrounded && Body.position.y < 1, 3);
            yield return Walk(11.6f);
            yield return Until(() => Controller.IsGrounded, 3);
            yield return Jump(13.3f, 0);
            Require(Player.TryTakeDamage(1, (Vector2)Player.transform.position + Vector2.left), "No se pudo preparar vida 4/5.");
            yield return Until(() => !Player.IsInvulnerable && !Controller.IsControlLocked, 3);
            Keys(Key.D);
            yield return Until(() => Room.Definition.RoomId == "B3" && !RoomSession.Instance.IsLoading, 12);
            Keys(); yield return Wait(.4f); AssertState("B3", 4);
            Keys(Key.A);
            yield return Until(() => Room.Definition.RoomId == "B2" && !RoomSession.Instance.IsLoading, 12);
            Keys(); yield return Wait(.4f); AssertState("B2", 4);
            Require(Object.FindObjectsByType<LookoutDiscovery>().Length == 1 &&
                !Object.FindAnyObjectByType<LookoutDiscovery>().IsExamined &&
                Object.FindObjectsByType<GroundEnemyController>().Length == 1 &&
                !Object.FindAnyObjectByType<EnemyHealth>().IsDead,
                "La recarga duplicó contenido o cambió su estado local inicial.");
            Debug.Log("TASK26_P03_STATE_PASS: B2→B3→B2 físico con contenido activo, 4/5 y arma; enemigo/observación se reinician al recargar sala.");

            // Explicit transfer/pose fixtures isolate checkpoint state from another combat attempt.
            yield return Transfer("B2-to-B", "B");
            Place(new Vector2(6.2f, .58f));
            yield return Wait(.2f);
            yield return Press(Key.E);
            Require(Player.CurrentHealth == 5 && Player.GetComponent<PlayerCheckpoint>().Location.IsValid,
                "La Llama no curó/activó tras el descubrimiento.");
            Require(Player.TryTakeDamage(1, (Vector2)Player.transform.position + Vector2.right), "No se pudo preparar retorno.");
            yield return Until(() => !Player.IsInvulnerable && !Controller.IsControlLocked, 3);
            yield return Transfer("B-to-B2", "B2");
            AssertState("B2", 4);
            Require(Player.TryTakeDamage(Player.MaximumHealth, (Vector2)Player.transform.position + Vector2.right), "No se pudo probar muerte B2.");
            yield return Press(Key.R);
            yield return Until(() => Room.Definition.RoomId == "B" && !RoomSession.Instance.IsLoading && !Player.IsDead, 12);
            yield return Wait(.3f); AssertState("B", 5);
            Require(Player.GetComponent<PlayerCheckpoint>().CurrentCheckpoint != null, "Se perdió la Llama tras R.");
            Debug.Log("TASK26_CHECKPOINT_PASS: fixtures de transferencia, E real cura/activa; muerte B2 y R a Llama B con 5/5 y Colmillo.");
        }

        private static float Remaining(GroundEnemyController enemy) =>
            (float)typeof(GroundEnemyController).GetField("stateRemaining", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(enemy);

        private static string ProbeDescription(GroundEnemyController enemy)
        {
            Bounds bounds = enemy.GetComponent<Collider2D>().bounds;
            var filter = new ContactFilter2D(); filter.SetLayerMask(1); filter.useTriggers = false;
            var results = new Collider2D[32];
            string info = "";
            foreach (int direction in new[] { -1, 1 })
            {
                float x = bounds.center.x + direction * (bounds.extents.x + 1.3f * Time.fixedDeltaTime + .05f);
                foreach (bool wall in new[] { false, true })
                {
                    Vector2 center = new Vector2(x, wall ? bounds.center.y : bounds.min.y - .12f);
                    Vector2 size = wall ? new Vector2(.08f, bounds.size.y * .7f) : new Vector2(.08f, .3f);
                    int count = Physics2D.OverlapBox(center, size, 0, filter, results);
                    info += $" {direction}/{(wall ? "wall" : "floor")}=[{string.Join(",", results.Take(count).Select(c => c.name))}]";
                }
            }
            return info;
        }

        private static void AssertState(string id, int hp)
        {
            Require(Room.Definition.RoomId == id && Player.CurrentHealth == hp && !Player.IsDead &&
                Player.GetComponent<PlayerAbilities>().FangOwned &&
                Object.FindObjectsByType<PlayerHealth>().Length == 1 && Object.FindObjectsByType<Camera>().Length == 1 &&
                Object.FindObjectsByType<PlayerHealthUI>().Length == 1 && Object.FindObjectsByType<RoomSession>().Length == 1,
                "Estado o duplicados inesperados en " + id);
        }

        private static IEnumerator Transfer(string connection, string target)
        {
            RoomExit exit = Room.GetComponentsInChildren<RoomExit>().Single(e => e.ConnectionId == connection);
            Require(RoomSession.Instance.TryTravel(Player, exit.Destination, exit.DestinationEntranceId), "Falló fixture " + connection);
            yield return Until(() => Room.Definition.RoomId == target && !RoomSession.Instance.IsLoading, 12);
            yield return Wait(.4f);
        }

        private static void Place(Vector2 position)
        {
            Body.linearVelocity = Vector2.zero; Body.position = position; Player.transform.position = position;
            Physics2D.SyncTransforms();
        }

        private static IEnumerator Walk(float x)
        {
            double end = EditorApplication.timeSinceStartup + 5;
            while (Mathf.Abs(Body.position.x - x) > .1f || Mathf.Abs(Body.linearVelocity.x) > .2f)
            {
                float delta = x - Body.position.x, speed = Body.linearVelocity.x;
                Keys(Mathf.Abs(delta) < .1f || delta * speed > 0 && Mathf.Abs(delta) < speed * speed / 120 + .05f
                    ? Array.Empty<Key>() : new[] { delta > 0 ? Key.D : Key.A });
                Require(EditorApplication.timeSinceStartup < end, "Caminar bloqueado hacia " + x + "; pose=" + Body.position);
                yield return null;
            }
            Keys(); yield return Wait(.12f);
        }

        private static IEnumerator Jump(float x, float top)
        {
            yield return Until(() => Controller.IsGrounded, 3);
            Keys(x < Body.position.x ? new[] { Key.A, Key.Space } : new[] { Key.D, Key.Space });
            yield return Wait(.1f);
            double end = EditorApplication.timeSinceStartup + 4;
            while (!Controller.IsGrounded)
            {
                float delta = x - Body.position.x, speed = Body.linearVelocity.x;
                Keys(Mathf.Abs(delta) < .1f || delta * speed > 0 && Mathf.Abs(delta) < speed * speed / 120 + .05f
                    ? new[] { Key.Space } : new[] { delta > 0 ? Key.D : Key.A, Key.Space });
                Require(EditorApplication.timeSinceStartup < end, "Salto excedió tiempo."); yield return null;
            }
            Keys(); yield return Wait(.15f);
            Require(Mathf.Abs(Body.position.x - x) < .4f && Mathf.Abs(Body.position.y - (top + .58f)) < .18f,
                "No se alcanzó el apoyo " + new Vector2(x, top) + "; pose=" + Body.position);
        }

        private static IEnumerator Press(Key key)
        {
            Keys(key); yield return Wait(.08f); Keys(); yield return Wait(.12f);
        }

        private static IEnumerator Until(Func<bool> condition, float seconds)
        {
            double end = EditorApplication.timeSinceStartup + seconds;
            while (!condition()) { Require(EditorApplication.timeSinceStartup < end, "Tiempo agotado en prueba T26; pose=" + Body.position); yield return null; }
        }

        private static void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));

        private static void ReleaseInput()
        {
            if (keyboard == null) return;
            InputSystem.RemoveDevice(keyboard); keyboard = null;
            if (gamepad != null) { InputSystem.RemoveDevice(gamepad); gamepad = null; }
            InputSystem.settings.backgroundBehavior = oldBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = oldEditorInput;
        }
    }
}
