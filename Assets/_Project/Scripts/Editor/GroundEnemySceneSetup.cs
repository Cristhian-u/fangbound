using System;
using System.IO;
using Fangbound.Combat;
using Fangbound.Enemies;
using Fangbound.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Fangbound.EditorTools
{
    public static class GroundEnemySceneSetup
    {
        internal const string ScenePath = "Assets/_Project/Scenes/Test/MovementTest.unity";
        internal const string PrefabPath = "Assets/_Project/Prefabs/Enemies/GroundEnemy.prefab";
        internal const string DataPath = "Assets/_Project/Data/Enemies/GroundEnemyData.asset";
        internal const string BodyLayerName = "EnemyBody";

        [MenuItem("Fangbound/Configure Movement Test Ground Enemy")]
        public static void Configure()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject environment = GameObject.Find("Environment");
            PlayerHealth player = UnityEngine.Object.FindAnyObjectByType<PlayerHealth>();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Placeholder/MovementTestSquare.asset");
            EnemyPostureData postureData = AssetDatabase.LoadAssetAtPath<EnemyPostureData>("Assets/_Project/Data/Enemies/EnemyPostureData.asset");
            TrainingAttackData trainingData = AssetDatabase.LoadAssetAtPath<TrainingAttackData>("Assets/_Project/Data/Enemies/TrainingAttackData.asset");
            if (environment == null || player == null || sprite == null || postureData == null || trainingData == null)
                throw new InvalidOperationException("MovementTest necesita el contenido de las tareas 07/08.");

            // Migrate only the trainer. Do not run the broader player/combat configurator.
            CombatTestSceneSetup.ConfigureParryTrainingTarget(environment.transform, sprite, postureData, trainingData);

            GroundEnemyData data = AssetDatabase.LoadAssetAtPath<GroundEnemyData>(DataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<GroundEnemyData>();
                AssetDatabase.CreateAsset(data, DataPath);
            }
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
                AssetDatabase.Refresh();
                GameObject root = CreateEnemy(sprite, data, postureData);
                prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                UnityEngine.Object.DestroyImmediate(root);
            }

            GameObject enemy = GameObject.Find("Ground Enemy");
            if (enemy == null)
            {
                enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefab, environment.transform);
                enemy.name = "Ground Enemy";
                enemy.transform.position = new Vector3(4.5f, -1.58f, 0f);
            }
            SetReference(enemy.GetComponent<GroundEnemyController>(), "target", player);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            ValidateScene();
            Debug.Log("TASK09_SETUP_PASS: prefab terrestre, entrenador migrado y MovementTest verificados.");
        }

        private static GameObject CreateEnemy(Sprite sprite, GroundEnemyData data, EnemyPostureData postureData)
        {
            GameObject root = new GameObject("Ground Enemy", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D));
            int bodyLayer = LayerMask.NameToLayer(BodyLayerName);
            if (bodyLayer < 0) throw new InvalidOperationException("Falta la capa EnemyBody.");
            root.layer = bodyLayer;
            SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.6f, 0.3f, 0.55f);
            renderer.sortingOrder = 4;
            Rigidbody2D body = root.GetComponent<Rigidbody2D>();
            body.gravityScale = 3f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            root.GetComponent<BoxCollider2D>().size = new Vector2(0.8f, 1f);
            root.GetComponent<BoxCollider2D>().sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(
                "Assets/_Project/Data/Player/PlayerNoFriction.physicsMaterial2D");
            root.AddComponent<PogoTarget>();

            SpriteRenderer postureIndicator = CreateVisual(root.transform, "Posture Indicator", sprite,
                new Vector3(0f, 0.68f, -0.1f), new Vector3(0.7f, 0.12f, 1f), new Color(0.5f, 0.75f, 1f));
            SpriteRenderer facingIndicator = CreateVisual(root.transform, "Enemy Facing Direction", sprite,
                new Vector3(0.53f, 0.15f, -0.1f), new Vector3(0.2f, 0.12f, 1f), Color.white);
            SpriteRenderer telegraph = CreateVisual(root.transform, "Enemy Attack Hitbox", sprite,
                new Vector3(data.HitboxOffset.x, data.HitboxOffset.y, -0.1f),
                new Vector3(data.HitboxSize.x, data.HitboxSize.y, 1f), data.TelegraphColor);
            telegraph.enabled = false;
            BoxCollider2D hitboxCollider = telegraph.gameObject.AddComponent<BoxCollider2D>();
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false;
            EnemyPosture posture = root.AddComponent<EnemyPosture>();
            SetReference(posture, "postureData", postureData);
            SetReference(posture, "postureIndicator", postureIndicator);
            EnemyAttackHitbox hitbox = telegraph.gameObject.AddComponent<EnemyAttackHitbox>();
            EnemyHealth health = root.AddComponent<EnemyHealth>();
            SetReference(health, "enemyData", data);
            GroundEnemyController controller = root.AddComponent<GroundEnemyController>();
            SetReference(controller, "enemyData", data);
            SetReference(controller, "attackHitbox", hitbox);
            SetReference(controller, "telegraphRenderer", telegraph);
            SetReference(controller, "facingIndicator", facingIndicator.transform);
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("patrolOffsets").vector2Value = new Vector2(-3.1f, 2.3f);
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        private static SpriteRenderer CreateVisual(Transform parent, string name, Sprite sprite, Vector3 position, Vector3 scale, Color color)
        {
            GameObject visual = new GameObject(name, typeof(SpriteRenderer));
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = position;
            visual.transform.localScale = scale;
            SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = 5;
            return renderer;
        }

        private static void SetReference(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null) throw new InvalidOperationException($"Falta el campo {field} en {target}.");
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void ValidateScene()
        {
            GroundEnemyController enemy = UnityEngine.Object.FindAnyObjectByType<GroundEnemyController>();
            if (enemy == null || enemy.GetComponent<PogoTarget>() == null || enemy.GetComponent<IDamageable>() == null || enemy.GetComponent<EnemyPosture>() == null ||
                enemy.GetComponentInChildren<EnemyAttackHitbox>() == null || !PrefabUtility.IsPartOfPrefabInstance(enemy))
                throw new InvalidOperationException("El enemigo terrestre no tiene prefab o contratos completos.");
            if (enemy.gameObject.layer != LayerMask.NameToLayer(BodyLayerName) || enemy.GetComponent<BoxCollider2D>().isTrigger)
                throw new InvalidOperationException("El cuerpo terrestre debe ser sólido y usar EnemyBody.");
            PlayerController player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            int playerGroundMask = new SerializedObject(player).FindProperty("groundLayers").intValue;
            if ((playerGroundMask & (1 << enemy.gameObject.layer)) != 0)
                throw new InvalidOperationException("EnemyBody no debe formar parte del suelo del jugador.");
            TrainingAttackEmitter trainer = UnityEngine.Object.FindAnyObjectByType<TrainingAttackEmitter>();
            SerializedObject emitter = new SerializedObject(trainer);
            EnemyAttackHitbox hitbox = emitter.FindProperty("attackHitbox").objectReferenceValue as EnemyAttackHitbox;
            if (hitbox == null || hitbox.GetComponent<BoxCollider2D>().enabled)
                throw new InvalidOperationException("El entrenador no tiene una hitbox migrada válida.");
            if (GameObject.Find("Pogo Target - First")?.GetComponent<PogoTarget>() == null ||
                GameObject.Find("Pogo Target - Second")?.GetComponent<PogoTarget>() == null ||
                UnityEngine.Object.FindObjectsByType<TrainingTarget>().Length != 2)
                throw new InvalidOperationException("MovementTest perdió objetivos anteriores.");
            foreach (GameObject root in enemy.gameObject.scene.GetRootGameObjects())
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) != 0)
                        throw new InvalidOperationException($"Script faltante: {child.name}");
        }
    }
}
