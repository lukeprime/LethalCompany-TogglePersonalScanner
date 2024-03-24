using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using System;
using UnityEngine;
using TogglePersonalScanner.Patches;

namespace TogglePersonalScanner
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        private readonly Harmony _harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            // Plugin startup logic
            Plugin.Log = base.Logger;

            Plugin.Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder codeBaseUri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(codeBaseUri.Path);
            string basePath = Path.GetDirectoryName(path);

            byte[] imageData = File.ReadAllBytes(Path.Combine(basePath, "scanner.png"));
            Texture2D crouchTexture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
            crouchTexture.LoadImage(imageData);
            crouchTexture.filterMode = FilterMode.Point;
            HUDManagerPatches.scannerIcon = Sprite.Create(
                crouchTexture,
                new Rect(0, 0, crouchTexture.width, crouchTexture.height),
                new Vector2(0, 0)
            );

            _harmony.PatchAll();
        }
    }
}
