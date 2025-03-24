using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ApparatusFix
{
  [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
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

  internal class LobbyCompatibilityCompatibility
  {
    public static void Init()
    {
      Plugin.logger.LogWarning("LobbyCompatibility detected, registering plugin with LobbyCompatibility.");

      Version pluginVersion = Version.Parse(ApparatusFix.PluginInfo.PLUGIN_VERSION);

      LobbyCompatibility.Features.PluginHelper.RegisterPlugin(
        "LethalRichPresence",
        pluginVersion,
        LobbyCompatibility.Enums.CompatibilityLevel.ClientOptional,
        LobbyCompatibility.Enums.VersionStrictness.None
      );
    }
  }

  [HarmonyPatch(typeof(GrabbableObject))]
  public class Patch
  {
    private static bool ShouldReturn(GrabbableObject item)
    {
      if (!item.isInShipRoom)
      {
        Plugin.logger.LogDebug($"{item.__getTypeName()} is not in ship room");
        return true;
      }

      if (item.__getTypeName() != "LungProp")
      {
        Plugin.logger.LogDebug($"{item.__getTypeName()} is not a predefined type");
        return true;
      }

      return false;
    }

    public static void Disable(GrabbableObject item)
    {
      if (ShouldReturn(item))
        return;

      if ("LungProp" == item.__getTypeName())
      {
        Plugin.logger.LogDebug("Disabling sound of LungProp");

        LungProp itemLung = (LungProp)item;
        itemLung.isLungDocked = false;
        itemLung.isLungDockedInElevator = false;
        itemLung.isLungPowered = false;
        itemLung.GetComponent<AudioSource>().Stop();
      }
    }

    [HarmonyPatch("DiscardItemClientRpc")]
    [HarmonyPostfix]
    public static void DiscardItemClientRpc(GrabbableObject __instance)
    {
      Plugin.logger.LogDebug($"DiscardItemClientRpc {__instance.itemProperties.itemName}");

      if (__instance.isInShipRoom)
      {
        Disable(__instance);
      }
    }

    [HarmonyPatch("DiscardItemOnClient")]
    [HarmonyPostfix]
    public static void DiscardItemOnClient(GrabbableObject __instance)
    {
      Plugin.logger.LogDebug($"DiscardItemOnClient {__instance.itemProperties.itemName}");

      if (__instance.isInShipRoom)
      {
        Disable(__instance);
      }
    }
  }
}
