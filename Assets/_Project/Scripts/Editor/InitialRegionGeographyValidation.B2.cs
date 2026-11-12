using System;
using System.Collections;
using System.Linq;
using Fangbound.Combat;
using Fangbound.Player;
using Fangbound.UI;
using Fangbound.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Fangbound.EditorTools
{
    public static partial class InitialRegionGeographyValidation
    {
        private const string B2Mode = "Fangbound.T24.Validation";
        private const string B2Direct = "Fangbound.T24.Direct";

        [MenuItem("Fangbound/Validate Central Sector B2 T24 in Play Mode")]
        public static void RunB2() => StartGeographyValidation(true, false);

        [MenuItem("Fangbound/Validate Direct Start B2 T24 in Play Mode")]
        public static void RunB2Direct() => StartGeographyValidation(true, true);

        private static void ValidateP02B1(RoomScene room)
        {
            Check("Exit B-to-B2", 15.6f, 1);
            Check("Entry from-B2", 14.6f, .58f);
            Check("Entry from-F", 14.4f, 5.18f);
            Check("Exit B-to-F", 15.6f, 5.6f);
            CheckSurface("B East Floor", 14, 16, 0);
            CheckSurface("B Upper", 14, 16, 4.6f);
            RoomExit p02 = room.GetComponentsInChildren<RoomExit>().Single(e => e.ConnectionId == "B-to-B2");
            Require(p02.Destination.RoomId == "B2" && p02.DestinationEntranceId == "from-B" &&
                room.FindEntrance("from-B2").FacingDirection == -1, "P02 B1 perdió destino/orientación.");
            CheckP02Global(p02, Vector2.zero);
        }

        private static void ValidateB2Assets(RoomScene room)
        {
            Require(room.Definition.RoomId == "B2" && room.Definition.RecoveryRoom.RoomId == "B" &&
                !room.Definition.StartWithoutFang && room.Definition.InitialEntranceId == "start",
                "Política de inicio/recuperación B2 incorrecta.");
            Check("Entry start", 2.2f, .58f);
            Check("Entry from-B", 2.2f, .58f);
            Check("Exit B2-to-B", .4f, 1);
            CheckSurface("B2 West Ground", 0, 4.5f, 0);
            CheckSurface("B2 Broad Root", 4.5f, 9.5f, 1.2f);
            CheckSurface("B2 Hollow Floor", 9.5f, 12.5f, -1.2f);
            CheckSurface("B2 East Ground", 12.5f, 16, 0);
            CheckSurface("B2 Canopy Terrace", 6.5f, 9.8f, 3);
            CheckSurface("B2 Lookout", 11.4f, 15, 4.8f);
            Require(Object.FindObjectsByType<MemoryFlame>().Length == 0 &&
                Object.FindObjectsByType<FangPickup>().Length == 0 && room.FindEntrance("from-B").FacingDirection == 1,
                "B2 contiene contenido ajeno o una llegada invertida.");
            B2EncounterValidation.ValidateAssets(room);
            Require(room.GetComponentsInChildren<RoomExit>().Length == 2, "B2 debe conservar P02 y añadir únicamente P03.");
            RoomExit p02 = room.GetComponentsInChildren<RoomExit>().Single(e => e.ConnectionId == "B2-to-B");
            Require(p02.Destination.RoomId == "B" && p02.DestinationEntranceId == "from-B2", "Retorno P02 inválido.");
            CheckP02Global(p02, new Vector2(16, 0));
            var bounds = room.GetComponentInChildren<Fangbound.CameraSystem.CameraFollow2D>()
                .transform.parent.Find("Camera Room Bounds").GetComponent<Collider2D>().bounds;
            Require(Vector2.Distance(bounds.min, new Vector2(0, -4)) < .02f &&
                Vector2.Distance(bounds.max, new Vector2(16, 12)) < .02f, "B2 invadió B1/B3 o cambió dimensiones.");
            Debug.Log("TASK24_ASSETS_PASS: B2 16x16; P02 global (16,0), orientaciones, suelo y reservas correctos.");
        }

        private static void CheckP02Global(RoomExit exit, Vector2 origin)
        {
            Bounds bounds = exit.GetComponent<Collider2D>().bounds;
            // P is the shared edge at floor height, not either inset trigger centre.
            float edge = origin.x == 0 ? bounds.max.x : bounds.min.x;
            Require(Mathf.Abs(origin.x + edge - 16) < .02f && Mathf.Abs(origin.y + bounds.min.y) < .02f,
                "P02 no coincide en el borde global X=16 / suelo Y=0.");
        }

        private static IEnumerator ValidateB2Routes()
        {
            Keys(Key.D);
            yield return UntilRoom("B2", 6);
            Keys();
            yield return Wait(.4f);
            AssertB2State("B2", 4);
            Require(!Player.GetComponent<SpriteRenderer>().flipX && Mathf.Abs(Body.position.x - 2.2f) < .15f,
                "B1→B2 no aparece por la izquierda mirando hacia dentro.");
            Debug.Log("TASK24_P02_FORWARD_PASS: paso horizontal real, vida 4/5 y Colmillo conservados.");
            yield return ValidateB2Walks();
            Keys(Key.A);
            yield return UntilRoom("B", 6);
            Keys();
            yield return Wait(.4f);
            AssertB2State("B", 4);
            Require(Player.GetComponent<SpriteRenderer>().flipX && Mathf.Abs(Body.position.x - 14.6f) < .15f,
                "B2→B1 no aparece por la derecha mirando hacia dentro.");
            Debug.Log("TASK24_P02_RETURN_PASS: llegada segura fuera de triggers, sin bucle.");

            yield return ReturnAcrossB1Pit();
            yield return Press(Key.E);
            Require(Player.CurrentHealth == 5 && Object.FindAnyObjectByType<MemoryFlame>().IsCurrent,
                "La Llama B no curó tras volver desde B2.");
            Require(Player.TryTakeDamage(1, (Vector2)Player.transform.position + Vector2.right),
                "No se pudo restaurar vida reducida de prueba.");
            yield return Until(() => !Player.IsInvulnerable && !Controller.IsControlLocked, 3);
            Debug.Log("TASK24_FLAME_HEAL_PASS: E cura 4/5→5/5 después del regreso B2 y mantiene el checkpoint.");
            Keys(Key.A);
            yield return UntilRoom("A", 6);
            Keys();
            yield return Wait(.4f);
            AssertB2State("A", 4);
            Keys(Key.D);
            yield return UntilRoom("B", 6);
            Keys();
            yield return Wait(.4f);
            AssertB2State("B", 4);
            // Repeat A→B1→B2→B1→A without taking the exit to C at all.
            yield return Walk(7.1f);
            yield return B2Jump(9.5f, 1.5f);
            yield return Walk(13.35f);
            yield return Until(() => Controller.IsGrounded, 3);
            Require(Room.Definition.RoomId == "B" && Mathf.Abs(Body.position.y + 1.42f) < .18f,
                "El acceso oriental de B1 desvió al jugador hacia C.");
            yield return B2Jump(14.4f, 0);
            Keys(Key.D);
            yield return UntilRoom("B2", 6);
            Keys();
            yield return Wait(.4f);
            AssertB2State("B2", 4);
            if (SessionState.GetBool(B3Mode, false)) yield return ValidateB3FromB2();
            Keys(Key.A);
            yield return UntilRoom("B", 6);
            Keys();
            yield return Wait(.4f);
            AssertB2State("B", 4);
            yield return ReturnAcrossB1Pit();
            Keys(Key.A);
            yield return UntilRoom("A", 6);
            Keys();
            yield return Wait(.4f);
            AssertB2State("A", 4);
            Keys(Key.D);
            yield return UntilRoom("B", 6);
            Keys();
            yield return Wait(.4f);
            AssertB2State("B", 4);
            Debug.Log("TASK24_A_B2_A_PASS: A→B1→B2→B1→A físico, sin tomar C, con arma y vida reducida.");
            if (SessionState.GetBool(B3Mode, false))
                Debug.Log("TASK25_A_B3_A_PASS: A→B1→B2→B3→B2→B1→A físico, sin tomar C, a 4/5 y con Colmillo.");

            // Explicit state fixture: the physical P02 routes were already tested.
            Player.GetComponent<PlayerAbilities>().SetDashUnlocked(true);
            Player.GetComponent<PlayerAbilities>().SetDoubleJumpUnlocked(true);
            expectUnlocked = true;
            yield return TravelFixture("B-to-B2", "B2");
            AssertB2State("B2", 4);
            if (SessionState.GetBool(B3Mode, false)) yield return ValidateB3Checkpoint();
            yield return Wait(.9f);
            Require(Player.TryTakeDamage(Player.MaximumHealth, (Vector2)Player.transform.position + Vector2.right),
                "No se pudo probar muerte en B2.");
            yield return Press(Key.R);
            yield return Until(() => Room != null && Room.Definition.RoomId == "B" &&
                !RoomSession.Instance.IsLoading && !Player.IsDead, 12);
            AssertB2State("B", 5);
            Require(Player.GetComponent<PlayerCheckpoint>().CurrentCheckpoint != null &&
                Object.FindAnyObjectByType<MemoryFlame>().IsCurrent, "B2 no recuperó en la Llama B activa.");
            Debug.Log("TASK24_RESPAWN_PASS: muerte B2→Llama B, 5/5, Colmillo y desbloqueos persistentes.");

            // Restore the baseline fixture for the inherited T22 legacy-link tests.
            Player.GetComponent<PlayerAbilities>().SetDashUnlocked(false);
            Player.GetComponent<PlayerAbilities>().SetDoubleJumpUnlocked(false);
            expectUnlocked = false;
            yield return Until(() => !Player.IsInvulnerable && !Controller.IsControlLocked, 3);
            Require(Player.TryTakeDamage(1, (Vector2)Player.transform.position + Vector2.right), "Falta daño de prueba.");
            yield return Until(() => !Player.IsInvulnerable && !Controller.IsControlLocked, 3);
        }

        private static IEnumerator ReturnAcrossB1Pit()
        {
            yield return Walk(13.35f);
            yield return Until(() => Controller.IsGrounded, 3);
            Require(Mathf.Abs(Body.position.y + 1.42f) < .18f, "No se descendió al apoyo oriental de B1.");
            yield return B2Jump(9.8f, -2);
            yield return B2Jump(8.3f, 0);
            yield return Walk(6.1f);
        }

        private static IEnumerator ValidateLegacyFApproach()
        {
            yield return Walk(7.1f);
            yield return B2Jump(9.5f, 1.5f);
            yield return Walk(10.4f);
            yield return B2Jump(13.4f, 3);
            yield return B2Jump(14.4f, 4.6f);
            Keys(Key.D);
            yield return UntilRoom("F", 6);
            Keys();
            yield return Wait(.4f);
            AssertB2State("F", 4);
            Keys(Key.D);
            yield return UntilRoom("B", 6);
            Keys();
            yield return Wait(.4f);
            AssertB2State("B", 4);
            yield return Walk(13.3f);
            yield return Until(() => Controller.IsGrounded, 3);
            yield return B2Jump(9.8f, 1.5f);
            yield return Walk(7.1f);
            yield return Until(() => Controller.IsGrounded, 3);
            Debug.Log("TASK24_LEGACY_F_PHYSICS_PASS: escalones B1→F y regreso recorridos sin habilidades; enlace sigue técnico.");
        }

        private static IEnumerator ValidateB2Walks()
        {
            Require(!expectUnlocked, "La navegación B2 debe probarse sin habilidades.");
            yield return Walk(3.2f);
            yield return B2Jump(5.2f, 1.2f);
            yield return Walk(9.0f);
            yield return Walk(10.1f);
            yield return B2Jump(13.3f, 0);
            AssertB2State("B2", Player.CurrentHealth);
            Debug.Log("TASK24_MAIN_ROUTE_PASS: raíz amplia, descenso, hondonada y extremo oriental sin habilidades.");
            yield return Walk(11.6f);
            yield return Until(() => Controller.IsGrounded, 3);
            Require(Mathf.Abs(Body.position.y + .62f) < .18f, "No se aterrizó en la hondonada.");
            yield return B2Jump(10.15f, 0);
            yield return B2Jump(8.9f, 1.2f);
            yield return Walk(5.2f);
            yield return B2Jump(7.2f, 3);
            yield return Walk(9.0f);
            yield return B2Jump(12.3f, 4.8f);
            // P03 now occupies the eastern edge: recover westward within B2.
            yield return Walk(10.3f);
            yield return Until(() => Controller.IsGrounded && Body.position.y < 1, 4);
            Require(Mathf.Abs(Body.position.y - .58f) < .18f, "Caída del mirador sin recuperación en el escalón inferior.");
            yield return Walk(11.6f);
            yield return Until(() => Controller.IsGrounded, 3);
            yield return B2Jump(10.15f, 0);
            yield return B2Jump(8.9f, 1.2f);
            yield return Walk(5.0f);
            yield return Walk(3.0f);
            yield return Until(() => Controller.IsGrounded, 3);
            Require(Mathf.Abs(Body.position.y - .58f) < .18f, "No se volvió al sendero occidental de B2.");
            Debug.Log("TASK24_SECONDARY_RECOVERY_PASS: terraza/mirador, caída al escalón y salida de hondonada con física real.");
        }

        private static IEnumerator B2Jump(float x, float top)
        {
            yield return Until(() => Controller.IsGrounded, 3);
            Keys(x < Body.position.x ? new[] { Key.A, Key.Space } : new[] { Key.D, Key.Space });
            yield return Wait(.10f);
            double end = Time.realtimeSinceStartupAsDouble + 4;
            while (!Controller.IsGrounded)
            {
                float delta = x - Body.position.x;
                float speed = Body.linearVelocity.x;
                float braking = speed * speed / 120f + .05f;
                Keys(Mathf.Abs(delta) < .1f || delta * speed > 0 && Mathf.Abs(delta) < braking
                    ? new[] { Key.Space } : new[] { delta > 0 ? Key.D : Key.A, Key.Space });
                Require(Time.realtimeSinceStartupAsDouble < end, "Salto B2 excedió tiempo.");
                yield return null;
            }
            Keys();
            yield return Wait(.15f);
            Require(Mathf.Abs(Body.position.x - x) < .4f && Mathf.Abs(Body.position.y - (top + .58f)) < .18f,
                "Salto no alcanzó destino " + new Vector2(x, top) + "; pose=" + Body.position);
        }

        private static void AssertB2State(string id, int health)
        {
            AssertRoom(id, health, true);
            Require(Object.FindObjectsByType<PlayerHealthUI>().Length == 1 &&
                Object.FindObjectsByType<CombatHitstop>().Length == 1 &&
                Player.transform.Find("Fang Visuals") != null &&
                Player.transform.Find("Fang Visuals").GetComponentsInChildren<SpriteRenderer>(true).Length == 9,
                "HUD/feedback duplicado o incompleto en " + id);
        }

        private static IEnumerator ValidateDirectB2()
        {
            yield return Wait(.5f);
            oldBackground = InputSystem.settings.backgroundBehavior;
            oldEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>("T24DirectKeyboard");
            AssertB2State("B2", 5);
            Require(!Player.GetComponent<PlayerCheckpoint>().Location.IsValid, "Inicio B2 heredó checkpoint.");
            yield return ValidateB2Walks();
            Require(Player.TryTakeDamage(Player.MaximumHealth, (Vector2)Player.transform.position + Vector2.right),
                "Falta muerte directa de prueba.");
            yield return Press(Key.R);
            yield return Until(() => Room != null && Room.Definition.RoomId == "B" &&
                !RoomSession.Instance.IsLoading && !Player.IsDead, 12);
            AssertB2State("B", 5);
            Require(!Player.GetComponent<PlayerCheckpoint>().Location.IsValid &&
                !Object.FindAnyObjectByType<MemoryFlame>().IsCurrent, "Respawn sin checkpoint activó una Llama.");
            Debug.Log("TASK24_DIRECT_START_PASS: B2 directa armada, sin habilidades/checkpoint, rutas y recuperación B sin checkpoint.");
        }
    }
}
