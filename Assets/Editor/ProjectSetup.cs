using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BuildPractice.Editor
{
    // Подготовка проекта (выполнена один раз, результат уже лежит в репозитории): демо-сцена, список сцен для сборки и
    // настройки Player Settings для ПК, Android и WebGL.
    public static class ProjectSetup
    {
        const string ScenePath = "Assets/Scenes/Main.unity";
        const string MaterialPath = "Assets/Materials/Cube.mat";

        [MenuItem("Build/Setup/Recreate Demo Scene and Settings", priority = 100)]
        public static void Run()
        {
            PlayerSettings.companyName = "KTSHKI";
            PlayerSettings.productName = "BuildPractice";
            PlayerSettings.bundleVersion = "1.0";
            // Android: свой идентификатор пакета; IL2CPP и ARM64, чтобы APK ставился и на телефоны без поддержки 32 бит.
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.ktshki.buildpractice");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            // WebGL: кастомный шаблон (задание 2) и файлы без сжатия, чтобы билд открывался с любого статического хостинга.
            PlayerSettings.WebGL.template = BuildScript.WebGLTemplate;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            CreateScene();
            AssetDatabase.SaveAssets();
            Debug.Log("[ProjectSetup] Сцена и настройки готовы.");
        }

        static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
            camera.transform.position = new Vector3(0f, 0f, -5f);
            var view = camera.GetComponent<Camera>();
            view.clearFlags = CameraClearFlags.SolidColor;
            view.backgroundColor = new Color(0.11f, 0.13f, 0.19f);

            var sun = new GameObject("Directional Light", typeof(Light));
            sun.GetComponent<Light>().type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Cube";
            cube.transform.localScale = Vector3.one * 1.6f;
            cube.GetComponent<Renderer>().sharedMaterial = CubeMaterial();
            cube.AddComponent<Spin>();

            new GameObject("Build Info", typeof(BuildInfoLabel));

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        static Material CubeMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material != null) return material;
            Directory.CreateDirectory(Path.GetDirectoryName(MaterialPath));
            material = new Material(Shader.Find("Standard")) { color = new Color(1f, 0.55f, 0.15f) };
            AssetDatabase.CreateAsset(material, MaterialPath);
            return material;
        }
    }
}
