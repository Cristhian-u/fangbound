using System.Collections.Generic;
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
        public const string F1ScenePath = Folder + "/Arboleda_F_GrutaRetorno.unity";
        private static readonly Color F1Stone = new Color(.28f, .27f, .22f);

        [MenuItem("Fangbound/Connect F1 and B3 P12 for T27 (once)")]
        public static void ConnectP12()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode &&
                !Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty),
                "Guardar escenas y salir de Play antes de T27.");
            // Verify every source before changing any scene. Never rerun T17/T22/T24/T25.
            EditorSceneManager.OpenScene(BPath);
            RequireRoom("B", 4);
            CheckPosition("Exit B-to-F", new Vector2(15.6f, 5.6f));
            CheckPosition("Entry from-F", new Vector2(14.4f, 5.18f));
            EditorSceneManager.OpenScene(CPath);
            RequireRoom("C", 3);
            CheckBounds("Safety Floor", new Vector2(12, -1), new Vector2(24, 2));
            CheckPosition("Exit C-to-F", new Vector2(22.8f, 1));
            EditorSceneManager.OpenScene(F1ScenePath);
            RequireRoom("F", 2);
            CheckPosition("Exit F-to-B", new Vector2(22.8f, 4.8f));
            CheckBounds("Safety Floor", new Vector2(12, -1), new Vector2(24, 2));
            EditorSceneManager.OpenScene(B3ScenePath);
            RequireRoom("B3", 1);
            CheckBounds("B3 Root Floor Seal", new Vector2(6, -5), new Vector2(12, 2));
            Require(GameObject.Find("T27 P12 Root Opening") == null &&
                AssetDatabase.LoadAssetAtPath<Fangbound.Enemies.GroundEnemyData>(B2EncounterSceneSetup.DataPath) != null,
                "Se requiere base T26 sin T27 aplicada.");

            RemoveLegacyB1();
            AdaptC1();
            AdaptF1();
            OpenB3();
            foreach (string path in new[] { BPath, CPath, F1ScenePath, B3ScenePath })
            {
                // Close, reopen and verify the persisted references, not just the in-memory objects.
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
                EditorSceneManager.OpenScene(path);
                RoomScene room = Object.FindAnyObjectByType<RoomScene>();
                foreach (RoomEntrance entry in room.GetComponentsInChildren<RoomEntrance>())
                    Require(entry.IsSafe(room.Player), "Entrada insegura guardada: " + path + "/" + entry.EntranceId);
                foreach (RoomExit exit in room.GetComponentsInChildren<RoomExit>())
                    Require(exit.Destination != null && exit.ConnectionId != "F-to-B" && exit.ConnectionId != "B-to-F",
                        "Salida obsoleta o sin destino guardada.");
            }
            Debug.Log("TASK27_SETUP_PASS: F reutilizada como F1 32x12, C1 16x16, P11/P12 alineados; F↔B1 retirado y referencias guardadas/reabiertas.");
        }

        private static void RemoveLegacyB1()
        {
            Scene scene = EditorSceneManager.OpenScene(BPath);
            GameObject exit = GameObject.Find("Exit B-to-F");
            GameObject entry = GameObject.Find("Entry from-F");
            var kept = Retain(scene, exit, entry);
            Object.DestroyImmediate(exit); Object.DestroyImmediate(entry);
            Transform group = Addition("T27 Retired Return Roots");
            Solid("B East Retired Return Wall", group, Square(), 16.3f, 5.5f, .6f, 2.6f, BSoil);
            Visual("Closed Return Root", group, Square(), 16.05f, 5.4f, .35f, 2.8f, BSoil * 1.3f, 0)
                .transform.rotation = Quaternion.Euler(0, 0, -8);
            SaveRetained(scene, kept);
        }

        private static void AdaptC1()
        {
            Scene scene = EditorSceneManager.OpenScene(CPath);
            RoomScene room = RequireRoom("C", 3);
            var obsolete = new[] { GameObject.Find("C Ruins Gate"), GameObject.Find("C Ruins Gate Cap"),
                FindExit(room, "C-to-D").transform.Find("Passage Frame").gameObject,
                FindExit(room, "C-to-F").transform.Find("Passage Frame").gameObject };
            var kept = Retain(scene, obsolete);
            foreach (GameObject item in obsolete) Object.DestroyImmediate(item);
            SetSolid("Safety Floor", 8, -1, 16, 2);
            SetSolid("East Boundary", 16.3f, 7.4f, .6f, 9.2f);
            SetSolid("West Boundary", -.3f, 7.4f, .6f, 9.2f);
            SetSolid("C Upper Roof East", 15.5f, 12.3f, 1, .6f);
            SetExistingPlatform("C Ruins Approach", 14.75f, 2.5f, 6);
            // Keep the existing D sample reachable, toward the future western C2 branch.
            // This is a technical link, not a claim that C2/P05/P06 are built.
            ResizeExit(FindExit(room, "C-to-D"), .4f, 1, .8f, 2);
            Move(room.FindEntrance("from-D").transform, 2.2f, .58f);
            Facing(room.FindEntrance("from-D"), true);
            ResizeExit(FindExit(room, "C-to-F"), 15.6f, 1, .8f, 2);
            Move(room.FindEntrance("from-F").transform, 13.8f, .58f);
            B2Bounds(room, 8, 6.5f, 16, 21);
            Transform group = Addition("T27 C1 Passage Geometry");
            Solid("C East Boundary Foot", group, Square(), 16.3f, -2, .6f, 4, CSoil);
            Solid("C West Boundary Foot", group, Square(), -.3f, -2, .6f, 4, CSoil);
            HorizontalOpening(group, "P11 C1 East Roots", 15.6f, CSoil);
            HorizontalOpening(group, "Technical D West Rocks", .4f, CSoil);
            SaveRetained(scene, kept);
        }

        private static void AdaptF1()
        {
            Scene scene = EditorSceneManager.OpenScene(F1ScenePath);
            RoomScene room = RequireRoom("F", 2);
            var obsolete = new[] { GameObject.Find("Exit F-to-B"), GameObject.Find("Entry from-B"),
                FindExit(room, "F-to-C").transform.Find("Passage Frame").gameObject };
            var kept = Retain(scene, obsolete);
            foreach (GameObject item in obsolete) Object.DestroyImmediate(item);
            SetSolid("Safety Floor", 16, -1, 32, 2);
            SetSolid("East Boundary", 32.3f, 2, .6f, 12);
            SetSolid("West Boundary", -.3f, 5.4f, .6f, 5.2f);
            Move(room.FindEntrance("from-C").transform, 2.2f, .58f);
            ResizeExit(FindExit(room, "F-to-C"), .4f, 1, .8f, 2);
            B2Bounds(room, 16, 4, 32, 16);
            Transform group = Addition("T27 F1 Root Ascent");
            Solid("F West Boundary Foot", group, Square(), -.3f, -2, .6f, 4, F1Stone);
            Solid("F Roof West", group, Square(), 9.5f, 8.3f, 19, .6f, F1Stone);
            Solid("F Roof East", group, Square(), 28.5f, 8.3f, 7, .6f, F1Stone);
            Platform("F P12 Rest", group, Square(), 22, 4, 6, F1Stone);
            Platform("F East Root Shelf", group, Square(), 28.25f, 4.5f, 2.2f, F1Stone);
            B2Entry(room.transform, "from-B3", 22, 6.58f, false);
            RoomDefinition b3 = AssetDatabase.LoadAssetAtPath<RoomDefinition>(B3DataPath);
            B2Exit(room.transform, "F-to-B3", b3, "from-F", 22);
            ResizeExit(FindExit(room, "F-to-B3"), 22, 8.4f, 6, .8f);
            HorizontalOpening(group, "P11 F1 West Rocks", .4f, F1Stone);
            VerticalOpening(group, "P12 F1 Upper Roots", 22, 8, F1Stone);
            Visual("F1 Cave Backdrop", group, Square(), 16, 3.5f, 40, 18, new Color(.075f, .10f, .105f), -12);
            Visual("F1 East Rock Mass", group, Square(), 30.8f, 4, 1.4f, 7, F1Stone * .55f, -6);
            SaveRetained(scene, kept);
        }

        private static void OpenB3()
        {
            Scene scene = EditorSceneManager.OpenScene(B3ScenePath);
            RoomScene room = RequireRoom("B3", 1);
            var kept = Retain(scene);
            SetSolid("B3 Root Floor Seal", 1.5f, -5, 3, 2);
            GameObject.Find("B3 Root Floor Seal").name = "B3 Root Floor West";
            SetSolid("Buried Root Moss", 1.5f, -4.03f, 3, .06f);
            Transform group = Addition("T27 P12 Root Opening");
            Solid("B3 Root Floor East", group, Square(), 10.5f, -5, 3, 2, B3Wood);
            Platform("B3 P12 Arrival Root", group, Square(), 6, 1.8f, -3.8f, B3Wood);
            Visual("East Buried Root Moss", group, Square(), 10.5f, -4.03f, 3, .06f, new Color(.30f, .39f, .19f), 0);
            B2Entry(room.transform, "from-F", 6, -3.22f, true);
            RoomDefinition f = AssetDatabase.LoadAssetAtPath<RoomDefinition>(RoomCircuitSceneSetup.DataFolder + "/Arboleda_F_GrutaRetorno.asset");
            B2Exit(room.transform, "B3-to-F", f, "from-B3", 6);
            ResizeExit(FindExit(room, "B3-to-F"), 6, -4.4f, 6, .8f);
            VerticalOpening(group, "P12 B3 Lower Roots", 6, -4, B3Wood);
            SaveRetained(scene, kept);
        }

        private static Sprite Square() => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Placeholder/MovementTestSquare.asset");

        private static Transform Addition(string name)
        {
            Transform group = new GameObject(name).transform;
            group.SetParent(GameObject.Find("Level Geometry").transform, false);
            return group;
        }

        private static Dictionary<GameObject, GlobalObjectId> Retain(Scene scene, params GameObject[] removed)
        {
            var excluded = new HashSet<GameObject>(removed.SelectMany(item => item.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject));
            return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Where(t => !excluded.Contains(t.gameObject))
                .ToDictionary(t => t.gameObject, t => GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject));
        }

        private static void SaveRetained(Scene scene, Dictionary<GameObject, GlobalObjectId> kept)
        {
            foreach (var pair in kept)
                Require(pair.Key != null && GlobalObjectId.GetGlobalObjectIdSlow(pair.Key).Equals(pair.Value), "Se perdió un objeto retenido.");
            EditorSceneManager.MarkSceneDirty(scene);
            Require(EditorSceneManager.SaveScene(scene), "No se guardó " + scene.path);
            Debug.Log($"TASK27_RETENTION: {scene.name}, {kept.Count} identidades previas conservadas.");
        }

        private static void ResizeExit(RoomExit exit, float x, float y, float width, float height)
        {
            Move(exit.transform, x, y);
            var collider = exit.GetComponent<BoxCollider2D>(); collider.size = new Vector2(width, height); Record(collider);
        }

        private static void Facing(RoomEntrance entry, bool right)
        {
            var data = new SerializedObject(entry); data.FindProperty("faceRight").boolValue = right;
            data.ApplyModifiedPropertiesWithoutUndo(); Record(entry);
        }

        private static void HorizontalOpening(Transform parent, string name, float x, Color color)
        {
            Transform group = new GameObject(name).transform; group.SetParent(parent, false);
            Visual("Recess", group, Square(), x, 1.25f, 1.6f, 2.5f, new Color(.035f, .05f, .045f), -3);
            Visual("Upper Root", group, Square(), x, 2.65f, 1.8f, .25f, color * 1.5f, 0)
                .transform.rotation = Quaternion.Euler(0, 0, 9);
            Visual("Rock Lip", group, Square(), x, -.12f, 1.8f, .22f, color * 1.35f, 0);
        }

        private static void VerticalOpening(Transform parent, string name, float x, float y, Color color)
        {
            Transform group = new GameObject(name).transform; group.SetParent(parent, false);
            Visual("Shaft Shadow", group, Square(), x, y, 6, 2.4f, new Color(.025f, .04f, .03f), -4);
            for (int side = -1; side <= 1; side += 2)
            {
                Visual("Root Edge", group, Square(), x + side * 3.08f, y + .1f, .24f, 1.8f, color * 1.5f, 0)
                    .transform.rotation = Quaternion.Euler(0, 0, side * 16);
                Visual("Broken Root", group, Square(), x + side * 2.65f, y + .7f, .7f, .18f, color * 1.3f, 0)
                    .transform.rotation = Quaternion.Euler(0, 0, side * 22);
            }
        }
    }
}
