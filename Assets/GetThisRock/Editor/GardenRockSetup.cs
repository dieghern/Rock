using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GetThisRock.Editor
{
    public static class GardenRockSetup
    {
        const string Root = "Assets/GetThisRock";
        public const string ScenePath = Root + "/Scenes/GardenRock.unity";
        [MenuItem("Get This Rock/Create Garden Rock Scene")]
        public static void CreateScene()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!Application.isBatchMode && File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Recrear escena", "Esto reemplaza GardenRock por la escena inicial.", "Recrear", "Cancelar")) return;
            Generate();
        }
        public static void Generate()
        {
            Directory.CreateDirectory(Root + "/Scenes");
            Directory.CreateDirectory(Root + "/Data");
            Directory.CreateDirectory(Root + "/Materials");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var grass = Material("Grass", new Color(.27f, .43f, .19f));
            var stone = Material("Stone", new Color(.38f, .40f, .43f));
            var wall = Material("House", new Color(.83f, .73f, .54f));
            var roof = Material("Roof", new Color(.32f, .19f, .14f));
            var road = Material("Road", new Color(.18f, .20f, .22f));
            var yellow = Material("Delivery", new Color(1f, .72f, .1f));
            var white = Material("White", new Color(.93f, .91f, .8f));
            Box("Ground", new Vector3(0, -.25f, 0), new Vector3(30, .5f, 30), grass);
            Box("Road", new Vector3(0, .005f, 10), new Vector3(29, .01f, 5), road, false);
            for (int x = -12; x <= 12; x += 4) Box("Road marking", new Vector3(x, .013f, 10), new Vector3(2, .01f, .12f), white, false);
            Box("House", new Vector3(-6, 1.8f, -2), new Vector3(6, 3.6f, 6), wall);
            Box("Roof", new Vector3(-6, 3.8f, -2), new Vector3(6.6f, .45f, 6.6f), roof);
            Box("Door", new Vector3(-2.98f, 1.1f, -2), new Vector3(.06f, 2.2f, 1.2f), roof);
            Box("Window", new Vector3(-2.95f, 2.3f, 0), new Vector3(.08f, 1, 1.3f), Material("Window", new Color(.3f, .65f, .76f)));
            // Boundary walls prevent an unrecoverable rock falling off the test lot.
            Box("Fence back", new Vector3(0, .8f, -14.5f), new Vector3(29, 1.6f, .25f), wall);
            Box("Fence left", new Vector3(-14.5f, .8f, 0), new Vector3(.25f, 1.6f, 29), wall);
            Box("Fence right", new Vector3(14.5f, .8f, 0), new Vector3(.25f, 1.6f, 29), wall);
            Box("Fence road", new Vector3(0, .8f, 14.5f), new Vector3(29, 1.6f, .25f), wall);

            var physics = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Root + "/Materials/RockPhysics.physicMaterial");
            if (physics == null)
            {
                physics = new PhysicsMaterial("RockPhysics") { dynamicFriction = .65f, staticFriction = .7f, bounciness = 0, frictionCombine = PhysicsMaterialCombine.Average };
                AssetDatabase.CreateAsset(physics, Root + "/Materials/RockPhysics.physicMaterial");
            }
            var rockObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rockObject.name = "Garden Rock - 180 kg";
            rockObject.transform.position = new Vector3(1, 1.05f, 0);
            rockObject.transform.localScale = Vector3.one * 1.8f;
            rockObject.GetComponent<Renderer>().sharedMaterial = stone;
            rockObject.GetComponent<Collider>().sharedMaterial = physics;
            var rb = rockObject.AddComponent<Rigidbody>();
            rb.mass = 180;
            rb.linearDamping = .15f;
            rb.angularDamping = .65f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var physical = rockObject.AddComponent<PhysicalObject>();
            physical.displayName = "Roca de jardín";
            physical.allowPickup = false;
            rockObject.AddComponent<Rock>();

            var systems = new GameObject("Session");
            var wallet = systems.AddComponent<PlayerWallet>();
            var manager = systems.AddComponent<ContractManager>();
            var contract = AssetDatabase.LoadAssetAtPath<Contract>(Root + "/Data/GardenRock.asset");
            if (contract == null) { contract = ScriptableObject.CreateInstance<Contract>(); AssetDatabase.CreateAsset(contract, Root + "/Data/GardenRock.asset"); }
            manager.activeContract = contract;
            manager.wallet = wallet;
            var zoneObject = new GameObject("Roadside Delivery Zone");
            zoneObject.transform.position = new Vector3(1, 2.45f, 6);
            var zoneCollider = zoneObject.AddComponent<BoxCollider>();
            zoneCollider.isTrigger = true;
            zoneCollider.size = new Vector3(5, 5, 3.5f);
            var zone = zoneObject.AddComponent<DeliveryZone>();
            zone.contracts = manager;
            foreach (float x in new[] { -1.5f, 3.5f }) Box("Delivery side", new Vector3(x, .015f, 6), new Vector3(.1f, .02f, 3.5f), yellow, false);
            foreach (float z in new[] { 4.25f, 7.75f }) Box("Delivery edge", new Vector3(1, .015f, z), new Vector3(5, .02f, .1f), yellow, false);
            Box("Delivery sign post", new Vector3(4, .8f, 7), new Vector3(.12f, 1.6f, .12f), roof);
            var sign = new GameObject("Delivery sign");
            sign.transform.position = new Vector3(4, 1.8f, 6.9f);
            var label = sign.AddComponent<TextMesh>();
            label.text = "ROCK DROP\n$100";
            label.fontSize = 48;
            label.characterSize = .065f;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = Color.yellow;

            var player = new GameObject("Player");
            player.transform.position = new Vector3(1, .1f, -4);
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = .32f;
            controller.center = new Vector3(0, .9f, 0);
            controller.stepOffset = .25f;
            var cameraObject = new GameObject("Player Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = new Vector3(0, 1.65f, 0);
            var camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = .05f;
            camera.farClipPlane = 150;
            camera.fieldOfView = 78;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.52f, .72f, .85f);
            cameraObject.AddComponent<AudioListener>();
            var input = player.AddComponent<PlayerInput>();
            var motor = player.AddComponent<PlayerController>();
            motor.input = input;
            motor.view = camera.transform;
            var interaction = player.AddComponent<PlayerInteraction>();
            interaction.input = input;
            interaction.view = camera;
            interaction.playerCollider = controller;
            var hud = systems.AddComponent<GameHUD>();
            hud.contracts = manager;
            hud.wallet = wallet;
            hud.interaction = interaction;
            var light = new GameObject("Sun").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 2;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(48, -30, 0);
            RenderSettings.ambientLight = new Color(.65f, .7f, .78f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            ProgressionSetup.Configure();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("GARDEN_ROCK_SCENE_CREATED");
        }
        static Material Material(string name, Color color)
        {
            string path = Root + "/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name, color = color };
            material.SetFloat("_Smoothness", .15f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
        static GameObject Box(string name, Vector3 position, Vector3 scale, Material material, bool collision = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collision) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }
    }
}
