using System;
using System.IO;
using System.Linq;
using Fangbound.CameraSystem;
using Fangbound.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Fangbound.EditorTools
{
    public static partial class InitialRegionGeographySetup
    {
        public const string B2ScenePath = Folder + "/Arboleda_B2_PasarelaQuebrada.unity";
        public const string B2DataPath = RoomCircuitSceneSetup.DataFolder + "/Arboleda_B2_PasarelaQuebrada.asset";

        // This is a separate migration, never a rerun of the T22 configurator.
        [MenuItem("Fangbound/Add Central Sector B2 for T24 (once)")]
        public static void AddB2()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode &&
                !Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty),
                "Guardar escenas y salir de Play antes de T24.");
            Require(!File.Exists(B2ScenePath) && !File.Exists(B2DataPath), "B2 ya existe; no se sobrescribe.");
            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Placeholder/MovementTestSquare.asset");
            Require(square != null && AssetDatabase.LoadAssetAtPath<GameObject>(RoomCircuitSceneSetup.RigPath) != null,
                "Faltan sprite o rig compartidos.");

            Scene source = EditorSceneManager.OpenScene(BPath, OpenSceneMode.Single);
            RoomScene b = RequireRoom("B", 3);
            Require(GameObject.Find("T22 Root Descent") != null && b.FindEntrance("from-B2") == null,
                "Se requiere B de T22 sin aplicar T24.");
            CheckBounds("B East Floor", new Vector2(19.5f, -1), new Vector2(9, 2));
            CheckBounds("B Root Pit Floor", new Vector2(12, -5), new Vector2(6, 2));
            CheckBounds("B Upper", new Vector2(21, 4.45f), new Vector2(6, .3f));
            CheckBounds("East Boundary", new Vector2(24.3f, 6), new Vector2(.6f, 20));
            CheckPosition("Entry from-C", new Vector2(12.3f, -3.42f));
            CheckPosition("Exit B-to-F", new Vector2(22.8f, 5.6f));
            CheckPosition("Entry from-F", new Vector2(19.5f, 5.18f));
            var retained = source.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .ToDictionary(t => t.gameObject, t => GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject));

            var definition = ScriptableObject.CreateInstance<RoomDefinition>();
            AssetDatabase.CreateAsset(definition, B2DataPath);
            B2String(definition, "roomId", "B2");
            B2String(definition, "scenePath", B2ScenePath);
            B2Reference(definition, "recoveryRoom", b.Definition);
            AssetDatabase.SaveAssets();

            // Two units of supported approach are needed between the pit and P02.
            // Preserve the western ledge, arrival from C and both normal-jump steps.
            SetSolid("B East Floor", 15, -1, 2, 2);
            SetSolid("B Root Pit Floor", 11.5f, -5, 5, 2);
            Move(GameObject.Find("B Root Pit East Wall").transform, 14.15f, -3);
            SetExistingPlatform("B Root East Ledge", 13.45f, 1.1f, -2);
            SetExistingPlatform("B Upper", 15, 2, 4.6f);
            SetSolid("East Boundary", 16.3f, 11.4f, .6f, 9.2f);
            Transform changes = new GameObject("T24 East Sendero Connection").transform;
            changes.SetParent(GameObject.Find("Level Geometry").transform, false);
            Solid("B East Boundary Middle", changes, square, 16.3f, 3.5f, .6f, 1.4f, BSoil);
            Visual("B Forest Margin", changes, square, 8, 4, 24, 20, new Color(.055f, .10f, .085f), -12);
            RoomExit legacy = FindExit(b, "B-to-F");
            Move(legacy.transform, 15.6f, 5.6f);
            legacy.GetComponent<BoxCollider2D>().size = new Vector2(.8f, 2);
            Record(legacy.GetComponent<BoxCollider2D>());
            Move(b.FindEntrance("from-F").transform, 14.4f, 5.18f);
            B2Entry(b.transform, "from-B2", 14.6f, .58f, false);
            B2Exit(b.transform, "B-to-B2", definition, "from-B", 15.6f);
            B2Bounds(b, 8, 3, 16, 18);
            foreach (var item in retained)
                Require(item.Key != null && GlobalObjectId.GetGlobalObjectIdSlow(item.Key).Equals(item.Value),
                    "Se perdió un objeto original de B.");
            EditorSceneManager.MarkSceneDirty(source);
            Require(EditorSceneManager.SaveScene(source), "No se guardó B.");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            definition = AssetDatabase.LoadAssetAtPath<RoomDefinition>(B2DataPath);
            RoomDefinition bDefinition = AssetDatabase.LoadAssetAtPath<RoomDefinition>(
                RoomCircuitSceneSetup.DataFolder + "/Arboleda_B_ClaroSendas.asset");
            var rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(RoomCircuitSceneSetup.RigPath));
            RoomScene room = rig.GetComponent<RoomScene>();
            B2Reference(room, "definition", definition);
            Move(room.FindEntrance("start").transform, 2.2f, .58f);
            B2Entry(rig.transform, "from-B", 2.2f, .58f, true);
            B2Exit(rig.transform, "B2-to-B", bDefinition, "from-B2", .4f);
            B2Bounds(room, 8, 4, 16, 16);
            Transform geometry = new GameObject("Level Geometry").transform;
            Solid("B2 West Ground", geometry, square, 2.25f, -2, 4.5f, 4, BSoil);
            Solid("B2 Broad Root", geometry, square, 7, -1.4f, 5, 5.2f, BSoil);
            Solid("B2 Hollow Floor", geometry, square, 11, -2.6f, 3, 2.8f, BSoil);
            Solid("B2 Hollow Step", geometry, square, 10.15f, -.6f, 1.3f, 1.2f, BSoil);
            Solid("B2 East Ground", geometry, square, 14.25f, -2, 3.5f, 4, BSoil);
            Platform("B2 Canopy Terrace", geometry, square, 8.15f, 3.3f, 3, BSoil);
            Platform("B2 Lookout", geometry, square, 13.2f, 3.6f, 4.8f, BSoil);
            Solid("B2 West Boundary Upper", geometry, square, -.3f, 9.4f, .6f, 13.2f, BSoil);
            Solid("B2 East Root Boundary", geometry, square, 16.3f, 6, .6f, 20, BSoil);

            Transform decor = new GameObject("Provisional Forest Decoration").transform;
            Visual("Forest Backdrop", decor, square, 8, 4, 24, 20, new Color(.055f, .10f, .085f), -12);
            for (int n = 0; n < 4; n++)
            {
                float x = 1 + n * 4.5f;
                Visual("Distant Trunk", decor, square, x, 5, .4f, 14, new Color(.09f, .16f, .12f), -8);
                Visual("Canopy Silhouette", decor, square, x + 1, 8.2f + n % 2, 4.3f, 1.2f,
                    new Color(.14f, .24f, .15f), -7);
            }
            Visual("Broad Root Moss", decor, square, 7, 1.17f, 5, .06f, new Color(.32f, .43f, .23f), 0);
            Visual("West Moss", decor, square, 2.25f, -.03f, 4.5f, .06f, new Color(.32f, .43f, .23f), 0);
            Visual("East Moss", decor, square, 14.25f, -.03f, 3.5f, .06f, new Color(.32f, .43f, .23f), 0);
            // A solid root closes the future P03. No frame, trigger or fake interaction.
            Visual("Overgrown Eastern Root", decor, square, 16.25f, 1.3f, .6f, 2.6f,
                new Color(.22f, .28f, .15f), -2);
            EditorSceneManager.MarkSceneDirty(scene);
            Require(EditorSceneManager.SaveScene(scene, B2ScenePath), "No se guardó B2.");
            var build = EditorBuildSettings.scenes.ToList();
            Require(build.All(item => item.path != B2ScenePath), "B2 duplicada en Build Settings.");
            build.Add(new EditorBuildSettingsScene(B2ScenePath, true));
            EditorBuildSettings.scenes = build.ToArray();
            AssetDatabase.SaveAssets();
            foreach (string path in new[] { BPath, B2ScenePath })
            {
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                RoomScene saved = Object.FindAnyObjectByType<RoomScene>();
                Require(saved.Definition != null && saved.GetComponentsInChildren<RoomExit>().All(e => e.Destination != null),
                    "Referencias perdidas al reabrir " + path);
                foreach (RoomEntrance entry in saved.GetComponentsInChildren<RoomEntrance>())
                    Require(entry.IsSafe(saved.Player), "Entrada insegura: " + entry.EntranceId);
            }
            Debug.Log("TASK24_SETUP_PASS: B1 16 unidades, B2 independiente, P02 a cota 0 y F técnico separado; objetos B conservados.");
        }

        private static void SetSolid(string name, float x, float y, float width, float height)
        {
            Transform item = GameObject.Find(name).transform;
            Move(item, x, y);
            item.localScale = new Vector3(width, height, 1);
            Record(item);
        }

        private static void SetExistingPlatform(string name, float x, float width, float top)
        {
            SetSolid(name, x, top - .15f, width, .3f);
            SetSolid(name + " Cap", x, top - .03f, width, .06f);
        }

        private static void B2Bounds(RoomScene room, float x, float y, float width, float height)
        {
            var bounds = room.GetComponentInChildren<CameraFollow2D>().transform.parent
                .Find("Camera Room Bounds").GetComponent<BoxCollider2D>();
            Move(bounds.transform, x, y);
            bounds.size = new Vector2(width, height);
            Record(bounds);
        }

        private static void B2Entry(Transform parent, string id, float x, float y, bool right)
        {
            var entry = new GameObject("Entry " + id).AddComponent<RoomEntrance>();
            entry.transform.SetParent(parent, false);
            entry.transform.position = new Vector3(x, y);
            B2String(entry, "entranceId", id);
            var data = new SerializedObject(entry);
            data.FindProperty("faceRight").boolValue = right;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void B2Exit(Transform parent, string id, RoomDefinition target, string entry, float x)
        {
            var exit = new GameObject("Exit " + id).AddComponent<RoomExit>();
            exit.transform.SetParent(parent, false);
            exit.transform.position = new Vector3(x, 1);
            exit.GetComponent<BoxCollider2D>().isTrigger = true;
            exit.GetComponent<BoxCollider2D>().size = new Vector2(.8f, 2);
            B2String(exit, "connectionId", id);
            B2String(exit, "destinationEntranceId", entry);
            B2Reference(exit, "destination", target);
        }

        private static void B2String(Object item, string field, string value)
        {
            var data = new SerializedObject(item);
            data.FindProperty(field).stringValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
            Record(item);
        }

        private static void B2Reference(Object item, string field, Object value)
        {
            var data = new SerializedObject(item);
            data.FindProperty(field).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
            Record(item);
        }
    }
}
