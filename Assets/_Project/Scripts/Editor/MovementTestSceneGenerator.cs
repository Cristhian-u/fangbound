using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Fangbound.Player;

namespace Fangbound.EditorTools
{
    public static class MovementTestSceneGenerator
    {
        private const string ProjectRoot = "Assets/_Project";
        private const string ScenePath = "Assets/_Project/Scenes/Test/MovementTest.unity";
        private const string SpriteAssetPath = "Assets/_Project/Art/Placeholder/MovementTestSquare.asset";
        private const string MovementDataPath = "Assets/_Project/Data/Player/PlayerMovementData.asset";
        private const string PlayerPhysicsMaterialPath = "Assets/_Project/Data/Player/PlayerNoFriction.physicsMaterial2D";
        private const string InputActionsPath = "Assets/Settings/InputSystem_Actions.inputactions";

        [MenuItem("Fangbound/Generate Movement Test Scene")]
        public static void Generate()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));

            Sprite squareSprite = EnsurePlaceholderSprite();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MovementTest";

            CreateCamera();
            Transform environment = CreateGroup("Environment");
            CreateBlock(environment, "Ground - Start", new Vector2(-5.7f, -2.5f), new Vector2(7f, 0.8f), new Color(0.25f, 0.38f, 0.31f), squareSprite);
            CreateBlock(environment, "Ground - Far Side", new Vector2(4.5f, -2.5f), new Vector2(8f, 0.8f), new Color(0.25f, 0.38f, 0.31f), squareSprite);
            CreateBlock(environment, "Platform - Low", new Vector2(-0.4f, -0.25f), new Vector2(2.8f, 0.35f), new Color(0.36f, 0.48f, 0.39f), squareSprite);
            CreateBlock(environment, "Platform - Mid", new Vector2(2.8f, 1.1f), new Vector2(2.8f, 0.35f), new Color(0.36f, 0.48f, 0.39f), squareSprite);
            CreateBlock(environment, "Platform - High", new Vector2(5.8f, 2.55f), new Vector2(3.2f, 0.35f), new Color(0.36f, 0.48f, 0.39f), squareSprite);
            CreateBlock(environment, "Wall - End", new Vector2(8.1f, 0.7f), new Vector2(0.45f, 5.4f), new Color(0.31f, 0.36f, 0.33f), squareSprite);

            Transform playerRoot = CreateGroup("Player Setup");
            GameObject spawnPoint = new GameObject("Player Spawn Point");
            spawnPoint.transform.SetParent(playerRoot, false);
            spawnPoint.transform.position = new Vector3(-7.3f, -1.45f, 0f);

            CreatePlayer(playerRoot, squareSprite, spawnPoint.transform.position);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            VerifySceneContents();
            Debug.Log($"MovementTest creada y verificada: {ScenePath}");
        }

        [MenuItem("Fangbound/Configure Movement Test Player")]
        public static void ConfigureMovementTestPlayer()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject player = GameObject.Find("Salamander Placeholder");
            GameObject directionIndicator = GameObject.Find("Facing Direction");
            GameObject environment = GameObject.Find("Environment");
            if (player == null || directionIndicator == null || environment == null)
            {
                throw new InvalidOperationException("No se encontró el personaje, el entorno o el indicador en MovementTest.");
            }

            if (GameObject.Find("Ceiling - Test") == null)
            {
                Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteAssetPath);
                if (squareSprite == null)
                {
                    throw new InvalidOperationException($"No se encontró el sprite de prueba: {SpriteAssetPath}");
                }

                CreateBlock(
                    environment.transform,
                    "Ceiling - Test",
                    new Vector2(-5.6f, 0.45f),
                    new Vector2(3.4f, 0.3f),
                    new Color(0.31f, 0.36f, 0.33f),
                    squareSprite);
            }

            PlayerMovementData movementData = AssetDatabase.LoadAssetAtPath<PlayerMovementData>(MovementDataPath);
            if (movementData == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(MovementDataPath));
                movementData = ScriptableObject.CreateInstance<PlayerMovementData>();
                AssetDatabase.CreateAsset(movementData, MovementDataPath);
            }

            ConfigureMovementData(movementData);

            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
            {
                throw new InvalidOperationException($"No se encontró el asset de Input Actions: {InputActionsPath}");
            }

            InputActionMap playerMap = inputActions.FindActionMap("Player", true);
            playerMap.FindAction("Move", true);
            playerMap.FindAction("Jump", true);
            playerMap.FindAction("Dash", true);

            int defaultLayer = LayerMask.NameToLayer("Default");
            if (defaultLayer < 0)
            {
                throw new InvalidOperationException("No se encontró la capa Default para la detección del suelo.");
            }

            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller == null)
            {
                controller = player.AddComponent<PlayerController>();
            }

            PlayerAbilities abilities = player.GetComponent<PlayerAbilities>();
            if (abilities == null)
            {
                abilities = player.AddComponent<PlayerAbilities>();
            }

            abilities.SetDashUnlocked(true);
            abilities.SetDoubleJumpUnlocked(true);
            EditorUtility.SetDirty(abilities);

            SerializedObject serializedController = new SerializedObject(controller);
            SetObjectReference(serializedController, "movementData", movementData);
            SetObjectReference(serializedController, "inputActions", inputActions);
            SetObjectReference(serializedController, "playerRenderer", player.GetComponent<SpriteRenderer>());
            SetObjectReference(serializedController, "facingIndicator", directionIndicator.transform);
            serializedController.FindProperty("groundLayers").intValue = 1 << defaultLayer;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            VerifyMovementConfiguration(player, controller, movementData, inputActions, directionIndicator.transform, abilities);
            Debug.Log($"Controlador y datos de movimiento enlazados en {ScenePath}");
        }

        private static void ConfigureMovementData(PlayerMovementData movementData)
        {
            SerializedObject serializedData = new SerializedObject(movementData);
            serializedData.FindProperty("moveSpeed").floatValue = 7.5f;
            serializedData.FindProperty("doubleJumpVelocity").floatValue = 12f;
            serializedData.FindProperty("dashDistance").floatValue = 3.2f;
            serializedData.FindProperty("dashSpeed").floatValue = 18f;
            serializedData.FindProperty("dashDuration").floatValue = 0.18f;
            serializedData.FindProperty("dashCooldown").floatValue = 0.35f;
            serializedData.FindProperty("dashHasInvulnerability").boolValue = true;
            serializedData.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(movementData);
        }

        [MenuItem("Fangbound/Apply No-Friction Player Material")]
        public static void ApplyNoFrictionPlayerMaterial()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject player = GameObject.Find("Salamander Placeholder");
            CapsuleCollider2D playerCollider = player != null ? player.GetComponent<CapsuleCollider2D>() : null;
            if (playerCollider == null)
            {
                throw new InvalidOperationException("No se encontró CapsuleCollider2D en Salamander Placeholder.");
            }

            PhysicsMaterial2D material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(PlayerPhysicsMaterialPath);
            if (material == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PlayerPhysicsMaterialPath));
                material = new PhysicsMaterial2D("PlayerNoFriction");
                AssetDatabase.CreateAsset(material, PlayerPhysicsMaterialPath);
            }

            material.friction = 0f;
            material.bounciness = 0f;
            material.frictionCombine = PhysicsMaterialCombine2D.Minimum;
            material.bounceCombine = PhysicsMaterialCombine2D.Minimum;
            playerCollider.sharedMaterial = material;
            EditorUtility.SetDirty(material);
            EditorUtility.SetDirty(playerCollider);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();

            if (playerCollider.sharedMaterial != material ||
                material.friction != 0f ||
                material.bounciness != 0f ||
                material.frictionCombine != PhysicsMaterialCombine2D.Minimum ||
                material.bounceCombine != PhysicsMaterialCombine2D.Minimum)
            {
                throw new InvalidOperationException("No se pudo verificar el material sin fricción del jugador.");
            }

            Debug.Log($"Material sin fricción aplicado al jugador: {PlayerPhysicsMaterialPath}");
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"No existe el campo serializado '{propertyName}'.");
            }

            property.objectReferenceValue = value;
        }

        private static void VerifyMovementConfiguration(
            GameObject player,
            PlayerController controller,
            PlayerMovementData movementData,
            InputActionAsset inputActions,
            Transform directionIndicator,
            PlayerAbilities abilities)
        {
            if (player.GetComponent<Rigidbody2D>() == null || player.GetComponent<Collider2D>() == null ||
                abilities == null || !abilities.DashUnlocked || !abilities.DoubleJumpUnlocked)
            {
                throw new InvalidOperationException("El personaje perdió su Rigidbody2D o Collider2D.");
            }

            SerializedObject serializedController = new SerializedObject(controller);
            if (serializedController.FindProperty("movementData").objectReferenceValue != movementData ||
                serializedController.FindProperty("inputActions").objectReferenceValue != inputActions ||
                serializedController.FindProperty("facingIndicator").objectReferenceValue != directionIndicator)
            {
                throw new InvalidOperationException("No se guardaron todas las referencias del PlayerController.");
            }
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 1.2f, -10f);

            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            camera.backgroundColor = new Color(0.09f, 0.13f, 0.12f);
            camera.clearFlags = CameraClearFlags.SolidColor;
        }

        private static Transform CreateGroup(string name)
        {
            GameObject group = new GameObject(name);
            return group.transform;
        }

        private static Sprite EnsurePlaceholderSprite()
        {
            Sprite existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteAssetPath);
            if (existingSprite != null)
            {
                return existingSprite;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(SpriteAssetPath));

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "MovementTestSquareTexture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            AssetDatabase.CreateAsset(texture, SpriteAssetPath);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            sprite.name = "MovementTestSquare";
            AssetDatabase.AddObjectToAsset(sprite, texture);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath<Sprite>(SpriteAssetPath);
        }

        private static void CreateBlock(Transform parent, string name, Vector2 position, Vector2 size, Color color, Sprite sprite)
        {
            GameObject block = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D));
            block.transform.SetParent(parent, false);
            block.transform.position = position;
            block.transform.localScale = size;

            BoxCollider2D collider = block.GetComponent<BoxCollider2D>();
            collider.size = Vector2.one;

            SpriteRenderer renderer = block.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = 0;
        }

        private static void CreatePlayer(Transform parent, Sprite sprite, Vector3 position)
        {
            GameObject player = new GameObject("Salamander Placeholder", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CapsuleCollider2D));
            player.transform.SetParent(parent, false);
            player.transform.position = position;
            player.transform.localScale = new Vector3(0.7f, 1.1f, 1f);

            SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.12f, 0.17f, 0.16f);
            renderer.sortingOrder = 2;

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CapsuleCollider2D capsule = player.GetComponent<CapsuleCollider2D>();
            capsule.direction = CapsuleDirection2D.Vertical;
            capsule.size = new Vector2(0.9f, 0.95f);

            GameObject directionMarker = new GameObject("Facing Direction", typeof(SpriteRenderer));
            directionMarker.transform.SetParent(player.transform, false);
            directionMarker.transform.localPosition = new Vector3(0.62f, 0.12f, 0f);
            directionMarker.transform.localScale = new Vector3(0.32f, 0.12f, 1f);

            SpriteRenderer markerRenderer = directionMarker.GetComponent<SpriteRenderer>();
            markerRenderer.sprite = sprite;
            markerRenderer.color = new Color(0.82f, 0.68f, 0.32f);
            markerRenderer.sortingOrder = 3;
        }

        private static void VerifySceneContents()
        {
            string[] requiredObjects =
            {
                "Main Camera",
                "Ground - Start",
                "Ground - Far Side",
                "Platform - Low",
                "Platform - Mid",
                "Platform - High",
                "Wall - End",
                "Player Spawn Point",
                "Salamander Placeholder",
                "Facing Direction"
            };

            foreach (string objectName in requiredObjects)
            {
                if (GameObject.Find(objectName) == null)
                {
                    throw new InvalidOperationException($"Falta el objeto requerido en MovementTest: {objectName}");
                }
            }

            if (GameObject.Find("Salamander Placeholder").GetComponent<Collider2D>() == null ||
                GameObject.Find("Ground - Start").GetComponent<Collider2D>() == null)
            {
                throw new InvalidOperationException("Faltan colisionadores 2D en el jugador o el suelo.");
            }
        }
    }
}