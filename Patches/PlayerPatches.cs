using HarmonyLib;
using UnityEngine;

namespace EventLogger;

[HarmonyPatch(typeof(Player), "EatFood")]
static class EatFoodPatch
{
    [HarmonyPostfix]
    static void Postfix(Player __instance, ItemDrop.ItemData item, bool __result)
    {
        try
        {
            if (!PluginConfig.EnablePlayerEvents.Value) return;
            if (!__result) return;
            string playerName = EventLoggerPlugin.CleanName(__instance.GetPlayerName());
            if (string.IsNullOrEmpty(playerName) || playerName == "unknown") return;
            string foodName = EventLoggerPlugin.CleanName(item?.m_dropPrefab?.name ?? item?.m_shared?.m_name ?? "unknown");
            EventLoggerPlugin.Log.LogInfo(
                $"[EventLog] EAT player={playerName} food={foodName}");
            EventLoggerPlugin.Stats.RecordFoodEaten(playerName);
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"EatFoodPatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(Chat), "OnNewChatMessage")]
static class ChatPingPatch
{
    [HarmonyPostfix]
    static void Postfix(Vector3 pos, Talker.Type type, UserInfo sender)
    {
        try
        {
            if (!PluginConfig.EnablePlayerEvents.Value) return;
            if (type != Talker.Type.Ping) return;
            string playerName = EventLoggerPlugin.CleanName(sender.Name);
            if (string.IsNullOrEmpty(playerName) || playerName == "unknown") return;
            EventLoggerPlugin.Log.LogInfo(
                $"[EventLog] PING player={playerName} x={pos.x:F0} z={pos.z:F0}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"ChatPingPatch error: {e}");
        }
    }
}
