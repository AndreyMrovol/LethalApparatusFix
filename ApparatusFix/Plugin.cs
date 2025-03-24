using System;
using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace ApparatusFix
{
    [BepInDependency(PluginInfos.LOBBY_COMPATIBILITY, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginInfos.PLUGIN_GUID, PluginInfos.PLUGIN_NAME, PluginInfos.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource logger;
        internal static Harmony harmony = new(PluginInfos.PLUGIN_GUID);

        private void Awake()
        {
            logger = Logger;
            harmony.PatchAll();

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfos.PLUGIN_GUID} is loaded!");
        }
    }

    internal class LobbyCompatibilityCompatibility
    {
        public static void Init()
        {
            Plugin.logger.LogWarning("LobbyCompatibility detected, registering plugin with LobbyCompatibility.");

            Version pluginVersion = Version.Parse(PluginInfos.PLUGIN_VERSION);

            LobbyCompatibility.Features.PluginHelper.RegisterPlugin(PluginInfos.PLUGIN_GUID, pluginVersion, LobbyCompatibility.Enums.CompatibilityLevel.ClientOptional, LobbyCompatibility.Enums.VersionStrictness.None);
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB))]
    public class Patch
    {

        public static void Disable(GrabbableObject item)
        {
            if ("LungProp" == item.__getTypeName())
            {
                LungProp itemLung = (LungProp)item;

                if (itemLung.GetComponent<AudioSource>().isPlaying)
                {
                    Plugin.logger.LogDebug("Disabling sound of LungProp");

                    itemLung.isLungDocked = false;
                    itemLung.isLungDockedInElevator = false;
                    itemLung.isLungPowered = false;
                    itemLung.GetComponent<AudioSource>().Stop();
                }
            }
        }

        [HarmonyPatch("GrabObjectServerRpc")]
        [HarmonyPostfix]
        public static void EquipItemServerRpc(PlayerControllerB __instance, ref NetworkObjectReference grabbedObject)
        {
            if (grabbedObject.TryGet(out var networkObject) && (bool)networkObject.GetComponentInChildren<GrabbableObject>())
            {
                Plugin.logger.LogDebug($"EquipItemServerRpc {networkObject.GetComponentInChildren<GrabbableObject>().itemProperties.itemName}");

                Disable(networkObject.GetComponentInChildren<GrabbableObject>());
            }
        }

        [HarmonyPatch("GrabObjectClientRpc")]
        [HarmonyPostfix]
        public static void EquipItemClientRpc(PlayerControllerB __instance, ref NetworkObjectReference grabbedObject)
        {
            if (grabbedObject.TryGet(out var networkObject) && (bool)networkObject.GetComponentInChildren<GrabbableObject>())
            {
                Plugin.logger.LogDebug($"EquipItemClientRpc {networkObject.GetComponentInChildren<GrabbableObject>().itemProperties.itemName}");

                Disable(networkObject.GetComponentInChildren<GrabbableObject>());
            }
        }
    }
}
