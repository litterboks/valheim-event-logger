using HarmonyLib;

namespace EventLogger;

[HarmonyPatch(typeof(CharacterDrop), "OnDeath")]
static class CharacterDropPatch
{
    [HarmonyPrefix]
    static void Prefix(CharacterDrop __instance)
    {
        try
        {
            if (!PluginConfig.EnableCombatEvents.Value) return;
            if (__instance == null) return;
            var character = __instance.GetComponent<Character>();
            if (character == null) return;

            if (character.IsPlayer())
            {
                string playerName = EventLoggerPlugin.CleanName((character as Player)?.GetPlayerName());
                string killer = "unknown";

                var lastHit = Traverse.Create(character).Field("m_lastHit").GetValue<HitData>();
                if (lastHit != null)
                {
                    var att = lastHit.GetAttacker();
                    if (att != null)
                        killer = EventLoggerPlugin.CleanName(att.m_name);

                    if (killer == "unknown")
                    {
                        var dmg = lastHit.m_damage;
                        if (dmg.m_fire > 0) killer = "fire";
                        else if (dmg.m_frost > 0) killer = "frost";
                        else if (dmg.m_lightning > 0) killer = "lightning";
                        else if (dmg.m_poison > 0) killer = "poison";
                        else if (dmg.m_spirit > 0) killer = "spirit";
                    }
                }

                var pos = character.transform.position;
                EventLoggerPlugin.Log.LogInfo(
                    $"[EventLog] PLAYER_DEATH player={playerName} killed_by={killer} pos={pos.x:F0},{pos.y:F0},{pos.z:F0}");
                return;
            }

            if (character.IsBoss())
            {
                string bossName = EventLoggerPlugin.CleanName(character.m_name);
                var pos = character.transform.position;
                EventLoggerPlugin.Log.LogInfo(
                    $"[EventLog] BOSS_KILL boss={bossName} pos={pos.x:F0},{pos.y:F0},{pos.z:F0}");
                return;
            }

            var charLastHit = Traverse.Create(character).Field("m_lastHit").GetValue<HitData>();
            if (charLastHit != null)
            {
                var attacker = charLastHit.GetAttacker();
                if (attacker != null && attacker.IsPlayer())
                {
                    var player = attacker as Player;
                    if (player != null)
                    {
                        string playerName = EventLoggerPlugin.CleanName(player.GetPlayerName());
                        EventLoggerPlugin.Stats.RecordKill(playerName);

                        int level = character.GetLevel();
                        if (level >= 2)
                        {
                            string mobName = EventLoggerPlugin.CleanName(character.m_name);
                            var pos = character.transform.position;
                            EventLoggerPlugin.Log.LogInfo(
                                $"[EventLog] MOB_KILL player={playerName} mob={mobName} stars={level} pos={pos.x:F0},{pos.y:F0},{pos.z:F0}");
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            EventLoggerPlugin.Log.LogError($"CharacterDropPatch error: {e}");
        }
    }
}
