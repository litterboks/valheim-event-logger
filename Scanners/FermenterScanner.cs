using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace EventLogger;

public class FermenterScanner
{
    private bool initialScanDone;
    private float fermentationDuration = 2400f;

    public void Scan()
    {
        try
        {
            if (!PluginConfig.EnableFermenterScanning.Value || !PluginConfig.EnableCraftingEvents.Value) return;
            if (ZDOMan.instance == null || ZNet.instance == null) return;

            if (!initialScanDone)
            {
                DiscoverExistingFermenters();
                initialScanDone = true;
            }

            CheckTrackedFermenters();
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"ScanFermenters error: {e}");
        }
    }

    private void DiscoverExistingFermenters()
    {
        var objectsByID = Traverse.Create(ZDOMan.instance).Field("m_objectsByID").GetValue<Dictionary<ZDOID, ZDO>>();
        if (objectsByID == null) return;

        int fermenterHash = "fermenter".GetStableHashCode();

        // Cache fermentation duration from prefab
        var fermenterObj = ZNetScene.instance?.GetPrefab(fermenterHash);
        if (fermenterObj != null)
        {
            var fc = fermenterObj.GetComponent<Fermenter>();
            if (fc != null) fermentationDuration = fc.m_fermentationDuration;
        }

        foreach (var zdo in objectsByID.Values)
        {
            if (zdo.GetPrefab() != fermenterHash) continue;
            string content = zdo.GetString(ZDOVars.s_content, "");
            if (string.IsNullOrEmpty(content)) continue;

            ZDOID uid = zdo.m_uid;
            if (EventLoggerPlugin.FermenterStates.ContainsKey(uid)) continue;

            string playerName = FindClosestPeer(zdo.GetPosition(), 15f);
            EventLoggerPlugin.FermenterFilledBy[uid] = playerName;
            EventLoggerPlugin.FermenterStates[uid] = "fermenting";
        }
    }

    private void CheckTrackedFermenters()
    {
        if (EventLoggerPlugin.FermenterStates.Count == 0) return;

        System.DateTime now = ZNet.instance.GetTime();
        var toRemove = new List<ZDOID>();
        var nowDone = new List<ZDOID>();

        foreach (var kvp in EventLoggerPlugin.FermenterStates)
        {
            ZDOID uid = kvp.Key;
            string state = kvp.Value;

            var zdo = ZDOMan.instance.GetZDO(uid);
            if (zdo == null)
            {
                toRemove.Add(uid);
                continue;
            }

            string content = zdo.GetString(ZDOVars.s_content, "");
            if (string.IsNullOrEmpty(content))
            {
                toRemove.Add(uid);
                continue;
            }

            if (state == "done") continue;

            long startTicks = zdo.GetLong("StartTime", 0L);
            if (startTicks == 0L) continue;

            double elapsed = (now - new System.DateTime(startTicks)).TotalSeconds;
            if (elapsed >= fermentationDuration)
                nowDone.Add(uid);
        }

        foreach (var uid in nowDone)
        {
            EventLoggerPlugin.FermenterStates[uid] = "done";
            string filledBy;
            EventLoggerPlugin.FermenterFilledBy.TryGetValue(uid, out filledBy);
            var zdo = ZDOMan.instance.GetZDO(uid);
            string doneItem = EventLoggerPlugin.CleanName(zdo?.GetString(ZDOVars.s_content, "") ?? "unknown");

            if (!string.IsNullOrEmpty(filledBy) && filledBy != "unknown")
                EventLoggerPlugin.Log.LogInfo($"[EventLog] FERMENTER_DONE player={filledBy} item={doneItem}");
            else
                EventLoggerPlugin.Log.LogInfo($"[EventLog] FERMENTER_DONE player=unknown item={doneItem}");
        }

        foreach (var uid in toRemove)
        {
            EventLoggerPlugin.FermenterStates.Remove(uid);
            EventLoggerPlugin.FermenterFilledBy.Remove(uid);
        }
    }

    internal static string FindClosestPeer(Vector3 pos, float maxDist)
    {
        if (ZNet.instance == null) return "unknown";
        var peers = ZNet.instance.GetPeers();
        if (peers == null) return "unknown";

        string closest = "unknown";
        float closestDist = maxDist;

        foreach (var peer in peers)
        {
            if (peer.m_characterID.IsNone()) continue;
            var peerZdo = ZDOMan.instance?.GetZDO(peer.m_characterID);
            if (peerZdo == null) continue;
            float dist = Vector3.Distance(pos, peerZdo.GetPosition());
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = EventLoggerPlugin.CleanName(peer.m_playerName);
            }
        }
        return closest;
    }
}
