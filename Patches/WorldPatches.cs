using HarmonyLib;

namespace EventLogger;

[HarmonyPatch(typeof(RandEventSystem), "SetActiveEvent")]
static class RandomEventPatch
{
    [HarmonyPostfix]
    static void Postfix(RandomEvent ev, bool end)
    {
        try
        {
            if (!PluginConfig.EnableWorldEvents.Value) return;
            if (ev != null && !end)
            {
                EventLoggerPlugin.Log.LogInfo(
                    $"[EventLog] EVENT_START event={ev.m_name}");
            }
            else if (end)
            {
                EventLoggerPlugin.Log.LogInfo("[EventLog] EVENT_STOP");
            }
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"RandomEventPatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(Plant), "Grow")]
static class PlantGrowPatch
{
    [HarmonyPostfix]
    static void Postfix(Plant __instance)
    {
        try
        {
            if (!PluginConfig.EnableWorldEvents.Value) return;
            string plantName = EventLoggerPlugin.CleanName(
                __instance.m_name ?? __instance.gameObject?.name ?? "unknown");
            string plantedBy = "";
            var piece = __instance.GetComponent<Piece>();
            if (piece != null)
            {
                long creatorId = piece.GetCreator();
                if (creatorId != 0 && EventLoggerPlugin.KnownPlayers.TryGetValue(creatorId, out string name))
                {
                    plantedBy = name;
                }
            }
            if (!string.IsNullOrEmpty(plantedBy))
                EventLoggerPlugin.Log.LogInfo(
                    $"[EventLog] CROP_GROWN plant={plantName} planted_by={plantedBy}");
            else
                EventLoggerPlugin.Log.LogInfo(
                    $"[EventLog] CROP_GROWN plant={plantName}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"PlantGrowPatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(OfferingBowl), "SpawnBoss")]
static class BossSummonPatch
{
    [HarmonyPrefix]
    static void Prefix(OfferingBowl __instance)
    {
        try
        {
            if (!PluginConfig.EnableWorldEvents.Value) return;
            string bossName = EventLoggerPlugin.CleanName(
                __instance.m_bossPrefab?.name ?? "unknown");
            var closestPlayer = Player.GetClosestPlayer(
                __instance.transform.position, 30f);
            string playerName = closestPlayer != null
                ? EventLoggerPlugin.CleanName(closestPlayer.GetPlayerName())
                : "unknown";
            var pos = __instance.transform.position;
            EventLoggerPlugin.Log.LogInfo(
                $"[EventLog] BOSS_SUMMON boss={bossName} player={playerName} pos={pos.x:F0},{pos.y:F0},{pos.z:F0}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"BossSummonPatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(Game), "SleepStart")]
static class SleepStartPatch
{
    [HarmonyPostfix]
    static void Postfix()
    {
        try
        {
            if (!PluginConfig.EnableWorldEvents.Value) return;
            EventLoggerPlugin.Log.LogInfo("[EventLog] SLEEP_START");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"SleepStartPatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(Game), "SleepStop")]
static class SleepStopPatch
{
    [HarmonyPostfix]
    static void Postfix()
    {
        try
        {
            if (!PluginConfig.EnableWorldEvents.Value) return;
            EventLoggerPlugin.Log.LogInfo("[EventLog] SLEEP_STOP");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"SleepStopPatch error: {e}");
        }
    }
}
