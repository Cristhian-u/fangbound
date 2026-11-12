using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fangbound.Combat;
using Fangbound.Enemies;
using Fangbound.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fangbound.EditorTools
{
    [InitializeOnLoad]
    public static partial class B2EncounterValidation
    {
        private const string Running = "Fangbound.T26.Running";
        private const string CombatMode = "Fangbound.T26.Combat";
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static bool hadError;
        private static double started;

        static B2EncounterValidation()
        {
            EditorApplication.update += Tick;
            Application.logMessageReceived += (_, __, type) =>
            {
                if (SessionState.GetBool(Running, false) &&
                    (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)) hadError = true;
            };
        }

        [MenuItem("Fangbound/Measure Ground Enemy Baseline T26")]
        public static void MeasureBaseline() => StartMeasurement(GroundEnemySceneSetup.DataPath);

        [MenuItem("Fangbound/Measure B2 Enemy Rhythm T26")]
        public static void MeasureB2() => StartMeasurement("Assets/_Project/Data/Enemies/B2GroundEnemyData.asset");

        private static void StartMeasurement(string dataPath)
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode && !Enumerable.Range(0,
                UnityEngine.SceneManagement.SceneManager.sceneCount).Any(i =>
                UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty), "Guardar escenas y salir de Play.");
            EditorSceneManager.OpenScene(GroundEnemySceneSetup.ScenePath);
            // Opening a scene can unload a profile used only by the previous room.
            GroundEnemyData data = AssetDatabase.LoadAssetAtPath<GroundEnemyData>(dataPath);
            Require(data != null, "Faltan los datos que se quieren medir.");
            GroundEnemyController enemy = Object.FindAnyObjectByType<GroundEnemyController>();
            SetData(enemy, data);
            SetData(enemy.GetComponent<EnemyHealth>(), data);
            SessionState.SetBool(CombatMode, false);
            SessionState.SetBool(Running, true);
            Steps.Clear(); hadError = false;
            EditorApplication.EnterPlaymode();
        }

        private static void SetData(Object item, GroundEnemyData data)
        {
            var serialized = new SerializedObject(item);
            serialized.FindProperty("enemyData").objectReferenceValue = data;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(item);
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(Running, false) || !EditorApplication.isPlaying) return;
            try
            {
                Require(!EditorApplication.isPaused, "Play se pausó durante la validación; revisar los errores anteriores.");
                if (Steps.Count == 0)
                {
                    started = EditorApplication.timeSinceStartup;
                    Steps.Push(SessionState.GetBool(CombatMode, false) ? ValidateCombat() : Measure());
                }
                Require(EditorApplication.timeSinceStartup - started < 100, "Tiempo agotado validando T26.");
                IEnumerator current = Steps.Peek();
                if (!current.MoveNext())
                {
                    Steps.Pop();
                    if (Steps.Count == 0) Finish(!hadError);
                }
                else if (current.Current is IEnumerator nested) Steps.Push(nested);
            }
            catch (Exception exception) { Debug.LogException(exception); Finish(false); }
        }

        private static IEnumerator Measure()
        {
            yield return Wait(.2f);
            GroundEnemyController enemy = Object.FindAnyObjectByType<GroundEnemyController>();
            PlayerHealth player = Object.FindAnyObjectByType<PlayerHealth>();
            player.GetComponent<PlayerController>().enabled = false;
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            body.constraints = RigidbodyConstraints2D.FreezeAll;
            // Inside detection/attack range but above the attack shape: no damage,
            // parry, hitstop or knockback changes the neutral timing measurement.
            body.position = (Vector2)enemy.transform.position + new Vector2(1.1f, 1.1f);
            player.transform.position = body.position;
            Physics2D.SyncTransforms();
            foreach (TrainingAttackEmitter trainer in Object.FindObjectsByType<TrainingAttackEmitter>()) trainer.enabled = false;
            var data = (GroundEnemyData)new SerializedObject(enemy).FindProperty("enemyData").objectReferenceValue;
            var attacks = new List<float>();
            var telegraphs = new List<float>();
            GroundEnemyController.EnemyState previous = enemy.State;
            while (attacks.Count < 5)
            {
                GroundEnemyController.EnemyState state = enemy.State;
                if (state != previous)
                {
                    if (state == GroundEnemyController.EnemyState.Telegraph) telegraphs.Add(Time.fixedTime);
                    if (state == GroundEnemyController.EnemyState.Attack) attacks.Add(Time.fixedTime);
                    previous = state;
                }
                yield return null;
            }
            float[] intervals = attacks.Skip(1).Select((time, i) => time - attacks[i]).ToArray();
            float mean = intervals.Average();
            Require(player.CurrentHealth == player.MaximumHealth && mean > data.TelegraphDuration + data.RecoveryDuration,
                "La medición fue alterada por daño o saltó estados.");
            Debug.Log($"TASK26_RHYTHM: data={data.name}; telegraph={data.TelegraphDuration:F2}; active={data.ActiveDuration:F2}; recovery={data.RecoveryDuration:F2}; range={data.AttackRange:F2}; chase={data.ChaseSpeed:F2}; intervals=[{string.Join(",", intervals.Select(x => x.ToString("F3")))}]; mean={mean:F3}; attacks/min={60 / mean:F2}; actualTelegraph={attacks[0] - telegraphs[0]:F3}; physicsStep={Time.fixedDeltaTime:F3}");
        }

        private static IEnumerator Wait(float seconds)
        {
            double end = EditorApplication.timeSinceStartup + seconds;
            while (EditorApplication.timeSinceStartup < end) yield return null;
        }

        private static void Finish(bool success)
        {
            ReleaseInput();
            SessionState.SetBool(Running, false); Steps.Clear();
            string prefix = SessionState.GetBool(CombatMode, false) ? "TASK26_PLAYMODE_" : "TASK26_MEASUREMENT_";
            Debug.Log(prefix + (success ? "PASS" : "FAIL"));
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
            else
            {
                if (!SessionState.GetBool(CombatMode, false))
                    EditorApplication.playModeStateChanged += RestoreMeasurementScene;
                EditorApplication.ExitPlaymode();
            }
        }

        private static void RestoreMeasurementScene(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.playModeStateChanged -= RestoreMeasurementScene;
            // Discard the unsaved measurement-only data override on MovementTest.
            EditorSceneManager.OpenScene(GroundEnemySceneSetup.ScenePath);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
