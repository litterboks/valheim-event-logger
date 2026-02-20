using BepInEx.Configuration;

namespace EventLogger;

public static class PluginConfig
{
    // Features
    public static ConfigEntry<bool> EnableFermenterScanning;
    public static ConfigEntry<bool> EnablePortalScanning;

    // Intervals
    public static ConfigEntry<float> FermenterScanInterval;

    // Events
    public static ConfigEntry<int> MobKillMinStars;
    public static ConfigEntry<bool> EnableCombatEvents;
    public static ConfigEntry<bool> EnableCraftingEvents;
    public static ConfigEntry<bool> EnableGatheringEvents;
    public static ConfigEntry<bool> EnableWorldEvents;
    public static ConfigEntry<bool> EnablePlayerEvents;

    public static void Bind(ConfigFile config)
    {
        // Features
        EnableFermenterScanning = config.Bind("Features", "EnableFermenterScanning", true,
            "Enable periodic fermenter ZDO scanning");
        EnablePortalScanning = config.Bind("Features", "EnablePortalScanning", true,
            "Enable portal scanning on startup and portal build/destroy/rename");

        // Intervals
        FermenterScanInterval = config.Bind("Intervals", "FermenterScanInterval", 10f,
            "Seconds between fermenter scans");

        // Events
        MobKillMinStars = config.Bind("Events", "MobKillMinStars", 1,
            "Minimum star level to log MOB_KILL events (0=all kills, 1=1-star+, 2=2-star+)");
        EnableCombatEvents = config.Bind("Events", "EnableCombatEvents", true,
            "Log PLAYER_DEATH, BOSS_KILL, MOB_KILL events");
        EnableCraftingEvents = config.Bind("Events", "EnableCraftingEvents", true,
            "Log SMELT_*, FERMENTER_*, COOK_* events");
        EnableGatheringEvents = config.Bind("Events", "EnableGatheringEvents", true,
            "Log PICKUP, HONEY_EXTRACT, SAP_EXTRACT events");
        EnableWorldEvents = config.Bind("Events", "EnableWorldEvents", true,
            "Log EVENT_START/STOP, BOSS_SUMMON, CROP_GROWN, SLEEP, TAME events");
        EnablePlayerEvents = config.Bind("Events", "EnablePlayerEvents", true,
            "Log EAT, PING events");
    }
}
