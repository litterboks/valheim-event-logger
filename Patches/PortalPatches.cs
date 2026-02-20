using HarmonyLib;

namespace EventLogger;

[HarmonyPatch(typeof(Piece), "SetCreator")]
static class PortalPlacePatch
{
    [HarmonyPostfix]
    static void Postfix(Piece __instance, long playerID)
    {
        try
        {
            if (!PluginConfig.EnablePortalScanning.Value) return;
            if (__instance.GetComponent<TeleportWorld>() == null) return;

            string playerName = "unknown";
            if (EventLoggerPlugin.KnownPlayers.TryGetValue(playerID, out string name))
                playerName = name;
            var pos = __instance.transform.position;
            EventLoggerPlugin.Log.LogInfo(
                $"[EventLog] PORTAL_BUILD player={playerName} pos={pos.x:F0},{pos.z:F0}");

            EventLoggerPlugin.PortalScanner?.RequestScan();
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"PortalPlacePatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(WearNTear), "Destroy")]
static class PortalDestroyPatch
{
    [HarmonyPrefix]
    static void Prefix(WearNTear __instance)
    {
        try
        {
            if (!PluginConfig.EnablePortalScanning.Value) return;
            var tp = __instance.GetComponent<TeleportWorld>();
            if (tp == null) return;

            var nview = Traverse.Create(tp).Field("m_nview").GetValue<ZNetView>();
            var zdo = nview?.GetZDO();
            string tag = zdo?.GetString(ZDOVars.s_tag, "") ?? "";
            if (string.IsNullOrEmpty(tag)) tag = "untagged";
            else tag = tag.Replace("|", "").Replace(",", "").Replace(" ", "_");
            var pos = __instance.transform.position;
            EventLoggerPlugin.Log.LogInfo(
                $"[EventLog] PORTAL_DESTROY tag={tag} pos={pos.x:F0},{pos.z:F0}");

            EventLoggerPlugin.PortalScanner?.RequestScan();
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"PortalDestroyPatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(TeleportWorld), "RPC_SetTag")]
static class PortalRenamePatch
{
    [HarmonyPostfix]
    static void Postfix(TeleportWorld __instance, long sender, string tag)
    {
        try
        {
            if (!PluginConfig.EnablePortalScanning.Value) return;
            string playerName = EventLoggerPlugin.GetPlayerNameFromPeer(sender);
            if (string.IsNullOrEmpty(playerName)) playerName = "unknown";
            string cleanTag = string.IsNullOrEmpty(tag) ? "untagged" : tag.Replace("|", "").Replace(",", "").Replace(" ", "_");
            var pos = __instance.transform.position;
            EventLoggerPlugin.Log.LogInfo(
                $"[EventLog] PORTAL_RENAME player={playerName} tag={cleanTag} pos={pos.x:F0},{pos.z:F0}");

            EventLoggerPlugin.PortalScanner?.RequestScan();
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"PortalRenamePatch error: {e}");
        }
    }
}
