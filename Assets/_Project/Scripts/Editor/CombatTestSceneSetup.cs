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

            PlayerCombatData combatData = AssetDatabase.LoadAssetAtPath<PlayerCombatData>(CombatDataPath);
            if (combatData == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CombatDataPath));
                combatData = ScriptableObject.CreateInstance<PlayerCombatData>();
                AssetDatabase.CreateAsset(combatData, CombatDataPath);
            }

            ConfigureCombatData(combatData);

            PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
            if (playerCombat == null)
            {
                playerCombat = player.AddComponent<PlayerCombat>();
            }

            SerializedObject serializedCombat = new SerializedObject(playerCombat);
            SetObjectReference(serializedCombat, "combatData", combatData);
            SetObjectReference(serializedCombat, "inputActions", inputActions);
            serializedCombat.ApplyModifiedPropertiesWithoutUndo();

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

            EditorUtility.SetDirty(playerCombat);
            EditorUtility.SetDirty(combatData);
            EditorUtility.SetDirty(pogoData);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();

            VerifyCombatSetup(player, playerCombat, combatData, inputActions, pogoData);
            Debug.Log("Combate básico configurado en MovementTest con objetivos de entrenamiento.");
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
            PlayerPogoData pogoData)
        {
            PlayerPogo playerPogo = player.GetComponent<PlayerPogo>();
            if (playerPogo == null || player.GetComponent<PlayerController>() == null ||
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

            SerializedObject serializedPogo = new SerializedObject(playerPogo);
            if (serializedPogo.FindProperty("pogoData").objectReferenceValue != pogoData ||
                serializedPogo.FindProperty("inputActions").objectReferenceValue != inputActions)
            {
                throw new InvalidOperationException("PlayerPogo perdió sus referencias serializadas.");
            }
        }
    }
}