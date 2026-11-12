using System;
using System.IO;
using System.Linq;
using Fangbound.Enemies;
using Fangbound.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Fangbound.EditorTools
{
    public static class B2EncounterSceneSetup
    {
        public const string DataPath = "Assets/_Project/Data/Enemies/B2GroundEnemyData.asset";

        [MenuItem("Fangbound/Add B2 Encounter and Discovery T26 (once)")]
        public static void Configure()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode && !Enumerable.Range(0,
                SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty), "Guardar escenas y salir de Play.");
            Require(!File.Exists(DataPath), "Los datos B2 ya existen; no se sobrescriben.");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GroundEnemySceneSetup.PrefabPath);
            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Placeholder/MovementTestSquare.asset");
            Require(prefab != null && square != null, "Faltan prefab o sprite compartidos.");
            Scene scene = EditorSceneManager.OpenScene(InitialRegionGeographySetup.B2ScenePath);
            RoomScene room = Object.FindAnyObjectByType<RoomScene>();
            Require(room != null && room.Definition.RoomId == "B2" &&
                room.GetComponentsInChildren<RoomExit>().Length == 2 &&
                Object.FindObjectsByType<GroundEnemyController>().Length == 0 &&
                Object.FindObjectsByType<LookoutDiscovery>().Length == 0,
                "Se necesita B2 de T25, sin duplicar contenido T26.");
            var retained = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                .ToDictionary(t => t.gameObject, t => GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject));
            Require(AssetDatabase.CopyAsset(GroundEnemySceneSetup.DataPath, DataPath), "No se pudo copiar el perfil base.");
            GroundEnemyData data = AssetDatabase.LoadAssetAtPath<GroundEnemyData>(DataPath);
            var parameters = new SerializedObject(data);
            parameters.FindProperty("recoveryDuration").floatValue = .75f;
            parameters.ApplyModifiedPropertiesWithoutUndo();

            Transform content = new GameObject("T26 Encounter and Discovery").transform;
            var enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefab, content);
            enemy.name = "B2 Ground Enemy";
            enemy.transform.position = new Vector3(7.4f, 1.72f);
            SetReference(enemy.GetComponent<GroundEnemyController>(), "enemyData", data);
            SetReference(enemy.GetComponent<EnemyHealth>(), "enemyData", data);
            SetReference(enemy.GetComponent<GroundEnemyController>(), "target", room.Player);
            var controller = new SerializedObject(enemy.GetComponent<GroundEnemyController>());
            controller.FindProperty("patrolOffsets").vector2Value = new Vector2(-1.2f, 1.2f);
            controller.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(enemy.transform);

            GameObject plant = new GameObject("Lookout Sprout - Examine");
            plant.transform.SetParent(content, false);
            plant.transform.position = new Vector3(13.2f, 5.1f);
            Visual("Stem", plant.transform, square, 0, 0, .08f, .6f, new Color(.63f, .52f, .27f));
            Visual("Left Leaf", plant.transform, square, -.14f, .16f, .34f, .16f,
                new Color(.76f, .66f, .38f)).transform.localRotation = Quaternion.Euler(0, 0, -32);
            Visual("Right Leaf", plant.transform, square, .14f, .3f, .34f, .16f,
                new Color(.9f, .76f, .44f)).transform.localRotation = Quaternion.Euler(0, 0, 32);
            SpriteRenderer marker = Visual("Discovery Glint", plant.transform, square, 0, .65f, .16f, .16f,
                new Color(1, .78f, .36f));
            marker.transform.localRotation = Quaternion.Euler(0, 0, 45);
            LookoutDiscovery discovery = plant.AddComponent<LookoutDiscovery>();
            SetReference(discovery, "marker", marker);
            InputActionAsset actions = (InputActionAsset)new SerializedObject(room.Player.GetComponent<Fangbound.Player.PlayerCombat>())
                .FindProperty("inputActions").objectReferenceValue;
            SetReference(discovery, "inputActions", actions);

            foreach (var item in retained)
                Require(item.Key != null && GlobalObjectId.GetGlobalObjectIdSlow(item.Key).Equals(item.Value),
                    "Se perdió un objeto existente de B2.");
            EditorSceneManager.MarkSceneDirty(scene);
            Require(EditorSceneManager.SaveScene(scene), "No se guardó B2.");
            AssetDatabase.SaveAssets();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            EditorSceneManager.OpenScene(InitialRegionGeographySetup.B2ScenePath);
            Require(Object.FindObjectsByType<GroundEnemyController>().Length == 1 &&
                Object.FindObjectsByType<LookoutDiscovery>().Length == 1, "Contenido perdido al reabrir.");
            Debug.Log("TASK26_SETUP_PASS: B2 conservada; enemigo de prefab en (7.4,1.72), brote en (13.2,5.1), recuperación local 0.75.");
        }

        private static SpriteRenderer Visual(string name, Transform parent, Sprite sprite, float x, float y,
            float width, float height, Color color)
        {
            var item = new GameObject(name, typeof(SpriteRenderer));
            item.transform.SetParent(parent, false);
            item.transform.localPosition = new Vector3(x, y);
            item.transform.localScale = new Vector3(width, height, 1);
            SpriteRenderer renderer = item.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite; renderer.color = color; renderer.sortingOrder = 5;
            return renderer;
        }

        private static void SetReference(Object item, string field, Object reference)
        {
            var serialized = new SerializedObject(item);
            serialized.FindProperty(field).objectReferenceValue = reference;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (PrefabUtility.IsPartOfPrefabInstance(item)) PrefabUtility.RecordPrefabInstancePropertyModifications(item);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
