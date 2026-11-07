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
    internal static class GroundEnemyPogoValidation
    {
        internal static IEnumerator Validate(PlayerHealth player)
        {
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            PlayerController controller = player.GetComponent<PlayerController>();
            PlayerPogo pogo = player.GetComponent<PlayerPogo>();
            PlayerPogoData data = (PlayerPogoData)ReadField(pogo, "pogoData");
            PlayerCombatData combatData = (PlayerCombatData)ReadField(player.GetComponent<PlayerCombat>(), "combatData");
            RigidbodyConstraints2D constraints = RigidbodyConstraints2D.FreezeRotation;
            InputSettings.BackgroundBehavior background = InputSystem.settings.backgroundBehavior;
            InputSettings.EditorInputBehaviorInPlayMode editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>("Task09PogoValidationKeyboard");
            GameObject fixture = null;
            GameObject platform = new GameObject("Pogo Validation Ground", typeof(BoxCollider2D));
            platform.transform.position = new Vector3(100f, -2.5f);
            platform.GetComponent<BoxCollider2D>().size = new Vector2(8f, 0.8f);
            try
            {
                fixture = CreateEnemy();
                yield return Wait(0.06f); // Let Start configure the existing collision exclusions.
                fixture.GetComponent<GroundEnemyController>().enabled = false;
                EnemyHealth health = fixture.GetComponent<EnemyHealth>();
                Rigidbody2D enemyBody = fixture.GetComponent<Rigidbody2D>();
                Require(Physics2D.GetIgnoreCollision(playerCollider, fixture.GetComponent<Collider2D>()),
                    "El fixture de pogo perdió las colisiones sin empuje.");

                PlacePlayer(player, new Vector2(102.3f, 2f), constraints);
                yield return Wait(0.04f); // Refresh grounded state after the fixture teleport.
                float fixedStart = Time.fixedTime;
                float initialVelocity = body.linearVelocity.y;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.S, Key.J));
                yield return Wait(data.AttackWindow + 0.08f);
                float naturalVelocity = initialVelocity + Physics2D.gravity.y * body.gravityScale * (Time.fixedTime - fixedStart);
                Require(health.CurrentHealth == 4 && Mathf.Abs(body.linearVelocity.y - naturalVelocity) < 0.35f,
                    "El pogo fallido rebotó, dañó o añadió impulso vertical a la caída natural.");
                Require((float)ReadField(pogo, "attackTimeRemaining") <= 0f && !(bool)ReadField(pogo, "attackAvailable"),
                    "El pogo fallido no cerró su ventana o devolvió un uso sin impacto.");
                yield return Release(keyboard);

                // Ordinary attacks use the same target with its PogoTarget present.
                // Keeping this fixture in the air separates attack bounce from floor contact.
                enemyBody.gravityScale = 0f;
                enemyBody.constraints = RigidbodyConstraints2D.FreezeAll;
                enemyBody.position = new Vector2(100f, 0.5f);
                fixture.transform.position = enemyBody.position;
                PlacePlayer(player, new Vector2(99.1f, 0.5f), constraints);
                yield return Wait(0.04f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.J));
                yield return Until(() => health.CurrentHealth == 3, 0.4f, "horizontal");
                Require(body.linearVelocity.y < 0f && (float)ReadField(pogo, "attackTimeRemaining") <= 0f,
                    "El ataque horizontal activó rebote de pogo.");
                yield return Release(keyboard, combatData.AttackWindow + 0.04f);
                PlacePlayer(player, new Vector2(100f, -0.4f), constraints);
                yield return Wait(0.04f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.J));
                yield return Until(() => health.CurrentHealth == 2, 0.4f, "ascendente");
                Require(body.linearVelocity.y < 0f && (float)ReadField(pogo, "attackTimeRemaining") <= 0f,
                    "El ataque ascendente activó rebote de pogo.");
                yield return Release(keyboard, combatData.AttackWindow + 0.04f);
                Object.Destroy(fixture);
                fixture = null;
                yield return null;

                foreach (string attackCase in new[] { "Alive", "Vulnerable", "Lethal" })
                {
                    fixture = CreateEnemy();
                    yield return Wait(0.06f);
                    fixture.GetComponent<GroundEnemyController>().enabled = false;
                    health = fixture.GetComponent<EnemyHealth>();
                    EnemyPosture posture = fixture.GetComponent<EnemyPosture>();
                    GroundEnemyData enemyData = AssetDatabase.LoadAssetAtPath<GroundEnemyData>(GroundEnemySceneSetup.DataPath);
                    if (attackCase == "Vulnerable")
                    {
                        while (!posture.IsVulnerable) Require(posture.TryApplyParryDamage(), "No se pudo preparar vulnerabilidad.");
                    }
                    if (attackCase == "Lethal") health.TakeDamage(health.CurrentHealth - 1);

                    // Multiple colliders must still produce only one damage/bounce.
                    GameObject extra = new GameObject("Pogo Validation Extra Collider", typeof(BoxCollider2D));
                    extra.transform.SetParent(fixture.transform, false);
                    extra.layer = fixture.layer;
                    extra.GetComponent<BoxCollider2D>().isTrigger = true;
                    Bounds bounds = fixture.GetComponent<Collider2D>().bounds;
                    int previousHealth = health.CurrentHealth;
                    int expectedDamage = data.Damage * (posture.IsVulnerable ? enemyData.VulnerableDamageMultiplier : 1);
                    int expectedHealth = Mathf.Max(0, previousHealth - expectedDamage);
                    PlacePlayer(player, new Vector2(bounds.center.x, bounds.max.y + playerCollider.bounds.extents.y + 0.12f), constraints);
                    yield return Wait(0.04f);
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.S, Key.J));
                    yield return Until(() => health.CurrentHealth == expectedHealth, 0.4f, attackCase);
                    Require(body.linearVelocity.y > 0f && body.linearVelocity.y <= data.BounceSpeed + 0.01f,
                        $"Pogo {attackCase}: no hubo rebote con la fuerza existente.");
                    Require((float)ReadField(pogo, "attackTimeRemaining") <= 0f && (bool)ReadField(pogo, "attackAvailable"),
                        $"Pogo {attackCase}: no cerró el impacto o no devolvió el uso.");
                    if (attackCase == "Lethal")
                        Require(health.IsDead && !fixture.GetComponent<Rigidbody2D>().simulated, "El pogo letal no completó muerte.");
                    yield return Release(keyboard);
                    Require(health.CurrentHealth == expectedHealth, "Un pogo aplicó daño repetido al mismo enemigo.");
                    Debug.Log($"TASK09_POGO_CASE_PASS: {attackCase}, vida {previousHealth} -> {expectedHealth}, rebote conservado.");
                    Object.Destroy(fixture);
                    fixture = null;
                    yield return null;
                }

                foreach (string targetName in new[] { "Pogo Target - First", "Pogo Target - Second" })
                {
                    PogoTarget target = GameObject.Find(targetName).GetComponent<PogoTarget>();
                    Bounds bounds = target.GetComponent<Collider2D>().bounds;
                    PlacePlayer(player, new Vector2(bounds.center.x, bounds.max.y + playerCollider.bounds.extents.y + 0.12f), constraints);
                    yield return Wait(0.04f);
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.S, Key.J));
                    yield return Until(() => body.linearVelocity.y > 0f, 0.4f, targetName);
                    Require(body.linearVelocity.y <= data.BounceSpeed + 0.01f, "Cambió la fuerza sobre los objetivos anteriores.");
                    yield return Release(keyboard);
                }
                Debug.Log("TASK09_POGO_PASS: vivo, vulnerable, letal, impacto único, fallo con gravedad natural, horizontal/ascendente sin rebote y ambos objetivos anteriores.");
            }
            finally
            {
                if (fixture != null) Object.Destroy(fixture);
                Object.Destroy(platform);
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

        private static GameObject CreateEnemy()
        {
            return Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(GroundEnemySceneSetup.PrefabPath),
                new Vector3(100f, -1.58f), Quaternion.identity);
        }

        private static void PlacePlayer(PlayerHealth player, Vector2 position, RigidbodyConstraints2D constraints)
        {
            player.RestartForTesting();
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            body.constraints = constraints;
            body.position = position;
            player.transform.position = position;
            body.linearVelocity = Vector2.down * 2f;
            Physics2D.SyncTransforms();
        }

        private static IEnumerator Release(Keyboard keyboard, float duration = 0.06f)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return Wait(duration);
        }

        private static IEnumerator Wait(float seconds)
        {
            float end = Time.time + seconds;
            while (Time.time < end) yield return null;
        }

        private static IEnumerator Until(Func<bool> condition, float timeout, string context)
        {
            float end = Time.time + timeout;
            while (!condition())
            {
                if (Time.time > end) throw new TimeoutException($"Pogo: no se alcanzó la condición esperada ({context}).");
                yield return null;
            }
        }

        private static object ReadField(object target, string field) => target.GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
