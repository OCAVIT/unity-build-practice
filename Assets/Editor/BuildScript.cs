using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BuildPractice.Editor
{
    // Автоматическая сборка проекта.
    //   Build/Build All — по очереди собирает билд под ПК (Windows x64) и под Android (APK).
    //   Build/Build PC (Windows), Build/Build Android — то же по отдельности.
    //   Build/Build WebGL (custom template) — WebGL-билд с кастомным шаблоном из Assets/WebGLTemplates (задание 2).
    // Билды складываются в папку Builds рядом с Assets. Те же методы запускаются из командной строки (build.bat):
    //   Unity.exe -batchmode -quit -projectPath <проект> -executeMethod BuildPractice.Editor.BuildScript.BuildAll
    public static class BuildScript
    {
        const string Root = "Builds";
        // Шаблон WebGL: PROJECT: — из папки Assets/WebGLTemplates проекта, APPLICATION: — встроенный в Unity.
        public const string WebGLTemplate = "PROJECT:Modern";
        // Эта функция есть только в index.html шаблона Modern: по ней проверяется, что билд собран именно с ним.
        const string TemplateMarker = "rebuildFullscreenGUI";

        [MenuItem("Build/Build All", priority = 0)]
        public static void BuildAll() => Run("Build All", Windows, Android);

        [MenuItem("Build/Build PC (Windows)", priority = 20)]
        public static void BuildWindows() => Run("Build PC (Windows)", Windows);

        [MenuItem("Build/Build Android", priority = 21)]
        public static void BuildAndroid() => Run("Build Android", Android);

        [MenuItem("Build/Build WebGL (custom template)", priority = 22)]
        public static void BuildWebGL() => Run("Build WebGL", WebGL);

        static string Windows() =>
            Build(BuildTarget.StandaloneWindows64, Path.Combine(Root, "Windows", PlayerSettings.productName + ".exe"));

        static string Android()
        {
            // APK, а не AAB: его можно сразу установить на телефон.
            EditorUserBuildSettings.buildAppBundle = false;
            return Build(BuildTarget.Android, Path.Combine(Root, "Android", PlayerSettings.productName + ".apk"));
        }

        static string WebGL()
        {
            PlayerSettings.WebGL.template = WebGLTemplate;
            var folder = Path.Combine(Root, "WebGL");
            var result = Build(BuildTarget.WebGL, folder);
            var html = File.ReadAllText(Path.Combine(folder, "index.html"));
            if (!html.Contains(TemplateMarker))
                throw new BuildFailedException($"index.html собран не из шаблона {WebGLTemplate}.");
            return result + $"\n  index.html собран из кастомного шаблона {WebGLTemplate}";
        }

        // Собирает включённые сцены из списка сборки (File > Build Profiles > Scene List) под платформу.
        static string Build(BuildTarget target, string location)
        {
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (scenes.Length == 0)
                throw new BuildFailedException("В списке сборки нет сцен (File > Build Profiles > Scene List).");
            WriteBuildInfo(target);
            Debug.Log($"[BuildScript] {target}: сборка в {location}");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = location,
                target = target,
                targetGroup = BuildPipeline.GetBuildTargetGroup(target),
                options = BuildOptions.None,
            });
            var summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"{target}: сборка не удалась ({summary.result}, ошибок: {summary.totalErrors}).");
            return $"{target}: {Path.GetFullPath(location)} ({summary.totalSize / (1024f * 1024f):0.0} МБ, {summary.totalTime:mm\\:ss})";
        }

        // Записывает в Resources/BuildInfo.txt платформу и время сборки: билд показывает их на экране.
        static void WriteBuildInfo(BuildTarget target)
        {
            const string path = "Assets/Resources/BuildInfo.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, $"Build: {target}\nBuilt: {DateTime.Now:dd.MM.yyyy HH:mm}");
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        // Запускает сборки по очереди и возвращает в редакторе платформу, что была выбрана. Итог пишется в консоль
        // и показывается в окне; из командной строки код выхода 0 — всё собрано, 1 — ошибка.
        static void Run(string title, params Func<string>[] builds)
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var active = EditorUserBuildSettings.activeBuildTarget;
            var log = new StringBuilder();
            bool ok = true;
            var started = DateTime.Now;
            try
            {
                foreach (var build in builds) log.AppendLine(build());
            }
            catch (Exception e)
            {
                ok = false;
                log.AppendLine("ОШИБКА: " + e.Message);
            }
            finally
            {
                if (EditorUserBuildSettings.activeBuildTarget != active)
                    EditorUserBuildSettings.SwitchActiveBuildTarget(group, active);
            }
            log.Append($"Всего: {DateTime.Now - started:mm\\:ss}");
            string message = $"[BuildScript] {title}: {(ok ? "готово" : "ошибка")}\n{log}";
            if (ok) Debug.Log(message);
            else Debug.LogError(message);

            if (Application.isBatchMode)
            {
                if (!ok) EditorApplication.Exit(1);
                return;
            }
            EditorUtility.DisplayDialog(title, log.ToString(), "OK");
            if (ok) EditorUtility.RevealInFinder(Root);
        }
    }
}
