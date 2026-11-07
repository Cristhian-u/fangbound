using System;
using System.IO;
using Fangbound.CameraSystem;
using Fangbound.Player;
using Fangbound.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fangbound.EditorTools
{
    public static class MemoryFlameSceneSetup
    {
        internal const string DataPath = "Assets/_Project/Data/World/MemoryFlameData.asset";
        internal const string PrefabPath = "Assets/_Project/Prefabs/World/MemoryFlame.prefab";

        [MenuItem("Fangbound/Configure Movement Test Memory Flames")]
        public static void Configure()
        {
            var scene = EditorSceneManager.OpenScene(GroundEnemySceneSetup.ScenePath, OpenSceneMode.Single);
            PlayerHealth player = UnityEngine.Object.FindAnyObjectByType<PlayerHealth>();
            CameraFollow2D camera = UnityEngine.Object.FindAnyObjectByType<CameraFollow2D>();
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Settings/InputSystem_Actions.inputactions");
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Placeholder/MovementTestSquare.asset");
            if (player == null || camera == null || actions == null || sprite == null)
                throw new InvalidOperationException("MovementTest necesita jugador, cámara, Input Actions y sprite existentes.");
            actions.FindActionMap("Player", true).FindAction("Interact", true);
            MemoryFlameData data = AssetDatabase.LoadAssetAtPath<MemoryFlameData>(DataPath);
            if (data == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DataPath));
                AssetDatabase.Refresh();
                data = ScriptableObject.CreateInstance<MemoryFlameData>();
                AssetDatabase.CreateAsset(data, DataPath);
            }
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
                AssetDatabase.Refresh();
                GameObject root = CreateFlame(data, sprite);
                prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                UnityEngine.Object.DestroyImmediate(root);
            }
            PlayerCheckpoint checkpoints = player.GetComponent<PlayerCheckpoint>();
            if (checkpoints == null) checkpoints = player.gameObject.AddComponent<PlayerCheckpoint>();
            SetReference(checkpoints, "inputActions", actions);
            SetReference(checkpoints, "cameraFollow", camera);
            GameObject group = GameObject.Find("Memory Flames");
            if (group == null) group = new GameObject("Memory Flames");
            Place(prefab, group.transform, "Memory Flame - Start", new Vector3(-7.9f, -2.1f));
            Place(prefab, group.transform, "Memory Flame - Upper", new Vector3(5.8f, 2.725f));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            ValidateScene();
            Debug.Log("TASK11_SETUP_PASS: dos Llamas del Recuerdo y respawn enlazados; controles y geometría anteriores conservados.");
        }

        private static void Place(GameObject prefab, Transform parent, string name, Vector3 position)
        {
            if (GameObject.Find(name) != null) return;
            GameObject flame = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            flame.name = name;
            flame.transform.position = position;
        }

        private static GameObject CreateFlame(MemoryFlameData data, Sprite sprite)
        {
            GameObject root = new GameObject("MemoryFlame");
            CreateVisual(root.transform, "Stone Base", sprite, new Vector2(0f, 0.07f), new Vector2(0.85f, 0.14f), new Color(0.23f, 0.28f, 0.29f), 1);
            CreateVisual(root.transform, "Stone Pedestal", sprite, new Vector2(0f, 0.2f), new Vector2(0.48f, 0.25f), new Color(0.35f, 0.41f, 0.41f), 1);
            CreateVisual(root.transform, "Stone Altar", sprite, new Vector2(0f, 0.34f), new Vector2(0.68f, 0.1f), new Color(0.46f, 0.52f, 0.51f), 1);
            Color glowColor = data.InactiveColor;
            glowColor.a = data.InactiveGlowAlpha;
            SpriteRenderer glow = CreateVisual(root.transform, "Flame Glow", sprite, new Vector2(0f, 0.65f), new Vector2(0.6f, 0.65f), glowColor, 2);
            SpriteRenderer flame = CreateVisual(root.transform, "Floating Flame", sprite, new Vector2(0f, 0.65f), new Vector2(0.22f, 0.32f), data.InactiveColor, 3);
            flame.transform.localRotation = Quaternion.Euler(0f, 0f, 35f);
            Transform spawn = new GameObject("Respawn Point").transform;
            spawn.SetParent(root.transform, false);
            spawn.localPosition = new Vector3(-0.6f, 0.58f);
            MemoryFlame checkpoint = root.AddComponent<MemoryFlame>();
            SetReference(checkpoint, "flameData", data);
            SetReference(checkpoint, "respawnPoint", spawn);
            SetReference(checkpoint, "flameRenderer", flame);
            SetReference(checkpoint, "glowRenderer", glow);
            return root;
        }

        private static SpriteRenderer CreateVisual(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 scale, Color color, int order)
        {
            GameObject visual = new GameObject(name, typeof(SpriteRenderer));
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = position;
            visual.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }

        private static void SetReference(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void ValidateScene()
        {
            PlayerCheckpoint player = UnityEngine.Object.FindAnyObjectByType<PlayerCheckpoint>();
            if (player == null) throw new InvalidOperationException("Falta PlayerCheckpoint.");
            foreach (string name in new[] { "Memory Flame - Start", "Memory Flame - Upper" })
            {
                GameObject flame = GameObject.Find(name);
                if (flame == null || flame.GetComponent<MemoryFlame>() == null || !PrefabUtility.IsPartOfPrefabInstance(flame) ||
                    flame.GetComponentsInChildren<Collider2D>().Length != 0)
                    throw new InvalidOperationException($"Llama inválida o con colisiones no previstas: {name}");
            }
        }
    }
}
