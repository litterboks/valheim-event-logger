using HarmonyLib;
using UnityEngine;

namespace EventLogger;

[HarmonyPatch(typeof(Player), "CreateTombStone")]
static class TombstoneCreatePatch
{
    [HarmonyPostfix]
    static void Postfix(Player __instance)
    {
        try
        {
            if (!PluginConfig.EnableCombatEvents.Value) return;
            string playerName = EventLoggerPlugin.CleanName(__instance.GetPlayerName());
            var pos = __instance.transform.position;
            var biome = Heightmap.FindBiome(pos);
            EventLoggerPlugin.Log.LogInfo(
                $"[EventLog] TOMBSTONE_CREATE player={playerName} biome={biome} pos={pos.x:F0},{pos.y:F0},{pos.z:F0}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"TombstoneCreatePatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(Container), "RPC_RequestTakeAll")]
static class TombstonePickupPatch
{
    [HarmonyPostfix]
    static void Postfix(Container __instance, long uid)
    {
        try
        {
            if (!PluginConfig.EnableCombatEvents.Value) return;
            if (__instance.GetComponent<TombStone>() == null) return;

            string playerName = EventLoggerPlugin.GetPlayerNameFromPeer(uid);
            if (string.IsNullOrEmpty(playerName)) playerName = "unknown";
            var pos = __instance.transform.position;
            EventLoggerPlugin.Log.LogInfo(
                $"[EventLog] TOMBSTONE_PICKUP player={playerName} pos={pos.x:F0},{pos.y:F0},{pos.z:F0}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"TombstonePickupPatch error: {e}");
        }
    }
}
