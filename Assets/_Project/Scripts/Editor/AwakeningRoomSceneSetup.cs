using System;
using System.IO;
using System.Linq;
using Fangbound.CameraSystem;
using Fangbound.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Fangbound.EditorTools
{
    public static class AwakeningRoomSceneSetup
    {
        public const string ScenePath = RoomCircuitSceneSetup.SceneFolder + "/Arboleda_A_ClaroDespertar.unity";
        public const string DataPath = RoomCircuitSceneSetup.DataFolder + "/Arboleda_A_ClaroDespertar.asset";
        private static readonly Color earth = new Color(.23f, .29f, .18f);
        private static Sprite square;

        [MenuItem("Fangbound/Add Awakening Room A for T20 (once)")]
        public static void Generate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Ejecutar en Edit Mode.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Guardar cambios antes de ampliar B.");
            if (File.Exists(ScenePath) || File.Exists(DataPath)) throw new InvalidOperationException("A ya existe; no se sobrescribe.");
            string bPath = RoomCircuitSceneSetup.SceneFolder + "/" + RoomCircuitSceneSetup.RoomNames[0] + ".unity";
            var bScene = EditorSceneManager.OpenScene(bPath);
            RoomScene bRoom = Object.FindAnyObjectByType<RoomScene>();
            if (bRoom == null || bRoom.Definition == null || bRoom.Definition.RoomId != "B" ||
                bRoom.GetComponentsInChildren<RoomExit>().Length != 2 || bRoom.FindEntrance("from-A") != null)
                throw new InvalidOperationException("Se requiere B aprobada sin conexión A aplicada.");
            Bounds step = GameObject.Find("B Step 1").GetComponent<Collider2D>().bounds;
            if (Vector2.Distance(step.center, new Vector2(9.5f, 1.35f)) > .02f ||
                Vector2.Distance(step.size, new Vector2(3, .3f)) > .02f)
                throw new InvalidOperationException("Revisar cambios manuales en B Step 1 antes de ampliar el acceso occidental.");
            square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Placeholder/MovementTestSquare.asset");
            if (square == null || AssetDatabase.LoadAssetAtPath<GameObject>(RoomCircuitSceneSetup.RigPath) == null)
                throw new InvalidOperationException("Faltan recursos compartidos del circuito.");
            var retained = bScene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .ToDictionary(t => t.gameObject, t => GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject));
            var definition = ScriptableObject.CreateInstance<RoomDefinition>();
            AssetDatabase.CreateAsset(definition, DataPath);
            Value(definition, "roomId", "A"); Value(definition, "scenePath", ScenePath);
            Ref(definition, "recoveryRoom", definition);
            var data = new SerializedObject(definition); data.FindProperty("startWithoutFang").boolValue = true; data.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Transform connection = new GameObject("T20 Awakening Connection").transform;
            connection.SetParent(GameObject.Find("Level Geometry").transform);
            Platform("B Awakening Approach", connection, 6.25f, 2.5f, 3.6f);
            Platform("B Awakening Landing", connection, 1.6f, 3.2f, 5.3f);
            Entry(bRoom.transform, "from-A", 2.65f, 5.88f, true);
            Exit(bRoom.transform, "B-to-A", definition, "from-B", 1.2f, 5.3f);
            foreach (var item in retained)
                if (item.Key == null || !GlobalObjectId.GetGlobalObjectIdSlow(item.Key).Equals(item.Value))
                    throw new InvalidOperationException("Se perdió un objeto aprobado de B.");
            EditorSceneManager.MarkSceneDirty(bScene);
            if (!EditorSceneManager.SaveScene(bScene)) throw new InvalidOperationException("No se guardó la conexión de B.");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            definition = AssetDatabase.LoadAssetAtPath<RoomDefinition>(DataPath);
            RoomDefinition bDefinition = AssetDatabase.LoadAssetAtPath<RoomDefinition>(RoomCircuitSceneSetup.DataFolder + "/" + RoomCircuitSceneSetup.RoomNames[0] + ".asset");
            GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(RoomCircuitSceneSetup.RigPath));
            Ref(rig.GetComponent<RoomScene>(), "definition", definition);
            var bounds = rig.GetComponentInChildren<CameraFollow2D>().transform.parent.Find("Camera Room Bounds").GetComponent<BoxCollider2D>();
            bounds.transform.position = new Vector3(10, 4); bounds.size = new Vector2(20, 16);
            PrefabUtility.RecordPrefabInstancePropertyModifications(bounds.transform); PrefabUtility.RecordPrefabInstancePropertyModifications(bounds);
            Entry(rig.transform, "from-B", 16.5f, .58f, false);
            Exit(rig.transform, "A-to-B", bDefinition, "from-A", 18.8f, 0);
            Transform geometry = new GameObject("Level Geometry").transform;
            Solid("Safety Floor", geometry, 10, -1, 20, 2);
            Solid("West Boundary", geometry, -.3f, 6, .6f, 20);
            Solid("East Boundary", geometry, 20.3f, 6, .6f, 20);
            Solid("A Root Rise", geometry, 13.25f, .2f, 2.5f, .4f);
            Visual("Root Moss", geometry, 13.25f, .38f, 2.5f, .04f, new Color(.4f, .48f, .24f), 0);

            Transform decor = new GameObject("Provisional Clearing Decoration").transform;
            for (int i = 0; i < 4; i++)
            {
                float x = 1.8f + i * 5.5f;
                Visual("Distant Trunk", decor, x, 5.8f, .5f, 13, new Color(.10f, .18f, .14f), -8);
                Visual("Canopy Silhouette", decor, x + .5f, 9.4f + i % 2, 5, 1, new Color(.15f, .25f, .16f), -7);
                Visual("Quiet Vegetation", decor, x + .7f, .2f, 1.1f, .4f, new Color(.23f, .35f, .19f), -4);
            }
            var pickup = new GameObject("Colmillo Pickup").AddComponent<FangPickup>();
            pickup.transform.position = new Vector3(8.2f, .55f);
            Transform presentation = new GameObject("Fang Presentation").transform; presentation.SetParent(pickup.transform, false);
            Visual("Bone Blade", presentation, 8.2f, .9f, .2f, 1.0f, new Color(.92f, .89f, .72f), 2).transform.rotation = Quaternion.Euler(0, 0, -18);
            Visual("Blade Tip", presentation, 8.36f, 1.38f, .17f, .17f, new Color(.92f, .89f, .72f), 2).transform.rotation = Quaternion.Euler(0, 0, 27);
            Visual("Fang Guard", presentation, 8.05f, .43f, .5f, .12f, new Color(.65f, .48f, .23f), 2);
            Visual("Fang Grip", presentation, 8.01f, .25f, .12f, .3f, new Color(.36f, .25f, .17f), 2).transform.rotation = Quaternion.Euler(0, 0, -18);
            Ref(pickup, "presentation", presentation.gameObject);
            Ref(pickup, "inputActions", AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Settings/InputSystem_Actions.inputactions"));

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("No se guardó A.");
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            foreach (string path in new[] { bPath, ScenePath })
            {
                EditorSceneManager.OpenScene(path);
                RoomScene saved = Object.FindAnyObjectByType<RoomScene>();
                if (saved.Definition == null || saved.GetComponentsInChildren<RoomExit>().Any(exit => exit.Destination == null))
                    throw new InvalidOperationException("Referencias perdidas al reabrir " + path);
            }
            Debug.Log($"TASK20_SETUP_PASS: A inicial, Colmillo, acceso occidental B y {retained.Count} objetos originales de B conservados.");
        }

        private static void Entry(Transform parent, string id, float x, float y, bool right)
        {
            var point = new GameObject("Entry " + id).AddComponent<RoomEntrance>(); point.transform.SetParent(parent); point.transform.position = new Vector3(x, y);
            Value(point, "entranceId", id);
            var data = new SerializedObject(point); data.FindProperty("faceRight").boolValue = right; data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Exit(Transform parent, string id, RoomDefinition target, string entry, float x, float floor)
        {
            var exit = new GameObject("Exit " + id).AddComponent<RoomExit>(); exit.transform.SetParent(parent); exit.transform.position = new Vector3(x, floor + 1);
            var trigger = exit.GetComponent<BoxCollider2D>(); trigger.isTrigger = true; trigger.size = new Vector2(1.2f, 2);
            Value(exit, "connectionId", id); Value(exit, "destinationEntranceId", entry); Ref(exit, "destination", target);
            Visual("Passage Frame", exit.transform, x, floor + 1.5f, .18f, 3, new Color(.5f, .47f, .32f), -2);
        }
        private static void Platform(string name, Transform parent, float x, float width, float top)
        {
            Solid(name, parent, x, top - .15f, width, .3f);
            Visual(name + " Cap", parent, x, top - .03f, width, .06f, new Color(.4f, .48f, .24f), 0);
        }
        private static void Solid(string name, Transform parent, float x, float y, float w, float h)
            => Visual(name, parent, x, y, w, h, earth, -1).gameObject.AddComponent<BoxCollider2D>();
        private static SpriteRenderer Visual(string name, Transform parent, float x, float y, float w, float h, Color color, int order)
        {
            var sprite = new GameObject(name).AddComponent<SpriteRenderer>(); sprite.transform.SetParent(parent);
            sprite.transform.position = new Vector3(x, y); sprite.transform.localScale = new Vector3(w, h, 1);
            sprite.sprite = square; sprite.color = color; sprite.sortingOrder = order; return sprite;
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
