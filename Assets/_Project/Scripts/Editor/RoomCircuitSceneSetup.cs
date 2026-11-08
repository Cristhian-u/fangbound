using System;
using System.IO;
using System.Linq;
using Fangbound.CameraSystem;
using Fangbound.Combat;
using Fangbound.Player;
using Fangbound.UI;
using Fangbound.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Fangbound.EditorTools
{
    public static class RoomCircuitSceneSetup
    {
        public const string SceneFolder = "Assets/_Project/Scenes/Arboleda/TechnicalCircuit";
        public const string DataFolder = "Assets/_Project/Data/World/Rooms";
        public const string RigPath = "Assets/_Project/Prefabs/Player/RoomPlayerRig.prefab";
        public static readonly string[] RoomNames = { "Arboleda_B_ClaroSendas", "Arboleda_C_RaicesProfundas", "Arboleda_F_GrutaRetorno" };
        private static Sprite square;

        [MenuItem("Fangbound/Create Three Room Circuit for T17 (once)")]
        public static void Generate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Ejecutar en Edit Mode.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Guardar cambios antes de crear salas.");
            if (File.Exists(RigPath) || RoomNames.Any(name => File.Exists(SceneFolder + "/" + name + ".unity") ||
                File.Exists(DataFolder + "/" + name + ".asset")))
                throw new InvalidOperationException("Hay assets T17 existentes; no se regeneran ni sobrescriben.");
            EditorSceneManager.OpenScene(GroundEnemySceneSetup.ScenePath, OpenSceneMode.Single);
            PlayerHealth source = Object.FindAnyObjectByType<PlayerHealth>();
            if (source == null || Object.FindAnyObjectByType<PlayerHealthUI>() == null || Object.FindAnyObjectByType<CombatHitstop>() == null)
                throw new InvalidOperationException("Faltan componentes reutilizables de MovementTest.");
            square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Placeholder/MovementTestSquare.asset");
            if (square == null || AssetDatabase.LoadAssetAtPath<GameObject>(MemoryFlameSceneSetup.PrefabPath) == null)
                throw new InvalidOperationException("Faltan sprite o prefab de Llama existentes.");
            Directory.CreateDirectory(SceneFolder);
            Directory.CreateDirectory(DataFolder);
            Directory.CreateDirectory(Path.GetDirectoryName(RigPath));
            AssetDatabase.Refresh();

            GameObject rig = new GameObject("Room Player Rig");
            RoomScene room = rig.AddComponent<RoomScene>();
            PlayerHealth player = Object.Instantiate(source.gameObject, rig.transform).GetComponent<PlayerHealth>();
            player.name = "Salamander";
            player.transform.position = new Vector3(4, .58f);
            player.GetComponent<PlayerAbilities>().SetDashUnlocked(false);
            player.GetComponent<PlayerAbilities>().SetDoubleJumpUnlocked(false);
            Ref(player, "spawnPoint", null);
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraFollow2D));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(rig.transform);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            camera.backgroundColor = new Color(.055f, .09f, .1f);
            camera.transform.position = new Vector3(12, 4, -10);
            BoxCollider2D bounds = new GameObject("Camera Room Bounds", typeof(BoxCollider2D)).GetComponent<BoxCollider2D>();
            bounds.transform.SetParent(rig.transform);
            bounds.transform.position = new Vector3(12, 4);
            bounds.size = new Vector2(24, 16);
            bounds.isTrigger = true;
            CameraFollow2D follow = cameraObject.GetComponent<CameraFollow2D>();
            Ref(follow, "target", player.transform);
            Ref(follow, "roomBounds", bounds);
            Ref(player.GetComponent<PlayerCheckpoint>(), "cameraFollow", follow);
            PlayerHealthUI hud = Object.Instantiate(Object.FindAnyObjectByType<PlayerHealthUI>().gameObject, rig.transform).GetComponent<PlayerHealthUI>();
            hud.name = "Player Health HUD";
            Ref(hud, "playerHealth", player);
            Object.Instantiate(Object.FindAnyObjectByType<CombatHitstop>().gameObject, rig.transform).name = "Combat Hitstop";
            Ref(room, "player", player);
            Ref(room, "cameraFollow", follow);
            Entrance(rig.transform, "start", 4, .58f, true);
            PrefabUtility.SaveAsPrefabAsset(rig, RigPath);
            Object.DestroyImmediate(rig);

            RoomDefinition[] definitions = new RoomDefinition[3];
            for (int i = 0; i < definitions.Length; i++)
            {
                definitions[i] = ScriptableObject.CreateInstance<RoomDefinition>();
                AssetDatabase.CreateAsset(definitions[i], DataFolder + "/" + RoomNames[i] + ".asset");
                Value(definitions[i], "roomId", new[] { "B", "C", "F" }[i]);
                Value(definitions[i], "scenePath", SceneFolder + "/" + RoomNames[i] + ".unity");
            }
            foreach (RoomDefinition definition in definitions) Ref(definition, "recoveryRoom", definitions[0]);
            AssetDatabase.SaveAssets();
            for (int i = 0; i < definitions.Length; i++) CreateRoom(i, definitions);
            var scenes = EditorBuildSettings.scenes.ToList();
            foreach (string name in RoomNames) scenes.Add(new EditorBuildSettingsScene(SceneFolder + "/" + name + ".unity", true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            foreach (string name in RoomNames)
            {
                EditorSceneManager.OpenScene(SceneFolder + "/" + name + ".unity");
                RoomScene saved = Object.FindAnyObjectByType<RoomScene>();
                if (saved == null || saved.Definition == null || saved.GetComponentsInChildren<RoomExit>().Any(exit => exit.Destination == null))
                    throw new InvalidOperationException("Referencias no persistidas en " + name);
            }
            EditorSceneManager.OpenScene(SceneFolder + "/" + RoomNames[0] + ".unity");
            Debug.Log("TASK17_SETUP_PASS: tres escenas, rig compartido, seis conexiones, una Llama y Build Settings aditivos.");
        }

        private static void CreateRoom(int index, RoomDefinition[] rooms)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Opening a scene can unload unused asset instances. Resolve fresh
            // asset references here instead of retaining native objects across it.
            rooms = RoomNames.Select(name => AssetDatabase.LoadAssetAtPath<RoomDefinition>(DataFolder + "/" + name + ".asset")).ToArray();
            GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(RigPath));
            Ref(rig.GetComponent<RoomScene>(), "definition", rooms[index]);
            Transform geometry = new GameObject("Level Geometry").transform;
            Color earth = index == 0 ? new Color(.16f, .25f, .23f) : index == 1 ? new Color(.25f, .23f, .16f) : new Color(.23f, .22f, .29f);
            Solid("Safety Floor", geometry, 12, -1, 24, 2, earth);
            Solid("West Boundary", geometry, -.3f, 6, .6f, 20, earth);
            Solid("East Boundary", geometry, 24.3f, 6, .6f, 20, earth);
            if (index == 0)
            {
                Platform("B Step 1", geometry, 9.5f, 3, 1.5f, earth);
                Platform("B Step 2", geometry, 14.5f, 3, 3, earth);
                Platform("B Upper", geometry, 21, 6, 4.6f, earth);
                Entrance(rig.transform, "from-C", 4, .58f, true);
                Entrance(rig.transform, "from-F", 19.5f, 5.18f, false);
                Exit(rig.transform, "B-to-C", rooms[1], "from-B", 1.2f, 0);
                Exit(rig.transform, "B-to-F", rooms[2], "from-B", 22.8f, 4.6f);
                GameObject flame = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(MemoryFlameSceneSetup.PrefabPath));
                flame.name = "Memory Flame - Claro";
                flame.transform.position = new Vector3(6.6f, 0);
                Value(flame.GetComponent<MemoryFlame>(), "checkpointId", "B.memory-flame");
            }
            else if (index == 1)
            {
                Platform("C Upper", geometry, 3.5f, 7, 8, earth);
                Platform("C Terrace 6", geometry, 11, 4, 6, earth);
                Platform("C Terrace 4", geometry, 6, 4, 4, earth);
                Platform("C Terrace 2", geometry, 12, 4, 2, earth);
                Transform start = rig.GetComponent<RoomScene>().FindEntrance("start").transform;
                start.position = new Vector3(4, 8.58f);
                PrefabUtility.RecordPrefabInstancePropertyModifications(start);
                Entrance(rig.transform, "from-B", 4, 8.58f, true);
                Entrance(rig.transform, "from-F", 20, .58f, false);
                Exit(rig.transform, "C-to-B", rooms[0], "from-C", 1.2f, 8);
                Exit(rig.transform, "C-to-F", rooms[2], "from-C", 22.8f, 0);
                BoxCollider2D bounds = rig.GetComponentInChildren<CameraFollow2D>().transform.parent.Find("Camera Room Bounds").GetComponent<BoxCollider2D>();
                bounds.transform.position = new Vector3(12, 6.5f);
                bounds.size = new Vector2(24, 21);
                PrefabUtility.RecordPrefabInstancePropertyModifications(bounds.transform);
                PrefabUtility.RecordPrefabInstancePropertyModifications(bounds);
            }
            else
            {
                Platform("F Low", geometry, 7.25f, 2.5f, .8f, earth);
                Platform("F Middle", geometry, 11.5f, 3, 2, earth);
                Platform("F High", geometry, 16.5f, 3, 3.8f, earth);
                Platform("F East", geometry, 21.5f, 5, 3.8f, earth);
                Entrance(rig.transform, "from-C", 4, .58f, true);
                Entrance(rig.transform, "from-B", 20, 4.38f, false);
                Exit(rig.transform, "F-to-C", rooms[1], "from-F", 1.2f, 0);
                Exit(rig.transform, "F-to-B", rooms[0], "from-F", 22.8f, 3.8f);
            }
            // Room identity, without flame-like decorative objects or text-dependent routes.
            Transform decor = new GameObject("Provisional Decoration").transform;
            for (int n = 0; n < 5; n++)
            {
                SpriteRenderer root = Visual("Trunk or Root", decor, 3 + n * 4.5f, 4, .25f, index == 1 ? 10 : 8,
                    new Color(earth.r * .6f, earth.g * .6f, earth.b * .6f), -6);
                root.transform.rotation = Quaternion.Euler(0, 0, index == 1 ? 12 - n * 5 : 0);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, rooms[index].ScenePath)) throw new InvalidOperationException("No se guardó la sala.");
        }

        private static void Entrance(Transform parent, string id, float x, float y, bool right)
        {
            RoomEntrance entrance = new GameObject("Entry " + id).AddComponent<RoomEntrance>();
            entrance.transform.SetParent(parent);
            entrance.transform.position = new Vector3(x, y);
            Value(entrance, "entranceId", id);
            var data = new SerializedObject(entrance);
            data.FindProperty("faceRight").boolValue = right;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Exit(Transform parent, string id, RoomDefinition target, string entry, float x, float floor)
        {
            RoomExit exit = new GameObject("Exit " + id).AddComponent<RoomExit>();
            exit.transform.SetParent(parent);
            exit.transform.position = new Vector3(x, floor + 1);
            BoxCollider2D trigger = exit.GetComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.2f, 2);
            Value(exit, "connectionId", id);
            Value(exit, "destinationEntranceId", entry);
            Ref(exit, "destination", target);
            Visual("Passage Frame", exit.transform, x, floor + 1.5f, .18f, 3, new Color(.5f, .47f, .32f), -2);
        }

        private static void Platform(string name, Transform parent, float x, float width, float top, Color color)
        {
            Solid(name, parent, x, top - .15f, width, .3f, color);
            Visual(name + " Cap", parent, x, top - .03f, width, .06f, color * 1.5f, 0);
        }
        private static void Solid(string name, Transform parent, float x, float y, float w, float h, Color color)
            => Visual(name, parent, x, y, w, h, color, -1).gameObject.AddComponent<BoxCollider2D>();
        private static SpriteRenderer Visual(string name, Transform parent, float x, float y, float w, float h, Color color, int order)
        {
            SpriteRenderer sprite = new GameObject(name).AddComponent<SpriteRenderer>();
            sprite.transform.SetParent(parent);
            sprite.transform.position = new Vector3(x, y);
            sprite.transform.localScale = new Vector3(w, h, 1);
            sprite.sprite = square; sprite.color = color; sprite.sortingOrder = order;
            return sprite;
        }
        private static void Ref(Object obj, string field, Object value)
        {
            var data = new SerializedObject(obj); data.FindProperty(field).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
            if (PrefabUtility.IsPartOfPrefabInstance(obj)) PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
        }
        private static void Value(Object obj, string field, string value)
        {
            var data = new SerializedObject(obj); data.FindProperty(field).stringValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
            if (PrefabUtility.IsPartOfPrefabInstance(obj)) PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
        }
    }
}
