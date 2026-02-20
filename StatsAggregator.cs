using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EventLogger;

public class PlayerStats
{
    public int kills;
    public float damage_dealt;
    public float damage_taken;
    public float distance;
    public Dictionary<string, int> pickups = new Dictionary<string, int>();
    public Dictionary<string, float> damage_by_mob = new Dictionary<string, float>();
    public int tames;
    public int food_eaten;
    public int items_smelted;

    public int TotalPickups() => pickups.Values.Sum();

    public void Reset()
    {
        kills = 0;
        damage_dealt = 0f;
        damage_taken = 0f;
        distance = 0f;
        pickups.Clear();
        damage_by_mob.Clear();
        tames = 0;
        food_eaten = 0;
        items_smelted = 0;
    }

    public bool HasActivity()
    {
        return kills > 0 || damage_dealt > 0f || damage_taken > 0f || distance > 0f || pickups.Count > 0 || tames > 0 || food_eaten > 0 || items_smelted > 0;
    }
}

public class AlertTracker
{
    public List<(float time, float value)> entries = new List<(float, float)>();
    public float lastFired;
}

public class StatsAggregator
{
    private Dictionary<string, PlayerStats> playerStats = new Dictionary<string, PlayerStats>();
    private Dictionary<string, Dictionary<string, AlertTracker>> alertTrackers = new Dictionary<string, Dictionary<string, AlertTracker>>();
    private float lastFlush;

    public StatsAggregator()
    {
        lastFlush = Time.time;
    }

    private PlayerStats GetOrCreate(string playerName)
    {
        if (!playerStats.ContainsKey(playerName))
        {
            playerStats[playerName] = new PlayerStats();
        }
        return playerStats[playerName];
    }

    private Dictionary<string, AlertTracker> GetPlayerAlerts(string playerName)
    {
        if (!alertTrackers.ContainsKey(playerName))
        {
            alertTrackers[playerName] = new Dictionary<string, AlertTracker>();
        }
        return alertTrackers[playerName];
    }

    private AlertTracker GetAlertTracker(string playerName, string alertType)
    {
        var playerAlerts = GetPlayerAlerts(playerName);
        if (!playerAlerts.ContainsKey(alertType))
        {
            playerAlerts[alertType] = new AlertTracker();
        }
        return playerAlerts[alertType];
    }

    public void RecordKill(string playerName)
    {
        GetOrCreate(playerName).kills++;
        CheckAlert(playerName, "mass_kill", 1f);
    }

    public void RecordDamageDealt(string playerName, float amount, string mobName = null)
    {
        var stats = GetOrCreate(playerName);
        stats.damage_dealt += amount;
        if (mobName != null)
        {
            if (!stats.damage_by_mob.ContainsKey(mobName))
                stats.damage_by_mob[mobName] = 0f;
            stats.damage_by_mob[mobName] += amount;
        }
    }

    public void RecordDamageTaken(string playerName, float amount)
    {
        GetOrCreate(playerName).damage_taken += amount;
        CheckAlert(playerName, "heavy_damage", amount);
    }

    public void RecordDistance(string playerName, float meters)
    {
        GetOrCreate(playerName).distance += meters;
    }

    public void RecordPickup(string playerName, string itemName, int count)
    {
        var stats = GetOrCreate(playerName);
        if (!stats.pickups.ContainsKey(itemName))
        {
            stats.pickups[itemName] = 0;
        }
        stats.pickups[itemName] += count;

        if (itemName == "SilverOre")
        {
            CheckAlert(playerName, "silver_rush", count);
        }
    }

    public void RecordTame(string playerName)
    {
        GetOrCreate(playerName).tames++;
    }

    public void RecordFoodEaten(string playerName)
    {
        GetOrCreate(playerName).food_eaten++;
    }

    public void RecordSmelt(string playerName)
    {
        GetOrCreate(playerName).items_smelted++;
    }

    private void CheckAlert(string playerName, string alertType, float value)
    {
        if (!PluginConfig.EnableAlerts.Value) return;

        int threshold;
        float windowMinutes;
        float cooldownMinutes = PluginConfig.CooldownMinutes.Value;
        string triggerItem;

        switch (alertType)
        {
            case "silver_rush":
                threshold = PluginConfig.SilverRushThreshold.Value;
                windowMinutes = PluginConfig.SilverRushWindowMinutes.Value;
                triggerItem = "SilverOre";
                break;
            case "mass_kill":
                threshold = PluginConfig.MassKillThreshold.Value;
                windowMinutes = PluginConfig.MassKillWindowMinutes.Value;
                triggerItem = null;
                break;
            case "heavy_damage":
                threshold = PluginConfig.HeavyDamageThreshold.Value;
                windowMinutes = PluginConfig.HeavyDamageWindowMinutes.Value;
                triggerItem = null;
                break;
            default:
                return;
        }

        var tracker = GetAlertTracker(playerName, alertType);
        float now = Time.time;
        float windowSeconds = windowMinutes * 60f;
        float cooldownSeconds = cooldownMinutes * 60f;

        if (now - tracker.lastFired < cooldownSeconds) return;

        tracker.entries.Add((now, value));
        tracker.entries.RemoveAll(e => now - e.time > windowSeconds);

        float totalInWindow = 0f;
        if (alertType == "mass_kill" || alertType == "silver_rush")
        {
            totalInWindow = tracker.entries.Count;
        }
        else if (alertType == "heavy_damage")
        {
            totalInWindow = tracker.entries.Sum(e => e.value);
        }

        if (totalInWindow >= threshold)
        {
            string detail = triggerItem ?? alertType;
            EventLoggerPlugin.Log.LogInfo(
                $"[EventLog] ALERT player={playerName} type={alertType} detail={detail} count={totalInWindow:F0} window={windowMinutes}m");
            tracker.lastFired = now;
            tracker.entries.Clear();
        }
    }

    public bool ShouldFlush()
    {
        if (!PluginConfig.EnableStatsAggregation.Value) return false;
        return Time.time - lastFlush >= PluginConfig.StatsFlushInterval.Value;
    }

    public void Flush()
    {
        foreach (var kvp in playerStats)
        {
            var name = EventLoggerPlugin.CleanName(kvp.Key);
            var stats = kvp.Value;

            if (!stats.HasActivity()) continue;

            EventLoggerPlugin.Log.LogInfo(
                $"[EventLog] STATS player={name} kills={stats.kills} damage_dealt={stats.damage_dealt:F0} damage_taken={stats.damage_taken:F0} distance={stats.distance:F0} pickups={stats.TotalPickups()} tames={stats.tames} food_eaten={stats.food_eaten} items_smelted={stats.items_smelted}");

            if (stats.damage_by_mob.Count > 0)
            {
                var sorted = new List<KeyValuePair<string, float>>(stats.damage_by_mob);
                sorted.Sort((a, b) => b.Value.CompareTo(a.Value));
                foreach (var kv in sorted)
                {
                    EventLoggerPlugin.Log.LogInfo(
                        $"[EventLog] MOB_DMG player={name} mob={kv.Key} damage={kv.Value:F0}");
                }
            }

            stats.Reset();
        }

        lastFlush = Time.time;
    }
}
