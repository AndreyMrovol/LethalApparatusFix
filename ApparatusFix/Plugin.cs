using System;
using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace ApparatusFix
{
  [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.SoftDependency)]
  public class Plugin : BaseUnityPlugin
  {
    internal static ManualLogSource logger;
    internal static Harmony harmony = new(PluginInfo.PLUGIN_GUID);

    private void Awake()
    {
      logger = Logger;
      harmony.PatchAll();

      // Plugin startup logic
      Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
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
