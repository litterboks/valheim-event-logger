using HarmonyLib;

namespace EventLogger;

[HarmonyPatch(typeof(Smelter), "OnAddFuel")]
static class SmelterFuelPatch
{
    [HarmonyPrefix]
    static void Prefix(Smelter __instance, Switch sw, Humanoid user, ItemDrop.ItemData item)
    {
        try
        {
            if (!PluginConfig.EnableCraftingEvents.Value) return;
            if (user == null || !user.IsPlayer()) return;
            string playerName = EventLoggerPlugin.CleanName((user as Player)?.GetPlayerName());
            if (string.IsNullOrEmpty(playerName) || playerName == "unknown") return;
            string stationName = EventLoggerPlugin.CleanName(__instance.m_name);
            string itemName = EventLoggerPlugin.CleanName(
                item?.m_dropPrefab?.name ?? __instance.m_fuelItem?.m_itemData?.m_shared?.m_name ?? "fuel");
            EventLoggerPlugin.Log.LogInfo(
                $"[EventLog] SMELT_FUEL player={playerName} station={stationName} item={itemName}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"SmelterFuelPatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(Smelter), "OnAddOre")]
static class SmelterOrePatch
{
    [HarmonyPrefix]
    static void Prefix(Smelter __instance, Switch sw, Humanoid user, ItemDrop.ItemData item)
    {
        try
        {
            if (!PluginConfig.EnableCraftingEvents.Value) return;
            if (user == null || !user.IsPlayer()) return;
            string playerName = EventLoggerPlugin.CleanName((user as Player)?.GetPlayerName());
            if (string.IsNullOrEmpty(playerName) || playerName == "unknown") return;
            string stationName = EventLoggerPlugin.CleanName(__instance.m_name);
            string itemName = EventLoggerPlugin.CleanName(item?.m_dropPrefab?.name ?? item?.m_shared?.m_name ?? "ore");
            EventLoggerPlugin.Log.LogInfo(
                $"[EventLog] SMELT_ORE player={playerName} station={stationName} item={itemName}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"SmelterOrePatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(Smelter), "Spawn")]
static class SmelterDonePatch
{
    [HarmonyPrefix]
    static void Prefix(Smelter __instance, string ore, int stack)
    {
        try
        {
            if (!PluginConfig.EnableCraftingEvents.Value) return;
            string stationName = EventLoggerPlugin.CleanName(__instance.m_name);
            string itemName = EventLoggerPlugin.CleanName(ore);
            EventLoggerPlugin.Log.LogInfo(
                $"[EventLog] SMELT_DONE station={stationName} item={itemName} count={stack}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"SmelterDonePatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(Fermenter), "RPC_Tap")]
static class FermenterTapPatch
{
    [HarmonyPrefix]
    static void Prefix(Fermenter __instance, long sender)
    {
        try
        {
            if (!PluginConfig.EnableCraftingEvents.Value) return;
            string playerName = EventLoggerPlugin.GetPlayerNameFromPeer(sender);
            if (string.IsNullOrEmpty(playerName) || playerName == "unknown") return;
            var nview = Traverse.Create(__instance).Field("m_nview").GetValue<ZNetView>();
            var zdo = nview?.GetZDO();
            string content = zdo?.GetString(ZDOVars.s_content, "") ?? "unknown";
            string itemName = EventLoggerPlugin.CleanName(content);

            if (zdo != null)
            {
                EventLoggerPlugin.FermenterStates.Remove(zdo.m_uid);
                EventLoggerPlugin.FermenterFilledBy.Remove(zdo.m_uid);
            }

            EventLoggerPlugin.Log.LogInfo($"[EventLog] FERMENTER_TAP player={playerName} item={itemName}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"FermenterTapPatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(Fermenter), "RPC_AddItem")]
static class FermenterAddPatch
{
    [HarmonyPrefix]
    static void Prefix(Fermenter __instance, long sender, string name)
    {
        try
        {
            if (!PluginConfig.EnableCraftingEvents.Value) return;
            string playerName = EventLoggerPlugin.GetPlayerNameFromPeer(sender);
            if (string.IsNullOrEmpty(playerName) || playerName == "unknown") return;
            string itemName = EventLoggerPlugin.CleanName(name);

            var nview = Traverse.Create(__instance).Field("m_nview").GetValue<ZNetView>();
            var zdo = nview?.GetZDO();
            if (zdo != null)
            {
                EventLoggerPlugin.FermenterStates[zdo.m_uid] = "fermenting";
                EventLoggerPlugin.FermenterFilledBy[zdo.m_uid] = playerName;
            }

            EventLoggerPlugin.Log.LogInfo($"[EventLog] FERMENTER_ADD player={playerName} item={itemName}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"FermenterAddPatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(CookingStation), "RPC_AddItem")]
static class CookingAddPatch
{
    [HarmonyPrefix]
    static void Prefix(long sender, string itemName)
    {
        try
        {
            if (!PluginConfig.EnableCraftingEvents.Value) return;
            string playerName = EventLoggerPlugin.GetPlayerNameFromPeer(sender);
            if (string.IsNullOrEmpty(playerName) || playerName == "unknown") return;
            string item = EventLoggerPlugin.CleanName(itemName);
            EventLoggerPlugin.Log.LogInfo($"[EventLog] COOK_ADD player={playerName} item={item}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"CookingAddPatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(CookingStation), "RPC_RemoveDoneItem")]
static class CookingRemoveDonePatch
{
    [HarmonyPrefix]
    static void Prefix(CookingStation __instance, long sender)
    {
        try
        {
            if (!PluginConfig.EnableCraftingEvents.Value) return;
            string playerName = EventLoggerPlugin.GetPlayerNameFromPeer(sender);
            if (string.IsNullOrEmpty(playerName) || playerName == "unknown") return;
            string item = "unknown";
            var nview = Traverse.Create(__instance).Field("m_nview").GetValue<ZNetView>();
            if (nview != null)
            {
                var zdo = nview.GetZDO();
                if (zdo != null)
                {
                    int slots = Traverse.Create(__instance).Field("m_slots").GetValue<int>();
                    for (int i = 0; i < slots; i++)
                    {
                        string slotItem = zdo.GetString("slot" + i, "");
                        float slotStatus = zdo.GetFloat("slot" + i, 0f);
                        if (!string.IsNullOrEmpty(slotItem) && slotStatus != 0f)
                        {
                            item = EventLoggerPlugin.CleanName(slotItem);
                            break;
                        }
                    }
                }
            }
            EventLoggerPlugin.Log.LogInfo($"[EventLog] COOK_DONE player={playerName} item={item}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"CookingRemoveDonePatch error: {e}");
        }
    }
}
