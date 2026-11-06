using System;
using System.IO;
using Fangbound.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Fangbound.EditorTools
{
    public static class PogoTestSceneSetup
    {
        private const string ScenePath = "Assets/_Project/Scenes/Test/MovementTest.unity";
        private const string DataPath = "Assets/_Project/Data/Player/PlayerPogoData.asset";
        private const string InputPath = "Assets/Settings/InputSystem_Actions.inputactions";
        private const string SpritePath = "Assets/_Project/Art/Placeholder/MovementTestSquare.asset";

        [MenuItem("Fangbound/Configure Movement Test Pogo")]
        public static void Configure()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject player = GameObject.Find("Salamander Placeholder");
            GameObject environment = GameObject.Find("Environment");
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);

            if (player == null || environment == null || actions == null || sprite == null ||
                player.GetComponent<PlayerController>() == null)
            {
                throw new InvalidOperationException("Faltan componentes o assets de MovementTest para configurar el pogo.");
            }

            InputActionMap map = actions.FindActionMap("Player", true);
            if (map.FindAction("Attack", true).bindings.Count == 0 || map.FindAction("Move", true).bindings.Count == 0)
            {
                throw new InvalidOperationException("Faltan bindings de Attack o Move.");
            }

            PlayerPogoData data = AssetDatabase.LoadAssetAtPath<PlayerPogoData>(DataPath);
            if (data == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DataPath));
                data = ScriptableObject.CreateInstance<PlayerPogoData>();
                AssetDatabase.CreateAsset(data, DataPath);
            }

            PlayerPogo pogo = player.GetComponent<PlayerPogo>();
            if (pogo == null) pogo = player.AddComponent<PlayerPogo>();
            SerializedObject serializedPogo = new SerializedObject(pogo);
            serializedPogo.FindProperty("pogoData").objectReferenceValue = data;
            serializedPogo.FindProperty("inputActions").objectReferenceValue = actions;
            serializedPogo.ApplyModifiedPropertiesWithoutUndo();

            CreateTarget(environment.transform, sprite, "Pogo Target - First", new Vector2(-2.5f, 0.25f));
            CreateTarget(environment.transform, sprite, "Pogo Target - Second", new Vector2(0.4f, 2.1f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();

            if (player.GetComponent<PlayerPogo>() == null ||
                GameObject.Find("Pogo Target - First") == null ||
                GameObject.Find("Pogo Target - Second") == null)
            {
                throw new InvalidOperationException("No se guardó la configuración de pogo en MovementTest.");
            }

            Debug.Log("Pogo configurado en MovementTest con dos objetivos de prueba.");
        }

        private static void CreateTarget(Transform parent, Sprite sprite, string name, Vector2 position)
        {
            GameObject target = GameObject.Find(name);
            if (target == null)
            {
                target = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(PogoTarget));
                target.transform.SetParent(parent, false);
            }

            target.transform.position = new Vector3(position.x, position.y, 0f);
            target.transform.localScale = new Vector3(1.1f, 0.3f, 1f);
            SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.9f, 0.65f, 0.2f);
            BoxCollider2D collider = target.GetComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            collider.isTrigger = true;
            if (target.GetComponent<PogoTarget>() == null) target.AddComponent<PogoTarget>();
        }
    }
}
