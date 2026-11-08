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

        [MenuItem("Fangbound/Expand Arboleda Prototype to T13 (once)")]
        public static void Expand()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(ScenePath))
                throw new InvalidOperationException("Ampliar la escena existente desde Edit Mode.");
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Guardar los cambios abiertos antes de ampliar; no se descartan cambios manuales.");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (GameObject.Find("T13 Expansion") != null)
                throw new InvalidOperationException("T13 ya está aplicada; no se sobrescribe ni duplica la ampliación.");
            // Check only objects which will move. Unrelated hand-authored objects
            // remain intact; unexpected layouts stop here before any mutation.
            ExpectPosition("C Arena", new Vector2(53.5f, 1.2f));
            ExpectPosition("D Deck", new Vector2(74f, 0.9f));
            ExpectPosition("Ground Enemy - Arena", new Vector2(57.5f, 2.92f));
            ExpectPosition("Memory Flame - Arboleda", new Vector2(47f, 2.4f));
            ExpectPosition("East Boundary", new Vector2(80.3f, 6f));
            square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Placeholder/MovementTestSquare.asset");
            if (square == null) throw new InvalidOperationException("Falta sprite provisional existente.");
            var retained = new System.Collections.Generic.Dictionary<GameObject, GlobalObjectId>();
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                    retained.Add(item.gameObject, GlobalObjectId.GetGlobalObjectIdSlow(item.gameObject));

            foreach (string name in new[] { "C - Checkpoint and Arena", "D - Finish", "Checkpoint", "Enemies", "Rest Sign",
                "End Pillar Left", "End Pillar Right", "End Lintel", "End Moss", "End Label" })
                GameObject.Find(name).transform.position += Vector3.right * 70f;
            GameObject.Find("C - Checkpoint and Arena").name = "E - Checkpoint and Arena";
            GameObject.Find("D - Finish").name = "F - Finish";
            Transform floor = GameObject.Find("Safety Floor").transform;
            floor.position = new Vector3(75f, -2f);
            floor.localScale = new Vector3(150f, 4f, 1f);
            foreach (string side in new[] { "West Boundary", "East Boundary" })
            {
                Transform wall = GameObject.Find(side).transform;
                wall.position = new Vector3(side == "West Boundary" ? -0.3f : 150.3f, 9f);
                wall.localScale = new Vector3(0.6f, 26f, 1f);
            }
            BoxCollider2D bounds = GameObject.Find("Camera Room Bounds").GetComponent<BoxCollider2D>();
            bounds.transform.position = new Vector3(75f, 9f);
            bounds.size = new Vector2(150f, 26f);

            Transform expansion = Group("T13 Expansion", GameObject.Find("Level Geometry").transform);
            Transform ruinRecovery = Group("Ruin Recovery and Return", expansion);
            Platform("Ruin Catch", ruinRecovery, 33.45f, 1f, 4.6f);
            Platform("Ruin Return 1", ruinRecovery, 46f, 3f, 6.5f);
            Platform("Ruin Return 2", ruinRecovery, 49f, 3f, 4.7f);
            Transform grove = Group("C - Switchback Grove", expansion);
            Block("Grove Entry Terrace", grove, 48f, 8f, 2.4f);
            Block("Grove Base", grove, 58.75f, 13.5f, 2.4f);
            Block("Grove Entry", grove, 55f, 6f, 3.9f);
            Block("Grove East", grove, 60f, 4f, 5.5f);
            Platform("Grove Switchback", grove, 54f, 4f, 7.2f);
            Platform("Grove Upper Rest", grove, 61.5f, 6f, 8.8f);
            Block("Grove Low Recovery", grove, 63.25f, 2.5f, 3.9f);
            Block("Grove Crossing", grove, 72.75f, 16.5f, 8.8f);

            Transform lookout = Group("Optional Lookout Route", expansion);
            Platform("Lookout 1", lookout, 68f, 3.5f, 10.7f);
            Platform("Lookout 2", lookout, 73f, 3.5f, 12.7f);
            Platform("Lookout Deck", lookout, 78.5f, 5f, 14.5f);
            Platform("Lookout Catch", lookout, 75.375f, 1.05f, 11.3f);
            Platform("Lookout Return 1", lookout, 82.5f, 3f, 12.5f);
            Platform("Lookout Return 2", lookout, 85.5f, 3f, 10.6f);
            Platform("Lookout Return 3", lookout, 88.5f, 3f, 8.7f);

            Transform passage = Group("D - Stepped Passages", expansion);
            Block("Passage Low Rest", passage, 86.5f, 11f, 6.8f);
            Block("Passage Recovery Terrace", passage, 100f, 16f, 6.8f);
            Block("Passage 1", passage, 94f, 4f, 8.6f);
            Platform("Passage 2", passage, 99.75f, 3.5f, 10.4f);
            Platform("Passage Upper Rest", passage, 105.5f, 4f, 11.9f);
            Platform("Passage Catch", passage, 102.5f, 1.7f, 8.7f);
            Block("Passage Descent 1", passage, 108.5f, 2f, 9.9f);
            Block("Passage Descent 2", passage, 110.5f, 2f, 7.9f);
            Block("Passage Descent 3", passage, 112.5f, 2f, 5.9f);
            Block("Passage Descent 4", passage, 114.5f, 2f, 4.1f);

            Transform decor = Group("T13 Grove and Lookout Decoration", GameObject.Find("Provisional Decoration").transform);
            Visual("Lookout Broken Post", decor, 76.35f, 14.95f, 0.3f, 0.9f, stone);
            Visual("Lookout Rock", decor, 80.5f, 14.7f, 0.7f, 0.4f, stone);
            for (int i = 0; i < 3; i++)
                Visual("Lookout Grass", decor, 80.15f + i * 0.2f, 15.1f, 0.1f, 0.8f - i * 0.15f, moss)
                    .transform.rotation = Quaternion.Euler(0, 0, 15f - i * 15f);
            for (int i = 0; i < 5; i++)
                Visual("Distant Rock Spire", decor, 83f + i * 3f, 11f - i * 0.3f, 2f, 8f - i * 0.7f,
                    new Color(0.075f, 0.14f, 0.15f), -10).transform.rotation = Quaternion.Euler(0, 0, 10f - i * 4f);
            for (int i = 0; i < 10; i++)
            {
                float x = 86f + i * 6.3f, height = 8f + (i % 3) * 2f;
                Visual("Extension Trunk", decor, x, height * 0.5f, 0.45f, height, new Color(0.09f, 0.16f, 0.16f), -8);
                Visual("Extension Canopy", decor, x, height, 5.5f, 1.6f, new Color(0.10f, 0.20f, 0.18f), -9);
            }
            Text("Grove Turn Sign", decor, "<  ASCENSO", 53.5f, 9.2f, 0.055f);
            Text("Lookout Fork Sign", decor, "MIRADOR  ^", 65.8f, 12.1f, 0.055f);
            Text("Lookout Label", decor, "MIRADOR", 78.5f, 16.8f, 0.065f);
            Text("Lookout Return Sign", decor, "REGRESO  >", 82f, 14.3f, 0.045f);
            Text("Passage Sign", decor, "PASARELAS  >", 90.5f, 10.8f, 0.055f);
            Text("Descent Sign", decor, "DESCENSO  >", 106.5f, 13.6f, 0.05f);
            foreach (var pair in retained)
                if (pair.Key == null || !GlobalObjectId.GetGlobalObjectIdSlow(pair.Key).Equals(pair.Value))
                    throw new InvalidOperationException("Se perdió la identidad de un objeto existente.");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"TASK13_SETUP_PASS: ampliación aditiva a 150 m; {retained.Count} objetos originales conservados con sus identificadores; ruina intacta, nuevo mirador y arena desplazada 70 m.");
        }

        [MenuItem("Fangbound/Connect Arboleda Routes for T14 (once)")]
        public static void ConnectRoutes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(ScenePath))
                throw new InvalidOperationException("Conectar la escena existente desde Edit Mode.");
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Guardar los cambios abiertos antes de conectar las rutas.");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (GameObject.Find("T14 Interconnected Routes") != null || GameObject.Find("T13 Expansion") == null)
                throw new InvalidOperationException("Se requiere T13 sin T14 aplicada; no se sobrescriben rutas existentes.");
            // Validate every surface to be edited before making any changes.
            ExpectSurface("Grove Crossing", new Vector2(72.75f, 4.4f), new Vector2(16.5f, 8.8f));
            ExpectSurface("Lookout 1", new Vector2(68f, 10.55f), new Vector2(3.5f, 0.3f));
            ExpectSurface("Lookout 2", new Vector2(73f, 12.55f), new Vector2(3.5f, 0.3f));
            ExpectSurface("Lookout Catch", new Vector2(75.375f, 11.15f), new Vector2(1.05f, 0.3f));
            ExpectSurface("Passage Low Rest", new Vector2(86.5f, 3.4f), new Vector2(11f, 6.8f));
            ExpectPosition("Lookout Fork Sign", new Vector2(65.8f, 12.1f));
            ExpectPosition("Memory Flame - Arboleda", new Vector2(117f, 2.4f));
            ExpectPosition("Ground Enemy - Arena", new Vector2(127.5f, 2.92f));
            if (GameObject.Find("Faint Crystal") != null || GameObject.Find("Crystal Glow") != null)
                throw new InvalidOperationException("Revisar la escena: la decoración retirada ha reaparecido.");
            foreach (string name in new[] { "Network Center 2", "Network Center 3", "Network Lower Entry", "Network Lower Tunnel", "Network Lower Step", "Lookout Bridge", "Lookout Upper Recovery" })
                if (GameObject.Find(name) != null) throw new InvalidOperationException("Nombre ya utilizado: " + name);
            square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Placeholder/MovementTestSquare.asset");
            if (square == null) throw new InvalidOperationException("Falta sprite existente.");
            var retained = new System.Collections.Generic.Dictionary<GameObject, GlobalObjectId>();
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                    retained.Add(item.gameObject, GlobalObjectId.GetGlobalObjectIdSlow(item.gameObject));

            Transform network = Group("T14 Interconnected Routes", GameObject.Find("Level Geometry").transform);
            Transform center = Group("Central Route", network);
            Transform lower = Group("Lower Route", network);
            Transform upper = GameObject.Find("Optional Lookout Route").transform;
            upper.SetParent(network, true);
            ResizeSurface("Grove Crossing", 68.5f, 4f, 8f, 0.3f, center);
            Platform("Network Center 2", center, 75.5f, 3f, 7f);
            Platform("Network Center 3", center, 80f, 3f, 8.8f);
            Block("Network Lower Entry", lower, 68.25f, 7.5f, 6f);
            Block("Network Lower Tunnel", lower, 76f, 8f, 4.6f);
            Block("Network Lower Step", lower, 81.5f, 3f, 5.9f);
            ResizeSurface("Lookout 1", 61.625f, 3.25f, 11f, 0.3f, upper);
            ResizeSurface("Lookout 2", 67.25f, 3.5f, 13f, 0.3f, upper);
            Platform("Lookout Bridge", upper, 72.75f, 3.5f, 13.9f);
            Platform("Lookout Upper Recovery", upper, 70f, 1.8f, 12.2f);
            ResizeSurface("Lookout Catch", 75.25f, 1.3f, 12f, 0.3f, upper);
            ResizeSurface("Passage Low Rest", 87.5f, 9f, 6.8f, 6.8f);
            GameObject.Find("Lookout Fork Sign").transform.position = new Vector3(60.8f, 12.8f);
            foreach (var pair in retained)
                if (pair.Key == null || !GlobalObjectId.GetGlobalObjectIdSlow(pair.Key).Equals(pair.Value))
                    throw new InvalidOperationException("Se perdió un objeto original al conectar rutas.");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"TASK14_SETUP_PASS: red local sin ampliar 150 m; {retained.Count} objetos originales conservados; tres rutas y conexión inferior-central, reconexión en x=83..92.");
        }

        [MenuItem("Fangbound/Add Arboleda Underpass for T15 (once)")]
        public static void AddUnderpass()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(ScenePath))
                throw new InvalidOperationException("Editar la escena existente desde Edit Mode.");
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Guardar los cambios abiertos antes de añadir la zona inferior.");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (GameObject.Find("T14 Interconnected Routes") == null || GameObject.Find("T15 Root Underpass") != null)
                throw new InvalidOperationException("Se requiere T14 sin T15 aplicada; no se sobrescribe la zona inferior.");
            ExpectSurface("Passage Recovery Terrace", new Vector2(100f, 3.4f), new Vector2(16f, 6.8f));
            ExpectSurface("Passage Catch", new Vector2(102.5f, 8.55f), new Vector2(1.7f, 0.3f));
            ExpectSurface("Passage 1", new Vector2(94f, 4.3f), new Vector2(4f, 8.6f));
            ExpectSurface("Passage 2", new Vector2(99.75f, 10.25f), new Vector2(3.5f, 0.3f));
            ExpectSurface("Passage Upper Rest", new Vector2(105.5f, 11.75f), new Vector2(4f, 0.3f));
            ExpectPosition("Memory Flame - Arboleda", new Vector2(117f, 2.4f));
            ExpectPosition("Ground Enemy - Arena", new Vector2(127.5f, 2.92f));
            if (GameObject.Find("Faint Crystal") != null || GameObject.Find("Crystal Glow") != null)
                throw new InvalidOperationException("Revisar decoración retirada antes de editar.");
            foreach (string name in new[] { "Underpass Roof Center", "Underpass Roof East", "Underpass West Upper",
                "Underpass West Lower", "Underpass West Base", "Underpass Center Stone", "Underpass East Lower", "Underpass East Upper" })
                if (GameObject.Find(name) != null) throw new InvalidOperationException("Nombre ya utilizado: " + name);
            square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Placeholder/MovementTestSquare.asset");
            if (square == null) throw new InvalidOperationException("Falta sprite provisional existente.");
            var retained = new System.Collections.Generic.Dictionary<GameObject, GlobalObjectId>();
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                    retained.Add(item.gameObject, GlobalObjectId.GetGlobalObjectIdSlow(item.gameObject));

            Transform underpass = Group("T15 Root Underpass", GameObject.Find("Level Geometry").transform);
            Transform roof = Group("Roof and Openings", underpass);
            Transform west = Group("West Switchback", underpass);
            Transform east = Group("East Ascent", underpass);
            // Retain the original terrace and collider; open two shafts in its
            // former volume. T14 routes, the floor and camera bounds stay intact.
            ResizeSurface("Passage Recovery Terrace", 94f, 4f, 6.8f, 0.3f, roof);
            // Its original filled support occupied the western staircase.
            // Keep exactly the approved top surface, but hollow the space below.
            ResizeSurface("Passage 1", 94f, 4f, 8.6f, 0.3f);
            Platform("Underpass Roof Center", roof, 100.5f, 5f, 6.8f);
            Platform("Underpass Roof East", roof, 107f, 2f, 6.8f);
            Platform("Underpass West Upper", west, 97.3f, 2.4f, 4.8f);
            Platform("Underpass West Lower", west, 94.4f, 2.8f, 2.8f);
            Block("Underpass West Base", west, 97.5f, 2.6f, 1f);
            Block("Underpass Center Stone", underpass, 99.7f, 2.2f, 0.9f);
            Platform("Underpass East Lower", east, 102.1f, 2.4f, 2.8f);
            Platform("Underpass East Upper", east, 105f, 2f, 4.8f);
            // Widen only the existing recovery support to provide the eastern
            // return to Passage 2. Its height and previous landing area remain.
            ResizeSurface("Passage Catch", 102.95f, 2.6f, 8.7f, 0.3f);

            Color rootStone = new Color(.35f, .33f, .28f);
            Color rootCap = new Color(.53f, .48f, .32f);
            foreach (SpriteRenderer sprite in underpass.GetComponentsInChildren<SpriteRenderer>())
                sprite.color = sprite.name.EndsWith(" Moss", StringComparison.Ordinal) ? rootCap : rootStone;
            Transform decor = Group("T15 Root Hollow Decoration", GameObject.Find("Provisional Decoration").transform);
            Visual("Root Hollow Backdrop", decor, 99.75f, 3f, 15.5f, 6f, new Color(.12f, .12f, .095f), -7);
            for (int i = 0; i < 4; i++)
            {
                float x = 93f + i * 4.3f;
                Visual("Hanging Root", decor, x, 5.7f, .12f, 1.5f - i * .15f,
                    new Color(.25f, .24f, .16f), -6).transform.rotation = Quaternion.Euler(0, 0, i % 2 == 0 ? 15f : -15f);
            }
            foreach (var pair in retained)
                if (pair.Key == null || !GlobalObjectId.GetGlobalObjectIdSlow(pair.Key).Equals(pair.Value))
                    throw new InvalidOperationException("Se perdió un objeto original al abrir la zona inferior.");
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("No se pudo guardar T15.");
            Debug.Log($"TASK15_SETUP_PASS: dos conexiones en x=96..98 y 103..106; {retained.Count} objetos conservados, suelo y rutas T14 intactos.");
        }

        private static void ExpectSurface(string name, Vector2 center, Vector2 size)
        {
            ExpectPosition(name, center);
            GameObject surface = GameObject.Find(name);
            BoxCollider2D collider = surface.GetComponent<BoxCollider2D>();
            if (collider == null || collider.isTrigger || collider.size != Vector2.one || collider.offset != Vector2.zero ||
                Vector2.Distance(collider.bounds.size, size) > 0.02f || surface.transform.parent.lossyScale != Vector3.one ||
                Quaternion.Angle(surface.transform.rotation, Quaternion.identity) > 0.01f || GameObject.Find(name + " Moss") == null)
                throw new InvalidOperationException("Revisar edición manual de la superficie antes de conectar: " + name);
        }

        private static void ResizeSurface(string name, float x, float width, float top, float thickness, Transform parent = null)
        {
            Transform surface = GameObject.Find(name).transform;
            Transform cap = GameObject.Find(name + " Moss").transform;
            if (parent != null) { surface.SetParent(parent, true); cap.SetParent(parent, true); }
            surface.position = new Vector3(x, top - thickness * 0.5f);
            surface.localScale = new Vector3(width, thickness, 1f);
            cap.position = new Vector3(x, top - 0.04f);
            cap.localScale = new Vector3(width, 0.08f, 1f);
        }

        private static void ExpectPosition(string name, Vector2 expected)
        {
            GameObject item = GameObject.Find(name);
            if (item == null || Vector2.Distance(item.transform.position, expected) > 0.05f)
                throw new InvalidOperationException("Revisar cambios manuales antes de desplazar: " + name);
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
