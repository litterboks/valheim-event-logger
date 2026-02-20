using HarmonyLib;

namespace EventLogger;

[HarmonyPatch(typeof(Tameable), "Tame")]
static class TameablePatch
{
    [HarmonyPostfix]
    static void Postfix(Tameable __instance)
    {
        try
        {
            if (!PluginConfig.EnableWorldEvents.Value) return;
            if (__instance == null) return;

            var character = __instance.GetComponent<Character>();
            string creatureName = EventLoggerPlugin.CleanName(character?.m_name ?? "unknown");

            var closestPlayer = Player.GetClosestPlayer(__instance.transform.position, 30f);
            if (closestPlayer == null) return;

            string playerName = EventLoggerPlugin.CleanName(closestPlayer.GetPlayerName());
            EventLoggerPlugin.Stats.RecordTame(playerName);
            EventLoggerPlugin.Log.LogInfo($"[EventLog] TAME player={playerName} creature={creatureName}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"TameablePatch error: {e}");
        }
    }
}
