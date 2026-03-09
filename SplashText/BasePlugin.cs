using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;
using TMPro;
using System.IO;
using System.Linq;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class MenuTextPlugin : BaseUnityPlugin
{
    public const string ModGUID = "denyscrasav4ik.basicallyukrainian.splashtext";
    public const string ModName = "Splash Text";
    public const string ModVersion = "1.0.0";

    public static MenuTextPlugin Instance;

    private void Awake()
    {
        Instance = this;

        Harmony harmony = new Harmony(ModGUID);
        harmony.PatchAll();

        Logger.LogInfo("Splash Text loaded");
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
        rect.anchoredPosition = new Vector2(190, 110);
        rect.rotation = Quaternion.Euler(0, 0, 30);
        rect.sizeDelta = new Vector2(100, 100);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = GetRandomLine();
        tmp.fontSize = 15;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;

        textObj.AddComponent<ScaleAnimator>();
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

        if (!File.Exists(path))
        {
            Logger.LogWarning($"Text file not found: {path}");
            return "Missing text file.";
        }

        string[] lines = File.ReadAllLines(path)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();

        if (lines.Length == 0)
            return "No text lines.";

        return lines[Random.Range(0, lines.Length)];
    }
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
