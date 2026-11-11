using System;
using System.IO;
using System.Linq;
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
        public const string B3ScenePath = Folder + "/Arboleda_B3_TroncoPartido.unity";
        public const string B3DataPath = RoomCircuitSceneSetup.DataFolder + "/Arboleda_B3_TroncoPartido.asset";
        private static readonly Color B3Wood = new Color(.29f, .23f, .15f);

        [MenuItem("Fangbound/Add Split Trunk B3 for T25 (once)")]
        public static void AddB3()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode &&
                !Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty),
                "Guardar escenas y salir de Play antes de T25.");
            Require(!File.Exists(B3ScenePath) && !File.Exists(B3DataPath), "B3 ya existe; no se sobrescribe.");
            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Placeholder/MovementTestSquare.asset");
            Require(square != null && AssetDatabase.LoadAssetAtPath<GameObject>(RoomCircuitSceneSetup.RigPath) != null,
                "Faltan sprite o rig compartidos.");
            Scene source = EditorSceneManager.OpenScene(B2ScenePath, OpenSceneMode.Single);
            RoomScene b2 = RequireRoom("B2", 1);
            GameObject closedRoot = GameObject.Find("Overgrown Eastern Root");
            Require(closedRoot != null && b2.FindEntrance("from-B3") == null && b2.FindEntrance("from-B") != null,
                "Se requiere B2 aprobada de T24, sin P03 aplicado.");
            CheckBounds("B2 East Root Boundary", new Vector2(16.3f, 6), new Vector2(.6f, 20));
            CheckBounds("B2 East Ground", new Vector2(14.25f, -2), new Vector2(3.5f, 4));
            CheckPosition("Exit B2-to-B", new Vector2(.4f, 1));
            var retained = source.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Where(t => t.gameObject != closedRoot)
                .ToDictionary(t => t.gameObject, t => GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject));
            Require(!AssetDatabase.FindAssets("t:RoomDefinition").Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<RoomDefinition>).Any(def => def != null && def.RoomId == "B3"),
                "RoomId B3 ya está asignado.");

            var definition = ScriptableObject.CreateInstance<RoomDefinition>();
            AssetDatabase.CreateAsset(definition, B3DataPath);
            B2String(definition, "roomId", "B3");
            B2String(definition, "scenePath", B3ScenePath);
            B2Reference(definition, "recoveryRoom", AssetDatabase.LoadAssetAtPath<RoomDefinition>(
                RoomCircuitSceneSetup.DataFolder + "/Arboleda_B_ClaroSendas.asset"));
            AssetDatabase.SaveAssets();

            // Open only the reserved P03. P02, terrain, camera and encounter reserves stay intact.
            Object.DestroyImmediate(closedRoot);
            SetSolid("B2 East Root Boundary", 16.3f, 9.4f, .6f, 13.2f);
            Transform addition = new GameObject("T25 Split Trunk Connection").transform;
            addition.SetParent(GameObject.Find("Level Geometry").transform, false);
            Solid("B2 East Boundary Foot", addition, square, 16.3f, -2, .6f, 4, BSoil);
            B2Entry(b2.transform, "from-B3", 13.8f, .58f, false);
            B2Exit(b2.transform, "B2-to-B3", definition, "from-B2", 15.6f);
            foreach (var item in retained)
                Require(item.Key != null && GlobalObjectId.GetGlobalObjectIdSlow(item.Key).Equals(item.Value),
                    "Se perdió un objeto original de B2.");
            EditorSceneManager.MarkSceneDirty(source);
            Require(EditorSceneManager.SaveScene(source), "No se guardó B2/P03.");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            definition = AssetDatabase.LoadAssetAtPath<RoomDefinition>(B3DataPath);
            RoomDefinition b2Definition = AssetDatabase.LoadAssetAtPath<RoomDefinition>(B2DataPath);
            var rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(RoomCircuitSceneSetup.RigPath));
            RoomScene room = rig.GetComponent<RoomScene>();
            B2Reference(room, "definition", definition);
            Move(room.FindEntrance("start").transform, 2.2f, 4.58f);
            B2Entry(rig.transform, "from-B2", 2.2f, 4.58f, true);
            B2Exit(rig.transform, "B3-to-B2", b2Definition, "from-B3", .4f);
            Move(FindExit(room, "B3-to-B2").transform, .4f, 5);
            // The lower visual margin keeps the sealed root basin visible, below the map envelope.
            B2Bounds(room, 6, 5, 12, 22);
            Transform geometry = new GameObject("Level Geometry").transform;
            Solid("B3 Root Floor Seal", geometry, square, 6, -5, 12, 2, B3Wood);
            Solid("B3 West Root Lower", geometry, square, -.3f, -1, .6f, 10, B3Wood);
            Solid("B3 West Root Upper", geometry, square, -.3f, 11.4f, .6f, 9.2f, B3Wood);
            Solid("B3 East Root Wall", geometry, square, 12.3f, 5, .6f, 22, B3Wood);
            Solid("B3 Closed Crown", geometry, square, 6, 16.3f, 12, .6f, B3Wood);
            Platform("B3 Entry Root", geometry, square, 1.8f, 3.6f, 4, B3Wood);
            Platform("B3 East Clearing", geometry, square, 9.5f, 5, 4, B3Wood);
            Platform("B3 Lower East Root", geometry, square, 8.6f, 3.2f, -2.2f, B3Wood);
            Platform("B3 Lower West Root", geometry, square, 3.3f, 3, -.3f, B3Wood);
            Platform("B3 Inner Knot", geometry, square, 5.9f, 2.2f, 1.8f, B3Wood);
            Platform("B3 West Fork", geometry, square, 5, 3, 5.8f, B3Wood);
            Platform("B3 East Fork", geometry, square, 9.7f, 3.8f, 7.5f, B3Wood);
            Platform("B3 Crown West", geometry, square, 5.4f, 3.2f, 9.5f, B3Wood);
            Platform("B3 Crown Rest", geometry, square, 9.6f, 4, 11.3f, B3Wood);
            Platform("B3 Split Pocket", geometry, square, 1.9f, 2.2f, 7.6f, B3Wood);

            Transform decor = new GameObject("Provisional Split Trunk Decoration").transform;
            Visual("Forest Margin", decor, square, 6, 5, 26, 26, new Color(.055f, .10f, .08f), -12);
            Visual("Hollow Trunk Interior", decor, square, 6, 6, 11.5f, 20, new Color(.10f, .13f, .095f), -10);
            Visual("West Bark Mass", decor, square, .6f, 6, 2, 20, B3Wood * .55f, -7);
            Visual("East Bark Mass", decor, square, 11.3f, 6, 2.2f, 20, B3Wood * .55f, -7);
            Visual("West Broken Tip", decor, square, 2.5f, 14.1f, 1.1f, 4.3f, B3Wood * .8f, -6)
                .transform.rotation = Quaternion.Euler(0, 0, -22);
            Visual("East Broken Tip", decor, square, 9.7f, 14.3f, 1.3f, 4.8f, B3Wood * .75f, -6)
                .transform.rotation = Quaternion.Euler(0, 0, 18);
            Visual("Lower Root Sweep", decor, square, 9.4f, -2.3f, .8f, 4, B3Wood * .7f, -6)
                .transform.rotation = Quaternion.Euler(0, 0, -35);
            Visual("West Root Sweep", decor, square, 2.9f, 1.2f, .65f, 5.2f, B3Wood * .7f, -6)
                .transform.rotation = Quaternion.Euler(0, 0, 28);
            Visual("Buried Root Moss", decor, square, 6, -4.03f, 12, .06f, new Color(.30f, .39f, .19f), 0);
            // P12 is only a documented floor reservation (6,-4); no exit, glyph or interaction.
            // P17 above the closed crown remains unapproved and has no scene reference.
            EditorSceneManager.MarkSceneDirty(scene);
            Require(EditorSceneManager.SaveScene(scene, B3ScenePath), "No se guardó B3.");
            var build = EditorBuildSettings.scenes.ToList();
            Require(build.All(item => item.path != B3ScenePath), "B3 duplicada en Build Settings.");
            build.Add(new EditorBuildSettingsScene(B3ScenePath, true));
            EditorBuildSettings.scenes = build.ToArray();
            AssetDatabase.SaveAssets();
            foreach (string path in new[] { B2ScenePath, B3ScenePath })
            {
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                RoomScene saved = Object.FindAnyObjectByType<RoomScene>();
                Require(saved.Definition != null && saved.GetComponentsInChildren<RoomExit>().All(e => e.Destination != null),
                    "Referencias perdidas al reabrir " + path);
                foreach (RoomEntrance entry in saved.GetComponentsInChildren<RoomEntrance>())
                    Require(entry.IsSafe(saved.Player), "Entrada insegura: " + entry.EntranceId);
            }
            Debug.Log("TASK25_SETUP_PASS: B3 independiente 12x20, P03 global (32,0), P12 reservado (38,-8), sin enlace funcional F.");
        }
    }
}
