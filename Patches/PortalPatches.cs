using HarmonyLib;

namespace EventLogger;

[HarmonyPatch(typeof(Piece), "SetCreator")]
static class PortalPlacePatch
{
    [HarmonyPostfix]
    static void Postfix(Piece __instance)
    {
        try
        {
            if (__instance.GetComponent<TeleportWorld>() != null)
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
            if (__instance.GetComponent<TeleportWorld>() != null)
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
    static void Postfix()
    {
        try
        {
            EventLoggerPlugin.PortalScanner?.RequestScan();
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"PortalRenamePatch error: {e}");
        }
    }
}
