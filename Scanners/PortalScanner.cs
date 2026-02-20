using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace EventLogger;

public class PortalScanner
{
    private float pendingScanTime = -1f;
    private const float DEBOUNCE_SECONDS = 2f;

    public void RequestScan()
    {
        pendingScanTime = Time.time + DEBOUNCE_SECONDS;
    }

    public void CheckAndScan()
    {
        if (pendingScanTime < 0f || Time.time < pendingScanTime) return;
        pendingScanTime = -1f;

        if (!PluginConfig.EnablePortalScanning.Value) return;

        if (ZDOMan.instance == null)
        {
            RequestScan(); // world not ready, retry
            return;
        }

        try
        {
            var objectsByID = Traverse.Create(ZDOMan.instance).Field("m_objectsByID").GetValue<Dictionary<ZDOID, ZDO>>();
            if (objectsByID == null) return;

            int portalWoodHash = "portal_wood".GetStableHashCode();
            int portalStoneHash = "portal_stone".GetStableHashCode();

            var entries = new List<string>();
            foreach (var zdo in objectsByID.Values)
            {
                int prefab = zdo.GetPrefab();
                if (prefab != portalWoodHash && prefab != portalStoneHash) continue;
                Vector3 pos = zdo.GetPosition();
                string tag = zdo.GetString(ZDOVars.s_tag, "");
                if (string.IsNullOrEmpty(tag)) tag = "untagged";
                else tag = tag.Replace("|", "").Replace(",", "").Replace(" ", "_");
                entries.Add($"{tag}|{pos.x:F0}|{pos.z:F0}");
            }

            string portalData = entries.Count > 0 ? string.Join(",", entries) : "none";
            EventLoggerPlugin.Log.LogInfo($"[EventLog] PORTAL_LIST portals={portalData}");
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"ScanPortals error: {e}");
        }
    }
}
