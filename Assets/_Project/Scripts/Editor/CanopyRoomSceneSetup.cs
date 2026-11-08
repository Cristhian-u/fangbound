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
    public static class CanopyRoomSceneSetup
    {
        public const string ScenePath = RoomCircuitSceneSetup.SceneFolder + "/Arboleda_E_CopasVigia.unity";
        public const string DataPath = RoomCircuitSceneSetup.DataFolder + "/Arboleda_E_CopasVigia.asset";
        private static readonly Color bark = new Color(.29f, .25f, .16f);
        private static readonly Color foliage = new Color(.47f, .55f, .27f);
        private static Sprite square;

        [MenuItem("Fangbound/Add Canopy Room E for T19 (once)")]
        public static void Generate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Ejecutar en Edit Mode.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Guardar cambios antes de ampliar D.");
            if (File.Exists(ScenePath) || File.Exists(DataPath)) throw new InvalidOperationException("E ya existe; no se sobrescribe.");
            var dScene = EditorSceneManager.OpenScene(RuinsRoomSceneSetup.ScenePath);
            RoomScene dRoom = Object.FindAnyObjectByType<RoomScene>();
            if (dRoom == null || dRoom.Definition == null || dRoom.Definition.RoomId != "D" ||
                dRoom.GetComponentsInChildren<RoomExit>().Length != 1 || dRoom.FindEntrance("from-E") != null)
                throw new InvalidOperationException("Se requiere D aprobada de T18 sin conexión E aplicada.");
            Bounds chamber = GameObject.Find("D Upper Chamber").GetComponent<Collider2D>().bounds;
            if (Vector2.Distance(chamber.center, new Vector2(17.75f, 10.65f)) > .02f ||
                Vector2.Distance(chamber.size, new Vector2(7.5f, .3f)) > .02f)
                throw new InvalidOperationException("Revisar cambios manuales en D Upper Chamber antes de añadir el paso.");
            square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Placeholder/MovementTestSquare.asset");
            if (square == null || AssetDatabase.LoadAssetAtPath<GameObject>(RoomCircuitSceneSetup.RigPath) == null)
                throw new InvalidOperationException("Faltan recursos compartidos del circuito.");
            var retained = dScene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .ToDictionary(t => t.gameObject, t => GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject));
            RoomDefinition definition = ScriptableObject.CreateInstance<RoomDefinition>();
            AssetDatabase.CreateAsset(definition, DataPath);
            Value(definition, "roomId", "E"); Value(definition, "scenePath", ScenePath);
            Ref(definition, "recoveryRoom", dRoom.Definition.RecoveryRoom);
            AssetDatabase.SaveAssets();
            // The passage occupies the recess already reserved at the end of D's ascent.
            Entry(dRoom.transform, "from-E", 16.5f, 11.38f, false);
            Exit(dRoom.transform, "D-to-E", definition, "from-D", 20, 10.8f);
            foreach (var item in retained)
                if (item.Key == null || !GlobalObjectId.GetGlobalObjectIdSlow(item.Key).Equals(item.Value))
                    throw new InvalidOperationException("Se perdió un objeto aprobado de D.");
            EditorSceneManager.MarkSceneDirty(dScene);
            if (!EditorSceneManager.SaveScene(dScene)) throw new InvalidOperationException("No se guardó la conexión de D.");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Re-resolve assets after changing scenes, as in T17/T18.
            definition = AssetDatabase.LoadAssetAtPath<RoomDefinition>(DataPath);
            RoomDefinition dDefinition = AssetDatabase.LoadAssetAtPath<RoomDefinition>(RuinsRoomSceneSetup.DataPath);
            GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(RoomCircuitSceneSetup.RigPath));
            Ref(rig.GetComponent<RoomScene>(), "definition", definition);
            BoxCollider2D cameraBounds = rig.GetComponentInChildren<CameraFollow2D>().transform.parent.Find("Camera Room Bounds").GetComponent<BoxCollider2D>();
            cameraBounds.transform.position = new Vector3(16, 9.5f);
            cameraBounds.size = new Vector2(32, 27);
            PrefabUtility.RecordPrefabInstancePropertyModifications(cameraBounds.transform);
            PrefabUtility.RecordPrefabInstancePropertyModifications(cameraBounds);
            Entry(rig.transform, "from-D", 4, .58f, true);
            Exit(rig.transform, "E-to-D", dDefinition, "from-E", 1.2f, 0);

            Transform geometry = new GameObject("Level Geometry").transform;
            Solid("Safety Floor", geometry, 16, -1, 32, 2);
            Solid("West Boundary", geometry, -.3f, 13, .6f, 30);
            Solid("East Boundary", geometry, 32.3f, 13, .6f, 30);
            // A broad fork, then a western zigzag with varied rises and gaps.
            Branch("E Fork Bough", geometry, 7, 15.5f, 1.4f);
            Branch("E Main West Bough", geometry, 2.8f, 7.5f, 3.2f);
            Branch("E Main Crossing", geometry, 9, 14, 5.1f);
            Branch("E Main Return", geometry, 3, 7, 6.5f);
            Branch("E Crown Junction", geometry, 9.5f, 17, 8.5f);
            Branch("E Watch Approach", geometry, 19, 24, 9.8f);
            Branch("E Watch Lookout", geometry, 25.5f, 32, 11.4f);
            // Eastern traverse curls back toward the junction; it is not another western staircase.
            Branch("E Side Sprig", geometry, 18, 22.5f, 2.5f);
            Branch("E Side Outer Bough", geometry, 25, 30, 4.5f);
            Branch("E Side Inner Bough", geometry, 19, 23.5f, 6.6f);
            Branch("E Lower Catch", geometry, 22.8f, 26, .9f);

            Transform decor = new GameObject("Provisional Canopy Decoration").transform;
            Color distant = new Color(.10f, .17f, .13f), leaves = new Color(.18f, .29f, .18f);
            for (int i = 0; i < 4; i++)
            {
                float x = 2 + i * 9;
                Visual("Distant Trunk", decor, x, 6.5f, .65f + i * .1f, 19, distant, -9);
                Visual("Distant Crown", decor, x + 1.2f, 13.8f + (i % 2), 7, 1.3f, leaves, -8);
                Visual("Forest Below", decor, x + 1, -.4f, 6, 1.1f, leaves, -8);
            }
            Visual("Fork Trunk", decor, 8.1f, 4.7f, .75f, 12, new Color(.17f, .21f, .13f), -6);
            Visual("Outer Trunk", decor, 27, 7.5f, .85f, 17, new Color(.16f, .23f, .14f), -6);
            Visual("Lookout Silhouette Left", decor, 26, 12.3f, .25f, 1.8f, bark, -2).transform.rotation = Quaternion.Euler(0, 0, -12);
            Visual("Lookout Overhead Bough", decor, 28.9f, 14, 5, .3f, bark, -2).transform.rotation = Quaternion.Euler(0, 0, 5);
            Visual("Lookout Leaves", decor, 29, 14.3f, 4.7f, .25f, foliage, -2);
            // No marker/trigger for B: any future return starts from this broad eastern crown.
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("No se guardó E.");
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            foreach (string path in new[] { RuinsRoomSceneSetup.ScenePath, ScenePath })
            {
                EditorSceneManager.OpenScene(path);
                RoomScene saved = Object.FindAnyObjectByType<RoomScene>();
                if (saved.Definition == null || saved.GetComponentsInChildren<RoomExit>().Any(exit => exit.Destination == null))
                    throw new InvalidOperationException("Referencias perdidas al reabrir " + path);
            }
            Debug.Log($"TASK19_SETUP_PASS: E, conexión aditiva D-E y {retained.Count} GameObjects originales de D conservados.");
        }

        private static void Entry(Transform parent, string id, float x, float y, bool right)
        {
            var point = new GameObject("Entry " + id).AddComponent<RoomEntrance>();
            point.transform.SetParent(parent); point.transform.position = new Vector3(x, y);
            Value(point, "entranceId", id);
            var data = new SerializedObject(point); data.FindProperty("faceRight").boolValue = right; data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Exit(Transform parent, string id, RoomDefinition target, string entry, float x, float floor)
        {
            var exit = new GameObject("Exit " + id).AddComponent<RoomExit>();
            exit.transform.SetParent(parent); exit.transform.position = new Vector3(x, floor + 1);
            var trigger = exit.GetComponent<BoxCollider2D>(); trigger.isTrigger = true; trigger.size = new Vector2(1.2f, 2);
            Value(exit, "connectionId", id); Value(exit, "destinationEntranceId", entry); Ref(exit, "destination", target);
            Visual("Passage Frame", exit.transform, x, floor + 1.5f, .18f, 3, new Color(.5f, .47f, .32f), -2);
        }
        private static void Branch(string name, Transform parent, float left, float right, float top)
        {
            float x = (left + right) / 2, width = right - left;
            Solid(name, parent, x, top - .15f, width, .3f);
            Visual(name + " Foliage", parent, x, top - .03f, width, .06f, foliage, 0);
            Visual(name + " Twig", parent, x + width * .15f, top - .4f, width * .65f, .16f, bark, -2)
                .transform.rotation = Quaternion.Euler(0, 0, 9);
        }
        private static void Solid(string name, Transform parent, float x, float y, float w, float h)
            => Visual(name, parent, x, y, w, h, bark, -1).gameObject.AddComponent<BoxCollider2D>();
        private static SpriteRenderer Visual(string name, Transform parent, float x, float y, float w, float h, Color color, int order)
        {
            var sprite = new GameObject(name).AddComponent<SpriteRenderer>(); sprite.transform.SetParent(parent);
            sprite.transform.position = new Vector3(x, y); sprite.transform.localScale = new Vector3(w, h, 1);
            sprite.sprite = square; sprite.color = color; sprite.sortingOrder = order;
            return sprite;
        }
        private static void Ref(Object obj, string field, Object value)
        {
            var data = new SerializedObject(obj); data.FindProperty(field).objectReferenceValue = value; data.ApplyModifiedPropertiesWithoutUndo();
            if (PrefabUtility.IsPartOfPrefabInstance(obj)) PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
        }
        private static void Value(Object obj, string field, string value)
        {
            var data = new SerializedObject(obj); data.FindProperty(field).stringValue = value; data.ApplyModifiedPropertiesWithoutUndo();
            if (PrefabUtility.IsPartOfPrefabInstance(obj)) PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
        }
    }
}
