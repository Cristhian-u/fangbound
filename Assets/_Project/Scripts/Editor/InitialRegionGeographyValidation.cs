using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fangbound.Combat;
using Fangbound.Player;
using Fangbound.UI;
using Fangbound.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Fangbound.EditorTools
{
    [InitializeOnLoad]
    public static class InitialRegionGeographyValidation
    {
        private const string Running = "Fangbound.T22.ValidationRunning";
        private const string Folder = RoomCircuitSceneSetup.SceneFolder;
        private static readonly string[] Names =
        {
            "Arboleda_A_ClaroDespertar", "Arboleda_B_ClaroSendas", "Arboleda_C_RaicesProfundas",
            "Arboleda_D_RuinasColmillo", "Arboleda_E_CopasVigia", "Arboleda_F_GrutaRetorno"
        };
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static Keyboard keyboard;
        private static InputSettings.BackgroundBehavior oldBackground;
        private static InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
        private static double started;
        private static bool hadError;
        private static bool expectUnlocked;

        private static RoomScene Room => RoomSession.Instance?.CurrentRoom;
        private static PlayerHealth Player => Room?.Player;
        private static Rigidbody2D Body => Player.GetComponent<Rigidbody2D>();
        private static PlayerController Controller => Player.GetComponent<PlayerController>();

        static InitialRegionGeographyValidation()
        {
            EditorApplication.update += Tick;
            Application.logMessageReceived += (_, stack, type) =>
            {
                if (SessionState.GetBool(Running, false) &&
                    (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) &&
                    stack.Contains("Fangbound."))
                    hadError = true;
            };
        }

        [MenuItem("Fangbound/Validate Initial Region Geography T22 in Play Mode")]
        public static void Run()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Iniciar en Edit Mode.");
            ValidateAssets();
            EditorSceneManager.OpenScene(Path(0), OpenSceneMode.Single);
            Steps.Clear();
            hadError = false;
            expectUnlocked = false;
            SessionState.SetBool(Running, true);
            EditorApplication.EnterPlaymode();
        }

        private static string Path(int index) => Folder + "/" + Names[index] + ".unity";

        private static void ValidateAssets()
        {
            var entrances = new Dictionary<string, HashSet<string>>();
            var destinations = new List<(string path, string entrance)>();
            for (int i = 0; i < Names.Length; i++)
            {
                Scene scene = EditorSceneManager.OpenScene(Path(i), OpenSceneMode.Single);
                RoomScene room = Object.FindAnyObjectByType<RoomScene>();
                Require(room != null && room.Definition != null && room.Definition.ScenePath == scene.path,
                    "RoomDefinition inválido: " + Names[i]);
                Require(Object.FindObjectsByType<PlayerHealth>().Length == 1 &&
                    Object.FindObjectsByType<Camera>().Length == 1 &&
                    Object.FindObjectsByType<PlayerHealthUI>().Length == 1 &&
                    Object.FindObjectsByType<CombatHitstop>().Length == 1,
                    "Rig duplicado/incompleto: " + Names[i]);
                RoomEntrance[] points = room.GetComponentsInChildren<RoomEntrance>();
                RoomExit[] exits = room.GetComponentsInChildren<RoomExit>();
                Require(exits.Length == new[] { 1, 3, 3, 2, 1, 2 }[i] &&
                    exits.Select(exit => exit.ConnectionId).Distinct().Count() == exits.Length,
                    "Salidas duplicadas/incompletas: " + Names[i]);
                entrances[scene.path] = new HashSet<string>(points.Select(point => point.EntranceId));
                Require(entrances[scene.path].Count == points.Length, "Entradas duplicadas: " + Names[i]);
                foreach (RoomEntrance point in points)
                {
                    Require(point.IsSafe(room.Player), "Entrada insegura: " + Names[i] + "/" + point.EntranceId);
                    Bounds player = room.Player.GetComponent<Collider2D>().bounds;
                    player.center += point.transform.position - room.Player.transform.position;
                    foreach (RoomExit exit in exits)
                        Require(!player.Intersects(exit.GetComponent<Collider2D>().bounds),
                            "Entrada superpuesta a trigger: " + Names[i] + "/" + point.EntranceId);
                }
                foreach (RoomExit exit in exits)
                {
                    Require(exit.Destination != null && !string.IsNullOrEmpty(exit.DestinationEntranceId) &&
                        exit.GetComponent<BoxCollider2D>().isTrigger,
                        "Salida sin destino o collider: " + exit.ConnectionId);
                    destinations.Add((exit.Destination.ScenePath, exit.DestinationEntranceId));
                }
                foreach (GameObject root in scene.GetRootGameObjects())
                    foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                        Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) == 0,
                            "Script faltante: " + Names[i]);

                if (i == 0) Require(GameObject.Find("Colmillo Pickup") != null,
                    "Falta el Colmillo en A.");
                if (i == 1)
                {
                    Require(Object.FindObjectsByType<MemoryFlame>().Length == 1 &&
                        GameObject.Find("T22 Root Descent") != null &&
                        GameObject.Find("T20 Awakening Connection") == null,
                        "B perdió Llama o pozo.");
                    Check("Exit B-to-A", 1.2f, 1);
                    Check("Exit B-to-C", 11, -3);
                    Check("Entry from-A", 3, .58f);
                    Check("Entry from-C", 12.3f, -3.42f);
                    CheckSurface("B Root Pit Floor", 9, 15, -4);
                    CheckSurface("B Root West Ledge", 9, 10.5f, -2);
                    CheckSurface("B Root East Ledge", 13, 15, -2);
                }
                if (i == 2)
                {
                    Require(GameObject.Find("T22 Upper Roots") != null, "Falta la abertura superior de C.");
                    Check("Exit C-to-B", 11, 13);
                    Check("Entry from-B", 13, 10.58f);
                    CheckSurface("C Arrival Landing", 11.5f, 14.5f, 10);
                    CheckSurface("C Return Step", 6.5f, 9.5f, 10);
                    CheckSurface("C Return Ledge", 9.5f, 12.5f, 12);
                }
            }
            foreach (var destination in destinations)
                Require(entrances.TryGetValue(destination.path, out HashSet<string> ids) && ids.Contains(destination.entrance),
                    "Destino sin entrada: " + destination.path + "/" + destination.entrance);
            Debug.Log("TASK22_ASSETS_PASS: seis escenas, 12 salidas, entradas seguras, pozo/repisa y referencias.");
        }

        private static void Check(string name, float x, float y)
        {
            GameObject item = GameObject.Find(name);
            Require(item != null && Vector2.Distance(item.transform.position, new Vector2(x, y)) < .02f,
                "Posición inesperada: " + name);
        }

        private static void CheckSurface(string name, float minX, float maxX, float top)
        {
            Collider2D collider = GameObject.Find(name)?.GetComponent<Collider2D>();
            Require(collider != null && Mathf.Abs(collider.bounds.min.x - minX) < .02f &&
                Mathf.Abs(collider.bounds.max.x - maxX) < .02f &&
                Mathf.Abs(collider.bounds.max.y - top) < .02f,
                "Apoyo inesperado: " + name);
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(Running, false) || !EditorApplication.isPlaying || EditorApplication.isPaused)
                return;
            try
            {
                if (Steps.Count == 0)
                {
                    started = EditorApplication.timeSinceStartup;
                    Steps.Push(ValidatePlay());
                }
                Require(EditorApplication.timeSinceStartup - started < 120, "T22 excedió 120 s.");
                IEnumerator current = Steps.Peek();
                if (!current.MoveNext())
                {
                    Steps.Pop();
                    if (Steps.Count == 0) Finish(!hadError);
                }
                else if (current.Current is IEnumerator nested) Steps.Push(nested);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Finish(false);
            }
        }

        private static void Finish(bool success)
        {
            Steps.Clear();
            SessionState.SetBool(Running, false);
            if (keyboard != null)
            {
                InputSystem.RemoveDevice(keyboard);
                keyboard = null;
                InputSystem.settings.backgroundBehavior = oldBackground;
                InputSystem.settings.editorInputBehaviorInPlayMode = oldEditorInput;
            }
            Debug.Log(success ? "TASK22_PLAYMODE_PASS" : "TASK22_PLAYMODE_FAIL");
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
            else EditorApplication.ExitPlaymode();
        }

        private static IEnumerator ValidatePlay()
        {
            yield return Wait(.5f);
            oldBackground = InputSystem.settings.backgroundBehavior;
            oldEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>("T22Keyboard");
            AssertRoom("A", 5, false);
            Require(!Player.GetComponent<PlayerAbilities>().DashUnlocked &&
                !Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked, "Habilidades iniciales cambiadas.");
            yield return Walk(8.2f);
            yield return Press(Key.E);
            Require(Player.GetComponent<PlayerAbilities>().FangOwned, "No se recogió el Colmillo.");
            yield return Walk(10.5f);
            Keys(Key.D, Key.Space);
            yield return Wait(.35f);
            Keys(Key.D);
            yield return UntilRoom("B", 8);
            Keys();
            yield return Wait(.35f);
            AssertRoom("B", 5, true);
            Require(Vector2.Distance(Player.transform.position, new Vector2(3, .58f)) < .15f,
                "A→B no llega al sendero occidental.");
            Debug.Log("TASK22_A_B_PASS: recogida y sendero horizontal con salto normal.");

            Keys(Key.A);
            yield return UntilRoom("A", 5);
            Keys();
            yield return Wait(.35f);
            AssertRoom("A", 5, true);
            Require(Vector2.Distance(Player.transform.position, new Vector2(16.5f, .58f)) < .15f,
                "B→A no llega al claro oriental.");
            Keys(Key.D);
            yield return UntilRoom("B", 5);
            Keys();
            yield return Wait(.35f);
            AssertRoom("B", 5, true);
            Debug.Log("TASK22_B_A_PASS: retorno horizontal y arma conservada.");

            yield return Walk(6.1f);
            yield return Press(Key.E);
            Require(Player.GetComponent<PlayerCheckpoint>().Location.IsValid,
                "La Llama de B no registró checkpoint.");
            Require(Player.TryTakeDamage(1, (Vector2)Player.transform.position + Vector2.right),
                "No se aplicó daño de prueba.");
            yield return Until(() => !Player.IsInvulnerable && !Controller.IsControlLocked, 2);
            Require(Player.CurrentHealth == 4, "La vida reducida cambió antes de viajar.");
            yield return Walk(7.1f);
            Keys(Key.D, Key.Space);
            yield return Until(() => Body.position.x >= 11.1f, 2);
            Keys();
            double end = Time.realtimeSinceStartupAsDouble + 7;
            while (Room.Definition.RoomId == "B" && !RoomSession.Instance.IsLoading)
            {
                Keys(Body.position.y < -2 && Body.position.x > 11.4f ? new[] { Key.A } : Array.Empty<Key>());
                Require(Time.realtimeSinceStartupAsDouble < end, "No se alcanzó el fondo del pozo B→C.");
                yield return null;
            }
            Keys();
            yield return UntilRoom("C", 12);
            AssertRoom("C", 4, true);
            Require(Vector2.Distance(Player.transform.position, new Vector2(13, 10.58f)) < .15f,
                "B→C no llega al apoyo superior seguro.");
            Debug.Log("TASK22_B_C_PASS: descenso físico por pozo, vida y checkpoint conservados.");

            yield return Wait(.4f);
            Require(Controller.IsGrounded, "La llegada desde B no está apoyada.");
            Keys(Key.Space);
            yield return Until(() => Body.position.y > 12.7f, 2);
            Keys(Key.A, Key.Space);
            yield return UntilRoom("B", 5);
            Keys();
            yield return Wait(.35f);
            AssertRoom("B", 4, true);
            Require(Vector2.Distance(Player.transform.position, new Vector2(12.3f, -3.42f)) < .15f,
                "C→B no vuelve al fondo del mismo pozo.");
            Debug.Log("TASK22_C_B_PASS: retorno por salto normal a la abertura superior.");

            yield return Wait(.4f);
            Require(Controller.IsGrounded, "La llegada desde C no está apoyada.");
            Keys(Key.Space);
            yield return Until(() => Body.position.y > -1.2f, 2);
            Keys(Key.D, Key.Space);
            yield return Until(() => Body.position.x > 13.8f, 2);
            Keys();
            yield return Until(() => Controller.IsGrounded, 3);
            Require(Mathf.Abs(Body.position.y - (-1.42f)) < .18f,
                "No se alcanzó la repisa oriental del pozo.");
            Keys(Key.Space);
            yield return Until(() => Body.position.y > .8f, 2);
            Keys(Key.D, Key.Space);
            yield return Until(() => Body.position.x > 15.7f, 2);
            Keys();
            yield return Until(() => Controller.IsGrounded, 3);
            Require(Mathf.Abs(Body.position.y - .58f) < .18f,
                "No se salió del pozo al suelo de B.");
            Debug.Log("TASK22_PIT_ESCAPE_PASS: salida del pozo con dos saltos normales.");

            PlayerAbilities abilities = Player.GetComponent<PlayerAbilities>();
            abilities.SetDashUnlocked(true);
            abilities.SetDoubleJumpUnlocked(true);
            expectUnlocked = true;

            yield return TravelFixture("B-to-F", "F");
            yield return TravelFixture("F-to-C", "C");
            yield return TravelFixture("C-to-D", "D");
            yield return TravelFixture("D-to-C", "C");
            yield return TravelFixture("C-to-F", "F");
            yield return TravelFixture("F-to-B", "B");
            Require(Object.FindAnyObjectByType<MemoryFlame>().IsCurrent &&
                Player.GetComponent<PlayerCheckpoint>().Location.IsValid,
                "La Llama/checkpoint de B no persistió.");
            Debug.Log("TASK22_LEGACY_LINKS_PASS: enlaces técnicos conservan vida, arma, checkpoint y una sesión.");

            yield return Wait(.9f);
            Require(Player.TryTakeDamage(Player.MaximumHealth,
                (Vector2)Player.transform.position + Vector2.right), "No se aceptó daño letal de prueba.");
            Require(Player.IsDead, "No se produjo la muerte de prueba.");
            yield return Press(Key.R);
            yield return Until(() => !RoomSession.Instance.IsLoading && !Player.IsDead, 12);
            AssertRoom("B", 5, true);
            Require(Player.GetComponent<PlayerCheckpoint>().CurrentCheckpoint != null,
                "La reaparición no restauró el checkpoint de B.");
            Debug.Log("TASK22_CHECKPOINT_PASS: muerte/R restaura Llama, vida y Colmillo.");
        }

        private static IEnumerator TravelFixture(string exitId, string destinationId)
        {
            RoomExit exit = Room.GetComponentsInChildren<RoomExit>().Single(item => item.ConnectionId == exitId);
            Require(RoomSession.Instance.TryTravel(Player, exit.Destination, exit.DestinationEntranceId),
                "No se pudo iniciar el enlace técnico " + exitId);
            yield return UntilRoom(destinationId, 12);
            yield return Wait(.35f);
            AssertRoom(destinationId, 4, true);
        }

        private static void AssertRoom(string id, int health, bool fang)
        {
            Require(Room != null && Room.Definition.RoomId == id &&
                SceneManager.sceneCount == 1 && Object.FindObjectsByType<RoomSession>().Length == 1 &&
                Object.FindObjectsByType<PlayerHealth>().Length == 1 && Object.FindObjectsByType<Camera>().Length == 1 &&
                !RoomSession.Instance.IsLoading && Player.CurrentHealth == health &&
                Player.GetComponent<PlayerAbilities>().FangOwned == fang &&
                Player.GetComponent<PlayerAbilities>().DashUnlocked == expectUnlocked &&
                Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked == expectUnlocked &&
                !Player.IsDead && Body.simulated && Controller.enabled,
                "Sesión/sala/estado incorrecto: " + id);
        }

        private static IEnumerator Walk(float target)
        {
            double end = Time.realtimeSinceStartupAsDouble + 6;
            while (Mathf.Abs(Body.position.x - target) > .1f || Mathf.Abs(Body.linearVelocity.x) > .2f)
            {
                float delta = target - Body.position.x;
                float speed = Body.linearVelocity.x;
                float brakingDistance = speed * speed / 120f + .05f;
                Keys(Mathf.Abs(delta) < .1f || delta * speed > 0 && Mathf.Abs(delta) < brakingDistance
                    ? Array.Empty<Key>() : new[] { delta > 0 ? Key.D : Key.A });
                Require(Time.realtimeSinceStartupAsDouble < end, "Caminar bloqueado hacia x=" + target);
                yield return null;
            }
            Keys();
            yield return Wait(.12f);
        }

        private static IEnumerator UntilRoom(string id, float timeout)
        {
            yield return Until(() => RoomSession.Instance != null && !RoomSession.Instance.IsLoading &&
                RoomSession.Instance.CurrentRoom != null && RoomSession.Instance.CurrentRoom.Definition.RoomId == id,
                timeout);
        }

        private static IEnumerator Until(Func<bool> condition, float timeout)
        {
            double end = Time.realtimeSinceStartupAsDouble + timeout;
            while (!condition())
            {
                Require(Time.realtimeSinceStartupAsDouble < end, "Tiempo agotado en ruta T22.");
                yield return null;
            }
        }

        private static IEnumerator Press(Key key)
        {
            Keys(key);
            yield return Wait(.08f);
            Keys();
            yield return Wait(.12f);
        }

        private static IEnumerator Wait(float seconds)
        {
            double end = Time.realtimeSinceStartupAsDouble + seconds;
            while (Time.realtimeSinceStartupAsDouble < end) yield return null;
        }

        private static void Keys(params Key[] keys)
            => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
