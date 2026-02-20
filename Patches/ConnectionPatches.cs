using HarmonyLib;

namespace EventLogger;

[HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
static class PeerInfoPatch
{
    [HarmonyPostfix]
    static void Postfix(ZRpc rpc)
    {
        try
        {
            if (ZNet.instance == null) return;
            var peers = ZNet.instance.GetPeers();
            if (peers == null) return;
            foreach (var peer in peers)
            {
                if (peer.m_rpc == rpc)
                {
                    string name = EventLoggerPlugin.CleanName(peer.m_playerName);
                    EventLoggerPlugin.KnownPlayers[peer.m_uid] = name;
                    EventLoggerPlugin.Log.LogInfo($"[EventLog] PEER_CONNECT player={name} uid={peer.m_uid}");
                    break;
                }
            }
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"PeerInfoPatch error: {e}");
        }
    }
}

[HarmonyPatch(typeof(ZNet), "RPC_Disconnect")]
static class PeerDisconnectPatch
{
    [HarmonyPrefix]
    static void Prefix(ZRpc rpc)
    {
        try
        {
            if (ZNet.instance == null) return;
            var peers = ZNet.instance.GetPeers();
            if (peers == null) return;
            foreach (var peer in peers)
            {
                if (peer.m_rpc == rpc)
                {
                    string name = EventLoggerPlugin.CleanName(peer.m_playerName);
                    EventLoggerPlugin.Log.LogInfo($"[EventLog] PEER_DISCONNECT player={name} uid={peer.m_uid}");
                    EventLoggerPlugin.KnownPlayers.Remove(peer.m_uid);
                    EventLoggerPlugin.PlayerTracker?.RemovePlayer(peer.m_uid);
                    break;
                }
            }
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"PeerDisconnectPatch error: {e}");
        }
    }
}
