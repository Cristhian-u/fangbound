using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Fangbound.EditorTools
{
    [InitializeOnLoad]
    public static class RoomCircuitValidation
    {
        const string Running = "Fangbound.Task17.Running", Fresh = "Fangbound.Task17.Fresh", Next = "Fangbound.Task17.Next", Index = "Fangbound.Task17.Index";
        const string Ruins = "Fangbound.Task18.Ruins";
        const string Canopy = "Fangbound.Task19.Canopy";
        static readonly Stack<IEnumerator> routines = new Stack<IEnumerator>();
        static Keyboard keyboard;
        static Gamepad gamepad;
        static InputSettings.BackgroundBehavior background;
        static InputSettings.EditorInputBehaviorInPlayMode editorInput;
        static double started;
        static bool hadError;
        static PlayerHealth Player => RoomSession.Instance.CurrentRoom.Player;
        static PlayerController Controller => Player.GetComponent<PlayerController>();
        static Rigidbody2D Body => Player.GetComponent<Rigidbody2D>();

        static RoomCircuitValidation()
        {
            EditorApplication.update += Tick;
            Application.logMessageReceived += (_, __, type) =>
            { if (SessionState.GetBool(Running, false) && (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)) hadError = true; };
        }

        [MenuItem("Fangbound/Validate Three Room Circuit in Play Mode")]
        public static void Run() => StartValidation(0);
        public static void RunC() => StartValidation(1);
        public static void RunF() => StartValidation(2);
        public static void RunD() => StartValidation(3);
        public static void RunE() => StartValidation(4);
        [MenuItem("Fangbound/Validate Ruins D Circuit in Play Mode")]
        public static void RunRuins() => StartValidation(0, ruins: true);
        [MenuItem("Fangbound/Validate Canopy E Circuit in Play Mode")]
        public static void RunCanopy() => StartValidation(0, canopy: true);

        static void StartValidation(int index, bool ruins = false, bool canopy = false)
        {
            SessionState.SetBool(Canopy, canopy); SessionState.SetBool(Ruins, ruins);
            SessionState.SetBool(Fresh, false); Begin(index);
        }

        // Editor-only fixtures for manual tests in rooms without enemies/hazards.
        [MenuItem("Fangbound/Room Circuit/Damage 1 HP (Play Mode)")]
        static void DamagePlayer() => ApplyDebugDamage(false);
        [MenuItem("Fangbound/Room Circuit/Defeat Player (Play Mode)")]
        static void DefeatPlayer() => ApplyDebugDamage(true);
        static void ApplyDebugDamage(bool lethal)
        {
            if (!DebugPlayer(out PlayerHealth player)) return;
            if (player.IsDead)
            { Debug.LogWarning("El jugador ya está derrotado. Enfoca Game View y pulsa R."); return; }
            int before = player.CurrentHealth;
            if (!player.TryTakeDamage(lethal ? player.MaximumHealth : 1, (Vector2)player.transform.position + Vector2.left))
            { Debug.LogWarning("Daño de prueba rechazado: espera a que termine la invulnerabilidad/parpadeo y repite el comando."); return; }
            Debug.Log($"ROOM_CIRCUIT_DEBUG_DAMAGE: {before}/{player.MaximumHealth} -> {player.CurrentHealth}/{player.MaximumHealth}." +
                (player.IsDead ? " Enfoca Game View y pulsa R para reaparecer." : " El daño conserva knockback e invulnerabilidad normales."));
        }
        [MenuItem("Fangbound/Room Circuit/Unlock Dash and Double Jump (session)")]
        static void UnlockAbilities()
        {
            if (!DebugPlayer(out PlayerHealth player)) return;
            player.GetComponent<PlayerAbilities>().SetDashUnlocked(true);
            player.GetComponent<PlayerAbilities>().SetDoubleJumpUnlocked(true);
            Debug.Log("ROOM_CIRCUIT_DEBUG_UNLOCK: dash y doble salto habilitados solo para esta sesión. Aterriza, enfoca Game View y prueba Shift; para doble salto, pulsa, suelta y vuelve a pulsar Espacio en el aire.");
        }
        [MenuItem("Fangbound/Room Circuit/Clear Checkpoint (session)")]
        static void ClearCheckpoint()
        {
            if (!DebugPlayer(out PlayerHealth player)) return;
            player.GetComponent<PlayerCheckpoint>().ClearCheckpoint();
            Debug.Log("ROOM_CIRCUIT_DEBUG_CHECKPOINT: registro borrado en esta sesión; la siguiente reaparición usará el inicio seguro.");
        }
        static bool DebugPlayer(out PlayerHealth player)
        {
            RoomSession session = RoomSession.Instance;
            player = EditorApplication.isPlaying && session != null && !session.IsLoading && session.CurrentRoom != null ? session.CurrentRoom.Player : null;
            if (player == null) Debug.LogWarning("Abrir una sala del circuito en Play Mode, fuera de una carga.");
            return player != null;
        }

        static string PathFor(int i) => i == 4 ? CanopyRoomSceneSetup.ScenePath : i == 3 ? RuinsRoomSceneSetup.ScenePath :
            RoomCircuitSceneSetup.SceneFolder + "/" + RoomCircuitSceneSetup.RoomNames[i] + ".unity";
        static void Begin(int index)
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Ejecutar desde Edit Mode.");
            if (!SessionState.GetBool(Fresh, false)) ValidateAssets();
            EditorSceneManager.OpenScene(PathFor(index));
            routines.Clear(); hadError = false;
            SessionState.SetInt(Index, index);
            SessionState.SetBool(Running, true);
            EditorApplication.EnterPlaymode();
        }

        static void ValidateAssets()
        {
            var entries = new Dictionary<string, HashSet<string>>();
            var exits = new List<(string scene, string entry)>();
            bool hasRuins = File.Exists(RuinsRoomSceneSetup.ScenePath);
            bool hasCanopy = File.Exists(CanopyRoomSceneSetup.ScenePath);
            for (int i = 0; i < (hasCanopy ? 5 : hasRuins ? 4 : 3); i++)
            {
                var scene = EditorSceneManager.OpenScene(PathFor(i));
                Require(EditorBuildSettings.scenes.Any(s => s.enabled && s.path == scene.path), "Sala fuera de Build Settings.");
                RoomScene room = Object.FindAnyObjectByType<RoomScene>();
                Require(room != null && room.Definition != null && room.Definition.RecoveryRoom != null &&
                    room.Definition.ScenePath == scene.path && room.Definition.RecoveryRoom.ScenePath == PathFor(0), "Definición de sala inválida.");
                Require(Object.FindObjectsByType<PlayerHealth>().Length == 1 && Object.FindObjectsByType<Camera>().Length == 1 &&
                    Object.FindObjectsByType<PlayerHealthUI>().Length == 1 && Object.FindObjectsByType<CombatHitstop>().Length == 1, "Rig duplicado o incompleto.");
                Require(Object.FindObjectsByType<MemoryFlame>().Length == (i == 0 ? 1 : 0) &&
                    Object.FindObjectsByType<GroundEnemyController>().Length == 0, "Llamas/enemigos inesperados.");
                RoomEntrance[] points = room.GetComponentsInChildren<RoomEntrance>();
                entries[scene.path] = new HashSet<string>(points.Select(p => p.EntranceId));
                int pointCount = i == 4 ? 2 : i == 3 ? (hasCanopy ? 3 : 2) : i == 1 && hasRuins ? 4 : 3;
                Require(points.Length == pointCount && entries[scene.path].Count == pointCount && entries[scene.path].Contains("start"), "IDs de entradas inválidos.");
                foreach (RoomEntrance point in points) Require(point.IsSafe(room.Player), "Entrada insegura: " + point.name);
                RoomExit[] connections = room.GetComponentsInChildren<RoomExit>();
                int exitCount = i == 4 ? 1 : i == 3 ? (hasCanopy ? 2 : 1) : i == 1 && hasRuins ? 3 : 2;
                Require(connections.Length == exitCount && connections.Select(e => e.ConnectionId).Distinct().Count() == exitCount, "Salidas duplicadas.");
                foreach (RoomExit exit in connections)
                {
                    Require(exit.Destination != null, "Salida sin destino: " + exit.name);
                    Require(exit.GetComponent<Collider2D>().isTrigger, "La salida debe ser un trigger.");
                    foreach (RoomEntrance point in points)
                        Require(!exit.GetComponent<Collider2D>().bounds.Contains(point.transform.position), "Entrada dentro de una salida.");
                    exits.Add((exit.Destination.ScenePath, exit.DestinationEntranceId));
                }
                foreach (GameObject root in scene.GetRootGameObjects())
                    foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                        Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) == 0, "Script ausente.");
            }
            foreach (var exit in exits) Require(entries.ContainsKey(exit.scene) && entries[exit.scene].Contains(exit.entry), "Destino sin entrada correspondiente.");
            Debug.Log("TASK17_ASSETS_PASS: salas independientes, entradas seguras, referencias y registro de carga válidos; incluye D=" + hasRuins + ", E=" + hasCanopy);
        }

        static void Tick()
        {
            if (SessionState.GetBool(Next, false) && !EditorApplication.isPlayingOrWillChangePlaymode)
            { SessionState.SetBool(Next, false); SessionState.SetBool(Fresh, true); Begin(2); return; }
            if (!SessionState.GetBool(Running, false) || !EditorApplication.isPlaying || EditorApplication.isPaused) return;
            try
            {
                if (routines.Count == 0) { started = EditorApplication.timeSinceStartup; routines.Push(Validate()); }
                Require(EditorApplication.timeSinceStartup - started < 300, "Validación de salas excedió 300 s.");
                IEnumerator step = routines.Peek();
                if (!step.MoveNext()) { routines.Pop(); if (routines.Count == 0) Finish(!hadError); }
                else if (step.Current is IEnumerator nested) routines.Push(nested);
            }
            catch (Exception exception) { Debug.LogException(exception); Finish(false); }
        }

        static void Finish(bool success)
        {
            routines.Clear(); SessionState.SetBool(Running, false);
            if (keyboard != null)
            {
                InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(gamepad); keyboard = null; gamepad = null;
                InputSystem.settings.backgroundBehavior = background;
                InputSystem.settings.editorInputBehaviorInPlayMode = editorInput;
            }
            if (success && SessionState.GetInt(Index, 0) == 0 && !SessionState.GetBool(Fresh, false) &&
                !SessionState.GetBool(Ruins, false) && !SessionState.GetBool(Canopy, false))
            { SessionState.SetBool(Next, true); EditorApplication.ExitPlaymode(); return; }
            Debug.Log(SessionState.GetBool(Canopy, false) ? (success ? "TASK19_PLAYMODE_PASS" : "TASK19_PLAYMODE_FAIL") :
                SessionState.GetBool(Ruins, false) ? (success ? "TASK18_PLAYMODE_PASS" : "TASK18_PLAYMODE_FAIL") :
                (success ? "TASK17_PLAYMODE_PASS" : "TASK17_PLAYMODE_FAIL"));
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
            else EditorApplication.ExitPlaymode();
        }

        static IEnumerator Validate()
        {
            yield return Wait(.6f);
            background = InputSystem.settings.backgroundBehavior;
            editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>("RoomCircuitKeyboard");
            gamepad = InputSystem.AddDevice<Gamepad>("RoomCircuitGamepad");
            AssertRoom();
            Require(Player.CurrentHealth == Player.MaximumHealth && !Player.GetComponent<PlayerCheckpoint>().Location.IsValid &&
                !Player.GetComponent<PlayerAbilities>().DashUnlocked && !Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked,
                "Una sesión nueva heredó vida, checkpoint o desbloqueos.");
            string initial = RoomSession.Instance.CurrentRoom.Definition.RoomId;
            Debug.Log("TASK17_DIRECT_START_PASS: " + initial);
            if (SessionState.GetBool(Canopy, false)) { yield return ValidateCanopyCircuit(); yield break; }
            if (SessionState.GetBool(Ruins, false)) { yield return ValidateRuinsCircuit(); yield break; }
            if (SessionState.GetBool(Fresh, false) || SessionState.GetInt(Index, 0) != 0)
            {
                yield return CheckAbilityInputs(false, false);
                ExecuteDebugMenu("Unlock Dash and Double Jump (session)");
                yield return CheckAbilityInputs(true, true);
                yield return DieAndRestart(false);
                Require(Player.GetComponent<PlayerAbilities>().DashUnlocked && Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked,
                    "Se perdió desbloqueo al volver sin checkpoint.");
                Debug.Log(SessionState.GetBool(Fresh, false) ? "TASK17_NEW_SESSION_PASS: segundo Play sin checkpoint/desbloqueos anteriores." :
                    "TASK17_DIRECT_RECOVERY_PASS: " + initial + " -> inicio B, vida y control restaurados.");
                yield break;
            }
            yield return CheckAbilityInputs(false, false);
            ExecuteDebugMenu("Unlock Dash and Double Jump (session)");
            yield return CheckAbilityInputs(true, true);
            Player.GetComponent<PlayerAbilities>().SetDashUnlocked(false);
            Player.GetComponent<PlayerAbilities>().SetDoubleJumpUnlocked(false);
            ExecuteDebugMenu("Damage 1 HP (Play Mode)");
            Require(Player.CurrentHealth == 4 && Player.IsInvulnerable, "El menú de daño no aplicó el contrato.");
            ExecuteDebugMenu("Defeat Player (Play Mode)");
            Require(!Player.IsDead && Player.CurrentHealth == 4, "El menú de muerte ignoró los i-frames.");
            Debug.Log("TASK17_DEBUG_IFRAMES_PASS: petición rechazada durante invulnerabilidad sin alterar vida o muerte.");
            yield return Until(() => !Player.IsInvulnerable, 2);
            ExecuteDebugMenu("Damage 1 HP (Play Mode)");
            Require(Player.CurrentHealth == 3, "Dos daños de menú no dejaron 3/5.");
            yield return Until(() => !Player.IsInvulnerable && !Controller.IsControlLocked, 2);
            Player.GetComponent<PlayerAbilities>().SetDashUnlocked(true);
            yield return Walk(3.8f);
            yield return Cross("B-to-C", true);
            yield return CheckAbilityInputs(true, false);
            ExecuteDebugMenu("Unlock Dash and Double Jump (session)");
            yield return CheckAbilityInputs(true, true);
            yield return Drop("C Upper", "C Terrace 6", 1, 10.1f);
            yield return Drop("C Terrace 6", "C Terrace 4", -1, 6.9f);
            yield return Drop("C Terrace 4", "C Terrace 2", 1, 11.1f);
            yield return Drop("C Terrace 2", "Safety Floor", 1, 15.1f);
            yield return Cross("C-to-F");
            yield return CheckAbilityInputs(true, true);
            yield return FUp();
            yield return Cross("F-to-B");
            Require(Player.CurrentHealth == 3 && Player.GetComponent<PlayerAbilities>().DashUnlocked &&
                Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked, "El circuito curó o perdió desbloqueos.");
            Debug.Log("TASK17_FORWARD_PASS: B-C-F-B, vida 3/5 persistente, dash/doble salto conservados y seis entradas sin duplicados.");
            yield return BDown();
            yield return Walk(6.1f);
            yield return Press(Key.E);
            Require(Player.CurrentHealth == 5 && Player.GetComponent<PlayerCheckpoint>().CurrentCheckpoint != null, "E no activó/curó.");
            CheckpointLocation location = Player.GetComponent<PlayerCheckpoint>().Location;
            Require(location.IsValid && location.ScenePath == PathFor(0) && location.FlameId == "B.memory-flame", "Registro de checkpoint inválido.");
            ExecuteDebugMenu("Damage 1 HP (Play Mode)");
            Require(Player.CurrentHealth == 4, "El menú no preparó curación con mando.");
            yield return Wait(.9f);
            yield return Walk(6.1f);
            InputSystem.QueueStateEvent(gamepad, new GamepadState().WithButton(GamepadButton.North));
            yield return Wait(.1f);
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            yield return Wait(.4f);
            Require(Player.CurrentHealth == 5 && Object.FindAnyObjectByType<MemoryFlame>().IsCurrent, "Norte del mando no curó.");
            CheckFlameVisual();
            Debug.Log("TASK17_FLAME_INPUT_PASS: E y norte, curación, escena/ID/posición registrados.");
            yield return Cross("B-to-C");
            yield return DieAndRestart(true);
            Require(Vector3.Distance(Player.GetComponent<PlayerCheckpoint>().Location.Position, location.Position) < .001f, "Posición de checkpoint cambió.");
            Debug.Log("TASK17_DEATH_C_PASS");
            yield return BUp();
            yield return Cross("B-to-F");
            yield return DieAndRestart(true);
            Debug.Log("TASK17_DEATH_F_PASS");
            ExecuteDebugMenu("Clear Checkpoint (session)");
            Player.GetComponent<PlayerAbilities>().SetDashUnlocked(false);
            Player.GetComponent<PlayerAbilities>().SetDoubleJumpUnlocked(true);
            yield return Cross("B-to-C");
            yield return DieAndRestart(false);
            Require(!Player.GetComponent<PlayerAbilities>().DashUnlocked && Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked,
                "Respawn sin checkpoint cambió habilidades.");
            Debug.Log("TASK17_NO_CHECKPOINT_PASS");
            Player.GetComponent<PlayerAbilities>().SetDoubleJumpUnlocked(false);
            yield return BUp();
            yield return Cross("B-to-F");
            yield return Jump("F East", "F High");
            yield return Jump("F High", "F Middle");
            yield return Jump("F Middle", "F Low");
            yield return Drop("F Low", "Safety Floor", -1, 5.1f);
            yield return Cross("F-to-C");
            yield return Jump("Safety Floor", "C Terrace 2");
            yield return Jump("C Terrace 2", "C Terrace 4");
            yield return Drop("C Terrace 4", "Safety Floor", 1, 8.8f);
            yield return Jump("Safety Floor", "C Terrace 2");
            yield return Jump("C Terrace 2", "C Terrace 4");
            yield return Jump("C Terrace 4", "C Terrace 6");
            yield return Jump("C Terrace 6", "C Upper");
            yield return Cross("C-to-B");
            Require(!Player.GetComponent<PlayerAbilities>().DashUnlocked && !Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked,
                "El retorno necesitó desbloqueos.");
            Debug.Log("TASK17_REVERSE_PASS: B-F-C-B sin dash/doble salto, caída y recuperación en C.");
            yield return Walk(6.1f); yield return Press(Key.E);
            Player.GetComponent<PlayerAbilities>().SetDashUnlocked(true);
            Player.GetComponent<PlayerAbilities>().SetDoubleJumpUnlocked(true);
            yield return ValidateTransientTransfers();
            Require(Player.TryTakeDamage(1, (Vector2)Player.transform.position + Vector2.left), "No se preparó aislamiento de sesiones.");
            Debug.Log("TASK17_SESSION_END_FIXTURE: checkpoint activo, desbloqueos y daño antes de salir de Play.");
        }

        static IEnumerator ToRuinsFromC()
        {
            yield return Jump("C Terrace 6", "C Ruins Approach");
            yield return Jump("C Ruins Approach", "C Ruins Gate");
            yield return Cross("C-to-D");
        }

        static IEnumerator RuinsUp()
        {
            yield return Jump("Safety Floor", "D Base Step");
            yield return Jump("D Base Step", "D West Landing");
            yield return Jump("D West Landing", "D Broken Crossing");
            yield return Jump("D Broken Crossing", "D East Rest");
            yield return Jump("D East Rest", "D Inner Ledge");
            yield return Jump("D Inner Ledge", "D Upper Chamber");
        }

        static IEnumerator UpperRuinsDownToCrossing()
        {
            yield return Drop("D Upper Chamber", "D Inner Ledge", -1, 10.9f);
            yield return Drop("D Inner Ledge", "D East Rest", 1, 15.1f);
            yield return Drop("D East Rest", "D Broken Crossing", -1, 10.9f);
        }

        static IEnumerator ValidateRuinsCircuit()
        {
            yield return Walk(6.1f); yield return Press(Key.E);
            Require(Player.GetComponent<PlayerCheckpoint>().Location.IsValid, "La Llama B no se activó antes de viajar a D.");
            ExecuteDebugMenu("Damage 1 HP (Play Mode)"); yield return Until(() => !Player.IsInvulnerable, 2);
            ExecuteDebugMenu("Damage 1 HP (Play Mode)"); yield return Until(() => !Player.IsInvulnerable, 2);
            yield return Cross("B-to-C");
            yield return Drop("C Upper", "C Terrace 6", 1, 10.1f);
            yield return ToRuinsFromC();
            Require(Player.CurrentHealth == 3 && Player.GetComponent<PlayerCheckpoint>().Location.IsValid &&
                Player.GetComponent<PlayerCheckpoint>().CurrentCheckpoint == null, "D perdió vida o registro estable de checkpoint.");
            yield return RuinsUp();
            Require(Object.FindObjectsByType<RoomExit>().Count(exit => exit.Destination.RoomId == "C") == 1 &&
                Object.FindObjectsByType<RoomExit>().All(exit => exit.Destination.RoomId == "C" || exit.Destination.RoomId == "E"),
                "D contiene una salida funcional inesperada.");
            Debug.Log("TASK18_ASCENT_PASS: cámara superior a 10.8, solo movimiento y salto normal, sin habilidades.");
            yield return Drop("D Upper Chamber", "D Inner Ledge", -1, 10.9f);
            yield return Jump("D Inner Ledge", "D Upper Chamber");
            yield return UpperRuinsDownToCrossing();
            yield return Jump("D Broken Crossing", "D West Lookout");
            yield return Drop("D West Lookout", "D Entry Roof", -1, .8f);
            Require(RoomSession.Instance.CurrentRoom.Definition.RoomId == "D", "La caída del mirador activó la salida.");
            yield return Jump("D Entry Roof", "D West Landing");
            yield return Jump("D West Landing", "D Broken Crossing");
            yield return Jump("D Broken Crossing", "D West Lookout");
            Debug.Log("TASK18_ENTRY_RECOVERY_PASS: caída lateral sobre dintel, sin activar salida y con retorno por salto normal.");
            yield return Jump("D West Lookout", "D Inner Ledge");
            yield return Jump("D Inner Ledge", "D Upper Chamber");
            Debug.Log("TASK18_OPTIONAL_LOOP_PASS: mirador lateral y reconexión con ascenso principal.");
            yield return UpperRuinsDownToCrossing();
            yield return Drop("D Broken Crossing", "Safety Floor", 1, 13.1f);
            yield return Jump("Safety Floor", "D Base Step");
            yield return Jump("D Base Step", "D West Landing");
            yield return Jump("D West Landing", "D Broken Crossing");
            Debug.Log("TASK18_RECOVERY_PASS: caída al suelo y regreso al recorrido, sin daño.");
            yield return Drop("D Broken Crossing", "D West Landing", -1, 4.9f);
            yield return Drop("D West Landing", "D Base Step", 1, 8.1f);
            yield return Drop("D Base Step", "Safety Floor", -1, 6.1f);
            yield return Cross("D-to-C");
            yield return Jump("C Ruins Gate", "C Ruins Approach");
            yield return Jump("C Ruins Approach", "C Terrace 6");
            yield return Drop("C Terrace 6", "C Terrace 4", -1, 6.9f);
            yield return Drop("C Terrace 4", "C Terrace 2", 1, 11.1f);
            yield return Drop("C Terrace 2", "Safety Floor", 1, 15.1f);
            yield return Cross("C-to-F"); yield return FUp(); yield return Cross("F-to-B");
            Require(Player.CurrentHealth == 3 && !Player.GetComponent<PlayerAbilities>().DashUnlocked &&
                !Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked, "El circuito curó o requirió desbloqueos.");
            CheckFlameVisual();
            Debug.Log("TASK18_CIRCUIT_PASS: B-C-D-C-F-B, 3/5 y checkpoint B conservados; ambos desbloqueos bloqueados.");
            yield return BDown(); yield return Walk(6.1f); yield return Press(Key.E);
            ExecuteDebugMenu("Unlock Dash and Double Jump (session)");
            ExecuteDebugMenu("Damage 1 HP (Play Mode)"); yield return Until(() => !Player.IsInvulnerable, 2);
            yield return Cross("B-to-C"); yield return Drop("C Upper", "C Terrace 6", 1, 10.1f);
            yield return ToRuinsFromC();
            Require(Player.CurrentHealth == 4 && Player.GetComponent<PlayerAbilities>().DashUnlocked &&
                Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked, "D perdió desbloqueos o curó al entrar.");
            yield return CheckAbilityInputs(true, true);
            yield return Cross("D-to-C");
            yield return Cross("C-to-D");
            Require(Player.CurrentHealth == 4 && Player.GetComponent<PlayerAbilities>().DashUnlocked &&
                Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked, "El regreso D-C-D alteró vida o desbloqueos.");
            Debug.Log("TASK18_UNLOCKED_RETURN_PASS: D-C-D, 4/5 y ambos desbloqueos conservados.");
            yield return DieAndRestart(true);
            Require(Player.GetComponent<PlayerAbilities>().DashUnlocked && Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked,
                "Reaparecer desde D perdió desbloqueos.");
            Debug.Log("TASK18_CHECKPOINT_PASS: muerte en D, R hacia Llama B activa, 5/5 y desbloqueos conservados.");
        }

        static IEnumerator ToCanopyFromB()
        {
            yield return Cross("B-to-C");
            yield return Drop("C Upper", "C Terrace 6", 1, 10.1f);
            yield return ToRuinsFromC();
            yield return RuinsUp();
            yield return Cross("D-to-E");
        }

        static IEnumerator CanopyMainUp()
        {
            yield return Jump("Safety Floor", "E Fork Bough");
            yield return Jump("E Fork Bough", "E Main West Bough");
            yield return Jump("E Main West Bough", "E Main Crossing");
            yield return Jump("E Main Crossing", "E Main Return");
            yield return Jump("E Main Return", "E Crown Junction");
            yield return CanopyJunctionToLookout();
        }

        static IEnumerator CanopyJunctionToLookout()
        {
            yield return Jump("E Crown Junction", "E Watch Approach");
            yield return Jump("E Watch Approach", "E Watch Lookout");
        }

        static IEnumerator CanopyLookoutToJunction()
        {
            yield return Drop("E Watch Lookout", "E Watch Approach", -1, 22.9f);
            yield return Drop("E Watch Approach", "E Crown Junction", -1, 15.9f);
        }

        static IEnumerator CanopyJunctionToFork()
        {
            yield return Drop("E Crown Junction", "E Main Return", -1, 5.9f);
            yield return Drop("E Main Return", "E Main Crossing", 1, 10.1f);
            yield return Drop("E Main Crossing", "E Main West Bough", -1, 6.4f);
            yield return Drop("E Main West Bough", "E Fork Bough", 1, 8.1f);
        }

        static IEnumerator RuinsToLowerCircuit()
        {
            yield return UpperRuinsDownToCrossing();
            yield return Drop("D Broken Crossing", "D West Landing", -1, 4.9f);
            yield return Drop("D West Landing", "D Base Step", 1, 8.1f);
            yield return Drop("D Base Step", "Safety Floor", -1, 6.1f);
            yield return Cross("D-to-C");
            yield return Jump("C Ruins Gate", "C Ruins Approach");
            yield return Jump("C Ruins Approach", "C Terrace 6");
            yield return Drop("C Terrace 6", "C Terrace 4", -1, 6.9f);
            yield return Drop("C Terrace 4", "C Terrace 2", 1, 11.1f);
            yield return Drop("C Terrace 2", "Safety Floor", 1, 15.1f);
            yield return Cross("C-to-F"); yield return FUp(); yield return Cross("F-to-B");
        }

        static IEnumerator ValidateCanopyCircuit()
        {
            yield return Walk(6.1f); yield return Press(Key.E);
            Require(Player.GetComponent<PlayerCheckpoint>().Location.IsValid, "No se activó la Llama B.");
            ExecuteDebugMenu("Damage 1 HP (Play Mode)"); yield return Until(() => !Player.IsInvulnerable, 2);
            ExecuteDebugMenu("Damage 1 HP (Play Mode)"); yield return Until(() => !Player.IsInvulnerable, 2);
            yield return ToCanopyFromB();
            Require(Player.CurrentHealth == 3 && Player.GetComponent<PlayerCheckpoint>().Location.IsValid &&
                Player.GetComponent<PlayerCheckpoint>().CurrentCheckpoint == null, "E perdió vida o checkpoint estable de B.");
            Require(Object.FindObjectsByType<RoomExit>().Length == 1 && Object.FindAnyObjectByType<RoomExit>().Destination.RoomId == "D",
                "E contiene una conexión distinta de D (B queda fuera de T19).");
            yield return CanopyMainUp();
            Require(!Player.GetComponent<PlayerAbilities>().DashUnlocked && !Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked,
                "La ruta principal necesitó habilidades.");
            Debug.Log("TASK19_MAIN_ROUTE_PASS: entrada, bifurcación occidental y mirador a 11.4, salto normal.");
            yield return CanopyLookoutToJunction(); yield return CanopyJunctionToFork();
            yield return Jump("E Fork Bough", "E Side Sprig");
            yield return Jump("E Side Sprig", "E Side Outer Bough");
            yield return Drop("E Side Outer Bough", "E Lower Catch", -1, 23.9f);
            yield return Jump("E Lower Catch", "E Side Sprig");
            Debug.Log("TASK19_LOWER_LOOP_PASS: descenso bajo la travesía exterior y recuperación hacia la rama baja.");
            yield return Jump("E Side Sprig", "E Side Outer Bough");
            yield return Jump("E Side Outer Bough", "E Side Inner Bough");
            yield return Jump("E Side Inner Bough", "E Crown Junction");
            yield return CanopyJunctionToLookout();
            Debug.Log("TASK19_SECONDARY_ROUTE_PASS: travesía oriental, giro de regreso y reconexión en la copa central, sin habilidades.");
            yield return CanopyLookoutToJunction();
            yield return Drop("E Crown Junction", "E Side Inner Bough", 1, 20.1f);
            yield return Drop("E Side Inner Bough", "E Side Outer Bough", 1, 26.1f);
            yield return Drop("E Side Outer Bough", "Safety Floor", 1, 31.1f);
            yield return Jump("Safety Floor", "E Lower Catch");
            yield return Jump("E Lower Catch", "E Side Sprig");
            yield return Drop("E Side Sprig", "Safety Floor", -1, 16.5f);
            yield return Jump("Safety Floor", "E Fork Bough");
            yield return Drop("E Fork Bough", "Safety Floor", -1, 6.1f);
            Require(Player.CurrentHealth == 3, "Las recuperaciones alteraron la vida.");
            Debug.Log("TASK19_RECOVERY_PASS: descenso desde mirador, caída al suelo oriental y regreso a la entrada con salto normal.");
            yield return Cross("E-to-D"); yield return RuinsToLowerCircuit();
            Require(Player.CurrentHealth == 3 && !Player.GetComponent<PlayerAbilities>().DashUnlocked &&
                !Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked, "El circuito curó o cambió desbloqueos.");
            CheckFlameVisual();
            Debug.Log("TASK19_CIRCUIT_PASS: B-C-D-E-D-C-F-B con 3/5, checkpoint B y sin habilidades.");
            yield return BDown(); yield return Walk(6.1f); yield return Press(Key.E);
            ExecuteDebugMenu("Unlock Dash and Double Jump (session)");
            ExecuteDebugMenu("Damage 1 HP (Play Mode)"); yield return Until(() => !Player.IsInvulnerable, 2);
            yield return ToCanopyFromB(); yield return CheckAbilityInputs(true, true);
            yield return Cross("E-to-D"); yield return Cross("D-to-E");
            Require(Player.CurrentHealth == 4 && Player.GetComponent<PlayerAbilities>().DashUnlocked &&
                Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked, "E-D-E perdió vida o desbloqueos.");
            Debug.Log("TASK19_UNLOCKED_RETURN_PASS: E-D-E con 4/5 y desbloqueos conservados.");
            yield return DieAndRestart(true);
            Require(Player.GetComponent<PlayerAbilities>().DashUnlocked && Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked,
                "La muerte en E perdió desbloqueos.");
            Debug.Log("TASK19_CHECKPOINT_PASS: muerte en E, R hacia Llama B activa con 5/5 y desbloqueos conservados.");
        }

        static void ExecuteDebugMenu(string command)
        {
            Require(EditorApplication.ExecuteMenuItem("Fangbound/Room Circuit/" + command), "Menú de pruebas no disponible: " + command);
        }

        static IEnumerator CheckAbilityInputs(bool dash, bool doubleJump)
        {
            RoomScene room = RoomSession.Instance.CurrentRoom;
            PlayerAbilities abilities = Player.GetComponent<PlayerAbilities>();
            Require(abilities.DashUnlocked == dash && abilities.DoubleJumpUnlocked == doubleJump, "Desbloqueos inesperados antes de comprobar entrada.");
            Vector3 original = Player.transform.position;
            Quaternion rotation = Player.transform.rotation;
            int health = Player.CurrentHealth;
            int facing = Player.GetComponent<SpriteRenderer>().flipX ? -1 : 1;
            // Separate fixtures, away from exits and with clear vertical space.
            // The complete traversal tests below continue from the original pose.
            Vector3 fixture = new Vector3(room.Definition.RoomId == "E" ? 16.5f : room.Definition.RoomId == "D" ? 13f :
                room.Definition.RoomId == "C" ? 16f : room.Definition.RoomId == "F" ? 3.3f : 4f, .58f);
            Keys();
            Player.RestoreAfterSceneLoad(fixture, Quaternion.identity, health, false);
            Controller.SetFacingDirection(1);
            room.SynchronizeCamera();
            yield return Wait(.2f);
            Require(Controller.IsGrounded, "Fixture de habilidad sin suelo.");
            float startX = Body.position.x;
            bool sawDash = false;
            double end = Time.realtimeSinceStartupAsDouble + .25;
            Keys(Key.LeftShift);
            while (Time.realtimeSinceStartupAsDouble < end)
            {
                Require(RoomSession.Instance.CurrentRoom == room && !RoomSession.Instance.IsLoading, "Fixture de dash alcanzó una salida.");
                sawDash |= Controller.IsDashing;
                CheckCamera(); yield return null;
            }
            Keys();
            Require(dash ? sawDash && Body.position.x - startX > 1f : !sawDash && Mathf.Abs(Body.position.x - startX) < .05f,
                "Dash no respeta su desbloqueo en " + room.Definition.RoomId);
            Player.RestoreAfterSceneLoad(fixture, Quaternion.identity, health, false);
            room.SynchronizeCamera(); yield return Wait(.2f);
            Keys(Key.Space);
            yield return Until(() => !Controller.IsGrounded, .5f);
            yield return Wait(.12f);
            Keys(); yield return Wait(.05f);
            Require(!Controller.IsGrounded, "Se aterrizó antes de la segunda pulsación.");
            Keys(Key.Space);
            float secondPeak = float.NegativeInfinity;
            end = Time.realtimeSinceStartupAsDouble + .09;
            while (Time.realtimeSinceStartupAsDouble < end)
            { secondPeak = Mathf.Max(secondPeak, Body.linearVelocity.y); CheckCamera(); yield return null; }
            Require(doubleJump ? secondPeak > 9f : secondPeak < 9f,
                "Doble salto no respeta su desbloqueo en " + room.Definition.RoomId + "; vy=" + secondPeak);
            Keys();
            if (doubleJump)
            {
                yield return Wait(.05f);
                Require(!Controller.IsGrounded, "Fixture de tercer salto aterrizó.");
                Keys(Key.Space); float thirdPeak = float.NegativeInfinity;
                end = Time.realtimeSinceStartupAsDouble + .08;
                while (Time.realtimeSinceStartupAsDouble < end)
                { thirdPeak = Mathf.Max(thirdPeak, Body.linearVelocity.y); CheckCamera(); yield return null; }
                Require(thirdPeak < 9f, "Se permitió un tercer salto aéreo.");
                Keys();
            }
            Player.RestoreAfterSceneLoad(original, rotation, health, false);
            Controller.SetFacingDirection(facing); room.SynchronizeCamera();
            yield return Wait(.2f);
            Require(Player.CurrentHealth == health && abilities.DashUnlocked == dash && abilities.DoubleJumpUnlocked == doubleJump,
                "La comprobación de habilidad alteró vida o desbloqueos.");
            Debug.Log($"TASK17_ABILITY_INPUT_PASS: {room.Definition.RoomId}, dash={dash}, doble={doubleJump}; Shift y pulsaciones de Espacio reales, sin tercer salto.");
        }

        static IEnumerator BUp()
        { yield return Jump("Safety Floor", "B Step 1"); yield return Jump("B Step 1", "B Step 2"); yield return Jump("B Step 2", "B Upper"); }
        static IEnumerator BDown()
        { yield return Jump("B Upper", "B Step 2"); yield return Jump("B Step 2", "B Step 1"); yield return Drop("B Step 1", "Safety Floor", -1, 7.1f); }
        static IEnumerator FUp()
        { yield return Jump("Safety Floor", "F Low"); yield return Jump("F Low", "F Middle"); yield return Jump("F Middle", "F High"); yield return Jump("F High", "F East"); }

        static IEnumerator DieAndRestart(bool checkpoint)
        {
            yield return Wait(.9f);
            ExecuteDebugMenu("Defeat Player (Play Mode)");
            Require(Player.IsDead, "Muerte por menú/contrato rechazada.");
            yield return Press(Key.R);
            yield return Until(() => RoomSession.Instance != null && !RoomSession.Instance.IsLoading &&
                RoomSession.Instance.CurrentRoom != null && RoomSession.Instance.CurrentRoom.Definition.RoomId == "B", 12);
            Vector3 expected = checkpoint ? Player.GetComponent<PlayerCheckpoint>().Location.Position :
                RoomSession.Instance.CurrentRoom.FindEntrance("start").transform.position;
            Require(!Player.IsDead && Player.CurrentHealth == Player.MaximumHealth && Vector3.Distance(Player.transform.position, expected) < .15f,
                "R no restauró vida/posición.");
            Require((Player.GetComponent<PlayerCheckpoint>().CurrentCheckpoint != null) == checkpoint, "Referencia/estado de Llama no restaurados.");
            if (checkpoint) CheckFlameVisual();
            AssertRoom(); yield return Wait(.3f); yield return Walk(Player.transform.position.x + .3f);
        }

        static IEnumerator ValidateTransientTransfers()
        {
            Keys(Key.J, Key.K); yield return Wait(.03f); Keys();
            Require(AttackRemaining() > 0 && Player.GetComponent<PlayerParry>().IsWindowOpen, "Fixture de ataque/parry no comenzó.");
            MemoryFlame oldFlame = Player.GetComponent<PlayerCheckpoint>().CurrentCheckpoint;
            RoomExit exit = Object.FindObjectsByType<RoomExit>().Single(e => e.ConnectionId == "B-to-C");
            CombatHitstop.Request(CombatHitstopKind.Normal);
            Require(Time.timeScale == 0 && RoomSession.Instance.TryTravel(Player, exit.Destination, exit.DestinationEntranceId), "No se inició carga con hitstop.");
            Require(Time.timeScale == 1, "La transición dejó hitstop activo.");
            yield return Until(() => !RoomSession.Instance.IsLoading && RoomSession.Instance.CurrentRoom.Definition.RoomId == "C", 12);
            AssertRoom();
            Require(AttackRemaining() == 0 && !Player.GetComponent<PlayerParry>().IsWindowOpen && oldFlame == null &&
                Player.GetComponent<PlayerCheckpoint>().CurrentCheckpoint == null && Player.GetComponent<PlayerCheckpoint>().Location.IsValid,
                "Estado temporal o referencia de Llama descargada conservados.");
            Debug.Log("TASK17_ATTACK_TRANSFER_PASS: fixture de ataque/parry/hitstop; carga conserva registro sin referencia descargada.");
            yield return Wait(.4f);
            // Separate fixture for physical knockback through the actual trigger.
            Player.RestoreAfterSceneLoad(new Vector3(21.6f, .58f), Quaternion.identity, Player.CurrentHealth, false);
            RoomSession.Instance.CurrentRoom.SynchronizeCamera();
            Require(Player.TryTakeDamage(1, (Vector2)Player.transform.position + Vector2.left), "No se preparó knockback.");
            yield return Cross("C-to-F", false, true);
            Require(Player.CurrentHealth == 4, "La transición durante knockback curó.");
            Debug.Log("TASK17_KNOCKBACK_TRANSFER_PASS: trigger real, daño conservado y control restaurado.");
            yield return DieAndRestart(true);
        }

        static float AttackRemaining() => (float)typeof(PlayerCombat).GetField("attackTimeRemaining",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(Player.GetComponent<PlayerCombat>());

        static void CheckFlameVisual()
        {
            MemoryFlame flame = Object.FindAnyObjectByType<MemoryFlame>();
            var serialized = new SerializedObject(flame);
            var data = (MemoryFlameData)serialized.FindProperty("flameData").objectReferenceValue;
            var visual = (SpriteRenderer)serialized.FindProperty("flameRenderer").objectReferenceValue;
            var glow = (SpriteRenderer)serialized.FindProperty("glowRenderer").objectReferenceValue;
            Require(flame.IsCurrent && !flame.IsActivationVisible && visual.color == data.ActiveColor &&
                Mathf.Approximately(glow.color.a, data.ActiveGlowAlpha), "Apariencia activa de Llama no restaurada.");
        }

        static IEnumerator Cross(string id, bool dash = false, bool requireKnockback = false)
        {
            RoomExit exit = Object.FindObjectsByType<RoomExit>().Single(e => e.ConnectionId == id);
            PlayerHealth source = Player;
            PlayerController sourceController = source.GetComponent<PlayerController>();
            RoomScene sourceRoom = RoomSession.Instance.CurrentRoom;
            int hp = source.CurrentHealth;
            bool dashUnlocked = source.GetComponent<PlayerAbilities>().DashUnlocked, doubleUnlocked = source.GetComponent<PlayerAbilities>().DoubleJumpUnlocked;
            bool dashed = false;
            int dir = exit.transform.position.x > Player.transform.position.x ? 1 : -1;
            double end = Time.realtimeSinceStartupAsDouble + 8;
            Keys(dash ? new[] { dir > 0 ? Key.D : Key.A, Key.LeftShift } : new[] { dir > 0 ? Key.D : Key.A });
            while (!RoomSession.Instance.IsLoading && RoomSession.Instance.CurrentRoom == sourceRoom)
            {
                dashed |= source.GetComponent<PlayerController>().IsDashing;
                CheckCamera(); Require(Time.realtimeSinceStartupAsDouble < end, "Salida no activada: " + id);
                yield return null;
            }
            Keys();
            if (requireKnockback) Require(sourceController.IsControlLocked, "La salida no se cruzó durante knockback.");
            Require(!RoomSession.Instance.TryTravel(source, exit.Destination, exit.DestinationEntranceId), "Dos cargas simultáneas aceptadas.");
            string scene = exit.Destination.ScenePath, entryId = exit.DestinationEntranceId;
            yield return Until(() => RoomSession.Instance != null && !RoomSession.Instance.IsLoading &&
                RoomSession.Instance.CurrentRoom != null && RoomSession.Instance.CurrentRoom.Definition.ScenePath == scene, 12);
            RoomEntrance entry = RoomSession.Instance.CurrentRoom.FindEntrance(entryId);
            Require(Vector3.Distance(Player.transform.position, entry.transform.position) < .15f && entry.IsSafe(Player), "Entrada equivocada/insegura.");
            Require(Player.GetComponent<SpriteRenderer>().flipX == (entry.FacingDirection < 0), "Orientación de entrada incorrecta.");
            Require(Player.CurrentHealth == hp && Player.GetComponent<PlayerAbilities>().DashUnlocked == dashUnlocked &&
                Player.GetComponent<PlayerAbilities>().DoubleJumpUnlocked == doubleUnlocked, "La carga curó o cambió habilidades.");
            if (dash) Require(dashed && !Controller.IsDashing, "Dash no se limpió durante transición.");
            AssertRoom(); yield return Wait(.4f);
            Require(!RoomSession.Instance.IsLoading && RoomSession.Instance.CurrentRoom.Definition.ScenePath == scene, "Bucle de cargas tras entrar.");
            Debug.Log("TASK17_CONNECTION_PASS: " + id + " -> " + entryId + ", vida=" + hp);
        }

        static void AssertRoom()
        {
            Require(SceneManager.sceneCount == 1 && Object.FindObjectsByType<RoomSession>().Length == 1 &&
                Object.FindObjectsByType<PlayerHealth>().Length == 1 && Object.FindObjectsByType<Camera>().Length == 1 &&
                Object.FindObjectsByType<PlayerHealthUI>().Length == 1 && Object.FindObjectsByType<CombatHitstop>().Length == 1,
                "Carga aditiva o instancias duplicadas.");
            Require(!Player.IsDead && Controller.enabled && !Controller.IsControlLocked && !Controller.IsDashing &&
                Player.GetComponent<PlayerCombat>().enabled && Player.GetComponent<PlayerParry>().enabled && Body.simulated &&
                Mathf.Approximately(Body.gravityScale, 3), "Estados temporales/control inválidos.");
            CheckCamera();
        }
        static void CheckCamera()
        {
            var camera = Object.FindAnyObjectByType<CameraFollow2D>().GetComponent<Camera>();
            Bounds bounds = Player.GetComponent<Collider2D>().bounds;
            Vector3 low = camera.WorldToViewportPoint(bounds.min), high = camera.WorldToViewportPoint(bounds.max);
            Require(low.x > -.02f && low.y > -.02f && high.x < 1.02f && high.y < 1.02f, "Jugador fuera de cámara.");
        }
        static Bounds Surface(string name) => GameObject.Find(name).GetComponent<Collider2D>().bounds;
        static IEnumerator Walk(float x)
        {
            double end = Time.realtimeSinceStartupAsDouble + 8;
            while (Mathf.Abs(Body.position.x - x) > .1f || Mathf.Abs(Body.linearVelocity.x) > .2f)
            {
                float delta = x - Body.position.x, speed = Body.linearVelocity.x;
                float brake = speed * speed / 120f + .05f;
                if (Mathf.Abs(delta) < .1f || (delta * speed > 0 && Mathf.Abs(delta) < brake)) Keys();
                else Keys(delta > 0 ? Key.D : Key.A);
                CheckCamera(); Require(Time.realtimeSinceStartupAsDouble < end, "Caminar bloqueado: " + x + " en " + Body.position);
                yield return null;
            }
            Keys(); yield return Wait(.16f);
        }
        static IEnumerator Jump(string from, string to)
        {
            Bounds a = Surface(from), b = Surface(to);
            int dir = b.center.x >= Body.position.x ? 1 : -1;
            yield return Walk(dir > 0 ? Mathf.Min(a.max.x - .65f, b.min.x - .9f) : Mathf.Max(a.min.x + .65f, b.max.x + .9f));
            Require(Controller.IsGrounded, "Despegue no apoyado: " + from);
            float target = dir > 0 ? Mathf.Min(b.min.x + 1.1f, b.center.x) : Mathf.Max(b.max.x - 1.1f, b.center.x);
            Keys(dir > 0 ? Key.D : Key.A, Key.Space);
            bool air = false, stop = false;
            double end = Time.realtimeSinceStartupAsDouble + 4;
            do
            {
                yield return null; CheckCamera(); air |= !Controller.IsGrounded;
                if (!stop && dir * (Body.position.x - target) >= 0) { Keys(Key.Space); stop = true; }
                Require(Time.realtimeSinceStartupAsDouble < end, "Salto bloqueado: " + from + " -> " + to);
            } while (!air || !Controller.IsGrounded);
            Keys(); yield return Wait(.12f);
            Require(Mathf.Abs(Player.GetComponent<Collider2D>().bounds.min.y - b.max.y) < .12f && Body.position.x > b.min.x && Body.position.x < b.max.x,
                "Aterrizaje incorrecto: " + from + " -> " + to + ", pos=" + Body.position);
            Debug.Log("TASK17_JUMP_PASS: " + from + " -> " + to);
        }
        static IEnumerator Drop(string from, string to, int dir, float target)
        {
            Bounds a = Surface(from), b = Surface(to);
            yield return Walk(dir > 0 ? a.max.x - .6f : a.min.x + .6f);
            Keys(dir > 0 ? Key.D : Key.A);
            bool air = false, stop = false; double end = Time.realtimeSinceStartupAsDouble + 4;
            do
            {
                yield return null; CheckCamera(); air |= !Controller.IsGrounded;
                float brake = Body.linearVelocity.x * Body.linearVelocity.x / 120f + .03f;
                if (!stop && dir * (Body.position.x - target) >= -brake) { Keys(); stop = true; }
                Require(Time.realtimeSinceStartupAsDouble < end, "Caída bloqueada.");
            } while (!air || !Controller.IsGrounded);
            Keys(); yield return Wait(.16f);
            Require(Mathf.Abs(Player.GetComponent<Collider2D>().bounds.min.y - b.max.y) < .12f, "Caída fuera del suelo seguro.");
        }
        static IEnumerator Until(Func<bool> condition, float seconds)
        { double end = Time.realtimeSinceStartupAsDouble + seconds; while (!condition()) { Require(Time.realtimeSinceStartupAsDouble < end, "Espera agotada."); yield return null; } }
        static void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        static IEnumerator Press(Key key) { Keys(key); yield return Wait(.08f); Keys(); yield return Wait(.1f); }
        static IEnumerator Wait(float seconds)
        { double end = Time.realtimeSinceStartupAsDouble + seconds; while (Time.realtimeSinceStartupAsDouble < end) yield return null; }
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
