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
    public static class RuinsRoomSceneSetup
    {
        public const string ScenePath = RoomCircuitSceneSetup.SceneFolder + "/Arboleda_D_RuinasColmillo.unity";
        public const string DataPath = RoomCircuitSceneSetup.DataFolder + "/Arboleda_D_RuinasColmillo.asset";
        private static readonly Color stone = new Color(.35f, .39f, .35f);
        private static Sprite square;

        [MenuItem("Fangbound/Add Ruins Room D for T18 (once)")]
        public static void Generate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Ejecutar en Edit Mode.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Guardar cambios antes de ampliar C.");
            if (File.Exists(ScenePath) || File.Exists(DataPath)) throw new InvalidOperationException("D ya existe; no se sobrescribe.");
            string cPath = RoomCircuitSceneSetup.SceneFolder + "/" + RoomCircuitSceneSetup.RoomNames[1] + ".unity";
            var cScene = EditorSceneManager.OpenScene(cPath);
            RoomScene cRoom = Object.FindAnyObjectByType<RoomScene>();
            if (cRoom == null || cRoom.Definition == null || cRoom.Definition.RoomId != "C" ||
                cRoom.GetComponentsInChildren<RoomExit>().Length != 2 || cRoom.FindEntrance("from-D") != null ||
                GameObject.Find("T18 Ruins Connection") != null)
                throw new InvalidOperationException("Se requiere la C aprobada de T17 sin conexión D aplicada.");
            Bounds terrace = GameObject.Find("C Terrace 6").GetComponent<Collider2D>().bounds;
            if (Vector2.Distance(terrace.center, new Vector2(11,5.85f)) > .02f || Vector2.Distance(terrace.size,new Vector2(4,.3f)) > .02f)
                throw new InvalidOperationException("Revisar cambios manuales en C Terrace 6 antes de añadir la rama.");
            square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Placeholder/MovementTestSquare.asset");
            if (square == null || AssetDatabase.LoadAssetAtPath<GameObject>(RoomCircuitSceneSetup.RigPath) == null)
                throw new InvalidOperationException("Faltan recursos compartidos de T17.");
            var retained = cScene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .ToDictionary(t => t.gameObject, t => GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject));
            RoomDefinition definition = ScriptableObject.CreateInstance<RoomDefinition>();
            AssetDatabase.CreateAsset(definition, DataPath);
            Set(definition, "roomId", "D"); Set(definition, "scenePath", ScenePath);
            Ref(definition, "recoveryRoom", cRoom.Definition.RecoveryRoom);
            AssetDatabase.SaveAssets();
            Transform connection = new GameObject("T18 Ruins Connection").transform;
            connection.SetParent(GameObject.Find("Level Geometry").transform);
            Platform("C Ruins Approach", connection, 16.5f, 3, 6);
            Platform("C Ruins Gate", connection, 22, 4, 6);
            Entry(cRoom.transform, "from-D", 20.5f, 6.58f, false);
            Exit(cRoom.transform, "C-to-D", definition, "from-C", 22.8f, 6);
            foreach (var item in retained)
                if (item.Key == null || !GlobalObjectId.GetGlobalObjectIdSlow(item.Key).Equals(item.Value))
                    throw new InvalidOperationException("Se perdió un objeto aprobado de C.");
            EditorSceneManager.MarkSceneDirty(cScene);
            if (!EditorSceneManager.SaveScene(cScene)) throw new InvalidOperationException("No se guardó la conexión de C.");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Resolve assets again after switching scenes; keep valid native references.
            definition = AssetDatabase.LoadAssetAtPath<RoomDefinition>(DataPath);
            RoomDefinition cDefinition = AssetDatabase.LoadAssetAtPath<RoomDefinition>(RoomCircuitSceneSetup.DataFolder + "/" + RoomCircuitSceneSetup.RoomNames[1] + ".asset");
            GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(RoomCircuitSceneSetup.RigPath));
            Ref(rig.GetComponent<RoomScene>(), "definition", definition);
            BoxCollider2D cameraBounds = rig.GetComponentInChildren<CameraFollow2D>().transform.parent.Find("Camera Room Bounds").GetComponent<BoxCollider2D>();
            cameraBounds.transform.position = new Vector3(11,8.5f);
            cameraBounds.size = new Vector2(22,25);
            PrefabUtility.RecordPrefabInstancePropertyModifications(cameraBounds.transform);
            PrefabUtility.RecordPrefabInstancePropertyModifications(cameraBounds);
            Entry(rig.transform, "from-C", 4, .58f, true);
            Exit(rig.transform, "D-to-C", cDefinition, "from-D", 1.2f, 0);
            Transform geometry = new GameObject("Level Geometry").transform;
            Solid("Safety Floor",geometry,11,-1,22,2);
            Solid("West Boundary",geometry,-.3f,13,.6f,30);
            Solid("East Boundary",geometry,22.3f,13,.6f,30);
            Platform("D Entry Roof",geometry,.95f,1.9f,2.8f);
            Platform("D Base Step",geometry,9,4,1.8f);
            Platform("D West Landing",geometry,4,4,3.6f);
            Platform("D Broken Crossing",geometry,9.5f,5,5.4f);
            Platform("D East Rest",geometry,17,6,7.2f);
            Platform("D Inner Ledge",geometry,10,4,9);
            Platform("D Upper Chamber",geometry,17.75f,7.5f,10.8f);
            Platform("D West Lookout",geometry,3,3,7.2f);
            Transform decor = new GameObject("Provisional Ruin Decoration").transform;
            Color dark = new Color(.12f,.18f,.15f), moss = new Color(.28f,.37f,.22f);
            Visual("Weathered Wall",decor,10,6,12,9,new Color(.10f,.15f,.13f),-9);
            Visual("Entry Broken Column",decor,5.7f,1.1f,.45f,2.2f,stone,-2);
            Visual("Entry Fallen Capital",decor,6.5f,.2f,1,.4f,stone,-2).transform.rotation=Quaternion.Euler(0,0,-12);
            Visual("Lookout Broken Post",decor,1.9f,7.85f,.4f,1.3f,stone,-2);
            Visual("Lookout Fallen Stone",decor,4,7.4f,.5f,.4f,stone,-2);
            Visual("Chamber Left Column",decor,14.5f,11.9f,.45f,2.2f,stone,-2);
            Visual("Chamber Right Column",decor,20.8f,11.9f,.45f,2.2f,stone,-2);
            Visual("Chamber Broken Lintel",decor,17.7f,13.1f,6.9f,.35f,stone,-2).transform.rotation=Quaternion.Euler(0,0,4);
            Visual("Chamber Moss",decor,17.7f,13.38f,6.4f,.1f,moss,-2);
            // A visual recess only: no RoomExit, collider trigger or interaction toward E.
            Visual("Upper Recess",decor,20,11.8f,1.2f,2,dark,-3);
            for (int i=0;i<5;i++)
                Visual("Invading Root",decor,5+i*3.1f,5.5f,.16f,7-i*.5f,moss,-5).transform.rotation=Quaternion.Euler(0,0,18-i*7);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene,ScenePath)) throw new InvalidOperationException("No se guardó D.");
            var scenes=EditorBuildSettings.scenes.ToList(); scenes.Add(new EditorBuildSettingsScene(ScenePath,true));
            EditorBuildSettings.scenes=scenes.ToArray();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(cPath);
            if (Object.FindAnyObjectByType<RoomScene>().GetComponentsInChildren<RoomExit>().Any(exit=>exit.Destination==null))
                throw new InvalidOperationException("Destino perdido al reabrir C.");
            EditorSceneManager.OpenScene(ScenePath);
            if (Object.FindAnyObjectByType<RoomScene>().Definition==null || Object.FindAnyObjectByType<RoomExit>().Destination==null)
                throw new InvalidOperationException("Referencias perdidas al reabrir D.");
            Debug.Log($"TASK18_SETUP_PASS: D compacta, conexión aditiva C-D y {retained.Count} objetos originales de C conservados.");
        }

        static void Entry(Transform parent,string id,float x,float y,bool right)
        {
            var point=new GameObject("Entry "+id).AddComponent<RoomEntrance>(); point.transform.SetParent(parent); point.transform.position=new Vector3(x,y);
            Set(point,"entranceId",id);
            var data=new SerializedObject(point); data.FindProperty("faceRight").boolValue=right; data.ApplyModifiedPropertiesWithoutUndo();
        }
        static void Exit(Transform parent,string id,RoomDefinition target,string entry,float x,float y)
        {
            var exit=new GameObject("Exit "+id).AddComponent<RoomExit>(); exit.transform.SetParent(parent); exit.transform.position=new Vector3(x,y+1);
            var trigger=exit.GetComponent<BoxCollider2D>(); trigger.size=new Vector2(1.2f,2); trigger.isTrigger=true;
            Set(exit,"connectionId",id); Set(exit,"destinationEntranceId",entry); Ref(exit,"destination",target);
            Visual("Passage Frame",exit.transform,x,y+1.5f,.18f,3,new Color(.5f,.47f,.32f),-2);
        }
        static void Platform(string name,Transform parent,float x,float w,float y)
        { Solid(name,parent,x,y-.15f,w,.3f); Visual(name+" Cap",parent,x,y-.03f,w,.06f,new Color(.44f,.48f,.38f),0); }
        static void Solid(string name,Transform parent,float x,float y,float w,float h)
            => Visual(name,parent,x,y,w,h,stone,-1).gameObject.AddComponent<BoxCollider2D>();
        static SpriteRenderer Visual(string name,Transform parent,float x,float y,float w,float h,Color color,int order)
        {
            var sprite=new GameObject(name).AddComponent<SpriteRenderer>(); sprite.transform.SetParent(parent);
            sprite.transform.position=new Vector3(x,y); sprite.transform.localScale=new Vector3(w,h,1); sprite.sprite=square; sprite.color=color; sprite.sortingOrder=order;
            return sprite;
        }
        static void Ref(Object obj,string field,Object value)
        { var data=new SerializedObject(obj); data.FindProperty(field).objectReferenceValue=value; data.ApplyModifiedPropertiesWithoutUndo(); if(PrefabUtility.IsPartOfPrefabInstance(obj)) PrefabUtility.RecordPrefabInstancePropertyModifications(obj); }
        static void Set(Object obj,string field,string value)
        { var data=new SerializedObject(obj); data.FindProperty(field).stringValue=value; data.ApplyModifiedPropertiesWithoutUndo(); if(PrefabUtility.IsPartOfPrefabInstance(obj)) PrefabUtility.RecordPrefabInstancePropertyModifications(obj); }
    }
}
