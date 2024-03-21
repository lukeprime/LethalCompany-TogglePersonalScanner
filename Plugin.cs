using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

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

            _harmony.PatchAll();
        }
    }
}
