using BepInEx.Configuration;

namespace EventLogger;

public static class PluginConfig
{
    // Features
    public static ConfigEntry<bool> EnableRpcSniffer;
    public static ConfigEntry<bool> EnableStatsAggregation;
    public static ConfigEntry<bool> EnableAlerts;
    public static ConfigEntry<bool> EnableFermenterScanning;
    public static ConfigEntry<bool> EnablePortalScanning;

    // Intervals
    public static ConfigEntry<float> StatsFlushInterval;
    public static ConfigEntry<float> FermenterScanInterval;
    public static ConfigEntry<float> DistanceCheckInterval;
    public static ConfigEntry<float> RpcReportInterval;

    // Alerts
    public static ConfigEntry<int> SilverRushThreshold;
    public static ConfigEntry<float> SilverRushWindowMinutes;
    public static ConfigEntry<int> MassKillThreshold;
    public static ConfigEntry<float> MassKillWindowMinutes;
    public static ConfigEntry<int> HeavyDamageThreshold;
    public static ConfigEntry<float> HeavyDamageWindowMinutes;
    public static ConfigEntry<float> CooldownMinutes;

    // Events
    public static ConfigEntry<bool> EnableCombatEvents;
    public static ConfigEntry<bool> EnableCraftingEvents;
    public static ConfigEntry<bool> EnableGatheringEvents;
    public static ConfigEntry<bool> EnableWorldEvents;
    public static ConfigEntry<bool> EnablePlayerEvents;

    public static void Bind(ConfigFile config)
    {
        // Features
        EnableRpcSniffer = config.Bind("Features", "EnableRpcSniffer", false,
            "Enable RPC sniffer for damage tracking via network packets");
        EnableStatsAggregation = config.Bind("Features", "EnableStatsAggregation", true,
            "Enable periodic stats aggregation and flush");
        EnableAlerts = config.Bind("Features", "EnableAlerts", true,
            "Enable alert system for unusual activity");
        EnableFermenterScanning = config.Bind("Features", "EnableFermenterScanning", true,
            "Enable periodic fermenter ZDO scanning");
        EnablePortalScanning = config.Bind("Features", "EnablePortalScanning", true,
            "Enable portal scanning on startup and portal build/destroy/rename");

        // Intervals
        StatsFlushInterval = config.Bind("Intervals", "StatsFlushInterval", 60f,
            "Seconds between stats flushes");
        FermenterScanInterval = config.Bind("Intervals", "FermenterScanInterval", 10f,
            "Seconds between fermenter scans");
        DistanceCheckInterval = config.Bind("Intervals", "DistanceCheckInterval", 5f,
            "Seconds between player distance checks");
        RpcReportInterval = config.Bind("Intervals", "RpcReportInterval", 300f,
            "Seconds between RPC sniffer reports");

        // Alerts
        SilverRushThreshold = config.Bind("Alerts", "SilverRushThreshold", 10,
            "Number of SilverOre pickups to trigger silver_rush alert");
        SilverRushWindowMinutes = config.Bind("Alerts", "SilverRushWindowMinutes", 10f,
            "Time window in minutes for silver rush detection");
        MassKillThreshold = config.Bind("Alerts", "MassKillThreshold", 20,
            "Number of kills to trigger mass_kill alert");
        MassKillWindowMinutes = config.Bind("Alerts", "MassKillWindowMinutes", 5f,
            "Time window in minutes for mass kill detection");
        HeavyDamageThreshold = config.Bind("Alerts", "HeavyDamageThreshold", 500,
            "Cumulative damage to trigger heavy_damage alert");
        HeavyDamageWindowMinutes = config.Bind("Alerts", "HeavyDamageWindowMinutes", 2f,
            "Time window in minutes for heavy damage detection");
        CooldownMinutes = config.Bind("Alerts", "CooldownMinutes", 15f,
            "Shared cooldown in minutes between alerts of the same type per player");

        // Events
        EnableCombatEvents = config.Bind("Events", "EnableCombatEvents", true,
            "Log PLAYER_DEATH, BOSS_KILL, MOB_KILL events");
        EnableCraftingEvents = config.Bind("Events", "EnableCraftingEvents", true,
            "Log SMELT_*, FERMENTER_*, COOK_* events");
        EnableGatheringEvents = config.Bind("Events", "EnableGatheringEvents", true,
            "Log pickup tracking, HONEY_EXTRACT, SAP_EXTRACT events");
        EnableWorldEvents = config.Bind("Events", "EnableWorldEvents", true,
            "Log EVENT_START/STOP, BOSS_SUMMON, CROP_GROWN, SLEEP, TAME events");
        EnablePlayerEvents = config.Bind("Events", "EnablePlayerEvents", true,
            "Log EAT, PING events");
    }
}
