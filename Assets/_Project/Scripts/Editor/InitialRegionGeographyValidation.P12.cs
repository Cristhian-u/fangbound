using System.Collections;
using System.Linq;
using Fangbound.Player;
using Fangbound.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Fangbound.EditorTools
{
    public static partial class InitialRegionGeographyValidation
    {
        private const string P12Mode = "Fangbound.T27.Validation";

        [MenuItem("Fangbound/Validate F1 B3 Return T27 in Play Mode")]
        public static void RunP12() => StartGeographyValidation(true, false, true, true);

        [MenuItem("Fangbound/Validate Direct Start F1 T27 in Play Mode")]
        public static void RunF1Direct() => StartGeographyValidation(false, true, false, true);

        private static void NeutralizeTransitionInput(Scene scene, LoadSceneMode mode)
        {
            // Release the synthetic device at load, before the arriving player's Update.
            // Otherwise a held source direction can immediately turn/move a vertical arrival.
            if (keyboard != null) InputState.Change(keyboard, new KeyboardState());
        }

        private static void ValidateP12Assets(RoomScene room)
        {
            string id = room.Definition.RoomId;
            if (id == "C")
            {
                CheckSurface("Safety Floor", 0, 16, 0);
                CheckSurface("C Upper Roof East", 15, 16, 12.6f);
                Check("Entry from-F", 13.8f, .58f);
                Check("Entry from-D", 2.2f, .58f);
                RoomExit p11 = room.GetComponentsInChildren<RoomExit>().Single(e => e.ConnectionId == "C-to-F");
                Require(p11.Destination.RoomId == "F" && p11.DestinationEntranceId == "from-C" &&
                    Vector2.Distance(new Vector2(p11.GetComponent<Collider2D>().bounds.max.x,
                    p11.GetComponent<Collider2D>().bounds.min.y - 16), new Vector2(16, -16)) < .02f,
                    "P11 C1 no coincide globalmente.");
                Require(room.FindEntrance("from-D").FacingDirection == 1 &&
                    room.GetComponentsInChildren<RoomExit>().Single(e => e.ConnectionId == "C-to-D").Destination.RoomId == "D" &&
                    GameObject.Find("C Ruins Gate") == null, "Ramal técnico D o extensión C1 inválidos.");
                Check("Exit C-to-D", .4f, 1);
                Check("Exit C-to-F", 15.6f, 1);
                CheckOpening("P11 C1 East Roots");
                CheckOpening("Technical D West Rocks");
            }
            if (id == "F")
            {
                CheckSurface("Safety Floor", 0, 32, 0);
                CheckSurface("F Low", 6, 8.5f, .8f);
                CheckSurface("F Middle", 10, 13, 2);
                CheckSurface("F High", 15, 18, 3.8f);
                CheckSurface("F East", 19, 24, 3.8f);
                CheckSurface("F P12 Rest", 20, 24, 6);
                CheckSurface("F East Root Shelf", 26, 30.5f, 2.2f);
                CheckSurface("F Roof West", 0, 19, 8.6f);
                CheckSurface("F Roof East", 25, 32, 8.6f);
                Check("Entry from-C", 2.2f, .58f);
                Check("Entry from-B3", 22, 6.58f);
                RoomExit p11 = room.GetComponentsInChildren<RoomExit>().Single(e => e.ConnectionId == "F-to-C");
                Require(p11.Destination.RoomId == "C" && p11.DestinationEntranceId == "from-F" &&
                    Vector2.Distance(new Vector2(p11.GetComponent<Collider2D>().bounds.min.x + 16,
                    p11.GetComponent<Collider2D>().bounds.min.y - 16), new Vector2(16, -16)) < .02f,
                    "P11 F1 no coincide globalmente.");
                CheckOpening("P11 F1 West Rocks");
                CheckOpening("P12 F1 Upper Roots");
                CheckP12(room, "F-to-B3", new Vector2(16, -16), true);
                Require(room.FindEntrance("from-B3").FacingDirection == -1 && room.FindEntrance("from-B") == null &&
                    room.GetComponentsInChildren<RoomExit>().All(e => e.Destination.RoomId != "B"), "F1 conserva retorno B o dirección incorrecta.");
                var cameraBounds = room.GetComponentInChildren<Fangbound.CameraSystem.CameraFollow2D>().transform.parent
                    .Find("Camera Room Bounds").GetComponent<Collider2D>().bounds;
                Require(Vector2.Distance(cameraBounds.min, new Vector2(0, -4)) < .02f &&
                    Vector2.Distance(cameraBounds.max, new Vector2(32, 12)) < .02f, "Cámara F1 no cubre su ancho/ascenso.");
            }
            if (id == "B3")
            {
                Check("Entry from-F", 6, -3.22f);
                CheckP12(room, "B3-to-F", new Vector2(32, -4), false);
                CheckOpening("P12 B3 Lower Roots");
                Require(room.FindEntrance("from-F").FacingDirection == 1, "La llegada B3 no mira al ascenso.");
            }
            if (id == "F" || id == "B3")
                Debug.Log("TASK27_ASSETS_PASS: " + id + "; P12 global (38,-8), abertura 6, llegadas sobre apoyos fuera de triggers y sin F↔B1.");
        }

        private static void CheckP12(RoomScene room, string connection, Vector2 anchor, bool upward)
        {
            RoomExit exit = room.GetComponentsInChildren<RoomExit>().Single(e => e.ConnectionId == connection);
            Bounds bounds = exit.GetComponent<Collider2D>().bounds;
            Vector2 edge = anchor + new Vector2(bounds.center.x, upward ? bounds.min.y : bounds.max.y);
            Require(Vector2.Distance(edge, new Vector2(38, -8)) < .02f &&
                Vector2.Distance(bounds.size, new Vector2(6, .8f)) < .02f &&
                exit.Destination.RoomId == (upward ? "B3" : "F") &&
                exit.DestinationEntranceId == (upward ? "from-F" : "from-B3"), "P12 no coincide en plano, ancho o destino.");
        }

        private static void CheckOpening(string name)
        {
            GameObject opening = GameObject.Find(name);
            Require(opening != null && opening.GetComponentsInChildren<SpriteRenderer>().Length >= 3 &&
                opening.GetComponentsInChildren<Collider2D>().Length == 0, "Acceso sin representación natural o con colisiones: " + name);
        }

        private static IEnumerator DescendB3ToP12()
        {
            yield return Walk(1.9f);
            yield return B2Jump(5, 5.8f);
            yield return Walk(8.8f); yield return LandB3(4);
            yield return Walk(6.2f); yield return LandB3(1.8f);
            yield return Walk(7.6f); yield return LandB3(-2.2f);
            yield return Walk(6.3f); yield return LandB3(-3.8f);
        }

        private static IEnumerator DropP12(int health)
        {
            // Walk off the arrival root into the right opening. Gravity does the descent.
            Keys(Key.D);
            yield return UntilRoom("F", 6);
            Keys(); yield return Wait(.4f);
            AssertB2State("F", health);
            Require(Vector2.Distance(Body.position, new Vector2(22, 6.58f)) < .18f &&
                Player.GetComponent<SpriteRenderer>().flipX, "P12 no llega al descanso F1 mirando hacia dentro: " + Body.position);
            yield return Wait(.4f);
            Require(Room.Definition.RoomId == "F" && !RoomSession.Instance.IsLoading, "P12 repite transición al llegar abajo.");
            Debug.Log("TASK27_P12_DOWN_PASS: caída natural B3→F1, mismo eje global X=38, apoyo seguro y sin bucle.");
        }

        private static IEnumerator F1FromGround(int health)
        {
            yield return Walk(5.1f);
            yield return B2Jump(7.2f, .8f);
            yield return B2Jump(11.3f, 2);
            yield return F1FromMiddle(health);
        }

        private static IEnumerator F1FromMiddle(int health)
        {
            yield return Walk(12.2f);
            yield return B2Jump(16.2f, 3.8f);
            yield return Walk(17.2f);
            yield return B2Jump(21.1f, 6);
            yield return Walk(22);
            Keys(Key.Space);
            yield return UntilRoom("B3", 6);
            Keys(); yield return Wait(.4f);
            AssertB2State("B3", health);
            Require(Vector2.Distance(Body.position, new Vector2(6, -3.22f)) < .18f &&
                !Player.GetComponent<SpriteRenderer>().flipX, "P12 no llega a la raíz B3 mirando hacia el ascenso.");
            yield return Wait(.4f);
            Require(Room.Definition.RoomId == "B3" && !RoomSession.Instance.IsLoading, "P12 repite transición al llegar arriba.");
            Debug.Log("TASK27_P12_UP_PASS: roca/raíces 0→0.8→2→3.8→6 y salto por P12; llegada B3 a -3.8, sin habilidades.");
        }

        private static IEnumerator RecoverF1East()
        {
            yield return Walk(28.5f);
            yield return Until(() => Controller.IsGrounded, 3);
            Require(Mathf.Abs(Body.position.y - 2.78f) < .18f, "La caída oriental no llegó al descanso.");
            yield return Walk(31.1f);
            yield return Until(() => Controller.IsGrounded, 3);
            Require(Mathf.Abs(Body.position.y - .58f) < .18f && Room.Definition.RoomId == "F", "Fondo F1 inseguro o salida F2 falsa.");
            yield return Walk(14.4f);
            yield return B2Jump(11.8f, 2);
            Debug.Log("TASK27_F1_RECOVERY_PASS: descanso oriental, suelo seguro y regreso a la ruta de ascenso con salto normal.");
        }

        private static IEnumerator B3FromP12ToSendero()
        {
            yield return Walk(5.5f);
            yield return B2Jump(8.3f, -2.2f);
            yield return Walk(7.7f);
            yield return B2Jump(4, -.3f);
            yield return Walk(3.1f);
            yield return B2Jump(5.8f, 1.8f);
            yield return B2Jump(2.6f, 4);
            Debug.Log("TASK27_B3_ASCENT_PASS: P12→P03 físico por raíces, desnivel visible y sin habilidades.");
        }

        private static IEnumerator ValidateP12RoundTrip()
        {
            Require(!expectUnlocked, "El circuito físico P12 debe probarse sin habilidades.");
            yield return DescendB3ToP12();
            yield return DropP12(4);
            yield return RecoverF1East();
            yield return F1FromMiddle(4);
            yield return B3FromP12ToSendero();
        }

        private static IEnumerator ValidateF1Checkpoint()
        {
            yield return TravelFixture("B2-to-B3", "B3");
            yield return TravelFixture("B3-to-F", "F");
            AssertB2State("F", 4);
            Require(expectUnlocked && Player.GetComponent<PlayerCheckpoint>().Location.IsValid, "Fixture F1 perdió desbloqueos/checkpoint.");
            Require(Player.TryTakeDamage(Player.MaximumHealth, (Vector2)Player.transform.position + Vector2.right), "No se pudo probar muerte F1.");
            yield return Press(Key.R);
            yield return Until(() => Room != null && Room.Definition.RoomId == "B" && !RoomSession.Instance.IsLoading && !Player.IsDead, 12);
            AssertB2State("B", 5);
            Require(Player.GetComponent<PlayerCheckpoint>().CurrentCheckpoint != null &&
                Object.FindAnyObjectByType<MemoryFlame>().IsCurrent, "F1 no recuperó en la Llama B.");
            Debug.Log("TASK27_CHECKPOINT_PASS: muerte F1/R→Llama B con 5/5, Colmillo y ambos desbloqueos conservados.");
            yield return Until(() => !Player.IsInvulnerable && !Controller.IsControlLocked, 3);
            Require(Player.TryTakeDamage(1, (Vector2)Player.transform.position + Vector2.right), "No se restauró fixture de vida 4/5.");
            yield return Until(() => !Player.IsInvulnerable && !Controller.IsControlLocked, 3);
            yield return TravelFixture("B-to-B2", "B2");
        }

        private static IEnumerator ValidateDirectF1()
        {
            yield return Wait(.5f);
            oldBackground = InputSystem.settings.backgroundBehavior;
            oldEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>("T27DirectKeyboard");
            AssertB2State("F", 5);
            Require(!Player.GetComponent<PlayerCheckpoint>().Location.IsValid && !expectUnlocked, "Inicio F1 heredó estado.");
            yield return F1FromGround(5);
            yield return DropP12(5);
            yield return RecoverF1East();
            yield return F1FromMiddle(5);
            yield return DropP12(5);
            Keys(Key.A);
            yield return UntilRoom("C", 10);
            Keys(); yield return Wait(.4f); AssertB2State("C", 5);
            Require(Vector2.Distance(Body.position, new Vector2(13.8f, .58f)) < .18f, "P11 no llega a C1 por el este.");
            Keys(Key.D);
            yield return UntilRoom("F", 6);
            Keys(); yield return Wait(.4f); AssertB2State("F", 5);
            Require(Vector2.Distance(Body.position, new Vector2(2.2f, .58f)) < .18f, "P11 no llega a F1 por el oeste.");
            Keys(Key.A);
            yield return UntilRoom("C", 6);
            Keys(); yield return Wait(.4f);
            Debug.Log("TASK27_P11_PHYSICS_PASS: F1→C1→F1→C1 por suelo global -16, entradas seguras y estado conservado.");
            Keys(Key.A);
            yield return UntilRoom("D", 8);
            Keys(); yield return Wait(.4f); AssertB2State("D", 5);
            Keys(Key.A);
            yield return UntilRoom("C", 6);
            Keys(); yield return Wait(.4f); AssertB2State("C", 5);
            Require(Vector2.Distance(Body.position, new Vector2(2.2f, .58f)) < .18f, "El retorno D no llega al ramal occidental C1.");
            Debug.Log("TASK27_CD_PHYSICS_PASS: ramal técnico C1 oeste→D→C1 físico; D/E intactas, sin atribuir C2/P06 construidos.");
            // Verify recovery from the actual lower C1 floor all the way back to B1.
            yield return Walk(8.3f);
            yield return B2Jump(11.5f, 2);
            yield return Walk(10.4f);
            yield return B2Jump(7, 4);
            yield return Walk(7.4f);
            yield return B2Jump(10.5f, 6);
            yield return Walk(9.6f);
            yield return B2Jump(6.1f, 8);
            yield return Walk(4.8f);
            yield return B2Jump(8, 10);
            yield return Walk(7.4f);
            Keys(Key.D, Key.Space);
            yield return UntilRoom("B", 6);
            Keys(); yield return Wait(.4f); AssertB2State("B", 5);
            Debug.Log("TASK27_C1_RECOVERY_PASS: suelo C1→terrazas 2/4/6/8/10→P04→B1 mediante saltos normales.");
            // State fixtures only: return to F1 to test death there without an activated flame.
            yield return TransferDirectF1("B-to-B2", "B2");
            yield return TransferDirectF1("B2-to-B3", "B3");
            yield return TransferDirectF1("B3-to-F", "F");
            Require(Player.TryTakeDamage(Player.MaximumHealth, (Vector2)Player.transform.position + Vector2.right), "No se pudo probar muerte directa F1.");
            yield return Press(Key.R);
            yield return Until(() => Room != null && Room.Definition.RoomId == "B" && !RoomSession.Instance.IsLoading && !Player.IsDead, 12);
            AssertB2State("B", 5);
            Require(!Player.GetComponent<PlayerCheckpoint>().Location.IsValid && !Object.FindAnyObjectByType<MemoryFlame>().IsCurrent,
                "F1 directa activó una Llama automáticamente.");
            Debug.Log("TASK27_DIRECT_F1_PASS: inicio armado sin habilidades/checkpoint, P12/P11 y recuperación físicos; muerte F1/R→inicio B sin activar Llama.");
        }

        private static IEnumerator TransferDirectF1(string connection, string target)
        {
            RoomExit exit = Room.GetComponentsInChildren<RoomExit>().Single(e => e.ConnectionId == connection);
            Require(RoomSession.Instance.TryTravel(Player, exit.Destination, exit.DestinationEntranceId), "Falló fixture " + connection);
            yield return UntilRoom(target, 12); yield return Wait(.4f); AssertB2State(target, 5);
        }
    }
}
