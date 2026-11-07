using System;
using System.IO;
using Fangbound.Combat;
using Fangbound.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Fangbound.EditorTools
{
    public static class CombatTestSceneSetup
    {
        private const string ScenePath = "Assets/_Project/Scenes/Test/MovementTest.unity";
        private const string CombatDataPath = "Assets/_Project/Data/Player/PlayerCombatData.asset";
        private const string ParryDataPath = "Assets/_Project/Data/Player/PlayerParryData.asset";
        private const string EnemyPostureDataPath = "Assets/_Project/Data/Enemies/EnemyPostureData.asset";
        private const string TrainingAttackDataPath = "Assets/_Project/Data/Enemies/TrainingAttackData.asset";
        private const string InputActionsPath = "Assets/Settings/InputSystem_Actions.inputactions";
        private const string SpritePath = "Assets/_Project/Art/Placeholder/MovementTestSquare.asset";

        [MenuItem("Fangbound/Configure Movement Test Combat")]
        public static void Configure()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject player = GameObject.Find("Salamander Placeholder");
            GameObject environment = GameObject.Find("Environment");
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            Sprite placeholderSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            PlayerPogoData pogoData = AssetDatabase.LoadAssetAtPath<PlayerPogoData>(
                "Assets/_Project/Data/Player/PlayerPogoData.asset");

            if (player == null || environment == null || inputActions == null || placeholderSprite == null || pogoData == null ||
                player.GetComponent<PlayerController>() == null || player.GetComponent<PlayerPogo>() == null)
            {
                throw new InvalidOperationException("MovementTest no tiene los componentes, assets o sistema de pogo requeridos.");
            }

            InputActionMap playerMap = inputActions.FindActionMap("Player", true);
            playerMap.FindAction("Move", true);
            playerMap.FindAction("Attack", true);
            playerMap.FindAction("Parry", true);

            PlayerCombatData combatData = AssetDatabase.LoadAssetAtPath<PlayerCombatData>(CombatDataPath);
            if (combatData == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CombatDataPath));
                combatData = ScriptableObject.CreateInstance<PlayerCombatData>();
                AssetDatabase.CreateAsset(combatData, CombatDataPath);
            }

            ConfigureCombatData(combatData);

            PlayerParryData parryData = LoadOrCreateData<PlayerParryData>(ParryDataPath);
            ConfigureParryData(parryData);

            EnemyPostureData enemyPostureData = LoadOrCreateData<EnemyPostureData>(EnemyPostureDataPath);
            ConfigureEnemyPostureData(enemyPostureData);

            TrainingAttackData trainingAttackData = LoadOrCreateData<TrainingAttackData>(TrainingAttackDataPath);
            ConfigureTrainingAttackData(trainingAttackData);

            PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
            if (playerCombat == null)
            {
                playerCombat = player.AddComponent<PlayerCombat>();
            }

            SerializedObject serializedCombat = new SerializedObject(playerCombat);
            SetObjectReference(serializedCombat, "combatData", combatData);
            SetObjectReference(serializedCombat, "inputActions", inputActions);
            serializedCombat.ApplyModifiedPropertiesWithoutUndo();

            PlayerParry playerParry = player.GetComponent<PlayerParry>();
            if (playerParry == null)
            {
                playerParry = player.AddComponent<PlayerParry>();
            }

            SerializedObject serializedParry = new SerializedObject(playerParry);
            SetObjectReference(serializedParry, "parryData", parryData);
            SetObjectReference(serializedParry, "inputActions", inputActions);
            serializedParry.ApplyModifiedPropertiesWithoutUndo();

            PlayerImpactReceiver impactReceiver = player.GetComponent<PlayerImpactReceiver>();
            if (impactReceiver == null)
            {
                impactReceiver = player.AddComponent<PlayerImpactReceiver>();
            }

            ConfigureTrainingTarget(
                environment.transform,
                placeholderSprite,
                "Training Target - Horizontal",
                new Vector2(-6f, -1.2f),
                new Color(0.82f, 0.39f, 0.24f));
            ConfigureTrainingTarget(
                environment.transform,
                placeholderSprite,
                "Training Target - Upward",
                new Vector2(-6f, -0.15f),
                new Color(0.32f, 0.58f, 0.67f));

            ConfigureParryTrainingTarget(environment.transform, placeholderSprite, enemyPostureData, trainingAttackData);

            EditorUtility.SetDirty(playerCombat);
            EditorUtility.SetDirty(combatData);
            EditorUtility.SetDirty(playerParry);
            EditorUtility.SetDirty(impactReceiver);
            EditorUtility.SetDirty(pogoData);
            EditorUtility.SetDirty(parryData);
            EditorUtility.SetDirty(enemyPostureData);
            EditorUtility.SetDirty(trainingAttackData);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();

            VerifyCombatSetup(
                player,
                playerCombat,
                combatData,
                inputActions,
                pogoData,
                playerParry,
                parryData,
                enemyPostureData,
                trainingAttackData);
            Debug.Log("Combate básico configurado en MovementTest con objetivos de entrenamiento.");
        }

        private static T LoadOrCreateData<T>(string assetPath) where T : ScriptableObject
        {
            T data = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (data != null)
            {
                return data;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            data = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(data, assetPath);
            return data;
        }

        private static void ConfigureCombatData(PlayerCombatData combatData)
        {
            SerializedObject serializedData = new SerializedObject(combatData);
            serializedData.FindProperty("damage").intValue = 1;
            serializedData.FindProperty("attackWindow").floatValue = 0.12f;
            serializedData.FindProperty("horizontalHitboxOffset").vector2Value = new Vector2(0.65f, 0.05f);
            serializedData.FindProperty("horizontalHitboxSize").vector2Value = new Vector2(0.85f, 0.55f);
            serializedData.FindProperty("upwardHitboxOffset").vector2Value = new Vector2(0f, 0.75f);
            serializedData.FindProperty("upwardHitboxSize").vector2Value = new Vector2(0.6f, 0.8f);
            serializedData.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureParryData(PlayerParryData parryData)
        {
            SerializedObject serializedData = new SerializedObject(parryData);
            serializedData.FindProperty("windowDuration").floatValue = 0.15f;
            serializedData.FindProperty("recoveryDuration").floatValue = 0.4f;
            serializedData.FindProperty("feedbackDuration").floatValue = 0.12f;
            serializedData.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureEnemyPostureData(EnemyPostureData postureData)
        {
            SerializedObject serializedData = new SerializedObject(postureData);
            serializedData.FindProperty("maximumPosture").intValue = 3;
            serializedData.FindProperty("parryDamage").intValue = 1;
            serializedData.FindProperty("vulnerableDuration").floatValue = 1.5f;
            serializedData.FindProperty("recoveryPerSecond").floatValue = 1f;
            serializedData.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureTrainingAttackData(TrainingAttackData attackData)
        {
            SerializedObject serializedData = new SerializedObject(attackData);
            serializedData.FindProperty("initialDelay").floatValue = 0.75f;
            serializedData.FindProperty("telegraphDuration").floatValue = 0.65f;
            serializedData.FindProperty("activeDuration").floatValue = 0.14f;
            serializedData.FindProperty("cooldown").floatValue = 1.3f;
            serializedData.FindProperty("hitboxOffset").vector2Value = new Vector2(-1.8f, -0.05f);
            serializedData.FindProperty("hitboxSize").vector2Value = new Vector2(3.6f, 0.75f);
            serializedData.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureParryTrainingTarget(
            Transform parent,
            Sprite sprite,
            EnemyPostureData postureData,
            TrainingAttackData attackData)
        {
            GameObject target = GameObject.Find("Parry Training Target");
            if (target == null)
            {
                target = new GameObject("Parry Training Target", typeof(SpriteRenderer), typeof(BoxCollider2D));
                target.transform.SetParent(parent, false);
            }

            target.transform.localPosition = new Vector3(-3.8f, -1.35f, 0f);
            target.transform.localScale = new Vector3(0.7f, 1f, 1f);

            SpriteRenderer targetRenderer = target.GetComponent<SpriteRenderer>();
            targetRenderer.sprite = sprite;
            targetRenderer.color = new Color(0.62f, 0.34f, 0.3f, 1f);
            targetRenderer.sortingOrder = 4;

            BoxCollider2D targetCollider = target.GetComponent<BoxCollider2D>();
            targetCollider.size = Vector2.one;
            targetCollider.isTrigger = true;

            GameObject telegraphObject = GameObject.Find("Attack Telegraph");
            if (telegraphObject == null)
            {
                telegraphObject = new GameObject("Attack Telegraph", typeof(SpriteRenderer));
                telegraphObject.transform.SetParent(target.transform, false);
            }

            telegraphObject.transform.localPosition = new Vector3(attackData.HitboxOffset.x, attackData.HitboxOffset.y, -0.1f);
            telegraphObject.transform.localScale = new Vector3(attackData.HitboxSize.x, attackData.HitboxSize.y, 1f);
            SpriteRenderer telegraphRenderer = telegraphObject.GetComponent<SpriteRenderer>();
            telegraphRenderer.sprite = sprite;
            telegraphRenderer.color = attackData.TelegraphColor;
            telegraphRenderer.sortingOrder = 5;
            telegraphRenderer.enabled = false;

            EnemyPosture posture = target.GetComponent<EnemyPosture>();
            if (posture == null)
            {
                posture = target.AddComponent<EnemyPosture>();
            }

            SerializedObject serializedPosture = new SerializedObject(posture);
            SetObjectReference(serializedPosture, "postureData", postureData);
            serializedPosture.ApplyModifiedPropertiesWithoutUndo();

            TrainingAttackEmitter emitter = target.GetComponent<TrainingAttackEmitter>();
            if (emitter == null)
            {
                emitter = target.AddComponent<TrainingAttackEmitter>();
            }

            SerializedObject serializedEmitter = new SerializedObject(emitter);
            SetObjectReference(serializedEmitter, "attackData", attackData);
            SetObjectReference(serializedEmitter, "posture", posture);
            SetObjectReference(serializedEmitter, "telegraphRenderer", telegraphRenderer);
            serializedEmitter.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(targetRenderer);
            EditorUtility.SetDirty(targetCollider);
            EditorUtility.SetDirty(posture);
            EditorUtility.SetDirty(emitter);
            EditorUtility.SetDirty(telegraphRenderer);
        }

        private static void ConfigureTrainingTarget(
            Transform parent,
            Sprite sprite,
            string name,
            Vector2 position,
            Color color)
        {
            GameObject target = GameObject.Find(name);
            if (target == null)
            {
                target = new GameObject(name, typeof(Transform), typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(TrainingTarget));
            }

            target.transform.SetParent(parent, true);
            target.transform.position = position;
            target.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = 4;

            BoxCollider2D boxCollider = target.GetComponent<BoxCollider2D>();
            boxCollider.size = Vector2.one;
            boxCollider.isTrigger = true;

            TrainingTarget trainingTarget = target.GetComponent<TrainingTarget>();
            SerializedObject serializedTarget = new SerializedObject(trainingTarget);
            serializedTarget.FindProperty("maximumHealth").intValue = 3;
            serializedTarget.FindProperty("hitFlashDuration").floatValue = 0.1f;
            serializedTarget.FindProperty("hitFlashColor").colorValue = Color.white;
            serializedTarget.FindProperty("depletedColor").colorValue = new Color(0.25f, 0.2f, 0.16f, 1f);
            serializedTarget.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"No existe el campo serializado '{propertyName}'.");
            }

            property.objectReferenceValue = value;
        }

        private static void VerifyCombatSetup(
            GameObject player,
            PlayerCombat playerCombat,
            PlayerCombatData combatData,
            InputActionAsset inputActions,
            PlayerPogoData pogoData,
            PlayerParry playerParry,
            PlayerParryData parryData,
            EnemyPostureData postureData,
            TrainingAttackData attackData)
        {
            PlayerPogo playerPogo = player.GetComponent<PlayerPogo>();
            PlayerImpactReceiver impactReceiver = player.GetComponent<PlayerImpactReceiver>();
            GameObject parryTarget = GameObject.Find("Parry Training Target");
            GameObject telegraphObject = GameObject.Find("Attack Telegraph");
            EnemyPosture enemyPosture = parryTarget != null ? parryTarget.GetComponent<EnemyPosture>() : null;
            TrainingAttackEmitter attackEmitter = parryTarget != null ? parryTarget.GetComponent<TrainingAttackEmitter>() : null;
            if (playerPogo == null || playerParry == null || impactReceiver == null ||
                player.GetComponent<PlayerController>() == null || player.GetComponent<PlayerCombat>() == null ||
                enemyPosture == null || attackEmitter == null || telegraphObject == null ||
                GameObject.Find("Pogo Target - First")?.GetComponent<PogoTarget>() == null ||
                GameObject.Find("Pogo Target - Second")?.GetComponent<PogoTarget>() == null ||
                GameObject.Find("Training Target - Horizontal")?.GetComponent<IDamageable>() == null ||
                GameObject.Find("Training Target - Upward")?.GetComponent<IDamageable>() == null)
            {
                throw new InvalidOperationException("Faltan componentes de combate, pogo o objetivos de entrenamiento.");
            }

            SerializedObject serializedCombat = new SerializedObject(playerCombat);
            if (serializedCombat.FindProperty("combatData").objectReferenceValue != combatData ||
                serializedCombat.FindProperty("inputActions").objectReferenceValue != inputActions)
            {
                throw new InvalidOperationException("PlayerCombat perdió sus referencias serializadas.");
            }

            SerializedObject serializedParry = new SerializedObject(playerParry);
            if (serializedParry.FindProperty("parryData").objectReferenceValue != parryData ||
                serializedParry.FindProperty("inputActions").objectReferenceValue != inputActions)
            {
                throw new InvalidOperationException("PlayerParry perdió sus referencias serializadas.");
            }

            SerializedObject serializedPogo = new SerializedObject(playerPogo);
            if (serializedPogo.FindProperty("pogoData").objectReferenceValue != pogoData ||
                serializedPogo.FindProperty("inputActions").objectReferenceValue != inputActions)
            {
                throw new InvalidOperationException("PlayerPogo perdió sus referencias serializadas.");
            }

            SerializedObject serializedPosture = new SerializedObject(enemyPosture);
            SerializedObject serializedEmitter = new SerializedObject(attackEmitter);
            if (serializedPosture.FindProperty("postureData").objectReferenceValue != postureData ||
                serializedEmitter.FindProperty("attackData").objectReferenceValue != attackData ||
                serializedEmitter.FindProperty("posture").objectReferenceValue != enemyPosture ||
                serializedEmitter.FindProperty("telegraphRenderer").objectReferenceValue != telegraphObject.GetComponent<SpriteRenderer>())
            {
                throw new InvalidOperationException("El entrenador de parry perdió sus referencias de datos.");
            }
        }
    }
}