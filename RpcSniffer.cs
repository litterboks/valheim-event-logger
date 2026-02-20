using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EventLogger;

[HarmonyPatch(typeof(ZRoutedRpc), "HandleRoutedRPC")]
static class RpcSnifferPatch
{
    static readonly int DamageTextHash = "RPC_DamageText".GetStableHashCode();
    static readonly Dictionary<int, int> ZdoRpcCounts = new Dictionary<int, int>();
    static int damageRpcCount;
    static int totalRpcCount;

    [HarmonyPrefix]
    static void Prefix(ZRoutedRpc.RoutedRPCData data)
    {
        try
        {
            if (!PluginConfig.EnableRpcSniffer.Value) return;

            totalRpcCount++;
            if (!data.m_targetZDO.IsNone())
            {
                if (ZdoRpcCounts.ContainsKey(data.m_methodHash))
                    ZdoRpcCounts[data.m_methodHash]++;
                else
                    ZdoRpcCounts[data.m_methodHash] = 1;
            }
            if (data.m_methodHash == DamageTextHash)
            {
                int pos = data.m_parameters.GetPos();
                try
                {
                    ProcessDamageText(data.m_senderPeerID, data.m_parameters);
                }
                finally
                {
                    data.m_parameters.SetPos(pos);
                }
            }
        }
        catch (Exception e)
        {
            EventLoggerPlugin.Log.LogWarning($"RpcSniffer.Prefix error: {e}");
        }
    }

    static void ProcessDamageText(long senderPeerID, ZPackage pkg)
    {
        try
        {
            byte[] raw = pkg.ReadByteArray();
            var inner = new ZPackage(raw);
            int textType = inner.ReadInt();
            Vector3 pos = inner.ReadVector3();
            string text = inner.ReadString();
            bool isPlayer = inner.ReadBool();

            if (textType != 0 && textType != 2) return;

            float dmg = 0f;
            if (float.TryParse(text, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float parsed))
                dmg = parsed;

            if (dmg <= 0f) return;

            string attackerName = EventLoggerPlugin.GetPlayerNameFromPeer(senderPeerID);

            if (attackerName != null && !isPlayer)
            {
                EventLoggerPlugin.Stats.RecordDamageDealt(attackerName, dmg);
                damageRpcCount++;
            }

            if (isPlayer)
            {
                string targetName = EventLoggerPlugin.GetPlayerNameFromPeer(senderPeerID);
                if (targetName != null)
                    EventLoggerPlugin.Stats.RecordDamageTaken(targetName, dmg);
                damageRpcCount++;
            }
        }
        catch (Exception e)
        {
            EventLoggerPlugin.Log.LogWarning($"RpcSniffer.ProcessDamageText error: {e}");
        }
    }

    internal static void FlushReport()
    {
        EventLoggerPlugin.Log.LogInfo(
            $"[EventLog] RPC_SNIFFER damage_rpcs={damageRpcCount} total_rpcs={totalRpcCount}");

        if (ZdoRpcCounts.Count > 0)
        {
            var sorted = new List<KeyValuePair<int, int>>(ZdoRpcCounts);
            sorted.Sort((a, b) => b.Value.CompareTo(a.Value));
            foreach (var kv in sorted)
            {
                EventLoggerPlugin.Log.LogInfo(
                    $"[EventLog] ZDO_RPC hash={kv.Key} count={kv.Value}");
            }
            ZdoRpcCounts.Clear();
        }

        damageRpcCount = 0;
        totalRpcCount = 0;
    }
}
