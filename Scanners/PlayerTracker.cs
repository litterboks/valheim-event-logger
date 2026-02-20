using UnityEngine;
using System.Collections.Generic;

namespace EventLogger;

public class PlayerTracker
{
    private readonly Dictionary<long, Vector3> lastPositions = new Dictionary<long, Vector3>();

    public void Update()
    {
        try
        {
            if (ZNet.instance == null) return;
            var peers = ZNet.instance.GetPeers();
            if (peers == null) return;

            foreach (var peer in peers)
            {
                if (peer.m_characterID.IsNone()) continue;
                long uid = peer.m_uid;
                string playerName = EventLoggerPlugin.CleanName(peer.m_playerName);
                if (playerName == "unknown") continue;

                EventLoggerPlugin.KnownPlayers[uid] = playerName;

                var zdo = ZDOMan.instance?.GetZDO(peer.m_characterID);
                if (zdo == null) continue;
                Vector3 currentPos = zdo.GetPosition();

                if (lastPositions.ContainsKey(uid))
                {
                    Vector3 lastPos = lastPositions[uid];
                    Vector2 delta2d = new Vector2(currentPos.x - lastPos.x, currentPos.z - lastPos.z);
                    float dist = delta2d.magnitude;

                    if (dist >= 0.5f && dist <= 500f)
                    {
                        EventLoggerPlugin.Stats.RecordDistance(playerName, dist);
                    }
                }

                lastPositions[uid] = currentPos;
            }
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"UpdatePlayerDistances error: {e}");
        }
    }

    public void RemovePlayer(long uid)
    {
        lastPositions.Remove(uid);
    }
}
