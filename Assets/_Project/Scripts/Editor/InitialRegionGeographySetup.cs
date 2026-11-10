using System;
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
        private const string Folder = RoomCircuitSceneSetup.SceneFolder;
        private const string APath = Folder + "/Arboleda_A_ClaroDespertar.unity";
        private const string BPath = Folder + "/Arboleda_B_ClaroSendas.unity";
        private const string CPath = Folder + "/Arboleda_C_RaicesProfundas.unity";
        private static readonly Color BSoil = new Color(.16f, .25f, .23f);
        private static readonly Color CSoil = new Color(.25f, .23f, .16f);

        [MenuItem("Fangbound/Apply Initial Region Geography T22 (once)")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty))
                throw new InvalidOperationException("Guardar escenas abiertas y salir de Play antes de aplicar T22.");

            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/_Project/Art/Placeholder/MovementTestSquare.asset");
            if (square == null) throw new InvalidOperationException("Falta el sprite provisional.");

            VerifyOriginalA();
            VerifyOriginalB();
            VerifyOriginalC();

            Scene bScene = EditorSceneManager.OpenScene(BPath, OpenSceneMode.Single);
            ApplyB(square);
            EditorSceneManager.MarkSceneDirty(bScene);
            if (!EditorSceneManager.SaveScene(bScene)) throw new InvalidOperationException("No se guardó B.");

            Scene cScene = EditorSceneManager.OpenScene(CPath, OpenSceneMode.Single);
            ApplyC(square);
            EditorSceneManager.MarkSceneDirty(cScene);
            if (!EditorSceneManager.SaveScene(cScene)) throw new InvalidOperationException("No se guardó C.");

            VerifySaved();
            Debug.Log("TASK22_SETUP_PASS: A sin cambios, B con sendero y pozo, C con llegada/retorno vertical; enlaces conservados.");
        }

        private static void VerifyOriginalA()
        {
            EditorSceneManager.OpenScene(APath, OpenSceneMode.Single);
            RoomScene room = RequireRoom("A", 1);
            Require(room.FindEntrance("from-B") != null && GameObject.Find("Colmillo Pickup") != null,
                "A perdió el Colmillo o la entrada desde B.");
        }

        private static void VerifyOriginalB()
        {
            EditorSceneManager.OpenScene(BPath, OpenSceneMode.Single);
            RoomScene room = RequireRoom("B", 3);
            Require(GameObject.Find("T22 Root Descent") == null &&
                GameObject.Find("T20 Awakening Connection") != null &&
                room.FindEntrance("from-A") != null && room.FindEntrance("from-C") != null &&
                GameObject.Find("Memory Flame - Claro") != null,
                "B ya fue adaptada o perdió accesos/Llama.");
            CheckBounds("Safety Floor", new Vector2(12, -1), new Vector2(24, 2));
            CheckPosition("Exit B-to-A", new Vector2(1.2f, 6.3f));
            CheckPosition("Exit B-to-C", new Vector2(1.2f, 1));
        }

        private static void VerifyOriginalC()
        {
            EditorSceneManager.OpenScene(CPath, OpenSceneMode.Single);
            RoomScene room = RequireRoom("C", 3);
            Require(GameObject.Find("T22 Upper Roots") == null &&
                room.FindEntrance("from-B") != null && room.FindEntrance("from-D") != null &&
                room.FindEntrance("from-F") != null,
                "C ya fue adaptada o perdió entradas técnicas.");
            CheckBounds("C Upper", new Vector2(3.5f, 7.85f), new Vector2(7, .3f));
            CheckPosition("Exit C-to-B", new Vector2(1.2f, 9));
        }

        private static void ApplyB(Sprite square)
        {
            RoomScene room = RequireRoom("B", 3);
            Object.DestroyImmediate(GameObject.Find("T20 Awakening Connection"));
            Transform floor = GameObject.Find("Safety Floor").transform;
            floor.position = new Vector3(4.5f, -1);
            floor.localScale = new Vector3(9, 2, 1);

            Transform group = new GameObject("T22 Root Descent").transform;
            group.SetParent(GameObject.Find("Level Geometry").transform, false);
            Solid("B East Floor", group, square, 19.5f, -1, 9, 2, BSoil);
            Solid("B Root Pit Floor", group, square, 12, -5, 6, 2, BSoil);
            Solid("B Root Pit West Wall", group, square, 8.85f, -3, .3f, 2, BSoil);
            Solid("B Root Pit East Wall", group, square, 15.15f, -3, .3f, 2, BSoil);
            Platform("B Root West Ledge", group, square, 9.75f, 1.5f, -2, BSoil);
            Platform("B Root East Ledge", group, square, 14, 2, -2, BSoil);
            Visual("Root Descent Backdrop", group, square, 12, -2, 5.8f, 4,
                new Color(.09f, .17f, .14f), -7);

            Move(room.FindEntrance("from-A").transform, 3, .58f);
            Move(room.FindEntrance("from-C").transform, 12.3f, -3.42f);
            Move(FindExit(room, "B-to-A").transform, 1.2f, 1);
            Move(FindExit(room, "B-to-C").transform, 11, -3);

            BoxCollider2D cameraBounds = room.GetComponentInChildren<CameraFollow2D>()
                .transform.parent.Find("Camera Room Bounds").GetComponent<BoxCollider2D>();
            cameraBounds.transform.position = new Vector3(12, 3);
            cameraBounds.size = new Vector2(24, 18);
            Record(cameraBounds.transform);
            Record(cameraBounds);
            Record(floor);
        }

        private static void ApplyC(Sprite square)
        {
            RoomScene room = RequireRoom("C", 3);
            Transform group = new GameObject("T22 Upper Roots").transform;
            group.SetParent(GameObject.Find("Level Geometry").transform, false);
            Platform("C Return Step", group, square, 8, 3, 10, CSoil);
            Platform("C Arrival Landing", group, square, 13, 3, 10, CSoil);
            Platform("C Return Ledge", group, square, 11, 3, 12, CSoil);
            Solid("C Upper Roof West", group, square, 3.5f, 12.3f, 7, .6f, CSoil);
            Solid("C Upper Roof East", group, square, 19.5f, 12.3f, 9, .6f, CSoil);
            Move(room.FindEntrance("from-B").transform, 13, 10.58f);
            Move(FindExit(room, "C-to-B").transform, 11, 13);
        }

        private static void VerifySaved()
        {
            EditorSceneManager.OpenScene(BPath, OpenSceneMode.Single);
            RoomScene b = RequireRoom("B", 3);
            CheckPosition("Exit B-to-A", new Vector2(1.2f, 1));
            CheckPosition("Exit B-to-C", new Vector2(11, -3));
            Require(GameObject.Find("T20 Awakening Connection") == null &&
                GameObject.Find("T22 Root Descent") != null &&
                b.FindEntrance("from-A").IsSafe(b.Player) &&
                b.FindEntrance("from-C").IsSafe(b.Player), "B guardada con entradas inseguras.");

            EditorSceneManager.OpenScene(CPath, OpenSceneMode.Single);
            RoomScene c = RequireRoom("C", 3);
            CheckPosition("Exit C-to-B", new Vector2(11, 13));
            Require(GameObject.Find("T22 Upper Roots") != null &&
                c.FindEntrance("from-B").IsSafe(c.Player) &&
                c.FindEntrance("from-D").IsSafe(c.Player) &&
                c.FindEntrance("from-F").IsSafe(c.Player), "C guardada con entradas inseguras.");
            EditorSceneManager.OpenScene(APath, OpenSceneMode.Single);
        }

        private static RoomScene RequireRoom(string id, int exitCount)
        {
            RoomScene room = Object.FindAnyObjectByType<RoomScene>();
            Require(room != null && room.Definition != null && room.Definition.RoomId == id &&
                room.GetComponentsInChildren<RoomExit>().Length == exitCount,
                "Sala o salidas inesperadas en " + id);
            return room;
        }

        private static RoomExit FindExit(RoomScene room, string id)
            => room.GetComponentsInChildren<RoomExit>().Single(exit => exit.ConnectionId == id);

        private static void CheckPosition(string name, Vector2 expected)
        {
            GameObject item = GameObject.Find(name);
            Require(item != null && Vector2.Distance(item.transform.position, expected) < .02f,
                "Posición inesperada: " + name);
        }

        private static void CheckBounds(string name, Vector2 center, Vector2 size)
        {
            Collider2D collider = GameObject.Find(name)?.GetComponent<Collider2D>();
            Require(collider != null && Vector2.Distance(collider.bounds.center, center) < .02f &&
                Vector2.Distance(collider.bounds.size, size) < .02f, "Geometría inesperada: " + name);
        }

        private static void Move(Transform item, float x, float y)
        {
            item.position = new Vector3(x, y, item.position.z);
            Record(item);
        }

        private static void Record(Object item)
        {
            EditorUtility.SetDirty(item);
            if (PrefabUtility.IsPartOfPrefabInstance(item))
                PrefabUtility.RecordPrefabInstancePropertyModifications(item);
        }

        private static void Platform(string name, Transform parent, Sprite square,
            float x, float width, float top, Color color)
        {
            Solid(name, parent, square, x, top - .15f, width, .3f, color);
            Visual(name + " Cap", parent, square, x, top - .03f, width, .06f, color * 1.5f, 0);
        }

        private static void Solid(string name, Transform parent, Sprite square,
            float x, float y, float width, float height, Color color)
            => Visual(name, parent, square, x, y, width, height, color, -1)
                .gameObject.AddComponent<BoxCollider2D>();

        private static SpriteRenderer Visual(string name, Transform parent, Sprite square,
            float x, float y, float width, float height, Color color, int order)
        {
            SpriteRenderer renderer = new GameObject(name).AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(parent, false);
            renderer.transform.position = new Vector3(x, y);
            renderer.transform.localScale = new Vector3(width, height, 1);
            renderer.sprite = square;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
