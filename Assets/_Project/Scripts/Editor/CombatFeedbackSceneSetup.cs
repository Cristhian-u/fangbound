using System;
using System.IO;
using Fangbound.Combat;
using Fangbound.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Fangbound.EditorTools
{
    public static class CombatFeedbackSceneSetup
    {
        internal const string DataPath = "Assets/_Project/Data/Combat/CombatFeedbackData.asset";

        [MenuItem("Fangbound/Configure Movement Test Combat Feedback")]
        public static void Configure()
        {
            CombatFeedbackData data = AssetDatabase.LoadAssetAtPath<CombatFeedbackData>(DataPath);
            if (data == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DataPath));
                AssetDatabase.Refresh();
                data = ScriptableObject.CreateInstance<CombatFeedbackData>();
                AssetDatabase.CreateAsset(data, DataPath);
            }
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Placeholder/MovementTestSquare.asset");
            if (sprite == null) throw new InvalidOperationException("Falta el sprite provisional existente.");

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(GroundEnemySceneSetup.PrefabPath);
            try
            {
                ConfigureVisual(prefabRoot, data, sprite);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, GroundEnemySceneSetup.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefabRoot); }

            var scene = EditorSceneManager.OpenScene(GroundEnemySceneSetup.ScenePath, OpenSceneMode.Single);
            PlayerParry player = UnityEngine.Object.FindAnyObjectByType<PlayerParry>();
            TrainingAttackEmitter trainer = UnityEngine.Object.FindAnyObjectByType<TrainingAttackEmitter>();
            if (player == null || trainer == null) throw new InvalidOperationException("Faltan jugador o entrenador en MovementTest.");
            ConfigureVisual(player.gameObject, data, sprite);
            ConfigureVisual(trainer.gameObject, data, sprite);
            CombatHitstop hitstop = UnityEngine.Object.FindAnyObjectByType<CombatHitstop>();
            if (hitstop == null) hitstop = new GameObject("Combat Hitstop").AddComponent<CombatHitstop>();
            SetReference(hitstop, "feedbackData", data);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            GroundEnemySceneSetup.ValidateScene();
            Debug.Log("TASK10_SETUP_PASS: hitstop y feedback visual enlazados sin reconfigurar combate/movimiento.");
        }

        private static void ConfigureVisual(GameObject actor, CombatFeedbackData data, Sprite sprite)
        {
            Transform signal = actor.transform.Find("Combat Feedback Signal");
            if (signal == null)
            {
                signal = new GameObject("Combat Feedback Signal", typeof(SpriteRenderer)).transform;
                signal.SetParent(actor.transform, false);
            }
            signal.localPosition = new Vector3(0f, 0f, 0.1f);
            signal.localScale = Vector3.one;
            SpriteRenderer renderer = signal.GetComponent<SpriteRenderer>();
            SpriteRenderer body = actor.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = body.sharedMaterial;
            renderer.sortingLayerID = body.sortingLayerID;
            renderer.sortingOrder = body.sortingOrder - 1;
            renderer.enabled = false;
            CombatVisualFeedback feedback = actor.GetComponent<CombatVisualFeedback>();
            if (feedback == null) feedback = actor.AddComponent<CombatVisualFeedback>();
            SetReference(feedback, "feedbackData", data);
            SetReference(feedback, "signalRenderer", renderer);
            EditorUtility.SetDirty(renderer);
        }

        private static void SetReference(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
