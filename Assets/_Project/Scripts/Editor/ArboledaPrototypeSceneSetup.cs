using System;
using System.IO;
using Fangbound.CameraSystem;
using Fangbound.Enemies;
using Fangbound.Player;
using Fangbound.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Fangbound.EditorTools
{
    // One-shot authoring helper. The saved scene has no dependency on this script.
    public static class ArboledaPrototypeSceneSetup
    {
        internal const string ScenePath = "Assets/_Project/Scenes/Test/ArboledaPrototype.unity";
        private static Sprite square;
        private static readonly Color earth = new Color(0.16f, 0.25f, 0.23f);
        private static readonly Color moss = new Color(0.31f, 0.50f, 0.34f);
        private static readonly Color stone = new Color(0.30f, 0.38f, 0.38f);
        private static readonly Color ink = new Color(0.67f, 0.79f, 0.70f);

        [MenuItem("Fangbound/Create Arboleda Prototype (once)")]
        public static void Generate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(ScenePath))
                throw new InvalidOperationException("Crear solo desde Edit Mode y sin escena destino existente; no se sobrescribe trabajo de nivel.");
            var source = EditorSceneManager.OpenScene(GroundEnemySceneSetup.ScenePath, OpenSceneMode.Single);
            if (!EditorSceneManager.SaveScene(source, ScenePath, true))
                throw new InvalidOperationException("No se pudo guardar la copia independiente.");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Placeholder/MovementTestSquare.asset");
            if (square == null) throw new InvalidOperationException("Falta sprite provisional existente.");
            UnityEngine.Object.DestroyImmediate(GameObject.Find("Environment"));
            UnityEngine.Object.DestroyImmediate(GameObject.Find("Memory Flames"));

            Transform geometry = Group("Level Geometry");
            Transform safety = Group("Safety Floor and Limits", geometry);
            Solid("Safety Floor", safety, 40f, -2f, 80f, 4f, earth);
            Solid("West Boundary", safety, -0.3f, 6f, 0.6f, 20f, earth);
            Solid("East Boundary", safety, 80.3f, 6f, 0.6f, 20f, earth);
            Transform main = Group("Main Route", geometry);
            Transform start = Group("A - Safe Start", main);
            Block("A1", start, 9f, 4f, 0.9f);
            Block("A2", start, 15f, 5f, 1.8f);
            Transform fork = Group("B - Platforms and Fork", main);
            Block("B Mesa", fork, 24.5f, 10f, 2.4f);
            Block("B Lower 1", fork, 33f, 4f, 1.4f);
            Block("B Lower 2", fork, 39f, 5f, 1.3f);
            Block("B Return Step", fork, 42.4f, 2f, 1.1f);
            Transform arena = Group("C - Checkpoint and Arena", main);
            Block("C Arena", arena, 53.5f, 19f, 2.4f);
            Transform finish = Group("D - Finish", main);
            Block("D Step", finish, 66f, 4f, 1.5f);
            Block("D Deck", finish, 74f, 10f, 1.8f);
            Transform optional = Group("Optional Upper Route - Four Platforms", geometry);
            Platform("Upper 1", optional, 26f, 3.4f, 3.9f);
            Platform("Upper 2", optional, 31f, 3.8f, 5.4f);
            Platform("Upper 3", optional, 36f, 4f, 6.9f);
            Platform("Upper Discovery", optional, 41.5f, 6f, 8.4f);

            Transform decoration = Group("Provisional Decoration");
            Transform forest = Group("Forest Silhouettes", decoration);
            for (int i = 0; i < 13; i++)
            {
                float x = 1.5f + i * 6.3f;
                float height = 7f + (i % 3) * 2f;
                Visual("Distant Trunk", forest, x, height * 0.5f, 0.45f, height, new Color(0.09f, 0.16f, 0.16f), -8);
                Visual("Distant Canopy", forest, x, height, 5.5f, 1.6f, new Color(0.10f, 0.20f, 0.18f), -9);
            }
            Transform ruin = Group("Ancient Ruin - Decoration Only", decoration);
            Visual("Broken Column Left", ruin, 39.5f, 9.05f, 0.35f, 1.3f, stone);
            Visual("Broken Capital", ruin, 39.6f, 9.72f, 0.7f, 0.2f, stone).transform.rotation = Quaternion.Euler(0, 0, -10);
            Visual("Column Right", ruin, 43.3f, 9.3f, 0.4f, 1.8f, stone);
            Visual("Broken Lintel", ruin, 42.6f, 10.22f, 1.8f, 0.25f, stone).transform.rotation = Quaternion.Euler(0, 0, 7);
            Visual("Fallen Stone", ruin, 40.05f, 8.55f, 0.65f, 0.25f, stone);
            Visual("Vine", ruin, 43.05f, 9.3f, 0.12f, 1.7f, moss);
            Visual("Moss", ruin, 42.5f, 10.4f, 1.3f, 0.08f, moss);
            Visual("Crystal Glow", ruin, 41.4f, 9.3f, 0.65f, 0.8f, new Color(0.25f, 0.62f, 0.60f, 0.10f));
            Visual("Faint Crystal", ruin, 41.4f, 9.3f, 0.23f, 0.4f, new Color(0.32f, 0.64f, 0.62f, 0.65f)).transform.rotation = Quaternion.Euler(0, 0, 30);

            Transform markers = Group("Start and Finish Markers");
            Text("Start Label", markers, "ARBOLEDA\nVERDESCENTE", 3f, 3f, 0.09f);
            Text("First Direction", markers, "INICIO  >", 4.2f, 1.7f, 0.055f);
            Text("Fork Sign", markers, "RUTA SUPERIOR  ^", 21.6f, 4.2f, 0.055f);
            Text("Main Sign", markers, "CAMINO PRINCIPAL  >", 33f, 3.4f, 0.05f);
            Text("Rest Sign", markers, "LLAMA DEL RECUERDO", 47.3f, 4.5f, 0.055f);
            Visual("End Pillar Left", markers, 73.3f, 3.2f, 0.45f, 2.8f, stone);
            Visual("End Pillar Right", markers, 77f, 3.2f, 0.45f, 2.8f, stone);
            Visual("End Lintel", markers, 75.15f, 4.7f, 4.2f, 0.3f, stone);
            Visual("End Moss", markers, 75.15f, 4.89f, 4.3f, 0.08f, moss);
            Text("End Label", markers, "FIN DEL RECORRIDO", 75.15f, 5.5f, 0.075f);

            PlayerHealth player = UnityEngine.Object.FindAnyObjectByType<PlayerHealth>();
            player.transform.position = new Vector3(3f, 0.6f);
            GameObject.Find("Player Spawn Point").transform.position = player.transform.position;
            GameObject.Find("Player Spawn Point").transform.SetParent(markers, true);
            Transform checkpoints = Group("Checkpoint");
            Place(MemoryFlameSceneSetup.PrefabPath, "Memory Flame - Arboleda", checkpoints, new Vector3(47f, 2.4f));
            Transform enemies = Group("Enemies");
            GameObject enemy = Place(GroundEnemySceneSetup.PrefabPath, "Ground Enemy - Arena", enemies, new Vector3(57.5f, 2.92f));
            SerializedObject enemyController = new SerializedObject(enemy.GetComponent<GroundEnemyController>());
            enemyController.FindProperty("target").objectReferenceValue = player;
            enemyController.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(enemy.GetComponent<GroundEnemyController>());
            BoxCollider2D bounds = GameObject.Find("Camera Room Bounds").GetComponent<BoxCollider2D>();
            bounds.transform.position = new Vector3(40f, 6f);
            bounds.size = new Vector2(80f, 20f);
            Camera camera = UnityEngine.Object.FindAnyObjectByType<CameraFollow2D>().GetComponent<Camera>();
            camera.transform.position = new Vector3(camera.orthographicSize * camera.aspect, 1.5f, -10f);
            camera.backgroundColor = new Color(0.055f, 0.09f, 0.10f);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("TASK12_SETUP_PASS: escena independiente de 80 unidades; dos rutas, una llama y un enemigo. Sin cambios en datos compartidos.");
        }

        private static Transform Group(string name, Transform parent = null)
        {
            Transform group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }

        private static SpriteRenderer Visual(string name, Transform parent, float x, float y, float width, float height, Color color, int order = -1)
        {
            GameObject go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(x, y);
            go.transform.localScale = new Vector3(width, height, 1f);
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }

        private static void Solid(string name, Transform parent, float x, float y, float width, float height, Color color)
        {
            Visual(name, parent, x, y, width, height, color).gameObject.AddComponent<BoxCollider2D>();
        }

        private static void Block(string name, Transform parent, float x, float width, float top)
        {
            Solid(name, parent, x, top * 0.5f, width, top, earth);
            Visual(name + " Moss", parent, x, top - 0.04f, width, 0.08f, moss, 0);
        }

        private static void Platform(string name, Transform parent, float x, float width, float top)
        {
            Solid(name, parent, x, top - 0.15f, width, 0.3f, stone);
            Visual(name + " Moss", parent, x, top - 0.04f, width, 0.08f, moss, 0);
        }

        private static GameObject Place(string path, string name, Transform parent, Vector3 position)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) throw new InvalidOperationException("Falta prefab existente: " + path);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            instance.transform.position = position;
            return instance;
        }

        private static void Text(string name, Transform parent, string text, float x, float y, float size)
        {
            GameObject go = new GameObject(name, typeof(TextMesh));
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(x, y);
            TextMesh mesh = go.GetComponent<TextMesh>();
            mesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            mesh.fontSize = 48;
            mesh.characterSize = size;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.text = text;
            mesh.color = ink;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mesh.font.material;
            renderer.sortingOrder = 8;
        }
    }
}
