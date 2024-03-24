using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TogglePersonalScanner.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatches
    {
        static InputAction.CallbackContext pingContext;
        static InputAction pingScanAction;
        static bool toggleScan = false;
        public static Sprite scannerIcon;
        public static UnityEngine.UI.Image scannerImage;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartPatch(HUDManager __instance)
        {
            pingScanAction = IngamePlayerSettings.Instance.playerInput.actions.FindAction("PingScan");

            GameObject hudSelfObject = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/Self");
            GameObject crouchIconObject = new GameObject("CrouchIcon");
            crouchIconObject.transform.SetParent(hudSelfObject.transform, false);
            crouchIconObject.transform.SetAsLastSibling();
            crouchIconObject.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            RectTransform rectTransform = crouchIconObject.AddComponent<RectTransform>();
            scannerImage = crouchIconObject.AddComponent<UnityEngine.UI.Image>();
            scannerImage.sprite = scannerIcon;
            rectTransform.sizeDelta = new Vector2(
                20,
                20
            );
            crouchIconObject.transform.position = hudSelfObject.transform.position + new Vector3(0.18f, 0.10f, 0);
            scannerImage.enabled = false;
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

            if (toggleScan)
            {
                scannerImage.enabled = true;
            }
            else
            {
                scannerImage.enabled = false;
            }
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
