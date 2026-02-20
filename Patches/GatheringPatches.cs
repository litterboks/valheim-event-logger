using HarmonyLib;

namespace EventLogger;

[HarmonyPatch(typeof(Pickable), "RPC_Pick")]
static class PickablePatch
{
    [HarmonyPrefix]
    static void Prefix(Pickable __instance, long sender)
    {
        try
        {
            if (!PluginConfig.EnableGatheringEvents.Value) return;
            if (__instance == null) return;

            string playerName = EventLoggerPlugin.GetPlayerNameFromPeer(sender);
            if (string.IsNullOrEmpty(playerName) || playerName == "unknown") return;

            string itemName = __instance.m_itemPrefab?.name;
            if (string.IsNullOrEmpty(itemName)) return;

            int amount = __instance.m_amount;
            EventLoggerPlugin.Stats.RecordPickup(playerName, itemName, amount);
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"PickablePatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(Beehive), "RPC_Extract")]
static class BeehiveExtractPatch
{
    [HarmonyPrefix]
    static void Prefix(long caller)
    {
        try
        {
            if (!PluginConfig.EnableGatheringEvents.Value) return;
            string playerName = EventLoggerPlugin.GetPlayerNameFromPeer(caller);
            if (string.IsNullOrEmpty(playerName) || playerName == "unknown") return;
            EventLoggerPlugin.Log.LogInfo($"[EventLog] HONEY_EXTRACT player={playerName}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"BeehiveExtractPatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(SapCollector), "RPC_Extract")]
static class SapExtractPatch
{
    [HarmonyPrefix]
    static void Prefix(long caller)
    {
        try
        {
            if (!PluginConfig.EnableGatheringEvents.Value) return;
            string playerName = EventLoggerPlugin.GetPlayerNameFromPeer(caller);
            if (string.IsNullOrEmpty(playerName) || playerName == "unknown") return;
            EventLoggerPlugin.Log.LogInfo($"[EventLog] SAP_EXTRACT player={playerName}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"SapExtractPatch error: {e}");
        }
    }
}
