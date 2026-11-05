using System;
using System.Collections;
using UnityEngine;

namespace BuildPractice
{
    // Подпись в углу экрана: на какой платформе запущен билд и когда он собран. Время сборки и платформу перед каждой
    // сборкой записывает BuildScript в Resources/BuildInfo.txt, так что видно, что запущен именно получившийся билд.
    // Текст на английском: у встроенного шрифта в WebGL нет кириллицы (в браузере нет системных шрифтов).
    // Для проверки без участия человека: с ключом -screenshot <файл> билд через 2 секунды сохраняет кадр и закрывается.
    public class BuildInfoLabel : MonoBehaviour
    {
        string text;
        GUIStyle style;

        void Awake()
        {
            var info = Resources.Load<TextAsset>("BuildInfo");
            text = $"{Application.productName} {Application.version}\n" +
                   $"Platform: {Application.platform}\n" +
                   (info != null ? info.text : "Running in the editor");
        }

        IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-screenshot");
            if (index < 0 || index + 1 >= args.Length) yield break;
            yield return new WaitForSeconds(2f);
            ScreenCapture.CaptureScreenshot(args[index + 1]);
            yield return new WaitForSeconds(1f);
            Application.Quit();
        }

        void OnGUI()
        {
            style ??= new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, wordWrap = true };
            style.fontSize = Mathf.Max(14, Mathf.RoundToInt(Mathf.Min(Screen.width, Screen.height) / 24f));
            float margin = style.fontSize;
            var area = new Rect(margin, margin, Screen.width - 2f * margin, Screen.height - 2f * margin);
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.Label(new Rect(area.x + 2f, area.y + 2f, area.width, area.height), text, style);
            GUI.color = Color.white;
            GUI.Label(area, text, style);
        }
    }
}
