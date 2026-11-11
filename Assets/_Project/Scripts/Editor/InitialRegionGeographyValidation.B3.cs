using System;
using System.Collections;
using System.Linq;
using Fangbound.Player;
using Fangbound.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Fangbound.EditorTools
{
    public static partial class InitialRegionGeographyValidation
    {
        private const string B3Mode = "Fangbound.T25.Validation";

        [MenuItem("Fangbound/Validate Split Trunk B3 T25 in Play Mode")]
        public static void RunB3() => StartGeographyValidation(true, false, true);

        [MenuItem("Fangbound/Validate Direct Start B3 T25 in Play Mode")]
        public static void RunB3Direct() => StartGeographyValidation(false, true, true);

        private static void ValidateP03B2(RoomScene room)
        {
            Check("Exit B2-to-B3", 15.6f, 1);
            Check("Entry from-B3", 13.8f, .58f);
            Require(GameObject.Find("Overgrown Eastern Root") == null, "La raíz cerrada T24 tapa P03.");
            RoomExit exit = room.GetComponentsInChildren<RoomExit>().Single(e => e.ConnectionId == "B2-to-B3");
            Require(exit.Destination.RoomId == "B3" && exit.DestinationEntranceId == "from-B2" &&
                room.FindEntrance("from-B3").FacingDirection == -1, "P03 B2 inválido.");
            CheckP03Global(exit, new Vector2(16, 0), true);
        }

        private static void ValidateB3Assets(RoomScene room)
        {
            Require(room.Definition.RoomId == "B3" && room.Definition.RecoveryRoom.RoomId == "B" &&
                !room.Definition.StartWithoutFang && room.Definition.InitialEntranceId == "start", "Definición B3 inválida.");
            Check("Entry start", 2.2f, 4.58f);
            Check("Entry from-B2", 2.2f, 4.58f);
            Check("Exit B3-to-B2", .4f, 5);
            CheckSurface("B3 Entry Root", 0, 3.6f, 4);
            CheckSurface("B3 East Clearing", 7, 12, 4);
            CheckSurface("B3 Root Floor Seal", 0, 12, -4);
            CheckSurface("B3 Lower East Root", 7, 10.2f, -2.2f);
            CheckSurface("B3 Lower West Root", 1.8f, 4.8f, -.3f);
            CheckSurface("B3 Inner Knot", 4.8f, 7, 1.8f);
            CheckSurface("B3 West Fork", 3.5f, 6.5f, 5.8f);
            CheckSurface("B3 East Fork", 7.8f, 11.6f, 7.5f);
            CheckSurface("B3 Crown West", 3.8f, 7, 9.5f);
            CheckSurface("B3 Crown Rest", 7.6f, 11.6f, 11.3f);
            CheckSurface("B3 Split Pocket", .8f, 3, 7.6f);
            RoomExit exit = room.GetComponentsInChildren<RoomExit>().Single();
            Require(exit.Destination.RoomId == "B2" && exit.DestinationEntranceId == "from-B3" &&
                room.FindEntrance("from-B2").FacingDirection == 1 &&
                room.GetComponentsInChildren<RoomEntrance>().Length == 2, "B3 tiene puertos falsos o llegada invertida.");
            CheckP03Global(exit, new Vector2(32, -4), false);
            // Planned F1 is not the current technical F scene. These values verify the reservation only.
            Vector2 p12B3 = new Vector2(32, -4) + new Vector2(6, -4);
            Vector2 p12F1 = new Vector2(16, -16) + new Vector2(22, 8);
            Require(p12B3 == new Vector2(38, -8) && p12B3 == p12F1 &&
                room.FindEntrance("from-F") == null && room.FindEntrance("from-E") == null,
                "P12 reservado no coincide o se creó una conexión F/E no autorizada.");
            Require(Object.FindObjectsByType<MemoryFlame>().Length == 0 &&
                Object.FindObjectsByType<FangPickup>().Length == 0 &&
                Object.FindObjectsByType<Fangbound.Enemies.GroundEnemyController>().Length == 0,
                "B3 contiene contenido fuera de T25.");
            foreach (Collider2D solid in GameObject.Find("Level Geometry").GetComponentsInChildren<Collider2D>())
                Require(!solid.isTrigger && solid.gameObject.layer == 0, "Geometría B3 debe ser sólida en Default.");
            Require(GameObject.Find("Provisional Split Trunk Decoration").GetComponentsInChildren<Collider2D>().Length == 0,
                "La decoración B3 no debe añadir colisiones.");
            var bounds = room.GetComponentInChildren<Fangbound.CameraSystem.CameraFollow2D>().transform.parent
                .Find("Camera Room Bounds").GetComponent<Collider2D>().bounds;
            Require(Vector2.Distance(bounds.min, new Vector2(0, -6)) < .02f &&
                Vector2.Distance(bounds.max, new Vector2(12, 16)) < .02f, "Límites visuales B3 incorrectos.");
            Debug.Log("TASK25_ASSETS_PASS: P03 global (32,0), B3 12x20/ancla (32,-4), P12 sellado en (38,-8), sin falsas salidas.");
        }

        private static void CheckP03Global(RoomExit exit, Vector2 anchor, bool east)
        {
            Bounds bounds = exit.GetComponent<Collider2D>().bounds;
            Vector2 point = anchor + new Vector2(east ? bounds.max.x : bounds.min.x, bounds.min.y);
            Require(Vector2.Distance(point, new Vector2(32, 0)) < .02f, "Las caras/suelos de P03 no coinciden globalmente.");
        }

        private static IEnumerator ValidateB3FromB2()
        {
            yield return Walk(3.2f);
            yield return B2Jump(5.2f, 1.2f);
            yield return Walk(10.1f);
            yield return B2Jump(13.3f, 0);
            Keys(Key.D);
            yield return UntilRoom("B3", 6);
            Keys();
            yield return Wait(.4f);
            AssertB2State("B3", 4);
            Require(!Player.GetComponent<SpriteRenderer>().flipX &&
                Vector2.Distance(Body.position, new Vector2(2.2f, 4.58f)) < .18f, "Llegada P03 B3 incorrecta.");
            yield return Wait(.4f);
            Require(Room.Definition.RoomId == "B3" && !RoomSession.Instance.IsLoading, "Bucle al entrar en B3.");
            Debug.Log("TASK25_P03_FORWARD_PASS: suelo global 0, entrada occidental segura a 4/5 con Colmillo.");
            yield return ValidateB3Walks();
            Keys(Key.A);
            yield return UntilRoom("B2", 6);
            Keys();
            yield return Wait(.4f);
            AssertB2State("B2", 4);
            Require(Player.GetComponent<SpriteRenderer>().flipX &&
                Vector2.Distance(Body.position, new Vector2(13.8f, .58f)) < .18f, "Regreso P03 B2 incorrecto.");
            yield return Walk(11.6f);
            yield return B2Jump(10.15f, 0);
            yield return B2Jump(8.9f, 1.2f);
            yield return Walk(3.0f);
            yield return Until(() => Controller.IsGrounded, 3);
            Debug.Log("TASK25_P03_RETURN_PASS: vuelta a B2, orientación, recuperación y continuidad hacia B1.");
        }

        private static IEnumerator LandB3(float top)
        {
            yield return Until(() => Controller.IsGrounded, 4);
            Require(Room.Definition.RoomId == "B3" && Mathf.Abs(Body.position.y - (top + .58f)) < .18f,
                "Aterrizaje B3 no llegó a " + top + "; pose=" + Body.position);
        }

        private static IEnumerator ValidateB3Walks()
        {
            Require(!expectUnlocked, "B3 debe recorrerse sin habilidades.");
            yield return Walk(1.9f);
            yield return B2Jump(5, 5.8f);
            yield return Walk(8.8f);
            yield return LandB3(4);
            yield return Walk(6.2f);
            yield return LandB3(1.8f);
            yield return Walk(7.6f);
            yield return LandB3(-2.2f);
            yield return Walk(6.3f);
            yield return LandB3(-4);
            Debug.Log("TASK25_LOWER_DESCENT_PASS: entrada, claro oriental y raíces hasta el suelo P12 sellado.");

            yield return Walk(5.5f);
            yield return B2Jump(8.3f, -2.2f);
            yield return Walk(7.7f);
            yield return B2Jump(4, -.3f);
            yield return Walk(3.1f);
            yield return B2Jump(5.8f, 1.8f);
            yield return B2Jump(2.6f, 4);
            Debug.Log("TASK25_LOWER_ASCENT_PASS: desde P12 previsto a P03 mediante saltos normales 1.8/1.9/2.1/2.2.");

            yield return Walk(1.9f);
            yield return B2Jump(5, 5.8f);
            yield return Walk(5.8f);
            yield return B2Jump(9, 7.5f);
            yield return B2Jump(5.4f, 9.5f);
            yield return B2Jump(9, 11.3f);
            Debug.Log("TASK25_MAIN_ASCENT_PASS: claros y bifurcaciones del tronco hasta descanso superior, sin dash/doble salto.");

            yield return Walk(6.3f);
            yield return LandB3(9.5f);
            yield return Walk(2.2f);
            yield return LandB3(7.6f);
            yield return B2Jump(5.2f, 9.5f);
            yield return Walk(8.8f);
            yield return LandB3(7.5f);
            yield return Walk(6);
            yield return LandB3(5.8f);
            yield return Walk(4.4f);
            yield return B2Jump(2, 7.6f);
            yield return B2Jump(5.2f, 9.5f);
            Debug.Log("TASK25_SECONDARY_RECOVERY_PASS: bolsillo oeste, reconexión y caídas recuperables entre ramas.");
            yield return Walk(8.8f);
            yield return LandB3(7.5f);
            yield return Walk(6);
            yield return LandB3(5.8f);
            yield return Walk(2.2f);
            yield return LandB3(4);
            Require(Room.GetComponentsInChildren<RoomExit>().Length == 1, "Apareció una salida F durante Play.");
        }

        private static IEnumerator ValidateB3Checkpoint()
        {
            yield return TravelFixture("B2-to-B3", "B3");
            AssertB2State("B3", 4);
            yield return Wait(.9f);
            Require(Player.TryTakeDamage(Player.MaximumHealth, (Vector2)Player.transform.position + Vector2.right),
                "No se produjo muerte en B3.");
            yield return Press(Key.R);
            yield return Until(() => Room != null && Room.Definition.RoomId == "B" &&
                !RoomSession.Instance.IsLoading && !Player.IsDead, 12);
            AssertB2State("B", 5);
            Require(Object.FindAnyObjectByType<MemoryFlame>().IsCurrent &&
                Player.GetComponent<PlayerCheckpoint>().CurrentCheckpoint != null, "B3 perdió checkpoint B.");
            Debug.Log("TASK25_CHECKPOINT_PASS: B3→Llama B con 5/5, arma y desbloqueos conservados.");
            yield return Until(() => !Player.IsInvulnerable && !Controller.IsControlLocked, 3);
            Require(Player.TryTakeDamage(1, (Vector2)Player.transform.position + Vector2.right), "No se restauró fixture 4/5.");
            yield return Until(() => !Player.IsInvulnerable && !Controller.IsControlLocked, 3);
            yield return TravelFixture("B-to-B2", "B2");
        }

        private static IEnumerator ValidateDirectB3()
        {
            yield return Wait(.5f);
            oldBackground = InputSystem.settings.backgroundBehavior;
            oldEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>("T25DirectKeyboard");
            AssertB2State("B3", 5);
            Require(!Player.GetComponent<PlayerCheckpoint>().Location.IsValid, "B3 directa heredó checkpoint.");
            yield return ValidateB3Walks();
            Require(Player.TryTakeDamage(Player.MaximumHealth, (Vector2)Player.transform.position + Vector2.right),
                "No se pudo probar recuperación sin checkpoint B3.");
            yield return Press(Key.R);
            yield return Until(() => Room != null && Room.Definition.RoomId == "B" &&
                !RoomSession.Instance.IsLoading && !Player.IsDead, 12);
            AssertB2State("B", 5);
            Require(!Player.GetComponent<PlayerCheckpoint>().Location.IsValid &&
                !Object.FindAnyObjectByType<MemoryFlame>().IsCurrent, "B3 directa activó una Llama automáticamente.");
            Debug.Log("TASK25_DIRECT_START_PASS: B3 directa, rutas sin habilidades y muerte/R sin checkpoint hacia B.");
        }
    }
}
