using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace EventLogger;

public class FermenterScanner
{
    public void Scan()
    {
        try
        {
            if (!PluginConfig.EnableFermenterScanning.Value || !PluginConfig.EnableCraftingEvents.Value) return;
            if (ZDOMan.instance == null || ZNet.instance == null) return;
            var objectsByID = Traverse.Create(ZDOMan.instance).Field("m_objectsByID").GetValue<Dictionary<ZDOID, ZDO>>();
            if (objectsByID == null) return;

            int fermenterHash = "fermenter".GetStableHashCode();
            System.DateTime now = ZNet.instance.GetTime();

            float fermentationDuration = 2400f;
            var fermenterObj = ZNetScene.instance?.GetPrefab(fermenterHash);
            if (fermenterObj != null)
            {
                var fc = fermenterObj.GetComponent<Fermenter>();
                if (fc != null) fermentationDuration = fc.m_fermentationDuration;
            }

            var seen = new HashSet<ZDOID>();

            foreach (var zdo in objectsByID.Values)
            {
                if (zdo.GetPrefab() != fermenterHash) continue;
                ZDOID uid = zdo.m_uid;
                seen.Add(uid);

                string content = zdo.GetString(ZDOVars.s_content, "");

                if (string.IsNullOrEmpty(content))
                {
                    EventLoggerPlugin.FermenterStates.Remove(uid);
                    EventLoggerPlugin.FermenterFilledBy.Remove(uid);
                    continue;
                }

                string prevState;
                EventLoggerPlugin.FermenterStates.TryGetValue(uid, out prevState);

                // Discover fermenters filled before plugin loaded (no log — FermenterAddPatch handles that)
                if (prevState == null)
                {
                    string playerName = FindClosestPeer(zdo.GetPosition(), 15f);
                    EventLoggerPlugin.FermenterFilledBy[uid] = playerName;
                    EventLoggerPlugin.FermenterStates[uid] = "fermenting";
                }

                // Check if done
                long startTicks = zdo.GetLong("StartTime", 0L);
                if (startTicks == 0L) continue;

                double elapsed = (now - new System.DateTime(startTicks)).TotalSeconds;
                if (elapsed < fermentationDuration) continue;

                if (EventLoggerPlugin.FermenterStates.ContainsKey(uid) && EventLoggerPlugin.FermenterStates[uid] == "done") continue;
                EventLoggerPlugin.FermenterStates[uid] = "done";

                string filledBy;
                EventLoggerPlugin.FermenterFilledBy.TryGetValue(uid, out filledBy);
                string doneItem = EventLoggerPlugin.CleanName(content);

                if (!string.IsNullOrEmpty(filledBy) && filledBy != "unknown")
                    EventLoggerPlugin.Log.LogInfo($"[EventLog] FERMENTER_DONE player={filledBy} item={doneItem}");
                else
                    EventLoggerPlugin.Log.LogInfo($"[EventLog] FERMENTER_DONE player=unknown item={doneItem}");
            }

            // Cleanup removed fermenters
            var toRemove = new List<ZDOID>();
            foreach (var uid in EventLoggerPlugin.FermenterStates.Keys)
                if (!seen.Contains(uid)) toRemove.Add(uid);
            foreach (var uid in toRemove)
            {
                EventLoggerPlugin.FermenterStates.Remove(uid);
                EventLoggerPlugin.FermenterFilledBy.Remove(uid);
            }
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"ScanFermenters error: {e}");
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
