using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class MenuTextPlugin : BaseUnityPlugin
{
    public const string ModGUID = "denyscrasav4ik.basicallyukrainian.splashtext";
    public const string ModName = "Splash Text";
    public const string ModVersion = "1.1.1";

    public static MenuTextPlugin Instance;

    public static List<string> ModdedSplashesEN = new List<string>();
    public static List<string> ModdedSplashesUA = new List<string>();

    private void Awake()
    {
        Instance = this;

        Harmony harmony = new Harmony(ModGUID);
        harmony.PatchAll();

        Logger.LogInfo("Splash Text loaded");
    }

    /// <summary>
    /// Registers all splash texts from a file for the English language.
    /// Each line in the file will be treated as a separate splash text.
    /// Empty lines are ignored.
    /// </summary>
    /// <param name="filePath">Absolute path to the text file containing splash texts.</param>
    public static void RegisterSplashesFromFileEN(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Instance?.Logger.LogWarning($"Splash file not found: {filePath}");
            return;
        }

        var lines = File.ReadAllLines(filePath)
            .Where(l => !string.IsNullOrWhiteSpace(l));

        foreach (var line in lines)
        {
            RegisterSplashEN(line);
        }

        Instance?.Logger.LogInfo($"Registered {lines.Count()} EN splashes from file.");
    }

    /// <summary>
    /// Registers all splash texts from a file for the Ukrainian language.
    /// Each line in the file will be treated as a separate splash text.
    /// Empty lines are ignored.
    /// </summary>
    /// <param name="filePath">Absolute path to the text file containing splash texts.</param>
    public static void RegisterSplashesFromFileUA(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Instance?.Logger.LogWarning($"Splash file not found: {filePath}");
            return;
        }

        var lines = File.ReadAllLines(filePath)
            .Where(l => !string.IsNullOrWhiteSpace(l));

        foreach (var line in lines)
        {
            RegisterSplashUA(line);
        }

        Instance?.Logger.LogInfo($"Registered {lines.Count()} UA splashes from file.");
    }

    /// <summary>
    /// Registers a single English splash text that can appear on the main menu.
    /// </summary>
    /// <param name="splash">The splash text to add.</param>
    public static void RegisterSplashEN(string splash)
    {
        if (!string.IsNullOrWhiteSpace(splash))
        {
            ModdedSplashesEN.Add(splash);
            Instance?.Logger.LogInfo($"Registered EN splash: {splash}");
        }
    }

    /// <summary>
    /// Registers a single Ukrainian splash text that can appear on the main menu.
    /// </summary>
    /// <param name="splash">The splash text to add.</param>
    public static void RegisterSplashUA(string splash)
    {
        if (!string.IsNullOrWhiteSpace(splash))
        {
            ModdedSplashesUA.Add(splash);
            Instance?.Logger.LogInfo($"Registered UA splash: {splash}");
        }
    }

    public void CreateTMPObject()
    {
        GameObject canvasObj = GameObject.Find("Menu");
        if (canvasObj == null)
        {
            Logger.LogWarning("Menu canvas not found.");
            return;
        }

        if (canvasObj.transform.Find("RandomMenuText") != null)
            return;

        GameObject textObj = new GameObject("RandomMenuText");
        textObj.transform.SetParent(canvasObj.transform, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(180, 110);
        rect.rotation = Quaternion.Euler(0, 0, 30);
        rect.sizeDelta = new Vector2(100, 100);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = GetRandomLine();
        tmp.fontSize = 15;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;

        textObj.AddComponent<ScaleAnimator>();

        MoveMenuTexts(canvasObj);
    }

    void MoveMenuTexts(GameObject menu)
    {
        Transform version = menu.transform.Find("Version");
        Transform reminder = menu.transform.Find("Reminder");

        float offset = -30f;

        if (version != null)
        {
            RectTransform rect = version.GetComponent<RectTransform>();
            rect.anchoredPosition += new Vector2(0, offset);
        }

        if (reminder != null)
        {
            RectTransform rect = reminder.GetComponent<RectTransform>();
            rect.anchoredPosition += new Vector2(0, offset);
        }
    }

    string GetRandomLine()
    {
        bool ukrainizationInstalled = Chainloader.PluginInfos.ContainsKey("Ukrainization");

        string fileName = ukrainizationInstalled ? "UA.txt" : "EN.txt";

        string path = Path.Combine(
            Application.streamingAssetsPath,
            "Modded",
            ModGUID,
            fileName
        );

        var lines = new System.Collections.Generic.List<string>();

        if (File.Exists(path))
        {
            lines.AddRange(
                File.ReadAllLines(path)
                .Where(l => !string.IsNullOrWhiteSpace(l))
            );
        }
        else
        {
            Logger.LogWarning($"Text file not found: {path}");
        }

        if (ukrainizationInstalled)
            lines.AddRange(ModdedSplashesUA);
        else
            lines.AddRange(ModdedSplashesEN);

        if (lines.Count == 0)
            return "No splash texts.";

        return lines[Random.Range(0, lines.Count)];
    }

    [HarmonyPatch(typeof(MainMenu), "Start")]
    class MainMenuPatch
    {
        static void Postfix()
        {
            if (MenuTextPlugin.Instance != null)
            {
                MenuTextPlugin.Instance.CreateTMPObject();
            }
        }
    }

    public class ScaleAnimator : MonoBehaviour
    {
        float speed = 1.5f;
        float minScale = 0.75f;
        float maxScale = 1.25f;

        void Update()
        {
            float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * speed) + 1f) / 2f);
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
