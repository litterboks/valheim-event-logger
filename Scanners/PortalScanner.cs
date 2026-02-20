using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace EventLogger;

public class PortalScanner
{
    public void Scan()
    {
        try
        {
            if (!PluginConfig.EnablePortalScanning.Value) return;
            if (ZDOMan.instance == null) return;
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
