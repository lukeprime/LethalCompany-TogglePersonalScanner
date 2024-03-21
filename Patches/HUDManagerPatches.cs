using HarmonyLib;
using UnityEngine.InputSystem;

namespace TogglePersonalScanner.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatches
    {
        static InputAction.CallbackContext pingContext;
        static InputAction pingScanAction;
        static bool toggleScan = false;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartPatch(HUDManager __instance)
        {
            pingScanAction = IngamePlayerSettings.Instance.playerInput.actions.FindAction("PingScan");
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePatch(HUDManager __instance)
        {
            if (GameNetworkManager.Instance.localPlayerController == null
                || GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
            {
                // Don't run our toggle scan logic while inside the ship
                return;
            }

            if (pingScanAction.WasPressedThisFrame())
            {

                toggleScan = !toggleScan;
            }

            if (!pingScanAction.IsPressed())
            {
                if (toggleScan && __instance.playerPingingScan <= 0)
                {
                    if (__instance.playersManager.shipIsLeaving)
                    {
                        toggleScan = false;
                    }
                    else
                    {
                        __instance.playerPingingScan = 0.3f;
                    }
                }
            }
            else
            {
                if (__instance.playerPingingScan <= -1)
                {
                    // Scan button is pressed so call PingScan_performed
                    toggleScan = false;
                    __instance.PingScan_performed(pingContext);
                }
            }

            return;
        }

        // Patch the HudManager.PingScan_performed function so we can capture the context for future reuse
        [HarmonyPatch(typeof(HUDManager), "PingScan_performed")]
        [HarmonyPrefix]
        static void OnScan(HUDManager __instance, InputAction.CallbackContext context)
        {
            pingContext = context;
        }

        [HarmonyPatch(typeof(HUDManager), "HideHUD")]
        [HarmonyPostfix]
        static void HideHUDPatch()
        {
            // HUD is being hidden, either player has died or round has ended, so turn off scan toggle
            toggleScan = false;
        }
    }
}
